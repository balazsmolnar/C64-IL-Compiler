using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Compiler.UnitTests;

// ILReachabilityPass: what the program can never use is not emitted, and
// everything the call graph cannot see (delegate targets, labels named in
// RawAssembly or in C64Address.FromLabel, static constructors, virtual
// slots of instantiated types) is kept. Test.Programs/DeadCodeTests.cs
// runs a program with removed code on the emulator.
[TestFixture]
public class DeadCodeTests
{
    static int counter;

    static (int exit, string asmDir, string errors) Compile(string source, params string[] switches)
    {
        var dir = Path.Combine(Path.GetTempPath(), "c64_dead_code_tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var name = "DeadCode" + ++counter;
        var dll = Path.Combine(dir, name + ".dll");
        var pdb = Path.Combine(dir, name + ".pdb");

        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
            .Split(Path.PathSeparator)
            .Select(p => MetadataReference.CreateFromFile(p))
            .Append(MetadataReference.CreateFromFile(Path.Combine(AppContext.BaseDirectory, "C64Lib.dll")))
            .ToList();
        var tree = CSharpSyntaxTree.ParseText(source, path: Path.Combine(dir, "snippet.cs"), encoding: System.Text.Encoding.UTF8);
        var compilation = CSharpCompilation.Create(name, new[] { tree }, references,
            new CSharpCompilationOptions(OutputKind.ConsoleApplication, optimizationLevel: OptimizationLevel.Release));
        using (var dllStream = File.Create(dll))
        using (var pdbStream = File.Create(pdb))
        {
            var emit = compilation.Emit(dllStream, pdbStream, options: new Microsoft.CodeAnalysis.Emit.EmitOptions(debugInformationFormat: Microsoft.CodeAnalysis.Emit.DebugInformationFormat.PortablePdb));
            Assert.That(emit.Success, Is.True, "the test's own C# does not compile:\n" + string.Join("\n", emit.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        }

        var outDir = Path.Combine(dir, "asm");
        Directory.CreateDirectory(outDir);
        var errors = new StringWriter();
        var exit = Program.Run(new[] { dll, outDir, Path.Combine(dir, "entry.asm"), "program" }.Concat(switches).ToArray(), errors);
        return (exit, outDir, errors.ToString());
    }

    static string AllAsm(string asmDir) =>
        string.Join("\n", Directory.GetFiles(asmDir, "*.asm")
            .Where(f => Path.GetFileName(f) != "generated.asm") // its header lists what was removed
            .Select(File.ReadAllText));

    const string Source = @"
using System;
using C64Lib;
[assembly: RawAssembly(Asm = ""RawUser jsr FromRaw_Called\n rts"")]

abstract class Element
{
    public abstract void Draw();
    public virtual void Unused() { C64.FillMemory(0x0400UL, 1, 1); }
}
class Used : Element
{
    public uint X;
    public override void Draw() { C64.FillMemory(0x0400UL, X, 1); }
    public override void Unused() { C64.FillMemory(0x0401UL, 2, 1); }
}
class NeverCreated : Element
{
    public override void Draw() { C64.FillMemory(0x0402UL, 3, 1); }
}
static class Helpers
{
    public static uint Live(uint x) => x + 1;
    public static uint Dead(uint x) { var s = ""dead string""; return (uint)s.Length + x; }
    public static uint Handler() => 4;
}
static class DeadType
{
    public static uint Field;
    public static void Touch() { Field = 3; }
}
static class FromRaw
{
    public static void Called() { C64.FillMemory(0x0404UL, 5, 1); }
}
static class ByLabel
{
    public static void Target() { C64.FillMemory(0x0405UL, 6, 1); }
}
class WithCtor
{
    public uint V;
    public WithCtor(uint v) { V = v * 2; }
}
class DeadWithCtor
{
    public uint V;
    public DeadWithCtor(uint v) { V = v * 3; }
}
static class WithCctor
{
    public static uint Value = 4;
}
class Program
{
    static void Main()
    {
        Element e = new Used { X = Helpers.Live(1) };
        e.Draw();
        Func<uint> a = Helpers.Handler;
        C64.FillMemory(0x0403UL, a(), 1);
        var address = C64Address.FromLabel(""ByLabel_Target"");
        C64.FillMemory(address, 0, 1);
        var w = new WithCtor(5);
        C64.FillMemory(0x0406UL, w.V, 1);
    }
}";

    [Test]
    public void Unreachable_Methods_And_Types_Are_Not_Emitted()
    {
        var (exit, asmDir, errors) = Compile(Source);
        Assert.That(exit, Is.EqualTo(0), errors);
        var asm = AllAsm(asmDir);

        Assert.That(asm, Does.Not.Contain("Helpers_Dead "), "an uncalled method is removed");
        Assert.That(asm, Does.Not.Contain("dead string"), "and so are its strings");
        Assert.That(asm, Does.Not.Contain("NeverCreated_Draw"), "an override in a type that is never created is removed");
        Assert.That(File.Exists(Path.Combine(asmDir, "NeverCreated.asm")), Is.False, "a dead type has no file");
        Assert.That(File.Exists(Path.Combine(asmDir, "DeadType.asm")), Is.False, "nor do its static fields");
        Assert.That(asm, Does.Not.Contain("Element_Unused "), "a virtual slot nobody calls is removed");
        Assert.That(asm, Does.Not.Contain("Used_Unused "), "with every override");
        Assert.That(File.ReadAllText(Path.Combine(asmDir, "Used.asm")), Does.Contain("Used_VTable:  .word Used_Draw,0"),
            "the dead slot keeps its index, holding 0");

        Assert.That(asm, Does.Contain("Used_Draw "), "a live override stays");
        Assert.That(asm, Does.Contain("Helpers_Handler "), "a delegate target stays");
        Assert.That(asm, Does.Contain("FromRaw_Called "), "a label used by RawAssembly stays");
        Assert.That(asm, Does.Contain("ByLabel_Target "), "a label named in C64Address.FromLabel stays");
        Assert.That(asm, Does.Contain("WithCctor_x_cctor"), "a static constructor stays (it runs at startup)");
        Assert.That(asm, Does.Contain("WithCtor_x_ctor "), "a constructor newobj calls stays");
        Assert.That(File.Exists(Path.Combine(asmDir, "DeadWithCtor.asm")), Is.False, "a type that is never created goes, constructor and all");
    }

    [Test]
    public void No_Dce_Switch_Keeps_Everything()
    {
        var (exit, asmDir, errors) = Compile(Source, "--no-dce");
        Assert.That(exit, Is.EqualTo(0), errors);
        var asm = AllAsm(asmDir);
        Assert.That(asm, Does.Contain("Helpers_Dead "));
        Assert.That(asm, Does.Contain("NeverCreated_Draw "));
    }

    // Removed code is still checked: an unsupported construct in it is
    // reported like anywhere else.
    [Test]
    public void Unsupported_Construct_In_Dead_Code_Is_Still_Reported()
    {
        var source = @"
class Program
{
    static void Main() { }
    static uint Dead(uint a) { try { return a / 2; } finally { a = 0; } }
}";
        var (exit, _, errors) = Compile(source);
        Assert.That(exit, Is.EqualTo(1), errors);
        Assert.That(errors, Does.Contain("Program.Dead"), errors);
    }
}
