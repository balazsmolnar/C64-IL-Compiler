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
        // Offsets 0-2 = $34, offset 3 = $12: word 1 is $1234.
        C64.FillMemory(0x0400UL, 0x12, 4);
        C64.FillMemory(0x0400UL, 0x34, 3);
        if (C64.GetWord(0x0400UL, 1) != 0x1234UL)
            Assert.Fail();

        // Word 0 is offsets 0 and 1: $34 high, $56 low.
        C64.FillMemory(0x0400UL, 0x56, 1);
        if (C64.GetWord(0x0400UL, 0) != 0x3456UL)
            Assert.Fail();
    }
}
