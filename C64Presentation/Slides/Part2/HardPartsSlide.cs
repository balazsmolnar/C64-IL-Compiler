using C64Lib;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 9: the section divider before the GC slide; its subtitle names the
// one hard part the talk now shows (strings and floats went with the
// "Borrow, not build" slide).
static class HardPartsSlide
{
    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new SectionElement { Title = "The hard parts", Subtitle = "A garbage collector, on a 6502", Icon = C64Address.FromLabel("img_SecChip") },
        ]
    };
}
