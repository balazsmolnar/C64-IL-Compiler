using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Compiler.UnitTests;

// Structural tests for the trivial-argument substitution optimization in
// ILMethodInliningPass -- when an inlined call's actual argument is a bare,
// side-effect-free read already resident at a fixed position in the
// caller's own frame (Ldarg_N/Ldloc_N, never written to by the callee),
// the splice should reference that position directly instead of copying
// it into a freshly materialized locals-stack slot via
// init_locals_pull_parameters_inline/method_exit_inline.
//
// This is the proven technique every mainstream inliner uses, just at
// different IR levels: Scheifler's 1977 CACM paper is the foundational
// correctness analysis; LLVM gets it for free via SSA value mapping
// (InlineFunction's ValueMap maps a formal parameter straight to the
// actual argument VALUE, no alloca, unless the ABI itself forces one);
// Go's documented source-level inliner (go.dev/blog/inliner) states the
// same rule explicitly: substitute trivial arguments directly, fall back
// to a temporary only when the argument has side effects or can't be
// re-read safely.
//
// These compile a small snippet with Roslyn, run the real compiler
// driver, and inspect the GENERATED ASM TEXT directly (host-side, no
// emulator) -- runtime correctness of the optimized splice is covered
// separately by Test.Runtime/InlineSubstitutionTests.cs.
[TestFixture]
public class InlineParameterSubstitutionTests
{
    static int counter;

    // Compiles `source`, runs the real compiler, and returns the directory
    // containing the generated per-type .asm files.
    static string CompileAndGetAsmDir(string source)
    {
        var dir = Path.Combine(Path.GetTempPath(), "c64_inline_subst_tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var name = "InlineSubst" + ++counter;
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
    public void Trivial_This_Argument_Skips_Materialization()
    {
        var source = Using + @"
class Holder
{
    public uint value_;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint GetValue() => value_;
    public uint Use() => GetValue();
}
class Program
{
    static void Main()
    {
        var h = new Holder();
        h.value_ = 42;
        var r = h.Use();
        C64Lib.C64.FillMemory(0x0900UL, r, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var holderAsm = ReadType(asmDir, "Holder");

        Assert.That(holderAsm, Does.Not.Contain("init_locals_pull_parameters_inline"),
            "trivial `this` argument should skip materializing a new locals-stack slot entirely:\n" + holderAsm);
        Assert.That(holderAsm, Does.Not.Contain("method_exit_inline"),
            "with the prologue skipped, there is nothing for the epilogue to release either:\n" + holderAsm);
    }

    // Guard: a parameter the callee WRITES to must never be substituted --
    // doing so would alias the caller's own variable, so a write inside the
    // callee would corrupt it instead of just the callee's own copy.
    [Test]
    public void Written_Parameter_Still_Materializes()
    {
        var source = Using + @"
class Holder
{
    public uint value_;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint GetValueTwice(uint fallback)
    {
        if (value_ == 0)
            fallback = 99;
        return fallback;
    }
    public uint Use(uint x) => GetValueTwice(x);
}
class Program
{
    static void Main()
    {
        var h = new Holder();
        var r = h.Use(7);
        C64Lib.C64.FillMemory(0x0900UL, r, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var holderAsm = ReadType(asmDir, "Holder");

        Assert.That(holderAsm, Does.Contain("init_locals_pull_parameters_inline"),
            "a parameter the callee writes to must still get its own independent copy:\n" + holderAsm);
    }

    // Guard: an argument that is NOT a bare Ldarg/Ldloc (here, a field read
    // off a parameter -- two IL instructions, not one) must still
    // materialize -- it has a real dereference to perform, not just "reuse
    // an existing slot".
    [Test]
    public void NonTrivial_Argument_Still_Materializes()
    {
        var source = Using + @"
class Carrier { public uint Value; }
class Holder
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Echo(uint v) => v;
    public uint Use(Carrier c) => Echo(c.Value);
}
class Program
{
    static void Main()
    {
        var c = new Carrier();
        c.Value = 3;
        var h = new Holder();
        var r = h.Use(c);
        C64Lib.C64.FillMemory(0x0900UL, r, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var holderAsm = ReadType(asmDir, "Holder");

        Assert.That(holderAsm, Does.Contain("init_locals_pull_parameters_inline"),
            "c.Value is a field read, not a bare Ldarg/Ldloc -- not trivial, must still materialize:\n" + holderAsm);
    }
}
