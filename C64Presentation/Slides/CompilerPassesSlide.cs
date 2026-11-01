using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// The pipeline C# -> IL -> asm -> prg, one stage per press, with the code
// of each stage replacing the previous one below.
class CompilerPassesSlide
{
    const string K = Theme.Kw, T = Theme.Ty, P = Theme.Pl, N = Theme.Num, C = Theme.Com;

    public static Slide Create()
    {
        var csCodeBlock = new CodeBlock()
        {
            Caption = "C#",
            Lines = new[]
            {
                K + "static " + T + "int" + P + " Add(" + T + "int" + P + " a, " + T + "int" + P + " b)",
                "    => a + b;",
            },
            X = 2, Y = 10, Width = 36, Height = 11
        };

        var ilCodeBlock = new CodeBlock()
        {
            Caption = "IL",
            Lines = new[]
            {
                C + "IL0000: " + K + "ldarg.0",
                C + "IL0001: " + K + "ldarg.1",
                C + "IL0002: " + K + "add",
                C + "IL0003: " + K + "ret",
            },
            X = 2, Y = 10, Width = 36, Height = 11
        };

        var asmCodeBlock = new CodeBlock()
        {
            Caption = "64tass macros",
            Lines = new[]
            {
                T + "Add:",
                K + "  #init_locals_pull_parameters" + P + " 0",
                K + "  #locals_push_value8" + N + " 1  " + C + "; ldarg.0",
                K + "  #locals_push_value8" + N + " 2  " + C + "; ldarg.1",
                K + "  #add8               " + C + "; add",
                K + "  #method_exit" + N + " 4      " + C + "; ret",
            },
            X = 2, Y = 10, Width = 36, Height = 11
        };

        var prgCodeBlock = new CodeBlock()
        {
            Caption = "6502",
            Lines = new string[]
            {
                C + ".3C0F A4 24     " + K + "LDY" + P + " STACKPOINTER",
                C + ".3C11 B9 FF C6  " + K + "LDA" + P + " LOCALSSTACK-1,Y",
                C + ".3C14 48        " + K + "PHA",
                C + ".3C15 A4 24     " + K + "LDY" + P + " STACKPOINTER",
                C + ".3C17 B9 FE C6  " + K + "LDA" + P + " LOCALSSTACK-2,Y",
                C + ".3C1A 48        " + K + "PHA",
                C + ".3C1B 68        " + K + "PLA",
                C + ".3C1C 85 32     " + K + "STA" + P + " $32",
                C + ".3C1E 68        " + K + "PLA",
            },
            X = 2, Y = 10, Width = 36, Height = 11
        };

        return new SimpleSlide()
        {
            Elements = new SlideElement[]
            {
                new Title() {Text = "How does it work?"},
                csCodeBlock,
                new Box() { X = 2, Y = 5, Width = 5, Height = 3, Text = "C#", Color = Theme.Accent, WaitAfter = true },
                new RightArrow() { X = 7, Y = 6, Width = 5, Text1 = ".NET" },
                ilCodeBlock,
                new Box() { X = 12, Y = 5, Width = 5, Height = 3, Text = "IL", Color = Colors.Violet, WaitAfter = true },
                new RightArrow() { X = 17, Y = 6, Width = 5, Text1 = "ours" },
                asmCodeBlock,
                new Box() { X = 22, Y = 5, Width = 5, Height = 3, Text = "asm", Color = Theme.Good, WaitAfter = true },
                new RightArrow() { X = 27, Y = 6, Width = 6, Text1 = "64tass" },
                prgCodeBlock,
                new Box() { X = 33, Y = 5, Width = 5, Height = 3, Text = "prg", Color = Theme.Accent2 },
            }
        };
    }
}
