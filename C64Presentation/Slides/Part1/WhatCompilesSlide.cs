using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 4: what compiles and what is a compile error, as two columns under
// green and red headings, a rule between them.
static class WhatCompilesSlide
{
    const Colors Works = Colors.LightGreen;   // a test-result green, this slide only
    const uint Left = Theme.LeftColumn, Right = Theme.RightColumn;
    const uint Heading = 5;

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "What compiles" },
            Head(Left, "WORKS", Works),
            Underline(Left, Works),
            Head(Right, "COMPILE ERROR", Theme.Bad),
            Underline(Right, Theme.Bad),
            new Rule { X = 20, Y = Heading, Length = 11, Vertical = true },

            Item(Left, 0, "classes, virtuals", wait: true),
            Item(Left, 1, "arrays, strings"),
            Item(Left, 2, "string interpolation"),
            Item(Left, 3, "float, Func<T>"),
            Item(Left, 4, "GC.Collect()"),

            Item(Right, 0, "generics", wait: true),
            Item(Right, 1, "interfaces"),
            Item(Right, 2, "struct, double"),
            Item(Right, 3, "try, throw, LINQ"),

            new TextElement { X = Theme.Left, Y = 18, Text = "Every error has a code and a hint", Wait = true },
        ]
    };

    static TextElement Head(uint column, string text, Colors color) => new() { X = column, Y = Heading, Text = text, Color = color };

    static Rule Underline(uint column, Colors color) => new() { X = column, Y = Heading + 1, Length = Theme.ColumnWidth, Color = color };

    // Item n of a column, every other row from row 7.
    static TextElement Item(uint column, uint n, string text, bool wait = false)
        => new() { X = column, Y = Heading + 2 + n + n, Text = text, Wait = wait };
}