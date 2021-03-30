using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Linq;
using Compiler.Ops;

namespace Compiler
{
    class ILAddressFromLabelPass : ICompilerMethodPass
    {
        public void Execute(CompilerMethodContext context)
        {
            var lines = context.Lines;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Operand is OpLdstr &&
                    lines[i + 1].Operand is OpCall && lines[i + 1].Parameter.ToString() == typeof(C64Lib.C64Address).GetMethod("FromLabel").GetLabel())
                {
                    string label = lines[i].Parameter.ToString();
                    ILLine newLine = new ILLine
                    {
                        Operand = new OpLoadPointerFromLabel(),
                    };
                    // hack
                    var p = ((string)lines[i].Parameter).Split('_')[1];
                    newLine.Parameter = context.CompilerContext.StringValues[int.Parse(p)];
                    lines.Insert(i + 2, newLine);
                    lines[i].Optimized = true;
                    lines[i + 1].Optimized = true;
                }
            }
        }
    }
}
