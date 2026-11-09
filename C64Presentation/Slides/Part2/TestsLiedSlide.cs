using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 11: what C64006 found in our own code: each value as written,
// struck through, and the value it really had.
static class TestsLiedSlide
{
    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Our tests lied" },
            new TextElement { X = Theme.Left, Y = Theme.BodyTop, Text = "The new error found, in our code:" },
            new Correction { Y = 8, Before = "i <", Old = "256", New = "0", After = " loops that ran 0 times" },
            new Correction { Y = 11, Before = "new uint[", Old = "256", New = "0", After = "]: an empty array" },
            new Correction { Y = 14, Before = "This deck: Wait(", Old = "500", New = "244", After = ") waited 244" },
        ]
    };
}

// A bullet line with a correction in it: Before, the Old value struck
// through in Grey2, the New value in light red, then After. Each part is
// in cells of its own (one colour per cell); New is set to end just
// before a cell edge, so After follows it at the usual letter spacing.
// One build step.
sealed class Correction : SlideElement
{
    const ulong TextOffset = 10;      // as BulletList: 5 px after the dot
    const ulong Gap = 4;

    public uint Y { get; init; }
    public required string Before { get; init; }
    public required string Old { get; init; }
    public required string New { get; init; }
    public required string After { get; init; }

    public override void Draw()
    {
        Deck.Step();
        ulong x = Screen.Px(Theme.Left);
        Text.Draw(x + 1, Y, Theme.Dot, Theme.Accent);
        x = Screen.CellAfter(Text.Draw(x + TextOffset, Y, Before, Theme.Heading), 1);
        ulong end = Text.Draw(x, Y, Old, Theme.Muted);
        ulong strike = Screen.Px(Y) + 4;
        Screen.Line(x, strike, end - 2, strike);
        x = Screen.CellAfter(end, Gap);
        ulong width = Text.Width(New);
        ulong slack = Screen.CellAfter(x + width, 0) - x - width;
        Text.Draw(slack > 2 ? x + slack - 2 : x, Y, New, Theme.Bad);
        Text.Draw(Screen.CellAfter(x + width, 0), Y, After, Theme.Heading);
    }
}
