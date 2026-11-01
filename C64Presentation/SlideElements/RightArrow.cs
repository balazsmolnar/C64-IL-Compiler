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
        Arrow.Draw(Wait, X, Y, Width, Color, false);
        ulong left = Helper.Screen.X(X);
        ulong width = Helper.Screen.X(Width);
        if (Text1 != null)
            Text.Center(left, width, Y - 1, Text1, Theme.Muted);
        if (Text2 != null)
            Text.Center(left, width, Y + 1, Text2, Theme.Muted);
    }
}

// A vertical arrow down the middle of column X, from row Y over Height
// cells, Text to its right (muted). Grows downwards when drawn live.
class DownArrow : SlideElement
{
    public uint X, Y, Height;
    public string Text;
    public Colors Color;
    public bool Wait;

    public override void Draw()
    {
        Arrow.Draw(Wait, X, Y, Height, Color, true);
        if (Text != null)
            Helper.Text.Draw(Helper.Screen.X(X) + 12, Y + Height / 2, Text, Theme.Muted);
    }
}

// The shared arrow: a 1 px line with a head, along row `row` (or down
// column `column`) for `length` cells, drawn a cell per frame when live.
static class Arrow
{
    public static void Draw(bool wait, uint column, uint row, uint length, Colors color, bool down)
    {
        if (wait)
            Deck.Step();
        Deck.Show();                // after a build step it is drawn live: it grows
        if (down)
            Deck.Paint(column, row, 1, length, Theme.Or(color, Theme.Accent), Theme.Background);
        else
            Deck.Paint(column, row, length, 1, Theme.Or(color, Theme.Accent), Theme.Background);
        ulong a0 = Helper.Screen.X(down ? row : column) + (down ? 1ul : 2ul);
        ulong a1 = Helper.Screen.X((down ? row : column) + length) - (down ? 2ul : 3ul);
        ulong c = Helper.Screen.X(down ? column : row) + (down ? 3ul : 4ul);
        for (ulong a = a0; a < a1; a += 8)
        {
            ulong to = a + 7;
            if (to > a1)
                to = a1;
            if (down)
                Helper.Screen.Line(c, a, c, to);
            else
                Helper.Screen.Line(a, c, to, c);
            if (Deck.Live)
                C64.Screen.WaitForVBlank();
        }
        if (down)
        {
            Helper.Screen.Line(c - 3, a1 - 3, c, a1);
            Helper.Screen.Line(c + 3, a1 - 3, c, a1);
        }
        else
        {
            Helper.Screen.Line(a1 - 3, c - 3, a1, c);
            Helper.Screen.Line(a1 - 3, c + 3, a1, c);
        }
    }
}
