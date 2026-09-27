using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Compiler.UnitTests;

// What the compiler says when a C# program uses something it can't compile.
// Each test compiles a small C# snippet with Roslyn (to a real assembly and
// PDB), runs the real compiler driver on it, and checks the diagnostics: a
// stable code, a plain-language message, a hint, and the C# source line.
// Host-side only -- nothing is assembled or run on the emulator here.
[TestFixture]
public class DiagnosticsTests
{
    static int counter;

    // Compiles `source` (which may use C64Lib) and runs the C64 compiler on it.
    // Returns the exit code and everything the compiler printed.
    static (int exitCode, string output) CompileProgram(string source)
    {
        var dir = Path.Combine(Path.GetTempPath(), "c64_diag_tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var name = "DiagSnippet" + ++counter;
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
        return (exit, errors.ToString());
    }

    // The snippet is wrapped in a class with a Main; `Body` is the body of a
    // static uint Run() called from it. Types goes before the class.
    static string Wrap(string types, string body) => $@"using C64Lib;
{types}
class Program
{{
    static void Main()
    {{
        uint r = Run();
        C64.FillMemory(0x0900UL, r, 2);
    }}

    static uint Run()
    {{
{body}
    }}
}}";

    static string Errors(string types, string body)
    {
        var (exit, output) = CompileProgram(Wrap(types, body));
        Assert.That(exit, Is.EqualTo(1), "expected the compiler to reject this program; output:\n" + output);
        return output;
    }

    // A value the C# compiler cannot fold into a constant, so the variable it
    // initialises really is emitted as a local.
    const string Input = "class In { public static uint N = 5; }";

    // The C# compiler's IL for the same snippet differs between versions (a
    // local may or may not exist), so the same problem can be reported at the
    // declaration (C64003) or at the instruction (C64001/C64002): accept either.
    static void AssertReportsAny(string output, string text, string hint)
    {
        Assert.That(output, Does.Match(@"error C64(001|002|003):"), output);
        Assert.That(output, Does.Contain(text), output);
        Assert.That(output, Does.Contain(hint), output);
    }

    static void AssertReports(string output, string code, string text, string hint = null)
    {
        Assert.That(output, Does.Contain($"error {code}:"), output);
        Assert.That(output, Does.Contain(text), output);
        if (hint != null)
            Assert.That(output, Does.Contain(hint), output);
    }

    // --- instructions ---------------------------------------------------

    [Test]
    public void Type_Test_With_Is_Is_Rejected_With_A_Hint()
    {
        var output = Errors("class A { } class B : A { }", "A a = new B(); return a is B ? 1u : 0u;");
        AssertReports(output, "C64001", "type test ('is' or 'as')", "virtual method");
    }

    [Test]
    public void Class_Cast_Is_Rejected()
    {
        var output = Errors("class A { } class B : A { public uint Y; }", "A a = new B(); B b = (B)a; return b.Y;");
        AssertReports(output, "C64001", "cast between class types");
    }

    [Test]
    public void Try_Catch_Is_Rejected()
    {
        var output = Errors("", "try { return 1; } catch (System.Exception) { return 2; }");
        AssertReports(output, "C64001", "try/catch/finally", "check for the error condition");
    }

    [Test]
    public void Throw_Is_Rejected_As_An_Exception()
    {
        var output = Errors("", "uint x = 1; if (x == 2) throw new System.InvalidOperationException(); return 1;");
        Assert.That(output, Does.Contain("Exceptions are not supported"), output);
    }

    // --- types ------------------------------------------------------------

    [Test]
    public void Short_Local_Names_The_Type_With_Its_C_Sharp_Keyword()
    {
        var output = Errors(Input, "short s = (short)In.N; s += 1; return (uint)s;");
        AssertReportsAny(output, "short", "ulong");
    }

    [Test]
    public void Double_Local_Suggests_Float()
    {
        var output = Errors(Input, "double d = In.N; d = d * 1.5; return (uint)d;");
        AssertReportsAny(output, "double", "float");
    }

    [Test]
    public void Struct_Local_Suggests_A_Class()
    {
        var output = Errors("struct P { public uint X; }", "var p = new P(); p.X = 3; return p.X;");
        AssertReports(output, "C64003", "type 'P' (a struct)", "class");
    }

    [Test]
    public void Ref_Parameter_Is_Rejected()
    {
        var output = Errors("class H { public static void Inc(ref uint x) { x++; } }", "uint v = 4; H.Inc(ref v); return v;");
        AssertReports(output, "C64003", "by-reference");
    }

    [Test]
    public void Field_Of_An_Unsupported_Type_Is_Rejected()
    {
        var output = Errors("class H { public short Value; }", "return 1;");
        AssertReports(output, "C64003", "field 'Value' of type 'short'");
    }

    [Test]
    public void Func_With_Parameters_Is_Rejected()
    {
        var output = Errors("", "System.Func<uint, uint> f = x => x + 1; return f(4);");
        AssertReports(output, "C64003", "'Func<...>'", "Func<TResult>");
    }

    // --- declarations ------------------------------------------------------

    [Test]
    public void Interface_Call_Is_Rejected_Instead_Of_Silently_Miscompiled()
    {
        var output = Errors("interface I { uint A(); uint B(); } class C : I { public uint A() => 1; public virtual uint Z() => 9; public uint B() => 2; }",
            "I i = new C(); return i.B();");
        AssertReports(output, "C64004", "interface method 'I.B'", "abstract base class");
    }

    [Test]
    public void Generic_Method_Is_Rejected()
    {
        var output = Errors("class G { public static T Id<T>(T x) => x; }", "return G.Id<uint>(4);");
        AssertReports(output, "C64004", "generic method 'Id'");
    }

    [Test]
    public void Method_Of_A_Generic_Class_Is_Rejected()
    {
        var output = Errors("class Box<T> { public T V; public T Get() => V; }", "var b = new Box<uint>(); return b.Get();");
        AssertReports(output, "C64004", "method 'Get' of the generic class 'Box<...>'");
    }

    [Test]
    public void Constructor_With_A_Body_Points_At_Object_Initializers()
    {
        var output = Errors("class A { public uint X; public A(uint x) { X = x; } }", "var a = new A(5); return a.X;");
        AssertReports(output, "C64004", "constructor of 'A' does real work", "object initializer");
    }

    [Test]
    public void Field_Initializer_Is_Recognised_As_Constructor_Work()
    {
        var output = Errors("class A { public uint X = 7; }", "var a = new A(); return a.X;");
        AssertReports(output, "C64004", "field initializer");
    }

    // --- class library calls -------------------------------------------------

    [Test]
    public void String_Equality_Compiles_Successfully()
    {
        // s == "abc"/s != "abc" (and s == null/s != null, and the static
        // string.Equals(a, b) overload) are supported -- see
        // Test.Runtime/LanguageFeatureTests.cs for the actual emulator-run
        // checks of what they compute. Only the .Equals()/Compare()/
        // CompareOrdinal() instance-method shapes stay rejected, see
        // Instance_String_Equals_Explains_What_To_Do below.
        var (exit, output) = CompileProgram(Wrap("", "string s = \"abc\"; return s == \"abc\" ? 1u : 0u;"));
        Assert.That(exit, Is.EqualTo(0), output);
    }

    [Test]
    public void Instance_String_Equals_Explains_What_To_Do()
    {
        var output = Errors("", "string s = \"abc\"; string t = \"abd\"; return s.Equals(t) ? 1u : 0u;");
        AssertReports(output, "C64002", "'String.Equals'", "Only s == other");
    }

    [Test]
    public void Math_Max_Suggests_A_Ternary()
    {
        var output = Errors("", "return System.Math.Max(3u, 4u);");
        AssertReports(output, "C64002", "'Math.Max'", "(a > b ? a : b)");
    }

    [Test]
    public void List_Explains_That_Collections_Need_Generics()
    {
        var output = Errors("", "var l = new System.Collections.Generic.List<uint>(); l.Add(3); return l[0];");
        AssertReportsAny(output, "List", "Use arrays");
    }

    [Test]
    public void Multi_Dimensional_Array_Suggests_A_Jagged_Array()
    {
        var output = Errors("", "uint[,] g = new uint[3, 3]; return g[1, 2];");
        AssertReports(output, "C64002", "", "jagged array");
    }

    [Test]
    public void Invoking_A_Custom_Delegate_Is_Rejected()
    {
        var output = Errors("delegate uint D(uint x); class H { public static uint Dbl(uint x) => x * 2; }", "D d = H.Dbl; return d(4);");
        AssertReports(output, "C64002", "delegate type 'D'", "Func<TResult>");
    }

    // --- location and completeness ------------------------------------------

    [Test]
    public void The_Message_Names_The_C_Sharp_File_And_Line()
    {
        // The offending statement is on line 12 of the wrapped source.
        var source = Wrap("class A { } class B : A { }", "A a = new B();\n        return a is B ? 1u : 0u;");
        var line = source.Split('\n').ToList().FindIndex(l => l.Contains("a is B")) + 1;
        var (exit, output) = CompileProgram(source);
        Assert.That(exit, Is.EqualTo(1));
        Assert.That(output, Does.Contain($"snippet.cs({line}): error C64001:"), output);
        Assert.That(output, Does.Contain("(in Program.Run)"), output);
    }

    [Test]
    public void Every_Problem_Is_Reported_Not_Just_The_First()
    {
        var types = Input + " class Bad { public static uint A() { short s = (short)In.N; s += 1; return (uint)s; } public static uint B() { double d = In.N; d = d * 2; return (uint)d; } public static uint C() { return System.Math.Max(1u, 2u); } }";
        var (exit, output) = CompileProgram(Wrap(types, "return Bad.A() + Bad.B() + Bad.C();"));
        Assert.That(exit, Is.EqualTo(1));
        Assert.That(output, Does.Contain("short"), output);
        Assert.That(output, Does.Contain("double"), output);
        Assert.That(output, Does.Contain("Math.Max"), output);
        Assert.That(output, Does.Contain("Compilation failed: 3 error(s)."), output);
    }

    [Test]
    public void An_Error_Is_Never_A_Raw_Exception()
    {
        // Every rejection is a coded diagnostic, not "KeyNotFoundException" etc.
        // (checked arithmetic is one that stays unsupported: no overflow checks on the C64).
        var output = Errors("", "uint x = 5; return checked(x * 2);");
        Assert.That(output, Does.Not.Contain("Exception"), output);
        Assert.That(output, Does.Contain("error C64"), output);
    }

    // --- what must keep working -----------------------------------------------

    [Test]
    public void A_Program_Using_Only_Supported_Features_Compiles_Cleanly()
    {
        var types = @"
enum Kind { A, B, C }
abstract class Shape { public abstract uint Area(); }
class Sq : Shape { public uint S; public override uint Area() => S * S; }
static class Data { public static readonly uint[] Table = { 1, 2, 3 }; }
static class Util { public static uint Inc(uint a) { a = a + 1; return a; } }
";
        var body = @"
        var shape = new Sq { S = 3 };
        Shape other = shape;
        bool same = other == shape;
        bool notNull = other != null;
        uint k = 2;
        System.Func<uint> f = () => k + 1;
        uint area = shape.Area();
        string text = $""area={area}"";
        Kind kind = Kind.B;
        uint sum = 0;
        foreach (var v in Data.Table) sum += v;
        switch (kind) { case Kind.A: sum += 1; break; case Kind.B: sum += 2; break; }
        byte[] bytes = { 1, 2, 3 };
        bool[] flags = new bool[2];
        flags[1] = true;
        uint inverted = ~sum & 15u;
        return (same ? 1u : 0u) + (notNull ? 1u : 0u) + f() + (uint)text.Length + sum + bytes[2] + Util.Inc(1) + inverted + (flags[1] ? 1u : 0u);";
        var (exit, output) = CompileProgram(Wrap(types, body));
        Assert.That(exit, Is.EqualTo(0), output);
    }
}
