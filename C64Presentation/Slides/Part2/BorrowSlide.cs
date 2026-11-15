using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 15: strings and floats are borrowed from what is already there.
// Two columns under accent headings, a hairline between them; each column
// is one build step.
static class BorrowSlide
{
    const uint ListTop = 9, ListPitch = 3;

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Borrow, not build" },
            Heading(Theme.LeftColumn, "STRINGS"),
            Heading(Theme.RightColumn, "FLOATS"),
            new ColumnRule(),
            new Press(),
            List(Theme.LeftColumn, ["Zero-terminated bytes", "Not on the heap", "a + b: one 40-byte buffer"]),
            new Press(),
            List(Theme.RightColumn, ["C64's 5-byte format", "Math: BASIC ROM routines", "Sin, Cos, Sqrt"]),
        ]
    };

    static TextElement Heading(uint column, string text)
        => new() { X = column, Y = Theme.BodyTop, Text = text, Face = TextFace.Heading, Color = Theme.Accent };

    static BulletList List(uint column, string[] items) => new() { Left = column, Top = ListTop, Pitch = ListPitch, Items = items };
}

// The 1 px Grey2 rule between the two columns, under their headings.
sealed class ColumnRule : SlideElement
{
    const uint Column = Theme.RightColumn - 2, Top = 8, Bottom = 17;

    public override void Draw()
    {
        Deck.Paint(Column, Top, 1, Bottom - Top, Theme.Muted, Theme.Background);
        ulong x = Screen.Px(Column) + 4;
        Screen.Line(x, Screen.Px(Top), x, Screen.Px(Bottom) - 1);
    }
}

// A build step on its own: what follows appears on the next press.
sealed class Press : SlideElement
{
    public override void Draw() => Deck.Step();
}
