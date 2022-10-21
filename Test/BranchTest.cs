using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.Test;

[TestFixture]
class BranchTest
{
    [Test]
    public void Unconditional_Branch()
    {
        goto l1;
        Assert.Fail();
        l1:
#pragma warning disable CS0219
        int a = 0;
#pragma warning restore CS0219
    }

    [Test]
    public void Branch_If_Equal()
    {
        int a = 5;
        int b = 6;

        if (a == b)
            Assert.Fail();
    }

    [Test]
    public void Branch_If_Equal_ulong()
    {
        ulong a = 5000;
        ulong b = 6000;

        if (a == b)
            Assert.Fail();
    }

    [Test]
    public void Branch_If_Not_Equal()
    {
        int a = 5;
        int b = 5;

        if (a != b)
            Assert.Fail();
    }

    [Test]
    public void Branch_If_Not_Equal_ulong()
    {
        ulong a = 5000;
        ulong b = 5000;

        if (a != b)
            Assert.Fail();
    }

    [Test]
    public void Branch_If_Greater()
    {
        int a = 5;
        int b = 5;

        if (a > b)
            Assert.Fail();
    }

    [Test]
    public void Branch_If_Greater_ulong()
    {
        ulong a = 5000;
        ulong b = 5000;

        if (a > b)
            Assert.Fail();
    }

    [Test]
    public void Branch_If_Less()
    {
        int a = 6;
        int b = 5;

        if (a < b)
            Assert.Fail();
    }

    [Test]
    public void Branch_If_Less_ulong()
    {
        ulong a = 6;
        ulong b = 5;

        if (a < b)
            Assert.Fail();
    }

    [Test]
    public void Branch_If_Greater_Equal()
    {
        int a = 6;

        if (a >= 6)
        {

        }
        else
        {
            Assert.Fail();
        }
    }

    [Test]
    public void Branch_If_Greater_Equal_ulong()
    {
        ulong a = 6000;

        if (a >= 6000)
        {

        }
        else
        {
            Assert.Fail();
        }
    }

    [Test]
    public void Branch_Combined_ulong()
    {
        ulong a = 6000;

        if (a >= 6000 && a <= 6001)
        {
            return;
        }
        Assert.Fail();
    }

    [Test]
    public void Branch_If_Less_Equal()
    {
        int a = 6;

        if (a <= 6)
        {

        }
        else
        {
            Assert.Fail();
        }
    }

    [Test]
    public void Branch_If_Less_Equal_ulong()
    {
        ulong a = 6000;

        if (a <= 6000)
        {

        }
        else
        {
            Assert.Fail();
        }
    }

    [Test]
    public void For_Loop()
    {
        int i = 0;
        for (; i < 10; i++)
        {

        }
        Assert.AreEqual(i, 10);
    }

    [Test]
    public void For_Loop_ulong()
    {
        ulong i = 0;
        for (; i < 1000; i++)
        {

        }

        if (i != 1000)
            Assert.Fail();
    }

    [Test]
    public void While_Loop()
    {
        int i = 0;
        while (i < 10)
        {
            i++;
        }
        Assert.AreEqual(i, 10);
    }

    [Test]
    public void While_Loop_With_Break()
    {
        int i = 0;
        while (true)
        {
            i++;
            if (i == 10)
                break;
        }
        Assert.AreEqual(i, 10);
    }

    [Test]
    public void Branch_Multiple_Conditions()
    {
        int a = 5;
        if (a > 3 && a < 6)
        {

        }
        else
        {
            Assert.Fail();
        }
    }

    [Test]
    public void Branch_Multiple_Conditions_2()
    {
        int a = 5;
        if (a > 3 || a < 6)
        {

        }
        else
        {
            Assert.Fail();
        }
    }

    [Test]
    public void Branch_Multiple_Conditions_3()
    {
        int a = 5;
        if (a > 6 || a < 1)
        {
            Assert.Fail();
        }
    }

    [Test]
    public void Test_Tenary_Operator()
    {
        uint a = 5;
        var b = a == 5 ? 1 : 0;
        Assert.AreEqual(b, 1);
    }

    [Test]
    public void Test_Tenary_Operator_2()
    {
        uint a = 5;
        var b = a == 6 ? 1 : 0;
        Assert.AreEqual(b, 0);
    }

    [Test]
    public void Test_Switch()
    {
        uint a = 1;
        bool b = false;
        switch (a)
        {
            case 0:
                Assert.Fail();
                break;
            case 1:
                b = true;
                break;
            case 2:
                Assert.Fail();
                break;
        }
        Assert.IsTrue(b);
    }

    [Test]
    public void Test_Switch_Last_Label()
    {
        uint a = 3;
        bool b = false;
        switch (a)
        {
            case 0:
                Assert.Fail();
                break;
            case 1:
                Assert.Fail();
                break;
            case 2:
                Assert.Fail();
                break;
            case 3:
                b = true;
                break;
        }
        Assert.IsTrue(b);
    }

    [Test]
    public void Test_Switch_Multiple_Labels()
    {
        uint a = 1;
        bool b = false;
        switch (a)
        {
            case 0:
            case 1:
                b = true;
                break;
            case 2:
                Assert.Fail();
                break;
            case 3:
                Assert.Fail();
                break;

        }
        Assert.IsTrue(b);
    }

    // [Test]
    // public void Test_Switch_Default()
    // {
    //     uint a = 27;
    //     bool b = false;
    //     switch (a)
    //     {
    //         case 0:
    //         case 1:
    //             Assert.Fail();
    //             break;
    //         case 2:
    //             Assert.Fail();
    //             break;
    //         default:
    //             b = true;
    //             break;
    //     }
    //     Assert.IsTrue(b);
    // }


}