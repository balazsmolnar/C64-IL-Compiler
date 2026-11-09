using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// A vertical memory map: Segments stacked from the top (high addresses
// first), each a Grey1 block of Height rows (2 or more) with a 3 px stripe
// in its Color down the left edge and a 1 px black line along its bottom
// (the boundary with the next), its name in white (and Detail under it in
// Grey3, from Height 3), and its start address right-aligned to the left
// on its bottom row, where that boundary is, in Grey2 with a tick to it.
// A Note under the map ("not to scale").
// Wait: one segment per build step, from the top.
//
//     new MemoryMap
//     {
//         X = 8, Y = 4, Width = 8, Note = "not to scale",
//         Segments =
//         [
//             new() { Name = "bitmap", Address = "$E000", Height = 4, Color = Theme.Accent2 },
//             new() { Name = "I/O", Address = "$D000", Height = 2, Color = Theme.Brand },
//         ],
//     }
sealed class MemoryMap : SlideElement
{
    public uint X { get; init; }
    public uint Y { get; init; }
    public uint Width { get; init; }
    public required Segment[] Segments { get; init; }
    public string? Note { get; init; }
    public bool Wait { get; init; }

    public override void Draw()
    {
        uint row = Theme.OrCells(Y, Theme.BodyTop);
        uint width = Theme.OrCells(Width, 8);
        ulong labelRight = Screen.Px(X) - 5;
        foreach (var segment in Segments)
        {
            if (Wait)
                Deck.Step();
            segment.Draw(X, row, width);
            uint bottom = row + segment.Height - 1;
            Text.Right(labelRight, bottom, segment.Address, Theme.Muted);
            ulong y = Screen.Px(bottom) + 7;
            Screen.Line(labelRight + 2, y, Screen.Px(X) - 1, y);
            row += segment.Height;
        }
        if (Note != null)
            Text.Center(Screen.Px(X), Screen.Px(width), row + 1, Note, Theme.Muted);
    }
}

// One block of a MemoryMap. (A class, not a record struct: the compiler
// has no structs or records.)
sealed class Segment
{
    public required string Name { get; init; }
    public required string Address { get; init; }
    public string? Detail { get; init; }       // a second line, for Height 4 or more
    public uint Height { get; init; }
    public Colors Color { get; init; }

    public void Draw(uint column, uint row, uint width)
    {
        Deck.Paint(column, row, width, Height, Theme.Background, Theme.Card);
        Deck.Paint(column, row, 1, Height, Theme.Or(Color, Theme.Accent), Theme.Card);
        ulong x = Screen.Px(column), top = Screen.Px(row), bottom = Screen.Px(row + Height) - 1;
        Screen.Fill(x, top, x + 2, bottom - 1);
        Screen.Line(x + 8, bottom, x + Screen.Px(width) - 1, bottom);   // black: the cells' ink
        // Name and Detail centred as a pair, so the Detail never sits on
        // the segment's last row against the next segment's name.
        bool detail = Detail != null && Height > 2;
        uint nameRow = row + (Height - (detail ? 2u : 1u)) / 2;
        Text.Draw(x + 8, nameRow, Name, Theme.Heading, Theme.Card);
        if (detail)
            Text.Draw(x + 8, nameRow + 1, Detail!, Theme.Body, Theme.Card);
    }
}
