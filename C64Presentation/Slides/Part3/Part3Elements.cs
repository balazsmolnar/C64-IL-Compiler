using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides.Part3;

// Elements only part 3 of the deck needs. Same conventions as
// SlideElements/: init properties, 0 / Colors.Black = the theme's value.

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
