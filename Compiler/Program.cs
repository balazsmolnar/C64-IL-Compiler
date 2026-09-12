using System;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Emit;
using System.IO;
using System.Collections.Generic;

namespace Compiler;

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
                OutputDirectory = output,
                Optimize = true
            };
            var passes = new List<ICompilerPass> {
                new ILRawAssemblyPass(),
                new ILCodePass(
                    new ICompilerTypePass[] {
                        new ILTypeStaticFieldInitPass(),
                        new ILTypeVTablePass(),
                    },
                    new ICompilerMethodPass[] {
                        new ILMethodCodePass(),
                        new ILMethodLabelPass(),
                        new ILMethodNextInstructionPass(),
                        new ILMethodBuildEvaluationStackPass(),
                        new ILAddressFromLabelPass(),
                    },
                    new ICompilerMethodPass[] {
                        new ILMethodIncOptimizer(),
                        new ILMethodDecOptimizer(),
                        // ILFieldIncrementOptimizer's 6-line "this.field++" pattern strictly
                        // contains ILPropertyGettterOptimizer's 2-line "ldarg.0; ldfld" pattern
                        // as a sub-window (the pattern's own ldarg.0+ldfld read of the field).
                        // It must run first, or the property-getter pass matches that
                        // sub-window on its own left-to-right scan before the increment pass
                        // gets a turn, shifting indices and permanently shadowing the longer
                        // match -- which is exactly what was happening (this.field++ silently
                        // never compiled to #incfld, falling back to the slower/larger
                        // #pushfld+#stack_push_int+#add+#stfld sequence instead). Still worth
                        // keeping this order even now that the optimizer set below runs to a
                        // fixpoint (see ILCodePass): the shadowing happens *within* a single
                        // sweep, before either pass has a chance to mark anything Optimized
                        // that the other would respect on a later sweep.
                        new ILFieldIncrementOptimizer(),
                        new ILPropertyGettterOptimizer(),
                        new ILSetFieldOptimizer(),
                        new ILObjectInitializerOptimizer(),
                        new ILMethodSetVariableOptimizer(),
                        new ILMethodBranchIfLessOptimizer(),
                        new ILMethodBranchIfNotEqualOptimizer(),
                        new ILMethodCompareConstOptimizer(),
                        new ILMethodBranchConstOptimizer(),
                    },
                    new ICompilerMethodPass[] {
                        new ILMethodEmitPass(),
                        new ILMethodJumpTablePass()
                    }),
                new ILStringResourcesPass() };
            passes.ForEach(p => p.Execute(context));
        }
    }
}