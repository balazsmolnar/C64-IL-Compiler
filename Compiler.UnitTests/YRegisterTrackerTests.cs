using Compiler;
using NUnit.Framework;

namespace Compiler.UnitTests;

// YRegisterTracker (Compiler/YRegisterTracker.cs): a localsStack access
// right after another one, with only Y-neutral code between, skips its
// `ldy stackPointer`; anything that may change Y forgets it.
[TestFixture]
public class YRegisterTrackerTests
{
    [Test]
    public void Second_Access_After_Neutral_Code_Skips_The_Reload()
    {
        var t = new YRegisterTracker();
        Assert.That(t.Process("    #locals_push_value8 2", false), Is.EqualTo("    #locals_push_value8 2"));
        Assert.That(t.Process("    #stack_push_int8 5", false), Is.EqualTo("    #stack_push_int8 5"));
        Assert.That(t.Process("    #add8", false), Is.EqualTo("    #add8"));
        Assert.That(t.Process("    #locals_pull_value8 3, 0", false), Is.EqualTo("    #locals_pull_value8_ky 3, 0"));
    }

    [Test]
    public void Prologue_Leaves_Y_Holding_The_Stack_Pointer()
    {
        var t = new YRegisterTracker();
        t.Process("    #init_locals_pull_parameters 2, [0], []", false);
        Assert.That(t.Process("    #init_var 2, 30", false), Is.EqualTo("    #init_var_ky 2, 30"));
    }

    [Test]
    public void Label_Call_And_Unknown_Macros_Forget_Y()
    {
        var t = new YRegisterTracker();
        t.Process("    #locals_push_value8 2", false);
        Assert.That(t.Process("    #locals_push_value8 2", true), Is.EqualTo("    #locals_push_value8 2"));
        t.Process("    jsr Foo", false);
        Assert.That(t.Process("    #locals_push_value8 2", false), Is.EqualTo("    #locals_push_value8 2"));
        t.Process("    #pushfld8 3", false);
        Assert.That(t.Process("    #locals_push_value8 2", false), Is.EqualTo("    #locals_push_value8 2"));
        t.Process("    sta stackPointer", false);
        Assert.That(t.Process("    #locals_push_value8 2", false), Is.EqualTo("    #locals_push_value8 2"));
    }

    [Test]
    public void Raw_Reload_In_A_Fused_Sequence_Is_Dropped()
    {
        var t = new YRegisterTracker();
        t.Process("    #locals_push_value8 2", false);
        var fused = "#copy_int8_const 10, zp_param1_low\nldy stackPointer\nlda localsStack-1,y\n#sub8_tail";
        Assert.That(t.Process(fused, false), Is.EqualTo("#copy_int8_const 10, zp_param1_low\nlda localsStack-1,y\n#sub8_tail"));
    }
}
