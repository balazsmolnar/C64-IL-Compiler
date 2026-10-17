using C64Lib;
using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.RuntimeTest;

// C64.GetWord(address, index): the 16-bit value (low byte first) at
// address + index * 2 -- asm/C64.asm's C64_GetWord.
[TestFixture]
public class GetWordTests
{
    [Test]
    public void GetWord_Reads_Low_Byte_First_At_Twice_The_Index()
    {
        // FillMemory writes offsets 1..size: after these two, offset 2 holds
        // $34 and offset 3 holds $12, i.e. word 1 is $1234.
        C64.FillMemory(0x0400UL, 0x12, 3);
        C64.FillMemory(0x0400UL, 0x34, 2);
        if (C64.GetWord(0x0400UL, 1) != 0x1234UL)
            Assert.Fail();

        // Word 0 is offsets 0 and 1: $34 high, whatever cleared byte low.
        C64.FillMemory(0x0400UL - 1UL, 0x56, 1);
        if (C64.GetWord(0x0400UL, 0) != 0x3456UL)
            Assert.Fail();
    }
}
