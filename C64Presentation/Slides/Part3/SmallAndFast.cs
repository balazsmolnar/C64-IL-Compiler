using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides.Part3;

// Section 6, slide 13 (DECK.md): the rule, and what it bought.
static class SmallAndFast
{
    public static Slide Section() => new SimpleSlide
    {
        Elements =
        [
            new SectionElement { Number = "03", Title = "Small and fast", Subtitle = "Pay only for what you use", Icon = C64Address.FromLabel("img_SecChart") },
            new TextElement { X = 19, Y = 15, Text = ".weak Flag_X", Color = Theme.Muted },
            new TextElement { X = Theme.Left, Y = 17, Text = $"Add: 39 bytes {Theme.Arrow} {Theme.Hi}12", Wait = true },
            new TextElement { X = Theme.Left, Y = 19, Text = $"Hunchback: {Theme.Hi}-24%{Theme.Off}, same C#", Wait = true },
        ]
    };
}
