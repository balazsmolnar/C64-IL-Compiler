using C64Lib;

namespace C64Presentation.Helper;

// All "next slide" input goes through here, so scene builds can fake it
// (C64Presentation.csproj, tools/presentation-shot.ps1):
//   SCENE_HOLD  never pressed: a slide stays up for screenshots
//   SCENE_AUTO  presses itself after a short while: the deck runs by itself
static class KeyBoard
{
#if SCENE_AUTO
    static uint _autoTicks;
#endif

    // True while "advance" is held: SPACE or joystick fire.
    public static bool AdvancePressed()
    {
#if SCENE_HOLD
        return false;
#elif SCENE_AUTO
        Delay.Wait(10);
        _autoTicks++;
        if (_autoTicks < 64)
            return false;
        _autoTicks = 0;
        return true;
#else
        return C64.IsKeyPressed(Keys.Space) || (C64.Joysticks.Joystick2.Pressed & JoystickButtons.Fire) == JoystickButtons.Fire;
#endif
    }

    // True while `key` is held: for a slide's own keys (D: the hidden
    // appendix). Never in a scene build, which presses nothing but advance.
    public static bool Pressed(Keys key)
    {
#if SCENE
        return false;
#else
        return C64.IsKeyPressed(key);
#endif
    }

    // One video frame. Animations and polling loops wait here, so that
    // deck-wide keys can be handled in one place.
    public static void Frame() => C64.Screen.WaitForVBlank();

    // Waits for SPACE/fire (returns Keys.Space) or B (back), then for its release.
    public static Keys WaitForKeys()
    {
#if SCENE_HOLD || SCENE_AUTO
        while (!AdvancePressed())
        {
        }
        return Keys.Space;
#else
        // to prevent a compiler bug
        int a = 0;
        Keys key;
        while (true)
        {

            if (AdvancePressed())
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
            // Was Wait(500): uint is 8-bit here, so it always waited 244
            // (500 & 255) -- now a C64006 error; the value it really had.
            Delay.Wait(244);

        }
        return key;
#endif
    }
}
