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
        Colors fill = Hollow ? Theme.Background : Theme.Card;
        DrawCard(X, Y, Width, Height, Theme.Or(BorderColor, Theme.Accent), fill);
        ulong x = Helper.Screen.X(X);
        if (ConnectToLeft)
        {
            Deck.Paint(X, Y + 1, 1, Height - 1, Theme.Background, fill);
            Helper.Screen.Line(x, Helper.Screen.X(Y + 1), x, Helper.Screen.X(Y + Height) - 1);
        }
        uint row = Y + Height / 2;
        if (Caption != null && Height > 3)
            row--;
        ulong w = Helper.Screen.X(Width);
        if (Text != null)
            Helper.Text.Center(x, w, row, Text, Theme.Or(TextColor, Theme.Heading), fill);
        if (Caption != null)
            Helper.Text.Center(x, w, row + 1, Caption, Theme.Body, fill);
        if (WaitAfter)
            Deck.Step();
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
