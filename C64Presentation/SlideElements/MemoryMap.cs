using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// A vertical memory map: Segments stacked from the top (high addresses
// first), each a Grey1 block of Height rows (1 or more) with a 3 px stripe
// in its Color down the left edge (its divider from the next), its name (and Detail, from Height 3) in
// white, its start address right-aligned to the left of it in Grey2, and a
// Note under the map ("not to scale").
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
            Text.Right(labelRight, row, segment.Address, Theme.Muted);
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
        Text.Draw(x + 8, row + (Height - 1) / 2, Name, Theme.Heading, Theme.Card);
        if (Detail != null && Height > 2)
            Text.Draw(x + 8, row + (Height - 1) / 2 + 1, Detail, Theme.Body, Theme.Card);
    }
}
