using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// A vertical arrow down the middle of column X, from row Y over Height
// cells, Text to its right (muted). After a build step it is drawn live and
// grows downwards. Color default Theme.Accent.
class DownArrow : SlideElement
{
    public uint X, Y, Height;
    public string Text;
    public Colors Color;
    public bool Wait;

    public override void Draw()
    {
        if (Wait)
            Deck.Step();
        Deck.Show();
        ulong x = Helper.Screen.X(X) + 3;
        ulong y0 = Helper.Screen.X(Y) + 1;
        ulong y1 = Helper.Screen.X(Y + Height) - 2;
        Deck.Paint(X, Y, 1, Height, Theme.Or(Color, Theme.Accent), Theme.Background);
        for (ulong y = y0; y < y1; y += 8)
        {
            ulong to = y + 7;
            if (to > y1)
                to = y1;
            Helper.Screen.Line(x, y, x, to);
            if (Deck.Live)
                C64.Screen.WaitForVBlank();
        }
        Helper.Screen.Line(x - 3, y1 - 3, x, y1);
        Helper.Screen.Line(x + 3, y1 - 3, x, y1);
        if (Text != null)
            Helper.Text.Draw(x + 9, Y + Height / 2, Text, Theme.Muted);
    }
}
