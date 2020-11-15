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
    class ILVariableInitPass: ICompilerPass
    {
        public void Execute(CompilerContext context)
        {
            var body = context.Method.GetMethodBody();
            var variables = body.LocalVariables;
            foreach (var variable in variables)
            {

                string outputLine = $".var{variable.LocalIndex} !byte 0,0"; 
                context.OutputFile.WriteLine(outputLine);
            }
        }
    }    
}
