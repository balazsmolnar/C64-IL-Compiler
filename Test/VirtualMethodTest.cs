using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.Test;

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

    public virtual int VirtualParam(int param) => param + 1;

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

    public override int VirtualParam(int param) => param + 2;
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

// Three-level hierarchy for exercising devirtualization across more than one
// inheritance step: GrandBase declares Depth() virtual, MidNoOverride
// inherits it unchanged, LeafOverride overrides it two levels down.
public class GrandBase
{
    public virtual int Depth() => 1;
}

public class MidNoOverride : GrandBase
{
}

public class LeafOverride : MidNoOverride
{
    public override int Depth() => 3;
}

// No subclasses anywhere in the assembly -- the simplest possible
// devirtualization case (declared virtual, but there's nothing else it
// could ever resolve to).
public class StandaloneVirtual
{
    public virtual int Value() => 42;
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
    public void Test_Virtual_With_Parameter()
    {
        TestA a = new TestA();
        TestA b = new TestB();
        Assert.AreEqual(a.VirtualParam(5), 6);
        Assert.AreEqual(b.VirtualParam(5), 7);
    }

    [Test]
    public void Test_Abstract_Method()
    {
        TestAbstract a = new TestDerived();
        Assert.AreEqual(a.Ab(), 17);
    }

    [Test]
    public void Test_Devirtualize_No_Subclasses()
    {
        var s = new StandaloneVirtual();
        Assert.AreEqual(s.Value(), 42);
    }

    [Test]
    public void Test_Devirtualize_Leaf_Override()
    {
        // LeafOverride itself has no subclasses, so a call through a
        // LeafOverride-typed variable should be safe to devirtualize even
        // though the method is declared virtual (and is, two levels up,
        // overridden relative to GrandBase).
        LeafOverride l = new LeafOverride();
        Assert.AreEqual(l.Depth(), 3);
    }

    [Test]
    public void Test_Virtual_Multilevel_Inherited()
    {
        // GrandBase-typed variable holding an instance of the *middle*
        // class, which doesn't override -- must resolve to GrandBase's own
        // implementation via inheritance, not fall through to LeafOverride's.
        GrandBase g = new MidNoOverride();
        Assert.AreEqual(g.Depth(), 1);
    }

    [Test]
    public void Test_Virtual_Multilevel_Overridden()
    {
        // GrandBase-typed variable holding a LeafOverride instance -- the
        // override two levels down the hierarchy must still be reachable
        // through the topmost static type. This is also the case that must
        // NOT be devirtualized: LeafOverride (a subclass of GrandBase, via
        // MidNoOverride) does override Depth, so a GrandBase-typed call site
        // can't be resolved to a single fixed target at compile time.
        GrandBase g = new LeafOverride();
        Assert.AreEqual(g.Depth(), 3);
    }

    [Test]
    public void Test_Virtual_Multilevel_Mid_Not_Overridden()
    {
        // Same idea one level down: a MidNoOverride-typed variable can also
        // hold a LeafOverride instance, so it must not be devirtualized
        // either, even though MidNoOverride itself never overrides Depth.
        MidNoOverride m = new LeafOverride();
        Assert.AreEqual(m.Depth(), 3);
    }
}