using System;

namespace C64Presentation.Slides;

// DECK.md section 5, "The hard parts": slides 9-16.
static class Part2
{
    public static readonly Func<Slide>[] Slides =
    [
        HardPartsSlide.Create,
        IntIs8BitsSlide.Create,
        TestsLiedSlide.Create,
        FramesSlide.Create,
        ReferencesSlide.Create,
        GcSlide.Create,
        BorrowSlide.Create,
        RomSlide.Create,
    ];
}
