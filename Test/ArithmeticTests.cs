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
        public int TestIncrement(int value)
        {
            var a = value;
            return ++a;
        }
    }
}