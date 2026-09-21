using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using C64TestFramework;
using Compiler;
using SimpleEmulator;

namespace TestDebugger;

// Resolves reference-typed values (plain objects and arrays -- the runtime
// can't tell them apart, see below) on top of EmulatorArgumentMarshaling
// .ReadLocal, which only handles scalars. Lives in TestDebugger (not
// C64TestFramework, where ReadLocal itself lives) because it needs
// Compiler's internals (CompilerContext.GetFieldPosition) and only
// TestDebugger has the InternalsVisibleTo grant for that
// (Compiler/AssemblyInfo.cs).
//
// Memory model (confirmed empirically against asm/helper/objectTables.asm,
// object.asm, heap.asm -- see the plan this was built from):
// any reference-typed value is a single byte -- a "handle", 0-255, index
// into 7 parallel 256-byte tables (objTableLow/High/Size/...). Heap address
// = objTableLow[handle] | (objTableHigh[handle]<<8), no header before
// field/element 0. There's no runtime tag distinguishing "array" from
// "plain object" from "which class" -- only the value's own static,
// reflected .NET Type (carried forward at each recursion level) says how
// to interpret a handle.
class ObjectInspector
{
    private readonly Emulator _emulator;
    private readonly CompilerContext _compilerContext = new();
    private readonly int _objTableLow;
    private readonly int _objTableHigh;
    private readonly int _objTableSize;

    public ObjectInspector(Emulator emulator, DebugMapModel model)
    {
        _emulator = emulator;
        _objTableLow = model.ResolveLabelAddress("objTableLow");
        _objTableHigh = model.ResolveLabelAddress("objTableHigh");
        _objTableSize = model.ResolveLabelAddress("objTableSize");
    }

    // Reads one value at `address` as `type`. For anything ReadLocal
    // already handles (scalars), delegates straight to it. For a
    // reference-counted type (IsReferenceCounted, not a bare !IsValueType
    // check -- that correctly excludes C64Lib.* types, which are also
    // single-byte but aren't part of the object-table system and keep
    // falling through to ReadLocal's own null, unchanged from before this
    // feature), reads the 1-byte handle and resolves it.
    public InspectedValue Read(Type type, int address)
    {
        if (!type.IsReferenceCounted())
        {
            var scalar = EmulatorArgumentMarshaling.ReadLocal(_emulator, type, address);
            return new InspectedValue
            {
                StaticType = type,
                IsReference = false,
                Summary = scalar?.ToString() ?? "null",
            };
        }

        var handle = _emulator.GetMemory(address);
        var value = new InspectedValue { StaticType = type, IsReference = true, Handle = handle };
        if (!TryResolveHeapAddress(handle, out _))
        {
            value.IsNull = true;
            value.Summary = "null";
            return value;
        }
        value.Summary = Summarize(type, handle);
        return value;
    }

    // Expands exactly one level of an already-resolved, non-null
    // reference. Purely lazy -- only ever runs for one specific value on
    // explicit request (a DAP `variables` request against one specific
    // variablesReference, or one `print a.b.c` segment) -- so no
    // depth/cycle guard is needed anywhere, even for a genuinely
    // self-referential object graph.
    public List<(string Name, InspectedValue Value)> Expand(InspectedValue value)
    {
        if (!value.IsReference || value.IsNull || !TryResolveHeapAddress(value.Handle, out var heapAddress))
            return new List<(string, InspectedValue)>();

        return value.StaticType.IsArray
            ? ExpandArray(value.StaticType.GetElementType(), heapAddress, value.Handle)
            : ExpandObject(value.StaticType, heapAddress);
    }

    // C# keyword for primitives, recurses through "[]" for arrays, else
    // the class name.
    public static string FriendlyTypeName(Type type)
    {
        if (type == typeof(int)) return "int";
        if (type == typeof(uint)) return "uint";
        if (type == typeof(byte)) return "byte";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(long)) return "long";
        if (type == typeof(ulong)) return "ulong";
        if (type == typeof(float)) return "float";
        if (type == typeof(string)) return "string";
        if (type.IsArray) return FriendlyTypeName(type.GetElementType()) + "[]";
        return type.Name;
    }

    // A handle of 0 can never be a real object -- asm/helper/object.asm's
    // findEmptySlot does `ldx #0 / inx` BEFORE ever testing a slot, so
    // handle 0 is structurally never returned as allocated (the smallest
    // real handle is 1). objTableHigh[handle]==0 catches every other
    // never-allocated handle (locals aren't zero-initialized -- a
    // reference-typed local read before its first real assignment can be
    // genuine stale garbage, not reliably 0). Both an explicit C# `null`
    // and uninitialized garbage collapse onto this same check.
    private bool TryResolveHeapAddress(int handle, out int heapAddress)
    {
        if (handle == 0 || _emulator.GetMemory(_objTableHigh + handle) == 0)
        {
            heapAddress = 0;
            return false;
        }
        heapAddress = _emulator.GetMemory(_objTableHigh + handle) * 256 + _emulator.GetMemory(_objTableLow + handle);
        return true;
    }

    private string Summarize(Type type, int handle)
    {
        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            var length = _emulator.GetMemory(_objTableSize + handle) / elementType.GetStorageBytes();
            return $"{FriendlyTypeName(elementType)}[{length}]";
        }
        return $"{type.Name}#{handle}";
    }

    private List<(string, InspectedValue)> ExpandObject(Type type, int heapAddress)
    {
        var result = new List<(string, InspectedValue)>();
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => !f.IsLiteral);
        foreach (var field in fields)
            result.Add((field.Name, Read(field.FieldType, heapAddress + _compilerContext.GetFieldPosition(field))));
        return result;
    }

    private List<(string, InspectedValue)> ExpandArray(Type elementType, int heapAddress, int handle)
    {
        var result = new List<(string, InspectedValue)>();
        var elementSize = elementType.GetStorageBytes();
        var length = _emulator.GetMemory(_objTableSize + handle) / elementSize;
        for (int i = 0; i < length; i++)
            result.Add(($"[{i}]", Read(elementType, heapAddress + i * elementSize)));
        return result;
    }
}
