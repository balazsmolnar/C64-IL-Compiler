using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 10: the C# types and their sizes, then a loop bound that does not
// fit: what it silently became, and the error it is now.
static class IntIs8BitsSlide
{
    const uint CodeRow = 12, CodeWidth = 26, ErrorRow = 18;
    const uint ResultColumn = Theme.Left + CodeWidth + 2;
    const string Loop = "for (uint i = 0; i < ";     // the code up to the literal

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "int is 8 bits" },
            Sizes(Theme.LeftColumn, ["C# type", "size"], [["int, uint", "8 bits"], ["long, ulong", "16 bits"]]),
            Sizes(Theme.RightColumn, ["", ""], [["float", "5 bytes"], ["double", $"{Theme.Red}error"]]),
            new CodeBlock
            {
                X = Theme.Left, Y = CodeRow, Width = CodeWidth, Rows = 2, Wait = true,
                Lines = [$"{Theme.Kw}for {Theme.Pl}({Theme.Kw}uint{Theme.Pl} i = 0; i < 700; i++)"],
            },
            new TextElement { X = ResultColumn, Y = CodeRow + 1, Text = $"{Theme.Arrow} 188", Color = Theme.Muted, Wait = true },
            new ErrorPanel
            {
                Y = ErrorRow, Wait = true,
                Lines = ["error C64006: The constant 700", "does not fit in 8 bits ... so it", "would become 188."],
            },
            new Underline { Column = Theme.Left + 1, Row = CodeRow + 1, Before = Loop, Text = "700" },
        ]
    };

    static Table Sizes(uint x, string[] header, string[][] rows) => new() { X = x, Y = Theme.BodyTop, Widths = [10, 7], Header = header, Rows = rows };
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

// A red 2 px underline under Text in a code line that starts at Column and
// reads Before first; drawn in the (empty) code row under it, so the code
// keeps its colours.
sealed class Underline : SlideElement
{
    public uint Column { get; init; }
    public uint Row { get; init; }
    public required string Before { get; init; }
    public required string Text { get; init; }

    public override void Draw()
    {
        ulong font = C64Address.FromLabel("font_BodyMono");
        ulong x = Screen.Px(Column) + C64.Screen.FontTextWidth(Before, font);
        ulong end = x + C64.Screen.FontTextWidth(Text, font) - 2;
        uint first = (uint)(x / Theme.CellPixels);
        Deck.Paint(first, Row + 1, (uint)(end / Theme.CellPixels) + 1 - first, 1, Theme.Bad, Theme.Code);
        ulong y = Screen.Px(Row + 1);
        Screen.Fill(x, y, end, y + 1);
    }
}
