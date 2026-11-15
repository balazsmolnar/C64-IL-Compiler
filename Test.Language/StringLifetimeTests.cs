using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.LanguageTest;

// A string built at run time lives in a shared scratch buffer: storing one
// in a field, static or array element is rejected at compile time (C64007,
// see Compiler/ILStringLifetimePass.cs and Compiler.UnitTests'
// DiagnosticsTests). What stays allowed must still work: building one in a
// helper, returning it, passing it to a method that only reads it, and
// keeping literals (not run-time strings) in fields.
class StringLabel
{
    public string Text = "none";
    public uint Number;

    // The number is kept; the text is built where it is used.
    public string Describe() => Text + Number.ToString();
}

[TestFixture]
public class StringLifetimeTests
{
    static string Make(uint n) => "S" + n.ToString();

    static string Pass(string s) => s;

    static uint Measure(string s) => (uint)s.Length;

    [Test]
    public void RunTimeStringReturnedAndReadRightAway()
    {
        Assert.AreEqualString(Pass(Make(42)), "S42");
    }

    [Test]
    public void RunTimeStringPassedToAReader()
    {
        Assert.AreEqual(Measure(Make(7) + "-x"), 4u);
    }

    [Test]
    public void LiteralsInFieldsAndTextBuiltOnUse()
    {
        var label = new StringLabel { Number = 3 };
        label.Text = "Slide ";
        var other = new StringLabel { Text = "Part ", Number = 9 };
        Assert.AreEqualString(label.Describe(), "Slide 3");
        Assert.AreEqualString(other.Describe(), "Part 9");
        Assert.AreEqualString(label.Text, "Slide ");
    }
}
