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
    class ILMethodVariableInitPass : ICompilerMethodPass
    {
        public void Execute(CompilerMethodContext context)
        {
            var body = context.Method.GetMethodBody();
            var variables = body.LocalVariables;

            context.CompilerContext.OutputFile.WriteLine($".{context.Method.GetLabel()}_ReturnAddress !byte 0,0");
            foreach (var variable in variables)
            {
                string outputLine = $".{context.Method.GetLabel()}_var{variable.LocalIndex} !byte 0";
                context.CompilerContext.OutputFile.WriteLine(outputLine);
            }
        }
    }
}
