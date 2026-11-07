using C64Lib;
using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.RuntimeTest;

// C64.FillMemory / C64.CopyMemory (asm/C64.asm) touch exactly
// dest .. dest+size-1 (source .. source+size-1), size 1-255, 0 = 256.
// CopyMemory runs from the last byte down: an overlapping copy to a higher
// address is correct. (They used to touch offsets 1..size instead.)
[TestFixture]
public class MemoryRoutineTests
{
    const ulong Page = 0x0500UL;

    // Page offsets 0..n-1 = 0..n-1.
    static void Ramp(ulong address, uint n)
    {
        for (uint i = 0; i < n; i++)
            C64.FillMemory(address + i, i, 1);
    }

    [Test]
    public void FillMemory_Size_1_Writes_Only_The_Address()
    {
        C64.FillMemory(Page, 0, 0);
        C64.FillMemory(Page + 10UL, 5, 1);
        Assert.AreEqual(C64.GetMemory(Page, 9), 0u);
        Assert.AreEqual(C64.GetMemory(Page, 10), 5u);
        Assert.AreEqual(C64.GetMemory(Page, 11), 0u);
    }

    [Test]
    public void FillMemory_Writes_Address_To_Address_Plus_Size_Minus_One()
    {
        C64.FillMemory(Page, 0, 0);
        C64.FillMemory(Page + 20UL, 6, 100);
        Assert.AreEqual(C64.GetMemory(Page, 19), 0u);
        Assert.AreEqual(C64.GetMemory(Page, 20), 6u);
        Assert.AreEqual(C64.GetMemory(Page, 119), 6u);
        Assert.AreEqual(C64.GetMemory(Page, 120), 0u);
    }

    [Test]
    public void FillMemory_Size_255_And_Size_0_Means_256()
    {
        C64.FillMemory(Page, 0, 0);
        C64.FillMemory(Page + 0x100UL, 0, 0);
        C64.FillMemory(Page + 1UL, 7, 255);
        Assert.AreEqual(C64.GetMemory(Page, 0), 0u);
        Assert.AreEqual(C64.GetMemory(Page, 1), 7u);
        Assert.AreEqual(C64.GetMemory(Page, 255), 7u);
        Assert.AreEqual(C64.GetMemory(Page + 0x100UL, 0), 0u);

        // 256 bytes from offset 1: up to offset 0 of the next page, not 1.
        C64.FillMemory(Page + 1UL, 8, 0);
        Assert.AreEqual(C64.GetMemory(Page, 0), 0u);
        Assert.AreEqual(C64.GetMemory(Page, 1), 8u);
        Assert.AreEqual(C64.GetMemory(Page + 0x100UL, 0), 8u);
        Assert.AreEqual(C64.GetMemory(Page + 0x100UL, 1), 0u);
    }

    [Test]
    public void CopyMemory_Size_1_Copies_Only_The_First_Byte()
    {
        Ramp(Page, 4);
        C64.FillMemory(Page + 0x100UL, 0xEE, 4);
        C64.CopyMemory(Page + 0x101UL, Page + 2UL, 1);
        Assert.AreEqual(C64.GetMemory(Page + 0x100UL, 0), 0xEEu);
        Assert.AreEqual(C64.GetMemory(Page + 0x100UL, 1), 2u);
        Assert.AreEqual(C64.GetMemory(Page + 0x100UL, 2), 0xEEu);
    }

    [Test]
    public void CopyMemory_Copies_Source_Range_To_Dest_Range()
    {
        Ramp(Page, 50);
        C64.FillMemory(Page + 0x100UL, 0xEE, 50);
        C64.CopyMemory(Page + 0x101UL, Page + 10UL, 30);
        Assert.AreEqual(C64.GetMemory(Page + 0x100UL, 0), 0xEEu);
        Assert.AreEqual(C64.GetMemory(Page + 0x100UL, 1), 10u);
        Assert.AreEqual(C64.GetMemory(Page + 0x100UL, 30), 39u);
        Assert.AreEqual(C64.GetMemory(Page + 0x100UL, 31), 0xEEu);
    }

    [Test]
    public void CopyMemory_Size_0_Copies_256_Bytes()
    {
        for (ulong i = 0; i < 256UL; i++)   // uint is a byte here: ulong to reach 256
            C64.FillMemory(Page + i, (uint)i, 1);
        C64.FillMemory(Page + 0x100UL, 0xEE, 0);
        C64.FillMemory(Page + 0x200UL, 0xEE, 2);
        C64.CopyMemory(Page + 0x101UL, Page, 0);
        Assert.AreEqual(C64.GetMemory(Page + 0x100UL, 0), 0xEEu);
        Assert.AreEqual(C64.GetMemory(Page + 0x100UL, 1), 0u);
        Assert.AreEqual(C64.GetMemory(Page + 0x100UL, 255), 254u);
        Assert.AreEqual(C64.GetMemory(Page + 0x200UL, 0), 255u);
        Assert.AreEqual(C64.GetMemory(Page + 0x200UL, 1), 0xEEu);
    }

    [Test]
    public void CopyMemory_Overlapping_Copy_Up_Is_Correct()
    {
        Ramp(Page, 12);
        C64.CopyMemory(Page + 2UL, Page, 8);    // 0..7 -> offsets 2..9
        Assert.AreEqual(C64.GetMemory(Page, 0), 0u);
        Assert.AreEqual(C64.GetMemory(Page, 1), 1u);
        Assert.AreEqual(C64.GetMemory(Page, 2), 0u);
        Assert.AreEqual(C64.GetMemory(Page, 3), 1u);
        Assert.AreEqual(C64.GetMemory(Page, 9), 7u);
        Assert.AreEqual(C64.GetMemory(Page, 10), 10u);
    }
}
