using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Compiler.UnitTests;

// Structural tests for ILMethodDivConstOptimizer: "x / (1, 2, or 4)" on
// an UNSIGNED operand becomes a fixed-count logical right shift
// (div_shift_const8/16), mirroring ILMethodMulConstOptimizer's own
// multiply-by-power-of-two fusion. The SIGNED case must never be
// touched by this -- a logical right shift zero-fills from the top
// regardless of sign, so folding it there would silently corrupt the
// result for any negative dividend. That guard is the most
// safety-critical part of this optimizer, so it gets the most direct
// test coverage here, not just a passing mention.
[TestFixture]
public class DivConstOptimizerTests
{
    static int counter;

    static string CompileAndGetAsmDir(string source)
    {
        var dir = Path.Combine(Path.GetTempPath(), "c64_div_const_tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var name = "DivConst" + ++counter;
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
    public void Unsigned_Divide_By_Power_Of_Two_Becomes_Shift()
    {
        var source = @"
class Program
{
    static void Main()
    {
        uint av = 20;
        uint a = av / 4;
        C64Lib.C64.FillMemory(0x0900UL, a, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        Assert.That(programAsm, Does.Contain("div_shift_const8 2"),
            "uint / 4 should fuse into a fixed 2-bit logical right shift:\n" + programAsm);
        // The unfused line still appears as a "; OPT" dead-code comment
        // (every PeepholeRule leaves the matched window's own lines in
        // place, just marked Optimized -- see
        // ILMethodCachedFieldAccessPass's own comment for that
        // mechanism) -- check it's not a REAL, live instruction instead.
        Assert.That(programAsm, Does.Not.Contain("\n    #div_unsigned8"),
            "the general div_unsigned8 macro should not run as real code once fused:\n" + programAsm);
    }

    [Test]
    public void Unsigned_Wide_Divide_By_Power_Of_Two_Becomes_Shift()
    {
        var source = @"
class Program
{
    static void Main()
    {
        ulong cv = 20;
        ulong c = cv / 4;
        C64Lib.C64.FillMemory(0x0900UL, (uint)c, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        Assert.That(programAsm, Does.Contain("div_shift_const16 2"),
            "ulong / 4 should fuse into a fixed 2-bit logical right shift:\n" + programAsm);
        Assert.That(programAsm, Does.Not.Contain("\n    #div_unsigned16"),
            "the general div_unsigned16 macro should not run as real code once fused:\n" + programAsm);
    }

    // The safety-critical guard: signed division by the SAME constants
    // must never be touched by this optimizer, regardless of how
    // tempting the shape looks.
    [Test]
    public void Signed_Divide_By_Power_Of_Two_Is_Never_Shifted()
    {
        var source = @"
class Program
{
    static void Main()
    {
        int bv = -7;
        int b = bv / 4;
        C64Lib.C64.FillMemory(0x0900UL, (uint)b, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        Assert.That(programAsm, Does.Not.Contain("div_shift_const"),
            "signed division must never be folded into a logical shift -- it would silently corrupt negative dividends:\n" + programAsm);
        Assert.That(programAsm, Does.Contain("#div8"),
            "signed division should still happen correctly via the general (sign-aware) macro:\n" + programAsm);
    }

    // Guard: a divisor that doesn't have a shift equivalent (3, 5, ...)
    // must fall through untouched to the general macro, same as
    // ILMethodMulConstOptimizer's own equivalent guard.
    [Test]
    public void Divide_By_Non_Power_Of_Two_Constant_Is_Not_Shifted()
    {
        var source = @"
class Program
{
    static void Main()
    {
        uint av = 20;
        uint a = av / 5;
        C64Lib.C64.FillMemory(0x0900UL, a, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        Assert.That(programAsm, Does.Not.Contain("div_shift_const"),
            "5 has no shift equivalent -- this must fall through to the general divide macro:\n" + programAsm);
        Assert.That(programAsm, Does.Contain("div_unsigned8"),
            "the general unsigned divide macro should still run:\n" + programAsm);
    }

    [Test]
    public void Divide_By_One_Becomes_A_No_Shift()
    {
        var source = @"
class Program
{
    static void Main()
    {
        uint av = 20;
        uint a = av / 1;
        C64Lib.C64.FillMemory(0x0900UL, a, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        Assert.That(programAsm, Does.Contain("div_shift_const8 0"),
            "dividing by 1 should still fuse, with a shift count of 0 (no-op shift, but no runtime divide either):\n" + programAsm);
    }
}
