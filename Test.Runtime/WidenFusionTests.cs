using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.RuntimeTest;

class Carrier
{
    public uint Value;
}

class Holder
{
    public Carrier Inner;
    public ulong Result;

    // Mirrors Hunchback's own "X = Rope.PlayerX" exactly: a single
    // expression chaining a field load off a NON-`this` object reference
    // (Inner.Value, not a field of `this` directly) straight into
    // something requiring a widen to 16 bits (ulong here, matching
    // Player.X's own ulong setter). ILMethodWiden8To16Optimizer used to
    // wrongly fuse that Ldfld with the following Conv_u8, inserting its
    // own "push 0" BETWEEN the Inner reference (already on the evaluation
    // stack from the preceding ldfld) and the Ldfld that still needed to
    // consume it -- so Ldfld ended up reading field Value off whichever
    // object happens to hold handle 0 instead of off Inner. Reported as
    // "the player disappears while hanging on the rope in Hunchback";
    // rewriting the assignment as "var x = Inner.Value; Result = x;"
    // (breaking the direct [Ldfld; Conv_u8] adjacency the rule matched on)
    // was the user's own working workaround.
    public void Copy()
    {
        Result = Inner.Value;
    }
}

[TestFixture]
public class WidenFusionTests
{
    [Test]
    public void Widen_Fusion_Does_Not_Corrupt_Field_Load_Off_NonThis_Reference()
    {
        // Allocated first so it's the most likely candidate for a low
        // object handle (e.g. 0) -- if the bug is present, Copy() below
        // reads from whichever object actually holds that handle instead
        // of from "inner", and 111 (not 77) is the value most likely to
        // leak through.
        var decoy = new Carrier { Value = 111 };
        var inner = new Carrier { Value = 77 };
        var holder = new Holder { Inner = inner };

        holder.Copy();

        Assert.AreEqual((uint)holder.Result, 77u);
        Assert.AreEqual(decoy.Value, 111u);
    }
}
