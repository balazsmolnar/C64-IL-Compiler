using C64Lib;
using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.Test;

// Exercises Compiler/ILStringOpsPass.cs's String.Concat(string,string)/
// .Length/.PadLeft(int,char) rewrite against asm/helper/stringops.asm.
// Assert.AreEqualString (not AreEqual) for every string-returning case --
// same reasoning as ToStringTests.cs: a computed string and a literal are
// almost never the same pointer even when their content matches.
[TestFixture]
public class StringOpsTests
{
    // Both operands are non-const locals (not literals) so Roslyn actually
    // emits a runtime String.Concat call instead of constant-folding the
    // whole expression into one literal at compile time -- a genuine risk
    // here since "foo" + "bar" (both literals) WOULD fold away and this
    // test would silently stop exercising String_Concat at all.
    [Test]
    public void Concat_TwoLocals()
    {
        string a = "foo";
        string b = "bar";
        Assert.AreEqualString(a + b, "foobar");
    }

    [Test]
    public void Concat_LiteralAndToString()
    {
        uint level = 3;
        // The actual motivating Hunchback pattern (LevelPlay.cs's
        // DrawDebugLevelNumber): a literal prefix plus a ToString() result
        // that's still sitting in tostring_buffer when Concat runs --
        // confirms stringops_buffer really is a separate buffer (see its
        // comment in objectTables.asm) and doesn't alias/corrupt that input.
        Assert.AreEqualString("L" + level.ToString(), "L3");
    }

    [Test]
    public void Concat_EmptyFirstOperand()
    {
        string a = "";
        string b = "x";
        Assert.AreEqualString(a + b, "x");
    }

    [Test]
    public void Concat_EmptySecondOperand()
    {
        string a = "x";
        string b = "";
        Assert.AreEqualString(a + b, "x");
    }

    // The test harness (RunInEmulatorAspect.CopyMethodArgumentsToEmulator)
    // has no marshaling case for a string TestCase argument, so these use
    // local variables (not parameters) -- same reason ToStringTests.cs never
    // parameterizes on string either. A non-const local also avoids Roslyn
    // constant-folding "literal".Length away at compile time the way it
    // would for `"hello".Length` used directly.
    [Test]
    public void Length_Empty()
    {
        string s = "";
        Assert.AreEqual((uint)s.Length, 0u);
    }

    [Test]
    public void Length_OneChar()
    {
        string s = "a";
        Assert.AreEqual((uint)s.Length, 1u);
    }

    [Test]
    public void Length_MultiChar()
    {
        string s = "hello";
        Assert.AreEqual((uint)s.Length, 5u);
    }

    [Test]
    public void Length_OnConcatResult()
    {
        string a = "foo";
        string b = "bar";
        string s = a + b;
        Assert.AreEqual((uint)s.Length, 6u);
    }

    [Test]
    public void PadLeft_ShorterThanWidth()
    {
        Assert.AreEqualString("3".PadLeft(5, '0'), "00003");
    }

    [Test]
    public void PadLeft_AlreadyAtWidth()
    {
        Assert.AreEqualString("12345".PadLeft(5, '0'), "12345");
    }

    [Test]
    public void PadLeft_LongerThanWidth()
    {
        Assert.AreEqualString("123456".PadLeft(5, '0'), "123456");
    }

    [Test]
    public void PadLeft_OnToStringResult()
    {
        // The actual motivating Hunchback pattern (LevelPlay.cs's
        // DrawZeroPaddedScore).
        ulong points = 42;
        Assert.AreEqualString(points.ToString().PadLeft(5, '0'), "00042");
    }

    [Test]
    public void PadLeft_ZeroWidth()
    {
        Assert.AreEqualString("x".PadLeft(0, '0'), "x");
    }

    // The actual planned real-world use: a Concat()/PadLeft() result passed
    // straight into C64.Screen.Write, not compared directly -- confirms the
    // full round trip through stringops_buffer by reading the rendered
    // characters back off the screen (same style as ToStringTests.cs's
    // ToString_ThroughScreenWrite).
    [Test]
    public void Concat_ThroughScreenWrite()
    {
        uint level = 7;
        C64.Screen.Write(1, 1, "L" + level.ToString());
        Assert.AreEqual(C64.Screen.GetChar(1, 1), 76); // 'L'
        Assert.AreEqual(C64.Screen.GetChar(2, 1), 55); // '7'
    }
}
