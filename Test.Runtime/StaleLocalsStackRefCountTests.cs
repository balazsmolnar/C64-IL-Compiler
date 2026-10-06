using System;
using System.Runtime.CompilerServices;
using C64Lib;
using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.RuntimeTest;

// Regression tests for a real, found-and-fixed compiler bug, not a
// hypothetical one: localsStack (asm/helper/objectTables.asm's
// `.fill 256`) is one shared, reused 256-byte region across every
// method call at every stack depth -- zero-filled once at program
// load, but NEVER re-zeroed between calls. Before the fix
// (CompilerMethodContext.GetLocalRefPositions, asm/helper/localsStack.asm's
// init_locals_pull_parameters/init_locals_pull_parameters_inline), a
// reference-typed LOCAL's own slot was left holding whatever the
// previous, unrelated call that happened to reuse the same physical
// stack depth left behind -- and since findEmptySlot
// (asm/helper/object.asm) always recycles the lowest-numbered free
// handle, that stale number was very likely to have been reassigned to
// a DIFFERENT, currently-live object by the time this local's first
// write (or an early-return method_exit, for a local never assigned on
// that path) ran, decrementing that unrelated object's refcount by
// mistake -- a premature free of something still legitimately
// referenced elsewhere, confirmed to actually happen before this fix,
// not just theorized.
//
// Also guarded by asm/helper/object.asm's DecRefCountIfAllocated (skips
// the decrement entirely if the stale handle's slot isn't even
// currently allocated) -- a narrower, second line of defense for handle
// 0 specifically (never a real allocated slot, findEmptySlot's own
// leading `inx` before its first check) that the prologue-zeroing fix
// doesn't make redundant on its own.
public class StaleLocalsStackRefCountObj
{
    public int Id;
}

[TestFixture]
public class StaleLocalsStackRefCountTests
{
    static StaleLocalsStackRefCountObj s_holder;

    // Two call sites each throughout this file (never AggressiveInlining,
    // never a single call site) so these stay real, standalone methods
    // reusing the same physical locals-stack depth across separate
    // calls -- the exact shape the bug needed.
    static void StoreAndDrop()
    {
        StaleLocalsStackRefCountObj local = new StaleLocalsStackRefCountObj();
        local.Id = 1;
    }

    static void StoreAndDropOther()
    {
        StaleLocalsStackRefCountObj local = new StaleLocalsStackRefCountObj();
        local.Id = 2;
    }

    [Test]
    public void StaleLocalsStackByte_DoesNotCorruptRecycledHandle()
    {
        StoreAndDrop();
        StoreAndDrop();
        GC.Collect();

        s_holder = new StaleLocalsStackRefCountObj { Id = 99 };
        var hid = C64.Debug.GetObjectId(s_holder);
        Assert.IsTrue(C64.Debug.IsAlive(hid), "should be alive right after allocation");

        // StoreAndDropOther's own `local` physically occupies the same
        // stack depth StoreAndDrop's did -- its first write there must
        // not decrement s_holder's (recycled) handle.
        StoreAndDropOther();
        StoreAndDropOther();

        GC.Collect();

        Assert.IsTrue(C64.Debug.IsAlive(hid), "must still be alive -- an unrelated method's own local write must not touch it");
        if (s_holder.Id != 99)
            Assert.Fail();
    }

    static StaleLocalsStackRefCountObj s_earlyReturnHolder;

    // The SAME method, called repeatedly, instead of two separate
    // methods -- removes any question of whether two DIFFERENT methods'
    // own `local` really lands at the identical stack position (an
    // earlier draft used two methods and could not be made to reliably
    // land on the same slot). A counter makes its own first two calls
    // really assign `local`, then every later call take the early
    // return before ever touching it -- same method, same frame, so
    // `local`'s own rel_pos is identical by construction across every
    // call.
    static int s_earlyReturnCounter;

    static void SometimesAssign()
    {
        s_earlyReturnCounter++;
        if (s_earlyReturnCounter > 2)
            return;
        StaleLocalsStackRefCountObj local = new StaleLocalsStackRefCountObj();
        local.Id = 3;
    }

    [Test]
    public void EarlyReturn_Before_Local_Ever_Assigned_Does_Not_Decrement_Recycled_Handle()
    {
        // Calls 1 and 2: real assignment, so `local`'s slot ends up
        // holding a real handle that then gets freed.
        SometimesAssign();
        SometimesAssign();
        GC.Collect();

        s_earlyReturnHolder = new StaleLocalsStackRefCountObj { Id = 77 };
        var hid = C64.Debug.GetObjectId(s_earlyReturnHolder);
        Assert.IsTrue(C64.Debug.IsAlive(hid), "should be alive right after allocation");

        // Call 3: early return BEFORE `local` is ever touched -- without
        // the prologue-zeroing fix, method_exit's own ref_list decrement
        // still fires on whatever stale handle is physically sitting in
        // `local`'s slot from calls 1/2 (by now s_earlyReturnHolder's
        // own, recycled handle). EXACTLY one such call matters, not an
        // arbitrary count: each spurious decrement is relative to
        // wherever the previous one left the count, so only the FIRST
        // one landing on a real count of 1 drives it to exactly 0 --
        // a second spurious decrement afterward would underflow it past
        // zero into a seemingly-valid nonzero value instead (255, then
        // 254, ...), masking the corruption from GC.Collect() below by
        // accident. Confirmed directly: this test only actually caught
        // the regression with exactly one call here, not two or three
        // (an earlier draft tried three, reasoning "odd count" was
        // enough, and was wrong -- it isn't odd/even that matters, it's
        // landing exactly on the single real decrement). Three total
        // calls to this method across the test (two real, one early-
        // return) is still comfortably past N=1, so this stays a real,
        // standalone method rather than a single-site inlining win.
        SometimesAssign();
        GC.Collect();

        Assert.IsTrue(C64.Debug.IsAlive(hid), "an early return before the local was ever assigned must not decrement an unrelated recycled handle");
        if (s_earlyReturnHolder.Id != 77)
            Assert.Fail();
    }

    class MaterializingInlineHolder
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StoreAndDropInlined()
        {
            StaleLocalsStackRefCountObj local = new StaleLocalsStackRefCountObj();
            local.Id = 4;
        }
    }

    static StaleLocalsStackRefCountObj s_inlineHolder;

    [Test]
    public void Materializing_Inlined_Callee_With_Local_Does_Not_Corrupt_Recycled_Handle()
    {
        // Single call site + AggressiveInlining forces the MATERIALIZING
        // splice path (OpInlinePrologue), not a real jsr'd method --
        // exercises init_locals_pull_parameters_inline's own zeroing
        // fix specifically, not init_locals_pull_parameters's.
        MaterializingInlineHolder.StoreAndDropInlined();
        GC.Collect();

        s_inlineHolder = new StaleLocalsStackRefCountObj { Id = 55 };
        var hid = C64.Debug.GetObjectId(s_inlineHolder);
        Assert.IsTrue(C64.Debug.IsAlive(hid), "should be alive right after allocation");

        // A second, different call site for the SAME inlined shape --
        // each splice gets its own fresh copy of the prologue, so this
        // confirms the fix applies per-site, not just once.
        MaterializingInlineHolder.StoreAndDropInlined();
        GC.Collect();

        Assert.IsTrue(C64.Debug.IsAlive(hid), "a materializing-inlined callee's own local must not corrupt an unrelated recycled handle");
        if (s_inlineHolder.Id != 55)
            Assert.Fail();
    }

    static uint StoreAndDropReturningId()
    {
        StaleLocalsStackRefCountObj local = new StaleLocalsStackRefCountObj();
        local.Id = 5;
        return C64.Debug.GetObjectId(local);
    }

    static uint StoreAndDropReturningIdOther()
    {
        StaleLocalsStackRefCountObj local = new StaleLocalsStackRefCountObj();
        local.Id = 6;
        return C64.Debug.GetObjectId(local);
    }

    // Sanity check: the fix must not break NORMAL refcounting -- a
    // local that really is the only reference to an object must still
    // correctly release it once the method returns and nothing else
    // holds it.
    [Test]
    public void Normal_Local_Lifecycle_Still_Releases_On_Scope_Exit()
    {
        var id = StoreAndDropReturningId();
        GC.Collect();
        if (C64.Debug.IsAlive(id))
            Assert.Fail();

        // And a second, different method reusing the same stack depth
        // afterward -- its OWN local must also release correctly.
        var id2 = StoreAndDropReturningIdOther();
        GC.Collect();
        if (C64.Debug.IsAlive(id2))
            Assert.Fail();
    }
}
