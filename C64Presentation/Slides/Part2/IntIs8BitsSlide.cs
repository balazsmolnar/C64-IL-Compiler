using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 10: the C# types and their sizes, then a loop bound that does not
// fit: what it silently became, and the error it is now.
static class IntIs8BitsSlide
{
    const uint CodeRow = 12, ErrorRow = 17;
    const uint LiteralColumn = 19;    // "700" starts on this cell edge: (3 + 16 characters) * 6 px = 152

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "int is 8 bits" },
            Sizes(Theme.LeftColumn, [["int, uint", "8 bits"], ["long, ulong", "16 bits"]]),
            Sizes(Theme.RightColumn, [["float", "5 bytes"], ["double", $"{Theme.Red}error"]]),
            new CodeBlock
            {
                X = Theme.Left, Y = CodeRow, Width = 26, Wait = true,
                Lines = [$"{Theme.Kw}for {Theme.Pl}({Theme.Kw}uint{Theme.Pl} i = 0; i < 700; i++)"],
            },
            new TextElement { X = 30, Y = CodeRow + 1, Text = $"{Theme.Arrow} 188", Color = Theme.Muted, Wait = true },
            new ErrorPanel
            {
                Y = ErrorRow, Wait = true,
                Lines = ["error C64006: The constant 700", "does not fit in 8 bits ... so it", "would become 188."],
            },
            new Squiggle { X = LiteralColumn, Y = CodeRow + 1, Width = 3 },
        ]
    };

    static Table Sizes(uint x, string[][] rows) => new() { X = x, Y = Theme.BodyTop, Widths = [10, 7], Header = ["C# type", "size"], Rows = rows };
}

// An IDE-style error: a red bar down the left, the message in the code face
// in light red, one line per row.
sealed class ErrorPanel : SlideElement
{
    public uint X { get; init; }
    public uint Y { get; init; }
    public required string[] Lines { get; init; }
    public bool Wait { get; init; }

    public override void Draw()
    {
        if (Wait)
            Deck.Step();
        uint left = Theme.OrCells(X, Theme.Left);
        uint height = (uint)Lines.Length;
        Deck.Paint(left, Y, 1, height, Theme.Bad, Theme.Background);
        ulong x = Screen.Px(left);
        Screen.Fill(x, Screen.Px(Y), x + 2, Screen.Px(Y + height) - 1);
        for (uint i = 0; i < height; i++)
            Text.Code(left + 1, Y + i, Lines[i], Theme.Bad, Theme.Background);
    }
}

// A red underline under Width cells of a code line, which turn red too.
sealed class Squiggle : SlideElement
{
    public uint X { get; init; }
    public uint Y { get; init; }
    public uint Width { get; init; }

    public override void Draw()
    {
        Deck.Paint(X, Y, Width, 1, Theme.Bad, Theme.Code);
        ulong x = Screen.Px(X), y = Screen.Px(Y) + 7;
        Screen.Line(x, y, x + Screen.Px(Width) - 7, y);
    }
}
