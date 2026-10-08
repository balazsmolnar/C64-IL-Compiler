using System;

namespace Compiler.Ops;

// Marker for a line that used to be a plain "add/sub/mul/divFloat" opcode
// but whose trailing result-push ILMethodFloatOperandFusionOptimizer has
// elided (asm/helper/float.asm's "_nopush" macros): the result is left
// sitting in zp_flt_a, about to be consumed directly by the very next op
// as its first operand, with nothing in between that could also need
// zp_flt_a as scratch. Built by mutating a still-live Add/Sub/Mul/Div/Neg
// line's own Operation in place (same technique
// ILMethodCachedFieldAccessPass uses for OpPushFldCached) -- the line
// keeps emitting real code at its own position, just without the push.
class OpFloatProducerNoPush : OpBase
{
    private readonly string _command;

    public OpFloatProducerNoPush(string command) : base(0)
    {
        _command = command;
    }

    public override string Emit(CompilerMethodContext context, ILOperation operation) => _command;
}

// Shared operand-copy helper for the fused float ops below: emits the
// direct-copy macro invocation for whichever kind of float-producing
// operation fed this fused operand. See ILMethodFloatOperandFusionOptimizer
// (Compiler/ILMethodFloatOperandFusionOptimizer.cs) for exactly which
// operation kinds its rules match -- an OpLdloc/OpLdloc_s/OpLdarg/OpLdarg_s
// local/param read (RawParameter is already the converted rel_pos string by
// this point, set by ILMethodCodePass well before any peephole pass runs),
// an already-fused IPushFldOperation field read (OpPushFld/OpPushFldAt/
// OpPushFldCached -- all three expose Pos uniformly, so this needs no
// per-class special-casing for the field side), an OpLdc_r4 float literal
// (RawParameter is already "b0,b1,b2,b3,b4", same MFLPT-byte string
// stack_push_mflpt_const itself takes), or a chained producer (the
// previous op's result, already sitting in zp_flt_a as a side effect of
// running it -- see OpFloatProducerNoPush above and
// OpFusedFloatBinOp/OpFusedFloatUnOp's own PushResult below).
static class FloatOperandCopy
{
    public static string Emit(ILOperation source, string target)
    {
        if (IsChainedProducer(source))
        {
            // Only ever requested for the first operand slot -- see
            // ILMethodFloatOperandFusionOptimizer's own comment on why
            // chaining is restricted to "feeds operand A, operand B (if
            // any) is a plain leaf": the result is already exactly where
            // operand A needs to be, so there is nothing left to emit.
            if (target != "zp_flt_a")
                throw new InvalidOperationException(
                    "FloatOperandCopy: a chained producer was requested for a non-zp_flt_a target.");
            return "";
        }

        switch (source.Operation)
        {
            case OpLdloc:
            case OpLdloc_s:
            case OpLdarg:
            case OpLdarg_s:
                return $"#locals_copy_valueflt {source.RawParameter}, {target}";
            case IPushFldOperation pushFld:
                return $"#pushfldflt_direct {pushFld.RelPos}, {pushFld.Pos}, {target}";
            case OpLdc_r4:
                return $"#copy_mflpt_const {source.RawParameter}, {target}";
            default:
                throw new InvalidOperationException(
                    $"FloatOperandCopy: unexpected source operation {source.Operation.GetType().Name}");
        }
    }

    public static bool IsChainedProducer(ILOperation source) =>
        source.Operation is OpFloatProducerNoPush ||
        (source.Operation is OpFusedFloatBinOp fb && !fb.PushResult) ||
        (source.Operation is OpFusedFloatUnOp fu && !fu.PushResult);
}

// Built by ILMethodFloatOperandFusionOptimizer, replacing a
// [float operand] [float operand] [Add/Sub/Mul/Div] window: copies both
// operands straight from their source (a local/param's localsStack slot via
// locals_copy_valueflt, an already-resolved field via pushfldflt_direct, a
// literal via copy_mflpt_const, or a chained producer's leftover zp_flt_a)
// into zp_flt_a/zp_flt_b, skipping the hardware-stack round trip the
// unfused OpLdloc/OpPushFld/OpLdc_r4 + addflt/subflt/mulflt/divflt path
// (asm/helper/float.asm) would otherwise use for each one.
class OpFusedFloatBinOp : OpBase
{
    private readonly ILOperation _a;
    private readonly ILOperation _b;
    private readonly string _floatRoutine;

    // Settable, not just constructor-supplied: ILMethodFloatOperandFusionOptimizer
    // may decide, on a LATER iteration of its own scan (after this op has
    // already been built and inserted), that THIS op's own result feeds
    // directly into a following op as its first operand -- at which point
    // it flips this to false retroactively, eliding the push below. See
    // that pass's own MarkAsChainProducer.
    public bool PushResult = true;

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
            PushResult ? "#stack_push_var_mflpt zp_flt_a" : "");
    }
}

// Same idea as OpFusedFloatBinOp, for negateflt's single operand -- no ROM
// call needed, MFLPT's sign is bit 7 of the first mantissa byte
// (zp_flt_a+1), skipped when the exponent byte is 0 so negating zero can't
// produce a non-canonical "negative zero" bit pattern. Mirrors negateflt's
// own body (asm/helper/float.asm) after its stack_pull_mflpt. PushResult
// has the same chaining purpose as OpFusedFloatBinOp's own copy above.
class OpFusedFloatUnOp : OpBase
{
    private readonly ILOperation _a;

    public bool PushResult = true;

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
            PushResult ? "+ #stack_push_var_mflpt zp_flt_a" : "+");
    }
}

// Same idea again, for compareLessflt/compareGreaterflt -- the result is a
// bool (pushed via stack_push_int_x), not an mflpt value, same as those two
// macros' own tail, so there is nothing to chain further (a compare's
// result is never itself a float operand) -- no PushResult toggle here.
// Float_Compare returns A = sign(a - b): 0, 1, or $FF; mirrors each
// macro's own branch-on-that-sign logic exactly.
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
