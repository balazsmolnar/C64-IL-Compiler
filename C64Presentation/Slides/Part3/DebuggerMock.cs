using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides.Part3;

// The hidden appendix (DECK.md): a mock of the VS Code debugger, stopped in
// Demo/RotatingCube.cs. Not in the running order; shown only if the live
// demo fails, by pressing D on slide 25 (ChangeItLiveSlide).
static class DebuggerMock
{
    const uint EditorTop = 5;
    const uint EditorWidth = 18;
    const uint TabRow = EditorTop + 1;
    const uint StopRow = EditorTop + 6;     // the fourth code line
    const uint ChipColumn = Theme.Left + EditorWidth - 5;

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Debug C# on a C64" },
            new CodeBlock
            {
                X = Theme.Left, Y = EditorTop, Width = EditorWidth, Caption = "RotatingCube.cs",
                Lines =
                [
                    $"{Theme.Kw}  public void{Theme.Pl} Step()",
                    "  {",
                    "    BuildBasis();",
                    $"{Theme.Kw}    float{Theme.Pl} bcx = cx_;",
                ],
            },
            new TextElement { X = Theme.Left + 1, Y = StopRow, Text = Theme.Dot, Color = Theme.Bad, Wait = true },
            new Highlight { X = Theme.Left + 2, Y = StopRow, Width = EditorWidth - 3, Ink = Colors.Black, Paper = Theme.Accent2, Wait = true },
            new TextElement { X = ChipColumn, Y = TabRow, Text = " VICE", Color = Theme.Heading },
            new Highlight { X = ChipColumn, Y = TabRow, Width = 4, Ink = Theme.Heading, Paper = Colors.LightBlue },
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
            new BulletList
            {
                Left = Theme.RightColumn, Top = 5, Pitch = 3, Wait = true,
                Items = ["Breakpoints on C# lines", "F10, F11, Shift+F11", "Locals, fields, arrays", "Edit a value", "Registers, disassembly"],
            },
        ]
    };
}
