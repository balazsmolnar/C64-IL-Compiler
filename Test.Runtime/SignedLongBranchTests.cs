using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.RuntimeTest;

// Exercises the 16-bit SIGNED relational branch macros just added to
// asm/helper/branch.asm (branch_less16/branch_less_equal16/
// branch_greater16/branch_greater_equal16) -- previously missing entirely
// (only 8-bit signed and 16-bit unsigned existed), so `long < long` (this
// compiler's long/ulong are 2-byte, TypeExtensions.GetStorageBytes) would
// have emitted a reference to a macro that simply didn't exist. Test/
// BranchTest.cs already covers the analogous ulong (unsigned 16-bit) cases
// thoroughly but never long -- this file is exactly that missing coverage.
// Every case uses `if (a OP b) ... else Assert.Fail();` (or the reverse),
// same style as BranchTest.cs, rather than TestCase/ExpectedResult, since
// what's under test is which way the branch goes, not a computed value.
// Moved here (from Test/) to make room in Test's own program, which was
// already past its $d000 headroom ceiling before this -- see the
// objectTables.asm boundary-check fix and commit history around it.
[TestFixture]
public class SignedLongBranchTests
{
    [Test]
    public void Less_BothPositive_True()
    {
        long a = 100;
        long b = 200;
        if (!(a < b))
            Assert.Fail();
    }

    [Test]
    public void Less_BothPositive_False()
    {
        long a = 200;
        long b = 100;
        if (a < b)
            Assert.Fail();
    }

    [Test]
    public void Less_NegativeVsPositive_True()
    {
        long a = -1;
        long b = 1;
        if (!(a < b))
            Assert.Fail();
    }

    [Test]
    public void Less_PositiveVsNegative_False()
    {
        long a = 1;
        long b = -1;
        if (a < b)
            Assert.Fail();
    }

    [Test]
    public void Less_BothNegative_True()
    {
        long a = -200;
        long b = -100;
        if (!(a < b))
            Assert.Fail();
    }

    [Test]
    public void Less_BothNegative_False()
    {
        long a = -100;
        long b = -200;
        if (a < b)
            Assert.Fail();
    }

    [Test]
    public void Less_Equal_False()
    {
        long a = 5;
        long b = 5;
        if (a < b)
            Assert.Fail();
    }

    // The exact boundary the overflow-correction step exists for: the true
    // difference (32767 - (-32768) = 65535) doesn't fit in a signed 16-bit
    // result at all, so this only comes out right if SBC's V flag is
    // actually being used to correct N.
    [Test]
    public void Less_ExtremeBoundary_True()
    {
        long a = -32768;
        long b = 32767;
        if (!(a < b))
            Assert.Fail();
    }

    [Test]
    public void Less_ExtremeBoundary_False()
    {
        long a = 32767;
        long b = -32768;
        if (a < b)
            Assert.Fail();
    }

    [Test]
    public void LessEqual_Equal_True()
    {
        long a = 5;
        long b = 5;
        if (!(a <= b))
            Assert.Fail();
    }

    [Test]
    public void LessEqual_Less_True()
    {
        long a = 4;
        long b = 5;
        if (!(a <= b))
            Assert.Fail();
    }

    [Test]
    public void LessEqual_Greater_False()
    {
        long a = 6;
        long b = 5;
        if (a <= b)
            Assert.Fail();
    }

    [Test]
    public void LessEqual_NegativeEqual_True()
    {
        long a = -32768;
        long b = -32768;
        if (!(a <= b))
            Assert.Fail();
    }

    [Test]
    public void Greater_BothPositive_True()
    {
        long a = 200;
        long b = 100;
        if (!(a > b))
            Assert.Fail();
    }

    [Test]
    public void Greater_BothPositive_False()
    {
        long a = 100;
        long b = 200;
        if (a > b)
            Assert.Fail();
    }

    [Test]
    public void Greater_PositiveVsNegative_True()
    {
        long a = 1;
        long b = -1;
        if (!(a > b))
            Assert.Fail();
    }

    [Test]
    public void Greater_Equal_False()
    {
        long a = 5;
        long b = 5;
        if (a > b)
            Assert.Fail();
    }

    [Test]
    public void Greater_ExtremeBoundary_True()
    {
        long a = 32767;
        long b = -32768;
        if (!(a > b))
            Assert.Fail();
    }

    [Test]
    public void GreaterEqual_Equal_True()
    {
        long a = 5;
        long b = 5;
        if (!(a >= b))
            Assert.Fail();
    }

    [Test]
    public void GreaterEqual_Greater_True()
    {
        long a = 6;
        long b = 5;
        if (!(a >= b))
            Assert.Fail();
    }

    [Test]
    public void GreaterEqual_Less_False()
    {
        long a = 4;
        long b = 5;
        if (a >= b)
            Assert.Fail();
    }

    [Test]
    public void GreaterEqual_NegativeEqual_True()
    {
        long a = -100;
        long b = -100;
        if (!(a >= b))
            Assert.Fail();
    }

    [Test]
    public void For_Loop_Long()
    {
        long i = -5;
        for (; i < 5; i++)
        {
        }
        if (i != 5)
            Assert.Fail();
    }

    [Test]
    public void Combined_Long()
    {
        long a = -10;
        if (a >= -10 && a <= -9)
        {
            return;
        }
        Assert.Fail();
    }
}
