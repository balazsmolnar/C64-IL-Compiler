using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// The object table (one row per handle) and the heap it points into.
class MemoryLayoutSlide
{
    public static Slide Create()
    {
        return new SimpleSlide()
        {
            Elements = new SlideElement[]
            {
                new Title() {Text = "Memory layout"},

                new TextElement() { X = 2, Y = 5, Text = "Object table", Color = Theme.Muted },
                Cell(2, 6, 4, "1", false), Cell(6, 6, 9, "Heap ptr", true), Cell(15, 6, 6, "Size", true),
                Cell(21, 6, 6, "Refs", true), Cell(27, 6, 5, "Root", true), Cell(32, 6, 6, "VTable", true),
                Cell(2, 9, 4, "2", false), Cell(6, 9, 9, "", true), Cell(15, 9, 6, "", true),
                Cell(21, 9, 6, "", true), Cell(27, 9, 5, "", true), Cell(32, 9, 6, "", true),
                new TextElement() { X = 3, Y = 12, Text = "...", Color = Theme.Muted },
                Cell(2, 13, 4, "255", false), Cell(6, 13, 9, "", true), Cell(15, 13, 6, "", true),
                Cell(21, 13, 6, "", true), Cell(27, 13, 5, "", true), Cell(32, 13, 6, "", true),

                new TextElement() { X = 2, Y = 17, Text = "Heap", Color = Theme.Muted, Wait = true },
                Heap(2, 5, "1", false), Heap(7, 8, "3", true), Heap(15, 5, "2", true), Heap(20, 3, "5", true),
                new Box() { X = 23, Y = 18, Width = 15, Height = 3, Text = "free", TextColor = Theme.Muted, BorderColor = Theme.Card, Hollow = true },
            }
        };
    }

    static Box Cell(uint x, uint y, uint w, string text, bool connect)
        => new Box() { X = x, Y = y, Width = w, Height = 3, Text = text, BorderColor = Theme.Accent, ConnectToLeft = connect };

    static Box Heap(uint x, uint w, string text, bool connect)
        => new Box() { X = x, Y = 18, Width = w, Height = 3, Text = text, BorderColor = Theme.Accent2, ConnectToLeft = connect };
}
