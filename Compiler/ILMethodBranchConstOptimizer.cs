using System.Collections.Generic;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

class ILMethodBranchConstOptimizer : PeepholeOptimizerPass
{
    // The original also re-checked "!lines[i].Optimized" as part of its own match
    // condition; that's now handled once, centrally, by PeepholeOptimizerPass for
    // every rule's start position, so it's dropped here as redundant.
    protected override IEnumerable<PeepholeRule> Rules => new[]
    {
        new PeepholeRule(
            (ctx, w) => new OpBranchConst(w[1].Operation.Command + "_const"),
            l => l.Operation is OpLdConst,
            l => l.Operation is OpShortJump && l.OpCode != ILOpCode.Br_s && string.IsNullOrEmpty(l.Label))
            .WithRawParameter((ctx, w) => w[0].RawParameter + ", " + w[1].RawParameter)
    };
}
