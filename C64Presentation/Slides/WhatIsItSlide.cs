using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C64Lib;
using C64Presentation.Animations;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides
{
    class WhatIsItSlide
    {
        public static Slide Create()
        {
            return new SimpleSlide()
            {
                Elements = new SlideElement[]
                {
                    new Title() { Text = "WHAT IS IT?" },
                    new BulletList()
                    {
                        Left = 0, Top = 7,
                        Items = new[]
                        {
                            "C# CODE TO C64 PRG",
                            "CHALLENGES",
                        },
                        Color = Colors.Green,
                        Wait = true
                    },
                    new Table { X = 0, Y = 12, CollNum = 4, RowNum = 4, CellHeight = 3, CellWidth = 10, WaitAfterRow = true, TextColor = Colors.Grey3, BorderColor = Colors.Grey1, Texts = new []
                    {
                        "", "C64", "PC", "DIFF",
                        "MEMORY", "64K", "32GB", "*524,288",
                        "CPU", "1MHZ", "8*1.8GHZ", "*14,400",
                        "BITS", "8", "64", "*8"
                    }},
                },

                Animation = new DiagonalClearAnimation()
            };
        }

    }
}
