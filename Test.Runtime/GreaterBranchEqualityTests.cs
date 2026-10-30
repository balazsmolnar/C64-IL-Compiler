using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.RuntimeTest;

// `if (a <= b)` compiles to a branch-if-greater (bgt / bgt.un) round the
// then-part, and `while (a > b)` to one back to the loop body, so the
// greater-than branch macros in asm/helper/branch.asm must NOT branch when
// a == b. The operands come in as parameters so nothing is folded at
// compile time; each case runs equal, smaller and larger.
[TestFixture]
public class GreaterBranchEqualityTests
{
    [TestCase(5u, 5u, ExpectedResult = 1u)]
    [TestCase(4u, 5u, ExpectedResult = 1u)]
    [TestCase(6u, 5u, ExpectedResult = 0u)]
    [TestCase(0u, 0u, ExpectedResult = 1u)]
    [TestCase(255u, 255u, ExpectedResult = 1u)]
    public uint UInt_LessOrEqual(uint a, uint b)
    {
        uint hits = 0;
        if (a <= b)
            hits = hits + 1;
        return hits;
    }

    [TestCase(5u, ExpectedResult = 1u)]
    [TestCase(4u, ExpectedResult = 1u)]
    [TestCase(6u, ExpectedResult = 0u)]
    public uint UInt_LessOrEqual_Const(uint a)
    {
        uint hits = 0;
        if (a <= 5)
            hits = hits + 1;
        return hits;
    }

    [TestCase(9u, 4u, ExpectedResult = 5u)]
    [TestCase(4u, 4u, ExpectedResult = 0u)]
    [TestCase(3u, 4u, ExpectedResult = 0u)]
    [TestCase(30u, 0u, ExpectedResult = 30u)]
    public uint UInt_WhileGreater_Counts_Down_To_Equal(uint a, uint b)
    {
        uint n = 0;
        while (a > b)
        {
            a = a - 1;
            n = n + 1;
        }
        return n;
    }

    [TestCase(1000UL, 1000UL, ExpectedResult = 1u)]
    [TestCase(999UL, 1000UL, ExpectedResult = 1u)]
    [TestCase(1001UL, 1000UL, ExpectedResult = 0u)]
    [TestCase(1255UL, 1000UL, ExpectedResult = 0u)]
    [TestCase(745UL, 1000UL, ExpectedResult = 1u)]
    public uint ULong_LessOrEqual(ulong a, ulong b)
    {
        uint hits = 0;
        if (a <= b)
            hits = hits + 1;
        return hits;
    }

    [TestCase(1000UL, ExpectedResult = 1u)]
    [TestCase(1001UL, ExpectedResult = 0u)]
    [TestCase(999UL, ExpectedResult = 1u)]
    public uint ULong_LessOrEqual_Const(ulong a)
    {
        uint hits = 0;
        if (a <= 1000)
            hits = hits + 1;
        return hits;
    }

    [TestCase(5, 5, ExpectedResult = 1u)]
    [TestCase(4, 5, ExpectedResult = 1u)]
    [TestCase(6, 5, ExpectedResult = 0u)]
    [TestCase(-3, -3, ExpectedResult = 1u)]
    [TestCase(-4, -3, ExpectedResult = 1u)]
    [TestCase(-2, -3, ExpectedResult = 0u)]
    public uint Int_LessOrEqual(int a, int b)
    {
        uint hits = 0;
        if (a <= b)
            hits = hits + 1;
        return hits;
    }

    [TestCase(5, ExpectedResult = 1u)]
    [TestCase(4, ExpectedResult = 1u)]
    [TestCase(6, ExpectedResult = 0u)]
    public uint Int_LessOrEqual_Const(int a)
    {
        uint hits = 0;
        if (a <= 5)
            hits = hits + 1;
        return hits;
    }

    [TestCase(9, 4, ExpectedResult = 5u)]
    [TestCase(4, 4, ExpectedResult = 0u)]
    [TestCase(-1, -4, ExpectedResult = 3u)]
    public uint Int_WhileGreater_Counts_Down_To_Equal(int a, int b)
    {
        uint n = 0;
        while (a > b)
        {
            a = a - 1;
            n = n + 1;
        }
        return n;
    }
}
