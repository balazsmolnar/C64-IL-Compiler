using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 3: a class with a primary constructor, then the statements that use
// it (in Main) under a rule on the same panel. The code compiles and runs on
// the C64 as shown (DECK.md, snippet S2).
//
// Each colour change starts on a cell edge (8 px) while the code face is
// 6 px a character, so the colours change at characters 4, 8, 12, ... or
// in a space where that costs the least: the runs below are cut there.
static class OrdinaryCSharpSlide
{
    const string K = Theme.Kw, T = Theme.Ty;
    const uint Code = Theme.Left + 1;     // the panel's first code column
    const uint Statements = 12;           // the row of the first statement

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "It's ordinary C#" },
            new CodeBlock
            {
                X = Theme.Left, Y = 5, Rows = 9,
                Lines =
                [
                    $"{K}class {T}Star({K}uint{T} speed)",
                    "{",
                    $"    {K}public uint {T}X {{ {K}get; private set;{T} }}",
                    $"    {K}public void {T}Move() => X += speed;",
                    "}",
                ],
            },
            new Rule { X = Code, Y = Statements - 1, Length = Theme.Width - 2, Wait = true },
            new CodeLine { Column = Code, Row = Statements, Text = $"Star[] sky = {K}[new{T}(1),{K} new{T}(2)];" },
            new CodeLine { Column = Code, Row = Statements + 1, Text = $"{K}foreach {T}({K}var {T}star{K} in {T}sky)" },
            new CodeLine { Column = Code, Row = Statements + 2, Text = "    star.Move();" },
            new TextElement { X = 32, Y = Statements, Text = "in Main", Color = Theme.Muted },
            new BulletList
            {
                Top = 17, Wait = true,
                Items = ["Primary constructors, properties, [a, b]", "C64Lib: the C64 as a C# API"],
            },
        ]
    };
}
