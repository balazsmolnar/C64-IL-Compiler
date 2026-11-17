using System;

namespace C64Presentation.Slides.Part3;

// Part 3 of the deck (DECK.md), in order: slides 13-17.
static class Part3
{
    public static readonly Func<Slide>[] Slides =
    [
        SmallAndFast.Section,
        Testing.TestsRunOn6502,
        Testing.PostSharpSwapsIt,
        DebuggerMock.Create,
        ThankYouSlide.Create,
    ];
}
