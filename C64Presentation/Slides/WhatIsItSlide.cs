using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

class WhatIsItSlide
{
    public static Slide Create()
    {
        return new SimpleSlide()
        {
            Elements = new SlideElement[]
            {
                new Title() { Text = "What is it?" },
                new BulletList()
                {
                    Items = new[]
                    {
                        "Running .NET code on the C64",
                        "Challenges:",
                    },
                    Wait = true
                },
                new Table { Y = 10, CollNum = 4, RowNum = 4, ColumnWidths = new uint[] { 9, 8, 9, 10 }, WaitAfterRow = true, Texts = new []
                {
                    "", "C64", "PC", "Ratio",
                    "Memory", "64 KB", "32 GB", "x524,288",
                    "CPU", "1 MHz", "8x1.8 GHz", "x14,400",
                    "Bits", "8", "64", "x8"
                }},
            }
        };
    }
}
