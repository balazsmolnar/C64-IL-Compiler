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

        //[TestCase(5u, 4u, ExpectedResult = false)]
        //[TestCase(100u, 100u, ExpectedResult = true)]
        //[TestCase(255u, 255u, ExpectedResult = true)]
        //public bool TestEquality_ulong(ulong a, ulong b)
        //{
        //    return a == b;
        //}

        //[TestCase(5u, ExpectedResult = false)]
        //[TestCase(4u, ExpectedResult = true)]
        //public bool TestEquality_ulong_const(ulong a)
        //{
        //    return a == 4u;
        //}

        //[TestCase(5, 4, ExpectedResult = false)]
        //[TestCase(100, 100, ExpectedResult = false)]
        //[TestCase(4, 5, ExpectedResult = true)]
        //[TestCase(-100, -99, ExpectedResult = true)]
        //[TestCase(-99, -100, ExpectedResult = false)]
        //public bool TestLess(long a, long b)
        //{
        //    return a < b;
        //}

        //[TestCase(10, ExpectedResult = false)]
        //[TestCase(100, ExpectedResult = false)]
        //[TestCase(4, ExpectedResult = true)]
        //public bool TestLess_Const(long a)
        //{
        //    return a < 10;
        //}

        //[TestCase(5u, 4u, ExpectedResult = false)]
        //[TestCase(100u, 100u, ExpectedResult = false)]
        //[TestCase(4u, 5u, ExpectedResult = true)]
        //[TestCase(200u, 201u, ExpectedResult = true)]
        //[TestCase(201u, 200u, ExpectedResult = false)]
        //[TestCase(10u, 240u, ExpectedResult = true)]
        //public bool TestLess_ulong(ulong a, ulong b)
        //{
        //    return a < b;
        //}

        //[TestCase(5, 4, ExpectedResult = false)]
        //[TestCase(100, 100, ExpectedResult = true)]
        //[TestCase(4, 5, ExpectedResult = true)]
        //[TestCase(-40, -39, ExpectedResult = true)]
        //public bool TestLessEqual(long a, long b)
        //{
        //    return a <= b;
        //}

        //[TestCase(5u, 4u, ExpectedResult = false)]
        //[TestCase(100u, 100u, ExpectedResult = true)]
        //[TestCase(4u, 5u, ExpectedResult = true)]
        //[TestCase(200u, 200u, ExpectedResult = true)]
        //[TestCase(200u, 201u, ExpectedResult = true)]
        //[TestCase(10u, 240u, ExpectedResult = true)]
        //public bool TestLessEqual_ulong(ulong a, ulong b)
        //{
        //    return a <= b;
        //}

        //[TestCase(5, 4, ExpectedResult = true)]
        //[TestCase(100, 100, ExpectedResult = false)]
        //[TestCase(4, 5, ExpectedResult = false)]
        //[TestCase(-100, -99, ExpectedResult = false)]
        //[TestCase(-100, -101, ExpectedResult = true)]
        //[TestCase(-2, 4, ExpectedResult = false)]
        //[TestCase(-1, -2, ExpectedResult = true)]
        //[TestCase(-2, -1, ExpectedResult = false)]
        //public bool TestGreater(long a, long b)
        //{
        //    return a > b;
        //}

        //[TestCase(5, ExpectedResult = true)]
        //[TestCase(4, ExpectedResult = false)]
        //[TestCase(2, ExpectedResult = false)]
        //[TestCase(-1, ExpectedResult = false)]
        //public bool TestGreater_const(long a)
        //{
        //    return a > 4;
        //}

        //[TestCase(5, ExpectedResult = true)]
        //[TestCase(-4, ExpectedResult = false)]
        //[TestCase(-5, ExpectedResult = false)]
        //[TestCase(-2, ExpectedResult = true)]
        //public bool TestGreater_const_negative(long a)
        //{
        //    return a > -4;
        //}

        //[TestCase(5u, 4u, ExpectedResult = true)]
        //[TestCase(100u, 100u, ExpectedResult = false)]
        //[TestCase(4u, 5u, ExpectedResult = false)]
        //[TestCase(200u, 201u, ExpectedResult = false)]
        //[TestCase(201u, 200u, ExpectedResult = true)]
        //[TestCase(10u, 240u, ExpectedResult = false)]
        //public bool TestGreater_ulong(ulong a, ulong b)
        //{
        //    return a > b;
        //}

        //[TestCase(5u, ExpectedResult = true)]
        //[TestCase(4u, ExpectedResult = false)]
        //[TestCase(2u, ExpectedResult = false)]
        //public bool TestGreater_ulong_const(ulong a)
        //{
        //    return a > 4u;
        //}

        //[TestCase(5, 4, ExpectedResult = true)]
        //[TestCase(100, 100, ExpectedResult = true)]
        //[TestCase(4, 5, ExpectedResult = false)]
        //public bool TestGreaterEqual(long a, long b)
        //{
        //    return a >= b;
        //}

        //[TestCase(5u, 4u, ExpectedResult = true)]
        //[TestCase(100u, 100u, ExpectedResult = true)]
        //[TestCase(4u, 5u, ExpectedResult = false)]
        //[TestCase(200u, 199u, ExpectedResult = true)]
        //[TestCase(200u, 201u, ExpectedResult = false)]
        //public bool TestGreaterEqual_ulong(ulong a, ulong b)
        //{
        //    return a >= b;
        //}

    }
}