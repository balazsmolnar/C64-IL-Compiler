using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Linq;
using Compiler.Ops;

namespace Compiler
{
    class ILMethodSetVariableOptimizer : ICompilerMethodPass
    {
        public void Execute(CompilerMethodContext context)
        {

            var lines = context.Lines;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Operand is OpLdConst &&
                    lines[i + 1].Operand is OpStloc && string.IsNullOrEmpty(lines[i + 1].Label))
                {
                    int variable = ((OpStloc)lines[i + 1].Operand).VarIndex;
                    int value = (int)lines[i].Parameter;
                    ILLine newLine = new ILLine
                    {
                        Operand = new OpInitVar(variable, value),
                    };
                    newLine.Parameter = newLine.Operand.ConvertParameter(context, variable);
                    lines.Insert(i + 2, newLine);
                    lines[i].Optimized = true;
                    lines[i + 1].Optimized = true;
                }
            }
        }
    }
}
