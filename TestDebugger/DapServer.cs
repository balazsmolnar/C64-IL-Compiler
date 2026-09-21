using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace TestDebugger;

// Debug Adapter Protocol server -- lets VS Code drive the exact same engine
// ReplLoop drives from the terminal (TestCompiler/DebugMapModel/
// BreakpointResolver/TestSession/LocalVariableInspector), just via DAP
// messages over stdio instead of REPL text. Only ever reports one thread and
// one stack frame -- matches TestSession's own "no call-stack walking"
// limitation (see LocalVariableInspector's comment).
//
// Lifecycle (standard DAP adapter order): initialize -> launch (compiles the
// debug build here, since it can take a few seconds) -> initialized event
// (tells the client it may now send breakpoints) -> setBreakpoints (per
// file, replacing that file's set every time) -> configurationDone (actually
// starts the test) -> stopped/output+terminated events as it runs.
class DapServer
{
    private const int ThreadId = 1;
    private const int LocalsVariablesReference = 1;
    private const int RegistersVariablesReference = 2;

    private readonly string _repoRoot;
    private readonly DapIo _io;
    private DebugMapModel _model;
    private DisassemblyListing _listing;
    private BreakpointResolver _resolver;
    private TestSession _session;
    private string _testSelector;
    private readonly Dictionary<string, HashSet<int>> _breakpointsByFile = new();

    // Maps a minted DAP variablesReference -> the InspectedValue it expands
    // (an object or non-null array). Reset on every new stop, since DAP
    // convention is that a variablesReference is only valid for the current
    // stopped state -- VS Code never reuses one across two different stops.
    // 1/2 stay reserved for the fixed Locals/Registers scopes themselves.
    private readonly Dictionary<int, InspectedValue> _variableRefs = new();
    private int _nextVariablesReference = 3;

    // Non-null when launched with testSelector omitted (or "*"/"all"):
    // every test in the assembly runs in turn (fresh emulator per test,
    // same as the real suite), auto-advancing past each Passed/Failed
    // result until either a breakpoint fires in whichever test hits it, or
    // the whole queue is exhausted.
    private List<string> _runAllQueue;
    private int _runAllIndex;

    public DapServer(string repoRoot, Stream input, Stream output)
    {
        _repoRoot = repoRoot;
        _io = new DapIo(input, output);
    }

    public void Run()
    {
        while (true)
        {
            DapIncomingMessage msg;
            try
            {
                msg = _io.ReadMessage();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"DAP read error: {ex.Message}");
                return;
            }
            if (msg == null)
                return; // client disconnected
            if (msg.Type != "request")
                continue;

            try
            {
                if (!Handle(msg))
                    return;
            }
            catch (Exception ex)
            {
                _io.WriteResponse(msg.Seq, msg.Command, false, null, ex.Message);
            }
        }
    }

    // Returns false to end the adapter process.
    private bool Handle(DapIncomingMessage msg)
    {
        switch (msg.Command)
        {
            case "initialize":
                _io.WriteResponse(msg.Seq, msg.Command, true, new
                {
                    supportsConfigurationDoneRequest = true,
                    supportsDisassembleRequest = true,
                });
                return true;

            case "launch":
                HandleLaunch(msg);
                return true;

            case "setBreakpoints":
                HandleSetBreakpoints(msg);
                return true;

            case "configurationDone":
                _io.WriteResponse(msg.Seq, msg.Command, true, null);
                SyncBreakpoints();
                ReportStop(SafeRun(_testSelector));
                return true;

            case "threads":
                _io.WriteResponse(msg.Seq, msg.Command, true, new
                {
                    threads = new[] { new { id = ThreadId, name = "test" } }
                });
                return true;

            case "stackTrace":
                HandleStackTrace(msg);
                return true;

            case "scopes":
                _io.WriteResponse(msg.Seq, msg.Command, true, new
                {
                    scopes = new[]
                    {
                        new { name = "Locals", variablesReference = LocalsVariablesReference, expensive = false },
                        new { name = "Registers", variablesReference = RegistersVariablesReference, expensive = false },
                    }
                });
                return true;

            case "variables":
                HandleVariables(msg);
                return true;

            case "next":
            case "stepIn":
            case "stepOut":
                _io.WriteResponse(msg.Seq, msg.Command, true, null);
                ReportStop(IsInstructionGranularity(msg) ? _session.StepInstruction() : _session.Step());
                return true;

            case "continue":
                _io.WriteResponse(msg.Seq, msg.Command, true, new { allThreadsContinued = true });
                ReportStop(_session.Continue());
                return true;

            case "disassemble":
                HandleDisassemble(msg);
                return true;

            case "disconnect":
            case "terminate":
                _io.WriteResponse(msg.Seq, msg.Command, true, null);
                return false;

            default:
                // Unimplemented optional request (e.g. evaluate,
                // loadedSources) -- reply empty-success so VS Code doesn't
                // stall waiting for a response it isn't strictly owed.
                _io.WriteResponse(msg.Seq, msg.Command, true, null);
                return true;
        }
    }

    private void HandleLaunch(DapIncomingMessage msg)
    {
        var selector = msg.Arguments.TryGetProperty("testSelector", out var s) ? s.GetString() : null;

        var compiler = new TestCompiler(_repoRoot);
        compiler.EnsureCompiled(forceRecompile: false);
        _model = DebugMapModel.Load(compiler.DebugMapPath, compiler.LabelsPath);
        _listing = DisassemblyListing.Load(compiler.DumpListingPath);
        _resolver = new BreakpointResolver(_model);
        var testAssembly = Assembly.LoadFrom(compiler.TestDllPath);
        _session = new TestSession(testAssembly, _model, compiler.PrgPath);

        if (string.IsNullOrEmpty(selector) || selector == "*" || selector.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            _runAllQueue = _session.DiscoverAllTestSelectors();
            if (_runAllQueue.Count == 0)
                throw new InvalidOperationException("No [Test]/[TestCase] methods found in the compiled test assembly.");
            _runAllIndex = 0;
            _testSelector = _runAllQueue[0];
        }
        else
        {
            _runAllQueue = null;
            _testSelector = selector;
        }

        _io.WriteResponse(msg.Seq, msg.Command, true, null);
        _io.WriteEvent("initialized", null);
    }

    private void HandleSetBreakpoints(DapIncomingMessage msg)
    {
        var source = msg.Arguments.GetProperty("source");
        var path = source.GetProperty("path").GetString();
        var fileName = Path.GetFileName(path);

        var resolved = new List<object>();
        var addresses = new HashSet<int>();
        if (msg.Arguments.TryGetProperty("breakpoints", out var bpArray))
        {
            foreach (var bp in bpArray.EnumerateArray())
            {
                var line = bp.GetProperty("line").GetInt32();
                try
                {
                    var addrs = _resolver.Resolve($"{fileName}:{line}", out _, out var resolvedLine);
                    foreach (var a in addrs)
                        addresses.Add(a);
                    resolved.Add(new { verified = true, line = resolvedLine });
                }
                catch (Exception)
                {
                    resolved.Add(new { verified = false, line });
                }
            }
        }

        _breakpointsByFile[fileName] = addresses;
        _io.WriteResponse(msg.Seq, msg.Command, true, new { breakpoints = resolved });
    }

    private void SyncBreakpoints()
    {
        var union = _breakpointsByFile.Values.SelectMany(x => x).ToHashSet();
        _session.ReplaceBreakpoints(union);
    }

    // Computed fresh from the LIVE emulator state every call (not a cached
    // "last stop" snapshot) -- necessary now that instruction-granularity
    // stepping can land somewhere with no source line at all; the
    // instructionPointerReference field is what VS Code actually uses to
    // sync the Disassembly View's highlighted row, independent of
    // source/line (which are simply omitted when there's no source here --
    // a normal, well-defined DAP state, not an error).
    private void HandleStackTrace(DapIncomingMessage msg)
    {
        var frames = new List<object>();
        if (_session?.Emulator != null)
        {
            var pc = _session.Emulator.ProgramCounter;
            var entry = _model.FindByAddress(pc);
            var frame = new Dictionary<string, object>
            {
                ["id"] = 1,
                ["name"] = _session.CurrentMethod?.Name,
                ["instructionPointerReference"] = $"0x{pc:X4}",
                ["line"] = entry?.Line ?? 0,
                ["column"] = 1,
            };
            if (entry != null)
                frame["source"] = new { name = entry.SourceFile, path = Path.Combine(_repoRoot, "Test", entry.SourceFile) };
            frames.Add(frame);
        }
        _io.WriteResponse(msg.Seq, msg.Command, true, new { stackFrames = frames, totalFrames = frames.Count });
    }

    private void HandleVariables(DapIncomingMessage msg)
    {
        var variablesReference = msg.Arguments.GetProperty("variablesReference").GetInt32();
        var variables = new List<object>();

        if (variablesReference == RegistersVariablesReference)
        {
            if (_session?.Emulator != null)
            {
                var r = _session.Emulator.Registers;
                var pc = _session.Emulator.ProgramCounter;
                var sp = _session.Emulator.HardwareStackPointer;
                variables.Add(RegVar("A", r.A));
                variables.Add(RegVar("X", r.X));
                variables.Add(RegVar("Y", r.Y));
                variables.Add(new { name = "PC", value = $"0x{pc:X4}", type = "register", variablesReference = 0 });
                variables.Add(new { name = "SP", value = $"0x{sp:X2}", type = "register", variablesReference = 0 });
                variables.Add(RegVar("P", r.P));
                variables.Add(new
                {
                    name = "Flags",
                    value = $"{(r.N ? 'N' : '-')}{(r.V ? 'V' : '-')}--{(r.I ? 'I' : '-')}-{(r.Z ? 'Z' : '-')}{(r.C ? 'C' : '-')}",
                    type = "register",
                    variablesReference = 0,
                });
            }
            _io.WriteResponse(msg.Seq, msg.Command, true, new { variables });
            return;
        }

        var locals = _session.Locals;
        if (variablesReference == LocalsVariablesReference)
        {
            if (locals != null)
            {
                foreach (var name in locals.LocalNames())
                    if (locals.TryGetLocalValue(name, out var value, out _))
                        variables.Add(ToDapVariable(name, value));
            }
        }
        else if (locals != null && _variableRefs.TryGetValue(variablesReference, out var parent))
        {
            foreach (var (name, value) in locals.Inspector.Expand(parent))
                variables.Add(ToDapVariable(name, value));
        }

        _io.WriteResponse(msg.Seq, msg.Command, true, new { variables });
    }

    private static object RegVar(string name, byte value) =>
        new { name, value = $"0x{value:X2} ({value})", type = "register", variablesReference = 0 };

    // Mints a fresh variablesReference (and registers it for a later
    // `variables` request to expand) only for a non-null object/array --
    // everything else is a leaf (variablesReference: 0).
    private object ToDapVariable(string name, InspectedValue value)
    {
        var reference = 0;
        if (value.IsReference && !value.IsNull)
        {
            reference = _nextVariablesReference++;
            _variableRefs[reference] = value;
        }
        return new
        {
            name,
            value = value.Summary,
            type = ObjectInspector.FriendlyTypeName(value.StaticType),
            variablesReference = reference,
        };
    }

    // Handles one SessionStop from Run/Step/Continue. In run-all mode, a
    // Passed/Failed result doesn't end the session -- it means only THAT
    // queued test is done, so this loops: log it, advance to the next
    // queued test's Run(), and re-evaluate its result, until either a real
    // breakpoint fires (in whichever test hits it) or the queue empties.
    private void ReportStop(SessionStop stop)
    {
        _variableRefs.Clear();
        _nextVariablesReference = 3; // 1 = Locals, 2 = Registers, both fixed

        while (true)
        {
            if (stop.Kind == SessionStopKind.Breakpoint || stop.Kind == SessionStopKind.Step)
            {
                var testLabel = _runAllQueue != null ? $" (in {_session.CurrentMethod?.Name})" : "";
                var reason = stop.Kind == SessionStopKind.Step ? "step" : "breakpoint";
                var description = (stop.Kind == SessionStopKind.Step ? "Paused after instruction step" : "Paused on breakpoint") + testLabel;
                _io.WriteEvent("stopped", new
                {
                    reason,
                    threadId = ThreadId,
                    allThreadsStopped = true,
                    description,
                });
                return;
            }

            var name = _session.CurrentMethod?.Name ?? "?";
            if (stop.Kind == SessionStopKind.Passed)
            {
                var suffix = stop.ReturnValue != null ? $" (returned {stop.ReturnValue})" : "";
                _io.WriteEvent("output", new { category = "console", output = $"PASSED  {name}{suffix}\n" });
            }
            else
            {
                _io.WriteEvent("output", new { category = "stderr", output = $"FAILED  {name}: {stop.Message}\n" });
            }

            if (_runAllQueue == null || ++_runAllIndex >= _runAllQueue.Count)
            {
                if (_runAllQueue != null)
                    _io.WriteEvent("output", new { category = "console", output = $"\nRan {_runAllQueue.Count} test(s).\n" });
                _io.WriteEvent("terminated", null);
                return;
            }

            stop = SafeRun(_runAllQueue[_runAllIndex]);
        }
    }

    // VS Code sends "granularity": "instruction" on next/stepIn/stepOut
    // automatically when the Disassembly View has focus (vs. omitted, or
    // "line"/"statement", when the source editor has focus) -- Arguments
    // defaults to an uninitialized JsonElement when a request carries no
    // "arguments" at all, so guard ValueKind before TryGetProperty.
    private static bool IsInstructionGranularity(DapIncomingMessage msg) =>
        msg.Arguments.ValueKind != JsonValueKind.Undefined
        && msg.Arguments.TryGetProperty("granularity", out var g)
        && g.GetString() == "instruction";

    private void HandleDisassemble(DapIncomingMessage msg)
    {
        var memRef = msg.Arguments.GetProperty("memoryReference").GetString();
        var baseAddress = ParseAddress(memRef);
        var offset = msg.Arguments.TryGetProperty("instructionOffset", out var o) ? o.GetInt32() : 0;
        var count = msg.Arguments.GetProperty("instructionCount").GetInt32();

        var startIndex = _listing.IndexOfRowAtOrBefore(baseAddress) + offset;

        var instructions = new List<object>();
        for (int i = 0; i < count; i++)
        {
            var rowIndex = startIndex + i;
            if (rowIndex < 0 || rowIndex >= _listing.RowCount)
            {
                // VS Code commonly over-fetches a bit before/after the
                // visible window -- pad with an explicit invalid entry
                // rather than returning fewer than `count` items.
                instructions.Add(new { address = "0x0", instruction = "??", presentationHint = "invalid" });
                continue;
            }

            var row = _listing.RowAt(rowIndex);
            var labels = _listing.LabelsAt(row.Address);
            var entry = _model.FindByAddress(row.Address);
            instructions.Add(new
            {
                address = $"0x{row.Address:X4}",
                instructionBytes = string.Join(" ", row.Bytes.Select(b => b.ToString("x2"))),
                instruction = row.SourceText,
                symbol = labels.Count > 0 ? labels[0] : null,
                location = entry != null ? new { name = entry.SourceFile, path = Path.Combine(_repoRoot, "Test", entry.SourceFile) } : null,
                line = entry?.Line,
            });
        }

        _io.WriteResponse(msg.Seq, msg.Command, true, new { instructions });
    }

    private static int ParseAddress(string s) =>
        s.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt32(s, 16) : int.Parse(s);

    // A single test's setup/execution can throw for reasons unrelated to
    // the debugger itself (e.g. an argument type WriteArgument doesn't
    // handle) -- in run-all mode that must not silently kill the whole
    // session's remaining tests, so convert it into a Failed stop instead
    // of letting it propagate. (Single-test mode still surfaces the same
    // exception normally, via Handle()'s own try/catch, since a bad
    // testSelector should be a real, visible error there.)
    private SessionStop SafeRun(string selector)
    {
        try
        {
            return _session.Run(selector);
        }
        catch (Exception ex) when (_runAllQueue != null)
        {
            return new SessionStop { Kind = SessionStopKind.Failed, Message = $"[debugger error] {ex.Message}" };
        }
    }
}
