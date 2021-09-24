using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.Test
{
    public class TestA
    {
        public static int F;
        public int NonVirtual() => 17;
        public virtual int Virtual() => 18;
        public int NonVirtual2() => 19;

        public virtual void VirtualVoid()
        {
            F = 12;
        }

        public virtual int Virtual2(int param)
        {
            VirtualVoid();
            return param;
        }

    }

    public class TestB : TestA
    {
        public override int Virtual() => 20;

        public override void VirtualVoid()
        {
            F = 14;
        }

        public override int Virtual2(int param)
        {
            return base.Virtual2(param) + 1;
        }
    }

    [TestFixture]
    class VirtualMethodTest
    {

        [Test]
        public void Test_Virtual_Method_Call()
        {
            TestA a = new TestA();
            TestA b = new TestB();

            Assert.AreEqual(a.NonVirtual(), 17);
            Assert.AreEqual(a.Virtual(), 18);
            Assert.AreEqual(a.NonVirtual2(), 19);

            Assert.AreEqual(b.Virtual(), 20);
        }

        [Test]
        public void Test_Virtual_Method_Call_void()
        {
            TestA a = new TestA();
            TestA b = new TestB();
            a.VirtualVoid();
            Assert.AreEqual(TestA.F, 12);
            b.VirtualVoid();
            Assert.AreEqual(TestA.F, 14);
        }

        [Test]
        public void Test_Virtual_Chain()
        {
            TestA a = new TestA();
            TestA b = new TestB();
            a.Virtual2(2);
            // Assert.AreEqual(a.Virtual2(2), 20);
        }
    }
}