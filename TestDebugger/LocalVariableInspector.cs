using System;
using System.Linq;
using System.Reflection;
using C64TestFramework;
using Compiler;
using SimpleEmulator;

namespace TestDebugger;

// Resolves "print <name>" against the currently-running test method's
// locals. No call-stack walking (innermost/only frame -- the method
// injected by `run` -- is all that's supported; a known v1 limitation).
class LocalVariableInspector
{
    private readonly Emulator _emulator;
    private readonly DebugMapModel _model;
    private readonly MethodBase _testMethod;
    private readonly int _localsStackBase;

    public LocalVariableInspector(Emulator emulator, DebugMapModel model, MethodBase testMethod)
    {
        _emulator = emulator;
        _model = model;
        _testMethod = testMethod;
        _localsStackBase = model.ResolveLabelAddress("localsStack");
    }

    public bool TryGetLocal(string name, out object value, out string error)
    {
        var methodLabel = _testMethod.GetLabel();
        var local = _model.LocalsForMethod(methodLabel).FirstOrDefault(l => l.Name == name);
        if (local == null)
        {
            value = null;
            error = $"No local named \"{name}\" in {_testMethod.Name} (or it never had a corresponding sequence point).";
            return false;
        }

        var methodContext = new CompilerMethodContext { Method = _testMethod };
        var relPos = methodContext.GetLocalVariableReferencePosition(local.Index);
        var type = methodContext.GetLocalVariableType(local.Index);
        var stackPointerZp = _emulator.GetMemory(0x4b);
        var address = _localsStackBase + stackPointerZp - relPos;

        value = EmulatorArgumentMarshaling.ReadLocal(_emulator, type, address);
        error = null;
        return true;
    }

    public System.Collections.Generic.IEnumerable<string> LocalNames() =>
        _model.LocalsForMethod(_testMethod.GetLabel()).Select(l => l.Name);
}
