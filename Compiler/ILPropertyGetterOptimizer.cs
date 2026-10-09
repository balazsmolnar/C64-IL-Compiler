using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

class ILPropertyGettterOptimizer : PeepholeOptimizerPass
{
    // IL_0001: ldarg.0
    // IL_0002: ldfld uint32 Player::x_
    // IL_0007: stloc.0
    // // (no C# code)
    // IL_0008: br.s IL_000a
    // IL_000a: ldloc.0
    protected override IEnumerable<PeepholeRule> Rules => new[]
    {
        new PeepholeRule(
            (ctx, w) => new OpPushFld(w[0].RawParameter.ToString(), w[1].RawParameter.ToString(), w[1].Operation.Is16Bit(ctx, w[1]), w[1].StackContent.Last() == typeof(float)),
            l => l.Operation is OpLdarg,
            l => l.Operation is OpLdfld,
            l => l.Operation is OpStloc,
            l => l.OpCode == ILOpCode.Br_s,
            l => l.Operation is OpLdloc)
            .AllowInlinedWindows(),

        // NOTE: original never set StackContent for this second, shorter pattern
        // (leaving it null, ILOperation's default) -- preserved via the override
        // below rather than defaulting to the last matched line's StackContent.
        new PeepholeRule(
            (ctx, w) => new OpPushFld(w[0].RawParameter.ToString(), w[1].RawParameter.ToString(), w[1].Operation.Is16Bit(ctx, w[1]), w[1].StackContent.Last() == typeof(float)),
            l => l.OpCode == ILOpCode.Ldarg_0,
            l => l.Operation is OpLdfld)
            .WithStackContent(w => null)
            .AllowInlinedWindows(),

        // Generalizes the rule above beyond `this` specifically: any
        // reference held in a local or a parameter (IL guarantees Ldfld's
        // receiver is reference-typed, so no extra type check is needed
        // here) gets the same fusion, via OpPushFldAt (Operands/
        // OperandInline.cs) -- already built for ILMethodInliningPass's
        // own argument substitution, and already strictly no worse than
        // OpPushFld's hardcoded-this macro even at relpos 1 (pushfld8_at's
        // own "ldx localsStack-relpos,y" bakes the offset into the
        // instruction itself, skipping the separate "dey" OpPushFld's
        // macro needs) -- so this rule runs last, only picking up
        // whatever Ldarg_0 the rule above didn't already claim. A field's
        // real reflection Type (not just IsFloat/Is16Bit booleans) is
        // already sitting in the Ldfld's own StackContent (OpLdfld.
        // SetStackContent), which is exactly what OpPushFldAt's
        // constructor wants. Left at null StackContent same as the rule
        // above, for the same reason: IPushFldOperation.IsFloat/
        // Is16BitField (not StackContent) is what every consumer of a
        // fused field read -- ILMethodCachedFieldAccessPass,
        // ILMethodFloatOperandFusionOptimizer, ILMethodIntOperandFusionOptimizer
        // -- already knows to check instead.
        new PeepholeRule(
            (ctx, w) => new OpPushFldAt(w[0].RawParameter.ToString(), w[1].RawParameter.ToString(), w[1].StackContent.Last()),
            l => l.Operation is OpLdloc || l.Operation is OpLdloc_s || l.Operation is OpLdarg || l.Operation is OpLdarg_s,
            l => l.Operation is OpLdfld)
            .WithStackContent(w => null)
            .AllowInlinedWindows(),
    };
}
