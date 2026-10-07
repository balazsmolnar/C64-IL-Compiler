using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Compiler;
using SimpleEmulator;

namespace TestDebugger;

// Resolves "print <name>" against the currently-running test method's
// locals, parameters, and (for an instance method) `this`'s own fields.
// No call-stack walking (innermost/only frame -- the method injected by
// `run` -- is all that's supported; a known v1 limitation).
class LocalVariableInspector
{
    private readonly IDebugTarget _emulator;
    private readonly DebugMapModel _model;
    private readonly MethodBase _testMethod;
    private readonly int _localsStackBase;
    private readonly ObjectInspector _objectInspector;
    private readonly CompilerMethodContext _methodContext;

    public LocalVariableInspector(IDebugTarget emulator, DebugMapModel model, MethodBase testMethod)
    {
        _emulator = emulator;
        _model = model;
        _testMethod = testMethod;
        _localsStackBase = model.ResolveLabelAddress("localsStack");
        _objectInspector = new ObjectInspector(emulator, model);
        _methodContext = new CompilerMethodContext { Method = testMethod };
    }

    public ObjectInspector Inspector => _objectInspector;

    private InspectedValue ReadAtRelPos(Type type, int relPos)
    {
        var stackPointerZp = _emulator.GetMemory(0x4b);
        var address = _localsStackBase + stackPointerZp - relPos;
        return _objectInspector.Read(type, address);
    }

    // `this` is always rel_pos 1 for any instance method (the very first
    // thing the prologue pulls) -- CompilerMethodContext
    // .GetParameterReferencePosition(0) already returns exactly this for
    // an instance method, but its matching GetParameterType(0) returns a
    // generic typeof(object) (the compiler itself never needs `this`'s
    // real type during emit, only its fixed 2-byte storage size) --
    // DeclaringType is used here instead so its fields can actually be
    // expanded/named.
    private bool TryGetThis(out InspectedValue value)
    {
        value = null;
        if (_testMethod.IsStatic)
            return false;
        value = ReadAtRelPos(_testMethod.DeclaringType, 1);
        return true;
    }

    public bool TryGetLocalValue(string name, out InspectedValue value, out string error)
    {
        var methodLabel = _testMethod.GetLabel();
        var local = _model.LocalsForMethod(methodLabel).FirstOrDefault(l => l.Name == name);
        if (local != null)
        {
            var relPos = _methodContext.GetLocalVariableReferencePosition(local.Index);
            var type = _methodContext.GetLocalVariableType(local.Index);
            value = ReadAtRelPos(type, relPos);
            error = null;
            return true;
        }

        if (name == "this" && TryGetThis(out value))
        {
            error = null;
            return true;
        }

        // Parameter names come straight from the assembly's own metadata
        // (the Param table, not PDB-dependent) via reflection -- no
        // separate debugmap.txt entry needed, unlike real locals (which
        // only exist as named PDB scope entries, never in metadata).
        var parameters = _testMethod.GetParameters();
        var paramIndex = Array.FindIndex(parameters, p => p.Name == name);
        if (paramIndex >= 0)
        {
            var unifiedIndex = paramIndex + (_testMethod.IsStatic ? 0 : 1);
            var relPos = _methodContext.GetParameterReferencePosition(unifiedIndex);
            value = ReadAtRelPos(parameters[paramIndex].ParameterType, relPos);
            error = null;
            return true;
        }

        // Not a local or a parameter -- fall back to an instance field of
        // `this`, the same way a bare field name (no "this." prefix)
        // resolves inside the method's own C# source. Covers e.g. a
        // Watch entry for "sprite_" when stopped inside an instance
        // method whose body just writes "sprite_.Visible = ...".
        if (TryGetThis(out var thisValue))
        {
            var match = _objectInspector.Expand(thisValue).FirstOrDefault(c => c.Name == name);
            if (match.Value != null)
            {
                value = match.Value;
                error = null;
                return true;
            }
        }

        value = null;
        error = $"No local named \"{name}\" in {_testMethod.Name} (or it never had a corresponding sequence point).";
        return false;
    }

    // Real locals, then real parameters, then -- flattened in directly,
    // unprefixed -- `this`'s own fields, so e.g. "sprite_" shows up in
    // the Locals/Variables panel the same way it resolves when typed
    // into Watch (TryGetLocalValue's own fallback above). Deliberately
    // never lists "this" itself by name (no instance method actually
    // declares a local/parameter called that, so it would be a strange,
    // redundant extra entry rather than useful).
    public IEnumerable<string> LocalNames()
    {
        foreach (var name in _model.LocalsForMethod(_testMethod.GetLabel()).Select(l => l.Name))
            yield return name;
        foreach (var p in _testMethod.GetParameters())
            if (!string.IsNullOrEmpty(p.Name))
                yield return p.Name;
        if (TryGetThis(out var thisValue))
            foreach (var (name, _) in _objectInspector.Expand(thisValue))
                yield return name;
    }

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
