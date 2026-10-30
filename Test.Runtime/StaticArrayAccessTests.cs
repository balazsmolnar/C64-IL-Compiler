using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.RuntimeTest;

// ILMethodStaticArrayAccessOptimizer: reads and writes through a static
// array field with a constant / local / parameter / static-field index go
// straight to X and Y. Stores whose value expression is not a plain one
// (a call that changes the index) must still see the index as it was
// before the value was computed.
[TestFixture]
public class StaticArrayAccessTests
{
    static uint[] bytes;
    static ulong[] words;
    static uint index;

    static uint Bump()
    {
        index = index + 1;
        return 50;
    }

    static uint ReadAt(uint i) => bytes[i];
    static ulong ReadWordAt(uint i) => words[i];

    [Test]
    public void Byte_Elements_Read_And_Write_With_Every_Index_Kind()
    {
        bytes = new uint[8];
        bytes[0] = 7;                       // constant index, constant value
        uint i = 3;
        bytes[i] = 9;                       // local index
        index = 5;
        bytes[index] = 11;                  // static-field index
        bytes[i + 1] = bytes[i] + bytes[0]; // computed index (not fused), fused reads
        bytes[2] = (bytes[3] * 2 + 1) & 0xFF; // value expression between index and store
        Assert.AreEqual(ReadAt(0), 7u);
        Assert.AreEqual(ReadAt(3), 9u);
        Assert.AreEqual(ReadAt(5), 11u);
        Assert.AreEqual(ReadAt(4), 16u);
        Assert.AreEqual(ReadAt(2), 19u);
        Assert.AreEqual(bytes[index], 11u);
        Assert.AreEqual(bytes[i], 9u);
    }

    [Test]
    public void Wide_Elements_Read_And_Write()
    {
        words = new ulong[4];
        words[1] = 1000;
        uint i = 2;
        words[i] = words[1] + 234;
        index = 3;
        words[index] = 65000;
        Assert.IsTrue(ReadWordAt(1) == 1000);
        Assert.IsTrue(ReadWordAt(2) == 1234);
        Assert.IsTrue(ReadWordAt(3) == 65000);
        Assert.IsTrue(words[index] == 65000);
        Assert.IsTrue(words[i] + words[1] == 2234);
    }

    [Test]
    public void Store_Uses_The_Index_From_Before_A_Call_In_The_Value()
    {
        bytes = new uint[8];
        index = 1;
        bytes[index] = Bump();              // C#: index read first, then Bump()
        Assert.AreEqual(ReadAt(1), 50u);
        Assert.AreEqual(ReadAt(2), 0u);
        Assert.AreEqual(index, 2u);
    }
}
