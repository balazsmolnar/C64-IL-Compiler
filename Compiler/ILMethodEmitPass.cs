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

namespace Compiler
{
    class ILMethodEmitPass: ICompilerPass
    {
        public void Execute(CompilerContext context)
        {
            context.OutputFile.WriteLine("");
            context.OutputFile.WriteLine("");
            context.OutputFile.WriteLine($"{context.Method.GetLabel()} ");

            context.OutputFile.WriteLine($"+stack_save_return_adress .{context.Method.GetLabel()}_ReturnAddress");

            foreach (var param in context.Method.GetParameters())
            {
                string outputLine = $"+stack_pull_int .{context.Method.GetLabel()}_{param.Name}"; 
                context.OutputFile.WriteLine(outputLine);
            }

            foreach (var line in context.Lines)
            {
                string outputLine = $"{(line.Label == null ? "" : line.Label+":")}    {CommandMap.Get(line.OpCode).Command} {(line.Parameter?.ToString() ?? "")}"; 
                context.OutputFile.WriteLine(outputLine);
            }

            context.Lines = null;
        }
    }    
}
