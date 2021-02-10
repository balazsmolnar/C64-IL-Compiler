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

            var lines = context.Lines;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Operand is OpLdarg &&
                    lines[i + 1].Operand is OpLdfld &&
                    lines[i + 2].Operand is OpStloc &&
                    lines[i + 3].OpCode == ILOpCode.Br_s &&
                    lines[i + 4].Operand is OpLdloc)
                {
                    ILLine newLine = new ILLine
                    {
                        Operand = new OpPushFld(lines[i].Parameter.ToString(), lines[i + 1].Parameter.ToString()),
                    };
                    newLine.Parameter = newLine.Operand.ConvertParameter(context, 0);
                    lines.Insert(i + 5, newLine);
                    lines[i].Optimized = true;
                    lines[i + 1].Optimized = true;
                    lines[i + 2].Optimized = true;
                    lines[i + 3].Optimized = true;
                    lines[i + 4].Optimized = true;
                }
                else if (lines[i].OpCode == ILOpCode.Ldarg_0 &&
                  lines[i + 1].Operand is OpLdfld)
                {
                    ILLine newLine = new ILLine
                    {
                        Operand = new OpPushFld(lines[i].Parameter.ToString(), lines[i + 1].Parameter.ToString()),
                    };
                    newLine.Parameter = newLine.Operand.ConvertParameter(context, 0);
                    lines.Insert(i + 2, newLine);
                    lines[i].Optimized = true;
                    lines[i + 1].Optimized = true;

                }

            }
        }
    }
}
