using System;
using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides.Part3;

// Elements only part 3 of the deck needs. Same conventions as
// SlideElements/: init properties, 0 / Colors.Black = the theme's value.

// One step of a numbered top-down flow (slide 15): its number in cyan on a
// thin Grey2 rail down the left margin (joined to the step above unless it
// starts a group), its text, and the addresses it touches as chips right
// aligned on its own row, each joined to the text by a dotted leader.
//
//     new FlowStep { Row = 9, Number = "2", Text = "Method address ~ $09FE", Addresses = ["$09FE"], Color = Theme.Brand }
sealed class FlowStep : SlideElement
{
    public uint Row { get; init; }
    public required string Number { get; init; }
    public required string Text { get; init; }
    public string[]? Addresses { get; init; }
    public Colors Color { get; init; }      // the chips' bar: the step's group
    public bool First { get; init; }        // starts a group: no rail above
    public bool Wait { get; init; }

    const uint RailColumn = Theme.Left;
    const uint TextColumn = Theme.Left + 1;
    const uint ChipCells = 5;
    const uint ChipGap = 1;                 // cells between two chips
    const uint Pitch = 2;
    const ulong TextInset = 3;              // pixels of air after the number

    public override void Draw()
    {
        if (Wait)
            Deck.Step();
        ulong rail = Screen.Px(RailColumn) + 2;
        if (!First)
        {
            Deck.Paint(RailColumn, Row - Pitch + 1, 1, Pitch - 1, Theme.Muted, Theme.Background);
            Screen.Line(rail, Screen.Px(Row - Pitch + 1), rail, Screen.Px(Row) - 1);
        }
        Helper.Text.Draw(Screen.Px(RailColumn), Row, Number, Theme.Accent);
        ulong end = Helper.Text.Draw(Screen.Px(TextColumn) + TextInset, Row, Text, Theme.Body);
        if (Addresses is not { } addresses)
            return;
        uint chip = Theme.Right - (uint)addresses.Length * (ChipCells + ChipGap) + ChipGap;
        Leader(end, Screen.Px(chip) - 4);
        foreach (var address in addresses)
        {
            Chip(chip, address);
            chip += ChipCells + ChipGap;
        }
    }

    // A Grey2 dotted line along the row's middle, in cells of its own.
    void Leader(ulong textEnd, ulong to)
    {
        ulong from = Screen.CellAfter(textEnd, 2);
        if (from >= to)
            return;
        uint first = (uint)(from / Theme.CellPixels);
        Deck.Paint(first, Row, (uint)(to / Theme.CellPixels) - first + 1, 1, Theme.Muted, Theme.Background);
        ulong y = Screen.Px(Row) + 4;
        for (ulong x = from; x <= to; x += 2)
            Screen.Line(x, y, x, y);
    }

    // A Grey1 chip with the address in white and a bar in the group colour.
    void Chip(uint column, string address)
    {
        Deck.Paint(column, Row, 1, 1, Color, Theme.Card);
        Deck.Paint(column + 1, Row, ChipCells - 1, 1, Theme.Heading, Theme.Card);
        ulong x = Screen.Px(column);
        Screen.Fill(x, Screen.Px(Row), x + 1, Screen.Px(Row + 1) - 1);
        Helper.Text.Center(Screen.Px(column + 1), Screen.Px(ChipCells - 1), Row, address, Theme.Heading, Theme.Card);
    }
}

// A 1 px line under some characters of a code line (cells keep their
// colour): marks values in code without recolouring them.
sealed class CodeUnderline : SlideElement
{
    public uint Column { get; init; }       // the code's first column
    public uint Row { get; init; }
    public uint From { get; init; }         // the first character, 0-based
    public uint Length { get; init; }
    public bool Wait { get; init; }

    const ulong Pitch = 6;                  // the code face

    public override void Draw()
    {
        if (Wait)
            Deck.Step();
        ulong x = Screen.Px(Column) + From * Pitch, y = Screen.Px(Row) + 7;
        Screen.Line(x, y, x + Length * Pitch - 2, y);
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

// The last element of a slide that can swap in a hidden one: once the
// slide is up, D (KeyBoard.Pressed) fades to Appendix, drawn as a hidden
// slide; advance or B there comes back to the slide, redrawn whole by
// Return (which builds this element with Enabled = false). Advance on the
// slide itself goes on as usual (the Presenter reads the key, still held).
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
            Deck.Wait(1);
            if (!KeyBoard.Pressed(Keys.D))
                continue;
            Deck.BeginHidden();
            Present(Appendix);
            KeyBoard.WaitForAdvance();
            if (KeyBoard.Pending == Navigation.Back)
                KeyBoard.Take();                // B on the appendix: back to this slide
            if (KeyBoard.Jumping)
                return;
            Deck.FadeOut();
            Deck.Begin(Deck.Number);
            Present(Return);
        }
    }

    static void Present(Func<Slide> slide)
    {
        slide().Present();
        Deck.Show();
        Deck.Reveal();
    }
}

#if SCENE
// A scene build lists the appendix as a slide of its own: entered the way
// D enters it, as a hidden slide, so screenshots show what the audience sees.
sealed class HiddenScene : Slide
{
    public static Slide Create() => new HiddenScene();

    public override void Present()
    {
        Deck.BeginHidden();
        DebuggerMock.Create().Present();
    }
}
#endif
