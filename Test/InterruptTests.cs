using C64Lib;
using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.Test;

[TestFixture]
public class InterruptTests
{
    // `C64.Interrupt += Handler;` used to fail to assemble at all --
    // OpNewObj (Compiler/Operands/OperandBase.cs) guessed a
    // "EventHandler_x_ctor" label for delegate construction that nothing
    // ever defined. This test's only job is proving that's fixed: the
    // subscription itself compiles, assembles, and runs without the
    // emulator hitting an undefined instruction/crashing.
    //
    // It can't prove the handler actually FIRES -- SimpleEmulator has no
    // real interrupt-timer model (see Emulator.cs's Interrupt(), a
    // deliberate no-op). That's covered instead by
    // SimpleEmulator.Test's InterruptDispatchTests, which loads this same
    // compiled prg and drives OnInterrupt directly, and by a live VICE
    // smoke test (interrupts are genuinely asynchronous hardware events,
    // the one thing neither test harness can exercise for real).
    [Test]
    public void Subscribe_Compiles_And_Runs()
    {
        C64.Interrupt += Handler;
        Assert.IsTrue(true);
    }

    // Referenced by SimpleEmulator.Test's InterruptDispatchTests via its
    // compiled label (InterruptTests_Handler) to drive a synthesized
    // interrupt directly. SetBorderColor is the observable side effect
    // (a fixed, well-known hardware register, $D020) *and* the thing that
    // proves zero-page save/restore actually matters: C64_SetBorderColor
    // (asm/C64.asm) uses zp_tmp1_low/high exactly like every other C64Lib
    // API macro, so if OnInterrupt's save/restore didn't work, that byte
    // would come back holding this call's own return address instead of
    // whatever the interrupted mainline code had there.
    public static void Handler()
    {
        C64.Screen.SetBorderColor(Colors.Green);
    }
}
