using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 9: the section divider, with the three topics of slides 10-12 as
// A6 icons, each over its word.
static class HardPartsSlide
{
    const uint IconRow = 15, Pitch = 12;
    const uint First = Theme.Left, Second = First + Pitch, Third = Second + Pitch;

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new SectionElement { Number = "05", Title = "The hard parts", Subtitle = "", Icon = C64Address.FromLabel("img_SecChip") },
            new Topic { X = First, Y = IconRow, Icon = C64Address.FromLabel("img_IconObject"), Label = "objects" },
            new Topic { X = Second, Y = IconRow, Icon = C64Address.FromLabel("img_IconGc"), Label = "GC" },
            new Topic { X = Third, Y = IconRow, Icon = C64Address.FromLabel("img_IconString"), Label = "strings & floats" },
        ]
    };
}

// A 3x3 pixels-only icon (A6) in cyan, its label under it in Grey3, both
// starting at the same column.
sealed class Topic : SlideElement
{
    const uint IconCells = 3, LabelGap = 1;

    public uint X { get; init; }
    public uint Y { get; init; }
    public ulong Icon { get; init; }
    public required string Label { get; init; }

    public override void Draw()
    {
        Deck.Paint(X, Y, IconCells, IconCells, Theme.Accent, Theme.Background);
        C64.Screen.DrawImage(X, Y, Icon);
        Text.Draw(Screen.Px(X), Y + IconCells + LabelGap, Label, Theme.Body);
    }
}
