using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.Test
{
    class Test
    {
        public int field1;
        public int field2;
        public static int staticField;
        public int Property1 { get; set; }

        public ulong Ulong1;
        private int prop2;

        public ulong Ulong2;

        public int Property2
        {
            get { return prop2; }
            set { prop2 = value + 2; }
        }

        public static int Add(int a, int b) => a + b;

        public void IncField1()
        {
            field1++;
        }
    }

    [TestFixture]
    class ObjectTest
    {
        [Test]
        public void Set_Get_Field()
        {
            var sut = new Test();
            sut.field1 = 5;
            sut.field2 = 3;
            Assert.AreEqual(sut.field1, 5);
            Assert.AreEqual(sut.field2, 3);
        }

        [Test]
        public void Set_Get_Property()
        {
            var sut = new Test();
            sut.Property1 = 5;
            sut.Property2 = 2;
            Assert.AreEqual(sut.Property1, 5);
            Assert.AreEqual(sut.Property2, 4);
        }

        [Test]
        public void Object_Initializer()
        {
            var sut = new Test()
            {
                field1 = 3,
                field2 = 5,
                Property1 = 6
            };
            Assert.AreEqual(sut.field1, 3);
            Assert.AreEqual(sut.field2, 5);
            Assert.AreEqual(sut.Property1, 6);
        }

        [Test]
        public void Set_Static_Field()
        {
            Test.staticField = 99;
            Assert.AreEqual(Test.staticField, 99);
        }

        [Test]
        public void Call_Static_Method()
        {
            var result = Test.Add(3, 5);
            Assert.AreEqual(result, 8);
        }

        [Test]
        public void Call_Instance_Method()
        {
            var sut = new Test();
            sut.field1 = 6;
            sut.IncField1();
            Assert.AreEqual(sut.field1, 7);
        }

        [Test]
        public void Two_Instances()
        {
            var sut1 = new Test()
            {
                field1 = 5
            };
            var sut2 = new Test()
            {
                field1 = 6
            };
            Assert.AreEqual(sut1.field1, 5);
            Assert.AreEqual(sut2.field1, 6);
        }

        [Test]
        public void Ulong_Property()
        {
            var sut1 = new Test();
            sut1.Ulong1 = 1000;
            sut1.Ulong2 = 2000;
            if (sut1.Ulong1 != 1000)
                Assert.Fail();
            if (sut1.Ulong2 != 2000)
                Assert.Fail();

            sut1.Ulong1 += 5;
            if (sut1.Ulong1 != 1005)
                Assert.Fail();


        }


    }
}