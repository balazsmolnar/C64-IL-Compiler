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
            l => l.Operation is OpStloc && string.IsNullOrEmpty(l.Label)),

        // "varB = varA;" / "var local = someParam;" -- a local/parameter
        // pushed and immediately stored to a different local, with
        // nothing else in between. Unfused this is a push then a pop
        // through the real hardware stack (locals_push_value8 +
        // locals_pull_value8); #copy_var does the same localsStack-slot
        // move directly. 8-bit only, and excludes any reference-counted
        // destination -- locals_pull_value8 does GC root-count
        // inc/dec when storing into one (asm/helper/localsStack.asm),
        // which a bare copy would silently skip and corrupt.
        new PeepholeRule(
            (ctx, w) => new OpCopyVar(w[0].Operation.ConvertParameter(ctx, w[0]), ((OpStloc)w[1].Operation).VarIndex),
            l => l.Operation is OpLdloc || l.Operation is OpLdarg,
            l => l.Operation is OpStloc && string.IsNullOrEmpty(l.Label))
            .WithGuard((ctx, w) =>
                !w[0].Operation.Is16Bit(ctx, w[0]) &&
                !w[1].Operation.Is16Bit(ctx, w[1]) &&
                !ctx.Method.GetMethodBody().LocalVariables[((OpStloc)w[1].Operation).VarIndex].LocalType.IsReferenceCounted())
    };
}
