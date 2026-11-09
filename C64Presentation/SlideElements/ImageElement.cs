using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// A picture made by tools/c64img.py --mode hires (two colours per cell;
// declare it with [assembly: RawImage(Resource = "chip.img", Label = "Chip")])
// with its top-left cell at (Column, Row), and an optional caption
// centred one row under it (Width, Height = its size in cells).
//
//     new ImageElement { Column = 2, Row = 6, Width = 16, Height = 8,
//                        Image = C64Address.FromLabel("img_Chip"), Caption = "MOS 6510" }
//
// An image keeps its own colours, so in a build step it appears at once
// rather than fading in. Full-bleed multicolour pictures: Master.Picture.
class ImageElement : SlideElement
{
    public uint Column { get; init; }
    public uint Row { get; init; }
    public uint Width { get; init; }
    public uint Height { get; init; }
    public ulong Image { get; init; }
    public string Caption { get; init; }
    public bool Wait { get; init; }

    public override void Draw()
    {
        if (Wait)
            Deck.Step();
        Deck.Show();
        C64.Screen.DrawImage(Column, Row, Image);
        if (Caption != null)
            Text.Center(Helper.Screen.X(Column), Helper.Screen.X(Width), Row + Height + 1, Caption, Theme.Muted);
    }
}
