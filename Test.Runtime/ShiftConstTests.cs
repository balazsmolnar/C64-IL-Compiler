using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.RuntimeTest;

// ILMethodMulConstOptimizer: x << n and x >> n by a constant 1..7 and
// multiplication by any power of two up to 128 are unrolled shifts. Checked
// against the general routines (count / factor passed as a parameter, so
// the compiler cannot see it) over every 8-bit value.
[TestFixture]
public class ShiftConstTests
{
    static uint Shl(uint x, int n) => x << n;
    static uint Shr(uint x, int n) => x >> n;
    static uint Mul(uint x, uint m) => x * m;
    static ulong Mul16(ulong x, ulong m) => x * m;

    [Test]
    public void Constant_Shifts_Match_Variable_Shifts()
    {
        uint bad = 0;
        for (uint x = 0; x < 256; x++)
        {
            if (((x << 1) & 255) != (Shl(x, 1) & 255)) bad++;
            if (((x << 3) & 255) != (Shl(x, 3) & 255)) bad++;
            if (((x << 7) & 255) != (Shl(x, 7) & 255)) bad++;
            if ((x >> 1) != Shr(x, 1)) bad++;
            if ((x >> 2) != Shr(x, 2)) bad++;
            if ((x >> 5) != Shr(x, 5)) bad++;
            if ((x >> 7) != Shr(x, 7)) bad++;
            if (((x * 8) & 255) != (Mul(x, 8) & 255)) bad++;
            if (((x * 32) & 255) != (Mul(x, 32) & 255)) bad++;
            if (((x * 128) & 255) != (Mul(x, 128) & 255)) bad++;
        }
        Assert.AreEqual(bad, 0u);
    }

    [Test]
    public void Wide_Multiply_By_Eight_Matches()
    {
        uint bad = 0;
        ulong x = 0;
        for (uint i = 0; i < 250; i++)
        {
            if (x * 8 != Mul16(x, 8)) bad++;
            x = x + 211;
        }
        Assert.AreEqual(bad, 0u);
    }
}
