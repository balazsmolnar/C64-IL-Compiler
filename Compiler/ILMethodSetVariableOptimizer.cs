using System.Collections.Generic;
using Compiler.Ops;

namespace Compiler;

class ILMethodSetVariableOptimizer : PeepholeOptimizerPass
{
    protected override IEnumerable<PeepholeRule> Rules => new[]
    {
        new PeepholeRule(
            (ctx, w) => new OpInitVar(((OpStloc)w[1].Operation).VarIndex, (int)w[0].RawParameter),
            l => l.Operation is OpLdConst,
            l => l.Operation is OpStloc && string.IsNullOrEmpty(l.Label))
    };
}
