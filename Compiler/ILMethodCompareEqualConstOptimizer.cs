using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Linq;
using Compiler.Ops;

namespace Compiler
{
    class ILMethodCompareEqualConstOptimizer : ICompilerMethodPass
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
                    lines[i + 1].OpCode == ILOpCode.Ceq && string.IsNullOrEmpty(lines[i + 1].Label))
                {
                    int value = (int)lines[i].RawParameter;
                    ILOperation newOperation = new ILOperation
                    {
                        Operation = new OpArithmetic2("compareEqual8_const"),
                    };
                    newOperation.RawParameter = lines[i].RawParameter;
                    lines.Insert(i + 2, newOperation);
                    lines[i].Optimized = true;
                    lines[i + 1].Optimized = true;
                }
            }
        }
    }
}
