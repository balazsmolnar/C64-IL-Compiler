using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Linq;
using Compiler.Ops;

namespace Compiler
{
    class ILMethodBranchConstOptimizer : ICompilerMethodPass
    {
        public void Execute(CompilerMethodContext context)
        {
            if (!context.TypeContext.CompilerContext.Optimize)
                return;

            var lines = context.Lines;
            for (int i = 0; i < lines.Count; i++)
            {
                if (
                    !lines[i].Optimized &&
                    lines[i].Operand is OpLdConst &&
                    lines[i + 1].Operand is OpShortJump && lines[i + 1].OpCode != ILOpCode.Br_s && string.IsNullOrEmpty(lines[i + 1].Label))
                {
                    ILLine newLine = new ILLine
                    {
                        Operand = new OpBranchConst(lines[i + 1].Operand.Command + "_const"),
                    };
                    newLine.Parameter = lines[i].Parameter + ", " + lines[i + 1].Parameter;
                    lines.Insert(i + 2, newLine);
                    lines[i].Optimized = true;
                    lines[i + 1].Optimized = true;
                }
            }
        }
    }
}
