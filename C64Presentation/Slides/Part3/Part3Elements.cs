using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides.Part3;

// Elements only part 3 of the deck needs. Same conventions as
// SlideElements/: init properties, 0 / Colors.Black = the theme's value.

// One vertical bar standing on a baseline: Pixels tall, its value above it,
// its label under the baseline, centred on it. Two of these side by side
// make a before/after pair.
//
//     new Column { X = 34, Bottom = 12, Pixels = 16, Text = "12 B", Label = "after", Color = Theme.Accent }
sealed class Column : SlideElement
{
    public uint X { get; init; }
    public uint Bottom { get; init; }       // the baseline's cell row
    public ulong Pixels { get; init; }
    public required string Text { get; init; }
    public required string Label { get; init; }
    public Colors Color { get; init; }

    const uint Width = 3;           // cells
    const ulong LabelGap = 2;       // pixels between the baseline and the label row

    public override void Draw()
    {
        ulong x0 = Screen.Px(X), span = Screen.Px(Width);
        ulong baseline = Screen.Px(Bottom + 1) - 1 - LabelGap, top = baseline + 1 - Pixels;
        uint topRow = (uint)(top / Theme.CellPixels);
        Deck.Paint(X, topRow, Width, Bottom - topRow + 1, Color, Theme.Background);
        Screen.Fill(x0, top, x0 + span - 1, baseline);
        Helper.Text.Center(x0, span, topRow - 1, Text, Theme.Heading);
        Helper.Text.Center(x0, span, Bottom + 1, Label, Theme.Body);
    }
}
// One step of a numbered top-down flow: its text on Row and a rail in the
// group's colour down the left margin, joined to the step above (Pitch
// rows up). Label starts a group: its name right aligned on the step's
// row in the group's colour, and the rail starts here.
//
//     new FlowStep { Row = 5, Text = "1 Load prg/unittest.prg", Label = "RunInEmulatorAspect.OnInvoke", Color = Theme.Brand }
//     new FlowStep { Row = 7, Text = "2 Method address ~ $09FE", Color = Theme.Brand }
sealed class FlowStep : SlideElement
{
    public uint Row { get; init; }
    public required string Text { get; init; }
    public string? Label { get; init; }
    public Colors Color { get; init; }      // Auto = Theme.Accent
    public bool Wait { get; init; }

    const uint TextColumn = Theme.Left + 1;
    const uint Pitch = 2;
    const ulong RailWidth = 2;

    public override void Draw()
    {
        if (Wait)
            Deck.Step();
        var color = Theme.Or(Color, Theme.Accent);
        bool first = Label != null;
        uint top = first ? Row : Row - Pitch + 1;
        Deck.Paint(Theme.Left, top, 1, Row - top + 1, color, Theme.Background);
        ulong rail = Screen.Px(Theme.Left) + 1;
        Screen.Fill(rail, Screen.Px(top), rail + RailWidth - 1, Screen.Px(Row + 1) - 1);
        Helper.Text.Draw(Screen.Px(TextColumn), Row, Text, Theme.Body);
        if (first)
            Helper.Text.Right(Screen.Px(Theme.Right) - 1, Row, Label!, color);
    }
}

// Key statements in the 16 px face, one every Pitch rows (3), each with a
// short bar on its left: the accent, or the item's own colour from Tints.
// Arrows: a down arrow above each bar, from the stage before. Wait:
// one per press.
//
//     new Statements { Top = 6, Items = ["Structure beats peepholes", "Measure on real programs"], Wait = true }
sealed class Statements : SlideElement
{
    public required string[] Items { get; init; }
    public Colors[]? Tints { get; init; }
    public bool Arrows { get; init; }
    public uint Top { get; init; }
    public uint Pitch { get; init; }        // Auto = 3 rows
    public bool Wait { get; init; }

    const ulong BarWidth = 3;
    const ulong TextOffset = 12;

    // A short arrow down through cell row `row`, under the bars.
    static void DownArrow(uint row)
    {
        Deck.Paint(Theme.Left, row, 1, 1, Theme.Muted, Theme.Background);
        ulong x = Screen.Px(Theme.Left) + 3, top = Screen.Px(row) + 1, tip = top + 5;
        Screen.Line(x, top, x, tip);
        Screen.Line(x - 3, tip - 3, x, tip);
        Screen.Line(x + 3, tip - 3, x, tip);
    }

    public override void Draw()
    {
        uint row = Theme.OrCells(Top, Theme.BodyTop);
        uint pitch = Theme.OrCells(Pitch, 3);
        ulong x = Screen.Px(Theme.Left);
        for (uint i = 0; i < Items.Length; i++)
        {
            if (Wait)
                Deck.Step();
            if (Arrows)
                DownArrow(row - 1);
            Deck.Paint(Theme.Left, row, 1, 2, Tints != null ? Tints[i] : Theme.Accent, Theme.Background);
            Screen.Fill(x, Screen.Px(row) + 1, x + BarWidth - 1, Screen.Px(row + 2) - 2);
            Helper.Text.Heading(x + TextOffset, row, Items[i]);
            row += pitch;
        }
    }
}
// Recolours a rectangle of cells (a highlighted line, a coloured chip).
sealed class Highlight : SlideElement
{
    public uint X { get; init; }
    public uint Y { get; init; }
    public uint Width { get; init; }
    public uint Height { get; init; }       // Auto = 1 row
    public Colors Ink { get; init; }
    public Colors Paper { get; init; }
    public bool Wait { get; init; }

    public override void Draw()
    {
        if (Wait)
            Deck.Step();
        Deck.Paint(X, Y, Width, Theme.OrCells(Height, 1), Ink, Paper);
    }
}
