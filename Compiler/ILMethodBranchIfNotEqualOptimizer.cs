using System.Collections.Generic;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

// if (x == 0)
// IL_0054: ldloc.0
// IL_0055: ldc.i4.0
// IL_0056: ceq
// IL_0058: stloc.s 7
// IL_005a: ldloc.s 7
// IL_005c: brfalse.s IL_0060
class ILMethodBranchIfNotEqualOptimizer : PeepholeOptimizerPass
{
    protected override IEnumerable<PeepholeRule> Rules => new[]
    {
        // NOTE: the original code casts lines[i].Operation to OpLdloc unconditionally
        // here even though the match also accepts OpLdloc_s, which are sibling types
        // (not related by inheritance) -- an InvalidCastException if that branch is
        // ever actually taken with an OpLdloc_s. Preserved as-is for behavioral
        // fidelity; flagged separately, not fixed as part of this refactor.
        new PeepholeRule(
            (ctx, w) => new OpBranchIfNotEqual(
                ((OpLdloc)w[0].Operation).VarIndex,
                (int)w[1].RawParameter,
                (string)w[5].RawParameter),
            l => l.Operation is OpLdloc || l.Operation is OpLdloc_s,
            l => l.Operation is OpLdConst,
            l => l.OpCode == ILOpCode.Ceq,
            l => l.Operation is OpStloc || l.Operation is OpStloc_s,
            l => l.Operation is OpLdloc || l.Operation is OpLdloc_s,
            l => l.OpCode == ILOpCode.Brfalse || l.OpCode == ILOpCode.Brfalse_s)
    };
}
