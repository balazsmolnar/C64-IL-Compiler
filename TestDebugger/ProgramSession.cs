using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Compiler;
using SimpleEmulator;

namespace TestDebugger;

// Debug session for a whole program running on VICE. Unlike TestSession there
// is no test method to run, no argument marshaling and no result byte: the
// machine just runs until a breakpoint (or a pause/step), and the "current
// method" for locals is derived from the PC every time it's asked for.
class ProgramSession : IDebugSession
{
    private readonly DebugMapModel _model;
    private readonly ViceTarget _target;
    private readonly Dictionary<string, MethodBase> _methodsByLabel;
    private readonly List<SourceLineEntry> _linesByAddress;
    private HashSet<int> _userBreakpoints = new();

    public ProgramSession(DebugMapModel model, ViceTarget target, Assembly programAssembly)
    {
        _model = model;
        _target = target;
        _target.RunTimeoutMs = -1;
        _methodsByLabel = IndexMethods(programAssembly);
        _linesByAddress = model.Lines.OrderBy(l => l.Address).ToList();
    }

    public IDebugTarget Target => _target;

    public void ReplaceBreakpoints(IEnumerable<int> addresses) => _userBreakpoints = addresses.ToHashSet();

    // The method whose code contains the current PC: the nearest source line
    // at or before it, if that's close enough to plausibly be the same method
    // (not some ROM routine far past the last compiled method). Null when
    // there's no such method.
    public MethodBase CurrentMethod
    {
        get
        {
            var pc = _target.ProgramCounter;
            SourceLineEntry best = null;
            foreach (var line in _linesByAddress)
            {
                if (line.Address > pc)
                    break;
                best = line;
            }
            if (best == null || pc - best.Address > 256)
                return null;
            return _methodsByLabel.TryGetValue(best.MethodLabel, out var method) ? method : null;
        }
    }

    public LocalVariableInspector Locals
    {
        get
        {
            var method = CurrentMethod;
            return method == null ? null : new LocalVariableInspector(_target, _model, method);
        }
    }

    public SessionStop Continue() => Advance(_userBreakpoints, SessionStopKind.Breakpoint);

    // Runs to the next source line anywhere (including into a called
    // method), the same way TestSession.Step does.
    public SessionStop Step() =>
        Advance(_model.AllSequencePointAddresses().Concat(_userBreakpoints).ToHashSet(), SessionStopKind.Step);

    public SessionStop StepInstruction()
    {
        try
        {
            if (!_target.StepOne())
                return Jammed();
            return StopAt(_target.ProgramCounter, SessionStopKind.Step);
        }
        catch (ViceProtocolException ex)
        {
            return Exited(ex.Message);
        }
    }

    // Stops a running machine; whatever thread is blocked in Continue/Step
    // then returns a Pause stop.
    public void Pause() => _target.Pause();

    private SessionStop Advance(HashSet<int> stopAt, SessionStopKind hitKind)
    {
        try
        {
            var result = _target.RunUntil(stopAt, 0, out var stoppedAt, out _);
            if (result == RunResult.Halted)
                return Jammed();
            return StopAt(stoppedAt, stopAt.Contains(stoppedAt) ? hitKind : SessionStopKind.Pause);
        }
        catch (ViceProtocolException ex)
        {
            return Exited(ex.Message);
        }
    }

    private SessionStop StopAt(int pc, SessionStopKind kind)
    {
        var entry = _model.FindByAddress(pc);
        return new SessionStop
        {
            Kind = kind,
            SourceFile = entry?.SourceFile ?? "?",
            Line = entry?.Line ?? -1,
        };
    }

    private SessionStop Jammed() =>
        Exited($"The CPU jammed at ${_target.ProgramCounter:X4}.");

    private static SessionStop Exited(string message) =>
        new SessionStop { Kind = SessionStopKind.Exited, Message = message };

    private static Dictionary<string, MethodBase> IndexMethods(Assembly assembly)
    {
        const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                               | BindingFlags.Static | BindingFlags.DeclaredOnly;
        var result = new Dictionary<string, MethodBase>();
        foreach (var type in assembly.GetTypes())
        {
            foreach (MethodBase method in type.GetMethods(all).Cast<MethodBase>().Concat(type.GetConstructors(all)))
            {
                try
                {
                    result[method.GetLabel()] = method;
                }
                catch
                {
                    // A method GetLabel can't name isn't one the compiler emitted either.
                }
            }
        }
        return result;
    }
}
