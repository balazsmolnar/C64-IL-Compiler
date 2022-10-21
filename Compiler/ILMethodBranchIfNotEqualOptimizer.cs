using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Linq;
using Compiler.Ops;

namespace Compiler;

// if (x == 0)
// IL_0054: ldloc.0
// IL_0055: ldc.i4.0
// IL_0056: ceq
// IL_0058: stloc.s 7
// IL_005a: ldloc.s 7
// IL_005c: brfalse.s IL_0060
class ILMethodBranchIfNotEqualOptimizer : ICompilerMethodPass
{
    public void Execute(CompilerMethodContext context)
    {
        if (!context.TypeContext.CompilerContext.Optimize)
            return;

        var lines = context.Lines;
        for (int i = 0; i < lines.Count; i++)
        {
            if ((lines[i].Operation is OpLdloc || lines[i].Operation is OpLdloc_s) &&
                lines[i + 1].Operation is OpLdConst &&
                lines[i + 2].OpCode == ILOpCode.Ceq &&
                (lines[i + 3].Operation is OpStloc || lines[i + 3].Operation is OpStloc_s) &&
                (lines[i + 4].Operation is OpLdloc || lines[i + 4].Operation is OpLdloc_s) &&
                (lines[i + 5].OpCode == ILOpCode.Brfalse || lines[i + 5].OpCode == ILOpCode.Brfalse_s))
            {
                int variable = ((OpLdloc)lines[i].Operation).VarIndex;
                int value = (int)lines[i + 1].RawParameter;
                string label = (string)lines[i + 5].RawParameter;
                ILOperation newOperation = new ILOperation
                {
                    Operation = new OpBranchIfNotEqual(variable, value, label),
                };
                newOperation.RawParameter = newOperation.Operation.ConvertParameter(context, null);
                newOperation.StackContent = lines[i + 5].StackContent;
                lines.Insert(i + 6, newOperation);

                for (int j = i; j < i + 6; j++)
                    lines[j].Optimized = true;
            }
        }
    }
}