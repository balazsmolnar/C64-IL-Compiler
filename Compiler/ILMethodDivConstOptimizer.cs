using System.Linq;
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
//
// Also, for any other constant divisor 3..255 (Div_un and Rem_un): a call
// into a routine specialised for that divisor (OpDivConst, asm/helper/
// arithmetic.asm's div_const8/16 and rem_const8/16), and Rem_un by a power
// of two up to 256 becomes a mask. Every window requires the Div/Rem and
// the Conv (a branch join would bring in another divisor) to be unlabelled.
class ILMethodDivConstOptimizer : PeepholeOptimizerPass
{
    private static readonly Dictionary<int, int> ShiftForValue = new()
    {
        [1] = 0,
        [2] = 1,
        [4] = 2,
        [8] = 3,
        [16] = 4,
        [32] = 5,
        [64] = 6,
        [128] = 7,
    };

    private static bool IsConst(ILOperation l, System.Func<int, bool> ok) =>
        l.Operation is OpLdConst && l.RawParameter is int v && ok(v);

    private static bool IsWiden(ILOperation l) =>
        (l.OpCode == ILOpCode.Conv_i8 || l.OpCode == ILOpCode.Conv_u8) && string.IsNullOrEmpty(l.Label);

    private static bool Is(ILOperation l, ILOpCode op) => l.OpCode == op && string.IsNullOrEmpty(l.Label);

    private static bool IsShift(int v) => ShiftForValue.ContainsKey(v);
    private static bool IsRoutineDivisor(int v) => v >= 3 && v <= 255 && !IsShift(v);
    private static bool IsMask(int v) => v >= 1 && v <= 256 && (v & (v - 1)) == 0;

    // Each rule in an 8-bit form (constant straight into the op) and a
    // 16-bit one (constant, Conv to 64-bit -- this compiler's 16 bits --,
    // op).
    private static IEnumerable<PeepholeRule> Both(System.Func<OpBase> build, System.Func<int, bool> constant, ILOpCode op, System.Func<int, object> parameter)
    {
        yield return new PeepholeRule((ctx, w) => build(), l => IsConst(l, constant), l => Is(l, op))
            .WithRawParameter((ctx, w) => parameter((int)w[0].RawParameter))
            .AllowInlinedWindows();
        yield return new PeepholeRule((ctx, w) => build(), l => IsConst(l, constant), IsWiden, l => Is(l, op))
            .WithRawParameter((ctx, w) => parameter((int)w[0].RawParameter))
            .AllowInlinedWindows();
    }

    protected override IEnumerable<PeepholeRule> Rules =>
        Both(() => new OpArithmetic2("#div_shift_const"), IsShift, ILOpCode.Div_un, v => ShiftForValue[v])
        .Concat(Both(() => new OpDivConst("#div_const"), IsRoutineDivisor, ILOpCode.Div_un, v => v))
        .Concat(Both(() => new OpDivConst("#rem_const"), IsRoutineDivisor, ILOpCode.Rem_un, v => v))
        .Concat(Both(() => new OpArithmetic2("#rem_mask"), IsMask, ILOpCode.Rem_un, v => v - 1));
}
