using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// A vertical memory map: segments stacked from the top (high addresses
// first), each a card of Heights[i] rows labelled Names[i] with its accent
// bar in Colors[i] (default cyan), its start address right-aligned to the
// left of it in Grey2, and an optional Note under the map ("not to scale").
// Wait: one segment per build step, from the top.
//
//     new MemoryMap { X = 8, Y = 5, Width = 10,
//                     Names = ["Bitmap", "I/O + sprites", "Colours", "Program + heap"],
//                     Addresses = ["$E000", "$D000", "$CC00", "$1000"],
//                     Heights = [3, 3, 2, 6], Note = "not to scale" }
class MemoryMap : SlideElement
{
    public uint X { get; init; }
    public uint Y { get; init; }
    public uint Width { get; init; }
    public string[] Names { get; init; }
    public string[] Addresses { get; init; }
    public uint[] Heights { get; init; }
    public Colors[] Colors { get; init; }
    public string Note { get; init; }
    public bool Wait { get; init; }

    public override void Draw()
    {
        uint row = Theme.OrCells(Y, Theme.BodyTop);
        uint width = Theme.OrCells(Width, 8);
        ulong labelRight = Helper.Screen.X(X) - 5;
        for (uint i = 0; i < Names.Length; i++)
        {
            if (Wait)
                Deck.Step();
            uint height = Heights[i];
            var accent = Colors != null ? Colors[i] : Theme.Accent;
            Box.Render(X, row, width, height, Names[i], "", accent, Theme.Card, false);
            Text.Right(labelRight, row + 1, Addresses[i], Theme.Muted);
            row += height;
        }
        if (Note != null)
            Text.Center(Helper.Screen.X(X), Helper.Screen.X(width), row + 1, Note, Theme.Muted);
    }
}
