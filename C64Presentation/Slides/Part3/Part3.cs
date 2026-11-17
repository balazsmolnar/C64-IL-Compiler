using System;

namespace C64Presentation.Slides.Part3;

// Part 3 of the deck (DECK.md), in order: slides 13-16. The hidden debugger
// appendix is reached with D on slide 15; a scene build also lists it,
// entered the same way, for tools\presentation-shot.ps1.
static class Part3
{
    public static readonly Func<Slide>[] Slides =
    [
        SmallAndFast.Section,
        Testing.TestsRunOn6502,
        Testing.PostSharpSwapsIt,
        ThankYouSlide.Create,
#if SCENE
        HiddenScene.Create,     // scene builds only: the appendix, entered as a hidden slide (-Slide 4)
#endif
    ];
}
