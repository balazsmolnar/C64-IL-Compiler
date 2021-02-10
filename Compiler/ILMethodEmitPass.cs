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
            output.WriteLine($"; TYPE: {context.Method.DeclaringType.FullName}");
            output.WriteLine($"; METHOD: {context.Method.Name}");
            output.WriteLine(";----------------------------------------");
            output.WriteLine($"{context.Method.GetLabel()} ");

            output.WriteLine($"    #locals_init_locals {context.GetLocalsSize()}");

            foreach (var param in context.Method.GetParameters().Reverse())
            {
                var isRef = param.ParameterType.IsReferenceCounted() ? "1" : "0";
                string outputLine = $"    #locals_pull_param_8 {isRef}";
                output.WriteLine(outputLine);
            }
            if (!context.Method.IsStatic)
            {
                string outputLine = $"    #locals_pull_param_8 0";
                output.WriteLine(outputLine);
            }

            foreach (var line in context.Lines)
            {
                string outputLine = $"{(line.Label == null ? "" : line.Label + ":")}  {(line.Optimized ? "; OPT " : "")}  {line.Operand.Emit(context, line.Parameter)} ; {line.OpCode}";
                output.WriteLine(outputLine);
            }
        }
    }
}
