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
            var type = asm.GetType("Demo.Program");
            if (type==null)
                throw new Exception("Type not found");
            var method = type.GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic);
            if (method==null)
                throw new Exception("Method not found");
            var body = method.GetMethodBody();
            var ilCode = body.GetILAsByteArray();

            using (var outputFile = File.CreateText(@"C:\Balazs\C64\asm\generated.asm"))
            {
                var context = new CompilerContext {
                    Assembly = asm,
                    Method = method,
                    OutputFile = outputFile
                };
                var passes = new List<ICompilerPass> {
                    new ILCodePass(
                        new ICompilerPass[] {
                            new ILMethodCodePass(), 
                            new ILMethodLabelPass(), 
                            new ILMethodEmitPass(), 
                            new ILMethodVariableInitPass()}), 
                     new ILStringResourcesPass() };
                passes.ForEach(p => p.Execute(context));
            }            
        }
    }
}
