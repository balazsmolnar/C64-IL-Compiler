using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 13 (DECK.md D5): a reference is an index into the object table,
// whose heap pointer leads to the object on the heap. The table header,
// then its rows, then the heap strip with the pointers, then the text.
static class ReferencesSlide
{
    const uint TableRow = 5, HeapRow = 12, TextRow = 16;
    const uint PointerColumn = 8;      // the heap ptr column's first cell; the heap strip starts there too

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "References: 1 byte" },
            new Table
            {
                Y = TableRow, Widths = [6, 8, 5, 5, 5, 7], WaitAfterHeader = true,
                Header = ["handle", "heap ptr", "size", "refs", "roots", "vtable"],
                Rows = [["1", "$4000", "5", "1", "1", "Node"], ["2", "$4008", "4", "0", "0", "Text"]],
            },
            // Row 1's pointer leaves its value to the left and drops down the
            // column's empty left cell; row 2's drops from under its value.
            new Arrow { FromCol = PointerColumn + 1, FromRow = TableRow + 2, ToCol = PointerColumn, ToRow = HeapRow - 1, Wait = true },
            new Arrow { FromCol = PointerColumn + 7, FromRow = TableRow + 5, ToCol = PointerColumn + 7, ToRow = HeapRow - 1 },
            Heap(PointerColumn, 3, "1"), Heap(PointerColumn + 3, 3, "3"), Heap(PointerColumn + 6, 4, "2"), Heap(PointerColumn + 10, 3, "5"),
            new Box { X = PointerColumn + 13, Y = HeapRow, Width = Theme.Right - PointerColumn - 13, Height = 3, Text = "free", Color = Theme.Frame, Hollow = true },
            Line(TextRow, "A reference is a 1-byte handle", wait: true),
            new TextElement { X = Theme.RightColumn, Y = TextRow, Text = "255 objects; handle 0 is null" },
            Line(TextRow + 2, $"Handle {Theme.Arrow} table {Theme.Arrow} heap address"),
            Line(TextRow + 4, $"Full: {Theme.Red}?OUT OF MEMORY ERROR"),
        ]
    };

    static Box Heap(uint x, uint width, string text)
        => new() { X = x, Y = HeapRow, Width = width, Height = 3, Text = text, ConnectToLeft = x != PointerColumn };

    static TextElement Line(uint row, string text, bool wait = false)
        => new() { X = Theme.Left, Y = row, Text = text, Wait = wait };
}
