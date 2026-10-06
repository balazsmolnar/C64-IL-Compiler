using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Compiler.UnitTests;

// Structural tests for ILMethodCachedFieldAccessPass: reading the SAME
// object's fields more than once anywhere in a method should only call
// resolveObjPtr once, reusing the already-resolved tmpPointer for every
// later read, correctly across arithmetic/branches/loop back-edges
// (a real forward dataflow over the method's CFG), but never across a
// real call or an access to a DIFFERENT object (both of which can
// overwrite the shared tmpPointer for real) -- safe because this GC
// only ever moves an object via an explicit GC.Collect() call, never
// implicitly on allocation.
//
// This generalizes an earlier version of the pass that only ever
// collapsed STRICTLY, PHYSICALLY adjacent reads -- found, while
// measuring how often resolveObjPtr is actually called in Hunchback,
// that real hot methods (Player.SetFrame, Player.Move, ...) read the
// same object's fields repeatedly across arithmetic/branches that don't
// themselves touch tmpPointer at all, which the narrower pass couldn't
// see through. See ILMethodCachedFieldAccessPass's own comment for the
// full derivation, including why crossing a real call was deliberately
// never pursued (measured directly: in the hottest real methods, a call
// almost always sits between two reads anyway, resolving some OTHER
// object internally).
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
    public void Different_Object_In_Between_Gets_Its_Own_Resolve_And_Invalidates_This()
    {
        // Guard: `this` is read, then a DIFFERENT object (a parameter --
        // also a this-shaped fusion target, via ldarg.1 instead of
        // ldarg.0) is read, then `this` again. Both the `other` read and
        // the SECOND `this` read must be real resolves: tmpPointer holds
        // `other`'s address after the middle read, so the final `this`
        // read can't trust it either.
        var source = @"
class Player
{
    public uint a_;
    public uint Mix(Player other)
    {
        uint x = a_;
        uint y = other.a_;
        uint z = a_;
        return x + y + z;
    }
}
class Program
{
    static void Main()
    {
        var p1 = new Player();
        var p2 = new Player();
        var p3 = new Player();
        uint r = p1.Mix(p2) + p3.Mix(p1);
        C64Lib.C64.FillMemory(0x0900UL, r, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var playerAsm = ReadType(asmDir, "Player");

        Assert.That(playerAsm, Does.Not.Contain("pushfld8_cached"),
            "a different object's read in between must prevent caching on either side of it:\n" + playerAsm);
    }

    [Test]
    public void Intervening_Static_Write_Does_Not_Prevent_Caching()
    {
        // A static field write between two `this` reads never touches
        // tmpPointer at all (static fields have a fixed address, no
        // resolveObjPtr involved) -- the dataflow should see straight
        // through it, unlike the narrower, physically-adjacent-only pass
        // this generalizes.
        var source = @"
class Player
{
    public uint a_;
    public uint b_;
    static uint s_sideEffect;
    public uint Sum()
    {
        uint x = a_;
        s_sideEffect = 1;
        uint y = b_;
        return x + y;
    }
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

        Assert.That(playerAsm, Does.Contain("pushfld8_cached"),
            "a static field write doesn't touch tmpPointer, so the second `this` read should still be cached:\n" + playerAsm);
    }

    [Test]
    public void Intervening_Call_Prevents_Caching()
    {
        // Guard: a real call sits between two `this` reads. The callee
        // might resolve any object internally (it reads ITS OWN `this`,
        // a different object from the caller's), so the cache must not
        // survive it.
        var source = @"
class Helper
{
    public uint v_;
    public uint GetV() => v_;
}
class Player
{
    public uint a_;
    public uint Sum(Helper h)
    {
        uint x = a_;
        uint mid = h.GetV();
        uint y = a_;
        return x + mid + y;
    }
}
class Program
{
    static void Main()
    {
        var p1 = new Player();
        var p2 = new Player();
        var h = new Helper();
        uint r = p1.Sum(h) + p2.Sum(h);
        C64Lib.C64.FillMemory(0x0900UL, r, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var playerAsm = ReadType(asmDir, "Player");

        Assert.That(playerAsm, Does.Not.Contain("pushfld8_cached"),
            "a real call in between must prevent caching -- the callee may resolve anything internally:\n" + playerAsm);
    }

    [Test]
    public void Loop_Caches_Second_Access_Within_Each_Iteration()
    {
        // The user's own "consider loop too" case: a loop body reading
        // the same object's two fields every iteration. The loop
        // header's own merge point (entered both from before the loop,
        // where nothing is cached yet, and from the back edge) can't
        // assume caching survives across iterations, so the FIRST access
        // each iteration still re-resolves -- but the SECOND access
        // within the SAME iteration, with nothing but arithmetic between
        // it and the first, is still a real, sound win.
        var source = @"
class Player
{
    public uint a_;
    public uint b_;
    public uint Sum(uint n)
    {
        uint sum = 0;
        for (uint i = 0; i < n; i++)
        {
            sum = sum + a_ + b_;
        }
        return sum;
    }
}
class Program
{
    static void Main()
    {
        var p1 = new Player();
        var p2 = new Player();
        uint r = p1.Sum(5) + p2.Sum(3);
        C64Lib.C64.FillMemory(0x0900UL, r, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var playerAsm = ReadType(asmDir, "Player");

        Assert.That(playerAsm, Does.Contain("pushfld8_cached"),
            "the second field read within one loop iteration should be cached:\n" + playerAsm);
    }

    [Test]
    public void Branch_Merge_Where_Both_Paths_Resolve_Same_Object_Still_Caches()
    {
        // Both the `if` and `else` branches read `this` before merging,
        // so the dataflow's meet at the merge point correctly agrees on
        // "this is cached" either way -- the read right after the merge
        // should still be a cache hit.
        var source = @"
class Player
{
    public uint a_;
    public uint b_;
    public uint c_;
    public uint Pick(bool flag)
    {
        uint mid;
        if (flag)
            mid = a_;
        else
            mid = b_;
        uint z = c_;
        return mid + z;
    }
}
class Program
{
    static void Main()
    {
        var p1 = new Player();
        var p2 = new Player();
        uint r = p1.Pick(true) + p2.Pick(false);
        C64Lib.C64.FillMemory(0x0900UL, r, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var playerAsm = ReadType(asmDir, "Player");

        Assert.That(playerAsm, Does.Contain("pushfld8_cached"),
            "both branches resolve `this` before the merge, so the read right after should be a cache hit:\n" + playerAsm);
    }
}
