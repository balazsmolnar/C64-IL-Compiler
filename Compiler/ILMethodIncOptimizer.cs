using System.Collections.Generic;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

class ILMethodIncOptimizer : PeepholeOptimizerPass
{
    protected override IEnumerable<PeepholeRule> Rules => new[]
    {
        // "x++;" / "++x;" as a bare statement -- result discarded. Zp-aware:
        // if x is a promoted local (see zp_local0's comment in
        // zeropage.asm), fuse into #inc_var_zp instead of #inc_var -- same
        // fusion, just addressed against its zero-page slot instead of a
        // localsStack-relative position.
        new PeepholeRule(
            (ctx, w) =>
            {
                var varIndex = ((OpStloc)w[3].Operation).VarIndex;
                var slot = ctx.PromotedLocalSlot(varIndex);
                return slot != null ? new OpIncVarZp(slot) : new OpIncVar(varIndex);
            },
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
        // being reassigned). Not zp-aware yet -- OpIncOrDecVarExpr builds
        // raw localsStack-addressed macro text directly, so this rule is
        // guarded off for a promoted x below rather than fused wrong; the
        // unfused IL still gets x's own ldloc/stloc routed to zp by
        // OpLdloc/OpStloc, just without this extra fusion.
        new PeepholeRule(
            (ctx, w) => new OpIncOrDecVarExpr("#inc_var", ((OpLdloc)w[0].Operation).VarIndex, ((OpStloc)w[5].Operation).VarIndex, isPostfix: false),
            l => l.Operation is OpLdloc,
            l => l.OpCode == ILOpCode.Ldc_i4_1,
            l => l.OpCode == ILOpCode.Add,
            l => l.Operation is OpDup,
            l => l.Operation is OpStloc,
            l => l.Operation is OpStloc)
            .WithGuard((ctx, w) =>
                !((OpLdloc)w[0].Operation).Is16Bit(ctx, w[0]) &&
                ((OpLdloc)w[0].Operation).VarIndex == ((OpStloc)w[4].Operation).VarIndex &&
                ctx.PromotedLocalSlot(((OpLdloc)w[0].Operation).VarIndex) == null),

        // "return x++;" / "y = x++;" -- same idea, but postfix dups the OLD
        // value *before* the add (ldloc, dup, ldc.i4.1, add, stloc, stloc),
        // the opposite order from prefix above, so it needs its own pattern.
        new PeepholeRule(
            (ctx, w) => new OpIncOrDecVarExpr("#inc_var", ((OpLdloc)w[0].Operation).VarIndex, ((OpStloc)w[5].Operation).VarIndex, isPostfix: true),
            l => l.Operation is OpLdloc,
            l => l.Operation is OpDup,
            l => l.OpCode == ILOpCode.Ldc_i4_1,
            l => l.OpCode == ILOpCode.Add,
            l => l.Operation is OpStloc,
            l => l.Operation is OpStloc)
            .WithGuard((ctx, w) =>
                !((OpLdloc)w[0].Operation).Is16Bit(ctx, w[0]) &&
                ((OpLdloc)w[0].Operation).VarIndex == ((OpStloc)w[4].Operation).VarIndex &&
                ctx.PromotedLocalSlot(((OpLdloc)w[0].Operation).VarIndex) == null),

        // "arr[x++] = v;" / "Foo(x++)" -- x's old value is consumed directly
        // by whatever follows (no second local), so only one stloc appears
        // (ldloc, dup, ldc.i4.1, add, stloc), one shorter than the rule
        // above. Must run after it: that longer, more specific pattern has
        // to get first refusal, or this one would match its first 5
        // elements and leave the real 6th line (the second stloc) to be
        // wrongly treated as a fresh, unrelated instruction.
        new PeepholeRule(
            (ctx, w) => new OpPostIncOrDecVarLeaveOnStack("#inc_var", ((OpLdloc)w[0].Operation).VarIndex),
            l => l.Operation is OpLdloc,
            l => l.Operation is OpDup,
            l => l.OpCode == ILOpCode.Ldc_i4_1,
            l => l.OpCode == ILOpCode.Add,
            l => l.Operation is OpStloc)
            .WithGuard((ctx, w) =>
                !((OpLdloc)w[0].Operation).Is16Bit(ctx, w[0]) &&
                ((OpLdloc)w[0].Operation).VarIndex == ((OpStloc)w[4].Operation).VarIndex &&
                ctx.PromotedLocalSlot(((OpLdloc)w[0].Operation).VarIndex) == null)
    };
}
