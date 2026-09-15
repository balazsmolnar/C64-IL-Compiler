using System.Collections.Generic;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

// Replaces "push constant 1/2/4; mul" with a fixed-count bit-shift (0/1/2
// asl's, baked in at compile time -- no runtime loop at all, unlike the
// general-purpose #shift_left which pulls its shift count off the stack
// every time it runs). Truncated multiplication by a power of two is
// exactly a left shift regardless of the other operand's sign, so this is
// safe for both int and uint.
//
// Mirrors ILMethodCompareConstOptimizer's shape (match "const, then op",
// replace with a "_const" macro variant) but only fires for the specific
// values that have a shift equivalent -- any other constant (3, 5, 100...)
// falls through untouched to the general #mul8/#mul16 shift-and-add macro.
class ILMethodMulConstOptimizer : PeepholeOptimizerPass
{
    private static readonly Dictionary<int, int> ShiftForValue = new()
    {
        [1] = 0,
        [2] = 1,
        [4] = 2,
    };

    protected override IEnumerable<PeepholeRule> Rules => new[]
    {
        // int/uint: the constant feeds straight into Mul.
        new PeepholeRule(
            (ctx, w) => new OpArithmetic2("#mul_shift_const"),
            l => l.Operation is OpLdConst && l.RawParameter is int v && ShiftForValue.ContainsKey(v),
            l => l.OpCode == ILOpCode.Mul && string.IsNullOrEmpty(l.Label))
            .WithRawParameter((ctx, w) => ShiftForValue[(int)w[0].RawParameter]),

        // long/ulong: Roslyn always widens an int constant with Conv_i8/
        // Conv_u8 before a 64-bit multiply (the same shape OpLdConst.Is16Bit
        // already special-cases to decide the constant itself needs a
        // 16-bit push) -- one more line in the window to match past that
        // conversion. Neither this compiler's other "_const" optimizers
        // (Compare/Branch) nor any "_const16" macro exist, so those never
        // fire for long/ulong at all; this one only extends that far
        // because #mul_shift_const16 was worth writing anyway for parity
        // with #mul16.
        new PeepholeRule(
            (ctx, w) => new OpArithmetic2("#mul_shift_const"),
            l => l.Operation is OpLdConst && l.RawParameter is int v && ShiftForValue.ContainsKey(v),
            l => l.OpCode == ILOpCode.Conv_i8 || l.OpCode == ILOpCode.Conv_u8,
            l => l.OpCode == ILOpCode.Mul && string.IsNullOrEmpty(l.Label))
            .WithRawParameter((ctx, w) => ShiftForValue[(int)w[0].RawParameter]),
    };
}
