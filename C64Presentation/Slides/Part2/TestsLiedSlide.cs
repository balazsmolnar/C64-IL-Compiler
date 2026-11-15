using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 11: what C64006 found in our own code: the value each line meant,
// and the value it really was (an 8-bit int keeps the low byte).
static class TestsLiedSlide
{
    const uint TableRow = 8;

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Our tests lied" },
            new TextElement { X = Theme.Left, Y = Theme.BodyTop, Text = "The new error found, in our code:" },
            new Table
            {
                Y = TableRow, Widths = [18, 9, 9], WaitAfterRow = true,
                Header = ["In our code", "meant", "was"],
                Rows =
                [
                    ["i < 256 loop", $"{Theme.Dim}256", $"{Theme.Red}0"],
                    ["new uint[256]", $"{Theme.Dim}256", $"{Theme.Red}0"],
                    ["This deck: Wait(500)", $"{Theme.Dim}500", $"{Theme.Red}244"],
                ],
            },
        ]
    };
}
