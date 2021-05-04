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
            if (!context.TypeContext.CompilerContext.Optimize)
                return;

            var lines = context.Lines;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Operation is OpLdloc &&
                    lines[i + 1].Operation is OpLdConst &&
                    (lines[i + 2].OpCode == ILOpCode.Clt || lines[i + 2].OpCode == ILOpCode.Clt_un) &&
                    lines[i + 3].Operation is OpStloc &&
                    lines[i + 4].Operation is OpLdloc &&
                    (lines[i + 5].OpCode == ILOpCode.Brtrue || lines[i + 5].OpCode == ILOpCode.Brtrue_s))
                {
                    int variable = ((OpLdloc)lines[i].Operation).VarIndex;
                    int value = (int)lines[i + 1].RawParameter;
                    string label = (string)lines[i + 5].RawParameter;
                    ILOperation newOperation = new ILOperation
                    {
                        Operation = new OpBranchIfVarLess(variable, value, label),
                    };
                    newOperation.RawParameter = newOperation.Operation.ConvertParameter(context, null);
                    newOperation.StackContent = lines[i + 5].StackContent;
                    lines.Insert(i + 6, newOperation);

                    for (int j = i; j < i + 6; j++)
                        lines[j].Optimized = true;
                }
            }
        }
    }
}
