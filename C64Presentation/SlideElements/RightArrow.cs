using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// A horizontal arrow through the middle of row Y, from column X over Width
// cells; Text1 above it, Text2 below (muted). After a build step it is
// drawn live and grows left to right. Color default Theme.Accent.
class RightArrow : SlideElement
{
    public uint X, Y, Width;
    public string Text1;
    public string Text2;
    public Colors Color;
    public bool Wait;

    public override void Draw()
    {
        if (Wait)
            Deck.Step();
        Deck.Show();
        ulong left = Helper.Screen.X(X);
        ulong width = Helper.Screen.X(Width);
        ulong x0 = left + 2;
        ulong x1 = left + width - 3;
        ulong y = Helper.Screen.X(Y) + 4;
        Deck.Paint(X, Y, Width, 1, Theme.Or(Color, Theme.Accent), Theme.Background);
        for (ulong x = x0; x < x1; x += 8)
        {
            ulong to = x + 7;
            if (to > x1)
                to = x1;
            Helper.Screen.Line(x, y, to, y);
            if (Deck.Live)
                C64.Screen.WaitForVBlank();
        }
        Helper.Screen.Line(x1 - 3, y - 3, x1, y);
        Helper.Screen.Line(x1 - 3, y + 3, x1, y);
        if (Text1 != null)
            Text.Center(left, width, Y - 1, Text1, Theme.Muted);
        if (Text2 != null)
            Text.Center(left, width, Y + 1, Text2, Theme.Muted);
    }
}
