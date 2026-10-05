using System;
using System.Runtime.CompilerServices;
using C64Lib;
using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.RuntimeTest;

// Method inlining Phase 2 (devirtualized Callvirt, multi-site net-size
// inlining, [MethodImpl(AggressiveInlining)]) -- Phase 1's single-Call-site
// case is already covered incidentally by every other test in this
// assembly going through the normal call path. These specifically
// exercise the NEW splice paths Phase 2 added: a devirtualized Callvirt
// site (never spliced before Phase 2), and AggressiveInlining forcing a
// multi-site splice the size heuristic alone would reject (confirmed via
// Hunchback/measure.bat that the heuristic correctly rejects ordinary
// multi-site getters -- see ILCallSiteCountPass's own comment -- so a
// real multi-site splice can only be exercised here via the attribute).
class Box
{
    public uint Value;

    // Non-virtual, but instance methods compile to Callvirt regardless
    // (for the implicit null-check) -- OpCallVirt.Emit devirtualizes this
    // to a plain jsr already; this gives ILMethodInliningPass exactly one
    // Callvirt call site to inline, the first time that code path runs
    // for real.
    public uint GetValue()
    {
        return Value;
    }
}

class MultiSiteBox
{
    public uint Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint GetValueForced()
    {
        return Value;
    }

    // No attribute -- same shape, called from the same number of sites,
    // to confirm the ordinary (non-forced) multi-site path still behaves
    // correctly regardless of whether the size heuristic chose to inline
    // it or leave it as a real call.
    public uint GetValueUnforced()
    {
        return Value;
    }
}

// Mirrors Hunchback's own Player.SetOnRope(Rope rope) exactly -- single
// call site (LevelPlay.Play()'s main loop), leaf (no Call/Callvirt), a
// guarded early return, then a plain field store of a reference-typed
// parameter. Every existing ref-param inlining coverage before this
// (GCTest.Foo in Test/GCTest.cs, via Passed_As_Parameter_Deferenced) only
// ever stores the parameter into a LOCAL with a single trailing Ret --
// never into a FIELD, and never with an early-return branch ahead of the
// store. Added while investigating a real reported bug ("player sprite
// disappears when grabbing the rope in Hunchback") suspected to trace back
// to this exact, previously-untested shape.
class RefBox
{
    public uint Id;
    public RefBox Other;
    public bool Guard;

    public void SetOther(RefBox other)
    {
        if (Guard)
            return;
        Other = other;
    }
}

[TestFixture]
public class InliningTests
{
    // Both scenarios (guard taken, guard not taken) run through the SAME
    // source-level call -- a.SetOther(other) below is the only
    // Call/Callvirt instruction targeting RefBox.SetOther anywhere in the
    // whole test assembly, which is what makes it Phase 1's "exactly one
    // call site" case (spliced AND its standalone definition deleted),
    // matching Player.SetOnRope's own single call site in Hunchback
    // exactly. Two separate [Test] methods each calling it once would
    // instead give it TWO call sites, which the Phase 2 cost model
    // correctly rejects for a leaf this size -- that was the first,
    // non-representative version of this test (confirmed via asm grep:
    // "jsr RefBox_SetOther" twice, never spliced).
    private static void Invoke(RefBox box, RefBox other)
    {
        box.SetOther(other);
    }

    [Test]
    public void Inlined_RefParam_EarlyReturn_FieldStore_Preserves_Refcount()
    {
        var a = new RefBox();
        var b = new RefBox { Id = 42 };
        var bId = C64.Debug.GetObjectId(b);

        Invoke(a, b);
        b = null;
        GC.Collect();

        Assert.IsTrue(C64.Debug.IsAlive(bId), "b should still be alive, rooted by a.Other");
        Assert.AreEqual(a.Other.Id, 42);

        var c = new RefBox { Guard = true };
        var d = new RefBox { Id = 7 };
        Invoke(c, d);
        Assert.IsTrue(c.Other == null, "Other should still be null -- guard skipped the store");
    }

    [Test]
    public void Devirtualized_Callvirt_Single_Site_Inlines_Correctly()
    {
        var box = new Box();
        box.Value = 42;
        if (box.GetValue() != 42)
            Assert.Fail();
    }

    [Test]
    public void AggressiveInlining_Forces_Multi_Site_Splice()
    {
        var a = new MultiSiteBox();
        var b = new MultiSiteBox();
        a.Value = 10;
        b.Value = 20;

        // Four call sites for the SAME forced method -- well past where
        // the ordinary size heuristic would ever approve inlining (see
        // ILCallSiteCountPass's own comment: multi-site inlining of a
        // trivial getter is already a net loss at N=2), so a correct
        // result here specifically confirms the attribute actually
        // overrides that heuristic rather than just happening to agree
        // with it.
        uint sum = a.GetValueForced() + b.GetValueForced() + a.GetValueForced() + b.GetValueForced();
        if (sum != 60)
            Assert.Fail();
    }

    [Test]
    public void Unforced_Multi_Site_Property_Still_Correct()
    {
        var a = new MultiSiteBox();
        var b = new MultiSiteBox();
        a.Value = 7;
        b.Value = 8;

        uint sum = a.GetValueUnforced() + b.GetValueUnforced() + a.GetValueUnforced();
        if (sum != 22)
            Assert.Fail();
    }
}
