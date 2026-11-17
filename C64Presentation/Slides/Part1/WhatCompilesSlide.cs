using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 4: what compiles and what is a compile error, as two columns under
// green and red headings, a rule between them. Written out as plain
// initializers: the compiler stores those as tables, cheaper than a helper.
static class WhatCompilesSlide
{
    const Colors Works = Colors.LightGreen;   // a test-result green, this slide only
    const uint Left = Theme.LeftColumn, Right = Theme.RightColumn, Width = Theme.ColumnWidth;
    const uint Heading = 5, Underline = 6;
    const uint Item1 = 7, Item2 = 9, Item3 = 11, Item4 = 13, Item5 = 15;
    const uint Gutter = 20;               // the divider, from the headings to the right column's last item
    const uint Point = 18;

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "What compiles" },
            new TextElement { X = Left, Y = Heading, Text = "WORKS", Color = Works },
            new Rule { X = Left, Y = Underline, Length = Width, Color = Works },
            new TextElement { X = Right, Y = Heading, Text = "COMPILE ERROR", Color = Theme.Bad },
            new Rule { X = Right, Y = Underline, Length = Width, Color = Theme.Bad },
            new Rule { X = Gutter, Y = Heading, Length = Item4 - Heading + 1, Vertical = true },

            new TextElement { X = Left, Y = Item1, Text = "classes, virtuals", Wait = true },
            new TextElement { X = Left, Y = Item2, Text = "arrays, strings" },
            new TextElement { X = Left, Y = Item3, Text = "string interpolation" },
            new TextElement { X = Left, Y = Item4, Text = "float, Func<T>" },
            new TextElement { X = Left, Y = Item5, Text = "GC.Collect()" },

            new TextElement { X = Right, Y = Item1, Text = "generics", Wait = true },
            new TextElement { X = Right, Y = Item2, Text = "interfaces" },
            new TextElement { X = Right, Y = Item3, Text = "struct, double" },
            new TextElement { X = Right, Y = Item4, Text = "try, throw, LINQ" },

            new TextElement { X = Theme.Left, Y = Point, Text = "Every error has a code and a hint", Wait = true },
        ]
    };
}