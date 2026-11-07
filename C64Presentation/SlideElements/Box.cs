using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// A card: a Grey1 panel with a 2 px accent bar along its top edge and its
// text centred in white. With a Caption the text sits on the first body row
// and the caption two rows lower (16 px apart): give such a box Height 4 or
// more. Hollow: no fill (an empty slot: just the bar). ConnectToLeft: a 1 px
// divider down its left edge, for a row of boxes that touch.
//
//     new Box { X = 2, Y = 6, Width = 8, Height = 3, Text = "C#", Color = Theme.Brand }
class Box : SlideElement
{
    public uint X { get; init; }
    public uint Y { get; init; }
    public uint Width { get; init; }
    public uint Height { get; init; }
    public string Text { get; init; }
    public string Caption { get; init; }
    public Colors Color { get; init; }      // the accent bar; default Theme.Accent
    public bool Hollow { get; init; }
    public bool ConnectToLeft { get; init; }
    public bool Wait { get; init; }
    public bool WaitAfter { get; init; }

    public override void Draw()
    {
        if (Wait)
            Deck.Step();
        Render(X, Y, Width, Height, Text, Caption, Theme.Or(Color, Theme.Accent),
            Hollow ? Theme.Background : Theme.Card, ConnectToLeft);
        if (WaitAfter)
            Deck.Step();
    }

    public static void Render(uint x, uint y, uint width, uint height, string text, string caption, Colors accent, Colors fill, bool divider)
    {
        DrawCard(x, y, width, height, accent, fill);
        ulong px = Helper.Screen.X(x);
        if (divider)
        {
            Deck.Paint(x, y + 1, 1, height - 1, Theme.Background, fill);
            Helper.Screen.Line(px, Helper.Screen.X(y + 1), px, Helper.Screen.X(y + height) - 1);
        }
        ulong w = Helper.Screen.X(width);
        var ink = fill == Theme.Card ? Theme.Heading : Theme.Body;
        bool captioned = caption != null && caption.Length != 0;
        uint row = captioned ? y + 1 : y + height / 2;
        if (text != null)
            Helper.Text.Center(px, w, row, text, ink, fill);
        if (captioned)
            Helper.Text.Center(px, w, row + 2, caption, ink, fill);
    }

    // The panel alone: fill, accent bar.
    public static void DrawCard(uint x, uint y, uint width, uint height, Colors accent, Colors fill)
    {
        Deck.Paint(x, y + 1, width, height - 1, fill, fill);
        Deck.Paint(x, y, width, 1, accent, fill);
        ulong px = Helper.Screen.X(x);
        ulong py = Helper.Screen.X(y);
        Helper.Screen.Fill(px, py, px + Helper.Screen.X(width) - 1, py + 1);
    }
}
