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
        [TestCase(1, ExpectedResult = 0)]
        public int TestDecrement(int value)
        {
            var a = value;
            return --a;
        }

        [TestCase(5, 4, ExpectedResult = 9)]
        [TestCase(100, 0, ExpectedResult = 100)]
        [TestCase(1, -1, ExpectedResult = 0)]
        public int TestAdd(int a, int b)
        {
            return a+b;
        }

        [TestCase(5, 4, ExpectedResult = 1)]
        [TestCase(100, 0, ExpectedResult = 100)]
        [TestCase(1, -1, ExpectedResult = 2)]
        public int TestSub(int a, int b)
        {
            return a - b;
        }

        [TestCase(1,  ExpectedResult = 8)]
        [TestCase(2, ExpectedResult = 16)]
        public int TestShiftLeft3(int a)
        {
            return a << 3;
        }

    }
}