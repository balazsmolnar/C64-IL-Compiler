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

        public virtual int Virtual2()
        {
            VirtualVoid();
            return 5;
        }

        public virtual int Virtual3()
        {
            return 5;
        }

    }

    public class TestB : TestA
    {
        public override int Virtual() => 20;

        public override void VirtualVoid()
        {
            F = 14;
        }

        public override int Virtual2()
        {
            return base.Virtual2() + 1;
        }
    }

    public abstract class TestAbstract
    {
        public abstract int Ab();
        public int M() => 17;
    }

    public class TestDerived : TestAbstract
    {
        public override int Ab() => M();
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

            // for (ulong i = 0; i < 5ul; i++)
            // a.Virtual3(5);
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
        public void Test_Virtual_No_Implementation()
        {
            TestA b = new TestB();
            Assert.AreEqual(b.Virtual3(), 5);
        }

        [Test]
        public void Test_Virtual_Chain()
        {
            TestA a = new TestA();
            TestA b = new TestB();
            Assert.AreEqual(a.Virtual2(), 5);
            Assert.AreEqual(b.Virtual2(), 6);
        }

        [Test]
        public void Test_Abstract_Method()
        {
            TestAbstract a = new TestDerived();
            Assert.AreEqual(a.Ab(), 17);
        }
    }
}