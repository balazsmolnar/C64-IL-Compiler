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
            if (!context.TypeContext.CompilerContext.Optimize)
                return;

            var lines = context.Lines;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Operation is OpLdConst &&
                    lines[i + 1].Operation is OpStloc && string.IsNullOrEmpty(lines[i + 1].Label))
                {
                    int variable = ((OpStloc)lines[i + 1].Operation).VarIndex;
                    int value = (int)lines[i].RawParameter;
                    ILOperation newOperation = new ILOperation
                    {
                        Operation = new OpInitVar(variable, value),
                    };
                    newOperation.RawParameter = newOperation.Operation.ConvertParameter(context, variable);
                    lines.Insert(i + 2, newOperation);
                    lines[i].Optimized = true;
                    lines[i + 1].Optimized = true;
                }
            }
        }
    }
}
