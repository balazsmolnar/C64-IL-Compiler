using System.Collections.Generic;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

// Replaces "push constant 1/2/4; div.un" with a fixed-count logical
// right shift (asm/helper/arithmetic.asm's div_shift_const8/16) --
// mirrors ILMethodMulConstOptimizer's own multiply-by-power-of-two
// fusion exactly, just in the other direction, with one deliberate
// restriction that optimizer doesn't need.
//
// Div_un ONLY, never the signed Div: the asm macro this fuses into
// (div_shift_const8/16) is a LOGICAL right shift (6502 `lsr`/`ror`),
// which zero-fills from the top regardless of the operand's own sign
// bit -- for a negative two's-complement value this doesn't just round
// differently, it produces a completely different, large POSITIVE
// number (e.g. 8-bit -4 is 0xFC; `lsr` once gives 0x7E = 126, nowhere
// near -4/2's correct answer of -2). This compiler can't prove a given
// runtime value is non-negative at compile time, so folding this for
// signed division would silently corrupt the result for any negative
// dividend. Division has no such ambiguity for UNSIGNED values: there's
// no sign to get wrong, so the shift is unconditionally exact there,
// same as multiplication's own "regardless of the other operand's sign"
// claim already relies on.
class ILMethodDivConstOptimizer : PeepholeOptimizerPass
{
    private static readonly Dictionary<int, int> ShiftForValue = new()
    {
        [1] = 0,
        [2] = 1,
        [4] = 2,
    };

    protected override IEnumerable<PeepholeRule> Rules => new[]
    {
        // uint: the constant divisor feeds straight into Div_un.
        new PeepholeRule(
            (ctx, w) => new OpArithmetic2("#div_shift_const"),
            l => l.Operation is OpLdConst && l.RawParameter is int v && ShiftForValue.ContainsKey(v),
            l => l.OpCode == ILOpCode.Div_un && string.IsNullOrEmpty(l.Label))
            .WithRawParameter((ctx, w) => ShiftForValue[(int)w[0].RawParameter]),

        // ulong: Roslyn widens an int constant divisor with Conv_i8 OR
        // Conv_u8 before a 64-bit unsigned divide -- same shape
        // ILMethodMulConstOptimizer's own second rule exists for, and
        // checking both for the same reason that rule does: verified
        // directly (not assumed, after guessing "always Conv_u8, never
        // Conv_i8" here was simply wrong -- a real ulong-parameter divide
        // actually compiled via Conv_i8), Roslyn's choice of widening
        // opcode for a small non-negative int LITERAL isn't tied to the
        // destination's signedness at all: the bit pattern is identical
        // either way for a value that fits non-negative in both, so it
        // doesn't matter which one shows up, only that this rule matches
        // whichever one does.
        new PeepholeRule(
            (ctx, w) => new OpArithmetic2("#div_shift_const"),
            l => l.Operation is OpLdConst && l.RawParameter is int v && ShiftForValue.ContainsKey(v),
            l => l.OpCode == ILOpCode.Conv_i8 || l.OpCode == ILOpCode.Conv_u8,
            l => l.OpCode == ILOpCode.Div_un && string.IsNullOrEmpty(l.Label))
            .WithRawParameter((ctx, w) => ShiftForValue[(int)w[0].RawParameter]),
    };
}
