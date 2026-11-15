using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 2: the target machine. The breadbox left, one fact per press right,
// the list centred on the picture; then the point, under both.
static class MachineSlide
{
    const uint Top = 7;          // the picture's and the list's first row
    const uint Point = 18;

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "64 KB at 1 MHz" },
            new ImageElement { Column = 1, Row = Top, Width = 18, Height = 8, Image = C64Address.FromLabel("img_Breadbox") },
            new BulletList
            {
                Left = Theme.RightColumn, Top = Top, Wait = true,
                Items = ["64 KB RAM", "1 MHz 6510 CPU", "A, X, Y: 8 bits", "256-byte stack", "320x200, 16 colours"],
            },
            new TextElement { X = Theme.Left, Y = Point, Text = "Everything that follows is fitting C# into that.", Color = Theme.Heading, Wait = true },
        ]
    };
}
