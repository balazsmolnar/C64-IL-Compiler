using System.Collections.Generic;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

// Mirror of ILMethodIncOptimizer for "x--;"/"--x;"/"return --x;" -- see
// there for the rationale behind each rule and guard. Roslyn compiles
// decrement as "ldc.i4.1; sub" (not "ldc.i4.m1; add"), confirmed against
// Test/ArithmeticTests.cs's TestDecrement's generated asm.
class ILMethodDecOptimizer : PeepholeOptimizerPass
{
    protected override IEnumerable<PeepholeRule> Rules => new[]
    {
        // "x--;" / "--x;" as a bare statement -- result discarded.
        new PeepholeRule(
            (ctx, w) => new OpDecVar(((OpStloc)w[3].Operation).VarIndex),
            l => l.Operation is OpLdloc,
            l => l.OpCode == ILOpCode.Ldc_i4_1,
            l => l.OpCode == ILOpCode.Sub,
            l => l.Operation is OpStloc),

        // "return --x;" / "y = --x;"
        new PeepholeRule(
            (ctx, w) => new OpIncOrDecVarExpr("#dec_var", ((OpLdloc)w[0].Operation).VarIndex, ((OpStloc)w[5].Operation).VarIndex),
            l => l.Operation is OpLdloc,
            l => l.OpCode == ILOpCode.Ldc_i4_1,
            l => l.OpCode == ILOpCode.Sub,
            l => l.Operation is OpDup,
            l => l.Operation is OpStloc,
            l => l.Operation is OpStloc)
            .WithGuard((ctx, w) =>
                !((OpLdloc)w[0].Operation).Is16Bit(ctx, w[0]) &&
                ((OpLdloc)w[0].Operation).VarIndex == ((OpStloc)w[4].Operation).VarIndex)
    };
}
