using System.IO;

namespace Compiler;

// Only runs when CompilerContext.EmitDebugInfo is set. Writes what
// ILMethodDebugLabelPass accumulated to a plain-text debugmap.txt next to
// generated.asm -- deliberately not into prg/, since that's 64tass's
// territory and this file is written before assembly even runs. `label` is
// symbolic, not an address (64tass hasn't assigned real addresses at this
// point) -- resolved to an address afterward by reading the .labels file
// 64tass produces, the same way RunInEmulatorAspect.GetMethodAddress already
// resolves method labels.
//
// Format (whitespace-split, one entry per line, matching .labels' own
// convention):
//   LINE <methodLabel> <label> <sourceFile> <line>
//   LOCAL <methodLabel> <slotIndex> <name>
class ILDebugMapPass : ICompilerPass
{
    public void Execute(CompilerContext context)
    {
        if (!context.EmitDebugInfo)
            return;

        using var writer = File.CreateText(Path.Combine(context.OutputDirectory, "debugmap.txt"));
        foreach (var sp in context.DebugSequencePoints)
            writer.WriteLine($"LINE {sp.MethodLabel} {sp.Label} {sp.SourceFile} {sp.Line}");
        foreach (var l in context.DebugLocals)
            writer.WriteLine($"LOCAL {l.MethodLabel} {l.Index} {l.Name}");
    }
}
