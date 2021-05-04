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
                    lines[i].Operation is OpLdConst &&
                    lines[i + 1].Operation is OpShortJump && lines[i + 1].OpCode != ILOpCode.Br_s && string.IsNullOrEmpty(lines[i + 1].Label))
                {
                    ILOperation newOperation = new ILOperation
                    {
                        Operation = new OpBranchConst(lines[i + 1].Operation.Command + "_const"),
                    };
                    newOperation.RawParameter = lines[i].RawParameter + ", " + lines[i + 1].RawParameter;
                    newOperation.StackContent = lines[i + 1].StackContent;
                    lines.Insert(i + 2, newOperation);
                    lines[i].Optimized = true;
                    lines[i + 1].Optimized = true;
                }
            }
        }
    }
}
