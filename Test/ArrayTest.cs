using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.Test;

class MockObject
{
    public int F;
}

[TestFixture]
class ArrayTest
{
    [Test]
    public void Int_Array()
    {
        var sut = new int[5];
        Assert.AreEqual(sut.Length, 5);
        sut[1] = 6;
        Assert.AreEqual(sut[1], 6);
    }

    [Test]
    public void Int_Array_Postincrement_Index()
    {
        var sut = new int[3];
        int i = 0;
        sut[i++] = 10;
        sut[i++] = 20;
        Assert.AreEqual(sut[0], 10);
        Assert.AreEqual(sut[1], 20);
        Assert.AreEqual(i, 2);
    }

    [Test]
    public void Long_Array()
    {
        var sut = new long[5];
        Assert.AreEqual(sut.Length, 5);
        sut[0] = 12L;
        sut[3] = 13L;
        Assert.AreEqual((int)sut[0], 12);
        Assert.AreEqual((int)sut[3], 13);
    }

    [Test]
    public void Multiple_Int_Array()
    {
        var sut1 = new int[3];
        sut1[0] = sut1[1] = sut1[2] = 1;
        var sut2 = new int[3];
        sut2[0] = sut2[1] = sut2[2] = 2;
        Assert.AreEqual(sut1[0], 1);
        Assert.AreEqual(sut1[1], 1);
        Assert.AreEqual(sut1[2], 1);
        Assert.AreEqual(sut2[0], 2);
        Assert.AreEqual(sut2[1], 2);
        Assert.AreEqual(sut2[2], 2);
    }

    [Test]
    public void Int_Array_Initializer()
    {
        int a = 5;
        int b = 6;
        var sut = new int[] { a, b };
        Assert.AreEqual(sut.Length, 2);
        Assert.AreEqual(sut[1], 6);
    }

    [Test]
    public void Object_Array()
    {
        var sut = new MockObject[5];
        Assert.AreEqual(sut.Length, 5);
        sut[1] = new MockObject { F = 5 };
        Assert.AreEqual(sut[1].F, 5);
    }

    [Test]
    public void Object_Array_Initializer()
    {
        var sut = new MockObject[] { new MockObject { F = 12 }, new MockObject { F = 13 } };
        Assert.AreEqual(sut.Length, 2);
        Assert.AreEqual(sut[1].F, 13);
    }

    [Test]
    public void String_Array_Initializer()
    {
        var sut = new string[] { "a", "bb" };
        Assert.AreEqual(sut.Length, 2);
        Assert.AreEqual(sut[0].Length, 1);
        Assert.AreEqual(sut[1].Length, 2);
    }

    // uint[] element reads (Ldelem_u4) crashed Compiler.exe outright --
    // only Ldelem_i4/i8/ref were mapped in CommandMap. int[] reads
    // (Ldelem_i4) already worked, confirming the gap was specifically the
    // unsigned opcode, not array reads in general.
    [Test]
    public void Uint_Array()
    {
        var sut = new uint[3];
        sut[0] = 10;
        sut[1] = 20;
        sut[2] = 30;
        Assert.AreEqual(sut.Length, 3);
        Assert.AreEqual((int)sut[1], 20);
    }

    // A `static readonly <primitive>[] = { lit, lit, ... }` field, all
    // constant values, is exactly the shape Roslyn lowers to
    // Newarr;Dup;Ldtoken;Call RuntimeHelpers.InitializeArray;Stsfld inside
    // the type's .cctor -- which nothing used to run at all (no mechanism
    // called any type's static constructor), so the field silently stayed
    // a null handle forever. Deliberately declared at class scope with all-
    // constant elements (not `new int[] { a, b }` with variables, like
    // Int_Array_Initializer above -- that shape never triggers Roslyn's
    // blob-based InitializeArray optimization in the first place, so it
    // doesn't exercise this bug).
    private static readonly int[] ConstIntArray = { 91, 95, 92, 98, 100 };
    private static readonly uint[] ConstUintArray = { 10, 13, 250 };

    [Test]
    public void Static_Readonly_Int_Array_Literal()
    {
        Assert.AreEqual(ConstIntArray.Length, 5);
        Assert.AreEqual(ConstIntArray[0], 91);
        Assert.AreEqual(ConstIntArray[4], 100);
    }

    [Test]
    public void Static_Readonly_Uint_Array_Literal()
    {
        Assert.AreEqual(ConstUintArray.Length, 3);
        Assert.AreEqual((int)ConstUintArray[1], 13);
        Assert.AreEqual((int)ConstUintArray[2], 250);
    }

}