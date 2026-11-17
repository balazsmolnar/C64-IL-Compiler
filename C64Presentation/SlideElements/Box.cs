using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// A card: a Grey1 panel with a 2 px accent bar along its top edge and its
// text centred in white. With a Caption the text sits on the first body row
// and the caption two rows lower (16 px apart): give such a box Height 4 or
// more. Hollow: no fill (an empty slot: just the bar). ConnectToLeft: a 1 px
// black divider down its left edge, for a row of boxes that touch; the text
// is then centred in the cells after it, so the divider's cell stays black.
//
//     new Box { X = 2, Y = 6, Width = 8, Height = 3, Text = "C#", Color = Theme.Brand }
sealed class Box : SlideElement
{
    public uint X { get; init; }
    public uint Y { get; init; }
    public uint Width { get; init; }
    public uint Height { get; init; }
    public string? Text { get; init; }
    public string? Caption { get; init; }
    public Colors Color { get; init; }      // the accent bar; Auto = Theme.Accent
    public bool Hollow { get; init; }
    public bool ConnectToLeft { get; init; }
    public bool Wait { get; init; }
    public bool WaitAfter { get; init; }

    public override void Draw()
    {
        if (Wait)
            Deck.Step();
        var fill = Hollow ? Theme.Background : Theme.Card;
        Card(X, Y, Width, Height, Theme.Or(Color, Theme.Accent), fill);
        ulong x = Screen.Px(X), width = Screen.Px(Width);
        if (ConnectToLeft)
        {
            Deck.Paint(X, Y + 1, 1, Height - 1, Theme.Background, fill);
            Screen.Line(x, Screen.Px(Y + 1), x, Screen.Px(Y + Height) - 1);
            x += Theme.CellPixels;
            width -= Theme.CellPixels;
        }
        var ink = Hollow ? Theme.Body : Theme.Heading;
        bool captioned = Caption != null;
        uint row = captioned ? Y + 1 : Y + Height / 2;
        if (Text != null)
            Helper.Text.Center(x, width, row, Text, ink, fill);
        if (captioned)      // the second line recedes: Grey3
            Helper.Text.Center(x, width, row + 2, Caption!, Theme.Body, fill);
        if (WaitAfter)
            Deck.Step();
    }

    // The panel alone: fill, accent bar.
    public static void Card(uint column, uint row, uint width, uint height, Colors accent, Colors fill)
    {
        Deck.Paint(column, row + 1, width, height - 1, fill, fill);
        Deck.Paint(column, row, width, 1, accent, fill);
        ulong x = Screen.Px(column), y = Screen.Px(row);
        Screen.Fill(x, y, x + Screen.Px(width) - 1, y + 1);
    }
}
