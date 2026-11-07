using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// The object table (one row per handle) and the heap it points into.
static class MemoryLayoutSlide
{
    const uint HeapRow = 18;

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Memory layout" },
            Label(5, "Object table"),
            Cell(2, 6, 4, "1", false), Cell(6, 6, 9, "Heap ptr"), Cell(15, 6, 6, "Size"),
            Cell(21, 6, 6, "Refs"), Cell(27, 6, 5, "Root"), Cell(32, 6, 6, "VTable"),
            Cell(2, 9, 4, "2", false), Cell(6, 9, 9), Cell(15, 9, 6), Cell(21, 9, 6), Cell(27, 9, 5), Cell(32, 9, 6),
            new TextElement { X = 3, Y = 12, Text = "...", Color = Theme.Muted },
            Cell(2, 13, 4, "255", false), Cell(6, 13, 9), Cell(15, 13, 6), Cell(21, 13, 6), Cell(27, 13, 5), Cell(32, 13, 6),
            Label(17, "Heap", wait: true),
            Heap(2, 5, "1", false), Heap(7, 8, "3"), Heap(15, 5, "2"), Heap(20, 3, "5"),
            new Box { X = 23, Y = HeapRow, Width = 15, Height = 3, Text = "free", Color = Theme.Card, Hollow = true },
        ]
    };

    static TextElement Label(uint row, string text, bool wait = false)
        => new() { X = Theme.Left, Y = row, Text = text, Color = Theme.Muted, Wait = wait };

    static Box Cell(uint x, uint y, uint width, string text = "", bool connect = true)
        => new() { X = x, Y = y, Width = width, Height = 3, Text = text, ConnectToLeft = connect };

    static Box Heap(uint x, uint width, string text, bool connect = true)
        => new() { X = x, Y = HeapRow, Width = width, Height = 3, Text = text, Color = Theme.Accent2, ConnectToLeft = connect };
}
