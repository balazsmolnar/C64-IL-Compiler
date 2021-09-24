using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Compiler
{
    class ILTypeVTablePass : ICompilerTypePass
    {
        public void Execute(CompilerTypeContext context)
        {
            var @type = context.Type;
            if (@type.IsValueType)
                return;

            context.OutputFile.WriteLine();
            List<string> labels = new();

            foreach (var m in @type.VirtualMethods())
            {
                labels.Add(m.GetLabel());
            }

            string outputLine = $"{context.Type.Name}_VTable: ";
            if (labels.Count > 0)
                outputLine += $" .word {string.Join(',', labels)}";
            else
                outputLine += $" .byte 0";

            context.OutputFile.WriteLine(outputLine);
        }
    }
}
