using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 15: strings and floats are borrowed from what is already there.
// Two columns under accent headings, a hairline between them.
static class BorrowSlide
{
    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Borrow, not build" },
            Heading(Theme.LeftColumn, "STRINGS"),
            Heading(Theme.RightColumn, "FLOATS"),
            new ColumnRule(),
            new Points { X = Theme.LeftColumn, Items = [["Pointer to bytes,", "zero-terminated"], ["Not on the heap"], ["a + b: a 40-byte", "buffer"]] },
            new Points { X = Theme.RightColumn, Items = [["5-byte C64 format"], ["Math: the BASIC", "ROM's own routines"], ["Sin, Cos, Sqrt"]] },
        ]
    };

    static TextElement Heading(uint column, string text)
        => new() { X = column, Y = Theme.BodyTop, Text = text, Face = TextFace.Heading, Color = Theme.Accent };
}

// The 1 px Grey2 rule between the two columns, under their headings.
sealed class ColumnRule : SlideElement
{
    const uint Column = 19, Top = 8, Bottom = 20;

    public override void Draw()
    {
        Deck.Paint(Column, Top, 1, Bottom - Top, Theme.Muted, Theme.Background);
        ulong x = Screen.Px(Column) + 4;
        Screen.Line(x, Screen.Px(Top), x, Screen.Px(Bottom) - 1);
    }
}

// Bullets that may wrap: each item a list of lines, 2 rows apart, items 3
// rows apart, from row 9. All in one build step.
sealed class Points : SlideElement
{
    const uint Top = 9, LinePitch = 2, ItemGap = 1;

    public uint X { get; init; }
    public required string[][] Items { get; init; }

    public override void Draw()
    {
        Deck.Step();
        ulong x = Screen.Px(X);
        uint row = Top;
        foreach (var lines in Items)
        {
            Text.Draw(x + 1, row, Theme.Dot, Theme.Accent);
            foreach (var line in lines)
            {
                Text.Draw(x + 10, row, line, Theme.Body);
                row += LinePitch;
            }
            row += ItemGap;
        }
    }
}
