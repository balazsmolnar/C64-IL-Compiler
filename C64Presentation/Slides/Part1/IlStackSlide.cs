using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 6: Add in C#, then its IL, then, one press each, the 6502 macro the
// compiler writes for each IL instruction (behind a Theme.Arrow), and the
// point. IL is violet (.NET), the macros cyan (ours). Columns and colour
// changes sit on multiples of 4 characters, where the 6 px code face meets
// the 8 px cells.
static class IlStackSlide
{
    const string K = Theme.Kw, T = Theme.Ty;
    const uint Panel = 5;                 // the panel's top row
    const uint Code = Theme.Left + 1;     // the panel's first code column
    const uint Macro = Code + 6;          // character 8: the arrow, then the macro at 12
    const uint Header = 9;                // the IL / macro header row
    const uint Ldarg0 = 10, Ldarg1 = 11, Add = 12, Ret = 13;
    const uint Point = 16;

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "IL is a stack" },
            new CodeBlock
            {
                X = Theme.Left, Y = Panel, Rows = 8,
                Lines = [$"{K}static uint {T}Add({K}uint{T} a, {K}uint{T} b)", "    => a + b;"],
            },
            new Rule { X = Code, Y = Header - 1, Length = Theme.Width - 2, Wait = true },
            new CodeLine { Column = Code, Row = Header, Text = "IL          6502 macro", Color = Theme.Body },
            new CodeLine { Column = Code, Row = Ldarg0, Text = "ldarg.0", Color = Theme.Brand },
            new CodeLine { Column = Code, Row = Ldarg1, Text = "ldarg.1", Color = Theme.Brand },
            new CodeLine { Column = Code, Row = Add, Text = "add", Color = Theme.Brand },
            new CodeLine { Column = Code, Row = Ret, Text = "ret", Color = Theme.Brand },

            new CodeLine { Column = Macro, Row = Ldarg0, Text = "~   #locals_push_value8 1", Color = Theme.Accent, Wait = true },
            new CodeLine { Column = Macro, Row = Ldarg1, Text = "~   #locals_push_value8 2", Color = Theme.Accent, Wait = true },
            new CodeLine { Column = Macro, Row = Add, Text = "~   #add8", Color = Theme.Accent, Wait = true },
            new CodeLine { Column = Macro, Row = Ret, Text = "~   #method_exit 4, []", Color = Theme.Accent, Wait = true },

            new TextElement { X = Theme.Left, Y = Point, Text = "One 6502 macro per IL instruction", Color = Theme.Accent, Wait = true },
        ]
    };
}
