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
    class ILMethodEmitPass : ICompilerMethodPass
    {
        public void Execute(CompilerMethodContext context)
        {
            var output = context.TypeContext.OutputFile;
            output.WriteLine("");
            output.WriteLine("");
            output.WriteLine(";----------------------------------------");
            output.WriteLine($"; TYPE: {context.Method.ReflectedType.FullName}");
            output.WriteLine($"; METHOD: {context.Method.Name}");
            output.WriteLine(";----------------------------------------");
            output.WriteLine($"{context.Method.GetLabel()} ");

            if (context.Method.IsAbstract)
            {
                output.WriteLine("    brk");
                return;
            }

            var ref_params = new List<string>();
            foreach (var param in context.Method.GetParameters().Reverse())
            {
                if (param.ParameterType.GetStorageBytes() == 2)
                {
                    ref_params.Add(param.ParameterType.IsReferenceCounted() ? "1" : "0, 0");
                }
                else
                {
                    ref_params.Add(param.ParameterType.IsReferenceCounted() ? "1" : "0");
                }
            }
            if (!context.Method.IsStatic)
            {
                ref_params.Add("0");
            }

            string outputLine = $"    #init_locals_pull_parameters {context.GetLocalsSize()}, [{string.Join(',', ref_params)}]";
            output.WriteLine(outputLine);

            foreach (var line in context.Lines)
            {
                outputLine = $"{(line.Label == null ? "" : line.Label + ":")}  {(line.Optimized ? "; OPT " : "")}  {line.Operation.Emit(context, line)} ; {line.OpCode}";
                output.WriteLine(outputLine);

            }
        }
    }
}
