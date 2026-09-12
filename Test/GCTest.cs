using System;
using C64Lib;
using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.Test;

public class TestObject
{

    public int Id;

    public TestObject Child;
}

public class TestObjectWithLong
{
    public int Id;
    public long Value;
    public int Extra;
}

[TestFixture]
public class GCTest
{
    static TestObject s_a1;
    static TestObject s_a2;

    [Test]
    public void Single_Instance_GC()
    {
        var a = new TestObject();
        var aid = C64.Debug.GetObjectId(a);
        a = null;
        GC.Collect();
        Assert.IsFalse(C64.Debug.IsAlive(aid), "instance should not be alive");
    }

    [Test]
    public void Change_Ref_Local_Variable()
    {
        var a = new TestObject();
        var aid = C64.Debug.GetObjectId(a);
        a = new TestObject();
        GC.Collect();
        Assert.IsFalse(C64.Debug.IsAlive(aid), "instance should not be alive");
    }

    [Test]
    public void Two_Instances_First_GCd_Static_Field()
    {
        s_a1 = new TestObject() { Id = 4 };
        s_a2 = new TestObject() { Id = 5 };

        var objId = C64.Debug.GetObjectId(s_a1);
        var objId2 = C64.Debug.GetObjectId(s_a2);
        Assert.IsTrue(C64.Debug.IsAlive(objId), "instance should be alive");
        s_a1 = null;
        GC.Collect();

        Assert.IsFalse(C64.Debug.IsAlive(objId), "instance should not be alive");
        Assert.IsTrue(C64.Debug.IsAlive(objId2), "instance should be alive");
        Assert.AreEqual(s_a2.Id, 5);
    }

    public void Foo(TestObject t)
    {
        var b = t;
    }

    [Test]
    public void Passed_As_Parameter_Deferenced()
    {
        var a = new TestObject();
        var aid = C64.Debug.GetObjectId(a);
        Foo(a);
        a = new TestObject();
        GC.Collect();
        Assert.IsFalse(C64.Debug.IsAlive(aid), "instance should not be alive");
    }

    [Test]
    public void Array_Root_in_Local_Var()
    {
        var a = new TestObject[] { new TestObject() { Id = 23 } };
        var aid = C64.Debug.GetObjectId(a[0]);
        GC.Collect();
        Assert.IsTrue(C64.Debug.IsAlive(aid), "instance should be alive");
        a = null;
        GC.Collect();
        Assert.IsFalse(C64.Debug.IsAlive(aid), "instance should not be alive");
    }

    [Test]
    public void Hierarchies_Local_Variable_Root()
    {
        var a1 = new TestObject() { Id = 1, Child = new TestObject() };
        var a2 = new TestObject() { Id = 2, Child = a1 };
        var a3 = new TestObject() { Id = 3, Child = a2 };

        var b1 = new TestObject() { Id = 4 };
        var b2 = new TestObject() { Id = 5, Child = b1 };
        var b3 = new TestObject() { Id = 6, Child = b2 };

        var a1id = C64.Debug.GetObjectId(a1);
        var a2id = C64.Debug.GetObjectId(a2);
        var a3id = C64.Debug.GetObjectId(a3);

        var b1id = C64.Debug.GetObjectId(b1);
        var b2id = C64.Debug.GetObjectId(b2);
        var b3id = C64.Debug.GetObjectId(b3);

        a1 = a2 = a3 = null;
        b1 = b2 = null;

        GC.Collect();

        Assert.IsFalse(C64.Debug.IsAlive(a1id), "IsAlive(a1id)");
        Assert.IsFalse(C64.Debug.IsAlive(a2id), "IsAlive(a2id)");
        Assert.IsFalse(C64.Debug.IsAlive(a3id), "IsAlive(a3id)");

        Assert.AreEqual(b3.Id, 6, "b3.Id==6");
        Assert.AreEqual(b3.Child.Id, 5, "b3.Child.Id == 5");
        Assert.AreEqual(b3.Child.Child.Id, 4, "b3.Child.Child.Id == 4");

        Assert.IsTrue(C64.Debug.IsAlive(b1id), "IsAlive(b1id)");
        Assert.IsTrue(C64.Debug.IsAlive(b2id), "IsAlive(b2id)");
        Assert.IsTrue(C64.Debug.IsAlive(b3id), "IsAlive(b3id)");
    }

    [Test]
    public void Cyclic_Reference_Collected()
    {
        var a = new TestObject() { Id = 1 };
        var b = new TestObject() { Id = 2 };
        a.Child = b;
        b.Child = a;

        var aid = C64.Debug.GetObjectId(a);
        var bid = C64.Debug.GetObjectId(b);

        a = null;
        b = null;
        GC.Collect();

        Assert.IsFalse(C64.Debug.IsAlive(aid), "instance a should not be alive");
        Assert.IsFalse(C64.Debug.IsAlive(bid), "instance b should not be alive");
    }

    [Test]
    public void Double_Collect_Is_Idempotent()
    {
        var live = new TestObject() { Id = 7 };
        var dead = new TestObject() { Id = 8 };
        var liveId = C64.Debug.GetObjectId(live);
        var deadId = C64.Debug.GetObjectId(dead);
        dead = null;

        GC.Collect();
        Assert.IsTrue(C64.Debug.IsAlive(liveId), "live should be alive after first collect");
        Assert.IsFalse(C64.Debug.IsAlive(deadId), "dead should not be alive after first collect");

        GC.Collect();
        Assert.IsTrue(C64.Debug.IsAlive(liveId), "live should be alive after second collect");
        Assert.AreEqual(live.Id, 7);
        Assert.IsFalse(C64.Debug.IsAlive(deadId), "dead should still not be alive after second collect");
    }

    [Test]
    public void Sustained_Allocate_And_Collect_Reclaims_Slots()
    {
        TestObject current = null;
        uint lastId = 0;
        for (int i = 0; i < 60; i++)
        {
            current = new TestObject();
            lastId = C64.Debug.GetObjectId(current);
            GC.Collect();
        }

        Assert.IsTrue(C64.Debug.IsAlive(lastId), "final instance should be alive");
    }

    [Test]
    public void Mixed_Size_Objects_Survive_Compaction()
    {
        var a = new TestObject() { Id = 1 };
        var b = new TestObjectWithLong() { Id = 2, Value = 111, Extra = 21 };
        var c = new TestObject() { Id = 3 };
        var d = new TestObjectWithLong() { Id = 4, Value = 222, Extra = 42 };

        var bId = C64.Debug.GetObjectId(b);
        var dId = C64.Debug.GetObjectId(d);

        a = null;
        c = null;
        GC.Collect();

        Assert.IsTrue(C64.Debug.IsAlive(bId), "b should be alive");
        Assert.IsTrue(C64.Debug.IsAlive(dId), "d should be alive");
        Assert.AreEqual(b.Id, 2);
        Assert.AreEqual((int)b.Value, 111);
        Assert.AreEqual(b.Extra, 21);
        Assert.AreEqual(d.Id, 4);
        Assert.AreEqual((int)d.Value, 222);
        Assert.AreEqual(d.Extra, 42);
    }

    [Test]
    public void Array_Element_Roots_Further_Object_Graph()
    {
        var survivorChild = new TestObject() { Id = 10 };
        var survivor = new TestObject() { Id = 11, Child = survivorChild };
        var dropped = new TestObject() { Id = 12 };

        var a = new TestObject[] { survivor, dropped };

        var survivorId = C64.Debug.GetObjectId(survivor);
        var survivorChildId = C64.Debug.GetObjectId(survivorChild);
        var droppedId = C64.Debug.GetObjectId(dropped);

        survivor = null;
        survivorChild = null;
        dropped = null;
        a[1] = null;

        GC.Collect();

        Assert.IsTrue(C64.Debug.IsAlive(survivorId), "surviving array element should be alive");
        Assert.IsTrue(C64.Debug.IsAlive(survivorChildId), "surviving element's child should be alive");
        Assert.IsFalse(C64.Debug.IsAlive(droppedId), "dropped array element should not be alive");
        Assert.AreEqual(a[0].Id, 11);
        Assert.AreEqual(a[0].Child.Id, 10);
    }
}