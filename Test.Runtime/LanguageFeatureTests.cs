using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.RuntimeTest;

// C# constructs that compile to real 6502 code and run on the emulator, as
// opposed to being rejected with a diagnostic (see Compiler.UnitTests'
// DiagnosticsTests for those). Each of these cost nothing for a program that
// doesn't use it: they map onto existing operations or onto macros that only
// take space where they are expanded. Lives in this project because
// Test/'s single shared program is at its memory ceiling.
class Thing
{
    public uint Id;
}

[TestFixture]
public class LanguageFeatureTests
{
    static uint AddOne(uint a)
    {
        a = a + 1; // assigning to a parameter
        return a;
    }

    static uint ReplaceReference(uint[] items)
    {
        items = new uint[2]; // a reference parameter: the caller's array must be untouched
        items[0] = 9;
        return items[0];
    }

    [Test]
    public void Byte_Array_Stores_And_Loads_Elements()
    {
        byte[] a = new byte[4];
        a[1] = 200;
        a[3] = 7;
        Assert.AreEqual((uint)a[1], 200u);
        Assert.AreEqual((uint)a[3], 7u);
        Assert.AreEqual((uint)a[0], 0u);
        Assert.AreEqual((uint)(a[1] + a[3]), 207u);
    }

    [Test]
    public void Byte_Array_Literal_Is_Initialised_With_Its_Values()
    {
        byte[] table = { 10, 20, 30 };
        Assert.AreEqual((uint)(table[0] + table[1] + table[2]), 60u);
        Assert.AreEqual(table.Length, 3);
    }

    [Test]
    public void Bool_Array_Elements_Can_Be_Set_And_Tested()
    {
        bool[] flags = new bool[3];
        flags[1] = true;
        Assert.IsFalse(flags[0]);
        Assert.IsTrue(flags[1]);
        Assert.IsFalse(flags[2]);
    }

    [Test]
    public void Byte_Local_Wraps_At_256()
    {
        byte b = 200;
        b += 100;
        Assert.AreEqual((uint)b, 44u);
    }

    [Test]
    public void Casting_A_16_Bit_Value_To_Byte_Keeps_The_Low_Byte()
    {
        ulong wide = 300;
        byte narrow = (byte)wide;
        Assert.AreEqual((uint)narrow, 44u);
    }

    [Test]
    public void Bitwise_Not_Of_An_8_Bit_Value()
    {
        uint x = 5;
        Assert.AreEqual(~x & 15u, 10u);
    }

    [Test]
    public void Bitwise_Not_Of_A_16_Bit_Value()
    {
        ulong y = 0x00F0;
        ulong notY = ~y & 0xFFFFUL;
        Assert.IsTrue(notY == 0xFF0FUL);
    }

    // Unary minus on a long/ulong used a macro that moved only one byte, so the
    // high byte was lost and the result was wrong.
    [Test]
    public void Negating_A_Long_Keeps_Its_High_Byte()
    {
        long a = 300;
        long b = -a;
        Assert.AreEqual((uint)((b + 1000L) / 10L), 70u);
    }

    [Test]
    public void A_Parameter_Can_Be_Assigned_To()
    {
        Assert.AreEqual(AddOne(4), 5u);
    }

    [Test]
    public void Assigning_A_New_Object_To_A_Reference_Parameter_Leaves_The_Callers_Object_Alone()
    {
        uint[] original = new uint[1];
        original[0] = 3;
        Assert.AreEqual(ReplaceReference(original), 9u);
        Assert.AreEqual(original[0], 3u);
    }

    [Test]
    public void Object_References_Can_Be_Compared_And_Tested_For_Null_As_Values()
    {
        var a = new Thing();
        var b = a;
        var c = new Thing();
        Thing nothing = null;

        bool same = a == b;
        bool different = a != c;
        bool notNull = a != null;
        bool isNull = nothing == null;
        bool notSame = a == c;

        Assert.IsTrue(same);
        Assert.IsTrue(different);
        Assert.IsTrue(notNull);
        Assert.IsTrue(isNull);
        Assert.IsFalse(notSame);
    }

    [Test]
    public void Array_References_Can_Be_Tested_For_Null_As_Values()
    {
        uint[] missing = null;
        bool wasNull = missing == null;
        missing = new uint[2];
        bool isSet = missing != null;
        Assert.IsTrue(wasNull);
        Assert.IsTrue(isSet);
    }
}
