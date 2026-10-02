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
        // "x--;" / "--x;" as a bare statement -- result discarded. Zp-aware,
        // see ILMethodIncOptimizer's own copy of this comment.
        new PeepholeRule(
            (ctx, w) =>
            {
                var varIndex = ((OpStloc)w[3].Operation).VarIndex;
                var slot = ctx.PromotedLocalSlot(varIndex);
                return slot != null ? new OpDecVarZp(slot) : new OpDecVar(varIndex);
            },
            l => l.Operation is OpLdloc,
            l => l.OpCode == ILOpCode.Ldc_i4_1,
            l => l.OpCode == ILOpCode.Sub,
            l => l.Operation is OpStloc),

        // "return --x;" / "y = --x;" (prefix)
        new PeepholeRule(
            (ctx, w) => new OpIncOrDecVarExpr("#dec_var", ((OpLdloc)w[0].Operation).VarIndex, ((OpStloc)w[5].Operation).VarIndex, isPostfix: false),
            l => l.Operation is OpLdloc,
            l => l.OpCode == ILOpCode.Ldc_i4_1,
            l => l.OpCode == ILOpCode.Sub,
            l => l.Operation is OpDup,
            l => l.Operation is OpStloc,
            l => l.Operation is OpStloc)
            .WithGuard((ctx, w) =>
                !((OpLdloc)w[0].Operation).Is16Bit(ctx, w[0]) &&
                ((OpLdloc)w[0].Operation).VarIndex == ((OpStloc)w[4].Operation).VarIndex &&
                ctx.PromotedLocalSlot(((OpLdloc)w[0].Operation).VarIndex) == null),

        // "return x--;" / "y = x--;" (postfix) -- dup precedes the subtract.
        new PeepholeRule(
            (ctx, w) => new OpIncOrDecVarExpr("#dec_var", ((OpLdloc)w[0].Operation).VarIndex, ((OpStloc)w[5].Operation).VarIndex, isPostfix: true),
            l => l.Operation is OpLdloc,
            l => l.Operation is OpDup,
            l => l.OpCode == ILOpCode.Ldc_i4_1,
            l => l.OpCode == ILOpCode.Sub,
            l => l.Operation is OpStloc,
            l => l.Operation is OpStloc)
            .WithGuard((ctx, w) =>
                !((OpLdloc)w[0].Operation).Is16Bit(ctx, w[0]) &&
                ((OpLdloc)w[0].Operation).VarIndex == ((OpStloc)w[4].Operation).VarIndex &&
                ctx.PromotedLocalSlot(((OpLdloc)w[0].Operation).VarIndex) == null),

        // "arr[x--] = v;" / "Foo(x--)" -- see ILMethodIncOptimizer's mirror
        // rule. Must stay after the two-stloc rule above for the same
        // "more specific pattern gets first refusal" reason.
        new PeepholeRule(
            (ctx, w) => new OpPostIncOrDecVarLeaveOnStack("#dec_var", ((OpLdloc)w[0].Operation).VarIndex),
            l => l.Operation is OpLdloc,
            l => l.Operation is OpDup,
            l => l.OpCode == ILOpCode.Ldc_i4_1,
            l => l.OpCode == ILOpCode.Sub,
            l => l.Operation is OpStloc)
            .WithGuard((ctx, w) =>
                !((OpLdloc)w[0].Operation).Is16Bit(ctx, w[0]) &&
                ((OpLdloc)w[0].Operation).VarIndex == ((OpStloc)w[4].Operation).VarIndex &&
                ctx.PromotedLocalSlot(((OpLdloc)w[0].Operation).VarIndex) == null)
    };
}
