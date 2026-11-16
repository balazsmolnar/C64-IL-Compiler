using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 10 (DECK.md D5): a reference is an index into the object table,
// whose heap pointer leads to the object on the heap. The table header,
// then its rows, then the heap strip with the pointers, then the text and
// a stat card.
static class ReferencesSlide
{
    const uint TableRow = Theme.BodyTop, FirstRow = TableRow + 2, SecondRow = TableRow + 4;
    const uint HeapRow = 12, HeapHeight = 3, TextRow = 16, CardColumn = 25, CardWidth = Theme.Right - CardColumn;
    const uint PointerColumn = 8;      // the heap ptr column's first cell, empty; the heap strip starts there too
    const uint ValueStart = 10;        // a cell clear of the left of a heap ptr value
    const uint ValueEnd = 16;          // the first cell right of it (the size column's empty left)

    // The heap by address: objects 1, 3, 2, 5 with the sizes in the table
    // ($4008 = $4000 + 5 + 3), each block starting where the one before ends.
    const uint Size1 = 5, Size3 = 3, Size2 = 4, Size5 = 3;
    const uint Block1 = PointerColumn, Block3 = Block1 + Size1, Block2 = Block3 + Size3, Block5 = Block2 + Size2, Free = Block5 + Size5;

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
            // column's empty first cell into block 1; row 2's drops from the
            // right end of its value into block 2. In the hairlines' grey, so
            // a crossing keeps the hairline's colour.
            new Arrow { FromCol = ValueStart, FromRow = FirstRow, ToCol = PointerColumn, ToRow = HeapRow - 1, Color = Theme.Muted, Wait = true },
            new Arrow { FromCol = ValueEnd, FromRow = SecondRow, ToCol = ValueEnd, ToRow = HeapRow - 1, Color = Theme.Muted },
            new TextElement { X = Theme.Left, Y = HeapRow + 1, Text = "heap", Color = Theme.Muted },
            new Box { X = Block1, Y = HeapRow, Width = Size1, Height = HeapHeight, Text = "1" },
            new Box { X = Block3, Y = HeapRow, Width = Size3, Height = HeapHeight, Text = "3", ConnectToLeft = true },
            new Box { X = Block2, Y = HeapRow, Width = Size2, Height = HeapHeight, Text = "2", ConnectToLeft = true },
            new Box { X = Block5, Y = HeapRow, Width = Size5, Height = HeapHeight, Text = "5", ConnectToLeft = true },
            new Box { X = Free, Y = HeapRow, Width = Theme.Right - Free, Height = HeapHeight, Text = "free", Color = Theme.Frame, Hollow = true },
            new TextElement { X = Theme.Left, Y = TextRow, Text = "A reference is a 1-byte handle", Wait = true },
            new TextElement { X = Theme.Left, Y = TextRow + 2, Text = $"Handle {Theme.Arrow} table {Theme.Arrow} heap address" },
            new TextElement { X = Theme.Left, Y = TextRow + 4, Text = $"Full: {Theme.Red}?OUT OF MEMORY ERROR" },
            new Box { X = CardColumn, Y = TextRow, Width = CardWidth, Height = 5, Text = "255 objects", Caption = "handle 0 is null" },
        ]
    };
}
