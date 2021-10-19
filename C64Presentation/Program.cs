using System;
using C64Lib;
using C64Presentation.Slides;

namespace C64Presentation
{
    class Program
    {
        static void Main()
        {

            var slides = new Func<Slide>[]
            {
                WhatIsItSlide.Create,
                () =>  new IntroSlide(),
                AgendaSlide.Create,
                CompilerPassesSlide.Create,
                () => new DemoSlide(),
                FeaturesSlide.Create,
                MemoryLayoutSlide.Create,
            };

            Presenter.Present(slides);
        }
    }
}
