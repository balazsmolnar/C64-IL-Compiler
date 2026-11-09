using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 6: Add in C#, then its IL, then, one press each, the 6502 macro the
// compiler writes for each IL instruction. IL is violet (.NET), the macros
// cyan (ours). Columns and colour changes sit on multiples of 4 characters,
// where the 6 px code face meets the 8 px cells.
static class IlStackSlide
{
    const string K = Theme.Kw, T = Theme.Ty, N = Theme.Num, C = Theme.Com, D = Theme.Dim;
    const string Il = "`4";               // violet: Theme.Brand as an inline colour
    const uint Code = Theme.Left + 1;     // the panel's first code column
    const uint Macro = Code + 6;          // character 8: the arrow, then the macro at 12
    const uint Header = 9;                // the IL / macro header row
    const string Becomes = D + Theme.Arrow + "   " + K;   // the arrow, then a macro

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "IL is a stack" },
            new CodeBlock
            {
                X = Theme.Left, Y = 5, Rows = 8,
                Lines = [$"{K}static uint {T}Add({K}uint{T} a, {K}uint{T} b)", "    => a + b;"],
            },
            new Rule { X = Code, Y = Header - 1, Length = Theme.Width - 2, Wait = true },
            new CodeLine { Column = Code, Row = Header, Text = $"{C}IL          6502 macro" },
            new CodeLine { Column = Code, Row = Header + 1, Text = $"{Il}ldarg.0" },
            new CodeLine { Column = Code, Row = Header + 2, Text = $"{Il}ldarg.1" },
            new CodeLine { Column = Code, Row = Header + 3, Text = $"{Il}add" },
            new CodeLine { Column = Code, Row = Header + 4, Text = $"{Il}ret" },
            Expands(1, $"{Becomes}#locals_push_value8 {N}1"),
            Expands(2, $"{Becomes}#locals_push_value8 {N}2"),
            Expands(3, $"{Becomes}#add8"),
            Expands(4, $"{Becomes}#method_exit{N} 4, []"),
        ]
    };

    // IL line n's macro (constant text: a string built at run time would
    // live in the shared concatenation buffer), the next build step.
    static CodeLine Expands(uint line, string macro) => new() { Column = Macro, Row = Header + line, Text = macro, Wait = true };
}
