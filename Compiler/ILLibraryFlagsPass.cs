using System.IO;

namespace Compiler;

// Writes library_flags.asm -- USE_JOYSTICK/USE_SOUND/USE_DEBUG equates that
// asm/C64.asm's `.if` guards use to skip assembling C64Joystick.asm/
// C64sound.asm/C64Debug.asm when a program never calls into them (see
// ILLibraryUsagePass, which records the findings this pass reads). Must run
// after ILCodePass has processed every method, and its output must be
// .include'd before C64.asm in every entry asm file (main.asm/hunchback.asm/
// presentation.asm/unittest.asm), since 64tass evaluates `.if` in source
// order -- the flags have to already be defined by the time C64.asm's
// guards are reached.
class ILLibraryFlagsPass : ICompilerPass
{
    public void Execute(CompilerContext context)
    {
        using var writer = File.CreateText(Path.Combine(context.OutputDirectory, "library_flags.asm"));
        writer.WriteLine($"USE_JOYSTICK = {(context.UsesJoystick ? 1 : 0)}");
        writer.WriteLine($"USE_SOUND = {(context.UsesSound ? 1 : 0)}");
        writer.WriteLine($"USE_DEBUG = {(context.UsesDebug ? 1 : 0)}");
    }
}
