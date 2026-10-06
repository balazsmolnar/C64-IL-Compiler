using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Compiler.UnitTests;

// Structural tests for constant-argument specialization
// (ILMethodInliningPass.IsTrivialConstantProducer/ConstantProducerFitsParameter):
// when a trivially-substitutable call site's argument is a bare integer/
// float LITERAL rather than a variable read, the callee's read of that
// parameter is replaced with a direct copy of the literal-push
// instruction, skipping materialization into a fresh locals-stack slot
// the same way the existing Ldarg/Ldloc trivial substitution does.
//
// IMPORTANT, verified (not assumed) finding: this does NOT additionally
// unlock further constant-folding inside the callee's own spliced body
// (e.g. MulConstOptimizer never fires on the specialized read) --
// PeepholeRule.TryApply's blanket SourceMethod-guard rejects a match the
// moment ANY line in its window belongs to a spliced callee, which every
// ordinary (non-Ret, non-parameter-read) line in the splice does,
// regardless of whether one of its operands happens to be a specialized
// constant. See ILMethodInliningPass.IsTrivialConstantProducer's own
// comment for the full story -- this file's own
// Specialized_Constant_Does_Not_Unlock_MulConst_Fusion test exists
// specifically to guard against that claim quietly becoming true (a
// welcome surprise, not a regression) or silently being re-introduced
// as a false assumption in a future change without being re-verified.
[TestFixture]
public class ConstantArgumentSpecializationTests
{
    static int counter;

    static string CompileAndGetAsmDir(string source)
    {
        var dir = Path.Combine(Path.GetTempPath(), "c64_const_arg_spec_tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var name = "ConstArgSpec" + ++counter;
        var dll = Path.Combine(dir, name + ".dll");
        var pdb = Path.Combine(dir, name + ".pdb");

        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
            .Split(Path.PathSeparator)
            .Select(p => MetadataReference.CreateFromFile(p))
            .Append(MetadataReference.CreateFromFile(Path.Combine(AppContext.BaseDirectory, "C64Lib.dll")))
            .ToList();
        var tree = CSharpSyntaxTree.ParseText(source, path: Path.Combine(dir, "snippet.cs"), encoding: System.Text.Encoding.UTF8);
        var compilation = CSharpCompilation.Create(name, new[] { tree }, references,
            new CSharpCompilationOptions(OutputKind.ConsoleApplication, optimizationLevel: OptimizationLevel.Release, allowUnsafe: false));

        using (var dllStream = File.Create(dll))
        using (var pdbStream = File.Create(pdb))
        {
            var emit = compilation.Emit(dllStream, pdbStream, options: new Microsoft.CodeAnalysis.Emit.EmitOptions(debugInformationFormat: Microsoft.CodeAnalysis.Emit.DebugInformationFormat.PortablePdb));
            Assert.That(emit.Success, Is.True, "the test's own C# does not compile:\n" + string.Join("\n", emit.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        }

        var outDir = Path.Combine(dir, "asm");
        Directory.CreateDirectory(outDir);
        var errors = new StringWriter();
        var exit = Program.Run(new[] { dll, outDir, Path.Combine(dir, "entry.asm"), "program" }, errors);
        Assert.That(exit, Is.EqualTo(0), "expected the program to compile cleanly; output:\n" + errors);
        return outDir;
    }

    static string ReadType(string asmDir, string typeName) =>
        File.ReadAllText(Path.Combine(asmDir, typeName + ".asm"));

    const string Using = "using System.Runtime.CompilerServices;\n";

    [Test]
    public void Constant_Argument_Skips_Materialization()
    {
        var source = Using + @"
class Holder
{
    public uint value_;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint AddN(uint n) => value_ + n;
    public uint Use() => AddN(10);
}
class Program
{
    static void Main()
    {
        var h = new Holder();
        h.value_ = 1;
        var r = h.Use();
        C64Lib.C64.FillMemory(0x0900UL, r, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var holderAsm = ReadType(asmDir, "Holder");

        Assert.That(holderAsm, Does.Not.Contain("init_locals_pull_parameters_inline"),
            "a literal argument should skip materializing a new locals-stack slot entirely:\n" + holderAsm);
        Assert.That(holderAsm, Does.Contain("stack_push_int8 10"),
            "the callee's own read of `n` should become a direct copy of the literal push:\n" + holderAsm);
    }

    [Test]
    public void Float_Constant_Argument_Skips_Materialization()
    {
        var source = Using + @"
class Holder
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float AddN(float v, float n) => v + n;
    public float Use(float v) => AddN(v, 2.5f);
}
class Program
{
    static void Main()
    {
        var h = new Holder();
        var r = h.Use(1.0f);
        C64Lib.C64.FillMemory(0x0900UL, (uint)r, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var holderAsm = ReadType(asmDir, "Holder");

        Assert.That(holderAsm, Does.Not.Contain("init_locals_pull_parameters_inline"),
            "a float literal argument should also skip materialization:\n" + holderAsm);
        Assert.That(holderAsm, Does.Contain("stack_push_mflpt_const"),
            "the callee's own read of `n` should become a direct copy of the float literal push:\n" + holderAsm);
    }

    // Guard: a long/ulong parameter receiving a small literal needs a
    // widening Conv_i8/u8 Roslyn always emits -- a TWO-instruction
    // producer, which the shared fixed-offset window this reuses from
    // ProducerRelPos must still reject (same as any other multi-
    // instruction producer), falling back to materialization.
    [Test]
    public void Wide_Typed_Parameter_With_Literal_Argument_Still_Materializes()
    {
        var source = Using + @"
class Holder
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong AddN(ulong v, ulong n) => v + n;
    public ulong Use(ulong v) => AddN(v, 10);
}
class Program
{
    static void Main()
    {
        var h = new Holder();
        var r = h.Use(1);
        C64Lib.C64.FillMemory(0x0900UL, (uint)r, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var holderAsm = ReadType(asmDir, "Holder");

        Assert.That(holderAsm, Does.Contain("init_locals_pull_parameters_inline"),
            "a long/ulong parameter's literal argument needs a widening conversion -- not a bare single-instruction producer -- so it must still materialize:\n" + holderAsm);
    }

    // Verified (not assumed) finding -- see this file's own top comment.
    // A specialized constant feeding a Mul that would otherwise match
    // ILMethodMulConstOptimizer's shift-fusion does NOT get it, because
    // every ordinary line inside the splice (the Mul included) carries
    // SourceMethod != null, which PeepholeRule.TryApply's blanket guard
    // rejects regardless of what feeds it.
    [Test]
    public void Specialized_Constant_Does_Not_Unlock_MulConst_Fusion()
    {
        var source = Using + @"
class Holder
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint MulBy(uint v, uint n) => v * n;
    public uint Use(uint v) => MulBy(v, 4);
}
class Program
{
    static void Main()
    {
        var h = new Holder();
        var r = h.Use(3);
        C64Lib.C64.FillMemory(0x0900UL, r, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var holderAsm = ReadType(asmDir, "Holder");

        // If this starts failing because "mul_shift_const" now DOES
        // appear, that's a welcome improvement, not a regression --
        // update this test (and the comment on IsTrivialConstantProducer
        // claiming it doesn't happen) to match reality, don't just widen
        // the assertion to let it through unnoticed.
        Assert.That(holderAsm, Does.Not.Contain("mul_shift_const"),
            "documents a known, verified limitation (PeepholeRule's SourceMethod guard) -- see this test's own comment:\n" + holderAsm);
        Assert.That(holderAsm, Does.Contain("#mul8"),
            "the multiply should still happen correctly, just via the general (non-fused) macro:\n" + holderAsm);
    }
}
