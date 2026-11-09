using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 5: the toolchain as a snake on a three-slot grid. Top row, left to
// right: C#, IL (Roslyn's half, violet: .NET); the compiler's arrow runs
// over the empty third slot and turns down. Bottom row, right to left:
// .asm, .prg (our half, cyan); under .prg, where it runs (yellow).
static class PipelineSlide
{
    const uint Slot1 = 2, Slot2 = 16, Slot3 = 31;    // box columns, 7 wide
    const uint SlotWidth = 7;
    const uint Top = 6, Bottom = 12, Targets = 17;   // box rows, 3 high
    const uint BoxHeight = 3;

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Roslyn does half" },
            Stage(Slot1, Top, "C#", Theme.Brand),
            Flow(Slot1 + SlotWidth, Slot2 - 1, Top, "dotnet build"),
            Stage(Slot2, Top, "IL .dll", Theme.Brand),
            new Arrow { FromCol = Slot2 + SlotWidth, FromRow = Top + 1, ToCol = Slot3 + 3, ToRow = Bottom - 1, Text = "Compiler.exe", Wait = true },
            new TextElement { X = 26, Y = Top + 2, Text = "46 passes", Color = Theme.Accent },
            Stage(Slot3, Bottom, ".asm", Theme.Accent),
            Flow(Slot3 - 1, Slot2 + SlotWidth, Bottom, "64tass"),
            Stage(Slot2, Bottom, ".prg", Theme.Accent),
            new Arrow { FromCol = Slot2, FromRow = Bottom + BoxHeight, ToCol = Slot2, ToRow = Targets - 1, Wait = true },
            new Arrow { FromCol = Slot2 + SlotWidth - 1, FromRow = Bottom + BoxHeight, ToCol = Slot2 + SlotWidth - 1, ToRow = Targets - 1 },
            Stage(Slot2 - 2, Targets, "VICE", Theme.Accent2, width: 5),
            Stage(Slot2 + SlotWidth - 3, Targets, "C64", Theme.Accent2, width: 5),
        ]
    };

    static Box Stage(uint x, uint y, string name, Colors color, uint width = SlotWidth)
        => new() { X = x, Y = y, Width = width, Height = BoxHeight, Text = name, Color = color };

    // A labelled arrow along a row of boxes, the next build step.
    static Arrow Flow(uint from, uint to, uint row, string tool)
        => new() { FromCol = from, FromRow = row + 1, ToCol = to, ToRow = row + 1, Text = tool, Wait = true };
}
