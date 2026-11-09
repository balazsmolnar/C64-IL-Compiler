using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Compiler.UnitTests;

// Which methods ILStaticFramePass gives a static (fixed-address) frame, and
// where. Its summary in generated.asm lists every static frame with its
// offset and every dynamic method with the reason. Runtime behaviour is
// covered by Test.Runtime/StaticFrameTests.cs.
[TestFixture]
public class StaticFrameTests
{
    static int counter;

    static string Compile(string source, params string[] switches)
    {
        var dir = Path.Combine(Path.GetTempPath(), "c64_static_frame_tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var name = "StaticFrame" + ++counter;
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
        var exit = Program.Run(new[] { dll, outDir, Path.Combine(dir, "entry.asm"), "program" }.Concat(switches).ToArray(), errors);
        Assert.That(exit, Is.EqualTo(0), "expected the program to compile cleanly; output:\n" + errors);
        return outDir;
    }

    static string Generated(string asmDir) => File.ReadAllText(Path.Combine(asmDir, "generated.asm"));

    // Every helper calls Tick() so the inliner leaves it a real subroutine.
    const string Source = @"
using C64Lib;
class Program
{
    static uint ticks;
    static void Tick() { ticks = ticks + 1; }

    static uint Leaf(uint a) { uint r = a + 1; Tick(); return r; }
    static uint Other(uint a, uint b) { uint r = a + b; Tick(); return r; }
    static uint Middle(uint a) { uint k = a + 2; Tick(); return Leaf(k) + Other(k, a); }

    static uint Fact(uint n) { Tick(); return n == 0 ? 1 : n * Fact(n - 1); }

    static uint Shared(uint a) { uint r = a + 3; Tick(); return r; }
    static void OnIrq() { C64.FillMemory(0x0900UL, Shared(1), 1); }

    static void Main()
    {
        C64.Interrupt += OnIrq;
        uint x = Middle(4) + Leaf(1) + Fact(3);
        C64.FillMemory(0x0900UL, x, 1);
    }
}";

    [Test]
    public void Call_Graph_Decides_Which_Methods_Are_Static()
    {
        // --static-frames: the suite also runs with C64_STATIC_FRAMES=0.
        var gen = Generated(Compile(Source, "--static-frames"));

        // Main's local x, then Middle (a, k) above it; Leaf and Other are
        // both called from Middle (Leaf from Main too), so they overlay
        // each other right above Middle's frame.
        Assert.That(gen, Does.Match(@";\s+Program_Main: offset 0, 1 bytes"), gen);
        Assert.That(gen, Does.Match(@";\s+Program_Middle: offset 1, 2 bytes"), gen);
        Assert.That(gen, Does.Match(@";\s+Program_Leaf: offset 3, 1 bytes"), gen);
        Assert.That(gen, Does.Match(@";\s+Program_Other: offset 3, 2 bytes"), gen);
        Assert.That(gen, Does.Contain("StaticFramesSize = 5"), gen);

        Assert.That(gen, Does.Match(@"; dynamic \(recursive\): 1: Program_Fact"), gen);
        // The interrupt handler and everything it calls keep localsStack
        // frames: an interrupt can arrive while Shared is already running.
        Assert.That(gen, Does.Match(@"; dynamic \(delegate/interrupt reachable\): \d+: .*Program_OnIrq"), gen);
        Assert.That(gen, Does.Match(@"; dynamic \(delegate/interrupt reachable\): \d+: .*Program_Shared"), gen);
        // Tick is reachable from the interrupt too (Shared calls it).
        Assert.That(gen, Does.Match(@"; dynamic \(delegate/interrupt reachable\): \d+: .*Program_Tick"), gen);
    }

    [Test]
    public void Static_Method_Has_No_Prologue_And_Callers_Fill_Its_Slots()
    {
        var asm = File.ReadAllText(Path.Combine(Compile(Source, "--static-frames"), "Program.asm"));
        var middle = asm.Substring(asm.IndexOf("METHOD: Middle"));
        middle = middle.Substring(0, middle.IndexOf("METHOD:", 10));
        Assert.That(middle, Does.Contain("; static frame: StaticFrames+1, 2 bytes"), middle);
        Assert.That(middle, Does.Not.Contain("init_locals_pull_parameters"), middle);
        Assert.That(middle, Does.Contain("#sf_exit 1, []"), middle);
        Assert.That(middle, Does.Contain("jsr Program_Leaf"), middle);

        var fact = asm.Substring(asm.IndexOf("METHOD: Fact"));
        Assert.That(fact.Substring(0, fact.IndexOf("METHOD:", 10)), Does.Contain("init_locals_pull_parameters"));
    }

    [Test]
    public void Switch_Turns_Static_Frames_Off()
    {
        var gen = Generated(Compile(Source, "--no-static-frames"));
        Assert.That(gen, Does.Not.Contain("StaticFramesSize"), gen);
    }
}
