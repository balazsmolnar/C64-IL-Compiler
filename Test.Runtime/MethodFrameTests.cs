using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.RuntimeTest;

// Method entry/exit (asm/helper/localsStack.asm, asm/helper/frame.asm):
// every `ret` jumps into a shared Method_Exit_N tail (N = frame size; 2..8
// have their own entry, larger frames go through Method_Exit_A), and a
// static, void, parameterless method is "frameless" -- its return address
// stays on the hardware stack and it leaves through rts or Method_Leave_N.
// Every helper here is called from more than one place so the inliner
// leaves it a real subroutine.
[TestFixture]
public class MethodFrameTests
{
    static uint counter;
    static uint[] kept;

    // Frameless, no locals: a bare rts.
    static void Bump()
    {
        counter = counter + 1;
    }

    // Frameless with locals and an early return (Method_Leave_N).
    static void BumpBy3()
    {
        uint a = 1;
        uint b = 2;
        if (counter > 200)
            return;
        counter = counter + a + b;
    }

    // Frameless with a wide set of locals (Method_Leave_A) and a reference
    // local, released on the way out.
    static void BumpWide()
    {
        ulong big = 1000;
        uint x = 4;
        uint y = 5;
        uint[] arr = new uint[2];
        arr[0] = x + y;
        if (big < 10)
            return;
        counter = counter + arr[0];
        Bump();
    }

    // Frameless helpers calling each other.
    static void Nested()
    {
        Bump();
        BumpBy3();
        Bump();
    }

    static void KeepArray()
    {
        uint[] a = new uint[3];
        a[2] = 77;
        kept = a;
    }

    // Normal frames of growing size, each with an early and a late return.
    static uint Frame3(uint a)
    {
        if (a == 0)
            return 100;
        return a + 1;
    }

    static uint Frame5(uint a, uint b, uint c)
    {
        if (a == 0)
            return 100;
        return a + b + c;
    }

    static uint Frame8(uint a, uint b, uint c, uint d, uint e)
    {
        uint t = a + b;
        if (t == 0)
            return 100;
        return t + c + d + e;
    }

    static uint Frame12(uint a, uint b, uint c, uint d, uint e, uint f)
    {
        uint t = a + b;
        uint u = c + d;
        uint v = e + f;
        uint w = t + u;
        if (w == 0)
            return 100;
        return w + v;
    }

    [Test]
    public void Frameless_Methods_Run_And_Return()
    {
        counter = 0;
        Bump();
        Bump();
        Assert.AreEqual(counter, 2u);
        BumpBy3();
        Assert.AreEqual(counter, 5u);
        BumpWide();
        Assert.AreEqual(counter, 15u);
        Nested();
        Assert.AreEqual(counter, 20u);
    }

    [Test]
    public void Frameless_Methods_Keep_The_Callers_Locals()
    {
        uint before = 11;
        uint after = 22;
        counter = 199;
        BumpBy3();
        Nested();
        BumpWide();
        Assert.AreEqual(before, 11u);
        Assert.AreEqual(after, 22u);
        // 199 -> 202 -> (Bump 203, BumpBy3 skipped, Bump 204) -> +9 +1
        Assert.AreEqual(counter, 214u);
    }

    [Test]
    public void Frameless_Method_Reference_Local_Survives_In_A_Field()
    {
        KeepArray();
        KeepArray();
        Assert.AreEqual(kept[2], 77u);
    }

    [Test]
    public void Every_Frame_Size_Returns_Its_Value()
    {
        uint keep = 5;
        Assert.AreEqual(Frame3(0), 100u);
        Assert.AreEqual(Frame3(4), 5u);
        Assert.AreEqual(Frame5(0, 1, 2), 100u);
        Assert.AreEqual(Frame5(1, 2, 3), 6u);
        Assert.AreEqual(Frame8(0, 0, 1, 1, 1), 100u);
        Assert.AreEqual(Frame8(1, 2, 3, 4, 5), 15u);
        Assert.AreEqual(Frame12(0, 0, 0, 0, 1, 1), 100u);
        Assert.AreEqual(Frame12(1, 2, 3, 4, 5, 6), 21u);
        Assert.AreEqual(Frame3(Frame5(1, 1, Frame8(1, 1, 1, 1, Frame12(1, 1, 1, 1, 1, 1)))), 13u);
        Assert.AreEqual(keep, 5u);
    }
}
