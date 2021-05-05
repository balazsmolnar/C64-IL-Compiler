using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.Test
{
    [TestFixture]
    public class Arithmetic16Tests
    {

        [TestCase(5L, ExpectedResult = 6L)]
        [TestCase(0L, ExpectedResult = 1L)]
        [TestCase(10000L, ExpectedResult = 10001L)]
        public long TestIncrement(long value)
        {
            var a = value;
            return ++a;
        }

        [TestCase(5, ExpectedResult = 4)]
        [TestCase(100, ExpectedResult = 99)]
        [TestCase(10000, ExpectedResult = 9999)]
        [TestCase(1, ExpectedResult = 0)]
        public long TestDecrement(long value)
        {
            var a = value;
            return --a;
        }

        [TestCase(5, 4, ExpectedResult = 9)]
        [TestCase(100, 0, ExpectedResult = 100)]
        [TestCase(10000, 10000, ExpectedResult = 20000)]
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
}