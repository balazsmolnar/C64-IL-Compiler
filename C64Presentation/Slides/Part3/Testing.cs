using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides.Part3;

// Section 7, slides 14-15: tests that run on a 6502 (DECK.md).
static class Testing
{
    const Colors Passed = Colors.LightGreen;    // NUnit's green
    const uint TickColumn = 31;                 // right of the [TestCase] line, in the panel

    // 14: the code panel and its first line, whose 5 and 6 are underlined.
    const uint CodeTop = 5;
    const uint CodeColumn = Theme.Left + 1;     // CodeBlock draws from inside its frame
    const uint TestCaseRow = CodeTop + 1;
    const string TestCase = "[TestCase(5, ExpectedResult = 6)]";
    const uint FiveAt = 10;                     // characters into TestCase
    const uint SixAt = 30;

    // 14: an ordinary NUnit test case whose body runs as 6502 code. The code
    // is shown exactly as written; the code face can change colour only on a
    // cell edge (every 4 characters), so 5 and 6 are marked by underlines,
    // and `int`, which can't start on one as a parameter type, stays white.
    public static Slide TestsRunOn6502() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Tests run on 6502" },
            new CodeBlock
            {
                X = Theme.Left, Y = CodeTop,
                Lines =
                [
                    TestCase,
                    "int TestIncrement(int value)",
                    "{",
                    $"    {Theme.Kw}var{Theme.Pl}a = value;",
                    $"    {Theme.Kw}return{Theme.Pl}++a;",
                    "}",
                ],
            },
            new CodeUnderline { Column = CodeColumn, Row = TestCaseRow, From = FiveAt, Length = 1 },
            new CodeUnderline { Column = CodeColumn, Row = TestCaseRow, From = SixAt, Length = 1 },
            new TextElement { X = Theme.Left, Y = 14, Text = "NUnit finds it, runs it, reports it", Wait = true },
            new TextElement { X = Theme.Left, Y = 16, Text = $"{Theme.Hi}5{Theme.Off} goes in via $09E0", Wait = true },
            new TextElement { X = Theme.Left, Y = 18, Text = $"{Theme.Hi}6{Theme.Off} comes back at $2C", Wait = true },
            new TextElement { X = Theme.Left, Y = 20, Text = "The body never runs as .NET", Wait = true },
            new TextElement { X = TickColumn, Y = TestCaseRow, Text = Theme.Tick, Face = TextFace.Heading, Color = Passed },
        ]
    };

    // 15 (D8): what the PostSharp aspect does around every test call: two
    // groups of steps on a rail, spread over the body rows.
    const Colors Result = Colors.LightGreen;    // CopyResultFromEmulator's group
    const uint InvokeRow = 5;                   // OnInvoke: steps 1-4
    const uint ResultRow = 15;                  // CopyResultFromEmulator: steps 5-6
    const uint CountRow = 20;

    public static Slide PostSharpSwapsIt() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "PostSharp swaps it" },
            new GroupHeader { Row = InvokeRow, Text = "RunInEmulatorAspect.OnInvoke", Color = Theme.Brand },
            new FlowStep { Row = InvokeRow + 1, Number = "1", Text = "Load prg/unittest.prg", First = true },
            new FlowStep { Row = InvokeRow + 3, Number = "2", Text = $"Method address {Theme.Arrow} $09FE" },
            new FlowStep { Row = InvokeRow + 5, Number = "3", Text = $"Arguments {Theme.Arrow} $09E0", Wait = true },
            new FlowStep { Row = InvokeRow + 7, Number = "4", Text = "Run from $1000" },
            new GroupHeader { Row = ResultRow, Text = "CopyResultFromEmulator", Color = Result, Wait = true },
            new FlowStep { Row = ResultRow + 1, Number = "5", Text = "$20: 0 pass, $FF fail, $FE fault", First = true },
            new FlowStep { Row = ResultRow + 3, Number = "6", Text = "Message at $2E/$2F, value at $2C" },
            new TextElement { X = Theme.Left, Y = CountRow, Text = "Over 700 test cases, all on 6502", Color = Theme.Accent, Wait = true },
        ]
    };
}