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
    class ILMethodParameterInitPass: ICompilerPass
    {
        public void Execute(CompilerContext context)
        {
            var parameters = context.Method.GetParameters();
            foreach (var parameter in parameters)
            {
                string outputLine = $".{context.Method.GetLabel()}_{parameter.Name} !byte 0,0"; 
                context.OutputFile.WriteLine(outputLine);
            }
        }
    }    
}
