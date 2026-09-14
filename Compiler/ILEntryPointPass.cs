using System.IO;
using System.Linq;
using System.Reflection;

namespace Compiler;

// Writes the top-level entry .asm file (e.g. asm/hunchback.asm) that used
// to be four hand-maintained, near-duplicate files differing only in which
// output folder's library_flags.asm/generated.asm they .include -- a
// folder name this pass already knows (context.OutputDirectory) the same
// way ILRawAssemblyPass/ILLibraryFlagsPass do. Two shapes, each an
// embedded template under Templates/ (same embedding convention as
// Hunchback/data.cs's RawAssembly resources): a normal program entry
// point (#start_at + jsr Program_Main), or the unittest.asm test harness
// (Run_Test/Assert_* -- no BASIC stub, invoked directly at $1000 by
// SimpleEmulator instead of loaded via BASIC's SYS).
class ILEntryPointPass : ICompilerPass
{
    public void Execute(CompilerContext context)
    {
        if (string.IsNullOrEmpty(context.EntryFilePath))
            return;

        var folder = Path.GetFileName(context.OutputDirectory.TrimEnd('/', '\\'));
        var templateName = context.IsUnitTest ? "UnitTestEntry" : "ProgramEntry";

        string template;
        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Compiler.Templates.{templateName}.asm"))
        using (var reader = new StreamReader(stream))
        {
            template = reader.ReadToEnd();
        }

        var staticCtorCalls = string.Join("\n", context.StaticConstructorLabels.Select(l => $"jsr {l}"));

        File.WriteAllText(context.EntryFilePath, template
            .Replace("{{FOLDER}}", folder)
            .Replace("{{STATIC_CTORS}}", staticCtorCalls));
    }
}
