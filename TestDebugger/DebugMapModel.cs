using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestDebugger;

class SourceLineEntry
{
    public string MethodLabel;
    public string Label;
    public string SourceFile;
    public int Line;
    public int Address;
}

class LocalEntry
{
    public string MethodLabel;
    public int Index;
    public string Name;
}

// Loads debugmap.txt (symbolic labels, written by Compiler's ILDebugMapPass)
// and <prgName>.labels (symbolic label -> real address, written by 64tass)
// and joins them into an address-resolved model the rest of TestDebugger
// works against.
class DebugMapModel
{
    public List<SourceLineEntry> Lines { get; } = new();
    public List<LocalEntry> Locals { get; } = new();
    private Dictionary<string, int> _labelAddresses = new();

    public static DebugMapModel Load(string debugMapPath, string labelsPath)
    {
        var model = new DebugMapModel();
        model._labelAddresses = LoadLabels(labelsPath);

        foreach (var rawLine in File.ReadAllLines(debugMapPath))
        {
            if (rawLine.Length == 0)
                continue;
            var parts = rawLine.Split(' ', 5);
            if (parts[0] == "LINE")
            {
                // LINE <methodLabel> <label> <sourceFile> <line>
                var label = parts[2];
                if (!model._labelAddresses.TryGetValue(label, out var address))
                    continue; // optimized-away/unreachable label -- shouldn't normally happen
                model.Lines.Add(new SourceLineEntry
                {
                    MethodLabel = parts[1],
                    Label = label,
                    SourceFile = parts[3],
                    Line = int.Parse(parts[4]),
                    Address = address
                });
            }
            else if (parts[0] == "LOCAL")
            {
                // LOCAL <methodLabel> <slotIndex> <name>
                model.Locals.Add(new LocalEntry
                {
                    MethodLabel = parts[1],
                    Index = int.Parse(parts[2]),
                    Name = parts[3]
                });
            }
        }

        return model;
    }

    private static Dictionary<string, int> LoadLabels(string labelsPath)
    {
        var result = new Dictionary<string, int>();
        foreach (var line in File.ReadAllLines(labelsPath))
        {
            var parts = line.Split(' ');
            if (parts.Length < 3)
                continue;
            // "al <hex address> .<label>" -- last-one-wins on a collision,
            // same defensive choice RunInEmulatorAspect.GetMethodAddress
            // already makes for this same file.
            result[parts[2].TrimStart('.')] = Convert.ToInt32(parts[1], 16);
        }
        return result;
    }

    public int ResolveLabelAddress(string label) => _labelAddresses[label];

    public bool TryResolveMethodAddress(string methodLabel, out int address) =>
        _labelAddresses.TryGetValue(methodLabel, out address);

    public IEnumerable<int> AllSequencePointAddresses() => Lines.Select(l => l.Address);

    public SourceLineEntry FindByAddress(int address) =>
        Lines.FirstOrDefault(l => l.Address == address);

    public IEnumerable<LocalEntry> LocalsForMethod(string methodLabel) =>
        Locals.Where(l => l.MethodLabel == methodLabel);
}
