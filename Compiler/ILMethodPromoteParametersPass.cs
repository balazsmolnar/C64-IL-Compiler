using System.Collections.Generic;
using System.Reflection.Metadata;

namespace Compiler;

// PROTOTYPE: companion to ILMethodPromoteLocalsPass -- same pool
// (ILMethodPromoteLocalsPass.Pool), same eligible-method gate
// (PassesSafeOpcodeGate), but for PARAMETERS (including `this`, though
// that's never actually eligible -- see below) instead of local
// variables. Must run strictly after ILMethodPromoteLocalsPass (see
// Program.cs's ordering): locals get first claim on the shared pool,
// this pass only fills whatever slots are left (context.PromotedLocals
// .Count tells it how many that was).
//
// Why this is a SEPARATE pass rather than folded into
// ILMethodPromoteLocalsPass: unlike a local (which is only ever born via
// Stloc, so redirecting both Ldloc and Stloc to a zp byte needs no extra
// code at all -- the local's nominal localsStack slot is simply never
// written), a PARAMETER's initial value arrives through the method's
// own prologue (#init_locals_pull_parameters, ILMethodEmitPass), which
// this deliberately does NOT touch -- the earlier, reverted whole-method
// "fast locals" prototype broke by changing the prologue's own
// parameter-pull order relative to the return-address handling, and nothing
// here repeats that. Instead, once a parameter is chosen, ILMethodEmitPass
// emits ONE extra copy (an ordinary #locals_push_value8 + #zp_pull_value8
// pair) right after the unmodified prologue runs, copying that parameter's
// already-pulled value from its localsStack slot into its promoted zp
// byte -- after that one-time copy, every Ldarg/Starg of that parameter
// for the rest of the method reads/writes the zp byte directly, the same
// as a promoted local does, and its localsStack slot is simply never
// touched again.
//
// `this` (unified parameter index 0 for an instance method) is never
// actually eligible: GetParameterType(0) always answers typeof(object)
// (a reference type), which IsReferenceCounted() always excludes, same
// mechanism that already excludes every other reference-typed parameter
// -- no special-casing needed.
class ILMethodPromoteParametersPass : ICompilerMethodPass
{
    public void Execute(CompilerMethodContext context)
    {
        if (!ILMethodPromoteLocalsPass.PassesSafeOpcodeGate(context))
            return;

        var pool = ILMethodPromoteLocalsPass.Pool;
        int slot = context.PromotedLocals.Count;
        if (slot >= pool.Length)
            return;

        bool isInstance = !context.Method.IsStatic;
        int unifiedCount = (isInstance ? 1 : 0) + context.Method.GetParameters().Length;

        for (int i = 0; i < unifiedCount && slot < pool.Length; i++)
        {
            // See TrivialSubstitutionSourceParameters's own comment
            // (ICompilerPass.cs) -- mirrors TrivialSubstitutionSourceLocals's
            // reasoning exactly, just for a parameter baked into an inlined
            // splice instead of a local.
            if (context.TrivialSubstitutionSourceParameters.Contains(i))
                continue;

            var type = context.GetParameterType(i);
            if (type.GetStorageBytes() != 1 || type.IsReferenceCounted())
                continue;

            // See HasFusableUse's own comment: promoting a parameter that
            // the optimizer group would otherwise have fused directly
            // (ILMethodIntOperandFusionOptimizer) is a net LOSS, not a
            // win, unlike promoting a local -- found via two real test
            // regressions (Local_And_Constant_8Bit_Add_Fuses,
            // Branch_Truthiness_On_Local_Fuses) the first time this pass
            // ran without this check.
            if (HasFusableUse(context.Lines, i))
                continue;

            // See CountUses's own comment: even with no fusable use, a
            // promoted parameter only pays off once its uses are frequent
            // enough to amortize the one-time copy-in. Found for real in
            // Hunchback (measure.bat went UP by 106 bytes despite the
            // HasFusableUse guard above) -- Enemy.set_Sprite's own
            // parameter was promoted, copied in, then read exactly once
            // more: a pure 6-byte loss, not caught by HasFusableUse since
            // that single use wasn't adjacent to Add/Sub/Mul/a branch.
            if (CountUses(context.Lines, i) < MinUsesToPromote)
                continue;

            context.PromotedParameters[i] = pool[slot];
            slot++;
        }
    }

    private static readonly HashSet<ILOpCode> ArithOrBranch = new()
    {
        ILOpCode.Add, ILOpCode.Sub, ILOpCode.Mul,
        ILOpCode.Brtrue, ILOpCode.Brtrue_s, ILOpCode.Brfalse, ILOpCode.Brfalse_s,
    };

    private static bool IsLeafShaped(ILOperation l) => l.OpCode switch
    {
        ILOpCode.Ldloc or ILOpCode.Ldloc_s or ILOpCode.Ldloc_0 or ILOpCode.Ldloc_1 or ILOpCode.Ldloc_2 or ILOpCode.Ldloc_3 => true,
        ILOpCode.Ldarg or ILOpCode.Ldarg_s or ILOpCode.Ldarg_0 or ILOpCode.Ldarg_1 or ILOpCode.Ldarg_2 or ILOpCode.Ldarg_3 => true,
        ILOpCode.Ldc_i4 or ILOpCode.Ldc_i4_s or ILOpCode.Ldc_i4_0 or ILOpCode.Ldc_i4_1 or ILOpCode.Ldc_i4_2 or ILOpCode.Ldc_i4_3
            or ILOpCode.Ldc_i4_4 or ILOpCode.Ldc_i4_5 or ILOpCode.Ldc_i4_6 or ILOpCode.Ldc_i4_7 or ILOpCode.Ldc_i4_8 or ILOpCode.Ldc_i4_m1 => true,
        ILOpCode.Ldfld => true,
        _ => false,
    };

    private static bool IsLdargFor(ILOperation l, int argIndex) => l.OpCode switch
    {
        ILOpCode.Ldarg_0 => argIndex == 0,
        ILOpCode.Ldarg_1 => argIndex == 1,
        ILOpCode.Ldarg_2 => argIndex == 2,
        ILOpCode.Ldarg_3 => argIndex == 3,
        ILOpCode.Ldarg or ILOpCode.Ldarg_s => (int)l.OriginalParameter == argIndex,
        _ => false,
    };

    // Conservative, read-only prediction of "would
    // ILMethodIntOperandFusionOptimizer (which runs later, in the
    // optimizer group) have fused this Ldarg occurrence" -- promoting a
    // parameter is only a net win when NONE of its uses would have been
    // fused instead: fusion already skips the hardware-stack round trip
    // for a single use just as well as promotion would, but a promoted
    // PARAMETER (unlike a local) also pays a one-time copy-in cost
    // (ILMethodEmitPass), so losing a fusion to gain promotion is a net
    // loss for that specific use. Runs in the setup group, before the
    // optimizer group even starts, so context.Lines has no Optimized/dead
    // lines yet -- plain index adjacency is exact here, no NextEffective-
    // style dead-line skipping needed (unlike the fusion pass itself,
    // which runs after inlining/other setup passes may have left some).
    // False positives (declining to promote a parameter that wouldn't
    // actually have fused) just forfeit promotion for that one parameter,
    // never a correctness issue -- deliberately conservative, not exact.
    // Measured directly from the macro bodies (asm/helper/localsStack.asm,
    // asm/helper/zeropage.asm): an unfused parameter read/write costs 6
    // bytes (locals_push_value8: ldy+lda abs,y+pha, or locals_pull_value8:
    // ldy+pla+sta abs,y); the zp-addressed equivalent costs 3 (zp_push_value8:
    // lda zp+pha, or zp_pull_value8: pla+sta zp) -- a flat 3-byte saving per
    // use, read or write alike. The one-time copy-in
    // (ILMethodEmitPass: #locals_push_value8 + #zp_pull_value8) itself
    // costs 6+3=9 bytes. Below 4 uses the copy-in cost isn't amortized
    // (3 uses break even at 9 saved == 9 spent, no actual gain), so this
    // is the minimum needed for promotion to be a real, not just
    // theoretical, win.
    private const int MinUsesToPromote = 4;

    private static int CountUses(List<ILOperation> lines, int argIndex)
    {
        int count = 0;
        foreach (var l in lines)
            if (IsLdargFor(l, argIndex) || IsStargFor(l, argIndex))
                count++;
        return count;
    }

    private static bool IsStargFor(ILOperation l, int argIndex) =>
        l.OpCode == ILOpCode.Starg_s && (int)l.OriginalParameter == argIndex;

    private static bool HasFusableUse(List<ILOperation> lines, int argIndex)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            if (!IsLdargFor(lines[i], argIndex))
                continue;

            // Directly followed by a branch, or is itself the second
            // operand of a binary op (stack evaluation order means the
            // second-pushed operand always sits immediately before the op).
            if (i + 1 < lines.Count && ArithOrBranch.Contains(lines[i + 1].OpCode))
                return true;
            // First operand of a binary op: another leaf follows directly,
            // then the op.
            if (i + 1 < lines.Count && IsLeafShaped(lines[i + 1]) &&
                i + 2 < lines.Count && ArithOrBranch.Contains(lines[i + 2].OpCode))
                return true;
        }
        return false;
    }
}
