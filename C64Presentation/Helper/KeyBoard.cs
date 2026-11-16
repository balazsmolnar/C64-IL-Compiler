using C64Lib;

namespace C64Presentation.Helper;

// Where the presenter wants to go: SPACE / fire advance a build step and
// then the slide; N and B jump to the next / previous slide at once, from
// any point of a slide (Presenter).
enum Navigation
{
    None,
    Next,
    Back,
}

// All presenter input goes through here, so scene builds can fake it
// (C64Presentation.csproj, tools/presentation-shot.ps1):
//   SCENE_HOLD     never pressed: a slide stays up for screenshots
//   SCENE_AUTO     presses itself after a short while: the deck runs by itself
//   SCENE_NAVTEST  AUTO, plus an N press during the first slide's builds and
//                  a B press on the slide that brings
//
// N and B are edge-triggered: a press sets Pending once, and holding the
// key does nothing more until it is released. While a jump is pending the
// engine draws nothing more (Deck) and every wait returns at once, so a
// slide's own loop must wait through AdvancePressed or Deck.Wait.
static class KeyBoard
{
    // The jump the presenter asked for; the Presenter takes it (Take).
    public static Navigation Pending { get; private set; }

    public static bool Jumping => Pending != Navigation.None;

    static bool _navHeld;           // N or B was down at the last poll
#if SCENE_AUTO
    static uint _autoTicks;
#endif
#if SCENE_NAVTEST
    static uint _fakeTicks;
    static uint _fakeStage;
#endif

    // Reads N and B: a new press sets Pending (unless a jump is pending).
    public static void Poll()
    {
#if SCENE_NAVTEST
        _fakeTicks++;
        if (_fakeStage == 0 && _fakeTicks == 100)          // during the first slide's second build step
            Request(Navigation.Next);
        else if (_fakeStage == 1 && _fakeTicks == 40)      // soon after the next slide is up
            Request(Navigation.Back);
#elif !(SCENE_HOLD || SCENE_AUTO)
        bool next = C64.IsKeyPressed(Keys.N), back = C64.IsKeyPressed(Keys.B);
        if (!_navHeld && !Jumping && (next || back))
            Pending = next ? Navigation.Next : Navigation.Back;
        _navHeld = next || back;
#endif
    }

#if SCENE_NAVTEST
    static void Request(Navigation navigation)
    {
        Pending = navigation;
        _fakeStage++;
        _fakeTicks = 0;
    }
#endif

    // The pending jump, cleared; Next when none is pending (the slide ended
    // with SPACE / fire).
    public static Navigation Take()
    {
        var navigation = Jumping ? Pending : Navigation.Next;
        Pending = Navigation.None;
        return navigation;
    }

    // True while "advance" is held (SPACE or joystick fire), or once a jump
    // is pending: a slide's own animation loop runs while this is false.
    public static bool AdvancePressed()
    {
        Poll();
        if (Jumping)
            return true;
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

    // A slide's own key (D: the hidden appendix), edge-triggered like N and
    // B: true once per press, until the key is released. Never in a scene
    // build. Not for N or B, which the engine owns.
    public static bool Pressed(Keys key)
    {
#if SCENE
        return false;
#else
        bool down = C64.IsKeyPressed(key);
        bool pressed = down && !_slideKeyHeld;
        _slideKeyHeld = down;
        return pressed;
#endif
    }

    static bool _slideKeyHeld;

    // Waits for SPACE / fire and its release, or until a jump is pending.
    public static void WaitForAdvance()
    {
        while (!AdvancePressed())
        {
            C64.Screen.WaitForVBlank();
        }
#if !(SCENE_HOLD || SCENE_AUTO)
        while (!Jumping && C64.IsKeyPressed(Keys.Space))
            Poll();
#endif
    }
}
