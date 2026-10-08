using System.Collections.Generic;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

// Fuses a binary/unary float op whose operand(s) are a simple local, a
// parameter, an already-resolved field read (any IPushFldOperation --
// ILPropertyGettterOptimizer has already collapsed a raw "this.field_"
// access to one of those earlier in this same optimizer group, which is
// why this doesn't also need to match a raw, unfused Ldfld), or a float
// literal (Ldc_r4) into one operation that copies straight from the
// source into zp_flt_a/zp_flt_b (locals_copy_valueflt / pushfldflt_direct
// / copy_mflpt_const, asm/helper/localsStack.asm / optimized.asm /
// stack.asm), instead of each operand independently round-tripping
// through the hardware stack via Ldloc/Ldarg/pushfld/Ldc_r4's own push +
// addflt/subflt/mulflt/divflt/negateflt/compareLessflt/compareGreaterflt's
// own stack_pull_mflpt (asm/helper/float.asm) -- 10 bytes and ~35 cycles of
// pure pha/pla overhead per fused operand. The jsr + result-push tail is
// unchanged, so it composes with whatever the result feeds next (another
// op, a Stloc, a Setfld) without this pass needing to know or care what
// that is.
//
// Also fuses a CHAINED first operand: when this op's own source is itself
// a float Add/Sub/Mul/Div/Neg (unfused, or already fused by an earlier
// iteration of this same scan), its result is already sitting in
// zp_flt_a as a side effect of running it -- MarkAsChainProducer elides
// that producer's own result-push (asm/helper/float.asm's "_nopush"
// macros / OpFusedFloatBinOp/UnOp's PushResult), and FloatOperandCopy
// (Compiler/Operands/OperandFloatFusion.cs) then emits nothing at all for
// this operand, since the value is already exactly where it needs to be.
// Restricted to the SAFE shape only: the chained value must land in the
// FIRST operand slot (zp_flt_a), and -- for a binary op -- the SECOND
// operand must be a plain leaf, never itself a producer. A producer's own
// computation uses zp_flt_a/zp_flt_b as scratch, so evaluating a second
// chained operand would clobber the first one's leftover result before
// the consuming op ever got to use it; a plain leaf's own copy only ever
// writes to zp_flt_b, never touching zp_flt_a, so it can't collide. The
// harder cases this deliberately leaves alone -- a chained SECOND operand
// (would need reordering emission relative to source order), and BOTH
// operands chained (would need a dedicated save slot so the first result
// survives the second producer's own use of zp_flt_a as scratch) -- are
// still real opportunities (e.g. "a*b - c*d"'s outer Sub), just not
// attempted here.
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
            l.Operation is OpLdarg || l.Operation is OpLdarg_s ||
            l.Operation is OpLdc_r4)
            return l.StackContent != null && l.StackContent.Count > 0 && l.StackContent.Last() == typeof(float);
        return false;
    }

    // A still-"live" (or already-fused-by-an-earlier-iteration-of-this-
    // pass) float-producing op whose result could instead be left in
    // zp_flt_a for a directly-following consumer -- see this class's own
    // top comment for the safety shape this is restricted to.
    private static bool IsChainableFloatProducer(ILOperation l)
    {
        if (l.SourceMethod != null || l.StackContent == null || l.StackContent.Count == 0 ||
            l.StackContent.Last() != typeof(float))
            return false;
        if (l.Operation is OpFusedFloatBinOp fb)
            return fb.PushResult;
        if (l.Operation is OpFusedFloatUnOp fu)
            return fu.PushResult;
        // Still the generic, un-replaced emitter CommandMap originally
        // assigned (OperandArithmetic.cs) -- OpCode gates out OpCompare
        // (Clt/Cgt/Ceq), which also derives from OpArithmetic2 but is
        // never a float producer (its result is always bool).
        if (l.Operation is OpArithmetic1 && l.OpCode == ILOpCode.Neg)
            return true;
        if (l.Operation is OpArithmetic2 && BinOpRoutine.ContainsKey(l.OpCode))
            return true;
        return false;
    }

    // Replaces a chainable producer's own Operation (or flips an
    // already-fused one's PushResult) so it stops pushing its result onto
    // the hardware stack, leaving it in zp_flt_a for the directly-
    // following consumer this pass is about to build. Only ever called
    // once the WHOLE match (including the consumer) has been confirmed --
    // never speculatively, so a failed match never leaves a line
    // half-mutated.
    private static void MarkAsChainProducer(ILOperation a)
    {
        switch (a.Operation)
        {
            case OpFusedFloatBinOp fb:
                fb.PushResult = false;
                return;
            case OpFusedFloatUnOp fu:
                fu.PushResult = false;
                return;
            case OpArithmetic1 when a.OpCode == ILOpCode.Neg:
                a.Operation = new OpFloatProducerNoPush("#negateflt_nopush");
                return;
            default:
                a.Operation = new OpFloatProducerNoPush(NoPushMacro[a.OpCode]);
                return;
        }
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

    private static readonly Dictionary<ILOpCode, string> NoPushMacro = new()
    {
        [ILOpCode.Add] = "#addflt_nopush",
        [ILOpCode.Sub] = "#subflt_nopush",
        [ILOpCode.Mul] = "#mulflt_nopush",
        [ILOpCode.Div] = "#divflt_nopush",
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
            if (lines[i].Optimized)
                continue;

            bool aIsLeaf = IsFusableFloatOperand(lines[i]);
            bool aIsProducer = !aIsLeaf && IsChainableFloatProducer(lines[i]);
            if (!aIsLeaf && !aIsProducer)
                continue;

            int j = NextEffective(lines, i);
            if (j < 0)
                continue;

            // [A] [Neg] -- A may be a leaf or a chained producer, Neg has
            // no second operand to worry about clobbering anything.
            if (lines[j].OpCode == ILOpCode.Neg && lines[j].SourceMethod == null && string.IsNullOrEmpty(lines[j].Label))
            {
                if (aIsProducer)
                {
                    MarkAsChainProducer(lines[i]);
                    InsertFused(lines, new OpFusedFloatUnOp(lines[i]), j, j);
                }
                else
                {
                    InsertFused(lines, new OpFusedFloatUnOp(lines[i]), j, i, j);
                }
                continue;
            }

            // From here on, a second operand B is needed too -- always a
            // plain leaf, never a producer (see this class's own
            // top-of-file comment on why).
            if (!IsFusableFloatOperand(lines[j]))
                continue;

            int k = NextEffective(lines, j);
            if (k < 0 || lines[k].SourceMethod != null || !string.IsNullOrEmpty(lines[k].Label))
                continue;

            // [A] [B] [Add/Sub/Mul/Div] -- B already confirmed float, so
            // IL's own type-checking guarantees the op itself (and a
            // chained A) operates on float too.
            if (BinOpRoutine.TryGetValue(lines[k].OpCode, out var routine))
            {
                if (aIsProducer)
                {
                    MarkAsChainProducer(lines[i]);
                    InsertFused(lines, new OpFusedFloatBinOp(lines[i], lines[j], routine), k, j, k);
                }
                else
                {
                    InsertFused(lines, new OpFusedFloatBinOp(lines[i], lines[j], routine), k, i, j, k);
                }
                continue;
            }

            // [A] [B] [Clt] -> compareLessflt's shape.
            if (lines[k].OpCode == ILOpCode.Clt)
            {
                if (aIsProducer)
                {
                    MarkAsChainProducer(lines[i]);
                    InsertFused(lines, new OpFusedFloatCompareOp(lines[i], lines[j], isGreater: false), k, j, k);
                }
                else
                {
                    InsertFused(lines, new OpFusedFloatCompareOp(lines[i], lines[j], isGreater: false), k, i, j, k);
                }
                continue;
            }

            // [A] [B] [Cgt] -> compareGreaterflt's shape.
            if (lines[k].OpCode == ILOpCode.Cgt)
            {
                if (aIsProducer)
                {
                    MarkAsChainProducer(lines[i]);
                    InsertFused(lines, new OpFusedFloatCompareOp(lines[i], lines[j], isGreater: true), k, j, k);
                }
                else
                {
                    InsertFused(lines, new OpFusedFloatCompareOp(lines[i], lines[j], isGreater: true), k, i, j, k);
                }
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

    // `insertAfterIndex` sites the new fused line (always the last matched
    // line's own position -- its StackContent is reused for the new line
    // too). `indicesToMarkOptimized` are the matched lines to retire; a
    // chained producer's own index is deliberately NOT included here (it
    // stays live, emitting its own, now-pushless, code at its own
    // position -- see MarkAsChainProducer).
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
