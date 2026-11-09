using System;
using C64Presentation.Slides;

namespace C64Presentation;

static class Program
{
    // The deck, in order. Each entry builds its slide when it is shown, so
    // only one slide's objects live on the heap at a time.
#if SCENE_ENGINE
    static readonly Func<Slide>[] Slides = EngineGallery.Slides;
#else
    // For now only DECK.md slides 1-8 (Slides/Part1); the other parts join
    // when they are merged.
    static readonly Func<Slide>[] Slides = Part1.Slides;
#endif

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
