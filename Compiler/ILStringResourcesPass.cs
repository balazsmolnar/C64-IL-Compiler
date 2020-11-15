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
    class ILStringResourcesPass: ICompilerPass
    {
        public void Execute(CompilerContext context)
        {
            foreach (var stringResource in context.StringValues)
            {

                string outputLine = $".string_{stringResource.Key} !pet \"{stringResource.Value}\",0"; 
                context.OutputFile.WriteLine(outputLine);
            }
        }
    }    
}
