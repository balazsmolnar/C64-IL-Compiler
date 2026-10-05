using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

// Replaces "[a PURE 1-byte producer]; Conv_u8/Conv_i8" with a single
// synthesized op that re-emits the producer completely unchanged, preceded
// by a literal 0-byte push for the new high byte -- see
// Compiler/Operands/OperandConv.cs's OpFusedWiden8To16 for the full
// reasoning (why pushing 0 first, rather than the #conv_8_16 macro's
// pull-the-byte-back-then-push-it-again, is correct for a PURE producer:
// one that only ever grows the stack by its own single push, never pops a
// value some EARLIER, separate instruction already left there for it).
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
            //
            // The PreviousInstructions/StackContent.Count comparison is
            // the actual safety check this rule depends on, and it used to
            // be missing entirely: the original version matched ANY
            // 1-byte-producing op, including ones that POP an operand some
            // earlier, separate instruction already pushed (Ldfld reading
            // a field off a DYNAMICALLY referenced object, not `this` --
            // that case is fused into the self-contained #pushfld macro
            // instead and never reaches here as a raw Ldfld, see
            // OpPushFld -- or Add/Sub/etc). For one of those, this rule's
            // extra "push 0 first" lands the 0 ON TOP of the real operand
            // the consuming op expected to find there, so it reads/adds
            // the wrong byte. Confirmed for real in Hunchback: `Rope.PlayerX`
            // (Ldfld on a non-`this` reference) silently loaded field 4 of
            // whichever object happens to hold handle 0 instead -- "X =
            // Rope.PlayerX" assigning the player's X went wrong (reported
            // as "the player disappears while hanging on the rope"), while
            // splitting it into "var x = Rope.PlayerX; X = x;" worked,
            // because the intervening Stloc/Ldloc pair breaks the direct
            // [producer; Conv] adjacency this rule matches on. A second,
            // independent instance (an Add, in LevelPlay.Play's own scoring
            // expression) was found the same way, confirming this needed a
            // general fix, not a one-off exclusion of Ldfld specifically.
            // A true PURE producer (Ldloc/Ldarg -- reads the locals stack,
            // not the evaluation stack; #pushfld -- resolves `this`
            // directly, same reason) only ever grows the evaluation stack
            // by the one byte it pushes, so StackContent.Count strictly
            // increases over its own immediate predecessor's; anything
            // that also pops first will, at best, leave the count
            // unchanged (pop 1, push 1) or shrink it (pop 2, push 1) --
            // exactly the operations this rule must now reject.
            l => l.StackContent != null && l.StackContent.Count > 0 && l.StackContent.Last().GetStorageBytes() == 1
                && l.PreviousInstructions.Count > 0
                && l.StackContent.Count > l.PreviousInstructions.First().StackContent.Count,
            l => (l.OpCode == ILOpCode.Conv_u8 || l.OpCode == ILOpCode.Conv_i8) && string.IsNullOrEmpty(l.Label))
            .WithStackContent(w => w[1].StackContent),
    };
}
