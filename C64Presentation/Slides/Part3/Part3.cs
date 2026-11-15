using System;

namespace C64Presentation.Slides.Part3;

// Slides 17-27 (DECK.md sections 6-9), in order. The hidden appendix is not
// in the deck's list: slide 25 shows it on D (ChangeItLiveSlide); a scene
// build appends it, for tools\presentation-shot.ps1.
static class Part3
{
    public static readonly Func<Slide>[] Slides =
    [
        SmallAndFast.Section,
        SmallAndFast.BytesTo12,
        SmallAndFast.Hunchback,
        SmallAndFast.NoSingleWinner,
        SmallAndFast.WorstFrame,
        Testing.TestsRunOn6502,
        Testing.PostSharpSwapsIt,
        Testing.LineToAddress,
        ChangeItLiveSlide.Create,
        Close.FourLessons,
        ThankYouSlide.Create,
#if SCENE
        DebuggerMock.Create,    // scene builds only, to look at it (-Slide 11)
#endif
    ];
}
