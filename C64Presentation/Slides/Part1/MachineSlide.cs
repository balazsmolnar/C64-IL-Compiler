using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 2: the target machine. The breadbox left, one fact per press right,
// the list centred on the picture.
static class MachineSlide
{
    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "64 KB at 1 MHz" },
            new ImageElement { Column = 1, Row = 7, Width = 18, Height = 8, Image = C64Address.FromLabel("img_Breadbox") },
            new BulletList
            {
                Left = Theme.RightColumn, Top = 7, Wait = true,
                Items = ["64 KB RAM", "1 MHz 6510 CPU", "A, X, Y: 8 bits", "256-byte stack", "320x200, 16 colours"],
            },
        ]
    };
}
