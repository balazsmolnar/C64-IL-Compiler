using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Linq;
using Compiler.Ops;

namespace Compiler
{
    // if (x == 0)
    // IL_0054: ldloc.0
    // IL_0055: ldc.i4.0
    // IL_0056: ceq
    // IL_0058: stloc.s 7
    // IL_005a: ldloc.s 7
    // IL_005c: brfalse.s IL_0060
    class ILMethodBranchIfNotEqualOptimizer : ICompilerMethodPass
    {
        public void Execute(CompilerMethodContext context)
        {
            if (!context.TypeContext.CompilerContext.Optimize)
                return;

            var lines = context.Lines;
            for (int i = 0; i < lines.Count; i++)
            {
                if ((lines[i].Operand is OpLdloc || lines[i].Operand is OpLdloc_s) &&
                    lines[i + 1].Operand is OpLdConst &&
                    lines[i + 2].OpCode == ILOpCode.Ceq &&
                    (lines[i + 3].Operand is OpStloc || lines[i + 3].Operand is OpStloc_s) &&
                    (lines[i + 4].Operand is OpLdloc || lines[i + 4].Operand is OpLdloc_s) &&
                    (lines[i + 5].OpCode == ILOpCode.Brfalse || lines[i + 5].OpCode == ILOpCode.Brfalse_s))
                {
                    int variable = ((OpLdloc)lines[i].Operand).VarIndex;
                    int value = (int)lines[i + 1].Parameter;
                    string label = (string)lines[i + 5].Parameter;
                    ILLine newLine = new ILLine
                    {
                        Operand = new OpBranchIfNotEqual(variable, value, label),
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
