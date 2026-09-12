using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

// Replaces the three previously near-identical
// ILMethodCompareEqualConstOptimizer / ILMethodCompareGreaterConstOptimizer /
// ILMethodCompareGreaterUnsignedConstOptimizer classes, which differed only in
// which IL compare opcode they matched and which asm macro suffix they emitted.
class ILMethodCompareConstOptimizer : PeepholeOptimizerPass
{
    private static readonly (ILOpCode Op, string Macro)[] Ops =
    {
        (ILOpCode.Ceq, "compareEqual_const"),
        (ILOpCode.Cgt, "compareGreater_const"),
        (ILOpCode.Cgt_un, "compareGreater_unsigned_const"),
    };

    // The three originals also each re-checked "!lines[i].Optimized" as part of
    // their own match condition; that's now handled once, centrally, by
    // PeepholeOptimizerPass for every rule's start position.
    protected override IEnumerable<PeepholeRule> Rules => Ops.Select(o =>
        new PeepholeRule(
            (ctx, w) => new OpArithmetic2(o.Macro),
            l => l.Operation is OpLdConst,
            l => l.OpCode == o.Op && string.IsNullOrEmpty(l.Label))
            .WithRawParameter((ctx, w) => w[0].RawParameter));
}
