using System;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Emit;
using System.IO;
using System.Collections.Generic;

namespace Compiler;

class Program
{
    static int Main(string[] args)
    {
        return Run(args, Console.Error);
    }

    // Compiles the assembly named by args[0]; 0 on success, 1 when the program
    // uses something unsupported (every problem is written to `error`, one per
    // line, before returning). Separate from Main so tests can call it.
    internal static int Run(string[] args, TextWriter error)
    {
        try
        {
            Compile(args);
            return 0;
        }
        catch (CompilationFailedException e)
        {
            foreach (var diagnostic in e.Diagnostics.Items)
                error.WriteLine(diagnostic.ToString());
            error.WriteLine($"Compilation failed: {e.Diagnostics.Items.Count} error(s).");
            return 1;
        }
    }

    static void Compile(string[] args)
    {
        // Switches (anywhere on the command line; the rest is positional):
        // --no-static-frames / --static-frames[=1|2] set ILStaticFramePass's
        // level: 0 off, 1 static methods with value-type frames only, 2 (the
        // default) also instance methods and reference parameters/locals.
        // C64_STATIC_FRAMES=0/1/2 does the same without touching the .bat
        // files.
        var staticFrames = int.TryParse(Environment.GetEnvironmentVariable("C64_STATIC_FRAMES"), out var level) ? level : 2;
        var deadCodeElimination = true;
        var positional = new List<string>();
        foreach (var arg in args)
        {
            if (arg == "--no-dce")
                deadCodeElimination = false;
            else if (arg == "--no-static-frames")
                staticFrames = 0;
            else if (arg == "--static-frames")
                staticFrames = 2;
            else if (arg.StartsWith("--static-frames="))
                staticFrames = int.Parse(arg.Substring("--static-frames=".Length));
            else
                positional.Add(arg);
        }
        args = positional.ToArray();

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
        MethodBaseExtension.ProgramAssembly = asm;

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
                EmitDebugInfo = emitDebugInfo,
                StaticFramesLevel = staticFrames,
                DeadCodeElimination = deadCodeElimination,
            };
            var passes = new List<ICompilerPass> {
                new ILRawAssemblyPass(),
                // Whole-program dead-code elimination, before every pass
                // that looks at the call graph: they only see live code.
                new ILReachabilityPass(),
                // Compile-time only: rejects storing a run-time string
                // (shared scratch buffer) where it would outlive the next
                // one; ILCodePass throws its diagnostics with its own.
                new ILStringLifetimePass(),
                // Whole-program pre-pass, must run before ILCodePass's own
                // method loop starts: ILMethodInliningPass (inside that loop)
                // needs a COMPLETE per-callee call-site count while still
                // processing the very first caller, and ILCodePass's own
                // method-enumeration order doesn't guarantee every caller is
                // visited before a method it calls -- see
                // ILCallSiteCountPass's own comment.
                new ILCallSiteCountPass(),
                // Whole-program: picks the methods whose parameters/locals
                // get fixed addresses instead of a localsStack frame (see
                // its own comment). Before ILCodePass, which hands each
                // picked method's context its StaticFrameInfo.
                new ILStaticFramePass(),
                new ILCodePass(
                    new ICompilerTypePass[] {
                        new ILTypeStaticFieldInitPass(),
                        new ILTypeVTablePass(),
                    },
                    new ICompilerMethodPass[] {
                        new ILMethodCodePass(),
                        // PROTOTYPE, Phase 1 (see the approved plan): splices a
                        // single-call-site leaf callee's own body directly into
                        // its one Call site, in place of jsr/rts. Must run
                        // before ILMethodPromoteLocalsPass -- a caller that
                        // loses its only Call to inlining can retroactively
                        // qualify as a leaf for that pass too. See
                        // ILMethodInliningPass's own comment for the full
                        // eligibility/splicing story.
                        new ILMethodInliningPass(),
                        // PROTOTYPE: promotes eligible locals of leaf methods into
                        // zero-page bytes -- see ILMethodPromoteLocalsPass and
                        // zp_local0's comment in asm/helper/zeropage.asm. Narrower
                        // and safer than the earlier whole-method "fast locals"
                        // prototype it replaces: never touches parameters, `this`,
                        // or the prologue/epilogue return-address handling.
                        new ILMethodPromoteLocalsPass(),
                        // PROTOTYPE companion to the pass above: promotes
                        // eligible PARAMETERS (never `this`, which is never
                        // actually eligible) of the same qualifying leaf
                        // methods, filling whatever pool slots locals left
                        // over. Must run after ILMethodPromoteLocalsPass --
                        // see that class's own comment for why, and
                        // ILMethodEmitPass for the one-time copy this needs
                        // that locals never did.
                        new ILMethodPromoteParametersPass(),
                        new ILLibraryUsagePass(),
                        new ILMethodLabelPass(),
                        new ILMethodDebugLabelPass(),
                        new ILMethodNextInstructionPass(),
                        new ILMethodBuildEvaluationStackPass(),
                        // Spilled object-initializer temporaries back to the
                        // dup form (needs the stack depths just built).
                        new ILInitializerTempPass(),
                        new ILTrivialConstructorCallPass(),
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
                        // Right after it: folds runs of the #newObjInit's it
                        // just built (each stored into an array slot) into
                        // one table, before any later pass fuses the Dup/Ldc
                        // index pushes around them.
                        new ILObjectArrayInitializerOptimizer(),
                        // Constant string/primitive elements stored right
                        // after a Newarr: one #newArrInit table (before the
                        // index pushes get fused, like the pass above).
                        new ILConstantArrayInitializerOptimizer(),
                        new ILStaticArrayInitializerPass(),
                        new ILMethodSetVariableOptimizer(),
                        new ILMethodBranchIfLessOptimizer(),
                        new ILMethodBranchIfNotEqualOptimizer(),
                        new ILMethodCompareConstOptimizer(),
                        new ILMethodBranchConstOptimizer(),
                        new ILMethodMulConstOptimizer(),
                        // Same reasoning, same ordering requirement as
                        // ILMethodMulConstOptimizer immediately above: its
                        // own second rule matches a 3-line Ldc-Conv_u8-Div_un
                        // window, which must get first claim before
                        // ILMethodWiden8To16Optimizer's broader "[any
                        // 1-byte-producing op]; Conv_u8/Conv_i8" rule below
                        // can also match that same Ldc-Conv_u8 prefix.
                        new ILMethodDivConstOptimizer(),
                        // Last: broadly matches "[any 1-byte-producing op];
                        // Conv_u8/Conv_i8", so it must run after
                        // ILMethodMulConstOptimizer's/ILMethodDivConstOptimizer's
                        // own, more specific Ldc-Conv-Mul/Div rules get first
                        // claim on any window they would otherwise also
                        // match (moot in practice -- see
                        // ILMethodWiden8To16Optimizer's own comment for why
                        // a constant producer never reaches this rule at
                        // all -- but the ordering is the correct, safe
                        // default regardless).
                        new ILMethodWiden8To16Optimizer(),
                        // Last of all: needs to see every OpPushFld
                        // ILPropertyGettterOptimizer's own rules in this
                        // same sweep already fused, so it can collapse a
                        // chain of them (plus any OpPushFldAt
                        // ILMethodInliningPass built earlier) that read
                        // the same object back to back -- see its own
                        // comment for why it's not a PeepholeRule.
                        new ILMethodCachedFieldAccessPass(),
                        // Also wants ILPropertyGettterOptimizer's/
                        // ILMethodCachedFieldAccessPass's fused field reads
                        // already in place (a raw, unfused Ldfld isn't
                        // matched -- see its own comment), so it runs
                        // after both; this whole optimizer group runs to a
                        // fixpoint (ILCodePass) regardless, so this mostly
                        // affects clarity, not correctness.
                        new ILMethodFloatOperandFusionOptimizer(),
                        // Int counterpart of the float pass immediately
                        // above -- same ordering reasoning (wants fused
                        // field reads already in place), independent of
                        // it otherwise (disjoint operand types).
                        new ILMethodIntOperandFusionOptimizer(),
                        // Last: a static array's element access with a
                        // simple index, once every other pass has fused
                        // what it can of the value being stored.
                        new ILMethodStaticArrayAccessOptimizer(),
                    },
                    new ICompilerMethodPass[] {
                        // After every optimizer, before emit: direct
                        // argument/result copies at static-frame call sites.
                        new ILStaticCallPass(),
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