using System;

namespace Compiler.Ops;

// Shared operand-copy helper for the fused float ops below: emits the
// direct-copy macro invocation for whichever kind of float-producing
// operation fed this fused operand. See ILMethodFloatOperandFusionOptimizer
// (Compiler/ILMethodFloatOperandFusionOptimizer.cs) for exactly which
// operation kinds its rules match -- an OpLdloc/OpLdloc_s/OpLdarg/OpLdarg_s
// local/param read (RawParameter is already the converted rel_pos string by
// this point, set by ILMethodCodePass well before any peephole pass runs),
// or an already-fused IPushFldOperation field read (OpPushFld/OpPushFldAt/
// OpPushFldCached -- all three expose Pos uniformly, so this needs no
// per-class special-casing for the field side).
static class FloatOperandCopy
{
    public static string Emit(ILOperation source, string target)
    {
        switch (source.Operation)
        {
            case OpLdloc:
            case OpLdloc_s:
            case OpLdarg:
            case OpLdarg_s:
                return $"#locals_copy_valueflt {source.RawParameter}, {target}";
            case IPushFldOperation pushFld:
                return $"#pushfldflt_direct {pushFld.Pos}, {target}";
            default:
                throw new InvalidOperationException(
                    $"FloatOperandCopy: unexpected source operation {source.Operation.GetType().Name}");
        }
    }
}

// Built by ILMethodFloatOperandFusionOptimizer, replacing a
// [float operand] [float operand] [Add/Sub/Mul/Div] window: copies both
// operands straight from their source (a local/param's localsStack slot via
// locals_copy_valueflt, or an already-resolved field via pushfldflt_direct)
// into zp_flt_a/zp_flt_b, skipping the hardware-stack round trip the
// unfused OpLdloc/OpPushFld + addflt/subflt/mulflt/divflt path
// (asm/helper/float.asm) would otherwise use for each one. The jsr +
// result-push tail below is copied unchanged from those macros' own
// bodies, so whatever consumes the result afterward (another op, a Stloc,
// a Setfld) keeps working exactly as before -- this fusion only ever
// touches the operand side.
class OpFusedFloatBinOp : OpBase
{
    private readonly ILOperation _a;
    private readonly ILOperation _b;
    private readonly string _floatRoutine;

    public OpFusedFloatBinOp(ILOperation a, ILOperation b, string floatRoutine) : base(0)
    {
        _a = a;
        _b = b;
        _floatRoutine = floatRoutine;
    }

    public override string Emit(CompilerMethodContext context, ILOperation operation)
    {
        return string.Join("\n",
            FloatOperandCopy.Emit(_a, "zp_flt_a"),
            FloatOperandCopy.Emit(_b, "zp_flt_b"),
            $"jsr {_floatRoutine}",
            "#stack_push_var_mflpt zp_flt_a");
    }
}

// Same idea as OpFusedFloatBinOp, for negateflt's single operand -- no ROM
// call needed, MFLPT's sign is bit 7 of the first mantissa byte
// (zp_flt_a+1), skipped when the exponent byte is 0 so negating zero can't
// produce a non-canonical "negative zero" bit pattern. Mirrors negateflt's
// own body (asm/helper/float.asm) after its stack_pull_mflpt.
class OpFusedFloatUnOp : OpBase
{
    private readonly ILOperation _a;

    public OpFusedFloatUnOp(ILOperation a) : base(0)
    {
        _a = a;
    }

    public override string Emit(CompilerMethodContext context, ILOperation operation)
    {
        return string.Join("\n",
            FloatOperandCopy.Emit(_a, "zp_flt_a"),
            "lda zp_flt_a",
            "beq +",
            "lda zp_flt_a+1",
            "eor #$80",
            "sta zp_flt_a+1",
            "+ #stack_push_var_mflpt zp_flt_a");
    }
}

// Same idea again, for compareLessflt/compareGreaterflt -- the result is a
// bool (pushed via stack_push_int_x), not an mflpt value, same as those two
// macros' own tail. Float_Compare returns A = sign(a - b): 0, 1, or $FF;
// mirrors each macro's own branch-on-that-sign logic exactly.
class OpFusedFloatCompareOp : OpBase
{
    private readonly ILOperation _a;
    private readonly ILOperation _b;
    private readonly bool _isGreater;

    public OpFusedFloatCompareOp(ILOperation a, ILOperation b, bool isGreater) : base(0)
    {
        _a = a;
        _b = b;
        _isGreater = isGreater;
    }

    public override string Emit(CompilerMethodContext context, ILOperation operation)
    {
        var branch = _isGreater
            ? "bmi +\n    beq +"
            : "bpl +";
        return string.Join("\n",
            FloatOperandCopy.Emit(_a, "zp_flt_a"),
            FloatOperandCopy.Emit(_b, "zp_flt_b"),
            "jsr Float_Compare",
            "ldx #0",
            "cmp #0",
            branch,
            "inx",
            "+ #stack_push_int_x");
    }
}
