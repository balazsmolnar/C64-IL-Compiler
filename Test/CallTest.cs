using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.Test
{
    [TestFixture]
    class CallTest
    {
        public int StaticMethod(int a, MockObject o1, MockObject o2)
        {
            Assert.AreEqual(a, 3);
            Assert.AreEqual(o1.F, 12);
            Assert.AreEqual(o2.F, 13);
            return 5;
        }

        public MockObject StaticMethodReturnObj() => new MockObject { F = 12 };
        public MockObject InstanceMethodReturnObj() => new MockObject { F = 12 };


        public int InstanceMethod(int a, MockObject o1, MockObject o2)
        {
            Assert.AreEqual(a, 3);
            Assert.AreEqual(o1.F, 12);
            Assert.AreEqual(o2.F, 13);
            return 5;
        }

        public ulong InstanceMethod_ulong(ulong a)
        {
            if (a != 1000)
                Assert.Fail();
            return 2000;
        }

        [Test]
        public void Test_Static_Method()
        {
            var result = StaticMethod(3, new MockObject { F = 12 }, new MockObject { F = 13 });
            Assert.AreEqual(result, 5);
        }

        [Test]
        public void Test_Instance_Method()
        {
            var result = StaticMethod(3, new MockObject { F = 12 }, new MockObject { F = 13 });
            Assert.AreEqual(result, 5);
        }

        [Test]
        public void Test_Static_Method_Returns_Obj()
        {           
            Assert.AreEqual(StaticMethodReturnObj().F, 12);
        }

        [Test]
        public void Test_Instance_Method_Returns_Obj()
        {
            Assert.AreEqual(StaticMethodReturnObj().F, 12);
        }

        [Test]
        public void Test_Instance_Method_ulong()
        {
            if (InstanceMethod_ulong(1000) != 2000)
                Assert.Fail();
        }


        public int RecursiveSum(int i)
        {
            if (i == 1)
                return 1;
            return i + RecursiveSum(i - 1);
        }

        public int RecursiveSumWithLocals(int i)
        {
            if (i == 1)
                return 1;
            int b = i + RecursiveSum(i - 1);
            int c = b;           
            return c;
        }

        [Test]
        public void Test_Recursive_Method()
        {
            Assert.AreEqual(RecursiveSum(10), 55);
        }

        [Test]
        public void Test_Recursive_Method_With_Locals()
        {
            Assert.AreEqual(RecursiveSumWithLocals(10), 55);
        }

    }
}