using System;
using System.Collections.Generic;

namespace TestDebugger;

class ReplLoop
{
    private readonly TestSession _session;
    private readonly DebugMapModel _model;
    private readonly BreakpointResolver _resolver;

    // Non-null while a `run all` is in progress: every test in the assembly
    // runs in turn, auto-advancing past each PASSED/FAILED until either a
    // breakpoint fires in whichever test hits it, or the queue empties --
    // same shape as DapServer's run-all mode, sharing TestSession
    // .DiscoverAllTestSelectors.
    private List<string> _runAllQueue;
    private int _runAllIndex;

    public ReplLoop(TestSession session, DebugMapModel model, BreakpointResolver resolver)
    {
        _session = session;
        _model = model;
        _resolver = resolver;
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
                    case "quit":
                    case "exit":
                        return;
                    default:
                        Console.WriteLine($"Unknown command: {command} (break/run [TestClass.TestMethod|all]/step/continue/print/locals/quit)");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private void PrintLocal(string name)
    {
        var locals = _session.Locals;
        if (locals == null)
        {
            Console.WriteLine("No active session.");
            return;
        }
        if (locals.TryGetLocal(name, out var value, out var error))
            Console.WriteLine($"{name} = {value}");
        else
            Console.WriteLine(error);
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
            if (locals.TryGetLocal(name, out var value, out _))
                Console.WriteLine($"{name} = {value}");
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
            if (stop.Kind == SessionStopKind.Breakpoint)
            {
                Console.WriteLine($"Stopped at {stop.SourceFile}:{stop.Line} (in {_session.CurrentMethod?.Name})");
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
