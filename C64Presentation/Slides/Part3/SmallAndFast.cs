using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides.Part3;

// Section 6, slide 13 (DECK.md): the rule, and what it bought, as two stat
// tiles (a figure in the 16 px face over a caption), one per press.
static class SmallAndFast
{
    const string Number = "06";             // DECK.md's section 6
    const uint MechanismRow = 16;           // under the master's subtitle (row 15)
    const uint TileRow = 18;
    const uint CaptionRow = TileRow + 2;

    public static Slide Section() => new SimpleSlide
    {
        Elements =
        [
            new SectionElement { Number = Number, Title = "Small and fast", Subtitle = "Pay only for what you use", Icon = C64Address.FromLabel("img_SecChart") },
            new TextElement { X = Theme.Left, Y = MechanismRow, Text = ".weak Flag_X", Face = TextFace.Code, Color = Theme.Muted },
            new TextElement { X = Theme.LeftColumn, Y = TileRow, Text = $"39 {Theme.Arrow} {Theme.Hi}12", Face = TextFace.Heading, Wait = true },
            new TextElement { X = Theme.LeftColumn, Y = CaptionRow, Text = "bytes for Add" },
            new TextElement { X = Theme.RightColumn, Y = TileRow, Text = $"{Theme.Hi}-24%", Face = TextFace.Heading, Wait = true },
            new TextElement { X = Theme.RightColumn, Y = CaptionRow, Text = "Hunchback, same C#" },
        ]
    };
}
