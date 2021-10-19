using C64Lib;
using C64Presentation.Animations;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides
{
    static class AgendaSlide
    {
        public static Slide Create()
        {
            return new SimpleSlide()
            {
                Elements = new SlideElement[]
                {
                    new Title() { Text = "AGENDA" },
                    new BulletList()
                    {
                        Left = 5, Top = 10,
                        Items = new[]
                        {
                            "WHAT IS IT?",
                            "HOW DOES IT WORK?", 
                            "FEATURES",
                            "GARBAGE COLLECTION",
                            "INTEGRATION TESTS"
                        },
                        Color = Colors.Green
                    }
                },
                Animation = new DiagonalClearAnimation()
            };
        }
    }
}
