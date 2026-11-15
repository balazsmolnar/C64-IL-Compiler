using System;

namespace C64Presentation.Slides.Part3;

// Part 3 of the deck (DECK.md), in order: slides 17, 22, 23 and 27 (the
// user cut 18-21 and 24-26). The hidden debugger appendix is in a scene
// build's list only, for tools\presentation-shot.ps1, until the storyboard
// decides whether and how it is reached.
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
