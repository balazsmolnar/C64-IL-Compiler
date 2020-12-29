using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Linq;
using Compiler.Ops;

namespace Compiler
{
    // IL_0032: ldloc.0
    // IL_0033: ldc.i4.s 10
    // IL_0035: clt
    // IL_0037: stloc.1
    // IL_0038: ldloc.1
    // IL_0039: brtrue.s IL_0021
    class ILMethodBranchIfLessOptimizer : ICompilerMethodPass
    {
        public void Execute(CompilerMethodContext context)
        {

            var lines = context.Lines;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Operand is OpLdloc &&
                    lines[i + 1].Operand is OpLdConst &&
                    lines[i + 2].OpCode == ILOpCode.Clt &&
                    lines[i + 3].Operand is OpStloc &&
                    lines[i + 4].Operand is OpLdloc &&
                    (lines[i + 5].OpCode == ILOpCode.Brtrue || lines[i + 5].OpCode == ILOpCode.Brtrue_s))
                {
                    int variable = ((OpLdloc)lines[i].Operand).VarIndex;
                    int value = (int)lines[i + 1].Parameter;
                    string label = (string)lines[i + 5].Parameter;
                    ILLine newLine = new ILLine
                    {
                        Operand = new OpBranchIfVarLess(variable, value, label),
                    };
                    newLine.Parameter = newLine.Operand.ConvertParameter(context, variable);
                    lines.Insert(i + 6, newLine);

                    for (int j = i; j < i + 6; j++)
                        lines[j].Optimized = true;
                }
            }
        }
    }
}
