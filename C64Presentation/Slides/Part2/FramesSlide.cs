using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 12: the problem (a full stack), the fix (a frame per method at a
// fixed address), and the trick: methods that never run at the same time
// share bytes. Left: the story; right: a call tree and the StaticFrames
// strip, where Update and Draw share one frame, and Move and Hud another.
static class FramesSlide
{
    // The story, left.
    const uint ProblemRow = Theme.BodyTop, IconRow = 7, FixRow = 12, ShareRow = 14, ResultRow = 17;
    // The call tree: three levels, the arrows in the gaps between them.
    const uint TreeLeft = 21, TreeWidth = 11, BoxWidth = 5, LevelPitch = 5;
    const uint Child = TreeLeft, Sibling = TreeLeft + TreeWidth - BoxWidth;
    const uint ChildMiddle = Child + BoxWidth / 2, SiblingMiddle = Sibling + BoxWidth / 2;
    const uint CaptionRow = 19;
    // The strip.
    const uint StripColumn = 33, StripWidth = Theme.Right - StripColumn, StripLabelColumn = 30, StripLabelRow = CaptionRow;
    const uint FreeRow = Theme.BodyTop + 10;

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Frames share bytes" },
            Line(ProblemRow, "Deep calls overflow 256 bytes"),
            new Topic { X = Theme.Left, Y = IconRow, Icon = C64Address.FromLabel("img_IconStack"), Label = "256 B: full", Color = Theme.Bad },

            Line(FixRow, "Fix: frames at fixed addresses", wait: true),
            Method(TreeLeft, 0, TreeWidth, "Main", Theme.Body),
            Call(ChildMiddle, 0), Call(SiblingMiddle, 0),
            Method(Child, 1, BoxWidth, "Update", Theme.Accent), Method(Sibling, 1, BoxWidth, "Draw", Theme.Accent2),
            Call(ChildMiddle, 1), Call(SiblingMiddle, 1),
            Method(Child, 2, BoxWidth, "Move", Theme.Accent), Method(Sibling, 2, BoxWidth, "Hud", Theme.Accent2),
            new TextElement { X = TreeLeft, Y = CaptionRow, Text = "illustration", Color = Theme.Muted },

            Line(ShareRow, "Never-concurrent calls share", wait: true),
            new TextElement { X = StripLabelColumn, Y = StripLabelRow, Text = "StaticFrames", Color = Theme.Muted },
            Frame(0, 2, "Main", "", Theme.Body),
            Frame(2, 4, "Update", "= Draw", Theme.Accent),
            Frame(6, 4, "Move", "= Hud", Theme.Accent),
            new Box { X = StripColumn, Y = FreeRow, Width = StripWidth, Height = 2, Text = "free", Color = Theme.Frame, Hollow = true },

            Line(ResultRow, $"{Theme.Hi}Catacombs: 118 of 125 methods", wait: true),
            Line(ResultRow + 2, $"{Theme.Hi}in 39 bytes of frames"),
        ]
    };

    static TextElement Line(uint row, string text, bool wait = false) => new() { X = Theme.Left, Y = row, Text = text, Wait = wait };

    static uint LevelRow(uint level) => Theme.BodyTop + level * LevelPitch;

    static Box Method(uint column, uint level, uint width, string name, Colors color)
        => new() { X = column, Y = LevelRow(level), Width = width, Height = 3, Text = name, Color = color };

    // A call: down from one level to the next, through the gap between them.
    static Arrow Call(uint column, uint level)
        => new() { FromCol = column, FromRow = LevelRow(level) + 3, ToCol = column, ToRow = LevelRow(level) + 4, Color = Theme.Muted };

    // One frame in the strip, `offset` rows down; a shared frame names its
    // second method in the caption.
    static Box Frame(uint offset, uint height, string name, string shared, Colors color)
        => new() { X = StripColumn, Y = Theme.BodyTop + offset, Width = StripWidth, Height = height, Text = name, Caption = shared, Color = color };
}
