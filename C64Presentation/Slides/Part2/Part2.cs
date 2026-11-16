using System;

namespace C64Presentation.Slides;

// DECK.md section 5, "The hard parts": slides 9-12 of the 16-slide talk.
static class Part2
{
    public static readonly Func<Slide>[] Slides =
    [
        HardPartsSlide.Create,
        ReferencesSlide.Create,
        GcSlide.Create,
        BorrowSlide.Create,
    ];
}
