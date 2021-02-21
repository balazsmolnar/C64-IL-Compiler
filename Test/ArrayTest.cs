using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.Test
{
    class MockObject
    {
        public int F;
    }

    [TestFixture]
    class ArrayTest
    {
        [Test]
        public void Int_Array()
        {
            var sut = new int[5];
            Assert.AreEqual(sut.Length, 5);
            sut[1] = 6;
            Assert.AreEqual(sut[1], 6);
        }

        [Test]
        public void Int_Array_Initializer()
        {
            int a = 5;
            int b = 6;
            var sut = new int[] { a , b };
            Assert.AreEqual(sut.Length, 2);
            Assert.AreEqual(sut[1], 6);
        }

        [Test]
        public void Object_Array()
        {
            var sut = new MockObject[5];
            Assert.AreEqual(sut.Length, 5);
            sut[1] = new MockObject { F = 5 };
            Assert.AreEqual(sut[1].F, 5);
        }

        [Test]
        public void Object_Array_Initializer()
        {
            var sut = new MockObject[] { new MockObject { F = 12 }, new MockObject { F = 13 } };
            Assert.AreEqual(sut.Length, 2);
            Assert.AreEqual(sut[1].F, 13);
        }

   }
}