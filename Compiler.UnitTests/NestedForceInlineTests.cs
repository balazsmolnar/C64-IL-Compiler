using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Compiler.UnitTests;

// Structural tests for "nested" [MethodImpl(AggressiveInlining)]: a
// force-inlined method that itself calls another method.
//
// ILCallSiteCountPass's own comment is explicit that this is a hard
// precondition, not a size decision the attribute can override: "a
// method qualifies only if it's a leaf (no Call/Callvirt/Switch anywhere
// in its own body -- recursion and nested inlining are both out of
// scope) ... hard preconditions, [MethodImpl(AggressiveInlining)]
// included." Eligibility is also decided once, upfront, for the whole
// program (ILCallSiteCountPass runs before ILCodePass's method loop even
// starts -- see Program.cs's own comment) against each method's
// ORIGINAL body, not re-evaluated after a sibling inline changes some
// other method's shape.
//
// So a force-inlined method that calls something else is never itself
// spliced anywhere, attribute or not -- but a call site INSIDE its own
// body can still inline normally if ITS target is a leaf. These tests
// exist because, before they were added, every existing AggressiveInlining
// test target in this project happened to be a pure leaf -- this
// specific interaction had never actually been exercised.
[TestFixture]
public class NestedForceInlineTests
{
    static int counter;

    static string CompileAndGetAsmDir(string source)
    {
        var dir = Path.Combine(Path.GetTempPath(), "c64_nested_force_inline_tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var name = "NestedForceInline" + ++counter;
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
    public void Non_Leaf_Force_Inline_Stays_Standalone_While_Its_Own_Leaf_Call_Still_Inlines()
    {
        var source = Using + @"
class Holder
{
    public uint value_;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint GetValue() => value_;

    // Also force-inline, but calls GetValue() -- not a leaf, so the hard
    // leaf precondition rejects splicing THIS method anywhere, attribute
    // or not. GetValue() itself is still a leaf, and still inlines
    // normally into this method's own body.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint GetValuePlusOne() => GetValue() + 1;

    public uint Use() => GetValuePlusOne();
}
class Program
{
    static void Main()
    {
        var h = new Holder();
        h.value_ = 41;
        var r = h.Use();
        C64Lib.C64.FillMemory(0x0900UL, r, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var holderAsm = ReadType(asmDir, "Holder");

        Assert.That(holderAsm, Does.Contain("Holder_GetValuePlusOne"),
            "GetValuePlusOne is not a leaf (it calls GetValue), so its standalone definition must be kept despite the attribute:\n" + holderAsm);
        Assert.That(holderAsm, Does.Not.Contain("jsr Holder_GetValue "),
            "GetValue IS a leaf and should still inline into GetValuePlusOne's own body, leaving no real call to it:\n" + holderAsm);
        Assert.That(holderAsm, Does.Contain("jsr Holder_GetValuePlusOne"),
            "GetValuePlusOne itself was never spliced anywhere, so Use() must still call it for real:\n" + holderAsm);
    }

    [Test]
    public void Two_Level_Force_Inline_Chain_Only_Inlines_The_Leaf_Level()
    {
        // A -> B -> C, all three marked AggressiveInlining, only C is a
        // leaf. Confirms the rejection isn't limited to exactly one
        // non-leaf level: B calls C (fine, C is a leaf, inlines into B);
        // A calls B, but B is NOT a leaf (it still has a real call to C
        // folded in before ILCallSiteCountPass's own eligibility
        // decision -- which runs once, upfront, against each method's
        // ORIGINAL body -- ever ran), so A is rejected for the same
        // reason B's own call site into C was accepted: it isn't about
        // depth, only about whether a given method's own un-inlined body
        // is a leaf.
        var source = Using + @"
class Holder
{
    public uint value_;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint C() => value_;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint B() => C() + 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint A() => B() + 1;

    public uint Use() => A();
}
class Program
{
    static void Main()
    {
        var h = new Holder();
        h.value_ = 40;
        var r = h.Use();
        C64Lib.C64.FillMemory(0x0900UL, r, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var holderAsm = ReadType(asmDir, "Holder");

        Assert.That(holderAsm, Does.Not.Contain("jsr Holder_C "),
            "C is a leaf and should inline into B's own body:\n" + holderAsm);
        Assert.That(holderAsm, Does.Contain("Holder_B"),
            "B is not a leaf (it calls C), so its standalone definition must be kept:\n" + holderAsm);
        Assert.That(holderAsm, Does.Contain("Holder_A"),
            "A is not a leaf (it calls B), so its standalone definition must be kept:\n" + holderAsm);
        Assert.That(holderAsm, Does.Contain("jsr Holder_A"),
            "A was never spliced anywhere, so Use() must still call it for real:\n" + holderAsm);
        Assert.That(holderAsm, Does.Contain("jsr Holder_B"),
            "B was never spliced anywhere either, so A must still call it for real:\n" + holderAsm);
    }
}
