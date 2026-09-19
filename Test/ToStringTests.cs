using C64Lib;
using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.Test;

// Exercises Compiler/ILNumericToStringPass.cs's constrained./callvirt
// Object::ToString() rewrite (see that file's comment for the IL shape
// involved) against asm/helper/tostring.asm's four integer routines and
// asm/helper/float.asm's Float_ToString/NumberFormat_FloatToString.
// Assert.AreEqualString (not AreEqual) throughout -- a ToString() result
// and a string literal are almost never the same pointer even when their
// content matches, so only a real byte-by-byte comparison (Compiler/
// Templates/UnitTestEntry.asm's Assert_AreEqualString) can verify this.
[TestFixture]
public class ToStringTests
{
    [Test]
    public void UInt_ToString_Zero()
    {
        uint value = 0;
        Assert.AreEqualString(value.ToString(), "0");
    }

    [Test]
    public void UInt_ToString_Max()
    {
        uint value = 255;
        Assert.AreEqualString(value.ToString(), "255");
    }

    [Test]
    public void UInt_ToString_LeadingZeroSuppression()
    {
        uint value = 9;
        Assert.AreEqualString(value.ToString(), "9");
    }

    [Test]
    public void UInt_ToString_MiddleZero()
    {
        uint value = 105;
        Assert.AreEqualString(value.ToString(), "105");
    }

    [Test]
    public void Int_ToString_Zero()
    {
        int value = 0;
        Assert.AreEqualString(value.ToString(), "0");
    }

    [Test]
    public void Int_ToString_Positive()
    {
        int value = 127;
        Assert.AreEqualString(value.ToString(), "127");
    }

    [Test]
    public void Int_ToString_Negative()
    {
        int value = -1;
        Assert.AreEqualString(value.ToString(), "-1");
    }

    [Test]
    public void Int_ToString_Min()
    {
        int value = -128;
        Assert.AreEqualString(value.ToString(), "-128");
    }

    [Test]
    public void ULong_ToString_Zero()
    {
        ulong value = 0;
        Assert.AreEqualString(value.ToString(), "0");
    }

    [Test]
    public void ULong_ToString_Max()
    {
        ulong value = 65535;
        Assert.AreEqualString(value.ToString(), "65535");
    }

    [Test]
    public void ULong_ToString_MiddleZeros()
    {
        ulong value = 10005;
        Assert.AreEqualString(value.ToString(), "10005");
    }

    [Test]
    public void Long_ToString_Negative()
    {
        long value = -12345;
        Assert.AreEqualString(value.ToString(), "-12345");
    }

    [Test]
    public void Long_ToString_Min()
    {
        long value = -32768;
        Assert.AreEqualString(value.ToString(), "-32768");
    }

    [Test]
    public void Float_ToString_Positive()
    {
        float value = 5f;
        Assert.AreEqualString(value.ToString(), "5");
    }

    [Test]
    public void Float_ToString_Negative()
    {
        float value = -5f;
        Assert.AreEqualString(value.ToString(), "-5");
    }

    [Test]
    public void Float_ToString_Fraction()
    {
        float value = 3.5f;
        Assert.AreEqualString(value.ToString(), "3.5");
    }

    // A local, not a field/parameter -- ldloca.s (not ldarga.s or
    // ldsflda), the most common real-world shape (e.g. a computed
    // intermediate passed straight to Write()).
    [Test]
    public void ToString_OnExpressionResult()
    {
        uint a = 2;
        uint b = 3;
        uint sum = a + b;
        Assert.AreEqualString(sum.ToString(), "5");
    }

    private class Holder
    {
        public uint Value;
    }

    // ldflda (address of an instance field), not ldloca.s/ldarga.s --
    // Hunchback/PlayerStats.cs's Score.ToString() is exactly this shape
    // (Score is an instance field), which crashed compilation entirely
    // (KeyNotFoundException: 'Ldflda') until CommandMap.cs registered it.
    [Test]
    public void ToString_OnInstanceField()
    {
        var holder = new Holder { Value = 42 };
        Assert.AreEqualString(holder.Value.ToString(), "42");
    }

    // The actual planned real-world use: a ToString() result passed
    // straight into C64.Screen.Write, not compared directly -- confirms the
    // full round trip (conversion routine -> shared tostring_buffer ->
    // Screen_Write's own string-pointer handling) by reading the rendered
    // character back off the screen.
    [Test]
    public void ToString_ThroughScreenWrite()
    {
        uint u = 200;
        C64.Screen.Write(1, 1, u.ToString());
        Assert.AreEqual(C64.Screen.GetChar(1, 1), 50); // '2'
        Assert.AreEqual(C64.Screen.GetChar(2, 1), 48); // '0'
        Assert.AreEqual(C64.Screen.GetChar(3, 1), 48); // '0'
    }
}
