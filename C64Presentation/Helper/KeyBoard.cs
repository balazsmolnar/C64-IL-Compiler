using C64Lib;

namespace C64Presentation.Helper;

static class KeyBoard
{
    public static Keys WaitForKeys()
    {
        // to prevent a compiler bug
        int a = 0;
        Keys key;
        while (true)
        {

            if (C64.IsKeyPressed(Keys.Space) || (C64.Joysticks.Joystick2.Pressed & JoystickButtons.Fire) == JoystickButtons.Fire)
            {
                key = Keys.Space;
                break;
            }

            if (C64.IsKeyPressed(Keys.B))
            {
                key = Keys.B;
                break;
            }
        }

        // wait for key up
        while (true)
        {
            if (!C64.IsKeyPressed(key))
                break;
            Delay.Wait(500);

        }
        return key;
    }
}