using System.Collections.Generic;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

class ILFieldIncrementOptimizer : PeepholeOptimizerPass
{
    // IL_00a8: ldarg.0
    // // this.data_++;
    // IL_00a9: ldarg.0
    // IL_00aa: ldfld int32 Player::data_
    // IL_00af: ldc.i4.1
    // IL_00b0: add
    // // (no C# code)
    // IL_00b1: stfld int32 Player::data_
    protected override IEnumerable<PeepholeRule> Rules => new[]
    {
        new PeepholeRule(
            (ctx, w) => new OpIncfld(w[0].RawParameter.ToString(), w[2].RawParameter.ToString()),
            l => l.Operation is OpLdarg,
            l => l.Operation is OpLdarg,
            l => l.Operation is OpLdfld,
            l => l.OpCode == ILOpCode.Ldc_i4_1,
            l => l.OpCode == ILOpCode.Add,
            l => l.Operation is OpStfld)
            // "incfld" (asm/helper/optimized.asm) only handles a single byte;
            // a 16-bit field must fall through to the general pushfld+add+setfld
            // path instead, which does have both width variants.
            .WithGuard((ctx, w) => !w[2].Operation.Is16Bit(ctx, w[2]))
    };
}
