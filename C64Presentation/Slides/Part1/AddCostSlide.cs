using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 7: where Add's values live without optimization. Left, the 6502's
// own stack (pha/pla) holds the evaluation stack; the prologue pulls the
// return address and the arguments off it into a frame on localsStack,
// which Y indexes. Then the cost of all that: 39 bytes.
static class AddCostSlide
{
    const uint Header = 5;                // the stacks' names
    const uint Push = 6;                  // "pha/pla" and its arrow into the stack
    const uint Cell1 = 8, Cell2 = 10, Cell3 = 12;   // the stacks' cells, two rows each (a blank row before the lines)
    const uint Hardware = Theme.Left;     // the 6502 stack's column
    const uint Locals = 22;               // localsStack's column
    const uint Pointer = 30;              // the Y register box
    const uint StackWidth = 6, PointerWidth = 7;
    const uint Into = Hardware + StackWidth - 1;    // the push arrow's column
    const uint Transfer = Cell2 + 1;      // the prologue arrow's row
    const uint Line1 = 16, Line2 = 18, Line3 = 20;

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Add costs 39 bytes" },
            new TextElement { X = Hardware, Y = Header, Text = "6502 stack, $0100" },
            new TextElement { X = Hardware, Y = Push, Text = "pha/pla", Color = Theme.Muted },
            new Arrow { FromCol = Into, FromRow = Push, ToCol = Into, ToRow = Cell1 - 1 },
            new Box { X = Hardware, Y = Cell1, Width = StackWidth, Height = 2, Text = "ret addr", Color = Theme.Muted },
            new Box { X = Hardware, Y = Cell2, Width = StackWidth, Height = 2, Text = "b", Color = Theme.Muted },
            new Box { X = Hardware, Y = Cell3, Width = StackWidth, Height = 2, Text = "a", Color = Theme.Muted },
            new TextElement { X = Theme.Left, Y = Line1, Text = "6502 stack: the evaluation stack" },

            new Arrow { FromCol = Hardware + StackWidth, FromRow = Transfer, ToCol = Locals - 2, ToRow = Transfer, Text = "pla, sta localsStack,y", Wait = true },
            new TextElement { X = Locals, Y = Header, Text = "localsStack, 256 B" },
            new Box { X = Locals, Y = Cell1, Width = StackWidth, Height = 2, Text = "ret addr" },
            new Box { X = Locals, Y = Cell2, Width = StackWidth, Height = 2, Text = "a" },
            new Box { X = Locals, Y = Cell3, Width = StackWidth, Height = 2, Text = "b" },
            new Box { X = Pointer, Y = Cell1, Width = PointerWidth, Height = 3, Text = "Y", Color = Theme.Accent2 },
            new Arrow { FromCol = Pointer - 1, FromRow = Cell1 + 1, ToCol = Pointer - 2, ToRow = Cell1 + 1 },
            new TextElement { X = Pointer, Y = Cell3, Text = "stackPointer", Color = Theme.Muted },
            new TextElement { X = Theme.Left, Y = Line2, Text = "localsStack: arguments and locals" },

            new TextElement { X = Theme.Left, Y = Line3, Text = "One line of C#: 39 bytes of 6502", Color = Theme.Accent, Wait = true },
        ]
    };
}
