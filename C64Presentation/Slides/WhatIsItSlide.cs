using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

static class WhatIsItSlide
{
    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "What is it?" },
            new BulletList { Items = ["Running .NET code on the C64", "Challenges:"], Wait = true },
            new Table
            {
                Y = 10, Widths = [9, 8, 9, 10], WaitAfterRow = true,
                Header = ["", "C64", "PC", "Ratio"],
                Rows =
                [
                    ["Memory", "64 KB", "32 GB", "x524,288"],
                    ["CPU", "1 MHz", "8x1.8 GHz", "x14,400"],
                    ["Bits", "8", "64", "x8"],
                ],
            },
        ]
    };
}
