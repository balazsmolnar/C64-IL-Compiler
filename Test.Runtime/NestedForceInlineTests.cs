using System.Runtime.CompilerServices;
using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.RuntimeTest;

// Runtime correctness for "nested" [MethodImpl(AggressiveInlining)] --
// see Compiler.UnitTests/NestedForceInlineTests.cs for the structural
// ("which methods actually got spliced, which stayed real") side of
// this and that file's own comment for why the scenario matters: a
// force-inlined method that itself calls something else is rejected by
// ILCallSiteCountPass's hard leaf precondition regardless of the
// attribute, and stays a real, standalone, jsr-called method -- these
// only check the observable VALUE stays correct end to end despite that
// silent rejection (no diagnostic is raised; the attribute just has no
// effect on a non-leaf method).
class Chain
{
    public uint value_;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint GetValue() => value_;

    // Also force-inline, but calls GetValue() -- not a leaf, stays real.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint GetValuePlusOne() => GetValue() + 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint C() => value_;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint B() => C() + 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint A() => B() + 1;
}

[TestFixture]
public class NestedForceInlineTests
{
    [Test]
    public void Non_Leaf_Force_Inline_Still_Computes_Correct_Value()
    {
        var h = new Chain { value_ = 41 };
        if (h.GetValuePlusOne() != 42)
            Assert.Fail();
    }

    [Test]
    public void Two_Level_Force_Inline_Chain_Still_Computes_Correct_Value()
    {
        var h = new Chain { value_ = 40 };
        if (h.A() != 42)
            Assert.Fail();
    }
}
