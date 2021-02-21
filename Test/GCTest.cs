using System;
using C64Lib;
using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.Test
{

    public class TestObject
    {
        public int Id;

        public TestObject Child;
    }

    [TestFixture]
    public class GCTest
    {
        static TestObject s_a1;
        static TestObject s_a2;

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
    }
}