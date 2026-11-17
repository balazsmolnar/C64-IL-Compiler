using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides.Part3;

// Slide 16 (DECK.md): a mock of the VS Code debugger, stopped in
// Demo/RotatingCube.cs. The breakpoint dot sits in the editor's gutter, a
// cell left free before the code.
static class DebuggerMock
{
    const uint EditorWidth = 19;
    const uint TabRow = 6;                  // the editor's caption row
    const uint StopRow = 11;                // the fourth code line
    const uint ChipColumn = Theme.Left + EditorWidth - 5;
    const uint Features = Theme.RightColumn + 1;

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Debug C# on a C64" },
            new CodeBlock
            {
                X = Theme.Left, Y = 5, Width = EditorWidth, Caption = "RotatingCube.cs", Indent = 1,
                Lines =
                [
                    $"{Theme.Kw}public void{Theme.Pl}Step()",
                    "{",
                    "  BuildBasis();",
                    $"{Theme.Kw}  float{Theme.Pl}bcx = cx_;",
                ],
            },
            new TextElement { X = ChipColumn, Y = TabRow, Text = " VICE", Color = Theme.Heading },
            new Highlight { X = ChipColumn, Y = TabRow, Width = 4, Ink = Theme.Heading, Paper = Colors.LightBlue },
            new TextElement { X = Theme.Left + 1, Y = StopRow, Text = Theme.Dot, Color = Theme.Bad, Wait = true },
            new Highlight { X = Theme.Left + 2, Y = StopRow, Width = 15, Ink = Colors.Black, Paper = Theme.Accent2, Wait = true },
            new Elbow { Column = ChipColumn + 3, FromRow = TabRow + 1, ToRow = StopRow, Color = Colors.LightBlue },
            new CodeBlock
            {
                X = Theme.Left, Y = 14, Width = EditorWidth, Wait = true,
                Lines =
                [
                    $"{Theme.Com}VARIABLES",
                    $"bcx     {Theme.Com}...",
                    $"cx_     {Theme.Com}...",
                    $"velX_   {Theme.Com}...",
                    $"cosAy_  {Theme.Com}...",
                ],
            },
            new TextElement { X = Features, Y = 5, Text = $"{Theme.Hi}{Theme.Dot}{Theme.Off}Breakpoints on", Wait = true },
            new TextElement { X = Features, Y = 7, Text = "  C# lines" },
            new TextElement { X = Features, Y = 9, Text = $"{Theme.Hi}{Theme.Dot}{Theme.Off}F10, F11, Shift+F11", Wait = true },
            new TextElement { X = Features, Y = 11, Text = $"{Theme.Hi}{Theme.Dot}{Theme.Off}Locals, fields," },
            new TextElement { X = Features, Y = 13, Text = "  arrays, hover", Wait = true },
            new TextElement { X = Features, Y = 15, Text = $"{Theme.Hi}{Theme.Dot}{Theme.Off}Edit a value" },
            new TextElement { X = Features, Y = 17, Text = $"{Theme.Hi}{Theme.Dot}{Theme.Off}Registers and", Wait = true },
            new TextElement { X = Features, Y = 19, Text = "  6502 disassembly" },
        ]
    };
}
