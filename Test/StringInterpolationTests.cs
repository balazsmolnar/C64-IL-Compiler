using C64Lib;
using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.Test;

// Exercises Compiler/ILStringInterpolationPass.cs's rewrite of the
// System.Runtime.CompilerServices.DefaultInterpolatedStringHandler pattern
// Roslyn emits for $"..." into NumberFormat_*ToString + String_Concat calls
// (reusing exactly the same routines StringOpsTests.cs/ToStringTests.cs
// already exercise directly). Assert.AreEqualString for every case, same
// reasoning as StringOpsTests.cs: a computed string and a literal are
// almost never the same pointer even when their content matches.
//
// No test here deliberately triggers the pass's rejection paths (format
// specifiers/alignment, an unsupported hole type) -- see
// ILStringInterpolationPass.cs's own comment on why: the whole Test/*.cs
// project is compiled as a single batch before any NUnit test runs, so a
// test method that makes this pass throw would fail every test in the
// batch, not just itself. Verified manually instead during development.
[TestFixture]
public class StringInterpolationTests
{
    [Test]
    public void Interpolation_TwoHolesAndTrailingExpression()
    {
        int a = 2;
        int b = 3;
        int c = a + b;
        Assert.AreEqualString($"{a}+{b}={c}", "2+3=5");
    }

    // The hole itself is a computed expression, not just a simple load --
    // confirms the pass doesn't assume a hole's value-producing
    // instructions are a single instruction.
    [Test]
    public void Interpolation_ExpressionInHole()
    {
        int a = 2;
        int b = 3;
        Assert.AreEqualString($"{a + b}", "5");
    }

    [Test]
    public void Interpolation_SingleHoleNoLiteral()
    {
        int a = 42;
        Assert.AreEqualString($"{a}", "42");
    }

    [Test]
    public void Interpolation_LiteralAfterLastHole()
    {
        int a = 7;
        Assert.AreEqualString($"{a}X", "7X");
    }

    [Test]
    public void Interpolation_LeadingLiteral()
    {
        int a = 7;
        Assert.AreEqualString($"X{a}", "X7");
    }

    // Deliberately only ONE surrounding literal, not two ($"[{s}]" would
    // have three all-string pieces -- literal, hole, literal -- and Roslyn
    // lowers an all-string interpolated string straight to a multi-arg
    // string.Concat call instead of the handler pattern this pass rewrites;
    // see ILStringOpsPass.cs's own comment on why a 3+-arg Concat chain
    // isn't supported. A single hole plus a single literal stays a 2-arg
    // Concat, already supported there).
    [Test]
    public void Interpolation_StringHole()
    {
        string s = "foo";
        Assert.AreEqualString($"{s}!", "foo!");
    }

    // Mixed numeric types across holes in one string -- exercises
    // NumericToStringSupport.ConversionLabel's branching within a single
    // pattern instead of just a single type repeated.
    [Test]
    public void Interpolation_MixedNumericTypes()
    {
        uint u = 9;
        float f = 2.5f;
        Assert.AreEqualString($"{u}/{f}", "9/2.5");
    }

    // The actual planned real-world use: an interpolated result passed
    // straight into C64.Screen.Write, not compared directly -- same style
    // as StringOpsTests.cs's Concat_ThroughScreenWrite.
    [Test]
    public void Interpolation_ThroughScreenWrite()
    {
        uint level = 7;
        C64.Screen.Write(1, 1, $"L{level}");
        Assert.AreEqual(C64.Screen.GetChar(1, 1), 76); // 'L'
        Assert.AreEqual(C64.Screen.GetChar(2, 1), 55); // '7'
    }
}
