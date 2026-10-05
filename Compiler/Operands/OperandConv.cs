using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compiler.Ops;

namespace Compiler.Ops;

class OpConv_8_16 : OpBase
{
    public OpConv_8_16() : base(0, "#conv_8_16")
    {

    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        var last = operation.StackContent.Last();
        // last.CheckCompatible(typeof(int));
        operation.StackContent.RemoveLast(1);
        operation.StackContent.Add(typeof(long));
    }

    public override string Emit(CompilerMethodContext context, ILOperation operation)
    {
        if (operation.PreviousInstructions.First().StackContent.Last().GetStorageBytes() == 1)
            return base.Emit(context, operation);
        else
            return "; conv";
    }

}

class OpConv_16_8 : OpBase
{
    public OpConv_16_8() : base(0, "#conv_16_8")
    {

    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        var last = operation.StackContent.Last();
        // last.CheckCompatible(typeof(long));
        operation.StackContent.RemoveLast(1);
        operation.StackContent.Add(typeof(int));
    }

    // Conv_i4/Conv_u4 (the only two IL opcodes mapped to this class) can
    // also arrive after a float value (a C# explicit "(int)someFloat" cast),
    // which needs the ROM's float-to-int routine, not the 16-to-8 narrowing
    // this class otherwise handles -- checked first, ahead of the existing
    // GetStorageBytes()==2 check, since a float's predecessor stack content
    // is never a match for that check anyway (5 bytes, not 2).
    public override string Emit(CompilerMethodContext context, ILOperation operation)
    {
        if (operation.PreviousInstructions.First().StackContent.Last() == typeof(float))
            return "#conv_float_to_int";
        if (operation.PreviousInstructions.First().StackContent.Last().GetStorageBytes() == 2)
            return base.Emit(context, operation);
        else
            return "; conv";
    }
}

// Conv_r4 (int -> float, e.g. an explicit "(float)someInt" cast or an
// implicit widening in a mixed-type expression). Mirrors OpConv_8_16/
// OpConv_16_8's "no real work when the predecessor is already the target
// shape" pattern, for the same reason: harmless to check, cheap insurance
// against a redundant Conv_r4 Roslyn might emit after something that
// already produced a float.
//
// Only int/uint sources are handled (the ROM's GIVAYF wants a genuine
// 16-bit value, and this compiler's int/uint are both 1 byte -- the two
// macros below differ only in whether that byte gets sign- or
// zero-extended to 16 bits before the call). long/ulong -> float is NOT
// implemented -- would need a different, wider path this pass doesn't
// build; falls through to #conv_int_to_float (silently wrong: only the low
// byte would convert) rather than a clean compile-time error, same
// "pre-existing gap, not newly introduced" territory as integer Div.
class OpConvIntToFloat : OpBase
{
    public OpConvIntToFloat() : base(0, "#conv_int_to_float")
    {
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        operation.StackContent.RemoveLast(1);
        operation.StackContent.Add(typeof(float));
    }

    public override string Emit(CompilerMethodContext context, ILOperation operation)
    {
        var sourceType = operation.PreviousInstructions.First().StackContent.Last();
        if (sourceType == typeof(float))
            return "; conv";
        if (sourceType == typeof(uint))
            return "#conv_uint_to_float";
        return "#conv_int_to_float";
    }
}

// Synthesized replacement for [a PURE 1-byte-producing operation; Conv_u8 or
// Conv_i8] -- see ILMethodWiden8To16Optimizer for the matching rule (and
// the StackContent.Count check that makes "pure" a real, checked
// precondition, not an assumption) and the full reasoning. OpConv_8_16's
// own #conv_8_16 macro (asm/helper/arithmetic.asm) already handles this
// correctly: pull the byte the producer just pushed back off the hardware
// stack, push a 0 high byte, push the byte back as the low byte. That's
// the producer's own push, plus a pull and two more pushes on top -- pure
// overhead, since #stack_push_int16's "high byte pushed first" convention
// means the 0 just needs to land on the stack BEFORE the producer's push
// runs, not be spliced in after it by pulling the producer's byte back
// out. So this re-emits the producer's own text completely unchanged,
// preceded by a literal 0-byte push -- correct ONLY for a producer that
// never pops anything a DIFFERENT, earlier instruction already left on the
// stack for it (a local/field-of-`this`/constant read, or a method call
// whose own return-value push is its very last action -- each one's own
// hardware-stack usage is self-balanced by construction, touching nothing
// below where it started). It is NOT correct for an operation that pops an
// operand pushed earlier (Ldfld on a non-`this` object reference, Add,
// Sub, ...): the 0 this op pushes first lands on TOP of that real operand,
// so the operation ends up popping the 0 instead -- confirmed as a real
// bug this way (Hunchback's `X = Rope.PlayerX` silently reading a
// different object's field instead), which is exactly why
// ILMethodWiden8To16Optimizer's matcher now checks StackContent.Count
// actually grew, instead of assuming every producer qualifies.
class OpFusedWiden8To16 : OpBase
{
    private readonly ILOperation _inner;

    public OpFusedWiden8To16(ILOperation inner) : base(0)
    {
        _inner = inner;
    }

    public override string Emit(CompilerMethodContext context, ILOperation operation)
    {
        return $"#stack_push_int8 0\n    {_inner.Operation.Emit(context, _inner)}";
    }
}