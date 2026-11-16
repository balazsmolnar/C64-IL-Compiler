using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides.Part3;

// Section 7, slides 14-15: tests that run on a 6502 (DECK.md).
static class Testing
{
    const Colors Passed = Colors.LightGreen;    // NUnit's green
    const uint TickColumn = 31;                 // right of the [TestCase] line, in the panel

    // 14: an ordinary NUnit test case whose body runs as 6502 code. The code
    // face changes colour on a cell edge only (every 4 characters): a keyword
    // that can't start on one stays white, and the cut itself is the space.
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
                    $"[TestCase({Theme.Kw}5{Theme.Pl}, ExpectedResult ={Theme.Kw}6{Theme.Pl})]",
                    $"{Theme.Kw}int{Theme.Pl}TestIncrement(int value)",
                    "{",
                    $"    {Theme.Kw}var{Theme.Pl}a = value;",
                    $"    {Theme.Kw}return{Theme.Pl}++a;",
                    "}",
                ],
            },
            new TextElement { X = Theme.Left, Y = 14, Text = "NUnit finds it, runs it, reports it", Wait = true },
            new TextElement { X = Theme.Left, Y = 16, Text = $"{Theme.Hi}5{Theme.Off} goes in via $09E0, {Theme.Hi}6{Theme.Off} back at $2C", Wait = true },
            new TextElement { X = Theme.Left, Y = 18, Text = "The body never runs as .NET", Wait = true },
            new TextElement { X = TickColumn, Y = 6, Text = Theme.Tick, Face = TextFace.Heading, Color = Passed },
        ]
    };

    // 15 (D8): what the PostSharp aspect does around every test call: the
    // steps on the left, the bytes they touch on the right. D swaps in the
    // hidden debugger mock, the fallback for a failed live demo.
    public static Slide PostSharpSwapsIt() => PostSharp(builds: true);

    static Slide PostSharpBuilt() => PostSharp(builds: false);

    const Colors Result = Colors.LightGreen;    // CopyResultFromEmulator's group
    const uint Strip = FlowStep.StripColumn;
    const uint StripWidth = Theme.Right - FlowStep.StripColumn;

    static Slide PostSharp(bool builds) => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "PostSharp swaps it" },
            new Box { X = Strip, Y = 6, Width = StripWidth, Height = 2, Text = "$0020", Color = Result },
            new Box { X = Strip, Y = 8, Width = StripWidth, Height = 2, Text = "$002C", Color = Result },
            new Box { X = Strip, Y = 10, Width = StripWidth, Height = 2, Text = "$002E", Color = Result },
            new Box { X = Strip, Y = 12, Width = StripWidth, Height = 2, Text = "$09E0", Color = Theme.Brand },
            new Box { X = Strip, Y = 14, Width = StripWidth, Height = 2, Text = "$09FE", Color = Theme.Brand },
            new Box { X = Strip, Y = 16, Width = StripWidth, Height = 2, Text = "$1000", Color = Theme.Brand },
            new Highlight { X = Theme.Left, Y = 5, Width = 1, Ink = Theme.Brand, Paper = Theme.Brand },
            new TextElement { X = Theme.Left + 1, Y = 5, Text = " RunInEmulatorAspect.OnInvoke" },
            new FlowStep { Row = 6, Text = "1 Load prg/unittest.prg" },
            new FlowStep { Row = 8, Text = $"2 Method address {Theme.Arrow} $09FE", Arrow = true, Leaders = [15] },
            new FlowStep { Row = 10, Text = $"3 Arguments {Theme.Arrow} $09E0", Arrow = true, Leaders = [13], Wait = builds },
            new FlowStep { Row = 12, Text = "4 Run from $1000", Arrow = true, Leaders = [17] },
            new Highlight { X = Theme.Left, Y = 14, Width = 1, Ink = Result, Paper = Result, Wait = builds },
            new TextElement { X = Theme.Left + 1, Y = 14, Text = " CopyResultFromEmulator" },
            new FlowStep { Row = 15, Text = "5 $20: 0 pass, $FF fail, $FE fault", Leaders = [7] },
            new FlowStep { Row = 17, Text = "6 Message at $2E/$2F, value at $2C", Arrow = true, Leaders = [9, 11] },
            new TextElement { X = Theme.Left, Y = 19, Text = "Over 700 test cases, all on 6502", Color = Theme.Accent, Wait = builds },
            new HiddenSlideKey { Enabled = builds, Appendix = DebuggerMock.Create, Return = PostSharpBuilt },
        ]
    };
}
