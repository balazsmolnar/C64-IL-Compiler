using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

static class AgendaSlide
{
    public static Slide Create()
    {
        return new SimpleSlide()
        {
            Elements = new SlideElement[]
            {
                new Title() { Text = "Agenda" },
                new BulletList()
                {
                    Items = new[]
                    {
                        "What is it?",
                        "How does it work?",
                        "Features",
                        "Garbage collection",
                        "Integration tests"
                    },
                    Top = 6,
                    Wait = true
                }
            }
        };
    }
}
