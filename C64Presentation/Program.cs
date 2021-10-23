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
                () =>  new IntroSlide(),
                AgendaSlide.Create,
                WhatIsItSlide.Create,
                CompilerPassesSlide.Create,
                () => new DemoSlide(),
                FeaturesSlide.Create,
                MemoryLayoutSlide.Create,
                () => new GarbageCollectorSlide(),
            };

            Presenter.Present(slides);
        }
    }
}
