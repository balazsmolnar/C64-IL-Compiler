using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// A key message: a card with a 3 px accent bar down its left edge, the
// lines in the 16 px heading face (white, about 30 characters each, 24 px
// apart: 8 px of leading) and an optional attribution line in the body face
// under them, white like all text on Grey1. Defaults: X Theme.Left, Width
// Theme.Width, Accent Theme.Accent; the height follows the lines: 3 rows a
// line + 1 (+2 with an attribution).
//
//     new Callout { Y = 16, Lines = ["Everything else is about", "fitting C# into that."] }
class Callout : SlideElement
{
    public uint X { get; init; }
    public uint Y { get; init; }
    public uint Width { get; init; }
    public required string[] Lines { get; init; }
    public string? Attribution { get; init; }
    public Colors Accent { get; init; }
    public bool Wait { get; init; }

    public override void Draw()
    {
        if (Wait)
            Deck.Step();
        Render(Theme.OrCells(X, Theme.Left), Y, Theme.OrCells(Width, Theme.Width), Lines, Attribution, Theme.Or(Accent, Theme.Accent));
    }

    static void Render(uint left, uint top, uint width, string[] lines, string? attribution, Colors accent)
    {
        uint height = (uint)lines.Length * LinePitch + (attribution != null ? 3u : 1u);
        Deck.Paint(left, top, width, height, Theme.Card, Theme.Card);
        Deck.Paint(left, top, 1, height, accent, Theme.Card);
        ulong x = Helper.Screen.Px(left);
        Helper.Screen.Fill(x, Helper.Screen.Px(top), x + 2, Helper.Screen.Px(top + height) - 1);
        uint row = top + 1;
        foreach (var line in lines)
        {
            Text.Heading(x + 12, row, line, Theme.Heading, Theme.Card);
            row += LinePitch;
        }
        if (attribution != null)
            Text.Draw(x + 12, row, attribution, Theme.Heading, Theme.Card);
    }

    const uint LinePitch = 3;     // rows: a 16 px line and 8 px of leading
}
