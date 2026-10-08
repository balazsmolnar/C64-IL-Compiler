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

    // Real correctness coverage for ILMethodIntOperandFusionOptimizer's
    // branch-truthiness fusion (Compiler/ILMethodIntOperandFusionOptimizer.cs):
    // a bare `if (x)`/`if (!x)` on a simple leaf (local, field, or
    // parameter) now loads straight into register A instead of round-
    // tripping through the hardware stack via branch_true8/branch_false8's
    // own stack_pull_int_a. Each checked both ways (condition true and
    // false) so a wrong branch direction (bne vs beq swapped) would fail
    // exactly one of the pair, not silently pass by accident.
    bool field_;

    [Test]
    public void Branch_Truthiness_Local_True()
    {
        bool a = true;
        if (!a)
            Assert.Fail();
    }

    [Test]
    public void Branch_Truthiness_Local_False()
    {
        bool a = false;
        if (a)
            Assert.Fail();
    }

    [Test]
    public void Branch_Truthiness_Field_True()
    {
        field_ = true;
        if (!field_)
            Assert.Fail();
    }

    [Test]
    public void Branch_Truthiness_Field_False()
    {
        field_ = false;
        if (field_)
            Assert.Fail();
    }

    static void AssertParamTrue(bool p)
    {
        if (!p)
            Assert.Fail();
    }

    static void AssertParamFalse(bool p)
    {
        if (p)
            Assert.Fail();
    }

    [Test]
    public void Branch_Truthiness_Param_True()
    {
        AssertParamTrue(true);
    }

    [Test]
    public void Branch_Truthiness_Param_False()
    {
        AssertParamFalse(false);
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

    [Test]
    public void Test_Switch_Default()
    {
        uint a = 27;
        bool b = false;
        switch (a)
        {
            case 0:
            case 1:
                Assert.Fail();
                break;
            case 2:
                Assert.Fail();
                break;
            default:
                b = true;
                break;
        }
        Assert.IsTrue(b);
    }


}