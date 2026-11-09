using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

// Constant string (and small primitive) array literals as tables.
//
// Roslyn compiles `new[] { "a", "b" }` / `["a", "b"]` (and any array of
// primitives it does not turn into a RuntimeHelpers.InitializeArray blob:
// enums, short arrays, arrays with some non-constant elements) to one store
// per element:
//
//   ldc N; newarr T
//   dup; ldc i; ldstr "a" (or ldc v [conv.i8]); stelem    ; per element
//
// about 30 bytes of 6502 per element (#stack_duplicate8, the index push,
// #stack_push_pointer, #stelem16). Strings are not heap objects here: a
// string value is the 16-bit address of its constant text (OpLdstr), and a
// string[] is a plain 2-byte-element array with no reference fields
// (#newArr16, #stelem16), exactly like a ulong[]. So the whole literal is
// just bytes known at compile time: this pass replaces the Newarr and every
// constant element store right after it with one #newArrInit (the same
// macro ILStaticArrayInitializerPass uses for InitializeArray blobs), whose
// Init_Values row holds every slot -- `<string_x, >string_x` for a string,
// the value's bytes (low first) for a number, zeros for slots the run does
// not cover. Nothing changes for the GC: the array is allocated by
// newObjLInit with 0 reference fields, as #newArr16/#newArr would.
//
// The run is the maximal sequence of constant element stores that directly
// follows the Newarr (any index order, each index once); the first element
// that is not constant (a call, a local, a nested array...) ends it and
// keeps its ordinary code, which then stores into the array #newArrInit
// left on the stack. Only applied when the estimated saving is positive
// (the table also costs one byte per array byte), so a big array with one
// constant element stays as it was. Pay only for what you use: #newArrInit
// and newObjLInit are already part of the runtime.
class ILConstantArrayInitializerOptimizer : ICompilerMethodPass
{
    // Code bytes saved per folded element (a Dup, an index push, a value
    // push and a #stelem/#stelem16 call: 25-35 bytes), against the macro's
    // own size difference (#newArrInit is ~12 bytes bigger than a constant
    // length push + #newArr16) and one table byte per array byte.
    private const int SavedPerElement = 22;
    private const int FixedOverhead = 12;

    public void Execute(CompilerMethodContext context)
    {
        if (!context.CompilerContext.Optimize)
            return;

        var lines = context.Lines;
        for (int i = 1; i < lines.Count; i++)
        {
            if (lines[i].Optimized || lines[i].Operation is not OpNewArr || lines[i].Label != null)
                continue;
            var lengthLine = lines[i - 1];
            if (lengthLine.Optimized || lengthLine.Operation is not OpLdConst || !TryInt(lengthLine.RawParameter, out var length))
                continue;
            if (lengthLine.NextInstructions.Count != 1 || lengthLine.NextInstructions[0] != lines[i])
                continue;

            var arrayType = lines[i].StackContent[^1];
            var elementType = arrayType.GetElementType();
            if (elementType == null)
                continue;
            bool isString = elementType == typeof(string);
            if (!isString && (!elementType.IsValueType || elementType == typeof(float) || elementType.IsReferenceCounted()))
                continue;
            int elementBytes = elementType.GetStorageBytes();
            if (elementBytes != 1 && elementBytes != 2)
                continue;
            int total = length * elementBytes;
            if (length <= 0 || total > 255)
                continue;

            var slots = new string[total];
            for (int k = 0; k < total; k++)
                slots[k] = "0";
            var seen = new HashSet<int>();
            int p = i + 1;
            int lastStore = -1;
            int folded = 0;
            while (TryElement(lines, p, isString, elementBytes, out var index, out var bytes, out var store))
            {
                if (index < 0 || index >= length || !seen.Add(index))
                    break;
                for (int b = 0; b < elementBytes; b++)
                    slots[index * elementBytes + b] = bytes[b];
                lastStore = store;
                folded++;
                p = store + 1;
            }

            if (folded == 0 || folded * SavedPerElement <= FixedOverhead + total)
                continue;

            for (int k = i - 1; k <= lastStore; k++)
                lines[k].Optimized = true;

            var label = context.CompilerContext.GetInitValueLabel(string.Join(',', slots));
            var newOperation = new ILOperation
            {
                Operation = new OpNewArrInit(total, label),
                Position = lines[i].Position,
                StackContent = lines[lastStore].StackContent,
            };
            newOperation.RawParameter = newOperation.Operation.ConvertParameter(context, null);
            lines.Insert(lastStore + 1, newOperation);
            i = lastStore + 1;
        }
    }

    // dup; ldc index; <constant>; stelem -- all live, no branch target.
    private static bool TryElement(List<ILOperation> lines, int p, bool isString, int elementBytes,
        out int index, out string[] bytes, out int store)
    {
        index = 0;
        bytes = null;
        store = -1;
        if (p + 3 >= lines.Count)
            return false;
        if (!Live(lines[p]) || lines[p].Operation is not OpDup)
            return false;
        if (!Live(lines[p + 1]) || lines[p + 1].Operation is not OpLdConst || !TryInt(lines[p + 1].RawParameter, out index))
            return false;
        // An index widened to 16 bits is not a plain index push.
        if (IsWidening(lines[p + 2]))
            return false;

        var value = lines[p + 2];
        if (!Live(value))
            return false;
        int s = p + 3;
        if (isString)
        {
            if (value.Operation is not OpLdstr || value.RawParameter is not string text)
                return false;
            bytes = new[] { $"<{text}", $">{text}" };
        }
        else
        {
            if (value.Operation is not OpLdConst || !TryLong(value.RawParameter, out var v))
                return false;
            if (elementBytes == 2)
            {
                // ldc.i4 v; conv.i8 (or a plain ldc.i8).
                if (IsWidening(lines[s]))
                {
                    if (!Live(lines[s]))
                        return false;
                    s++;
                }
                else if (value.OpCode != ILOpCode.Ldc_i8)
                    return false;
                bytes = new[] { (v & 0xFF).ToString(CultureInfo.InvariantCulture), ((v >> 8) & 0xFF).ToString(CultureInfo.InvariantCulture) };
            }
            else
            {
                if (IsWidening(lines[s]) || v < -128 || v > 255)
                    return false;
                bytes = new[] { (v & 0xFF).ToString(CultureInfo.InvariantCulture) };
            }
        }

        if (s >= lines.Count || !Live(lines[s]) || lines[s].Operation is not OpStElem)
            return false;
        store = s;
        return true;
    }

    private static bool Live(ILOperation line) => !line.Optimized && line.Label == null;

    private static bool IsWidening(ILOperation line) =>
        line.OpCode is ILOpCode.Conv_i8 or ILOpCode.Conv_u8;

    private static bool TryInt(object raw, out int value)
    {
        value = 0;
        return raw != null && int.TryParse(raw.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryLong(object raw, out long value)
    {
        value = 0;
        return raw != null && long.TryParse(raw.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }
}
