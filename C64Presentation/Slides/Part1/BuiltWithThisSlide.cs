using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 8: a Catacombs room (A1, multicolour, from the disk) above a raster
// split, and a lower third in the hi-res band under it: the title with an
// arrow up at the picture, the two facts beside it. Row 21, the first under
// the split, stays blank.
static class BuiltWithThisSlide
{
    const uint Split = 21;
    const uint Top = Split + 1, Middle = Split + 2, Bottom = Split + 3;   // the band's rows
    const uint Facts = 18;    // the facts' column, right of the title

    public static Slide Create() => new SplitPictureSlide
    {
        Picture = DiskPicture.ShotRoom,
        Split = Split,
        Elements =
        [
            new Arrow { FromCol = Theme.Left, FromRow = Bottom, ToCol = Theme.Left, ToRow = Top },   // up at the picture
            new TextElement { X = Theme.Left + 2, Y = Middle, Text = "Built with this", Color = Theme.Accent },
            new TextElement { X = Facts, Y = Top, Text = "Catacombs: 3,477 lines of C#", Wait = true },
            new TextElement { X = Facts, Y = Bottom, Text = "61 rooms, 10,478 bytes still free", Wait = true },
        ]
    };
}
