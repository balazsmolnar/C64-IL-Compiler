using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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

    private readonly string _repoRoot;
    private readonly DapIo _io;
    private DebugMapModel _model;
    private BreakpointResolver _resolver;
    private TestSession _session;
    private string _testSelector;
    private readonly Dictionary<string, HashSet<int>> _breakpointsByFile = new();
    private string _lastStopFile;
    private int _lastStopLine;

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
                _io.WriteResponse(msg.Seq, msg.Command, true, new { supportsConfigurationDoneRequest = true });
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
                        new { name = "Locals", variablesReference = LocalsVariablesReference, expensive = false }
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
                ReportStop(_session.Step());
                return true;

            case "continue":
                _io.WriteResponse(msg.Seq, msg.Command, true, new { allThreadsContinued = true });
                ReportStop(_session.Continue());
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

    private void HandleStackTrace(DapIncomingMessage msg)
    {
        var frames = new List<object>();
        if (_lastStopFile != null)
        {
            frames.Add(new
            {
                id = 1,
                name = _session.CurrentMethod?.Name,
                source = new { name = _lastStopFile, path = Path.Combine(_repoRoot, "Test", _lastStopFile) },
                line = _lastStopLine,
                column = 1,
            });
        }
        _io.WriteResponse(msg.Seq, msg.Command, true, new { stackFrames = frames, totalFrames = frames.Count });
    }

    private void HandleVariables(DapIncomingMessage msg)
    {
        var locals = _session.Locals;
        var variables = new List<object>();
        if (locals != null)
        {
            foreach (var name in locals.LocalNames())
            {
                if (locals.TryGetLocal(name, out var value, out _))
                    variables.Add(new { name, value = value?.ToString() ?? "null", variablesReference = 0 });
            }
        }
        _io.WriteResponse(msg.Seq, msg.Command, true, new { variables });
    }

    // Handles one SessionStop from Run/Step/Continue. In run-all mode, a
    // Passed/Failed result doesn't end the session -- it means only THAT
    // queued test is done, so this loops: log it, advance to the next
    // queued test's Run(), and re-evaluate its result, until either a real
    // breakpoint fires (in whichever test hits it) or the queue empties.
    private void ReportStop(SessionStop stop)
    {
        while (true)
        {
            if (stop.Kind == SessionStopKind.Breakpoint)
            {
                _lastStopFile = stop.SourceFile;
                _lastStopLine = stop.Line;
                var testLabel = _runAllQueue != null ? $" (in {_session.CurrentMethod?.Name})" : "";
                _io.WriteEvent("stopped", new
                {
                    reason = "breakpoint",
                    threadId = ThreadId,
                    allThreadsStopped = true,
                    description = "Paused on breakpoint" + testLabel,
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
