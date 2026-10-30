using System.Collections.Generic;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

class ILFieldIncrementOptimizer : PeepholeOptimizerPass
{
    // IL_00a8: ldarg.0
    // // this.data_++;
    // IL_00a9: ldarg.0
    // IL_00aa: ldfld int32 Player::data_
    // IL_00af: ldc.i4.1
    // IL_00b0: add
    // // (no C# code)
    // IL_00b1: stfld int32 Player::data_
    protected override IEnumerable<PeepholeRule> Rules => new[]
    {
        new PeepholeRule(
            (ctx, w) => new OpIncfld(w[0].RawParameter.ToString(), w[2].RawParameter.ToString()),
            l => l.Operation is OpLdarg,
            l => l.Operation is OpLdarg,
            l => l.Operation is OpLdfld,
            l => l.OpCode == ILOpCode.Ldc_i4_1,
            l => l.OpCode == ILOpCode.Add,
            l => l.Operation is OpStfld)
            // "incfld" (asm/helper/optimized.asm) only handles a single byte;
            // a 16-bit field must fall through to the general pushfld+add+setfld
            // path instead, which does have both width variants.
            .WithGuard((ctx, w) => !w[2].Operation.Is16Bit(ctx, w[2]))
            .AllowInlinedWindows(),

        // "s_count++;" / "s_count--;" (also "s = s + 1;") on a one-byte
        // static field: one inc/dec of its absolute address (3 bytes)
        // instead of push, push 1, add, pull (about 20). Reuses the
        // inc_var_zp/dec_var_zp macros, which are a bare inc/dec of any
        // address. Only for a value type of one byte: not a reference
        // (that store does reference counting), not wider.
        StaticStep(ILOpCode.Add, f => new OpIncVarZp(f)),
        StaticStep(ILOpCode.Sub, f => new OpDecVarZp(f)),
    };

    private static PeepholeRule StaticStep(ILOpCode op, System.Func<string, OpBase> build) =>
        new PeepholeRule(
            (ctx, w) => build(StaticFieldLabel(ctx, w[0])),
            l => l.OpCode == ILOpCode.Ldsfld,
            l => l.OpCode == ILOpCode.Ldc_i4_1,
            l => l.OpCode == op,
            l => l.OpCode == ILOpCode.Stsfld)
            .WithGuard((ctx, w) =>
            {
                if (!Equals(w[0].OriginalParameter, w[3].OriginalParameter))
                    return false;
                var field = ctx.Method.ReflectedType.Module.ResolveField((int)w[0].OriginalParameter);
                return field.FieldType != typeof(float) && !field.FieldType.IsReferenceCounted() &&
                       field.FieldType.GetStorageBytes() == 1;
            });

    private static string StaticFieldLabel(CompilerMethodContext ctx, ILOperation l)
    {
        var field = ctx.Method.ReflectedType.Module.ResolveField((int)l.OriginalParameter);
        return $"{field.ReflectedType.FullName.ToValidName()}_field_{field.Name.ToValidName()}";
    }
}
