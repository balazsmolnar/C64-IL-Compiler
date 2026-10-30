using C64Lib;
using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.RuntimeTest;

[TestFixture]
public class JoystickAndLoopTests
{
    // $DC00 is joystick 2 and also the keyboard's column select. A keyboard
    // scan that leaves a column with bit 4 clear selected (e.g. $EF) must not
    // read as FIRE: Joystick_get_Pressed deselects every column first. (The
    // emulator's $DC00 is plain memory, so before the fix this read $EF back
    // and reported FIRE.)
    [Test]
    public void Joystick2_Ignores_The_Keyboard_Column_Left_Selected()
    {
        C64.FillMemory(0xDBFFUL, 0xEF, 1);     // FillMemory writes bytes 1..n: this is $DC00
        uint pressed = (uint)C64.Joysticks.Joystick2.Pressed;
        Assert.AreEqual(pressed, 0u);
    }

    // A method whose first statement is a loop: its first IL instruction is
    // a branch target whose only predecessor is the loop's own back edge.
    // ILMethodBuildEvaluationStackPass used to fail on it (C64900).
    static uint CountUp(uint n)
    {
        for (;;)
        {
            n = n + 1;
            if (n > 5)
                return n;
        }
    }

    [Test]
    public void A_Method_Can_Start_With_A_Loop()
    {
        Assert.AreEqual(CountUp(2), 6u);
    }
}
