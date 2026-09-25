using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Compiler;
using SimpleEmulator;

namespace TestDebugger;

// Resolves "print <name>" against the currently-running test method's
// locals. No call-stack walking (innermost/only frame -- the method
// injected by `run` -- is all that's supported; a known v1 limitation).
class LocalVariableInspector
{
    private readonly IMemoryReader _emulator;
    private readonly DebugMapModel _model;
    private readonly MethodBase _testMethod;
    private readonly int _localsStackBase;
    private readonly ObjectInspector _objectInspector;

    public LocalVariableInspector(IMemoryReader emulator, DebugMapModel model, MethodBase testMethod)
    {
        _emulator = emulator;
        _model = model;
        _testMethod = testMethod;
        _localsStackBase = model.ResolveLabelAddress("localsStack");
        _objectInspector = new ObjectInspector(emulator, model);
    }

    public ObjectInspector Inspector => _objectInspector;

    public bool TryGetLocalValue(string name, out InspectedValue value, out string error)
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

        value = _objectInspector.Read(type, address);
        error = null;
        return true;
    }

    public IEnumerable<string> LocalNames() =>
        _model.LocalsForMethod(_testMethod.GetLabel()).Select(l => l.Name);

    // Resolves a full "root.field[i].field" path (VariablePath.Parse's
    // shape) against this frame's locals -- shared by ReplLoop's `print`
    // and DapServer's `evaluate` (Debug Console / Watch panel), so both
    // walk exactly the same way instead of duplicating the segment loop.
    public bool TryResolvePath(string path, out InspectedValue value, out string error)
    {
        List<string> segments;
        string root;
        try
        {
            (root, segments) = VariablePath.Parse(path);
        }
        catch (ArgumentException ex)
        {
            value = null;
            error = ex.Message;
            return false;
        }

        if (!TryGetLocalValue(root, out value, out error))
            return false;

        foreach (var segment in segments)
        {
            if (!value.IsReference)
            {
                error = $"\"{value.Summary}\" has no members.";
                value = null;
                return false;
            }
            if (value.IsNull)
            {
                error = "null reference.";
                value = null;
                return false;
            }
            var match = _objectInspector.Expand(value).FirstOrDefault(c => c.Name == segment);
            if (match.Value == null)
            {
                error = $"no member \"{segment}\".";
                value = null;
                return false;
            }
            value = match.Value;
        }
        return true;
    }
}
