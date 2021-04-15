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
                if (lines[i].Operation is OpLdstr &&
                    lines[i + 1].Operation is OpCall && lines[i + 1].RawParameter.ToString() == typeof(C64Lib.C64Address).GetMethod("FromLabel").GetLabel())
                {
                    string label = lines[i].RawParameter.ToString();
                    ILOperation newOperation = new ILOperation
                    {
                        Operation = new OpLoadPointerFromLabel(),
                    };
                    // hack
                    var p = ((string)lines[i].RawParameter).Split('_')[1];
                    newOperation.RawParameter = context.CompilerContext.StringValues[int.Parse(p)];
                    lines.Insert(i + 2, newOperation);
                    lines[i].Optimized = true;
                    lines[i + 1].Optimized = true;
                }
            }
        }
    }
}
