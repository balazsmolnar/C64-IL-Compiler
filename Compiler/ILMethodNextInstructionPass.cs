using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Linq;

namespace Compiler;

class ILMethodNextInstructionPass : ICompilerMethodPass
{
    public void Execute(CompilerMethodContext context)
    {
        var lines = context.Lines;

        for (int i = 0; i < lines.Count; i++)
        {
            lines[i].Operation.SetNextInstructions(context, lines[i], i < lines.Count - 1 ? lines[i + 1] : null);
        }

        foreach (var line in lines)
        {
            foreach (var nextInstruction in line.NextInstructions)
            {
                if (!nextInstruction.PreviousInstructions.Contains(line))
                    nextInstruction.PreviousInstructions.Add(line);
            }
        }

    }
}