using System;
using System.Collections.Generic;
using System.Linq;

namespace TestDebugger;

class BreakpointResolver
{
    private readonly DebugMapModel _model;

    public BreakpointResolver(DebugMapModel model)
    {
        _model = model;
    }

    // Resolves "file:line" to the set of addresses that should stop
    // execution. Snaps down to the next executable line in that file if the
    // exact line has no sequence point (e.g. blank line, comment, or a line
    // whose only instruction got optimized into an earlier one). Sets
    // breakpoints on every method that matches (this project's Test/*.cs
    // files are flat with non-overlapping method line ranges, so more than
    // one match isn't expected in practice, but extra unreachable
    // breakpoints are harmless).
    public HashSet<int> Resolve(string spec, out string resolvedFile, out int resolvedLine)
    {
        var colonIndex = spec.LastIndexOf(':');
        if (colonIndex < 0)
            throw new ArgumentException($"Expected file:line, got \"{spec}\"");

        var file = spec.Substring(0, colonIndex);
        if (!int.TryParse(spec.Substring(colonIndex + 1), out var line))
            throw new ArgumentException($"Expected file:line, got \"{spec}\"");

        var candidates = _model.Lines
            .Where(l => string.Equals(l.SourceFile, file, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (candidates.Count == 0)
            throw new ArgumentException($"No debug info for source file \"{file}\"");

        var exact = candidates.Where(l => l.Line == line).ToList();
        List<SourceLineEntry> matches;
        int matchedLine;
        if (exact.Count > 0)
        {
            matches = exact;
            matchedLine = line;
        }
        else
        {
            var nextLine = candidates.Where(l => l.Line > line).Select(l => l.Line).DefaultIfEmpty(-1).Min();
            if (nextLine < 0)
                throw new ArgumentException($"No executable code at or after {file}:{line}");
            matches = candidates.Where(l => l.Line == nextLine).ToList();
            matchedLine = nextLine;
        }

        resolvedFile = file;
        resolvedLine = matchedLine;
        return matches.Select(l => l.Address).ToHashSet();
    }
}
