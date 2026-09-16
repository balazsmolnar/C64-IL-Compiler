using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Compiler;

class ILTypeVTablePass : ICompilerTypePass
{
    public void Execute(CompilerTypeContext context)
    {
        var @type = context.Type;
        if (@type.IsValueType)
            return;

        context.OutputFile.WriteLine();
        List<string> labels = new();

        foreach (var m in @type.GetVirtualMethods())
        {
            labels.Add(m.GetLabel());
        }

        // FullName, not Name -- must match OpNewObj's own vtable reference
        // (Operands/OperandBase.cs) exactly; see ILCodePass.cs's comment
        // for why short names alone collide (Roslyn's synthesized "<>O").
        string outputLine = $"{context.Type.FullName.ToValidName()}_VTable: ";
        if (labels.Count > 0)
            outputLine += $" .word {string.Join(',', labels)}";
        else
            outputLine += $" .byte 0";

        context.OutputFile.WriteLine(outputLine);
    }
}