using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Compiler.UnitTests;

// Structural tests for the generalization of ILPropertyGettterOptimizer
// (reads) and ILSetFieldOptimizer (writes) beyond `this` specifically --
// any reference held in a local or parameter now gets the same direct-
// resolve fusion (OpPushFldAt/OpSetfld/OpSetfldConst), instead of only
// `this.field` reads and `this.field = parameter;` writes fusing.
//
// This generalization exposed a real, serious bug in this session's
// earlier float/int operand fusion work: pushfldflt_direct/
// pushfld8_direct/pushfld16_direct (asm/helper/optimized.asm) were
// hardcoded to relpos 1 ("this"), safe only because every
// IPushFldOperation those passes could ever see used to be `this`-only.
// Once ILPropertyGettterOptimizer could also build an OpPushFldAt for an
// arbitrary object, those macros silently read the wrong object instead
// -- caught for real via Test/FloatTests.cs's own TestField
// (`obj.Value = obj.Value + 1f;`, obj being a local) returning 1.0f
// instead of 4.5f. Fixed by giving all three an explicit \relpos
// parameter (same addressing pushfld8_at already used) and passing
// IPushFldOperation.RelPos through from Compiler/Operands/
// OperandFloatFusion.cs and OperandIntFusion.cs. See
// Test/FloatTests.cs's TestField for the permanent real-correctness
// regression guard (run via SimpleEmulator); the tests here check the
// generalization's own asm shape structurally.
[TestFixture]
public class FieldAccessGeneralizationTests
{
    static int counter;

    static string CompileAndGetAsmDir(string source)
    {
        var dir = Path.Combine(Path.GetTempPath(), "c64_field_access_gen_tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var name = "FieldAccessGen" + ++counter;
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
    public void Read_Through_Local_Object_Fuses_With_Correct_Relpos()
    {
        // A SECOND parameter (Ldarg_1), not the first -- ILPropertyGettterOptimizer's
        // existing short rule already handled a method's own first
        // parameter even before this session's change (it matches
        // Ldarg_0 specifically, which for a static method is just
        // "whatever the first parameter is", `this`-ness aside), so a
        // first-parameter Holder wouldn't actually exercise the NEW,
        // generalized rule (OpPushFldAt) at all -- found while writing
        // this test. A second parameter can only ever be matched by the
        // new rule.
        var source = @"
class Holder
{
    public uint a_;
}
class Program
{
    static uint Sum(uint dummy, Holder h) => h.a_ + h.a_;
    static void Main()
    {
        var h = new Holder();
        h.a_ = 7u;
        uint x = Sum(0u, h);
        x = Sum(x, h);
        C64Lib.C64.FillMemory(0x0900UL, x, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        // h's own rel_pos as Sum's SECOND parameter is 2. Asserting the
        // literal value catches a relpos mixup (e.g. falling back to a
        // hardcoded 1/"this") structurally, not just "some number
        // appeared" -- confirmed against the actual generated rel_pos,
        // not assumed.
        Assert.That(programAsm, Does.Contain("pushfld8_at 2, 0"),
            "the parameter's own real rel_pos should be used, not a hardcoded this-relpos:\n" + programAsm);
    }

    [Test]
    public void Write_Through_Local_Object_With_Leaf_Value_Fuses()
    {
        // Helper() -- a same-assembly call -- is deliberately here to keep
        // Main disqualified from ILMethodPromoteLocalsPass (see
        // IsSafeExternalCall's own comment: a same-assembly target is
        // never automatically safe). Without it, Main would now ALSO
        // qualify (its only other call, FillMemory, is external) and `v`
        // -- its sole eligible local -- would get promoted to zp_local0,
        // which is exactly the OTHER, correct behavior
        // Write_Through_Local_Object_With_Promoted_Value_Does_Not_Fuse
        // below checks for. Helper() itself calls FillMemory too (not
        // left empty) deliberately: a truly empty/trivial callee gets
        // substituted away by ILMethodInliningPass, which REPLACES the
        // original Call opcode with a synthesized Nop-tagged prologue op
        // rather than merely marking the old Call line dead while keeping
        // its opcode -- PromoteLocalsPass would then never see a Call
        // opcode for it at all, defeating the point. A callee that itself
        // contains a Call is categorically excluded from InlineCandidates
        // (ILCallSiteCountPass: "recursion and nested inlining are both
        // out of scope"), so THIS call site is guaranteed to stay a real,
        // live, un-inlined Call in Main's own Lines.
        var source = @"
class Holder
{
    public uint a_;
}
class Program
{
    static void Helper() { C64Lib.C64.FillMemory(0x0A00UL, 0, 1); }
    static void Main()
    {
        var h = new Holder();
        uint v = 9u;
        h.a_ = v;
        Helper();
        C64Lib.C64.FillMemory(0x0900UL, h.a_, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        Assert.That(programAsm, Does.Not.Contain("zp_local"), "v should not be promoted in this test -- that's the OTHER test below:\n" + programAsm);
        Assert.That(programAsm, Does.Contain("#setfld8"), "the write through a local should fuse via setfld8:\n" + programAsm);
        Assert.That(programAsm, Does.Not.Contain("\n    #stfld8 "),
            "the unfused stfld8 macro should not run as real code once fused:\n" + programAsm);
    }

    // Companion to the test above, for the interaction ILMethodPromoteLocalsPass's
    // Call/Callvirt-to-external-assembly relaxation newly made reachable:
    // a method whose ONLY calls are external (here, just FillMemory) now
    // qualifies for local promotion, so `v` -- its sole eligible local --
    // gets promoted to zp_local0. ILSetFieldOptimizer's IsPromotedLocal
    // guard (see its own comment) must then decline to fuse "h.a_ = v;"
    // (OpSetfld has no zp-aware variant), falling through to the general,
    // unfused stfld8 + zp_push_value8 path instead of silently reading
    // stale localsStack memory.
    [Test]
    public void Write_Through_Local_Object_With_Promoted_Value_Does_Not_Fuse()
    {
        var source = @"
class Holder
{
    public uint a_;
}
class Program
{
    static void Main()
    {
        var h = new Holder();
        uint v = 9u;
        h.a_ = v;
        C64Lib.C64.FillMemory(0x0900UL, h.a_, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        Assert.That(programAsm, Does.Contain("zp_local0"), "v should be promoted now that Main's only call is external:\n" + programAsm);
        Assert.That(programAsm, Does.Contain("\n    #stfld8 "),
            "fusion must decline for a promoted value -- OpSetfld has no zp-aware variant:\n" + programAsm);
        Assert.That(programAsm, Does.Not.Contain("#setfld8"),
            "must not silently fuse a promoted local's stale localsStack slot into setfld8:\n" + programAsm);
    }

    [Test]
    public void Write_Through_Local_Object_With_8Bit_Constant_Fuses()
    {
        var source = @"
class Holder
{
    public uint a_;
}
class Program
{
    static void Main()
    {
        var h = new Holder();
        h.a_ = 42u;
        C64Lib.C64.FillMemory(0x0900UL, h.a_, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        Assert.That(programAsm, Does.Contain("setfld8_const"), "a constant value write should fuse via setfld8_const:\n" + programAsm);
        Assert.That(programAsm, Does.Not.Contain("\n    #stfld8 "),
            "the unfused stfld8 macro should not run as real code once fused:\n" + programAsm);
    }

    [Test]
    public void Write_Through_Local_Object_With_Widened_16Bit_Constant_Fuses()
    {
        var source = @"
class Holder
{
    public ulong a_;
}
class Program
{
    static void Main()
    {
        var h = new Holder();
        h.a_ = 1000ul;
        C64Lib.C64.FillMemory(0x0900UL, (uint)h.a_, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        Assert.That(programAsm, Does.Contain("setfld16_const"), "a widened 16-bit constant write should fuse via setfld16_const:\n" + programAsm);
        Assert.That(programAsm, Does.Not.Contain("\n    #stfld16 "),
            "the unfused stfld16 macro should not run as real code once fused:\n" + programAsm);
    }

    // Guard against the exact regression this generalization introduced
    // and then fixed: a float field written through ANY object (this or
    // not) must fall through to the general, unfused (but float-aware)
    // Stfld path -- OpSetfld/setfld8/setfld16 have no float variant at
    // all, so fusing this would silently write only 1 of the field's 5
    // bytes.
    [Test]
    public void Write_Float_Field_Through_Local_Object_Is_Not_Fused()
    {
        var source = @"
class Holder
{
    public float a_;
}
class Program
{
    static void Main()
    {
        var h = new Holder();
        float v = 2.5f;
        h.a_ = v;
        C64Lib.C64.FillMemory(0x0900UL, (uint)h.a_, 1);
    }
}";
        var asmDir = CompileAndGetAsmDir(source);
        var programAsm = ReadType(asmDir, "Program");

        Assert.That(programAsm, Does.Not.Contain("#setfld8"),
            "a float field write must never go through OpSetfld -- it has no float width concept at all:\n" + programAsm);
        Assert.That(programAsm, Does.Contain("#stfldflt"),
            "should still compile correctly via the general, float-aware Stfld path:\n" + programAsm);
    }
}
