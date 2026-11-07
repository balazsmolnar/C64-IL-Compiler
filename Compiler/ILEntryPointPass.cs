using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Compiler.Ops;

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
            .Replace("{{STATIC_CTORS}}", staticCtorCalls)
            .Replace("{{CALL_MAIN}}", CallMain(context)));
    }

    // The call of the assembly's entry point, whatever C# named it: a
    // declared `static void Main()` is Program_Main, but top-level
    // statements compile to `<Main>$(string[] args)` (label
    // Program_x_Main__), and Main may take `string[] args` or return int.
    // A parameterless void Main -- every program so far -- is a plain
    // `jsr`, exactly as the template used to hard-code it.
    private static string CallMain(CompilerContext context)
    {
        var main = context.Assembly.EntryPoint;
        if (main == null)
            return "jsr Program_Main";

        var sb = new StringBuilder();
        // `args`: there is no command line, so it is null (a body that reads
        // it is rejected, ILMethodCodePass). Pushed like any argument.
        foreach (var parameter in main.GetParameters())
            for (int i = 0; i < parameter.ParameterType.GetStorageBytes(); i++)
                sb.Append("lda #0\npha\n");

        var frame = context.GetStaticFrame(main);
        bool callerStores = frame != null && !frame.CalleePulls;
        if (callerStores)
            sb.Append(OpCall.StaticFramePulls(frame).Replace("\n    ", "\n"));
        sb.Append($"jsr {main.GetLabel()}");

        // An int Main's result: in A/X for a caller-stores static frame,
        // otherwise left on the stack -- drop it, BASIC's SYS gets the rts.
        if (main.ReturnType != typeof(void) && !callerStores)
            for (int i = 0; i < main.ReturnType.GetStorageBytes(); i++)
                sb.Append("\npla");
        return sb.ToString();
    }
}
