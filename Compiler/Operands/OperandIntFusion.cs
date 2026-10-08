using System;

namespace Compiler.Ops;

// Int counterpart of FloatOperandCopy (OperandFloatFusion.cs) -- emits the
// direct-copy macro invocation for whichever kind of int-producing
// operation fed this fused operand. Width (8 vs 16-bit) is read off the
// source's own StackContent (GetStorageBytes()) for a local/param/const --
// but NOT for a field: ILPropertyGettterOptimizer's common "Ldarg_0;Ldfld"
// rule deliberately leaves the synthesized OpPushFld's StackContent null
// (same hazard FloatOperandCopy's own comment calls out), so a field's
// width comes from IPushFldOperation.Is16BitField directly instead --
// relying on StackContent there crashes with a NullReferenceException the
// moment a fused field read is actually exercised (found via
// Compiler.UnitTests' own CachedFieldAccessTests/DiagnosticsTests once
// this pass was wired in). See ILMethodIntOperandFusionOptimizer
// (Compiler/ILMethodIntOperandFusionOptimizer.cs) for exactly which
// operation kinds its rules match, and for the 16-bit widened-constant
// case (Ldc_i4 immediately followed by Conv_i8/Conv_u8, which itself
// emits nothing -- OpConv_8_16.Emit) -- that pass always hands this class
// the Ldc line itself, never the Conv line, so no extra case is needed
// here for it.
static class IntOperandCopy
{
    private static bool Is16(ILOperation source) => source.StackContent.Last().GetStorageBytes() == 2;

    public static string Emit(ILOperation source, string target)
    {
        switch (source.Operation)
        {
            case OpLdloc:
            case OpLdloc_s:
            case OpLdarg:
            case OpLdarg_s:
                return Is16(source)
                    ? $"#locals_copy_value16 {source.RawParameter}, {target}"
                    : $"#locals_copy_value8 {source.RawParameter}, {target}";
            case IPushFldOperation pushFld:
                return pushFld.Is16BitField
                    ? $"#pushfld16_direct {pushFld.Pos}, {target}"
                    : $"#pushfld8_direct {pushFld.Pos}, {target}";
            case OpLdConst:
                return Is16(source)
                    ? $"#copy_int16_const {source.RawParameter}, {target}"
                    : $"#copy_int8_const {source.RawParameter}, {target}";
            default:
                throw new InvalidOperationException(
                    $"IntOperandCopy: unexpected source operation {source.Operation.GetType().Name}");
        }
    }

    // 8-bit only -- add8/sub8's own convention of taking their first
    // operand directly in register A, never via a named zp byte (see
    // OpFusedIntBinOp's own comment on why this exists alongside Emit
    // above rather than every caller routing through a zp byte
    // regardless).
    public static string EmitToRegisterA(ILOperation source)
    {
        switch (source.Operation)
        {
            case OpLdloc:
            case OpLdloc_s:
            case OpLdarg:
            case OpLdarg_s:
                return $"ldy stackPointer\nlda localsStack-{source.RawParameter},y";
            case IPushFldOperation pushFld:
                return $"ldy stackPointer\ndey\nldx localsStack,y\njsr resolveObjPtr\nldy #{pushFld.Pos}\nlda (tmpPointer),y";
            case OpLdConst:
                return $"lda #<{source.RawParameter}";
            default:
                throw new InvalidOperationException(
                    $"IntOperandCopy: unexpected source operation {source.Operation.GetType().Name}");
        }
    }
}

// Built by ILMethodIntOperandFusionOptimizer, replacing a
// [int operand] [int operand] [Add/Sub/Mul] window: copies both operands
// straight from their source into add8_tail/sub8_tail/mul8_tail's (or
// their 16-bit counterparts') own expected scratch locations, instead of
// each operand independently round-tripping through the hardware stack.
//
// Two different conventions, both already set by the existing, unfused
// macros this reuses the "_tail" of (asm/helper/arithmetic.asm) -- this
// class doesn't choose either, it just replicates whichever one the
// target op already uses:
//   - add8/sub8: the SECOND-evaluated operand (b) goes to a named zp byte
//     (zp_param1_low), but the FIRST-evaluated operand (a) is consumed
//     directly out of register A -- aIsRegister=true, aTarget unused.
//   - mul8/add16/sub16/mul16: BOTH operands go to named zp
//     bytes/pairs -- aIsRegister=false, aTarget given.
// B is copied first, A second (matching the original macros' own pull
// order -- B off the hardware stack first, A second), so A lands in
// place immediately before the tail needs it.
class OpFusedIntBinOp : OpBase
{
    private readonly ILOperation _a;
    private readonly ILOperation _b;
    private readonly bool _aIsRegister;
    private readonly string _aTarget;
    private readonly string _bTarget;
    private readonly string _tailMacro;

    public OpFusedIntBinOp(ILOperation a, ILOperation b, bool aIsRegister, string aTarget, string bTarget, string tailMacro) : base(0)
    {
        _a = a;
        _b = b;
        _aIsRegister = aIsRegister;
        _aTarget = aTarget;
        _bTarget = bTarget;
        _tailMacro = tailMacro;
    }

    public override string Emit(CompilerMethodContext context, ILOperation operation)
    {
        var bCopy = IntOperandCopy.Emit(_b, _bTarget);
        var aCopy = _aIsRegister ? IntOperandCopy.EmitToRegisterA(_a) : IntOperandCopy.Emit(_a, _aTarget);
        return string.Join("\n", bCopy, aCopy, _tailMacro);
    }
}
