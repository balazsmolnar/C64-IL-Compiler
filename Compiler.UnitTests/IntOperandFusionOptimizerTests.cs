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

        // Two locals/parameters: added straight from where they are
        // (arith16_direct, asm/helper/arithmetic.asm), no zp_param copies.
        Assert.That(programAsm, Does.Contain("#arith16_direct 0, 1, "), "16-bit local operands should fuse:\n" + programAsm);
        Assert.That(programAsm, Does.Not.Contain("locals_copy_value16"), "no copy into zp_param1/2 is left:\n" + programAsm);
        Assert.That(programAsm, Does.Not.Contain("\n    #add16 "),
            "the unfused add16 macro should not run as real code once fused:\n" + programAsm);
    }

    // The dominant real-world 16-bit shape (found via Hunchback's own
    // generated asm): a local plus an int literal immediately widened by
    // Conv_i8/Conv_u8 (e.g. "x + 40" on a ulong) -- OpLdConst already
    // computes the full 16-bit push at compile time, leaving the Conv
    // line to emit nothing (OpConv_8_16.Emit), which is why
    // TryMatchInt16Leaf needs to look one line past the Ldc itself.
    // `x += c` on a 16-bit local: computed and stored in one go
    // (arith16_store), and so is a sum stored to another local.
    [Test]
    public void Local_16Bit_Add_Is_Stored_Directly()
    {
        var source = @"
class Program
{
    static void Main()
    {
        ulong address = 0x0400ul;
        for (uint y = 0; y < 5; y++)
            address += 40ul;
        ulong other = address + 1000ul;
        C64Lib.C64.FillMemory(other, 1, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        Assert.That(programAsm, Does.Match(@"#arith16_store 0, 1, (\d+), 0, 40, \1\b"), "x += 40 should be done in place:\n" + programAsm);
        // (the replaced lines stay in the listing as "; OPT" comments)
        // (the C# compiler keeps no local for `other`: the sum is the argument)
        Assert.That(programAsm, Does.Match(@"#arith16_direct 0, 1, \d+, 0, 1000"), "local + constant as an argument:\n" + programAsm);
        Assert.That(programAsm, Does.Not.Contain("\n    #add16"), "no generic 16-bit add runs as real code:\n" + programAsm);
    }

    // The first operand already on the evaluation stack (a call's result):
    // the constant is added as it is pulled (arith16_stack).
    [Test]
    public void Stack_Plus_Constant_16Bit_Adds_On_The_Stack()
    {
        var source = @"
class Program
{
    static ulong Base(ulong a) { C64Lib.C64.FillMemory(a, 1, 1); C64Lib.C64.FillMemory(a, 2, 1); return a; }
    static void Main()
    {
        ulong x = Base(0x0900ul) + 300ul;
        ulong y = Base(x) - 2ul;
        C64Lib.C64.FillMemory(y, 3, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        Assert.That(programAsm, Does.Contain("#arith16_stack 0, 0, 300"), "call result + constant:\n" + programAsm);
        Assert.That(programAsm, Does.Contain("#arith16_stack 1, 0, 2"), "call result - constant:\n" + programAsm);
    }

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

        // The constant goes in as an immediate operand of the subtraction.
        Assert.That(programAsm, Does.Match(@"#arith16_direct 1, 1, \d+, 0, 40"), "the widened constant operand should fuse:\n" + programAsm);
        Assert.That(programAsm, Does.Not.Contain("copy_int16_const"), "no copy into zp_param1 is left:\n" + programAsm);
        Assert.That(programAsm, Does.Not.Contain("\n    #sub16 "),
            "the unfused sub16 macro should not run as real code once fused:\n" + programAsm);
    }

    // Branch-truthiness fusion (ExecuteBranchTruthiness/
    // OpFusedBranchTruthiness): a bare `if (x)`/`if (!x)` on a simple
    // leaf loads straight into register A and branches, instead of
    // round-tripping the value through the hardware stack via
    // branch_true8/branch_false8's own stack_pull_int_a. Checks both
    // directions (bne for Brtrue, beq for Brfalse) so a swapped branch
    // sense would be caught structurally here, not just by the real
    // correctness tests in Test/BranchTest.cs.
    [Test]
    public void Branch_Truthiness_On_Local_Fuses()
    {
        var source = @"
class Program
{
    static void Check(bool x)
    {
        if (x) C64Lib.C64.FillMemory(0x0900UL, 1, 1);
        if (!x) C64Lib.C64.FillMemory(0x0900UL, 2, 1);
    }
    static void Main()
    {
        bool a = true;
        bool b = false;
        Check(a);
        Check(b);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        Assert.That(programAsm, Does.Not.Contain("\n    #branch_true8"),
            "the unfused branch_true8 macro should not run as real code once fused:\n" + programAsm);
        Assert.That(programAsm, Does.Not.Contain("\n    #branch_false8"),
            "the unfused branch_false8 macro should not run as real code once fused:\n" + programAsm);
        Assert.That(programAsm, Does.Contain("bne Program_Check"), "Brtrue should fuse into a bne:\n" + programAsm);
        Assert.That(programAsm, Does.Contain("beq Program_Check"), "Brfalse should fuse into a beq:\n" + programAsm);
    }
}
