using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 9: the section divider, with the six topics of slides 10-16 as
// icons (A6) in two rows of three, on even columns.
static class HardPartsSlide
{
    const uint FirstRow = 15, SecondRow = 19;
    const uint First = Theme.Left, Pitch = 13, Second = First + Pitch, Third = Second + Pitch;

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new SectionElement { Number = "05", Title = "The hard parts", Subtitle = "", Icon = C64Address.FromLabel("img_SecChip") },
            new Topic { X = First, Y = FirstRow, Icon = C64Address.FromLabel("img_IconInt8"), Label = "8-bit int" },
            new Topic { X = Second, Y = FirstRow, Icon = C64Address.FromLabel("img_IconStack"), Label = "stack" },
            new Topic { X = Third, Y = FirstRow, Icon = C64Address.FromLabel("img_IconObject"), Label = "objects" },
            new Topic { X = First, Y = SecondRow, Icon = C64Address.FromLabel("img_IconGc"), Label = "GC" },
            new Topic { X = Second, Y = SecondRow, Icon = C64Address.FromLabel("img_IconString"), Label = "strings & floats" },
            new Topic { X = Third, Y = SecondRow, Icon = C64Address.FromLabel("img_IconBitmap"), Label = "bitmaps" },
        ]
    };
}

// A 3x3 pixels-only icon (A6) with its label beside its middle row: a cyan
// icon with a Grey3 label, or both in Color.
sealed class Topic : SlideElement
{
    const uint IconCells = 3;
    const ulong LabelGap = 4;

    public uint X { get; init; }
    public uint Y { get; init; }
    public ulong Icon { get; init; }
    public required string Label { get; init; }
    public Colors Color { get; init; }

    public override void Draw()
    {
        Deck.Paint(X, Y, IconCells, IconCells, Theme.Or(Color, Theme.Accent), Theme.Background);
        C64.Screen.DrawImage(X, Y, Icon);
        Text.Draw(Screen.Px(X + IconCells) + LabelGap, Y + 1, Label, Theme.Or(Color, Theme.Body));
    }
}
