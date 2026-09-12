using System.Collections.Generic;
using Compiler.Ops;

namespace Compiler;

class ILSetFieldOptimizer : PeepholeOptimizerPass
{
    // IL_0000: ldarg.0
    // IL_0001: ldarg.1
    // IL_0002: stfld uint32 PlatformEnemy::'<MaxX>k__BackingField'
    protected override IEnumerable<PeepholeRule> Rules => new[]
    {
        new PeepholeRule(
            (ctx, w) => new OpSetfld(
                w[0].RawParameter.ToString(),
                w[1].RawParameter.ToString(),
                w[2].RawParameter.ToString(),
                w[2].Operation.Is16Bit(ctx, w[2])),
            l => l.Operation is OpLdarg,
            l => l.Operation is OpLdarg,
            l => l.Operation is OpStfld)
    };
}
