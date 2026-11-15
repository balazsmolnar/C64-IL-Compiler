using System;

namespace C64Presentation.Slides;

// DECK.md section 5, "The hard parts": slides 9, 13, 14 and 15 (10-12 and 16 were cut).
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
