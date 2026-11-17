using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 9: the section divider, with the topics of the two slides that
// follow as A6 icons, each over its word, the pair centred on the content.
static class HardPartsSlide
{
    const uint IconRow = 15, Pitch = 12;
    const uint First = 12, Second = First + Pitch;      // icon columns 12 and 24: centres 13.5 and 25.5, about 19.5

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new SectionElement { Number = "05", Title = "The hard parts", Subtitle = "", Icon = C64Address.FromLabel("img_SecChip") },
            new Topic { X = First, Y = IconRow, Icon = C64Address.FromLabel("img_IconGc"), Label = "GC" },
            new Topic { X = Second, Y = IconRow, Icon = C64Address.FromLabel("img_IconString"), Label = "strings & floats" },
        ]
    };
}

// A 3x3 pixels-only icon (A6) in cyan, its label centred under it in Grey3.
sealed class Topic : SlideElement
{
    const uint IconCells = 3, LabelGap = 1, LabelCells = 11;    // the label is centred over 11 cells around the icon

    public uint X { get; init; }
    public uint Y { get; init; }
    public ulong Icon { get; init; }
    public required string Label { get; init; }

    public override void Draw()
    {
        Deck.Paint(X, Y, IconCells, IconCells, Theme.Accent, Theme.Background);
        C64.Screen.DrawImage(X, Y, Icon);
        Text.Center(Screen.Px(X - (LabelCells - IconCells) / 2), Screen.Px(LabelCells), Y + IconCells + LabelGap, Label, Theme.Body);
    }
}
