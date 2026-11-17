using System;
using C64Presentation.Slides;

#if !SCENE_ENGINE
using Part3 = C64Presentation.Slides.Part3.Part3;
#endif

namespace C64Presentation;

static class Program
{
    // The deck, in order. Each entry builds its slide when it is shown, so
    // only one slide's objects live on the heap at a time.
#if SCENE_ENGINE
    static readonly Func<Slide>[] Slides = EngineGallery.Slides;
#else
    // DECK.md order: slides 1-8 (Slides/Part1), 9-12 (Slides/Part2) and
    // 13-17 (Slides/Part3).
    static readonly Func<Slide>[] Slides = [.. Part1.Slides, .. Part2.Slides, .. Part3.Slides];
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
