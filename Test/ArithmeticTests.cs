using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.Test
{
    [TestFixture]
    public class ArithmeticTests
    {

        [TestCase(5, ExpectedResult = 6)]
        [TestCase(0, ExpectedResult = 1)]
        [TestCase(100, ExpectedResult = 101)]
        [TestCase(-1, ExpectedResult = 0)]
        public int TestIncrement(int value)
        {
            var a = value;
            return ++a;
        }

        [TestCase(5, ExpectedResult = 4)]
        [TestCase(100, ExpectedResult = 99)]
        [TestCase(-100, ExpectedResult = -101)]
        [TestCase(1, ExpectedResult = 0)]
        public int TestDecrement(int value)
        {
            var a = value;
            return --a;
        }

        [TestCase(5, 4, ExpectedResult = 9)]
        [TestCase(100, 0, ExpectedResult = 100)]
        [TestCase(1, -1, ExpectedResult = 0)]
        [TestCase(-10, -1, ExpectedResult = -11)]
        [TestCase(-10, 11, ExpectedResult = 1)]
        public int TestAdd(int a, int b)
        {
            return a + b;
        }

        [TestCase(5u, 4u, ExpectedResult = 9)]
        [TestCase(100u, 0u, ExpectedResult = 100)]
        [TestCase(200u, 55u, ExpectedResult = 255)]
        public uint TestAdd_uint(uint a, uint b)
        {
            return a + b;
        }

        [TestCase(5, ExpectedResult = 18)]
        [TestCase(100, ExpectedResult = 113)]
        [TestCase(-5, ExpectedResult = 8)]
        public int TestAdd_Const(int a)
        {
            return a + 13;
        }

        [TestCase(5, ExpectedResult = 4)]
        [TestCase(100, ExpectedResult = 99)]
        [TestCase(-5, ExpectedResult = -6)]
        public int TestAdd_Const_Negative(int a)
        {
            return a + -1;
        }

        [TestCase(5, 4, ExpectedResult = 1)]
        [TestCase(100, 0, ExpectedResult = 100)]
        [TestCase(1, -1, ExpectedResult = 2)]
        public int TestSub(int a, int b)
        {
            return a - b;
        }

        [TestCase(1, ExpectedResult = 8)]
        [TestCase(2, ExpectedResult = 16)]
        public int TestShiftLeft3(int a)
        {
            return a << 3;
        }

        [TestCase(5, 4, ExpectedResult = false)]
        [TestCase(100, 100, ExpectedResult = true)]
        [TestCase(-10, -10, ExpectedResult = true)]
        public bool TestEquality(int a, int b)
        {
            return a == b;
        }

        [TestCase(5, ExpectedResult = false)]
        [TestCase(4, ExpectedResult = true)]
        public bool TestEquality_Const(int a)
        {
            return a == 4;
        }

        [TestCase(5u, 4u, ExpectedResult = false)]
        [TestCase(100u, 100u, ExpectedResult = true)]
        [TestCase(255u, 255u, ExpectedResult = true)]
        public bool TestEquality_uint(uint a, uint b)
        {
            return a == b;
        }

        [TestCase(5u, ExpectedResult = false)]
        [TestCase(4u, ExpectedResult = true)]
        public bool TestEquality_uint_const(uint a)
        {
            return a == 4u;
        }

        [TestCase(5, 4, ExpectedResult = false)]
        [TestCase(100, 100, ExpectedResult = false)]
        [TestCase(4, 5, ExpectedResult = true)]
        [TestCase(-100, -99, ExpectedResult = true)]
        [TestCase(-99, -100, ExpectedResult = false)]
        public bool TestLess(int a, int b)
        {
            return a < b;
        }

        [TestCase(10, ExpectedResult = false)]
        [TestCase(100, ExpectedResult = false)]
        [TestCase(4, ExpectedResult = true)]
        public bool TestLess_Const(int a)
        {
            return a < 10;
        }

        [TestCase(5u, 4u, ExpectedResult = false)]
        [TestCase(100u, 100u, ExpectedResult = false)]
        [TestCase(4u, 5u, ExpectedResult = true)]
        [TestCase(200u, 201u, ExpectedResult = true)]
        [TestCase(201u, 200u, ExpectedResult = false)]
        [TestCase(10u, 240u, ExpectedResult = true)]
        public bool TestLess_uint(uint a, uint b)
        {
            return a < b;
        }

        [TestCase(5, 4, ExpectedResult = false)]
        [TestCase(100, 100, ExpectedResult = true)]
        [TestCase(4, 5, ExpectedResult = true)]
        [TestCase(-40, -39, ExpectedResult = true)]
        public bool TestLessEqual(int a, int b)
        {
            return a <= b;
        }

        [TestCase(5u, 4u, ExpectedResult = false)]
        [TestCase(100u, 100u, ExpectedResult = true)]
        [TestCase(4u, 5u, ExpectedResult = true)]
        [TestCase(200u, 200u, ExpectedResult = true)]
        [TestCase(200u, 201u, ExpectedResult = true)]
        [TestCase(10u, 240u, ExpectedResult = true)]
        public bool TestLessEqual_uint(uint a, uint b)
        {
            return a <= b;
        }

        [TestCase(5, 4, ExpectedResult = true)]
        [TestCase(100, 100, ExpectedResult = false)]
        [TestCase(4, 5, ExpectedResult = false)]
        [TestCase(-100, -99, ExpectedResult = false)]
        [TestCase(-100, -101, ExpectedResult = true)]
        [TestCase(-2, 4, ExpectedResult = false)]
        [TestCase(-1, -2, ExpectedResult = true)]
        [TestCase(-2, -1, ExpectedResult = false)]
        public bool TestGreater(int a, int b)
        {
            return a > b;
        }

        [TestCase(5, ExpectedResult = true)]
        [TestCase(4, ExpectedResult = false)]
        [TestCase(2, ExpectedResult = false)]
        [TestCase(-1, ExpectedResult = false)]
        public bool TestGreater_const(int a)
        {
            return a > 4;
        }

        [TestCase(5, ExpectedResult = true)]
        [TestCase(-4, ExpectedResult = false)]
        [TestCase(-5, ExpectedResult = false)]
        [TestCase(-2, ExpectedResult = true)]
        public bool TestGreater_const_negative(int a)
        {
            return a > -4;
        }

        [TestCase(5u, 4u, ExpectedResult = true)]
        [TestCase(100u, 100u, ExpectedResult = false)]
        [TestCase(4u, 5u, ExpectedResult = false)]
        [TestCase(200u, 201u, ExpectedResult = false)]
        [TestCase(201u, 200u, ExpectedResult = true)]
        [TestCase(10u, 240u, ExpectedResult = false)]
        public bool TestGreater_uint(uint a, uint b)
        {
            return a > b;
        }

        [TestCase(5u, ExpectedResult = true)]
        [TestCase(4u, ExpectedResult = false)]
        [TestCase(2u, ExpectedResult = false)]
        public bool TestGreater_uint_const(uint a)
        {
            return a > 4u;
        }

        [TestCase(5, 4, ExpectedResult = true)]
        [TestCase(100, 100, ExpectedResult = true)]
        [TestCase(4, 5, ExpectedResult = false)]
        public bool TestGreaterEqual(int a, int b)
        {
            return a >= b;
        }

        [TestCase(5u, 4u, ExpectedResult = true)]
        [TestCase(100u, 100u, ExpectedResult = true)]
        [TestCase(4u, 5u, ExpectedResult = false)]
        [TestCase(200u, 199u, ExpectedResult = true)]
        [TestCase(200u, 201u, ExpectedResult = false)]
        public bool TestGreaterEqual_uint(uint a, uint b)
        {
            return a >= b;
        }

    }
}