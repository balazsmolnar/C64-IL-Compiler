using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

// Replaces "[any 1-byte-producing op]; Conv_u8/Conv_i8" with a single
// synthesized op that re-emits the producer completely unchanged, preceded
// by a literal 0-byte push for the new high byte -- see
// Compiler/Operands/OperandConv.cs's OpFusedWiden8To16 for the full
// reasoning (why pushing 0 first, rather than the #conv_8_16 macro's
// pull-the-byte-back-then-push-it-again, is correct for ANY producer, not
// just simple ones).
//
// A constant (Ldc_i4/_s/_const) followed by Conv_u8/Conv_i8 is already
// handled with zero overhead by OpLdConst itself (SetStackContent/Is16Bit,
// Compiler/Operands/OperandBase.cs -- it looks ahead at its own next
// instruction and self-emits as a 16-bit push directly), so its own
// StackContent is already 2 bytes wide by the time this rule's window[0]
// check runs and it correctly never matches here -- no overlap with
// ILMethodMulConstOptimizer's Ldc-Conv-Mul rule despite both watching for
// Conv_u8/Conv_i8.
//
// Real-world impact: found by inspecting Demo/Program.cs's generated
// asm -- every uint argument passed to a C64Lib API expecting ulong (the
// bitmap graphics library's Screen.SetPixel/DrawLine/DrawRectangle/
// DrawCircle, Sprite.X/Y) hit the never-optimized case, each one paying for
// a full extra pull-and-push-twice round trip through the hardware stack.
class ILMethodWiden8To16Optimizer : PeepholeOptimizerPass
{
    protected override IEnumerable<PeepholeRule> Rules => new[]
    {
        new PeepholeRule(
            (ctx, w) => new OpFusedWiden8To16(w[0]),
            // Unlike every other existing rule, this matcher probes
            // StackContent on EVERY operation unconditionally (nothing
            // gates it behind a specific OpCode/type check first) --
            // exposing a real crash no other rule ever hit: some
            // operations' StackContent is null, not merely empty (found
            // via a live NullReferenceException, Compiler.exe crashing
            // silently non-zero-exit under run.bat's own errorlevel check
            // getting masked by the shell pipe it ran through).
            l => l.StackContent != null && l.StackContent.Count > 0 && l.StackContent.Last().GetStorageBytes() == 1,
            l => (l.OpCode == ILOpCode.Conv_u8 || l.OpCode == ILOpCode.Conv_i8) && string.IsNullOrEmpty(l.Label))
            .WithStackContent(w => w[1].StackContent),
    };
}
