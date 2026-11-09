using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 8: a Catacombs room (A1, multicolour, from the disk) above a raster
// split, and a lower third in the hi-res band under it: the title with an
// arrow up at the picture, the two facts beside it. No deck chrome. Row 21,
// the first under the split, stays blank.
static class BuiltWithThisSlide
{
    const uint Split = 21;
    const uint Facts = 18;    // the facts' column

    public static Slide Create() => new SplitPictureSlide
    {
        Picture = DiskPicture.ShotRoom,
        Split = Split,
        Elements =
        [
            new Arrow { FromCol = Theme.Left, FromRow = Split + 3, ToCol = Theme.Left, ToRow = Split + 1 },   // up at the picture
            new TextElement { X = Theme.Left + 2, Y = Split + 2, Text = "Built with this", Color = Theme.Accent },
            new TextElement { X = Facts, Y = Split + 1, Text = "Catacombs: 3,477 lines of C#", Wait = true },
            new TextElement { X = Facts, Y = Split + 3, Text = "61 rooms, 10,478 bytes still free", Wait = true },
        ]
    };
}

// A multicolour picture from the disk over the whole width, from the top
// down to a raster split, and elements in the hi-res band under it.
sealed class SplitPictureSlide : SimpleSlide
{
    public DiskPicture Picture { get; init; }
    public uint Split { get; init; }

    public override void Present()
    {
        Deck.Picture(ArtDisk.Load(Picture), 0, 0, Split);
        base.Present();
    }
}
