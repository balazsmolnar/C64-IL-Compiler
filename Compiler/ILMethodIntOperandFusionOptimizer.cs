using System.Collections.Generic;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

// Int counterpart of ILMethodFloatOperandFusionOptimizer: fuses an 8- or
// 16-bit Add/Sub/Mul whose operands are a simple local, a parameter, an
// already-resolved field read, or an int literal, into one operation that
// copies each operand straight into add8_tail/sub8_tail/mul8_tail's (or
// their 16-bit counterparts') own scratch locations
// (asm/helper/arithmetic.asm), instead of each operand independently
// round-tripping through the hardware stack.
//
// Measured first, same discipline as the float pass: in Hunchback's own
// generated asm, roughly two thirds of its live 8-bit Add/Sub/Mul
// operands are exactly "a local/field plus a constant" (array/loop-index
// arithmetic, e.g. "arr[i + 1]"), and -- found only once this pass was
// actually being built -- essentially ALL of its 16-bit Add/Sub ops are
// "a local plus an int literal immediately widened via Conv_i8/Conv_u8"
// (e.g. "x + 40" on a long/ulong), which OpLdConst/OpConv_8_16 already
// special-case at compile time so the widening itself costs nothing at
// runtime (OpConv_8_16.Emit emits a bare "; conv" once the preceding Ldc
// has already produced the full 16-bit value) -- see
// TryMatchInt16Leaf below for why that shape still needs its own,
// slightly wider match than a single line.
//
// Two different per-operand conventions, matching whichever one the
// target op's existing (unfused) macro already uses -- see
// OpFusedIntBinOp's own comment:
//   - 8-bit add/sub: first operand -> register A directly, second -> a
//     named zp byte.
//   - 8-bit mul, and every 16-bit op: both operands -> named zp bytes/
//     pairs.
//
// Also fuses a bare truthiness branch (Brtrue/Brfalse, i.e. `if (x)`/
// `if (!x)`) on a simple leaf directly into register A -- see
// ExecuteBranchTruthiness/OpFusedBranchTruthiness below.
//
// Deliberately NOT matching Neg/Div/other compares (Clt/Cgt/Ceq, or the
// 2-operand branch_less_unsigned8-style comparisons), and NOT the output
// side (a result feeding straight into another op or a Stloc/Setfld) --
// same technique would apply to any of those, but none were part of what
// this pass's own Hunchback investigation measured (the 2-operand branch
// comparisons specifically: measured and found rare, ~30% simple-leaf-
// shaped versus the 1-operand truthiness case's 81%), and extending to
// the output side (particularly given the register-vs-zp-byte asymmetry
// above) is real additional complexity this pass doesn't attempt. Not a
// PeepholeRule, for the same reason ILMethodFloatOperandFusionOptimizer
// isn't one -- see that class's own comment.
class ILMethodIntOperandFusionOptimizer : ICompilerMethodPass
{
    // Mirrors each 8-bit op's own "jsr"-free computation (asm/helper/
    // arithmetic.asm) -- aIsRegister/aTarget say where the FIRST operand
    // needs to land; the second always goes to zp_param1_low.
    private static readonly Dictionary<ILOpCode, (string tailMacro, bool aIsRegister, string aTarget)> Int8Ops = new()
    {
        [ILOpCode.Add] = ("#add8_tail", true, null),
        [ILOpCode.Sub] = ("#sub8_tail", true, null),
        [ILOpCode.Mul] = ("#mul8_tail", false, "zp_param2_low"),
    };

    // Every 16-bit op already pulls both operands into named zp pairs --
    // first operand always zp_param2_low/high, second always
    // zp_param1_low/high.
    private static readonly Dictionary<ILOpCode, string> Int16TailMacro = new()
    {
        [ILOpCode.Add] = "#add16_tail",
        [ILOpCode.Sub] = "#sub16_tail",
        [ILOpCode.Mul] = "#mul16_tail",
    };

    // A promoted local (ILMethodPromoteLocalsPass) lives at a fixed
    // zero-page byte, never written to its nominal localsStack slot at
    // all -- OpLdloc.Emit/OpLdloc_s.Emit already special-case this
    // (emitting #zp_push_value8 instead of #locals_push_value8), and
    // ILMethodIncOptimizer/ILMethodDecOptimizer/ILMethodSetVariableOptimizer
    // all guard their own rules against it the same way this does
    // (ctx.PromotedLocalSlot(...) == null). IntOperandCopy/EmitToRegisterA
    // don't know about promotion at all, so without this guard a promoted
    // local's "rel_pos" is read as a real localsStack offset, loading
    // stale/wrong memory -- found for real via Test/ArithmeticTests.cs's
    // TestIncrement/TestDecrement (both use a promoted local; the
    // variable's contribution was silently lost, 5++ reading back as
    // just the literal 1). Only matters for 8-bit: promotion is "always a
    // single byte by eligibility" (locals_push_valueflt's own comment),
    // so 16-bit locals are never promoted and TryMatchInt16Leaf below
    // needs no equivalent check.
    private static bool IsFusableInt8Leaf(CompilerMethodContext context, ILOperation l)
    {
        if (l.SourceMethod != null)
            return false;
        if (l.Operation is IPushFldOperation pf)
            return !pf.IsFloat && !pf.Is16BitField;
        if (l.Operation is OpLdloc ldloc)
            return IsNarrowAndNotPromoted(context, l, ldloc.VarIndex);
        if (l.Operation is OpLdloc_s)
            return IsNarrowAndNotPromoted(context, l, (int)l.OriginalParameter);
        if (l.Operation is OpLdarg || l.Operation is OpLdarg_s || l.Operation is OpLdConst)
            return l.StackContent != null && l.StackContent.Count > 0 &&
                   l.StackContent.Last() != typeof(float) && l.StackContent.Last().GetStorageBytes() == 1;
        return false;
    }

    private static bool IsNarrowAndNotPromoted(CompilerMethodContext context, ILOperation l, int varIndex)
    {
        if (l.StackContent == null || l.StackContent.Count == 0 ||
            l.StackContent.Last() == typeof(float) || l.StackContent.Last().GetStorageBytes() != 1)
            return false;
        return context.PromotedLocalSlot(varIndex) == null;
    }

    // Returns the leaf's own representative line and how many PHYSICAL
    // lines it spans starting at `index` (1 normally, 2 for a widened int
    // constant), or null if lines[index] isn't a fusable 16-bit leaf at
    // all. The widened case -- Ldc_i4/_s/_const immediately followed by
    // Conv_i8/Conv_u8 -- only exists because OpLdConst itself already
    // looked ahead and emitted the full 16-bit push (OpLdConst.Is16Bit),
    // leaving the Conv line to emit nothing (OpConv_8_16.Emit); the Ldc
    // line is what IntOperandCopy needs (its RawParameter is the real
    // value), the Conv line is only retired (marked Optimized) for
    // bookkeeping, same as every other matched line.
    private static (ILOperation leaf, int span)? TryMatchInt16Leaf(List<ILOperation> lines, int index)
    {
        var l = lines[index];
        if (l.SourceMethod != null)
            return null;
        if (l.Operation is IPushFldOperation pf)
            return (!pf.IsFloat && pf.Is16BitField) ? (l, 1) : null;
        if (l.Operation is OpLdloc || l.Operation is OpLdloc_s || l.Operation is OpLdarg || l.Operation is OpLdarg_s)
            return (l.StackContent != null && l.StackContent.Count > 0 && l.StackContent.Last().GetStorageBytes() == 2)
                ? (l, 1) : null;
        if (l.Operation is OpLdConst && l.StackContent != null && l.StackContent.Count > 0 && l.StackContent.Last() == typeof(long) &&
            index + 1 < lines.Count && lines[index + 1].Operation is OpConv_8_16 &&
            lines[index + 1].SourceMethod == null && string.IsNullOrEmpty(lines[index + 1].Label))
            return (l, 2);
        return null;
    }

    public void Execute(CompilerMethodContext context)
    {
        if (!context.TypeContext.CompilerContext.Optimize)
            return;
        if (context.Lines == null)
            return;

        var lines = context.Lines;
        Execute8Bit(context, lines);
        Execute16Bit(lines);
        ExecuteBranchTruthiness(context, lines);
    }

    // [leaf] [Brtrue/Brfalse] -- a bare `if (x)`/`if (!x)` truthiness
    // test (branch_true8/branch_false8, asm/helper/branch.asm: both just
    // pull one value into register A then branch) on a simple leaf.
    // Measured in Hunchback's own generated asm: 81% of these are
    // already exactly this shape. 8-bit only -- IsFusableInt8Leaf's own
    // width check already excludes a 2-byte value (a string reference,
    // the one case OpJump's own comment calls out as needing real 16-bit
    // truthiness handling) from matching here at all, and there were no
    // branch_true16/branch_false16 uses anywhere in Hunchback to justify
    // handling that width too.
    private void ExecuteBranchTruthiness(CompilerMethodContext context, List<ILOperation> lines)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Optimized || !IsFusableInt8Leaf(context, lines[i]))
                continue;

            int j = NextEffective(lines, i);
            if (j < 0 || lines[j].SourceMethod != null || !string.IsNullOrEmpty(lines[j].Label))
                continue;

            bool isTrue = lines[j].OpCode == ILOpCode.Brtrue_s || lines[j].OpCode == ILOpCode.Brtrue;
            bool isFalse = lines[j].OpCode == ILOpCode.Brfalse_s || lines[j].OpCode == ILOpCode.Brfalse;
            if (!isTrue && !isFalse)
                continue;

            var fused = new OpFusedBranchTruthiness(lines[i], isTrue, lines[j].RawParameter.ToString());
            InsertFused(lines, fused, j, i, j);
        }
    }

    private void Execute8Bit(CompilerMethodContext context, List<ILOperation> lines)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Optimized || !IsFusableInt8Leaf(context, lines[i]))
                continue;

            int j = NextEffective(lines, i);
            if (j < 0 || !IsFusableInt8Leaf(context, lines[j]))
                continue;

            int k = NextEffective(lines, j);
            if (k < 0 || lines[k].SourceMethod != null || !string.IsNullOrEmpty(lines[k].Label))
                continue;

            if (!Int8Ops.TryGetValue(lines[k].OpCode, out var spec))
                continue;

            var fused = new OpFusedIntBinOp(lines[i], lines[j], spec.aIsRegister, spec.aTarget, "zp_param1_low", spec.tailMacro);
            InsertFused(lines, fused, k, i, j, k);
        }
    }

    private void Execute16Bit(List<ILOperation> lines)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Optimized)
                continue;

            var aMatch = TryMatchInt16Leaf(lines, i);
            if (aMatch == null)
                continue;
            int aEnd = i + aMatch.Value.span - 1;

            int j = NextEffective(lines, aEnd);
            if (j < 0)
                continue;

            var bMatch = TryMatchInt16Leaf(lines, j);
            if (bMatch == null)
                continue;
            int bEnd = j + bMatch.Value.span - 1;

            int k = NextEffective(lines, bEnd);
            if (k < 0 || lines[k].SourceMethod != null || !string.IsNullOrEmpty(lines[k].Label))
                continue;

            if (!Int16TailMacro.TryGetValue(lines[k].OpCode, out var tailMacro))
                continue;

            var fused = new OpFusedIntBinOp(aMatch.Value.leaf, bMatch.Value.leaf, false, "zp_param2_low", "zp_param1_low", tailMacro);

            var toMark = new List<int>();
            for (int x = i; x <= aEnd; x++) toMark.Add(x);
            for (int x = j; x <= bEnd; x++) toMark.Add(x);
            toMark.Add(k);
            InsertFused(lines, fused, k, toMark.ToArray());
        }
    }

    // The next non-Optimized line after `from`, or -1 if none exists, or
    // if any skipped (Optimized/dead) line in between is itself a branch
    // target -- see ILMethodFloatOperandFusionOptimizer's identical
    // helper for the full reasoning (duplicated here rather than shared,
    // same as that class keeps its own copy self-contained).
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
