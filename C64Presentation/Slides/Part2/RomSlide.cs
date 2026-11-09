using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 16 (DECK.md D7): the memory map, the bitmap and the sprites moving
// in under the ROM and the I/O chips; the facts beside their segments.
static class RomSlide
{
    const uint MapColumn = 8, MapTop = Theme.BodyTop, FactColumn = 19;

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Hide under the ROM" },
            new MemoryMap
            {
                X = MapColumn, Y = MapTop + 5, Note = "not to scale",
                Segments =
                [
                    new() { Name = "colours", Address = "$CC00", Height = 2, Color = Theme.Accent },
                    new() { Name = "program", Address = "$1000", Height = 3, Color = Theme.Body },
                    new() { Name = "tables", Address = "$0800", Height = 2, Color = Theme.Muted },
                    new() { Name = "zero page", Address = "$0000", Height = 2, Color = Theme.Muted },
                ],
            },
            Map(MapTop, new() { Name = "bitmap", Address = "$E000", Height = 3, Color = Theme.Accent2 }),
            Fact(MapTop, "Bitmap under the"),
            Fact(MapTop + 2, "KERNAL ROM"),
            Map(MapTop + 3, new() { Name = "I/O", Address = "$D000", Height = 2, Color = Theme.Muted }),
            Fact(MapTop + 4, "Sprites under I/O"),
            new TextElement { X = FactColumn, Y = 11, Text = "Frees 9.4 KB", Face = TextFace.Heading, Color = Theme.Accent, Wait = true },
            Fact(15, "Pictures packed:", wait: true),
            Fact(17, $"8,400 {Theme.Arrow} 2,118 B"),
        ]
    };

    // One segment on top of the map, as a build step.
    static MemoryMap Map(uint row, Segment segment) => new() { X = MapColumn, Y = row, Segments = [segment], Wait = true };

    static TextElement Fact(uint row, string text, bool wait = false) => new() { X = FactColumn, Y = row, Text = text, Wait = wait };
}
