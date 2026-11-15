using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 5: the toolchain as a snake on a three-slot grid. Top row, left to
// right: C#, IL, under a violet "Roslyn" bracket (.NET's half); the
// compiler's arrow runs over the empty third slot and turns down, under a
// cyan "ours" bracket. Bottom row, right to left: .asm, .prg (cyan); under
// .prg, two short arrows from its edges to where it runs (yellow).
static class PipelineSlide
{
    const uint Slot1 = 2, Slot2 = 17, Slot3 = 31;    // box columns
    const uint Slot1End = Slot1 + SlotWidth, Slot2End = Slot2 + SlotWidth;
    const uint SlotWidth = 7, BoxHeight = 3;
    const uint Top = 6, Bottom = 12, Targets = 17;   // box rows
    const uint TopArrow = Top + 1, BottomArrow = Bottom + 1;
    const uint Turn = Slot3 + 3;                     // the compiler arrow's way down
    const uint Brackets = Top + BoxHeight, BracketNames = Brackets + 1;
    const uint OursFrom = Slot2End + 1;
    const uint RoslynName = 11, OursName = Turn - 6, Passes = Turn - 7;   // centred under their spans
    const uint Vice = Slot2 - 2, C64 = Slot2End - 3, TargetWidth = 5;     // centred on the two arrows

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Roslyn does half" },
            new Box { X = Slot1, Y = Top, Width = SlotWidth, Height = BoxHeight, Text = "C#", Color = Theme.Brand },

            new Arrow { FromCol = Slot1End, FromRow = TopArrow, ToCol = Slot2 - 1, ToRow = TopArrow, Text = "dotnet build", Wait = true },
            new Box { X = Slot2, Y = Top, Width = SlotWidth, Height = BoxHeight, Text = "IL .dll", Color = Theme.Brand },
            new Rule { X = Slot1, Y = Brackets, Length = Slot2End - Slot1, Color = Theme.Brand },
            new TextElement { X = RoslynName, Y = BracketNames, Text = "Roslyn", Color = Theme.Brand },

            new Arrow { FromCol = Slot2End, FromRow = TopArrow, ToCol = Turn, ToRow = Bottom - 1, Text = "Compiler.exe", Wait = true },
            new TextElement { X = Passes, Y = Top + 2, Text = "46 passes", Color = Theme.Accent },
            new Rule { X = OursFrom, Y = Brackets, Length = Turn - OursFrom, Color = Theme.Accent },
            new TextElement { X = OursName, Y = BracketNames, Text = "ours", Color = Theme.Accent },
            new Box { X = Slot3, Y = Bottom, Width = SlotWidth, Height = BoxHeight, Text = ".asm" },

            new Arrow { FromCol = Slot3 - 1, FromRow = BottomArrow, ToCol = Slot2End, ToRow = BottomArrow, Text = "64tass", Wait = true },
            new Box { X = Slot2, Y = Bottom, Width = SlotWidth, Height = BoxHeight, Text = ".prg" },

            new Arrow { FromCol = Slot2, FromRow = Bottom + BoxHeight, ToCol = Slot2, ToRow = Targets - 1, Wait = true },
            new Arrow { FromCol = Slot2End - 1, FromRow = Bottom + BoxHeight, ToCol = Slot2End - 1, ToRow = Targets - 1 },
            new Box { X = Vice, Y = Targets, Width = TargetWidth, Height = BoxHeight, Text = "VICE", Color = Theme.Accent2 },
            new Box { X = C64, Y = Targets, Width = TargetWidth, Height = BoxHeight, Text = "C64", Color = Theme.Accent2 },
        ]
    };
}
