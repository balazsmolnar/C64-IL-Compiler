using System;
using C64Presentation.Slides;

#if !SCENE_ENGINE
using C64Presentation.Slides.Part3;
#endif

namespace C64Presentation;

static class Program
{
    // The deck, in order. Each entry builds its slide when it is shown, so
    // only one slide's objects live on the heap at a time. Method groups,
    // no lambdas: the compiler names every lambda class <>c, and two of them
    // in one program collide.
#if SCENE_ENGINE
    static readonly Func<Slide>[] Slides = EngineGallery.Slides;
#else
    static readonly Func<Slide>[] Slides =
    [
        // 1-8 (Slides/Part1): the hook, what it is, how it works, built with this
        TitleSlide.Create,
        MachineSlide.Create,
        OrdinaryCSharpSlide.Create,
        WhatCompilesSlide.Create,
        PipelineSlide.Create,
        IlStackSlide.Create,
        AddCostSlide.Create,
        BuiltWithThisSlide.Create,
        // 9-11 (Slides/Part2): the hard parts
        HardPartsSlide.Create,
        GcSlide.Create,
        BorrowSlide.Create,
        // 12-16 (Slides/Part3): small and fast, testing, debugging, the close
        SmallAndFast.Section,
        Testing.TestsRunOn6502,
        Testing.PostSharpSwapsIt,
        DebuggerMock.Create,
        ThankYouSlide.Create,
    ];
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
