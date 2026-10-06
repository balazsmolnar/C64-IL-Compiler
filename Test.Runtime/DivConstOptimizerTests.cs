using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.RuntimeTest;

// Runtime correctness for ILMethodDivConstOptimizer -- see
// Compiler.UnitTests/DivConstOptimizerTests.cs for the structural ("did
// it actually fuse, and did it correctly NOT fuse for signed division")
// side of this. These check the actual computed VALUE, with particular
// emphasis on negative dividends for the signed case: that's exactly
// the boundary a logical-right-shift-based fusion would get silently
// wrong if it were ever (incorrectly) applied there, so it gets
// deliberately exercised here even though the compiler itself already
// refuses to fuse it structurally.
[TestFixture]
public class DivConstOptimizerTests
{
    [Test]
    public void Unsigned_Divide_By_Four_Is_Correct()
    {
        uint a = 20;
        if (a / 4 != 5)
            Assert.Fail();

        uint b = 21;
        if (b / 4 != 5)
            Assert.Fail();

        uint c = 3;
        if (c / 4 != 0)
            Assert.Fail();
    }

    [Test]
    public void Unsigned_Divide_By_Two_Is_Correct()
    {
        uint a = 20;
        if (a / 2 != 10)
            Assert.Fail();

        uint b = 21;
        if (b / 2 != 10)
            Assert.Fail();
    }

    [Test]
    public void Unsigned_Divide_By_One_Is_Correct()
    {
        uint a = 20;
        if (a / 1 != 20)
            Assert.Fail();
    }

    [Test]
    public void Unsigned_Wide_Divide_By_Four_Is_Correct()
    {
        ulong a = 20;
        if (a / 4 != 5)
            Assert.Fail();

        ulong b = 1000;
        if (b / 4 != 250)
            Assert.Fail();
    }

    [Test]
    public void Unsigned_Divide_By_Non_Power_Of_Two_Is_Still_Correct()
    {
        uint a = 21;
        if (a / 5 != 4)
            Assert.Fail();
    }

    // The safety-critical case: signed division by the same constants,
    // with NEGATIVE dividends specifically -- exactly where a logical
    // (zero-filling) right shift would diverge from C#'s truncate-
    // toward-zero semantics if it were ever mistakenly applied here.
    [Test]
    public void Signed_Divide_By_Four_With_Negative_Dividend_Truncates_Toward_Zero()
    {
        int a = -7;
        // -7 / 4 truncates toward zero: -1 (not -2, which floor
        // division -- or a naive arithmetic shift -- would give).
        if (a / 4 != -1)
            Assert.Fail();

        int b = -8;
        if (b / 4 != -2)
            Assert.Fail();

        int c = -1;
        if (c / 4 != 0)
            Assert.Fail();
    }

    [Test]
    public void Signed_Divide_By_Two_With_Negative_Dividend_Truncates_Toward_Zero()
    {
        int a = -3;
        if (a / 2 != -1)
            Assert.Fail();

        int b = -4;
        if (b / 2 != -2)
            Assert.Fail();
    }

    [Test]
    public void Signed_Divide_By_Four_With_Positive_Dividend_Is_Correct()
    {
        int a = 7;
        if (a / 4 != 1)
            Assert.Fail();
    }
}
