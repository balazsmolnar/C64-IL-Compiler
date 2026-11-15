using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides.Part3;

// Section 6, slide 17 (DECK.md).
static class SmallAndFast
{
    // 17: the rule every optimization follows.
    public static Slide Section() => new SimpleSlide
    {
        Elements =
        [
            new SectionElement { Number = "03", Title = "Small and fast", Subtitle = "Pay only for what you use", Icon = C64Address.FromLabel("img_SecChart") },
            new TextElement { X = Theme.Left, Y = 17, Text = ".weak Flag_X", Color = Theme.Muted },
        ]
    };
}
