using System.Threading;
using System.Globalization;
using System.Reflection;
using System.Security.AccessControl;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Linq;
using System.Text;

namespace Compiler
{
    class ILMethodJumpTablePass : ICompilerMethodPass
    {
        public void Execute(CompilerMethodContext context)
        {
            foreach (var line in context.Lines)
            {
                if (line.OpCode == ILOpCode.Switch)
                {
                    var name = $"{context.Method.GetLabel()}_Jump_{line.Position}";
                    var sb = new StringBuilder();
                    sb.Append(name);
                    sb.Append(" .word ");

                    List<int> parameters = (List<int>)line.RawParameter;
                    foreach (var parameter in parameters)
                    {
                        var target = parameter + line.Position + parameters.Count * 4 + 5;
                        var label = $"{context.Method.GetLabel()}_{target}";
                        sb.Append($" {label}-1, ");
                    }
                    sb.Append("0");
                    context.TypeContext.OutputFile.WriteLine(sb.ToString());
                }
            }
        }
    }
}
