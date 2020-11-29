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
    class ILMethodEmitPass: ICompilerMethodPass
    {
        public void Execute(CompilerMethodContext context)
        {
            var output = context.CompilerContext.OutputFile;
            output.WriteLine("");
            output.WriteLine("");
            output.WriteLine($"{context.Method.GetLabel()} ");

            output.WriteLine($"+stack_save_return_adress .{context.Method.GetLabel()}_ReturnAddress");

            foreach (var param in context.Method.GetParameters())
            {
                string outputLine = $"+stack_pull_int .{context.Method.GetLabel()}_{param.Name}"; 
                output.WriteLine(outputLine);
            }

            foreach (var line in context.Lines)
            {
                var op = CommandMap.Get(line.OpCode);

                string outputLine = $"{(line.Label == null ? "" : line.Label+":")}    {op.Emit(context, line.Parameter)}"; 
                output.WriteLine(outputLine);
            }
        }
    }    
}
