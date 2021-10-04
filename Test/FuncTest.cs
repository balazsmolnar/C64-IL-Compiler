using System;

using NUnit.Framework;
using Assert = C64TestFramework.Assert;


namespace Compiler.Test
{


    class FuncTestA
    {
        public int X;
        public static int Foo() => 17;

        public int Bar() => X;
    }

    [TestFixture]
    class FuncTest
    {
        // [Test]
        // public void FuncTest_Lambda()
        // {
        //     Func<int> func = () => 5;
        //     Assert.AreEqual(func(), 5);
        // }

        [Test]
        public void FuncTest_Static_Method()
        {
            Func<int> func = FuncTestA.Foo;
            Assert.AreEqual(func(), 17);
        }

        [Test]
        public void FuncTest_Instance_Method()
        {
            Func<int> func = (new FuncTestA { X = 19 }).Bar;
            Assert.AreEqual(func(), 19);
        }
    }
}