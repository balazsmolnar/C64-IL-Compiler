using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Linq;
using Compiler.Ops;

namespace Compiler
{
    class ILPropertyGettterOptimizer : ICompilerMethodPass
    {
        // IL_0001: ldarg.0
        // IL_0002: ldfld uint32 Player::x_
        // IL_0007: stloc.0
        // // (no C# code)
        // IL_0008: br.s IL_000a
        // IL_000a: ldloc.0
        public void Execute(CompilerMethodContext context)
        {
            if (!context.TypeContext.CompilerContext.Optimize)
                return;

            var lines = context.Lines;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Operation is OpLdarg &&
                    lines[i + 1].Operation is OpLdfld &&
                    lines[i + 2].Operation is OpStloc &&
                    lines[i + 3].OpCode == ILOpCode.Br_s &&
                    lines[i + 4].Operation is OpLdloc)
                {
                    ILOperation newOperation = new ILOperation
                    {
                        Operation = new OpPushFld(lines[i].RawParameter.ToString(), lines[i + 1].RawParameter.ToString()),
                    };
                    newOperation.RawParameter = newOperation.Operation.ConvertParameter(context, null);
                    lines.Insert(i + 5, newOperation);
                    lines[i].Optimized = true;
                    lines[i + 1].Optimized = true;
                    lines[i + 2].Optimized = true;
                    lines[i + 3].Optimized = true;
                    lines[i + 4].Optimized = true;
                }
                else if (lines[i].OpCode == ILOpCode.Ldarg_0 &&
                  lines[i + 1].Operation is OpLdfld)
                {
                    ILOperation newOperation = new ILOperation
                    {
                        Operation = new OpPushFld(lines[i].RawParameter.ToString(), lines[i + 1].RawParameter.ToString()),
                    };
                    newOperation.RawParameter = newOperation.Operation.ConvertParameter(context, null);
                    lines.Insert(i + 2, newOperation);
                    lines[i].Optimized = true;
                    lines[i + 1].Optimized = true;

                }

            }
        }
    }
}
