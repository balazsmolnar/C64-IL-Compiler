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
                    ? $"#pushfld16_direct {pushFld.RelPos}, {pushFld.Pos}, {target}"
                    : $"#pushfld8_direct {pushFld.RelPos}, {pushFld.Pos}, {target}";
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
                return LocalsAccess.Load(source.RawParameter);
            case IPushFldOperation pushFld:
                return $"{LocalsAccess.Load(pushFld.RelPos, "ldx")}\njsr resolveObjPtr\nldy #{pushFld.Pos}\nlda (tmpPointer),y";
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

// A 16-bit add/subtract operand the arith16_* macros (asm/helper/
// arithmetic.asm) can address directly: a constant, or a local/parameter.
static class Int16DirectOperand
{
    public static bool IsDirect(ILOperation source) =>
        source.Operation is OpLdloc || source.Operation is OpLdloc_s || source.Operation is OpLdarg ||
        source.Operation is OpLdarg_s || source.Operation is OpLdConst;

    // "kind, value" as the macros take it.
    public static string Emit(ILOperation source) =>
        source.Operation is OpLdConst ? $"0, {source.RawParameter}" : $"1, {source.RawParameter}";
}

// Built by ILMethodIntOperandFusionOptimizer, replacing a
// [16-bit local/parameter/constant] [the same] [Add/Sub] window, and the
// Stloc after it if there is one: the sum is computed straight from the
// operands ("#arith16_direct") and, with a destination, straight into it
// ("#arith16_store"), instead of both operands being copied to zp_param1/2,
// added there and the result pushed.
class OpFusedInt16Direct : OpBase
{
    private readonly ILOperation _a;
    private readonly ILOperation _b;
    private readonly bool _subtract;
    private readonly ILOperation _store;

    public OpFusedInt16Direct(ILOperation a, ILOperation b, bool subtract, ILOperation store) : base(0)
    {
        _a = a;
        _b = b;
        _subtract = subtract;
        _store = store;
    }

    // The rel_pos of a Stloc's local (its parameter is "rel_pos, isRef").
    public static string StoreRelPos(ILOperation store) => store.RawParameter.ToString().Split(',')[0].Trim();

    public override string Emit(CompilerMethodContext context, ILOperation operation)
    {
        var operands = $"{(_subtract ? 1 : 0)}, {Int16DirectOperand.Emit(_a)}, {Int16DirectOperand.Emit(_b)}";
        return _store == null
            ? $"#arith16_direct {operands}"
            : $"#arith16_store {operands}, {StoreRelPos(_store)}";
    }
}

// Built by ILMethodIntOperandFusionOptimizer: a 16-bit Add/Sub whose first
// operand is already on the evaluation stack (a field, a call's result, an
// expression) and whose second is a constant or a local/parameter, which is
// added byte by byte as it is pulled ("#arith16_stack") instead of being
// pushed first and both then copied to zp_param1/2.
class OpFusedInt16Stack : OpBase
{
    private readonly ILOperation _b;
    private readonly bool _subtract;

    public OpFusedInt16Stack(ILOperation b, bool subtract) : base(0)
    {
        _b = b;
        _subtract = subtract;
    }

    public override string Emit(CompilerMethodContext context, ILOperation operation) =>
        $"#arith16_stack {(_subtract ? 1 : 0)}, {Int16DirectOperand.Emit(_b)}";
}

// Built by ILMethodIntOperandFusionOptimizer: And/Or/Xor whose second
// operand is an int constant, which goes in as an immediate
// ("and8_const 7", asm/helper/arithmetic.asm) instead of being pushed and
// pulled. The first operand is either already on the evaluation stack
// (a == null: "#and8_const c", pla/and #c/pha) or, 8-bit only, a simple
// leaf loaded straight into A ("#and8_const_tail c"). 16-bit always works
// on the stack: copying a leaf into zp_param2 first is no shorter than
// pushing it, and the stack form skips each byte the constant leaves alone.
class OpFusedIntConstOp : OpBase
{
    private readonly ILOperation _a;
    private readonly object _value;
    private readonly string _op;
    private readonly bool _is16;

    public OpFusedIntConstOp(ILOperation a, object value, string op, bool is16) : base(0)
    {
        _a = a;
        _value = value;
        _op = op;
        _is16 = is16;
    }

    public override string Emit(CompilerMethodContext context, ILOperation operation)
    {
        var width = _is16 ? "16" : "8";
        if (_a == null)
            return $"#{_op}{width}_const {_value}";
        return string.Join("\n", IntOperandCopy.EmitToRegisterA(_a), $"#{_op}{width}_const_tail {_value}");
    }
}

// Built by ILMethodIntOperandFusionOptimizer, replacing a
// [int operand] [Brtrue/Brfalse] window -- a bare `if (x)`/`if (!x)`
// truthiness test on a simple leaf (branch_true8/branch_false8,
// asm/helper/branch.asm, both just "#stack_pull_int_a" then a single
// bne/beq). Unlike OpFusedIntBinOp there is no "_tail" macro to reuse --
// the tail is one instruction, simple enough to inline directly here --
// so this just loads the operand straight into register A
// (IntOperandCopy.EmitToRegisterA, the same register-direct convention
// add8/sub8's own first operand already uses) and branches.
class OpFusedBranchTruthiness : OpBase
{
    private readonly ILOperation _a;
    private readonly bool _branchIfTrue;
    private readonly string _label;

    public OpFusedBranchTruthiness(ILOperation a, bool branchIfTrue, string label) : base(0)
    {
        _a = a;
        _branchIfTrue = branchIfTrue;
        _label = label;
    }

    public override string Emit(CompilerMethodContext context, ILOperation operation)
    {
        var branch = _branchIfTrue ? "bne" : "beq";
        return string.Join("\n", IntOperandCopy.EmitToRegisterA(_a), $"{branch} {_label}");
    }
}
