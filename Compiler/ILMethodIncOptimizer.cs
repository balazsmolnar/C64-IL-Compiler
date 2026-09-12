using System.Collections.Generic;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

class ILMethodIncOptimizer : PeepholeOptimizerPass
{
    protected override IEnumerable<PeepholeRule> Rules => new[]
    {
        // "x++;" / "++x;" as a bare statement -- result discarded.
        new PeepholeRule(
            (ctx, w) => new OpIncVar(((OpStloc)w[3].Operation).VarIndex),
            l => l.Operation is OpLdloc,
            l => l.OpCode == ILOpCode.Ldc_i4_1,
            l => l.OpCode == ILOpCode.Add,
            l => l.Operation is OpStloc),

        // "return ++x;" / "y = ++x;" -- x's incremented value is itself
        // used, so Roslyn dups it before storing back to x, then stores
        // the dup'd copy to a second, separate local. inc_var is 8-bit
        // only (see asm/helper/optimized.asm), and only valid when the
        // first store really does target the same variable that was
        // loaded (ruling out an unrelated lookalike like "y = x + 1;
        // z = y;", where dup also precedes two stlocs but x itself isn't
        // being reassigned).
        new PeepholeRule(
            (ctx, w) => new OpIncOrDecVarExpr("#inc_var", ((OpLdloc)w[0].Operation).VarIndex, ((OpStloc)w[5].Operation).VarIndex),
            l => l.Operation is OpLdloc,
            l => l.OpCode == ILOpCode.Ldc_i4_1,
            l => l.OpCode == ILOpCode.Add,
            l => l.Operation is OpDup,
            l => l.Operation is OpStloc,
            l => l.Operation is OpStloc)
            .WithGuard((ctx, w) =>
                !((OpLdloc)w[0].Operation).Is16Bit(ctx, w[0]) &&
                ((OpLdloc)w[0].Operation).VarIndex == ((OpStloc)w[4].Operation).VarIndex)
    };
}
