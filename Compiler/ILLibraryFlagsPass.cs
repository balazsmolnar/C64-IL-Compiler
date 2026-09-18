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
//
// A handful of hand-written asm subroutines (C64_Set_Screen_Ptr,
// C64_SetChar_Core, C64_GetChar_Core, OnInterrupt) exist only to be jsr'd
// (or, OnInterrupt's case, address-of'd into the IRQ vector) by ANOTHER
// library subroutine -- never directly by compiled C# code, so
// ILLibraryUsagePass's Call/Callvirt scan structurally can never see them,
// and no Flag_ ever gets written here for them. Rather than tracking that
// here with a lookup table, those subroutines' own .weak/.if guards in
// asm/C64.asm just reference their caller's flag directly (or an OR of
// several callers' flags) instead of declaring one of their own -- see the
// comment above C64_Set_Screen_Ptr there.
class ILLibraryFlagsPass : ICompilerPass
{
    public void Execute(CompilerContext context)
    {
        using var writer = File.CreateText(Path.Combine(context.OutputDirectory, "library_flags.asm"));
        foreach (var label in context.UsedLibraryLabels.OrderBy(l => l))
            writer.WriteLine($"Flag_{label} = 1");
    }
}
