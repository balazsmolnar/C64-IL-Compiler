using System.Collections.Generic;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

class ILSetFieldOptimizer : PeepholeOptimizerPass
{
    // IL_0000: ldarg.0
    // IL_0001: ldarg.1
    // IL_0002: stfld uint32 PlatformEnemy::'<MaxX>k__BackingField'
    //
    // Both the object and the value were originally restricted to
    // OpLdarg only -- widened to also accept OpLdloc/OpLdloc_s/OpLdarg_s
    // (IL guarantees Stfld's receiver is reference-typed and its value is
    // already type-compatible with the field, so no extra width/type
    // check is needed for either beyond what already existed), plus two
    // more rules below for a constant value -- "obj.field = <literal>;"
    // -- which OpSetfld itself can't express (it always reads the value
    // from a second localsStack slot); see OpSetfldConst's own comment.
    private static bool IsRefOrLocal(ILOperation l) =>
        l.Operation is OpLdarg || l.Operation is OpLdarg_s || l.Operation is OpLdloc || l.Operation is OpLdloc_s;

    // A promoted local (ILMethodPromoteLocalsPass) lives at a fixed
    // zero-page byte, never written to its nominal localsStack slot at
    // all -- OpSetfld reads its value operand via a plain, unguarded
    // RawParameter (a localsStack rel_pos), so without this check a
    // promoted local used as "obj.field = local;"'s value would silently
    // load stale/wrong memory, the same hazard ILMethodIncOptimizer/
    // DecOptimizer/SetVariableOptimizer and the operand-fusion optimizers
    // already guard against. Only ever matters for the VALUE operand
    // (w[1] in the rule below) -- the object reference (w[0]) is always
    // reference-typed, which ILMethodPromoteLocalsPass's own
    // IsReferenceCounted() check already excludes from promotion
    // entirely, so it can never be one and needs no check here. Was a
    // latent gap, not yet a real bug, before Ldfld/Stfld joined
    // ILMethodPromoteLocalsPass's SafeOpCodes: a method with a Stfld
    // could never also have a promoted local (Stfld disqualified the
    // whole method), so this situation couldn't occur until that change.
    //
    // Uses the sourceMethod-aware PromotedLocalSlot overload
    // (ICompilerPass.cs), not the plain one -- this rule is also marked
    // AllowInlinedWindows(), so w[1] may be a line spliced in from an
    // inlined callee (l.SourceMethod != null); that overload always
    // answers "not promoted" for such a line (an inlined local is never
    // promoted, full stop) instead of risking a coincidental VarIndex
    // collision against THIS method's own PromotedLocals map.
    private static bool IsPromotedLocal(CompilerMethodContext ctx, ILOperation l) =>
        (l.Operation is OpLdloc ldloc && ctx.PromotedLocalSlot(l.SourceMethod, ldloc.VarIndex) != null) ||
        (l.Operation is OpLdloc_s && ctx.PromotedLocalSlot(l.SourceMethod, (int)l.OriginalParameter) != null);

    protected override IEnumerable<PeepholeRule> Rules => new[]
    {
        // Guarded against a float field: OpSetfld/setfld8/setfld16 have
        // no float-width concept at all (no "setfldflt" macro exists --
        // this rule was always Ldarg+Ldarg only before, which apparently
        // never happened to hit a float field in any existing test, so
        // the gap was latent rather than theoretical). OpStfld.Is16Bit
        // only ever answers 8 vs 16 (GetStorageBytes()==2), so a float
        // value (5 bytes) silently got "_is16Bit=false" -- #setfld8 --
        // writing just its first byte and leaving the other four
        // untouched. Found for real via Test/FloatTests.cs's own
        // TestField (`obj.Value = obj.Value + 1f;`) the moment OpLdloc
        // started being accepted here. Falling through to the general,
        // unfused Stfld path (which does know about "flt" -- OpStfld's
        // own SizeSuffix) is correct; adding real float support to
        // OpSetfld is a separate, not-yet-done piece of work.
        new PeepholeRule(
            (ctx, w) => new OpSetfld(
                w[0].RawParameter.ToString(),
                w[1].RawParameter.ToString(),
                w[2].RawParameter.ToString(),
                w[2].Operation.Is16Bit(ctx, w[2])),
            IsRefOrLocal,
            IsRefOrLocal,
            l => l.Operation is OpStfld)
            .WithGuard((ctx, w) => w[2].PreviousInstructions[0].StackContent.Last() != typeof(float) && !IsPromotedLocal(ctx, w[1]))
            .AllowInlinedWindows(),

        // obj.field = <8-bit literal>; -- PeepholeRule's own strict
        // consecutive-window matching already keeps this from also
        // matching the 4-line widened shape below (there, lines[i+2] is
        // Conv_i8/Conv_u8, not OpStfld, so this 3-line rule's own last
        // matcher simply fails and leaves it for the rule below).
        new PeepholeRule(
            (ctx, w) => new OpSetfldConst(
                w[0].RawParameter.ToString(),
                w[1].RawParameter.ToString(),
                w[2].RawParameter.ToString(),
                w[2].Operation.Is16Bit(ctx, w[2])),
            IsRefOrLocal,
            l => l.Operation is OpLdConst,
            l => l.Operation is OpStfld)
            .AllowInlinedWindows(),

        // obj.field = <16-bit literal>; -- Roslyn widens an int literal
        // with Conv_i8/Conv_u8 before a long/ulong field store, same
        // shape ILMethodIntOperandFusionOptimizer's own TryMatchInt16Leaf
        // already handles for an arithmetic operand (OpLdConst already
        // computes the full 16-bit push at compile time once it sees the
        // following Conv_i8/Conv_u8, which is why the window has to look
        // past it rather than treating the constant as a single line).
        new PeepholeRule(
            (ctx, w) => new OpSetfldConst(
                w[0].RawParameter.ToString(),
                w[1].RawParameter.ToString(),
                w[3].RawParameter.ToString(),
                w[3].Operation.Is16Bit(ctx, w[3])),
            IsRefOrLocal,
            l => l.Operation is OpLdConst,
            l => l.OpCode == ILOpCode.Conv_i8 || l.OpCode == ILOpCode.Conv_u8,
            l => l.Operation is OpStfld)
            .AllowInlinedWindows(),
    };
}
