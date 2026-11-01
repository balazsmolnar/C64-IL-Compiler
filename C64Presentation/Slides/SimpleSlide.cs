using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// A slide made of elements, drawn in order. Elements with Wait / WaitAfter
// set are build steps.
class SimpleSlide : Slide
{
    public SlideElement[] Elements;

    public override void Present()
    {
        foreach (var slideElement in Elements)
        {
            slideElement.Draw();
        }
    }
}
