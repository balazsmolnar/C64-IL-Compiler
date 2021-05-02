using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Linq;
using Compiler.Ops;

namespace Compiler
{
    class ILMethodIncOptimizer : ICompilerMethodPass
    {
        public void Execute(CompilerMethodContext context)
        {
            if (!context.TypeContext.CompilerContext.Optimize)
                return;

            var lines = context.Lines;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Operation is OpLdloc &&
                    lines[i + 1].OpCode == ILOpCode.Ldc_i4_1 &&
                    lines[i + 2].OpCode == ILOpCode.Add &&
                    lines[i + 3].Operation is OpStloc)
                {
                    int variable = ((OpStloc)lines[i + 3].Operation).VarIndex;
                    ILOperation newOperation = new ILOperation
                    {
                        Operation = new OpIncVar(variable),
                    };
                    newOperation.RawParameter = newOperation.Operation.ConvertParameter(context, null);
                    lines.Insert(i + 4, newOperation);
                    lines[i].Optimized = true;
                    lines[i + 1].Optimized = true;
                    lines[i + 2].Optimized = true;
                    lines[i + 3].Optimized = true;
                }
            }
        }
    }
}
