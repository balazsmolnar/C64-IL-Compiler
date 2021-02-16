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

            var lines = context.Lines;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Operand is OpLdarg &&
                    lines[i + 1].Operand is OpLdarg &&
                    lines[i + 2].Operand is OpStfld)
                {
                    ILLine newLine = new ILLine
                    {
                        Operand = new OpSetfld(lines[i].Parameter.ToString(), lines[i + 1].Parameter.ToString(), lines[i + 2].Parameter.ToString()),
                    };
                    newLine.Parameter = newLine.Operand.ConvertParameter(context, 0);
                    lines.Insert(i + 3, newLine);
                    lines[i].Optimized = true;
                    lines[i + 1].Optimized = true;
                    lines[i + 2].Optimized = true;
                }
            }
        }
    }
}
