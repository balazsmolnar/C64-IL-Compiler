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
            if (args.Length < 1)
                throw new InvalidOperationException("Assembly must be specified.");

            if (args.Length < 2)
                throw new InvalidOperationException("Output folder must be specified.");

            var asmLocation = args[0];
            var output = args[1];
            var asm = Assembly.LoadFrom(asmLocation);

            using (var outputFile = File.CreateText(Path.Combine(output, "generated.asm")))
            {
                var context = new CompilerContext
                {
                    Assembly = asm,
                    GlobalOutputFile = outputFile,
                    OutputDirectory = output
                };
                var passes = new List<ICompilerPass> {
                    new ILCodePass(
                        new ICompilerTypePass[] {
                            new ILTypeStaticFieldInitPass()
                        },
                        new ICompilerMethodPass[] {
                            new ILMethodCodePass(),
                            new ILMethodLabelPass(),
                            new ILMethodIncOptimizer(),
                            new ILPropertyGettterOptimizer(),
                            // new ILFieldIncrementOptimizer(),
                            new ILMethodSetVariableOptimizer(),
                            // new ILMethodBranchIfLessOptimizer(),
                            // new ILMethodBranchIfNotEqualOptimizer(),
                            new ILMethodEmitPass()}),
                     new ILStringResourcesPass() };
                passes.ForEach(p => p.Execute(context));
            }
        }
    }
}
