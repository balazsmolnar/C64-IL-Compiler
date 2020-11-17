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
            context.OutputFile.WriteLine($"{context.Method.GetLabel()} ");
            foreach (var line in context.Lines)
            {
                string outputLine = $"{(line.Label == null ? "" : line.Label+":")}    {CommandMap.Get(line.OpCode)} {(line.Parameter?.ToString() ?? "")}"; 
                context.OutputFile.WriteLine(outputLine);
            }

            context.Lines = null;
        }
    }    
}
