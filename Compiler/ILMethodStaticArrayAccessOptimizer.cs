using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

// Array element access through a STATIC array field with a simple index:
//
//   Ldsfld arr; <index>; Ldelem          (8- or 16-bit value-type element)
//   Ldsfld arr; <index>; <value...>; Stelem
//
// where <index> is an int literal, an unpromoted 8-bit local/parameter or
// an 8-bit static field. Unfused, the array handle and the index are each
// pushed on the hardware stack and pulled back by #ldelem/#stelem; fused,
// the index goes straight into Y and the handle into X (OpStaticArrayLoad /
// OpStaticArrayStore). For a store the value expression in between may be
// anything that leaves the array field and the index untouched (no call, no
// store, no branch target): the handle and index are read after it instead
// of before, which only reorders reads of values nothing in between writes.
class ILMethodStaticArrayAccessOptimizer : ICompilerMethodPass
{
    public void Execute(CompilerMethodContext context)
    {
        if (!context.TypeContext.CompilerContext.Optimize || context.Lines == null)
            return;

        var lines = context.Lines;
        for (int i = 0; i < lines.Count; i++)
        {
            var arr = lines[i];
            if (arr.Optimized || !IsStaticArray(arr, out var elementBytes))
                continue;

            int j = NextEffective(lines, i);
            if (j < 0 || !string.IsNullOrEmpty(lines[j].Label) || !IsIndexLeaf(context, lines[j]))
                continue;

            int k = NextEffective(lines, j);
            if (k < 0 || !string.IsNullOrEmpty(lines[k].Label))
                continue;

            if (lines[k].Operation is OpLdElem && SameElement(lines[k], arr))
            {
                InsertFused(lines, new OpStaticArrayLoad(arr, lines[j], elementBytes), k, i, j, k);
                continue;
            }

            int s = FindStore(lines, i, j);
            if (s >= 0)
                InsertFused(lines, new OpStaticArrayStore(arr, lines[j], elementBytes), s, i, j, s);
        }
    }

    // A static field holding an array of 1- or 2-byte non-float value types.
    private static bool IsStaticArray(ILOperation l, out int elementBytes)
    {
        elementBytes = 0;
        if (l.OpCode != ILOpCode.Ldsfld || l.Operation is not OpLdsld || l.StackContent == null || l.StackContent.Count == 0)
            return false;
        var type = l.StackContent.Last();
        if (!type.IsArray)
            return false;
        var element = type.GetElementType();
        if (!element.IsValueType || element == typeof(float))
            return false;
        elementBytes = element.GetStorageBytes();
        return elementBytes == 1 || elementBytes == 2;
    }

    private static bool SameElement(ILOperation access, ILOperation arr) =>
        access.RawParameter is Type t && t == arr.StackContent.Last().GetElementType();

    private static bool IsIndexLeaf(CompilerMethodContext context, ILOperation l)
    {
        if (l.StackContent == null || l.StackContent.Count == 0 || l.StackContent.Last().GetStorageBytes() != 1 ||
            l.StackContent.Last() == typeof(float))
            return false;
        switch (l.Operation)
        {
            case OpLdConst:
                return l.RawParameter is int;
            case OpLdloc ldloc:
                return context.PromotedLocalSlot(l.SourceMethod, ldloc.VarIndex) == null && l.SourceMethod == null;
            case OpLdloc_s:
                return context.PromotedLocalSlot(l.SourceMethod, (int)l.OriginalParameter) == null && l.SourceMethod == null;
            case OpLdarg arg:
                return context.PromotedParameterSlot(arg.ArgIndex) == null && l.SourceMethod == null;
            case OpLdarg_s:
                return context.PromotedParameterSlot((int)l.OriginalParameter) == null && l.SourceMethod == null;
            case OpLdsld:
                return l.OpCode == ILOpCode.Ldsfld;
            default:
                return false;
        }
    }

    // The Stelem consuming the array pushed at `arrIndex`, if everything
    // between the index and it is safe to run before the handle and the
    // index are read; -1 otherwise.
    private static int FindStore(List<ILOperation> lines, int arrIndex, int indexIndex)
    {
        int depth = lines[arrIndex].StackContent.Count - 1;     // below the array
        for (int x = indexIndex + 1; x < lines.Count; x++)
        {
            var l = lines[x];
            if (!string.IsNullOrEmpty(l.Label))
                return -1;
            if (l.Optimized)
                continue;
            if (l.Operation is OpStElem)
            {
                if (l.StackContent.Count != depth || !SameElement(l, lines[arrIndex]))
                    return -1;
                return x;
            }
            if (!IsPureValueOp(l))
                return -1;
            // Something consumed the array or the index: not a plain store.
            if (l.StackContent == null || l.StackContent.Count < depth + 2)
                return -1;
        }
        return -1;
    }

    // Pushes and arithmetic only: no call, no store of any kind, no
    // control flow.
    private static bool IsPureValueOp(ILOperation l)
    {
        switch (l.OpCode)
        {
            case ILOpCode.Ldc_i4: case ILOpCode.Ldc_i4_s: case ILOpCode.Ldc_i4_0: case ILOpCode.Ldc_i4_1:
            case ILOpCode.Ldc_i4_2: case ILOpCode.Ldc_i4_3: case ILOpCode.Ldc_i4_4: case ILOpCode.Ldc_i4_5:
            case ILOpCode.Ldc_i4_6: case ILOpCode.Ldc_i4_7: case ILOpCode.Ldc_i4_8: case ILOpCode.Ldc_i4_m1:
            case ILOpCode.Ldloc: case ILOpCode.Ldloc_s: case ILOpCode.Ldloc_0: case ILOpCode.Ldloc_1:
            case ILOpCode.Ldloc_2: case ILOpCode.Ldloc_3:
            case ILOpCode.Ldarg: case ILOpCode.Ldarg_s: case ILOpCode.Ldarg_0: case ILOpCode.Ldarg_1:
            case ILOpCode.Ldarg_2: case ILOpCode.Ldarg_3:
            case ILOpCode.Ldsfld: case ILOpCode.Ldfld: case ILOpCode.Ldelem_u1: case ILOpCode.Ldelem_i1:
            case ILOpCode.Ldelem_u4: case ILOpCode.Ldelem_i4: case ILOpCode.Ldelem_i8:
            case ILOpCode.Add: case ILOpCode.Sub: case ILOpCode.Mul: case ILOpCode.And: case ILOpCode.Or:
            case ILOpCode.Xor: case ILOpCode.Shl: case ILOpCode.Shr: case ILOpCode.Shr_un:
            case ILOpCode.Div_un: case ILOpCode.Rem_un: case ILOpCode.Not: case ILOpCode.Neg:
            case ILOpCode.Conv_u4: case ILOpCode.Conv_i4: case ILOpCode.Conv_u8: case ILOpCode.Conv_i8:
            case ILOpCode.Conv_u1: case ILOpCode.Conv_u2: case ILOpCode.Conv_i1: case ILOpCode.Conv_i2:
            case ILOpCode.Conv_u: case ILOpCode.Conv_i:
            case ILOpCode.Ceq: case ILOpCode.Cgt: case ILOpCode.Cgt_un: case ILOpCode.Clt: case ILOpCode.Clt_un:
            case ILOpCode.Dup:
                return true;
            case ILOpCode.Nop:
                // Fused replacements of the above carry Nop; only pure ones
                // are recognised (field reads and int arithmetic/widening).
                return l.Operation is IPushFldOperation || l.Operation is OpFusedIntBinOp ||
                       l.Operation is OpStaticArrayLoad || l.Operation is OpDivConst ||
                       l.Operation.GetType() == typeof(OpArithmetic2) || l.Operation.GetType() == typeof(OpArithmetic1);
            default:
                return false;
        }
    }

    private static int NextEffective(List<ILOperation> lines, int from)
    {
        for (int idx = from + 1; idx < lines.Count; idx++)
        {
            if (!lines[idx].Optimized)
                return idx;
            if (!string.IsNullOrEmpty(lines[idx].Label))
                return -1;
        }
        return -1;
    }

    private static void InsertFused(List<ILOperation> lines, OpBase op, int insertAfterIndex, params int[] indicesToMarkOptimized)
    {
        var newOperation = new ILOperation
        {
            Operation = op,
            StackContent = lines[insertAfterIndex].StackContent,
        };
        lines.Insert(insertAfterIndex + 1, newOperation);
        foreach (var idx in indicesToMarkOptimized)
            lines[idx].Optimized = true;
    }
}
