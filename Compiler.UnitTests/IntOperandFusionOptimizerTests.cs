using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Compiler.UnitTests;

// Structural tests for ILMethodIntOperandFusionOptimizer: the int
// counterpart of FloatOperandFusionOptimizerTests, covering Add/Sub/Mul
// for both 8-bit (int/uint) and 16-bit (long/ulong), with local/param/
// field/constant operands. See that class's own file for the shared
// CompileAndGetAsmDir/ReadType helpers' shape and the call-site-count
// reasoning (a trivially inlinable method with constant arguments gets
// spliced away with its arguments substituted, leaving no real Ldarg/Ldc
// pair to fuse against -- several call sites with non-constant arguments
// avoids that here too).
[TestFixture]
public class IntOperandFusionOptimizerTests
{
    static int counter;

    static string CompileAndGetAsmDir(string source)
    {
        var dir = Path.Combine(Path.GetTempPath(), "c64_int_fusion_tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var name = "IntFusion" + ++counter;
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
    public void Local_And_Constant_8Bit_Add_Fuses()
    {
        var source = @"
class Program
{
    static uint AddOne(uint a) => a + 1u;
    static void Main()
    {
        uint x = 5u;
        x = AddOne(x);
        x = AddOne(x);
        x = AddOne(x);
        x = AddOne(x);
        x = AddOne(x);
        C64Lib.C64.FillMemory(0x0900UL, x, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        Assert.That(programAsm, Does.Contain("copy_int8_const"), "the constant operand should fuse:\n" + programAsm);
        Assert.That(programAsm, Does.Not.Contain("\n    #add8 "),
            "the unfused add8 macro should not run as real code once fused:\n" + programAsm);
    }

    [Test]
    public void Two_Instance_Fields_8Bit_Mul_Fuses_Both_Operands()
    {
        var source = @"
class Holder
{
    public uint a_, b_;
    public uint Product() => a_ * b_;
}
class Program
{
    static void Main()
    {
        var h = new Holder { a_ = 3u, b_ = 4u };
        uint c = h.Product();
        h.a_ = c;
        uint d = h.Product();
        C64Lib.C64.FillMemory(0x0900UL, d, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var holderAsm = ReadType(asmDir, "Holder");

        Assert.That(holderAsm, Does.Contain("pushfld8_direct"), "both field operands should fuse:\n" + holderAsm);
        Assert.That(holderAsm, Does.Not.Contain("\n    #mul8 "),
            "the unfused mul8 macro should not run as real code once fused:\n" + holderAsm);
    }

    // NOTE on the promoted-local guard (IsNarrowAndNotPromoted in
    // ILMethodIntOperandFusionOptimizer.cs): this pass originally fused a
    // promoted local (ILMethodPromoteLocalsPass -- lives at a fixed
    // zero-page byte, never written to its nominal localsStack slot at
    // all) the same way as an ordinary one, silently reading stale
    // memory. Reproducing the exact trigger shape here proved surprisingly
    // hard -- Roslyn keeps coalescing a trivial "var a = value;" local
    // into its own parameter slot in this file's lightweight single-
    // snippet compilation, unlike inside the real Test project (compiled
    // under PostSharp's weaving). Test/ArithmeticTests.cs's own
    // TestIncrement/TestDecrement -- `var a = value; return ++a;` /
    // `return --a;` -- are the real, originally-failing case (confirmed:
    // both failed with the variable's own contribution silently lost
    // before this guard was added) and remain the regression coverage
    // for this specific fix, run for real via SimpleEmulator every time
    // the Test suite runs.

    [Test]
    public void Two_Params_16Bit_Add_Fuses_Both_Operands()
    {
        var source = @"
class Program
{
    static ulong Add(ulong a, ulong b) => a + b;
    static void Main()
    {
        ulong x = 1000ul;
        ulong y = 2000ul;
        ulong c = Add(x, y);
        c = Add(y, c);
        c = Add(c, x);
        c = Add(y, x);
        c = Add(c, y);
        C64Lib.C64.FillMemory(0x0900UL, (uint)c, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        Assert.That(programAsm, Does.Contain("locals_copy_value16"), "16-bit local operands should fuse:\n" + programAsm);
        Assert.That(programAsm, Does.Not.Contain("\n    #add16 "),
            "the unfused add16 macro should not run as real code once fused:\n" + programAsm);
    }

    // The dominant real-world 16-bit shape (found via Hunchback's own
    // generated asm): a local plus an int literal immediately widened by
    // Conv_i8/Conv_u8 (e.g. "x + 40" on a ulong) -- OpLdConst already
    // computes the full 16-bit push at compile time, leaving the Conv
    // line to emit nothing (OpConv_8_16.Emit), which is why
    // TryMatchInt16Leaf needs to look one line past the Ldc itself.
    [Test]
    public void Local_And_Widened_Constant_16Bit_Sub_Fuses()
    {
        var source = @"
class Program
{
    static ulong SubForty(ulong a) => a - 40ul;
    static void Main()
    {
        ulong x = 1000ul;
        x = SubForty(x);
        x = SubForty(x);
        x = SubForty(x);
        x = SubForty(x);
        x = SubForty(x);
        C64Lib.C64.FillMemory(0x0900UL, (uint)x, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        Assert.That(programAsm, Does.Contain("copy_int16_const"), "the widened constant operand should fuse:\n" + programAsm);
        Assert.That(programAsm, Does.Not.Contain("\n    #sub16 "),
            "the unfused sub16 macro should not run as real code once fused:\n" + programAsm);
    }
}
