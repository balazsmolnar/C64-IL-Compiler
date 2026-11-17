using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 12: strings and floats are borrowed from what is already there.
// Two columns under accent headings, a hairline between them; each column,
// its bullets and then a byte diagram of what they say, is one build step.
static class BorrowSlide
{
    const uint ListTop = 9, ListPitch = 2, BytesRow = 15, ByteHeight = 3, CaptionRow = 19;
    const uint L = Theme.LeftColumn, R = Theme.RightColumn, Cell = 3;    // one byte of a diagram: 3 cells

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Borrow, not build" },
            new TextElement { X = L, Y = Theme.BodyTop, Text = "STRINGS", Face = TextFace.Heading, Color = Theme.Accent },
            new TextElement { X = R, Y = Theme.BodyTop, Text = "FLOATS", Face = TextFace.Heading, Color = Theme.Accent },
            new ColumnRule(),

            new BuildStep { Press = true },
            new BulletList { Left = L, Top = ListTop, Pitch = ListPitch, Items = ["Zero-terminated bytes", "Not on the heap", "a + b: one 40-byte buffer"] },
            new BuildStep(),
            new Box { X = L, Y = BytesRow, Width = Cell, Height = ByteHeight, Text = "C" },
            new Box { X = L + Cell, Y = BytesRow, Width = Cell, Height = ByteHeight, Text = "6", ConnectToLeft = true },
            new Box { X = L + 2 * Cell, Y = BytesRow, Width = Cell, Height = ByteHeight, Text = "4", ConnectToLeft = true },
            new Box { X = L + 3 * Cell, Y = BytesRow, Width = Cell, Height = ByteHeight, Text = "0", Color = Theme.Accent2, ConnectToLeft = true },    // the terminator
            new TextElement { X = L, Y = CaptionRow, Text = "'C64' in memory", Color = Theme.Muted },

            new BuildStep { Press = true },
            new BulletList { Left = R, Top = ListTop, Pitch = ListPitch, Items = ["C64's 5-byte format", "Math: BASIC ROM routines", "Sin, Cos, Sqrt"] },
            new BuildStep(),
            new Box { X = R, Y = BytesRow, Width = Cell, Height = ByteHeight, Text = "exp" },
            new Box { X = R + Cell, Y = BytesRow, Width = Cell, Height = ByteHeight, Text = "m1", ConnectToLeft = true },
            new Box { X = R + 2 * Cell, Y = BytesRow, Width = Cell, Height = ByteHeight, Text = "m2", ConnectToLeft = true },
            new Box { X = R + 3 * Cell, Y = BytesRow, Width = Cell, Height = ByteHeight, Text = "m3", ConnectToLeft = true },
            new Box { X = R + 4 * Cell, Y = BytesRow, Width = Cell, Height = ByteHeight, Text = "m4", ConnectToLeft = true },
            new TextElement { X = R, Y = CaptionRow, Text = "one float: 5 bytes", Color = Theme.Muted },
        ]
    };
}

// The 1 px Grey2 rule between the two columns, under their headings.
sealed class ColumnRule : SlideElement
{
    const uint Column = Theme.RightColumn - 2, Top = 8, Bottom = 21;

    public override void Draw()
    {
        Deck.Paint(Column, Top, 1, Bottom - Top, Theme.Muted, Theme.Background);
        ulong x = Screen.Px(Column) + 4;
        Screen.Line(x, Screen.Px(Top), x, Screen.Px(Bottom) - 1);
    }
}

// A build step on its own. Press: what follows appears with the next press
// (here a whole column is one step, which BulletList.Wait, one step per
// item, cannot express); otherwise what came before fades in now and what
// follows fades in right after it (a cascade, and each fade stays within
// the deck's limit of rectangles per step).
sealed class BuildStep : SlideElement
{
    public bool Press { get; init; }

    public override void Draw()
    {
        if (Press)
            Deck.Step();
        else
            Deck.Build(onSurface: false);
    }
}
