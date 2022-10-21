using C64Lib;
using C64Presentation.Animations;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

class CompilerPassesSlide
{

    public static Slide Create()
    {
        var prgCodeBlock = new CodeBlock()
        {
            Lines = new string[]
            {
                ".3C0F   A4 24      LDY STACKPOINTER",
                ".3C11   B9 FF C6   LDA LOCALSSTACK-1,Y",
                ".3C14   48         PHA",
                ".3C15   A4 24      LDY STACKPOINTER",
                ".3C17   B9 FE C6   LDA LOCALSSTACK-2,Y",
                ".3C1A   48         PHA",
                ".3C1B   68         PLA",
                ".3C1C   85 32      STA $32",
                ".3C1E   68         PLA",
                ".3C1F   18         CLC",
                ".3C20   65 32      ADC $32",
                ".3C22   48         PHA",
            },
            X = 0,
            Y = 13,
            Width = 40,
            Height = 10
        };
        var csCodeBlock = new CodeBlock()
        {
            Lines = new[]
            {
                "  STATIC INT ADD(INT A, INT B) => A+B",
            },
            X = 0,
            Y = 15,
            Width = 38,
            Height = 10

        };

        var ilCodeBlock = new CodeBlock()
        {
            Lines = new[]
            {
                "IL0000: LDARG.0",
                "IL0001: LDARG.1",
                "IL0002: ADD",
                "IL0003: RET"
            },
            X = 0,
            Y = 15,
            Width = 38,
            Height = 10
        };

        var asmCodeBlock = new CodeBlock()
        {
            Lines = new[]
            {
                "ADD:",
                "  #INIT-LOCALS-PULL-PARAMETERS 0, [0,0]",
                "  #LOCALS-PUSH-VALUE8 1 ; LDARG.0",
                "  #LOCALS-PUSH-VALUE8 2 ; LDARG.1",
                "  #ADD8 ; ADD",
                "  #METHOD-EXIT 4, [] ; RET",
            },
            X = 0,
            Y = 15,
            Width = 38,
            Height = 10
        };



        return new SimpleSlide()
        {
            Elements = new SlideElement[]
            {
                new Title() {Text = "HOW DOES IT WORK?"},
                csCodeBlock,
                new Box() { X = 0, Y = 6, Width = 4, Height = 7, Text = "C#", Color = Colors.Cyan, WaitAfter = true },
                new RightArrow() { X = 4, Y= 9, Width = 8, Text1 = ".NET", Text2 = "COMP"},
                ilCodeBlock,
                new Box() { X = 12, Y = 6, Width = 4, Height = 7, Text = "IL", Color = Colors.Violet, WaitAfter = true },
                new RightArrow() { X = 16, Y= 9, Width = 6, Text2 = "(*)"},
                asmCodeBlock,
                new Box() { X = 22, Y = 6, Width = 5, Height = 7, Text = "ASM", Color = Colors.LightGreen, WaitAfter = true  },
                new RightArrow() { X = 27, Y= 9, Width = 8, Text2 = "64TASS"},
                prgCodeBlock,
                new Box() { X = 35, Y = 6, Width = 5, Height = 7, Text = "PRG", Color = Colors.Yellow },
            },
            Animation = new RandomDisappearAnimation()
        };
    }
}