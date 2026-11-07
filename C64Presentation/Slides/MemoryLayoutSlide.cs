using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// DECK.md D5: a reference is an index into the object table, whose heap
// pointer leads to the object on the heap. The table header first, then
// its rows, then the heap strip with the pointers, then two lines.
static class MemoryLayoutSlide
{
    const uint TableRow = 4, HeapRow = 14, LinksRow = 12, TextRow = 18;

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "References: 1 byte" },
            new Table
            {
                Y = TableRow, Widths = [6, 8, 5, 5, 5, 7], WaitAfterRow = true,
                Header = ["handle", "heap ptr", "size", "refs", "roots", "vtable"],
                Rows = [["1", "", "", "", "", ""], ["2", "", "", "", "", ""], ["255", "", "", "", "", ""]],
            },
            Link(10, 10, wait: true),
            Link(13, 17),
            Heap(8, 5, "1"), Heap(13, 3, "3", true), Heap(16, 4, "2", true), Heap(20, 2, "5", true),
            new Box { X = 22, Y = HeapRow, Width = 16, Height = 3, Text = "free", Color = Theme.Frame, Hollow = true },
            Line(TextRow, "A reference is a 1-byte handle: 255 objects", wait: true),
            Line(TextRow + 2, $"Handle {Theme.Arrow} table {Theme.Arrow} heap address"),
        ]
    };

    // From the heap-pointer column, under the table, down into a heap block.
    static Arrow Link(uint fromColumn, uint toColumn, bool wait = false)
        => new() { FromCol = fromColumn, FromRow = LinksRow, ToCol = toColumn, ToRow = HeapRow - 1, Wait = wait };

    static Box Heap(uint x, uint width, string text, bool connect = false)
        => new() { X = x, Y = HeapRow, Width = width, Height = 3, Text = text, ConnectToLeft = connect };

    static TextElement Line(uint row, string text, bool wait = false)
        => new() { X = Theme.Left, Y = row, Text = text, Wait = wait };
}
