using C64Presentation.Helper;

namespace C64Presentation.Slides;

// The cover: the art's title picture.
sealed class IntroSlide : Slide
{
    public override void Present() => Master.Cover();
}
