using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// A card: a filled panel (Theme.Card) with a 2 px accent bar along its top
// edge and its text centred (the middle row; Caption, if any, in the row
// below it). Diagram nodes, memory blocks, labels. Hollow: no fill (an
// empty slot: just the bar). ConnectToLeft: a 1 px divider down its left
// edge, for a row of boxes that touch.
//
//     new Box { X = 2, Y = 6, Width = 8, Height = 3, Text = "C#", Color = Theme.Accent }
class Box : SlideElement
{
    public uint X, Y, Width, Height;
    public string Text;
    public string Caption;
    public Colors TextColor;        // default Theme.Heading
    public Colors BorderColor;      // the accent bar; default Theme.Accent
    public bool Hollow;

    public Colors Color
    {
        get => BorderColor;
        set => BorderColor = value;
    }

    public bool WaitAfter;
    public bool Wait;
    public bool ConnectToLeft;

    public override void Draw()
    {
        if (Wait)
            Deck.Step();
        Render(X, Y, Width, Height, Text, Caption, Theme.Or(TextColor, Theme.Heading), Theme.Or(BorderColor, Theme.Accent),
            Hollow ? Theme.Background : Theme.Card, ConnectToLeft);
        if (WaitAfter)
            Deck.Step();
    }

    public static void Render(uint x, uint y, uint width, uint height, string text, string caption, Colors textColor, Colors accent, Colors fill, bool divider)
    {
        DrawCard(x, y, width, height, accent, fill);
        ulong px = Helper.Screen.X(x);
        if (divider)
        {
            Deck.Paint(x, y + 1, 1, height - 1, Theme.Background, fill);
            Helper.Screen.Line(px, Helper.Screen.X(y + 1), px, Helper.Screen.X(y + height) - 1);
        }
        uint row = y + height / 2;
        if (caption != null && height > 3)
            row--;
        ulong w = Helper.Screen.X(width);
        if (text != null)
            Helper.Text.Center(px, w, row, text, textColor, fill);
        if (caption != null)
            Helper.Text.Center(px, w, row + 1, caption, Theme.Body, fill);
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
