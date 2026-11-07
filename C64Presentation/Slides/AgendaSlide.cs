using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

static class AgendaSlide
{
    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Agenda" },
            new BulletList
            {
                Top = 6,
                Wait = true,
                Items = ["What is it?", "How does it work?", "Features", "Garbage collection", "Integration tests"],
            },
        ]
    };
}
