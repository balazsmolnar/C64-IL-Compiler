using System;
using C64Presentation.Slides;

#if !SCENE_ENGINE
using Part3 = C64Presentation.Slides.Part3;
#endif

namespace C64Presentation;

static class Program
{
    // The deck, in order. Each entry builds its slide when it is shown, so
    // only one slide's objects live on the heap at a time.
    // (Part 3 only while the parts are built in parallel: the integrator
    // combines Part1, Part2 and Part3 here.)
#if SCENE_ENGINE
    static readonly Func<Slide>[] Slides = EngineGallery.Slides;
#else
    static readonly Func<Slide>[] Slides = Part3.Part3.Slides;
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
