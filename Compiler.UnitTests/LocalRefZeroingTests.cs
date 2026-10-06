using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Compiler.UnitTests;

// Structural tests for the prologue zeroing fix
// (CompilerMethodContext.GetLocalRefPositions,
// asm/helper/localsStack.asm's init_locals_pull_parameters/
// init_locals_pull_parameters_inline) -- see
// Test.Runtime/StaleLocalsStackRefCountTests.cs for the real,
// reproduced-and-fixed bug this exists to prevent and the runtime
// correctness side of this. These only check WHAT GETS EMITTED: a
// method with a reference-typed local must get the zeroing code, and
// -- just as important, matching this project's own "pay only for what
// you use" rule -- a method with NO reference-typed locals must get
// NONE of it (not even an empty, harmless no-op).
[TestFixture]
public class LocalRefZeroingTests
{
    static int counter;

    static string CompileAndGetAsmDir(string source)
    {
        var dir = Path.Combine(Path.GetTempPath(), "c64_local_ref_zeroing_tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var name = "LocalRefZeroing" + ++counter;
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
    public void Method_With_RefTyped_Local_Gets_Nonempty_Local_Ref_List()
    {
        // `local` is RETURNED (not just written to then dropped), so it
        // must genuinely survive across the branch merge below --
        // forces Roslyn to keep a real local rather than eliding it
        // (release-mode Roslyn folds away a local used only once, the
        // same elision this project's own session history already hit
        // writing "var player = new Player(); uint r = player.Sum();"
        // style snippets).
        var source = @"
class Holder { public int Id; }
class Program
{
    static Holder UseLocal(bool flag)
    {
        Holder local = new Holder();
        if (flag)
            local.Id = 1;
        else
            local.Id = 2;
        return local;
    }
    static void Main()
    {
        var a = UseLocal(true);
        var b = UseLocal(false);
        C64Lib.C64.FillMemory(0x0900UL, (uint)(a.Id + b.Id), 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        // rel_pos 2, not 1: `flag` (the bool parameter) occupies
        // position 1, `local` is the next slot out at position 2 --
        // observed directly, not guessed.
        Assert.That(programAsm, Does.Contain("init_locals_pull_parameters 1, [0], [2]"),
            "a method with one reference-typed local (and one bool param) must pass a non-empty local_ref_list naming its position:\n" + programAsm);
    }

    [Test]
    public void Method_With_Only_ValueTyped_Locals_Gets_Empty_Local_Ref_List()
    {
        var source = @"
class Program
{
    static uint UseLocal(bool flag)
    {
        uint local = flag ? 1u : 2u;
        return local + 1;
    }
    static void Main()
    {
        var r = UseLocal(true) + UseLocal(false);
        C64Lib.C64.FillMemory(0x0900UL, r, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        // localsSize 0, not 1: Roslyn elides `local` entirely here (used
        // only once, same elision the other tests in this file work
        // around) -- but the POINT of this test (an empty local_ref_list
        // when nothing reference-typed exists) still holds regardless of
        // whether Roslyn happens to keep a real value-typed local slot
        // or not, so this is observed, not fought.
        Assert.That(programAsm, Does.Contain("init_locals_pull_parameters 0, [0], []"),
            "a method with only value-typed locals must pass an EMPTY local_ref_list, not merely a correct one:\n" + programAsm);
    }

    [Test]
    public void Method_With_No_Locals_At_All_Gets_Empty_Local_Ref_List()
    {
        // A computed expression argument (x + x), not a bare literal or
        // a bare Ldarg/Ldloc: either of those would qualify every call
        // site for trivial-argument substitution (ILMethodInliningPass
        // -- constant OR variable producers both splice it away
        // unconditionally, regardless of call count), leaving no
        // standalone definition to assert against -- confirmed directly,
        // earlier drafts of this test using AddOne(1)/AddOne(2) and then
        // AddOne(x)/AddOne(y) each hit exactly that for a different one
        // of the two producer kinds TryBuildTrivialSubstitution accepts.
        var source = @"
class Program
{
    static uint AddOne(uint v) => v + 1;
    static void Main()
    {
        uint x = 1;
        uint y = 2;
        var r = AddOne(x + x) + AddOne(y + y);
        C64Lib.C64.FillMemory(0x0900UL, r, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        Assert.That(programAsm, Does.Contain("init_locals_pull_parameters 0, [0], []"),
            "a method with no locals at all must still pass an empty local_ref_list (nothing to zero):\n" + programAsm);
    }

    [Test]
    public void Materializing_Inline_Splice_With_RefTyped_Local_Gets_Nonempty_Local_Ref_List()
    {
        // A genuine if/else merge point, same as
        // Method_With_RefTyped_Local_Gets_Nonempty_Local_Ref_List above
        // -- confirmed necessary, not just reading two fields off it
        // (Roslyn can still Dup-fold a straight-line "build it, read two
        // fields off it, return it" sequence with no real Stloc at all,
        // same elision as everywhere else in this file, an earlier draft
        // of exactly this test hit it). A real branch forces the object
        // reference to actually survive as a local across the merge.
        var source = @"
using System.Runtime.CompilerServices;
class Holder { public int A; }
class Program
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Holder UseLocal(bool flag)
    {
        Holder local = new Holder();
        if (flag)
            local.A = 1;
        else
            local.A = 2;
        return local;
    }
    static void Main()
    {
        var h = UseLocal(true);
        C64Lib.C64.FillMemory(0x0900UL, (uint)h.A, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        Assert.That(programAsm, Does.Contain("init_locals_pull_parameters_inline"),
            "UseLocal has a local variable (disqualifying trivial substitution) and a single call site with AggressiveInlining, so it should materialize-inline:\n" + programAsm);
        // rel_pos 2, not 1: `flag` occupies position 1, `local` is the
        // next slot out at position 2 -- same reasoning, observed the
        // same way, as Method_With_RefTyped_Local_Gets_Nonempty_Local_Ref_List
        // above.
        Assert.That(programAsm, Does.Contain("init_locals_pull_parameters_inline 1, [0], [2]"),
            "the materializing splice's own inline prologue must zero UseLocal's reference-typed local:\n" + programAsm);
    }
}
