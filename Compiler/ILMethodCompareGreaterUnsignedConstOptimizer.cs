using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Linq;
using Compiler.Ops;

namespace Compiler
{
    class ILMethodCompareGreaterUnsignedConstOptimizer : ICompilerMethodPass
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
                    lines[i + 1].OpCode == ILOpCode.Cgt_un && string.IsNullOrEmpty(lines[i + 1].Label))
                {
                    int value = (int)lines[i].Parameter;
                    ILLine newLine = new ILLine
                    {
                        Operand = new OpArithmetic2("compareGreater_unsigned8_const"),
                    };
                    newLine.Parameter = lines[i].Parameter;
                    lines.Insert(i + 2, newLine);
                    lines[i].Optimized = true;
                    lines[i + 1].Optimized = true;
                }
            }
        }
    }
}
