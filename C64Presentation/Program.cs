using System;
using C64Lib;
using C64Presentation.SlideElements;
using C64Presentation.Slides;

namespace C64Presentation;

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
            () => new DemoSlide(),
            MemoryLayoutSlide.Create,
            () => new GarbageCollectorSlide(),
            () => new DemoSlide(),
        };

        Presenter.Present(slides);
    }
}