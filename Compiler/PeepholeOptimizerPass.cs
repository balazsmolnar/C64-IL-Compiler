using System.Collections.Generic;

namespace Compiler;

// Shared skeleton for peephole optimizer passes: the Optimize-flag guard, the
// left-to-right scan, and applying each rule are all handled once here instead
// of being copy-pasted into every optimizer. A subclass just declares its
// Rules. Rules run one at a time, each scanning the whole method left to
// right before the next rule starts -- equivalent to the original single
// interleaved scan for this codebase's rule set, since a matched window is
// always marked Optimized immediately (so no rule can ever re-match it) and a
// freshly-inserted synthesized operation never looks like a raw IL opcode
// sequence another rule is hunting for.
abstract class PeepholeOptimizerPass : ICompilerMethodPass
{
    protected abstract IEnumerable<PeepholeRule> Rules { get; }

    public void Execute(CompilerMethodContext context)
    {
        if (!context.TypeContext.CompilerContext.Optimize)
            return;

        var lines = context.Lines;
        foreach (var rule in Rules)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Optimized)
                    continue;
                rule.TryApply(context, lines, i);
            }
        }
    }
}
