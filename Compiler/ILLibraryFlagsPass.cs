using System.IO;
using System.Linq;

namespace Compiler;

// Writes library_flags.asm -- one `Flag_<label> = 1` equate per C64Lib
// method actually called (ILLibraryUsagePass records these), which the
// matching `.weak Flag_<label> = 0 .endweak .if Flag_<label> ...
// .endif`-wrapped subroutine in asm/C64.asm, asm/c64sprite.asm,
// asm/c64Keys.asm, asm/C64Sound.asm, asm/c64Joystick.asm, or
// asm/C64Debug.asm reads. Anything NOT in this file simply keeps its own
// weak default of 0 (excluded) -- no need to enumerate the full C64Lib
// surface here, 64tass's .weak/.endweak resolves a "stronger symbol from
// outside, if any" regardless of source order (see the 64tass README's
// ".weak" section, and the worked example there this whole mechanism is
// built on). Must run after ILCodePass has processed every method, and its
// output must be .include'd before C64.asm in every entry asm file
// (main.asm/hunchback.asm/presentation.asm/unittest.asm), since a
// `Flag_X = 1` here only overrides a later-declared `.weak` default if
// 64tass has already SEEN this strong definition by the time it resolves
// the symbol -- .include order still needs to put this first, same
// constraint the old USE_JOYSTICK/USE_SOUND/USE_DEBUG version had.
class ILLibraryFlagsPass : ICompilerPass
{
    // A handful of hand-written asm subroutines exist only to be jsr'd (or,
    // in add_Interrupt's case, address-of'd into the IRQ vector) by ANOTHER
    // library subroutine -- never directly by compiled C# code, so
    // ILLibraryUsagePass's Call/Callvirt scan structurally can never see
    // them. Found by exhaustively reading asm/C64.asm's internal call
    // graph: C64_SetChar jumps (tail-call, not jsr, but an equally hard
    // dependency) into C64_SetChar_Core; C64_GetChar jsr's
    // C64_GetChar_Core; C64_SetChar/C64_GetChar/C64_Write all jsr
    // C64_Set_Screen_Ptr; C64_add_Interrupt pokes OnInterrupt's address
    // into $0314/$0315. If a future asm edit adds a new internal-only
    // helper called by an existing public one, add an entry here too --
    // otherwise it'll default to excluded (0) the moment its caller is
    // used without it, and 64tass will fail with an undefined symbol.
    private static readonly (string used, string[] implies)[] ImpliedLabels = new[]
    {
        ("C64_SetChar", new[] { "C64_Set_Screen_Ptr", "C64_SetChar_Core" }),
        ("C64_GetChar", new[] { "C64_Set_Screen_Ptr", "C64_GetChar_Core" }),
        ("C64_Write", new[] { "C64_Set_Screen_Ptr" }),
        ("C64_add_Interrupt", new[] { "OnInterrupt" }),
    };

    public void Execute(CompilerContext context)
    {
        var labels = context.UsedLibraryLabels;
        foreach (var (used, implies) in ImpliedLabels)
        {
            if (labels.Contains(used))
                foreach (var implied in implies)
                    labels.Add(implied);
        }

        using var writer = File.CreateText(Path.Combine(context.OutputDirectory, "library_flags.asm"));
        foreach (var label in labels.OrderBy(l => l))
            writer.WriteLine($"Flag_{label} = 1");
    }
}
