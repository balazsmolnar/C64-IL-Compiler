using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 13 (DECK.md D5): a reference is an index into the object table,
// whose heap pointer leads to the object on the heap. The table header,
// then its rows, then the heap strip with the pointers, then the text and
// a stat card.
static class ReferencesSlide
{
    const uint TableRow = Theme.BodyTop, FirstRow = TableRow + 2, SecondRow = TableRow + 4;
    const uint HeapRow = 12, TextRow = 16, CardColumn = 25, CardWidth = Theme.Right - CardColumn;
    const uint PointerColumn = 8;      // the heap ptr column's first cell, empty; the heap strip starts there too
    const uint ValueStart = 10;        // a cell clear of the left of a heap ptr value
    const uint ValueEnd = 16;          // the first cell right of it (the size column's empty left)

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
            // column's empty first cell; row 2's drops from its right end. In
            // the hairlines' grey, so a crossing keeps the hairline's colour.
            new Arrow { FromCol = ValueStart, FromRow = FirstRow, ToCol = PointerColumn, ToRow = HeapRow - 1, Color = Theme.Muted, Wait = true },
            new Arrow { FromCol = ValueEnd, FromRow = SecondRow, ToCol = ValueEnd, ToRow = HeapRow - 1, Color = Theme.Muted },
            Heap(PointerColumn, 3, "1"), Heap(PointerColumn + 3, 3, "3"), Heap(PointerColumn + 6, 4, "2"), Heap(PointerColumn + 10, 3, "5"),
            new Box { X = PointerColumn + 13, Y = HeapRow, Width = Theme.Right - PointerColumn - 13, Height = 3, Text = "free", Color = Theme.Frame, Hollow = true },
            Line(TextRow, "A reference is a 1-byte handle", wait: true),
            Line(TextRow + 2, $"Handle {Theme.Arrow} table {Theme.Arrow} heap address"),
            Line(TextRow + 4, $"Full: {Theme.Red}?OUT OF MEMORY ERROR"),
            new Box { X = CardColumn, Y = TextRow, Width = CardWidth, Height = 5, Text = "255 objects", Caption = "handle 0 is null" },
        ]
    };

    static Box Heap(uint x, uint width, string text)
        => new() { X = x, Y = HeapRow, Width = width, Height = 3, Text = text, ConnectToLeft = x != PointerColumn };

    static TextElement Line(uint row, string text, bool wait = false)
        => new() { X = Theme.Left, Y = row, Text = text, Wait = wait };
}
