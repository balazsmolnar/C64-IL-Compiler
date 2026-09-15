using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.Test;

[TestFixture]
public class Arithmetic16Tests
{

    [TestCase(5L, ExpectedResult = 6L)]
    [TestCase(0L, ExpectedResult = 1L)]
    [TestCase(10000L, ExpectedResult = 10001L)]
    [TestCase(-5L, ExpectedResult = -4L)]
    [TestCase(-1L, ExpectedResult = 0L)]
    [TestCase(-1000L, ExpectedResult = -999L)]
    public long TestIncrement(long value)
    {
        var a = value;
        return ++a;
    }

    [TestCase(5, ExpectedResult = 4)]
    [TestCase(100, ExpectedResult = 99)]
    [TestCase(10000, ExpectedResult = 9999)]
    [TestCase(1, ExpectedResult = 0)]
    [TestCase(0, ExpectedResult = -1)]
    [TestCase(-1000, ExpectedResult = -1001)]
    public long TestDecrement(long value)
    {
        var a = value;
        return --a;
    }

    [TestCase(5, 4, ExpectedResult = 9)]
    [TestCase(100, 0, ExpectedResult = 100)]
    [TestCase(10000, 10000, ExpectedResult = 20000)]
    [TestCase(10000, 1, ExpectedResult = 10001)]
    [TestCase(10000, -1, ExpectedResult = 9999)]
    [TestCase(100, -100, ExpectedResult = 0)]
    [TestCase(1000, -1000, ExpectedResult = 0)]
    [TestCase(-1000, 1, ExpectedResult = -999)]
    [TestCase(-1000, -1000, ExpectedResult = -2000)]
    public long TestAdd(long a, long b)
    {
        return a + b;
    }

    [TestCase(5u, 4u, ExpectedResult = 9)]
    [TestCase(100u, 0u, ExpectedResult = 100)]
    [TestCase(20000u, 20000u, ExpectedResult = 40000)]
    public ulong TestAdd_ulong(ulong a, ulong b)
    {
        return a + b;
    }

    [TestCase(5, ExpectedResult = 10005)]
    [TestCase(10000, ExpectedResult = 20000)]
    public long TestAdd_Const(long a)
    {
        return a + 10000L;
    }

    [TestCase(5, 4, ExpectedResult = 1)]
    [TestCase(100, 0, ExpectedResult = 100)]
    [TestCase(20000, 10000, ExpectedResult = 10000)]
    public long TestSub(long a, long b)
    {
        return a - b;
    }

    // Kept well inside 16-bit range -- this compiler's storage is only
    // ever 2 bytes wide (see add16/sub16), so unlike real .NET long the
    // product truncates mod 65536 past that, same as every other 16-bit
    // op here already does.
    [TestCase(5, 4, ExpectedResult = 20)]
    [TestCase(100, 0, ExpectedResult = 0)]
    [TestCase(200, 150, ExpectedResult = 30000)]
    [TestCase(-200, 150, ExpectedResult = -30000)]
    [TestCase(-200, -150, ExpectedResult = 30000)]
    public long TestMul(long a, long b)
    {
        return a * b;
    }

    [TestCase(5UL, 4UL, ExpectedResult = 20)]
    [TestCase(200UL, 300UL, ExpectedResult = 60000)]
    public ulong TestMul_ulong(ulong a, ulong b)
    {
        return a * b;
    }

    // Not a power of two -- exercises the general shift-and-add #mul16.
    [TestCase(300, ExpectedResult = 900)]
    [TestCase(-300, ExpectedResult = -900)]
    public long TestMul_Const3(long a)
    {
        return a * 3L;
    }

    // 1/2/4 -- each of these should compile to
    // ILMethodMulConstOptimizer's #mul_shift_const16 (0/1/2 asl/rol's)
    // instead of the general #mul16 loop.
    [TestCase(300, ExpectedResult = 300)]
    [TestCase(-300, ExpectedResult = -300)]
    public long TestMul_Const1(long a)
    {
        return a * 1L;
    }

    [TestCase(300, ExpectedResult = 600)]
    [TestCase(-300, ExpectedResult = -600)]
    public long TestMul_Const2(long a)
    {
        return a * 2L;
    }

    [TestCase(300, ExpectedResult = 1200)]
    [TestCase(-300, ExpectedResult = -1200)]
    public long TestMul_Const4(long a)
    {
        return a * 4L;
    }

    [TestCase(5, 4, ExpectedResult = false)]
    [TestCase(100, 100, ExpectedResult = true)]
    [TestCase(10000, 10000, ExpectedResult = true)]
    [TestCase(10000, 10001, ExpectedResult = false)]
    public bool TestEquality(long a, long b)
    {
        return a == b;
    }

    [TestCase(10001, ExpectedResult = false)]
    [TestCase(10000, ExpectedResult = true)]
    public bool TestEquality_Const(long a)
    {
        return a == 10000L;
    }

    [TestCase(5UL, 4UL, ExpectedResult = false)]
    [TestCase(100UL, 100UL, ExpectedResult = true)]
    [TestCase(10000UL, 10000UL, ExpectedResult = true)]
    [TestCase(10000UL, 10001UL, ExpectedResult = false)]
    public bool TestEquality_ulong(ulong a, ulong b)
    {
        return a == b;
    }

    [TestCase(10001UL, ExpectedResult = false)]
    [TestCase(10000UL, ExpectedResult = true)]
    public bool TestEquality_Const_ulong(ulong a)
    {
        return a == 10000UL;
    }


    [TestCase(5, 4, ExpectedResult = false)]
    [TestCase(10000, 9999, ExpectedResult = false)]
    [TestCase(10000, 10000, ExpectedResult = false)]
    [TestCase(513, 10000, ExpectedResult = true)]
    [TestCase(10000, 513, ExpectedResult = false)]
    [TestCase(9999, 10000, ExpectedResult = true)]
    [TestCase(-5, 3, ExpectedResult = true)]
    [TestCase(5, -3, ExpectedResult = false)]
    [TestCase(-5, -3, ExpectedResult = true)]
    [TestCase(-3, -5, ExpectedResult = false)]
    [TestCase(-3, -3, ExpectedResult = false)]
    [TestCase(-32768, 32767, ExpectedResult = true)]
    [TestCase(32767, -32768, ExpectedResult = false)]
    public bool TestLess(long a, long b)
    {
        return a < b;
    }

    [TestCase(5UL, 4UL, ExpectedResult = false)]
    [TestCase(10000UL, 9999UL, ExpectedResult = false)]
    [TestCase(10000UL, 10000UL, ExpectedResult = false)]
    [TestCase(513UL, 10000UL, ExpectedResult = true)]
    [TestCase(10000UL, 513UL, ExpectedResult = false)]
    [TestCase(9999UL, 10000UL, ExpectedResult = true)]
    public bool TestLess_ulong(ulong a, ulong b)
    {
        return a < b;
    }

    [TestCase(1001, ExpectedResult = true)]
    [TestCase(10001, ExpectedResult = false)]
    [TestCase(10000, ExpectedResult = false)]
    [TestCase(9999, ExpectedResult = true)]
    public bool TestLess_Const(long a)
    {
        return a < 10000L;
    }

    [TestCase(1001UL, ExpectedResult = true)]
    [TestCase(10001UL, ExpectedResult = false)]
    [TestCase(10000UL, ExpectedResult = false)]
    [TestCase(9999UL, ExpectedResult = true)]
    public bool TestLess_Const_ulong(ulong a)
    {
        return a < 10000UL;
    }


    [TestCase(5, 4, ExpectedResult = false)]
    [TestCase(10000, 9999, ExpectedResult = false)]
    [TestCase(10000, 10000, ExpectedResult = true)]
    [TestCase(513, 10000, ExpectedResult = true)]
    [TestCase(10000, 513, ExpectedResult = false)]
    [TestCase(9999, 10000, ExpectedResult = true)]
    public bool TestLess_Equal(long a, long b)
    {
        return a <= b;
    }

    [TestCase(5UL, 4UL, ExpectedResult = false)]
    [TestCase(10000UL, 9999UL, ExpectedResult = false)]
    [TestCase(10000UL, 10000UL, ExpectedResult = true)]
    [TestCase(513UL, 10000UL, ExpectedResult = true)]
    [TestCase(10000UL, 513UL, ExpectedResult = false)]
    [TestCase(9999UL, 10000UL, ExpectedResult = true)]
    public bool TestLess_Equal_ulong(ulong a, ulong b)
    {
        return a <= b;
    }


    [TestCase(5, 4, ExpectedResult = true)]
    [TestCase(10000, 9999, ExpectedResult = true)]
    [TestCase(10000, 10000, ExpectedResult = false)]
    [TestCase(513, 10000, ExpectedResult = false)]
    [TestCase(10000, 513, ExpectedResult = true)]
    [TestCase(9999, 10000, ExpectedResult = false)]
    [TestCase(-5, 3, ExpectedResult = false)]
    [TestCase(5, -3, ExpectedResult = true)]
    [TestCase(-5, -3, ExpectedResult = false)]
    [TestCase(-3, -5, ExpectedResult = true)]
    [TestCase(-3, -3, ExpectedResult = false)]
    [TestCase(-32768, 32767, ExpectedResult = false)]
    [TestCase(32767, -32768, ExpectedResult = true)]
    public bool TestGreater(long a, long b)
    {
        return a > b;
    }

    [TestCase(5UL, 4UL, ExpectedResult = true)]
    [TestCase(10000UL, 9999UL, ExpectedResult = true)]
    [TestCase(10000UL, 10000UL, ExpectedResult = false)]
    [TestCase(513UL, 10000UL, ExpectedResult = false)]
    [TestCase(10000UL, 513UL, ExpectedResult = true)]
    [TestCase(9999UL, 10000UL, ExpectedResult = false)]
    public bool TestGreater_ulong(ulong a, ulong b)
    {
        return a > b;
    }

    [TestCase(5UL, 4UL, ExpectedResult = true)]
    [TestCase(10000UL, 9999UL, ExpectedResult = true)]
    [TestCase(10000UL, 10000UL, ExpectedResult = false)]
    [TestCase(513UL, 10000UL, ExpectedResult = false)]
    [TestCase(10000UL, 513UL, ExpectedResult = true)]
    [TestCase(9999UL, 10000UL, ExpectedResult = false)]
    public bool TestGreater_if_ulong(ulong a, ulong b)
    {
        if (a > b)
            return true;
        else
            return false;
    }
}