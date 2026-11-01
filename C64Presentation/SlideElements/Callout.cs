using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// A callout / quote: a card with a 3 px accent bar down its left edge, the
// lines in white (one per row, from the second row) and an optional
// attribution in the accent colour under them. Defaults: X Theme.Left,
// Width Theme.Width, Height = lines + 2 (+ 2 with an attribution),
// Accent Theme.Accent.
class Callout : SlideElement
{
    public uint X, Y, Width, Height;
    public string[] Lines;
    public string Attribution;
    public Colors Accent;
    public bool Wait;

    public override void Draw()
    {
        if (Wait)
            Deck.Step();
        string[] lines = Lines;
        uint height = Height;
        if (height == 0)
            height = (uint)lines.Length + (Attribution != null ? 4u : 2u);
        Render(X == 0 ? Theme.Left : X, Y, Width == 0 ? Theme.Width : Width, height, lines, Attribution, Theme.Or(Accent, Theme.Accent));
    }

    static void Render(uint left, uint top, uint width, uint height, string[] lines, string attribution, Colors accent)
    {
        Deck.Paint(left, top, width, height, Theme.Card, Theme.Card);
        Deck.Paint(left, top, 1, height, accent, Theme.Card);
        ulong x = Helper.Screen.X(left);
        Helper.Screen.Fill(x, Helper.Screen.X(top), x + 2, Helper.Screen.X(top + height) - 1);
        uint row = top + 1;
        foreach (var line in lines)
        {
            Text.Draw(x + 12, row, line, Theme.Heading, Theme.Card);
            row++;
        }
        if (attribution != null)
            Text.Draw(x + 12, row + 1, attribution, accent, Theme.Card);
    }
}
