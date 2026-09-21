using System;
using System.Collections.Generic;
using System.Linq;

namespace TestDebugger;

class ReplLoop
{
    private readonly TestSession _session;
    private readonly DebugMapModel _model;
    private readonly BreakpointResolver _resolver;
    private readonly DisassemblyListing _listing;

    // Non-null while a `run all` is in progress: every test in the assembly
    // runs in turn, auto-advancing past each PASSED/FAILED until either a
    // breakpoint fires in whichever test hits it, or the queue empties --
    // same shape as DapServer's run-all mode, sharing TestSession
    // .DiscoverAllTestSelectors.
    private List<string> _runAllQueue;
    private int _runAllIndex;

    public ReplLoop(TestSession session, DebugMapModel model, BreakpointResolver resolver, DisassemblyListing listing)
    {
        _session = session;
        _model = model;
        _resolver = resolver;
        _listing = listing;
    }

    // Usable both before the first `run` (queues onto the session's
    // breakpoint set, which Run() doesn't clear) and mid-session (takes
    // effect on the next step/continue).
    public void AddBreakpoint(string spec)
    {
        var addresses = _resolver.Resolve(spec, out var file, out var line);
        foreach (var a in addresses)
            _session.AddBreakpoint(a);
        Console.WriteLine($"Breakpoint set at {file}:{line}");
    }

    public void RunTest(string selector)
    {
        try
        {
            if (selector == "all" || selector == "*")
            {
                _runAllQueue = _session.DiscoverAllTestSelectors();
                _runAllIndex = 0;
                if (_runAllQueue.Count == 0)
                {
                    Console.WriteLine("No tests found.");
                    return;
                }
                Report(_session.Run(_runAllQueue[0]));
            }
            else
            {
                _runAllQueue = null;
                Report(_session.Run(selector));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public void Loop()
    {
        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (line == null)
                return;
            line = line.Trim();
            if (line.Length == 0)
                continue;

            var spaceIndex = line.IndexOf(' ');
            var command = spaceIndex < 0 ? line : line.Substring(0, spaceIndex);
            var rest = spaceIndex < 0 ? "" : line.Substring(spaceIndex + 1).Trim();

            try
            {
                switch (command)
                {
                    case "break":
                        AddBreakpoint(rest);
                        break;
                    case "run":
                        RunTest(rest);
                        break;
                    case "step":
                        Report(_session.Step());
                        break;
                    case "continue":
                        Report(_session.Continue());
                        break;
                    case "print":
                        PrintLocal(rest);
                        break;
                    case "locals":
                        PrintAllLocals();
                        break;
                    case "regs":
                        PrintRegisters();
                        break;
                    case "stepi":
                        Report(_session.StepInstruction());
                        break;
                    case "disasm":
                        PrintDisassembly(rest);
                        break;
                    case "quit":
                    case "exit":
                        return;
                    default:
                        Console.WriteLine("Unknown command: " + command +
                            " (break/run [TestClass.TestMethod|all]/step/stepi/continue/print [name|name.field|name[i]]/locals/regs/disasm [count]/quit)");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    // Supports drilling into fields/array elements via a dotted/bracketed
    // path ("print obj.Child.Id", "print arr[3]", "print arr[1].F") -- a
    // bare name is just the zero-segment case of the same walk. Each
    // segment is resolved via ObjectInspector.Expand, one level at a time,
    // so this only ever recurses exactly as deep as the user typed.
    private void PrintLocal(string path)
    {
        var locals = _session.Locals;
        if (locals == null)
        {
            Console.WriteLine("No active session.");
            return;
        }

        (string root, List<string> segments) parsed;
        try
        {
            parsed = VariablePath.Parse(path);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return;
        }

        if (!locals.TryGetLocalValue(parsed.root, out var value, out var error))
        {
            Console.WriteLine(error);
            return;
        }

        foreach (var segment in parsed.segments)
        {
            if (!value.IsReference)
            {
                Console.WriteLine($"{path}: \"{value.Summary}\" has no members.");
                return;
            }
            if (value.IsNull)
            {
                Console.WriteLine($"{path}: null reference.");
                return;
            }
            var match = locals.Inspector.Expand(value).FirstOrDefault(c => c.Name == segment);
            if (match.Value == null)
            {
                Console.WriteLine($"{path}: no member \"{segment}\".");
                return;
            }
            value = match.Value;
        }

        Console.WriteLine($"{path} = {value.Summary}");
    }

    private void PrintAllLocals()
    {
        var locals = _session.Locals;
        if (locals == null)
        {
            Console.WriteLine("No active session.");
            return;
        }
        foreach (var name in locals.LocalNames())
        {
            if (locals.TryGetLocalValue(name, out var value, out _))
                Console.WriteLine($"{name} = {value.Summary}");
        }
    }

    private void PrintRegisters()
    {
        var emu = _session.Emulator;
        if (emu == null)
        {
            Console.WriteLine("No active session.");
            return;
        }
        var r = emu.Registers;
        Console.WriteLine($"A=${r.A:X2} X=${r.X:X2} Y=${r.Y:X2} PC=${emu.ProgramCounter:X4} SP=${emu.HardwareStackPointer:X2} P=${r.P:X2} " +
            $"[{(r.N ? 'N' : '-')}{(r.V ? 'V' : '-')}--{(r.I ? 'I' : '-')}-{(r.Z ? 'Z' : '-')}{(r.C ? 'C' : '-')}]");
    }

    // "disasm" (defaults to 10 rows at the current PC) or "disasm N".
    private void PrintDisassembly(string arg)
    {
        var emu = _session.Emulator;
        if (emu == null)
        {
            Console.WriteLine("No active session.");
            return;
        }
        var count = string.IsNullOrEmpty(arg) ? 10 : int.Parse(arg);
        var startIndex = _listing.IndexOfRowAtOrBefore(emu.ProgramCounter);
        for (int i = 0; i < count && startIndex + i < _listing.RowCount; i++)
        {
            var row = _listing.RowAt(startIndex + i);
            foreach (var label in _listing.LabelsAt(row.Address))
                Console.WriteLine($"{label}:");
            var marker = row.Address == emu.ProgramCounter ? "=> " : "   ";
            var hex = string.Join(" ", row.Bytes.Select(b => b.ToString("x2")));
            Console.WriteLine($"{marker}${row.Address:X4}  {hex,-9}  {row.SourceText}");
        }
    }

    // In run-all mode, a Passed/Failed result doesn't end the session -- it
    // means only that one queued test is done, so this loops: print it,
    // advance to the next queued test's Run(), and re-evaluate its result,
    // until either a real breakpoint fires or the queue empties. Mirrors
    // DapServer.ReportStop exactly.
    private void Report(SessionStop stop)
    {
        while (true)
        {
            if (stop.Kind == SessionStopKind.Breakpoint || stop.Kind == SessionStopKind.Step)
            {
                if (stop.Line >= 0)
                    Console.WriteLine($"Stopped at {stop.SourceFile}:{stop.Line} (in {_session.CurrentMethod?.Name})");
                else
                    Console.WriteLine($"Stepped to ${_session.Emulator.ProgramCounter:X4} (in {_session.CurrentMethod?.Name}, no source line here)");
                return;
            }

            var name = _session.CurrentMethod?.Name ?? "?";
            if (stop.Kind == SessionStopKind.Passed)
                Console.WriteLine(stop.ReturnValue != null ? $"PASSED  {name} (returned {stop.ReturnValue})" : $"PASSED  {name}");
            else
                Console.WriteLine($"FAILED  {name}: {stop.Message}");

            if (_runAllQueue == null || ++_runAllIndex >= _runAllQueue.Count)
            {
                if (_runAllQueue != null)
                    Console.WriteLine($"Ran {_runAllQueue.Count} test(s).");
                return;
            }

            stop = SafeRun(_runAllQueue[_runAllIndex]);
        }
    }

    // See DapServer.SafeRun -- one test's setup/execution throwing must not
    // kill the rest of a `run all` queue.
    private SessionStop SafeRun(string selector)
    {
        try
        {
            return _session.Run(selector);
        }
        catch (Exception ex)
        {
            return new SessionStop { Kind = SessionStopKind.Failed, Message = $"[debugger error] {ex.Message}" };
        }
    }
}
