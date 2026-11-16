using C64Lib;

namespace Hunchback;

// The controls: a joystick in port 2, or A / D to walk and W to jump. Both
// work at all times (the original asks which before every game).
class Input
{
    public static bool Left =>
        C64.IsKeyPressed(Keys.A) || (C64.Joysticks.Joystick2.Pressed & JoystickButtons.Left) == JoystickButtons.Left;

    public static bool Right =>
        C64.IsKeyPressed(Keys.D) || (C64.Joysticks.Joystick2.Pressed & JoystickButtons.Right) == JoystickButtons.Right;

    public static bool Fire =>
        C64.IsKeyPressed(Keys.W) || (C64.Joysticks.Joystick2.Pressed & JoystickButtons.Fire) == JoystickButtons.Fire;
}
