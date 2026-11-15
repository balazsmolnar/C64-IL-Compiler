using System;

namespace C64Presentation.Slides.Part3;

// Part 3 of the deck (DECK.md), in order: slides 13-16. The hidden debugger
// appendix is reached with D on slide 15; a scene build also lists it, for
// tools\presentation-shot.ps1.
static class Part3
{
    public static readonly Func<Slide>[] Slides =
    [
        SmallAndFast.Section,
        Testing.TestsRunOn6502,
        Testing.PostSharpSwapsIt,
        ThankYouSlide.Create,
#if SCENE
        DebuggerMock.Create,    // scene builds only, to look at it (-Slide 4)
#endif
    ];
}
