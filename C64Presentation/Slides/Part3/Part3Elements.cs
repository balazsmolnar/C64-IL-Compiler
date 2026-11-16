using System;
using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides.Part3;

// Elements only part 3 of the deck needs. Same conventions as
// SlideElements/: init properties, 0 / Colors.Black = the theme's value.

// One step of a numbered top-down flow (slide 15): its text from the left
// margin, a small down arrow in the row above it from the previous step
// (Arrow), and Grey2 leaders from the end of its text to the address
// strip on the right, one per row in Leaders.
//
//     new FlowStep { Row = 8, Text = "2 Method address ~ $09FE", Arrow = true, Leaders = [15] }
sealed class FlowStep : SlideElement
{
    public uint Row { get; init; }
    public required string Text { get; init; }
    public bool Arrow { get; init; }
    public uint[]? Leaders { get; init; }
    public bool Wait { get; init; }

    public const uint LeaderColumn = 25;    // leaders turn here towards the strip
    public const uint StripColumn = 28;     // the address strip's first cell

    public override void Draw()
    {
        if (Wait)
            Deck.Step();
        ulong x = Screen.Px(Theme.Left);
        if (Arrow)
            DownArrow(x + 2, Row - 1);
        ulong end = Helper.Text.Draw(x, Row, Text, Theme.Body);
        if (Leaders is { } leaders)
        {
            foreach (var target in leaders)
                Leader(end, target);
        }
    }

    static void DownArrow(ulong x, uint row)
    {
        Deck.Paint((uint)(x / Theme.CellPixels), row, 1, 1, Theme.Muted, Theme.Background);
        ulong top = Screen.Px(row) + 1, tip = top + 5;
        Screen.Line(x, top, x, tip);
        Screen.Line(x - 2, tip - 2, x, tip);
        Screen.Line(x + 2, tip - 2, x, tip);
    }

    // Along the step's row to the turn, then straight to the target row's
    // middle at the strip's edge.
    void Leader(ulong textEnd, uint target)
    {
        ulong start = Screen.CellAfter(textEnd, 2), turn = Screen.Px(LeaderColumn), strip = Screen.Px(StripColumn) - 2;
        uint top = Row < target ? Row : target, bottom = Row < target ? target : Row;
        ulong y = Screen.Px(Row) + 4;
        if (start < turn)
        {
            uint first = (uint)(start / Theme.CellPixels);
            Deck.Paint(first, Row, LeaderColumn - first, 1, Theme.Muted, Theme.Background);
            Screen.Line(start, y, turn, y);
        }
        Deck.Paint(LeaderColumn, top, StripColumn - LeaderColumn, bottom - top + 1, Theme.Muted, Theme.Background);
        Screen.Line(turn, y, strip, Screen.Px(target) + 4);
    }
}

// The last element of a slide that can swap in a hidden one: once the
// slide is up, D (KeyBoard.Pressed) fades to Appendix, drawn as a hidden
// slide; advance there comes back to the slide, redrawn whole by Return
// (which builds this element with Enabled = false). Advance on the slide
// itself goes on as usual (the Presenter reads the key, still held).
//
//     new HiddenSlideKey { Enabled = builds, Appendix = DebuggerMock.Create, Return = PostSharpBuilt }
sealed class HiddenSlideKey : SlideElement
{
    public bool Enabled { get; init; }
    public required Func<Slide> Appendix { get; init; }
    public required Func<Slide> Return { get; init; }

    public override void Draw()
    {
        if (!Enabled)
            return;
        Deck.Show();
        Deck.Reveal();
        while (!KeyBoard.AdvancePressed())
        {
            KeyBoard.Frame();
            if (!KeyBoard.Pressed(Keys.D))
                continue;
            Show(Appendix, hidden: true);
            KeyBoard.WaitForKeys();
            Show(Return, hidden: false);
        }
    }

    static void Show(Func<Slide> slide, bool hidden)
    {
        if (hidden)
            Deck.BeginHidden();
        else
        {
            Deck.FadeOut();
            Deck.Begin(Deck.Number);
        }
        slide().Present();
        Deck.Show();
        Deck.Reveal();
    }
}

// A thin elbow from a tag to a line: down the middle of Column from FromRow
// to ToRow, then left across the cell before it (the appendix's VICE tag
// to the line it stopped on).
sealed class Elbow : SlideElement
{
    public uint Column { get; init; }
    public uint FromRow { get; init; }
    public uint ToRow { get; init; }
    public Colors Color { get; init; }

    public override void Draw()
    {
        Deck.Paint(Column, FromRow, 1, ToRow - FromRow + 1, Color, Theme.Background);
        Deck.Paint(Column - 1, ToRow, 1, 1, Color, Theme.Background);
        ulong x = Screen.Px(Column) + 3, y = Screen.Px(ToRow) + 4;
        Screen.Line(x, Screen.Px(FromRow), x, y);
        Screen.Line(Screen.Px(Column - 1), y, x, y);
    }
}