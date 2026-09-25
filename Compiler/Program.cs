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
        var entryFilePath = args.Length > 2 ? args[2] : null;
        var isUnitTest = args.Length > 3 && args[3] == "unittest";
        var emitDebugInfo = args.Length > 4 && args[4] == "debug";
        var asm = Assembly.LoadFrom(asmLocation);

        using (var outputFile = File.CreateText(Path.Combine(output, "generated.asm")))
        {
            var context = new CompilerContext
            {
                Assembly = asm,
                GlobalOutputFile = outputFile,
                OutputDirectory = output,
                EntryFilePath = entryFilePath,
                IsUnitTest = isUnitTest,
                Optimize = true,
                EmitDebugInfo = emitDebugInfo
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
                        new ILLibraryUsagePass(),
                        new ILMethodLabelPass(),
                        new ILMethodDebugLabelPass(),
                        new ILMethodNextInstructionPass(),
                        new ILMethodBuildEvaluationStackPass(),
                        new ILAddressFromLabelPass(),
                        new ILNumericToStringPass(),
                        new ILStringOpsPass(),
                        new ILStringInterpolationPass(),
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
                        new ILStaticArrayInitializerPass(),
                        new ILMethodSetVariableOptimizer(),
                        new ILMethodBranchIfLessOptimizer(),
                        new ILMethodBranchIfNotEqualOptimizer(),
                        new ILMethodCompareConstOptimizer(),
                        new ILMethodBranchConstOptimizer(),
                        new ILMethodMulConstOptimizer(),
                        // Last: broadly matches "[any 1-byte-producing op];
                        // Conv_u8/Conv_i8", so it must run after
                        // ILMethodMulConstOptimizer's own, more specific
                        // Ldc-Conv-Mul rule gets first claim on any window
                        // it would otherwise also match (moot in practice --
                        // see ILMethodWiden8To16Optimizer's own comment for
                        // why a constant producer never reaches this rule at
                        // all -- but the ordering is the correct, safe
                        // default regardless).
                        new ILMethodWiden8To16Optimizer(),
                    },
                    new ICompilerMethodPass[] {
                        new ILMethodEmitPass(),
                        new ILMethodJumpTablePass()
                    }),
                new ILLibraryFlagsPass(),
                new ILStringResourcesPass(),
                new ILDebugMapPass(),
                new ILEntryPointPass() };
            passes.ForEach(p => p.Execute(context));
        }
    }
}