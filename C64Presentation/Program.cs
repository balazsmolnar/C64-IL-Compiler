using System;
using C64Presentation.Slides;

namespace C64Presentation;

static class Program
{
    // The deck, in order. Each entry builds its slide when it is shown, so
    // only one slide's objects live on the heap at a time.
    static readonly Func<Slide>[] Slides =
    [
        () => new IntroSlide(),
        AgendaSlide.Create,
        WhatIsItSlide.Create,
        CompilerPassesSlide.Create,
        () => new DemoSlide(),
        FeaturesSlide.Create,
        () => new DemoSlide(),
        MemoryLayoutSlide.Create,
        () => new GarbageCollectorSlide(),
        () => new DemoSlide(),
    ];

    static void Main()
    {
#if SCENE_ART
        ArtScene.Run();   // art/ArtScene.cs: asset review pages
#endif
#if SCENE_DISK
        DiskScene.Run();  // art/DiskScene.cs: a picture loaded from the disk
#endif
        Presenter.Present(Slides);
    }
}
