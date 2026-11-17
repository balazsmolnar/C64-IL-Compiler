using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// Recolours a rectangle of cells over what is drawn there: a highlighted
// code line (black on yellow), a coloured chip or tag. Height: Auto = 1 row.
//
//     new Highlight { X = 4, Y = 11, Width = 13, Ink = Colors.Black, Paper = Theme.Accent2, Wait = true }
sealed class Highlight : SlideElement
{
    public uint X { get; init; }
    public uint Y { get; init; }
    public uint Width { get; init; }
    public uint Height { get; init; }
    public Colors Ink { get; init; }
    public Colors Paper { get; init; }
    public bool Wait { get; init; }

    public override void Draw()
    {
        if (Wait)
            Deck.Step();
        Deck.Paint(X, Y, Width, Theme.OrCells(Height, 1), Ink, Paper);
    }
}
