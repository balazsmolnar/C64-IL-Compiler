using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// DECK.md D5: a reference is an index into the object table, whose heap
// pointer leads to the object on the heap. The table header first, then
// its rows, then the heap strip with the pointers, then two lines.
static class MemoryLayoutSlide
{
    const uint TableRow = 5, HeapRow = 14, TextRow = 18;
    const uint PointerColumn = 8;      // the heap ptr column's first cell; the heap strip starts there too

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "References: 1 byte" },
            new Table
            {
                Y = TableRow, Widths = [6, 8, 5, 5, 5, 7], WaitAfterRow = true,
                Header = ["handle", "heap ptr", "size", "refs", "roots", "vtable"],
                Rows = [["1", "$4000", "5", "1", "1", "Node"], ["2", "$4008", "4", "0", "0", "Text"], ["255", "", "-", "-", "-", "-"]],
            },
            // Row 1's pointer leaves its value to the left and drops down the
            // column's empty left cell; row 2's drops from under its value.
            // Both land on their blocks.
            new Arrow { FromCol = PointerColumn + 2, FromRow = TableRow + 2, ToCol = PointerColumn, ToRow = HeapRow - 1, Wait = true },
            new Arrow { FromCol = PointerColumn + 6, FromRow = TableRow + 5, ToCol = PointerColumn + 6, ToRow = HeapRow - 1 },
            Heap(PointerColumn, 2, "1"), Heap(PointerColumn + 2, 3, "3", true), Heap(PointerColumn + 5, 4, "2", true), Heap(PointerColumn + 9, 3, "5", true),
            new Box { X = PointerColumn + 12, Y = HeapRow, Width = Theme.Right - PointerColumn - 12, Height = 3, Text = "free", Color = Theme.Frame, Hollow = true },
            Line(TextRow, "A reference is a 1-byte handle: 255 objects", wait: true),
            Line(TextRow + 2, $"Handle {Theme.Arrow} table {Theme.Arrow} heap address"),
        ]
    };

    static Box Heap(uint x, uint width, string text, bool connect = false)
        => new() { X = x, Y = HeapRow, Width = width, Height = 3, Text = text, ConnectToLeft = connect };

    static TextElement Line(uint row, string text, bool wait = false)
        => new() { X = Theme.Left, Y = row, Text = text, Wait = wait };
}
