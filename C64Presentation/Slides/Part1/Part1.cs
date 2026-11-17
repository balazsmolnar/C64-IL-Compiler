using System;

namespace C64Presentation.Slides;

// DECK.md slides 1-8: the hook, what it is, how it works, built with this.
static class Part1
{
    // Method groups, no lambdas: the compiler names every lambda class <>c,
    // and two of them in one program collide.
    public static readonly Func<Slide>[] Slides =
    [
        TitleSlide.Create,
        MachineSlide.Create,
        OrdinaryCSharpSlide.Create,
        WhatCompilesSlide.Create,
        PipelineSlide.Create,
        IlStackSlide.Create,
        AddCostSlide.Create,
        BuiltWithThisSlide.Create,
    ];
}
