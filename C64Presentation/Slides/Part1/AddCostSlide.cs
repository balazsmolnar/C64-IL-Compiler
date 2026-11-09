using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 7: where Add's values live without optimization. Left, the 6502's
// own stack (pha / pla) holds the evaluation stack; the prologue pulls the
// return address and the arguments off it into a frame on localsStack,
// which Y indexes. Then the cost of all that: 39 bytes.
static class AddCostSlide
{
    const uint Hardware = Theme.Left;     // the 6502 stack's column
    const uint Locals = 22;               // localsStack's column
    const uint StackWidth = 6;
    const uint StackTop = 7;              // three cells, two rows each
    const uint Middle = StackTop + 3;     // the transfer arrow's row
    const uint Pointer = 30;              // the Y register box
    const uint Caption = 14;

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Add costs 39 bytes" },
            new Arrow { FromCol = Hardware + 3, FromRow = StackTop - 2, ToCol = Hardware + 3, ToRow = StackTop - 1, Text = "pha / pla" },
            Cell(Hardware, 0, "ret addr", Theme.Muted),
            Cell(Hardware, 1, "b", Theme.Muted),
            Cell(Hardware, 2, "a", Theme.Muted),
            new TextElement { X = Hardware, Y = Caption, Text = "6502 stack, $0100", Color = Theme.Muted },
            Line(16, "6502 stack: the evaluation stack", wait: false),

            new Arrow { FromCol = Hardware + StackWidth, FromRow = Middle, ToCol = Locals - 1, ToRow = Middle, Text = "pla, sta localsStack,y", Wait = true },
            Cell(Locals, 0, "ret addr", Theme.Accent),
            Cell(Locals, 1, "a", Theme.Accent),
            Cell(Locals, 2, "b", Theme.Accent),
            new Box { X = Pointer, Y = StackTop, Width = StackWidth, Height = 3, Text = "Y", Color = Theme.Accent2 },
            new Arrow { FromCol = Pointer - 1, FromRow = StackTop + 1, ToCol = Pointer - 2, ToRow = StackTop + 1 },
            new TextElement { X = Pointer, Y = StackTop + 4, Text = "stackPointer", Color = Theme.Muted },
            new TextElement { X = Locals, Y = Caption, Text = "localsStack, 256 B", Color = Theme.Muted },
            Line(18, "localsStack: arguments and locals", wait: false),

            new TextElement { X = Theme.Left, Y = 20, Text = "One line of C#: 39 bytes of 6502", Color = Theme.Accent, Wait = true },
        ]
    };

    // Cell n of a stack, top down, two rows each.
    static Box Cell(uint column, uint n, string text, Colors color)
        => new() { X = column, Y = StackTop + n + n, Width = StackWidth, Height = 2, Text = text, Color = color };

    static TextElement Line(uint row, string text, bool wait) => new() { X = Theme.Left, Y = row, Text = text, Wait = wait };
}
