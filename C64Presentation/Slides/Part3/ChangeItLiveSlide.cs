using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.Slides.Part3;

// Slide 25: the live demo's checklist, up while the presenter works on the
// PC. Each press shows the next step and ticks the one before; a cursor
// blinks where the next step will appear, and LIVE blinks in the footer.
// The press after the last step ticks it; the one after that moves on.
//
// Fallback: if the demo fails, D (at any point on this slide) swaps in the
// hidden appendix, the debugger mock (DebuggerMock.cs); SPACE then goes on
// to slide 26 as usual.
sealed class ChangeItLiveSlide : Slide
{
    public static Slide Create() => new ChangeItLiveSlide();

    static readonly string[] Steps =
    [
        "1 Run: a cube on light blue",
        "2 Edit one line, build, run",
        "3 Breakpoint in Step(), F5",
        "4 velX_ = 0, F9, F5",
    ];

    const uint FirstRow = 6;
    const uint Pitch = 2;
    const uint BoxColumn = Theme.Left;
    const uint TextColumn = Theme.Left + 2;
    const uint BadgeColumn = 27;
    const uint BlinkFrames = 25;

    uint _frames;
    bool _lit;

    public override void Present()
    {
        Master.Content("Change it live");
        Text.Draw(Screen.Px(BadgeColumn) + 1, Theme.FooterRow, Theme.Dot, Theme.Bad);
        Text.Draw(Screen.Px(BadgeColumn) + 10, Theme.FooterRow, "LIVE", Theme.Heading);
        Deck.Reveal();

        for (uint i = 0; i < Steps.Length; i++)
        {
            if (!AwaitPress(Row(i), consume: true))
                return;
            if (i != 0)
                Tick(Row(i - 1));
            DrawStep(i);
        }
        if (!AwaitPress(Row((uint)Steps.Length), consume: true))
            return;
        Tick(Row((uint)Steps.Length - 1));
        AwaitPress(Row((uint)Steps.Length), consume: false);   // the press that moves on
    }

    static uint Row(uint step) => FirstRow + step * Pitch;

    // A step fades in with an empty check box before it.
    static void DrawStep(uint step)
    {
        uint row = Row(step);
        Screen.ClearRect(TextColumn, row, 1, 1);        // the cursor was here
        Deck.Build(false);
        Deck.Paint(BoxColumn, row, 1, 1, Theme.Muted, Theme.Background);
        ulong x = Screen.Px(BoxColumn);
        ulong y = Screen.Px(row);
        Screen.Frame(x, y, x + 6, y + 6);
        Text.Draw(Screen.Px(TextColumn), row, Steps[step], Theme.Body);
        Deck.Show();
    }

    // Done: the box fills with the accent and shows a tick.
    static void Tick(uint row) => Text.Draw(Screen.Px(BoxColumn) + 1, row, Theme.Tick, Theme.Background, Theme.Accent);

    // Blinks the cursor at `row` and the LIVE dot until "advance" (true) or
    // D, which shows the fallback slide (false). consume: also wait for the
    // key's release, for a press that stays on this slide. In a SCENE_HOLD
    // build only the last wait waits, so the slide shows every step.
    bool AwaitPress(uint row, bool consume)
    {
        Screen.Fill(Screen.Px(TextColumn), Screen.Px(row), Screen.Px(TextColumn) + 5, Screen.Px(row) + 6);
        Deck.Paint(TextColumn, row, 1, 1, Theme.Accent, Theme.Background);
#if SCENE_HOLD
        if (consume)
            return true;
#endif
        while (!KeyBoard.AdvancePressed())
        {
            if (C64.IsKeyPressed(Keys.D))
            {
                ShowFallback();
                return false;
            }
            C64.Screen.WaitForVBlank();
            if (++_frames < BlinkFrames)
                continue;
            _frames = 0;
            _lit = !_lit;
            var on = _lit ? Theme.Accent : Theme.Background;
            Deck.Paint(TextColumn, row, 1, 1, on, Theme.Background);
            Deck.Paint(BadgeColumn, Theme.FooterRow, 1, 1, _lit ? Theme.Bad : Theme.Background, Theme.Background);
        }
        if (consume)
            KeyBoard.WaitForKeys();
        return true;
    }

    static void ShowFallback()
    {
        Deck.FadeOut();
        Deck.Begin(Deck.Number);
        DebuggerMock.Create().Present();
    }
}
