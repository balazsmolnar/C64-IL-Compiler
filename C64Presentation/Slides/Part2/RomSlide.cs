using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 16 (DECK.md D7): the memory map, the bitmap and the sprites moving
// in under the ROM and the I/O chips; each fact level with its segment.
static class RomSlide
{
    const uint MapColumn = 8, BitmapRow = Theme.BodyTop, IoRow = BitmapRow + 3, RestRow = IoRow + 2;
    const uint FactColumn = 19, SavingRow = 11, PictureRow = 15;

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Hide under the ROM" },
            new MemoryMap
            {
                X = MapColumn, Y = RestRow, Note = "not to scale",
                Segments =
                [
                    new() { Name = "colours", Address = "$CC00", Height = 2, Color = Theme.Accent },
                    new() { Name = "program", Address = "$1000", Height = 3, Color = Theme.Body },
                    new() { Name = "tables", Address = "$0800", Height = 2, Color = Theme.Muted },
                    new() { Name = "zero page", Address = "$0000", Height = 2, Color = Theme.Muted },
                ],
            },
            Map(BitmapRow, new() { Name = "bitmap", Address = "$E000", Height = 3, Color = Theme.Accent2 }),
            Fact(BitmapRow + 1, "Bitmap under the KERNAL ROM"),
            Map(IoRow, new() { Name = "I/O", Address = "$D000", Height = 2, Color = Theme.Muted }),
            Fact(IoRow + 1, "Sprites under I/O"),
            new TextElement { X = FactColumn, Y = SavingRow, Text = "Frees 9.4 KB", Face = TextFace.Heading, Color = Theme.Accent, Wait = true },
            Fact(PictureRow, $"Pictures: 8,400 {Theme.Arrow} 2,118 B", wait: true),
        ]
    };

    // One segment on top of the map, as a build step.
    static MemoryMap Map(uint row, Segment segment) => new() { X = MapColumn, Y = row, Segments = [segment], Wait = true };

    static TextElement Fact(uint row, string text, bool wait = false) => new() { X = FactColumn, Y = row, Text = text, Wait = wait };
}
