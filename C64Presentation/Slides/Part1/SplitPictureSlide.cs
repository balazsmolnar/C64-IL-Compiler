using C64Presentation.Helper;

namespace C64Presentation.Slides;

// A multicolour picture from the disk over the whole width, from the top
// down to a raster split, and elements in the hi-res band under it. No
// deck chrome.
//
//     new SplitPictureSlide { Picture = DiskPicture.ShotRoom, Split = 21, Elements = [...] }
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
