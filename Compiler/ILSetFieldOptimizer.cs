using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Linq;
using Compiler.Ops;

namespace Compiler
{
    class ILSetFieldOptimizer : ICompilerMethodPass
    {
        // IL_0000: ldarg.0
        // IL_0001: ldarg.1
        // IL_0002: stfld uint32 PlatformEnemy::'<MaxX>k__BackingField'
        public void Execute(CompilerMethodContext context)
        {

            if (!context.TypeContext.CompilerContext.Optimize)
                return;

            var lines = context.Lines;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Operation is OpLdarg &&
                    lines[i + 1].Operation is OpLdarg &&
                    lines[i + 2].Operation is OpStfld)
                {
                    bool is16Bit = lines[i + 2].Operation.Is16Bit(context, lines[i + 2]);
                    ILOperation newOperation = new ILOperation
                    {
                        Operation = new OpSetfld(lines[i].RawParameter.ToString(), lines[i + 1].RawParameter.ToString(), lines[i + 2].RawParameter.ToString(), is16Bit),
                    };
                    newOperation.RawParameter = newOperation.Operation.ConvertParameter(context, null);
                    newOperation.StackContent = lines[i + 2].StackContent;
                    lines.Insert(i + 3, newOperation);
                    lines[i].Optimized = true;
                    lines[i + 1].Optimized = true;
                    lines[i + 2].Optimized = true;
                }
            }
        }
    }
}
