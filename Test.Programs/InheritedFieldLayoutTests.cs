using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.ProgramTest;

// A base-class field and a derived-class field in one object: every access
// must find a field at the same offset whether it goes through the base or
// the derived type (TypeExtension.GetInstanceLayout: the base class's
// layout first, then the derived class's own fields).
class LayoutBase
{
    public uint BaseValue;
    public ulong BaseWide;

    public virtual uint Sum() => BaseValue + 100;
    public uint ReadBase() => BaseValue;
    public void WriteBase(uint v) { BaseValue = v; }
    public ulong ReadBaseWide() => BaseWide;
}

class LayoutDerived : LayoutBase
{
    public uint Own;
    public ulong OwnWide;

    public override uint Sum() => BaseValue + Own;
    public uint ReadBaseFromDerived() => BaseValue;
    public void WriteBaseFromDerived(uint v) { BaseValue = v; }
    public uint ReadOwn() => Own;
}

[TestFixture]
public class InheritedFieldLayoutTests
{
    static uint s_next;
    static uint Next() => ++s_next;

    static uint SumOf(LayoutBase b) => b.Sum();
    static uint BaseValueOf(LayoutBase b) => b.BaseValue;
    static ulong BaseWideOf(LayoutBase b) => b.BaseWide;

    [Test]
    public void Fields_Through_Base_And_Derived()
    {
        var d = new LayoutDerived { BaseValue = 1, BaseWide = 300, Own = 2, OwnWide = 500 };
        LayoutBase b = d;
        Assert.IsTrue(d.BaseValue == 1 && b.BaseValue == 1, "base value");
        Assert.IsTrue(d.BaseWide == 300 && b.BaseWide == 300 && BaseWideOf(d) == 300, "base wide");
        Assert.IsTrue(d.Own == 2 && d.OwnWide == 500, "own");
        Assert.IsTrue(b.ReadBase() == 1 && d.ReadBaseFromDerived() == 1 && d.ReadOwn() == 2, "non-virtual reads");
        Assert.IsTrue(b.ReadBaseWide() == 300, "non-virtual wide read");
        Assert.IsTrue(SumOf(d) == 3 && b.Sum() == 3, "virtual");

        b.BaseValue = 10;
        Assert.IsTrue(d.BaseValue == 10 && d.Own == 2, "write through base");
        d.WriteBaseFromDerived(20);
        Assert.IsTrue(b.ReadBase() == 20 && BaseValueOf(d) == 20, "write from derived method");
        b.WriteBase(30);
        Assert.IsTrue(d.ReadBaseFromDerived() == 30 && d.Own == 2 && d.OwnWide == 500 && d.BaseWide == 300, "write from base method");
        d.Own = 5;
        Assert.IsTrue(SumOf(d) == 35, "virtual after writes");
    }

    [Test]
    public void Computed_And_Constant_Initializer_Values()
    {
        s_next = 0;
        var d = new LayoutDerived { Own = Next(), BaseValue = 7, OwnWide = 1000, BaseWide = Next() };
        Assert.IsTrue(d.Own == 1 && d.BaseValue == 7 && d.OwnWide == 1000 && d.BaseWide == 2, "fields");
        Assert.IsTrue(SumOf(d) == 8, "virtual");
    }

    [Test]
    public void Object_Array_Tables()
    {
        LayoutDerived[] derived =
        [
            new LayoutDerived { BaseValue = 1, Own = 2, BaseWide = 3, OwnWide = 4 },
            new LayoutDerived { BaseValue = 5, Own = 6, BaseWide = 7, OwnWide = 8 },
            new LayoutDerived { BaseValue = 9, Own = 10, BaseWide = 11, OwnWide = 12 },
        ];
        Assert.IsTrue(derived[1].BaseValue == 5 && derived[1].Own == 6 && derived[1].BaseWide == 7 && derived[1].OwnWide == 8, "derived table");
        Assert.IsTrue(SumOf(derived[2]) == 19 && derived[2].ReadBase() == 9, "derived via base");

        LayoutBase[] mixed =
        [
            new LayoutDerived { BaseValue = 1, Own = 2 },
            new LayoutDerived { BaseValue = 3, Own = 4 },
            new LayoutBase { BaseValue = 5, BaseWide = 6 },
            new LayoutBase { BaseValue = 7, BaseWide = 8 },
        ];
        Assert.IsTrue(mixed[0].Sum() == 3 && mixed[1].Sum() == 7, "derived in base array");
        Assert.IsTrue(mixed[2].Sum() == 105 && mixed[3].BaseWide == 8, "base in base array");
        Assert.IsTrue(mixed[1].BaseValue == 3, "base field of derived element");
    }
}
