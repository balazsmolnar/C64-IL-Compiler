using System;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Emit;
using System.IO;
using System.Collections.Generic;

namespace Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            var asmLocation = @"c:\Balazs\C64\Demo\bin\Debug\Demo.dll";
            var asm = Assembly.LoadFrom(asmLocation);

            using (var outputFile = File.CreateText(@"C:\Balazs\C64\asm\generated.asm"))
            {
                var context = new CompilerContext {
                    Assembly = asm,
                    OutputFile = outputFile
                };
                var passes = new List<ICompilerPass> {
                    new ILCodePass(
                        new ICompilerMethodPass[] {
                            new ILMethodCodePass(), 
                            new ILMethodLabelPass(), 
                            new ILMethodEmitPass(), 
                            new ILMethodParameterInitPass(),
                            new ILMethodVariableInitPass()}), 
                     new ILStringResourcesPass() };
                passes.ForEach(p => p.Execute(context));
            }            
        }
    }
}
