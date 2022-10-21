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

}