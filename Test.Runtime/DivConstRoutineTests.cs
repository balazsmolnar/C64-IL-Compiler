using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.RuntimeTest;

// Unsigned division and remainder by a constant (ILMethodDivConstOptimizer):
// divisors 3..255 call a routine specialised for the divisor
// (DivConst8_<d>/DivConst16_<d>), powers of two shift or mask. Checked
// against the general routines, reached through a divisor the compiler
// cannot see (a parameter), over every 8-bit dividend and a spread of
// 16-bit ones.
[TestFixture]
public class DivConstRoutineTests
{
    static uint Div8(uint x, uint d) => x / d;
    static uint Rem8(uint x, uint d) => x % d;
    static ulong Div16(ulong x, ulong d) => x / d;
    static ulong Rem16(ulong x, ulong d) => x % d;

    [Test]
    public void Byte_Division_By_Constants_Matches_General_Division()
    {
        uint bad = 0;
        for (uint x = 0; x < 256; x++)
        {
            if (x / 3 != Div8(x, 3)) bad++;
            if (x % 3 != Rem8(x, 3)) bad++;
            if (x / 7 != Div8(x, 7)) bad++;
            if (x / 10 != Div8(x, 10)) bad++;
            if (x % 10 != Rem8(x, 10)) bad++;
            if (x / 100 != Div8(x, 100)) bad++;
            if (x / 200 != Div8(x, 200)) bad++;
            if (x % 200 != Rem8(x, 200)) bad++;
            if (x / 255 != Div8(x, 255)) bad++;
            if (x / 16 != Div8(x, 16)) bad++;
            if (x % 16 != Rem8(x, 16)) bad++;
            if (x / 128 != Div8(x, 128)) bad++;
            if (x % 128 != Rem8(x, 128)) bad++;
        }
        Assert.AreEqual(bad, 0u);
    }

    [Test]
    public void Wide_Division_By_Constants_Matches_General_Division()
    {
        uint bad = 0;
        ulong x = 0;
        for (uint i = 0; i < 700; i++)
        {
            if (x / 3 != Div16(x, 3)) bad++;
            if (x % 3 != Rem16(x, 3)) bad++;
            if (x / 10 != Div16(x, 10)) bad++;
            if (x % 10 != Rem16(x, 10)) bad++;
            if (x / 100 != Div16(x, 100)) bad++;
            if (x % 100 != Rem16(x, 100)) bad++;
            if (x / 200 != Div16(x, 200)) bad++;
            if (x / 255 != Div16(x, 255)) bad++;
            if (x % 255 != Rem16(x, 255)) bad++;
            if (x / 16 != Div16(x, 16)) bad++;
            if (x % 64 != Rem16(x, 64)) bad++;
            if (x % 256 != Rem16(x, 256)) bad++;
            x = x + 93;
        }
        Assert.AreEqual(bad, 0u);
    }

    [Test]
    public void Wide_Division_By_Constant_At_The_Top_Of_The_Range()
    {
        ulong x = 65535;
        Assert.IsTrue(x / 100 == 655);
        Assert.IsTrue(x % 100 == 35);
        Assert.IsTrue(x / 3 == 21845);
        Assert.IsTrue(x / 255 == 257);
        Assert.IsTrue(x % 255 == 0);
        ulong y = 65534;
        Assert.IsTrue(y / 255 == 256);
        Assert.IsTrue(y % 255 == 254);
    }
}
