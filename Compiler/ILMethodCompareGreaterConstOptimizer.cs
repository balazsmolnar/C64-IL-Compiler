using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Linq;
using Compiler.Ops;

namespace Compiler;

class ILMethodCompareGreaterConstOptimizer : ICompilerMethodPass
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
                lines[i].Operation is OpLdConst &&
                lines[i + 1].OpCode == ILOpCode.Cgt && string.IsNullOrEmpty(lines[i + 1].Label))
            {
                int value = (int)lines[i].RawParameter;
                ILOperation newOperation = new ILOperation
                {
                    Operation = new OpArithmetic2("compareGreater_const"),
                };
                newOperation.RawParameter = lines[i].RawParameter;
                newOperation.StackContent = lines[i + 1].StackContent;
                lines.Insert(i + 2, newOperation);
                lines[i].Optimized = true;
                lines[i + 1].Optimized = true;
            }
        }
    }
}