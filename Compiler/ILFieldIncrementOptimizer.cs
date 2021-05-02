using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Linq;
using Compiler.Ops;

namespace Compiler
{
    class ILFieldIncrementOptimizer : ICompilerMethodPass
    {
        // IL_00a8: ldarg.0
        // // this.data_++;
        // IL_00a9: ldarg.0
        // IL_00aa: ldfld int32 Player::data_
        // IL_00af: ldc.i4.1
        // IL_00b0: add
        // // (no C# code)
        // IL_00b1: stfld int32 Player::data_
        public void Execute(CompilerMethodContext context)
        {
            if (!context.TypeContext.CompilerContext.Optimize)
                return;

            var lines = context.Lines;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Operation is OpLdarg &&
                    lines[i + 1].Operation is OpLdarg &&
                    lines[i + 2].Operation is OpLdfld &&
                    lines[i + 3].OpCode == ILOpCode.Ldc_i4_1 &&
                    lines[i + 4].OpCode == ILOpCode.Add &&
                    lines[i + 5].Operation is OpStfld)
                {
                    ILOperation newOperation = new ILOperation
                    {
                        Operation = new OpIncfld(lines[i].RawParameter.ToString(), lines[i + 2].RawParameter.ToString()),
                    };
                    newOperation.RawParameter = newOperation.Operation.ConvertParameter(context, null);
                    lines.Insert(i + 6, newOperation);
                    lines[i].Optimized = true;
                    lines[i + 1].Optimized = true;
                    lines[i + 2].Optimized = true;
                    lines[i + 3].Optimized = true;
                    lines[i + 4].Optimized = true;
                    lines[i + 5].Optimized = true;
                }
            }
        }
    }
}
