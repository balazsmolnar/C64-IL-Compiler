using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Compiler.UnitTests;

// Structural tests for ILMethodFloatOperandFusionOptimizer: a float binary/
// unary/compare op whose operand(s) are a simple local, parameter, or
// already-resolved field read should fuse into one of the OpFusedFloat*
// operations (Compiler/Operands/OperandFloatFusion.cs), skipping the
// hardware-stack round trip (locals_push_valueflt/pushfldflt +
// stack_pull_mflpt) the unfused path uses. Mirrors DivConstOptimizerTests'
// shape (full-pipeline: compile a snippet, assert the generated asm text).
//
// Uses PARAMETERS (not two separately-declared locals) for the local-
// operand cases, same as FloatTests.cs's own TestAdd/TestMul/etc: Roslyn's
// Release-mode IL for "float a = 2f; float b = 3f; var c = a * b;" keeps
// the first constant sitting on the natural evaluation stack rather than
// materializing it into its own local slot (it's never read again), so
// only ONE of the two operands ends up as a real Ldloc -- a parameter is
// always a real, named Ldarg at its use site regardless. Field-operand
// methods are called from two sites, to keep the inlining pass
// (ILMethodInliningPass) from splicing the whole call away -- an inlined
// window is marked SourceMethod != null, which PeepholeRule.TryApply
// deliberately never fuses (the "VarIndex hazard" guard), so an inlined
// call site would silently never reach this optimizer at all.
[TestFixture]
public class FloatOperandFusionOptimizerTests
{
    static int counter;

    static string CompileAndGetAsmDir(string source)
    {
        var dir = Path.Combine(Path.GetTempPath(), "c64_float_fusion_tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var name = "FloatFusion" + ++counter;
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

    [Test]
    public void Two_Params_Multiply_Fuses_Both_Operands()
    {
        // Several call sites, non-constant arguments: a single- or
        // double-call-site method this tiny still gets fully inlined AND
        // (for constant arguments) constant-substituted
        // (ILMethodInliningPass's own net-size cost model finds splicing a
        // one-op body cheaper than a dedicated jsr up to a few sites),
        // leaving no real Ldarg at all to fuse against -- enough call
        // sites tips that calculus the other way, leaving a real, un-
        // inlined Program_Mul with a genuine Ldarg+Ldarg+Mul body.
        var source = @"
class Program
{
    static float Mul(float a, float b) => a * b;
    static void Main()
    {
        float x = 2f;
        float y = 3f;
        float c = Mul(x, y);
        c = Mul(y, c);
        c = Mul(c, x);
        c = Mul(y, x);
        c = Mul(c, y);
        C64Lib.C64.FillMemory(0x0900UL, (uint)c, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        Assert.That(programAsm, Does.Contain("locals_copy_valueflt"),
            "two float params feeding a Mul should fuse via the direct-copy macro:\n" + programAsm);
        Assert.That(programAsm, Does.Not.Contain("\n    #mulflt"),
            "the unfused mulflt macro should not run as real code once fused:\n" + programAsm);
    }

    [Test]
    public void Two_Instance_Fields_Add_Fuses_Both_Operands()
    {
        var source = @"
class Holder
{
    public float a_, b_;
    public float Sum() => a_ + b_;
}
class Program
{
    static void Main()
    {
        var h = new Holder { a_ = 2f, b_ = 3f };
        float c = h.Sum();
        h.a_ = c;
        float d = h.Sum();
        C64Lib.C64.FillMemory(0x0900UL, (uint)d, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var holderAsm = ReadType(asmDir, "Holder");

        Assert.That(holderAsm, Does.Contain("pushfldflt_direct"),
            "two float fields feeding an Add should fuse via the direct-copy macro:\n" + holderAsm);
        Assert.That(holderAsm, Does.Not.Contain("\n    #addflt"),
            "the unfused addflt macro should not run as real code once fused:\n" + holderAsm);
    }

    [Test]
    public void Field_And_Param_Mixed_Operands_Fuse()
    {
        var source = @"
class Holder
{
    public float a_;
    public float Scale(float b) => a_ * b;
}
class Program
{
    static void Main()
    {
        var h = new Holder { a_ = 2f };
        float c = h.Scale(3f);
        float d = h.Scale(c);
        C64Lib.C64.FillMemory(0x0900UL, (uint)d, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var holderAsm = ReadType(asmDir, "Holder");

        Assert.That(holderAsm, Does.Contain("pushfldflt_direct"), "field operand should fuse:\n" + holderAsm);
        Assert.That(holderAsm, Does.Contain("locals_copy_valueflt"), "param operand should fuse:\n" + holderAsm);
        Assert.That(holderAsm, Does.Not.Contain("\n    #mulflt"),
            "the unfused mulflt macro should not run as real code once fused:\n" + holderAsm);
    }

    [Test]
    public void Negate_Param_Fuses()
    {
        var source = @"
class Program
{
    static float Negate(float a) => -a;
    static void Main()
    {
        float x = 5f;
        x = Negate(x);
        x = Negate(x);
        x = Negate(x);
        x = Negate(x);
        x = Negate(x);
        C64Lib.C64.FillMemory(0x0900UL, (uint)x, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        Assert.That(programAsm, Does.Contain("locals_copy_valueflt"), "negate operand should fuse:\n" + programAsm);
        Assert.That(programAsm, Does.Not.Contain("\n    #negateflt"),
            "the unfused negateflt macro should not run as real code once fused:\n" + programAsm);
    }

    [Test]
    public void Compare_Greater_Params_Fuses()
    {
        var source = @"
class Program
{
    static bool Greater(float a, float b) => a > b;
    static void Main()
    {
        float x = 5f;
        float y = 3f;
        uint n = 0;
        if (Greater(x, y)) n = n + 1;
        if (Greater(y, x)) n = n + 1;
        if (Greater(x, n)) n = n + 1;
        if (Greater(n, x)) n = n + 1;
        if (Greater(y, n)) n = n + 1;
        C64Lib.C64.FillMemory(0x0900UL, n, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        Assert.That(programAsm, Does.Contain("locals_copy_valueflt"), "compare operands should fuse:\n" + programAsm);
        Assert.That(programAsm, Does.Not.Contain("\n    #compareGreaterflt"),
            "the unfused compareGreaterflt macro should not run as real code once fused:\n" + programAsm);
    }

    // A float literal (Ldc_r4) operand fuses via copy_mflpt_const
    // (asm/helper/stack.asm), same mechanism as a local/param/field
    // operand -- see ILMethodFloatOperandFusionOptimizer.IsFusableFloatOperand.
    [Test]
    public void Param_And_Constant_Operand_Fuses()
    {
        var source = @"
class Program
{
    static float Mul(float a) => a * 3f;
    static void Main()
    {
        float x = 2f;
        float y = 5f;
        float c = Mul(x);
        c = Mul(y);
        c = Mul(c);
        c = Mul(y);
        c = Mul(c);
        C64Lib.C64.FillMemory(0x0900UL, (uint)c, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        Assert.That(programAsm, Does.Contain("copy_mflpt_const"),
            "a constant operand should fuse via copy_mflpt_const:\n" + programAsm);
        Assert.That(programAsm, Does.Not.Contain("\n    #mulflt"),
            "the unfused mulflt macro should not run as real code once fused:\n" + programAsm);
    }

    // Chained first operand: a*b's own result, left in zp_flt_a instead of
    // being pushed to the hardware stack, feeds Neg directly -- see
    // ILMethodFloatOperandFusionOptimizer's IsChainableFloatProducer/
    // MarkAsChainProducer and OperandFloatFusion.cs's PushResult/
    // OpFloatProducerNoPush. Two call sites, same reason as the other
    // field-operand tests: keeps the method from being inlined away.
    [Test]
    public void Chain_Negate_Of_Mul_Result_Fuses()
    {
        var source = @"
class Holder
{
    public float a_, b_;
    public float Chain() => -(a_ * b_);
}
class Program
{
    static void Main()
    {
        var h = new Holder { a_ = 2f, b_ = 3f };
        float c = h.Chain();
        h.a_ = c;
        float d = h.Chain();
        C64Lib.C64.FillMemory(0x0900UL, (uint)d, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var holderAsm = ReadType(asmDir, "Holder");

        Assert.That(holderAsm, Does.Contain("jsr Float_Mul"), "the mul should still run:\n" + holderAsm);
        Assert.That(CountOccurrences(holderAsm, "stack_push_var_mflpt"), Is.EqualTo(1),
            "exactly one push is expected -- Neg's own result, Chain()'s return value; the mul's own intermediate push into Neg should be elided:\n" + holderAsm);
        Assert.That(holderAsm, Does.Not.Contain("stack_pull_mflpt"),
            "nothing should pull from the hardware stack -- both the mul's own field operands and its chained result into Neg are fused:\n" + holderAsm);
    }

    static int CountOccurrences(string haystack, string needle)
    {
        int count = 0, index = 0;
        while ((index = haystack.IndexOf(needle, index)) != -1)
        {
            count++;
            index += needle.Length;
        }
        return count;
    }

    // Chained first operand into a binary op, with a plain leaf as the
    // second operand: (a*b) + c -- the mul's result stays in zp_flt_a,
    // c's own field read copies directly into zp_flt_b, same restriction
    // as the class comment's "second operand must be a plain leaf".
    [Test]
    public void Chain_Mul_Then_Add_With_Leaf_Second_Operand_Fuses()
    {
        var source = @"
class Holder
{
    public float a_, b_, c_;
    public float ChainAdd() => a_ * b_ + c_;
}
class Program
{
    static void Main()
    {
        var h = new Holder { a_ = 2f, b_ = 3f, c_ = 4f };
        float c = h.ChainAdd();
        h.a_ = c;
        float d = h.ChainAdd();
        C64Lib.C64.FillMemory(0x0900UL, (uint)d, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var holderAsm = ReadType(asmDir, "Holder");

        Assert.That(holderAsm, Does.Contain("jsr Float_Mul"), "the mul should still run:\n" + holderAsm);
        Assert.That(holderAsm, Does.Contain("jsr Float_Add"), "the add should still run:\n" + holderAsm);
        Assert.That(CountOccurrences(holderAsm, "stack_push_var_mflpt"), Is.EqualTo(1),
            "exactly one push is expected -- Add's own result, ChainAdd()'s return value; the mul's own intermediate push into Add should be elided:\n" + holderAsm);
        Assert.That(holderAsm, Does.Not.Contain("stack_pull_mflpt"),
            "nothing should pull from the hardware stack -- every operand (both of the mul's and c_ for the add) is fused:\n" + holderAsm);
    }
}
