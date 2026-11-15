using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides.Part3;

// Section 7, slides 22-23: tests that run on a 6502 (DECK.md).
static class Testing
{
    const Colors Passed = Colors.LightGreen;    // NUnit's green: this slide only

    // 22: an ordinary NUnit test case whose body runs as 6502 code.
    public static Slide TestsRunOn6502() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Tests run on 6502" },
            new CodeBlock
            {
                X = Theme.Left, Y = 5,
                Lines =
                [
                    "[TestCase(5, ExpectedResult = 6)]",
                    $"{Theme.Kw}int {Theme.Pl}TestIncrement({Theme.Kw}int{Theme.Pl} value)",
                    "{",
                    $"    {Theme.Kw}var {Theme.Pl}a = value;",
                    $"    {Theme.Kw}return{Theme.Pl} ++a;",
                    "}",
                ],
            },
            Line(14, "NUnit finds it, runs it, reports it"),
            Line(16, $"{Theme.Hi2}5{Theme.Off} goes in via $09E0, {Theme.Hi2}6{Theme.Off} back at $2C"),
            Line(18, "The body never runs as .NET"),
            new TextElement { X = 31, Y = 6, Text = Theme.Tick, Face = TextFace.Heading, Color = Passed },
        ]
    };

    static TextElement Line(uint row, string text) => new() { X = Theme.Left, Y = row, Text = text, Wait = true };

    // 23 (D8): what the PostSharp aspect does around every test call.
    public static Slide PostSharpSwapsIt() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "PostSharp swaps it" },
            new FlowStep { Row = 5, Text = "1 Load prg/unittest.prg", Label = "RunInEmulatorAspect.OnInvoke", Color = Theme.Brand },
            InCall(7, $"2 Method address {Theme.Arrow} $09FE"),
            InCall(9, $"3 Arguments {Theme.Arrow} $09E0", wait: true),
            InCall(11, "4 Run from $1000"),
            new FlowStep { Row = 14, Text = "5 $20: 0 pass, $FF fail, $FE fault", Label = "CopyResultFromEmulator", Wait = true },
            new FlowStep { Row = 16, Text = "6 Message at $2E/$2F, value at $2C" },
            new TextElement { X = Theme.Left, Y = 19, Text = "Over 700 test cases, all on 6502", Color = Theme.Accent, Wait = true },
        ]
    };

    static FlowStep InCall(uint row, string text, bool wait = false)
        => new() { Row = row, Text = text, Color = Theme.Brand, Wait = wait };
}
