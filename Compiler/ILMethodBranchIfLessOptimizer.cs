using System.Collections.Generic;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

// IL_0032: ldloc.0
// IL_0033: ldc.i4.s 10
// IL_0035: clt
// IL_0037: stloc.1
// IL_0038: ldloc.1
// IL_0039: brtrue.s IL_0021
class ILMethodBranchIfLessOptimizer : PeepholeOptimizerPass
{
    protected override IEnumerable<PeepholeRule> Rules => new[]
    {
        new PeepholeRule(
            (ctx, w) => new OpBranchIfVarLess(
                ((OpLdloc)w[0].Operation).VarIndex,
                (int)w[1].RawParameter,
                (string)w[5].RawParameter),
            l => l.Operation is OpLdloc,
            l => l.Operation is OpLdConst,
            l => l.OpCode == ILOpCode.Clt || l.OpCode == ILOpCode.Clt_un,
            l => l.Operation is OpStloc,
            l => l.Operation is OpLdloc,
            l => l.OpCode == ILOpCode.Brtrue || l.OpCode == ILOpCode.Brtrue_s)
    };
}
