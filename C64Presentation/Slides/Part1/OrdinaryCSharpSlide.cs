using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 3: a class with a primary constructor, then the statements that use
// it (in Main) under a rule on the same panel. The code compiles and runs on
// the C64 as shown (DECK.md, snippet S2).
//
// A colour can only change on a cell edge (8 px) while the code face is
// 6 px a character, so only keywords that start and end on an edge (or in
// a space next to one) are cyan; the rest stays white rather than colour a
// bracket or a semicolon.
static class OrdinaryCSharpSlide
{
    const string K = Theme.Kw, T = Theme.Ty;
    const uint Panel = 5;                 // the panel's top row
    const uint Code = Theme.Left + 1;     // the panel's first code column
    const uint Statements = 12;           // the row of the first statement
    const uint Label = 32;                // "in Main", right of the first statement
    const uint Points = 17;

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "It's ordinary C#" },
            new CodeBlock
            {
                X = Theme.Left, Y = Panel, Rows = 9,
                Lines =
                [
                    $"{K}class{T} Star(uint speed)",
                    "{",
                    $"    {K}public uint {T}X {{ get; private set; }}",
                    $"    {K}public void {T}Move() => X += speed;",
                    "}",
                ],
            },
            new Rule { X = Code, Y = Statements - 1, Length = Theme.Width - 2, Wait = true },
            new CodeLine { Column = Code, Row = Statements, Text = "Star[] sky = [new(1), new(2)];" },
            new CodeLine { Column = Code, Row = Statements + 1, Text = $"foreach {T}(var star in sky)", Color = Theme.Accent },
            new CodeLine { Column = Code, Row = Statements + 2, Text = "    star.Move();" },
            new TextElement { X = Label, Y = Statements, Text = "in Main", Color = Theme.Muted },
            new BulletList
            {
                Top = Points, Wait = true,
                Items = ["Primary constructors, properties, [a, b]", "C64Lib: the C64 as a C# API"],
            },
        ]
    };
}
