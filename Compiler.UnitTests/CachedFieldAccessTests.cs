using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Compiler.UnitTests;

// Structural tests for ILMethodCachedFieldAccessPass: reading two fields
// off the SAME object back to back should only call resolveObjPtr once,
// reusing the already-resolved tmpPointer for the second read -- safe
// because this GC only ever moves an object via an explicit GC.Collect()
// call (Runtime_CheckHeapRoom's own comment: allocation alone just
// hard-faults on insufficient room, never compacts). Found while
// investigating how often resolveObjPtr actually gets called in a real
// build (494 times in Hunchback) -- 156 of those were a provably-
// redundant re-resolve within a few lines of the one right before it.
//
// Covers both of the two places a chain of IPushFldOperation reads can
// come from: ILMethodInliningPass's trivial-argument-substitution fusion
// (OpPushFldAt, built early) and ILPropertyGettterOptimizer's own
// `this.field` fusion (OpPushFld, built later via a PeepholeRule, whose
// freshly-inserted operation never gets real PreviousInstructions -- see
// ILMethodCachedFieldAccessPass's own comment for why that needed a
// different safety check than the splice-marker case).
[TestFixture]
public class CachedFieldAccessTests
{
    static int counter;

    static string CompileAndGetAsmDir(string source)
    {
        var dir = Path.Combine(Path.GetTempPath(), "c64_cached_field_tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var name = "CachedField" + ++counter;
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
    public void Two_Trivially_Substituted_Getters_On_Same_Object_Share_One_Resolve()
    {
        var source = @"
class Player
{
    public ulong x_;
    public uint y_;
    public ulong X { get => x_; set { x_ = value; } }
    public uint Y { get => y_; set { y_ = value; } }
}
class Program
{
    static void Main()
    {
        var player = new Player();
        ulong tmp = player.X + player.Y;
        C64Lib.C64.FillMemory(0x0900UL, (uint)tmp, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        Assert.That(programAsm, Does.Contain("pushfld16_at"), "the first read should still do a real resolve:\n" + programAsm);
        Assert.That(programAsm, Does.Contain("pushfld8_cached"),
            "the second read (same object) should reuse the already-resolved pointer instead of resolving again:\n" + programAsm);
        Assert.That(programAsm, Does.Not.Contain("pushfld8_at"),
            "the second read should have been replaced, not left as its own separate resolve:\n" + programAsm);
    }

    [Test]
    public void Two_This_Field_Reads_In_One_Method_Share_One_Resolve()
    {
        // Two call sites (N=2) so Sum() is never a cost-model win and
        // stays a real, standalone, never-spliced method -- isolates
        // ILPropertyGettterOptimizer's own #pushfld fusion path (a
        // PeepholeRule, built late) from ILMethodInliningPass's (built
        // early), which need different safety-checking internally.
        var source = @"
class Player
{
    public uint a_;
    public uint b_;
    public uint Sum() => a_ + b_;
}
class Program
{
    static void Main()
    {
        var p1 = new Player();
        var p2 = new Player();
        uint r = p1.Sum() + p2.Sum();
        C64Lib.C64.FillMemory(0x0900UL, r, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var playerAsm = ReadType(asmDir, "Player");

        Assert.That(playerAsm, Does.Contain("pushfld8 "), "the first read should still do a real resolve:\n" + playerAsm);
        Assert.That(playerAsm, Does.Contain("pushfld8_cached"),
            "the second read (same `this`) should reuse the already-resolved pointer instead of resolving again:\n" + playerAsm);
    }

    [Test]
    public void Different_Objects_Each_Get_Their_Own_Resolve()
    {
        // Guard: p1 and p2 are DIFFERENT objects -- even though both
        // reads are still "trivial" and adjacent, neither should be
        // treated as cached, since tmpPointer would hold the wrong
        // object's pointer by the second read.
        var source = @"
class Player
{
    public uint a_;
}
class Program
{
    static void Main()
    {
        var p1 = new Player { a_ = 3 };
        var p2 = new Player { a_ = 5 };
        uint r = p1.a_ + p2.a_;
        C64Lib.C64.FillMemory(0x0900UL, r, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        Assert.That(programAsm, Does.Not.Contain("pushfld8_cached"), "different objects must never share a resolve:\n" + programAsm);
    }

    [Test]
    public void Intervening_Statement_Prevents_Caching()
    {
        // Guard: a real statement (writing to a static field) sits
        // between the two reads of the same object -- the narrow,
        // first-cut scope deliberately only trusts DIRECTLY adjacent
        // reads, so this must NOT be collapsed even though nothing about
        // the intervening statement actually touches tmpPointer.
        var source = @"
class Player
{
    public uint a_;
    public uint b_;
}
class Program
{
    static uint s_sideEffect;
    static void Main()
    {
        var player = new Player { a_ = 3, b_ = 4 };
        uint x = player.a_;
        s_sideEffect = 1;
        uint y = player.b_;
        C64Lib.C64.FillMemory(0x0900UL, x + y, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        Assert.That(programAsm, Does.Not.Contain("pushfld8_cached"), "an intervening statement must block caching in this narrow first cut:\n" + programAsm);
    }
}
