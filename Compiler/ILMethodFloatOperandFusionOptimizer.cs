using System.Collections.Generic;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

// Fuses a binary/unary float op whose operand(s) are a simple local, a
// parameter, or an already-resolved field read (any IPushFldOperation --
// ILPropertyGettterOptimizer has already collapsed a raw "this.field_"
// access to one of those earlier in this same optimizer group, which is
// why this doesn't also need to match a raw, unfused Ldfld) into one
// operation that copies straight from the source into zp_flt_a/zp_flt_b
// (locals_copy_valueflt / pushfldflt_direct, asm/helper/localsStack.asm /
// optimized.asm), instead of each operand independently round-tripping
// through the hardware stack via Ldloc/Ldarg/pushfld's own push +
// addflt/subflt/mulflt/divflt/negateflt/compareLessflt/compareGreaterflt's
// own stack_pull_mflpt (asm/helper/float.asm) -- 10 bytes and ~35 cycles of
// pure pha/pla overhead per fused operand. The jsr + result-push tail is
// unchanged, so it composes with whatever the result feeds next (another
// op, a Stloc, a Setfld) without this pass needing to know or care what
// that is.
//
// NOT a PeepholeRule/PeepholeOptimizerPass, deliberately -- same reason
// ILMethodCachedFieldAccessPass (which this pass's own matched operands
// often depend on having already run) isn't one either: PeepholeRule only
// ever matches STRICTLY CONSECUTIVE lines[i], lines[i+1], ...,
// lines[i+n-1]. But a window like "a_ + b_" (two field reads) ends up as
// [PushFld(a_)] [OPT Ldarg_0] [OPT Ldfld] [PushFldCached(b_)] [Add] once
// ILPropertyGettterOptimizer has already fused each "Ldarg_0;Ldfld" pair
// in place -- the SECOND field read's own dead (now-Optimized) source
// lines physically sit BETWEEN the two live operand-producing lines this
// pass needs to match, breaking strict adjacency. This pass scans for the
// next EFFECTIVE (non-Optimized) line instead, the same
// "effectiveLines = context.Lines.Where(l => !l.Optimized)" idea that
// pass already uses, generalized just enough to need no full CFG walk:
// a straight-line IL sequence for one expression never branches, so a
// simple skip-dead-lines scan is enough, PROVIDED none of the skipped
// lines themselves are a branch target (checked below) -- otherwise
// falling through to the fused code mid-window would silently skip
// whatever the branch's own source code meant to do first.
//
// Deliberately NOT matching a float constant (Ldc) or the output side
// (result feeding straight into Stloc/Setfld) -- same technique would
// apply to both, left as a natural follow-up; this covers the operand
// side only, which both the Demo rotating-cube investigation that
// motivated it and this compiler's existing peephole conventions (see
// ILMethodMulConstOptimizer/ILMethodDivConstOptimizer) scope tightly to
// one well-understood pattern at a time.
class ILMethodFloatOperandFusionOptimizer : ICompilerMethodPass
{
    // SourceMethod != null means this line was spliced in from an inlined
    // callee's own body (ILMethodInliningPass) -- its VarIndex/RelPos
    // resolves against the CALLEE, not context.Method, the same "VarIndex
    // hazard" PeepholeRule.TryApply centrally guards against for every
    // other optimizer. Checked here (not centrally) since this pass
    // doesn't go through PeepholeRule at all -- see this class's own
    // top-of-file comment for why.
    //
    // IPushFldOperation's own IsFloat (not StackContent) is the right
    // check for a field operand: ILPropertyGettterOptimizer's common
    // "Ldarg_0;Ldfld" rule (ILPropertyGetterOptimizer.cs) deliberately
    // leaves the synthesized OpPushFld's StackContent null (see its own
    // comment), computing IsFloat once at build time from the original
    // Ldfld's StackContent instead -- relying on StackContent here would
    // silently never match the single most common field-read shape.
    private static bool IsFusableFloatOperand(ILOperation l)
    {
        if (l.SourceMethod != null)
            return false;
        if (l.Operation is IPushFldOperation pf)
            return pf.IsFloat;
        if (l.Operation is OpLdloc || l.Operation is OpLdloc_s ||
            l.Operation is OpLdarg || l.Operation is OpLdarg_s)
            return l.StackContent != null && l.StackContent.Count > 0 && l.StackContent.Last() == typeof(float);
        return false;
    }

    // Mirrors addflt/subflt/mulflt/divflt's own "jsr Float_X" (asm/helper/
    // float.asm) -- the ROM-backed routine each macro calls after pulling
    // its operands.
    private static readonly Dictionary<ILOpCode, string> BinOpRoutine = new()
    {
        [ILOpCode.Add] = "Float_Add",
        [ILOpCode.Sub] = "Float_Sub",
        [ILOpCode.Mul] = "Float_Mul",
        [ILOpCode.Div] = "Float_Div",
    };

    public void Execute(CompilerMethodContext context)
    {
        if (!context.TypeContext.CompilerContext.Optimize)
            return;
        if (context.Lines == null)
            return;

        var lines = context.Lines;
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Optimized || !IsFusableFloatOperand(lines[i]))
                continue;

            int j = NextEffective(lines, i);
            if (j < 0)
                continue;

            // [A] [Neg]
            if (lines[j].OpCode == ILOpCode.Neg && lines[j].SourceMethod == null && string.IsNullOrEmpty(lines[j].Label))
            {
                InsertFused(lines, new OpFusedFloatUnOp(lines[i]), i, j);
                continue;
            }

            if (!IsFusableFloatOperand(lines[j]))
                continue;

            int k = NextEffective(lines, j);
            if (k < 0 || lines[k].SourceMethod != null || !string.IsNullOrEmpty(lines[k].Label))
                continue;

            // [A] [B] [Add/Sub/Mul/Div] -- both operands already confirmed
            // float, so IL's own type-checking guarantees the op itself
            // operates on float too.
            if (BinOpRoutine.TryGetValue(lines[k].OpCode, out var routine))
            {
                InsertFused(lines, new OpFusedFloatBinOp(lines[i], lines[j], routine), i, j, k);
                continue;
            }

            // [A] [B] [Clt] -> compareLessflt's shape.
            if (lines[k].OpCode == ILOpCode.Clt)
            {
                InsertFused(lines, new OpFusedFloatCompareOp(lines[i], lines[j], isGreater: false), i, j, k);
                continue;
            }

            // [A] [B] [Cgt] -> compareGreaterflt's shape.
            if (lines[k].OpCode == ILOpCode.Cgt)
            {
                InsertFused(lines, new OpFusedFloatCompareOp(lines[i], lines[j], isGreater: true), i, j, k);
                continue;
            }
        }
    }

    // The next non-Optimized line after `from`, or -1 if none exists, or if
    // any skipped (Optimized/dead) line in between is itself a branch
    // target -- fusing across one of those would silently skip whatever a
    // branch landing there expected to run first. A live (non-skipped)
    // line's own label is checked separately by the caller, since Neg's
    // single-operand window and the Clt/Cgt/Add/Sub/Mul/Div final line
    // allow different continuations.
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

    private static void InsertFused(List<ILOperation> lines, OpBase op, params int[] matchedIndices)
    {
        int lastIndex = matchedIndices[^1];
        var newOperation = new ILOperation
        {
            Operation = op,
            StackContent = lines[lastIndex].StackContent,
        };
        lines.Insert(lastIndex + 1, newOperation);
        foreach (var idx in matchedIndices)
            lines[idx].Optimized = true;
    }
}
