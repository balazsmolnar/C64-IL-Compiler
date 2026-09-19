using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.Test;

// Exercises Compiler/CommandMap.cs's And/Or/Xor mappings (all three reuse
// OpArithmetic2, same as Add/Sub/Mul/Div) against asm/helper/arithmetic.asm's
// and8/or8/xor8 (uint/int) and or16/xor16 (ulong/long) macros. And already had
// 8-bit coverage informally via other tests but no dedicated cases; Or/Xor
// are new (added alongside 16-bit support neither previously had).
[TestFixture]
public class BitwiseTests
{
    [TestCase(0xFFu, 0x0Fu, ExpectedResult = 0x0Fu)]
    [TestCase(0xF0u, 0x0Fu, ExpectedResult = 0x00u)]
    [TestCase(0xACu, 0xACu, ExpectedResult = 0xACu)]
    [TestCase(0x00u, 0xFFu, ExpectedResult = 0x00u)]
    public uint TestAndUInt(uint a, uint b)
    {
        return a & b;
    }

    [TestCase(0xF0u, 0x0Fu, ExpectedResult = 0xFFu)]
    [TestCase(0xACu, 0xACu, ExpectedResult = 0xACu)]
    [TestCase(0x00u, 0x00u, ExpectedResult = 0x00u)]
    [TestCase(0x80u, 0x01u, ExpectedResult = 0x81u)]
    public uint TestOrUInt(uint a, uint b)
    {
        return a | b;
    }

    [TestCase(0xFFu, 0x0Fu, ExpectedResult = 0xF0u)]
    [TestCase(0xACu, 0xACu, ExpectedResult = 0x00u)]
    [TestCase(0x00u, 0xFFu, ExpectedResult = 0xFFu)]
    [TestCase(0x81u, 0x01u, ExpectedResult = 0x80u)]
    public uint TestXorUInt(uint a, uint b)
    {
        return a ^ b;
    }

    [TestCase(-1, 0x0F, ExpectedResult = 0x0F)]
    [TestCase(0x0F, -1, ExpectedResult = 0x0F)]
    public int TestAndInt(int a, int b)
    {
        return a & b;
    }

    [TestCase(-1, 0, ExpectedResult = -1)]
    [TestCase(0, 0, ExpectedResult = 0)]
    public int TestOrInt(int a, int b)
    {
        return a | b;
    }

    [TestCase(-1, 0, ExpectedResult = -1)]
    [TestCase(-1, -1, ExpectedResult = 0)]
    public int TestXorInt(int a, int b)
    {
        return a ^ b;
    }

    [TestCase(0xFFFFul, 0x0F0Ful, ExpectedResult = 0x0F0Ful)]
    [TestCase(0xFF00ul, 0x00FFul, ExpectedResult = 0x0000ul)]
    [TestCase(0xACACul, 0xACACul, ExpectedResult = 0xACACul)]
    public ulong TestAndULong(ulong a, ulong b)
    {
        return a & b;
    }

    [TestCase(0xFF00ul, 0x00FFul, ExpectedResult = 0xFFFFul)]
    [TestCase(0xACACul, 0xACACul, ExpectedResult = 0xACACul)]
    [TestCase(0x8000ul, 0x0001ul, ExpectedResult = 0x8001ul)]
    public ulong TestOrULong(ulong a, ulong b)
    {
        return a | b;
    }

    [TestCase(0xFFFFul, 0x0F0Ful, ExpectedResult = 0xF0F0ul)]
    [TestCase(0xACACul, 0xACACul, ExpectedResult = 0x0000ul)]
    [TestCase(0x8001ul, 0x0001ul, ExpectedResult = 0x8000ul)]
    public ulong TestXorULong(ulong a, ulong b)
    {
        return a ^ b;
    }

    [TestCase(-1L, 0x0FL, ExpectedResult = 0x0FL)]
    [TestCase(1000L, -1L, ExpectedResult = 1000L)]
    public long TestAndLong(long a, long b)
    {
        return a & b;
    }

    [TestCase(-1L, 0L, ExpectedResult = -1L)]
    [TestCase(0L, 0L, ExpectedResult = 0L)]
    public long TestOrLong(long a, long b)
    {
        return a | b;
    }

    [TestCase(-1L, 0L, ExpectedResult = -1L)]
    [TestCase(-1L, -1L, ExpectedResult = 0L)]
    public long TestXorLong(long a, long b)
    {
        return a ^ b;
    }
}
