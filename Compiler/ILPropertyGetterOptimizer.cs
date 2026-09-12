using System.Collections.Generic;
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
            (ctx, w) => new OpPushFld(w[0].RawParameter.ToString(), w[1].RawParameter.ToString(), w[1].Operation.Is16Bit(ctx, w[1])),
            l => l.Operation is OpLdarg,
            l => l.Operation is OpLdfld,
            l => l.Operation is OpStloc,
            l => l.OpCode == ILOpCode.Br_s,
            l => l.Operation is OpLdloc),

        // NOTE: original never set StackContent for this second, shorter pattern
        // (leaving it null, ILOperation's default) -- preserved via the override
        // below rather than defaulting to the last matched line's StackContent.
        new PeepholeRule(
            (ctx, w) => new OpPushFld(w[0].RawParameter.ToString(), w[1].RawParameter.ToString(), w[1].Operation.Is16Bit(ctx, w[1])),
            l => l.OpCode == ILOpCode.Ldarg_0,
            l => l.Operation is OpLdfld)
            .WithStackContent(w => null),
    };
}
