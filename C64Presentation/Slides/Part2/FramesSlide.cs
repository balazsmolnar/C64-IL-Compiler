using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 12: the problem (a full stack), the fix (a frame per method at a
// fixed address), and the trick: methods that never run at the same time
// share bytes. Left: the story; right: a call tree and the StaticFrames
// strip, where Draw lands on Update's bytes and Hud on Move's.
static class FramesSlide
{
    const uint TreeTop = 5, TreePitch = 4;              // the tree's three levels
    const uint Left = 21, Right = 27, Middle = 24;      // its box columns
    const uint StripColumn = 34, StripTop = 6;

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Frames share bytes" },
            Line(5, "Deep calls overflow 256 bytes"),
            new Topic { X = Theme.Left, Y = 8, Icon = C64Address.FromLabel("img_IconStack"), Label = "", Color = Theme.Bad },
            new Arrow { FromCol = 6, FromRow = 10, ToCol = 6, ToRow = 7, Color = Theme.Bad },
            Line(12, "Fix: frames at fixed addresses", wait: true),
            Method(Middle, 0, "Main", Theme.Body),
            Method(Left, 1, "Update", Theme.Accent), Method(Right, 1, "Draw", Theme.Accent2),
            Method(Left, 2, "Move", Theme.Accent), Method(Right, 2, "Hud", Theme.Accent2),
            new TreeLink { From = Middle + 2, To = Left + 2, Row = TreeTop + 3 },
            new TreeLink { From = Middle + 2, To = Right + 2, Row = TreeTop + 3 },
            new TreeLink { From = Left + 2, To = Left + 2, Row = TreeTop + TreePitch + 3 },
            new TreeLink { From = Right + 2, To = Right + 2, Row = TreeTop + TreePitch + 3 },
            new TextElement { X = 24, Y = 17, Text = "illustration", Color = Theme.Muted },
            Line(14, "Never-concurrent calls share", wait: true),
            new TextElement { X = 30, Y = 5, Text = "StaticFrames", Color = Theme.Muted },
            Frame(0, 2, Theme.Body), Frame(2, 3, Theme.Accent), Frame(2, 3, Theme.Accent2, shared: true),
            Frame(5, 2, Theme.Accent), Frame(5, 2, Theme.Accent2, shared: true), Frame(7, 1, Theme.Card),
            Line(17, $"{Theme.Hi}Catacombs: 118 of 125 methods", wait: true),
            Line(19, $"{Theme.Hi}in 39 bytes of frames"),
        ]
    };

    static TextElement Line(uint row, string text, bool wait = false) => new() { X = Theme.Left, Y = row, Text = text, Wait = wait };

    static Box Method(uint column, uint level, string name, Colors color)
        => new() { X = column, Y = TreeTop + level * TreePitch, Width = 5, Height = 3, Text = name, Color = color };

    static FrameBlock Frame(uint offset, uint height, Colors color, bool shared = false)
        => new() { X = StripColumn, Y = StripTop + offset, Width = 4, Height = height, Color = color, Shared = shared };
}

// A tree edge from a parent box's bottom (cell column From, ending on Row - 1)
// down into the gap row Row - 1, across, and down to the child at column To,
// whose box starts on Row + 1... drawn as a 1 px Grey2 line through the
// middle of the cells.
sealed class TreeLink : SlideElement
{
    public uint From { get; init; }
    public uint To { get; init; }
    public uint Row { get; init; }

    public override void Draw()
    {
        uint first = From < To ? From : To;
        uint span = (From < To ? To - From : From - To) + 1;
        Deck.Paint(first, Row, span, 1, Theme.Muted, Theme.Background);
        ulong x0 = Screen.Px(From) + 4, x1 = Screen.Px(To) + 4, top = Screen.Px(Row), y = top + 3;
        Screen.Line(x0, top, x0, y);
        Screen.Line(x0, y, x1, y);
        Screen.Line(x1, y, x1, top + 7);
    }
}

// One method's frame in an address strip: a block of cells in its colour,
// inset 1 px so neighbours stay apart. Shared: it lands on cells another
// frame already has, and takes every other one (a checker of both colours).
// Each block fades in on its own, one after another.
sealed class FrameBlock : SlideElement
{
    const uint FadeFrames = 6;

    public uint X { get; init; }
    public uint Y { get; init; }
    public uint Width { get; init; }
    public uint Height { get; init; }
    public Colors Color { get; init; }
    public bool Shared { get; init; }

    public override void Draw()
    {
        Deck.Build(onSurface: false);
        for (uint row = Y; row < Y + Height; row++)
            for (uint column = X; column < X + Width; column++)
                if (!Shared || ((row + column) & 1) == 1)
                    Deck.Paint(column, row, 1, 1, Color, Theme.Background);
        ulong x = Screen.Px(X), y = Screen.Px(Y);
        Screen.Fill(x + 1, y + 1, x + Screen.Px(Width) - 2, y + Screen.Px(Height) - 2);
        Deck.Show();
        for (uint frame = 0; frame < FadeFrames; frame++)
            C64.Screen.WaitForVBlank();
    }
}
