using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.Test;

// Static (compiled) frames (Compiler/ILStaticFramePass.cs): every helper
// below that is not a test entry point gets fixed-address parameters and
// locals, overlaid along the call graph -- helpers called one after the
// other from the same caller share the same bytes. Each helper calls
// something, so the inliner leaves it a real subroutine.
class FrameNode
{
    public uint value_;
    public FrameNode next_;

    // Instance method in a static frame: `this`, a reference parameter and
    // a reference local.
    public uint SumWith(FrameNode other)
    {
        FrameNode tmp = new FrameNode { value_ = other.value_ };
        StaticFrameTests.Touch();
        return value_ + tmp.value_;
    }

    // Enough parameter bytes and call sites for the callee-pulls
    // convention (StaticFrameInfo.CalleePulls): the method pulls its own
    // arguments, references included, and returns through the saved
    // return address.
    public uint Pick(FrameNode a, FrameNode b, uint which)
    {
        FrameNode chosen = which == 0 ? a : b;
        StaticFrameTests.Touch();
        return chosen.value_ + value_;
    }

    public FrameNode Link(FrameNode other)
    {
        StaticFrameTests.Touch();
        next_ = other;
        return this;
    }
}

[TestFixture]
public class StaticFrameTests
{
    static uint touched;
    static ulong wideField;

    public static void Touch()
    {
        touched = touched + 1;
    }

    static uint Twice(uint v)
    {
        uint r = v + v;
        Touch();
        return r;
    }

    static uint Minus(uint a, uint b)
    {
        uint r = a - b;
        Touch();
        return r;
    }

    // Called with arguments that are themselves calls: Twice/Minus share
    // their frame bytes with Combine's (same caller), which is only safe
    // because the arguments land in Combine's slots after every one of
    // them is evaluated.
    static uint Combine(uint a, uint b, uint c)
    {
        uint t = a + b;
        Touch();
        return t + c;
    }

    // A float result can only come back on the stack: callee-pulls.
    static float Scale(float v, float k)
    {
        float r = v * k;
        Touch();
        return r;
    }

    static ulong Widen(ulong a, uint b)
    {
        ulong r = a + b;
        Touch();
        return r;
    }

    // Calls two helpers whose frames overlay each other (both sit right
    // above this one); its own local must survive both.
    static uint Outer(uint x)
    {
        uint keep = x + 1;
        uint p = Twice(x);
        uint q = Minus(keep, 1);
        return keep + p + q;
    }

    // A reference local in a static frame, assigned many times: each
    // assignment must release the previous object (otherwise the 255
    // object handles run out), and the exit must release the last one.
    static uint MakeAndRead(uint v)
    {
        FrameNode n = new FrameNode { value_ = v };
        n = new FrameNode { value_ = n.value_ + 1 };
        Touch();
        return n.value_;
    }

    [Test]
    public void Nested_Calls_In_Arguments()
    {
        uint x = 3;
        uint y = 10;
        Assert.AreEqual(Combine(Twice(x), Minus(y, 4), Twice(Minus(y, x))), 26u);
        Assert.AreEqual(Combine(1, Twice(2), 3), 8u);
        Assert.AreEqual(x, 3u);
        Assert.AreEqual(y, 10u);
    }

    [Test]
    public void Overlaid_Frames_Keep_The_Callers_Locals()
    {
        uint before = 7;
        Assert.AreEqual(Outer(5), 6u + 10u + 5u);
        // Outer(1) = 2 + 2 + 1 = 5, Outer(5) = 21
        Assert.AreEqual(Outer(Outer(1)), 21u);
        Assert.AreEqual(before, 7u);
    }

    [Test]
    public void Sixteen_Bit_Parameters_And_Results()
    {
        ulong a = 1000;
        ulong r = Widen(a, 24);
        if (r != 1024)
            Assert.Fail();
        wideField = Widen(Widen(r, 255), 1);
        if (wideField != 1280)
            Assert.Fail();
        if (Widen(60000, 200) != 60200)
            Assert.Fail();
    }

    [Test]
    public void Reference_Locals_Are_Released()
    {
        uint sum = 0;
        // A handle comes back only through GC.Collect, and only once its
        // root count is 0: 400 calls, so a count leaked at the reassignment
        // or at the exit runs out of the 255 handles.
        for (uint i = 0; i < 200; i++)
        {
            sum = (sum + MakeAndRead(i) + MakeAndRead(i)) & 0x7f;
            System.GC.Collect();
        }
        Assert.AreEqual(MakeAndRead(41), 42u);
        // 2 * (1 + ... + 200) = 40200 = 8 (mod 128)
        Assert.AreEqual(sum, 8u);
    }

    [Test]
    public void Callee_Pulls_Frame_With_References()
    {
        var x = new FrameNode { value_ = 1 };
        var y = new FrameNode { value_ = 20 };
        var z = new FrameNode { value_ = 100 };
        uint total = 0;
        for (uint i = 0; i < 150; i++)
        {
            total = z.Pick(x, y, i & 1) + z.Pick(y, x, 0);
            System.GC.Collect();
        }
        // i = 149: Pick(x, y, 1) = 120, Pick(y, x, 0) = 120
        Assert.AreEqual(total, 240u);
        Assert.AreEqual(x.Pick(x, z, 0), 2u);
        // Combine2 picks y (20); y.Pick(y, z, 0) = 20 + 20
        Assert.AreEqual(y.Pick(Combine2(x, y), z, 0), 40u);
    }

    static FrameNode Combine2(FrameNode a, FrameNode b)
    {
        Touch();
        return a.value_ > b.value_ ? a : b;
    }

    [Test]
    public void Float_Results_Through_A_Callee_Pulls_Frame()
    {
        float a = Scale(Scale(2f, 3f), 0.5f);
        if (a != 3f)
            Assert.Fail();
        if (Scale(a, Scale(1f, 4f)) != 12f)
            Assert.Fail();
    }

    [Test]
    public void Instance_Methods_With_Reference_Parameters()
    {
        var a = new FrameNode { value_ = 5 };
        var b = new FrameNode { value_ = 30 };
        uint total = 0;
        for (uint i = 0; i < 150; i++)
        {
            total = a.SumWith(b);
            System.GC.Collect();
        }
        Assert.AreEqual(total, 35u);
        var c = a.Link(b).Link(a);
        Assert.AreEqual(c.next_.value_, 5u);
        Assert.AreEqual(b.SumWith(a.next_), 35u);
    }
}
