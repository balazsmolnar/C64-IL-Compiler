using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides.Part3;

// Slide 12 (DECK.md): slide 7's Add, compiled again with three passes
// switched on one at a time, one press each. Every number is measured
// (DECK.md, "Measurements", snippet S1): a bar is Add's own bytes, the grey
// line under it the real 6502 the pass changed (struck through: removed).
static class PassesSlide
{
    const string K = Theme.Kw, T = Theme.Ty;
    const uint Naive = 43, Frames = 18, Tos = 14, Fused = 12;     // Add's bytes after each step
    const uint Step1 = 9, Step2 = Step1 + 3, Step3 = Step2 + 3, Step4 = Step3 + 3;   // bar, code, a blank row
    const uint Bar = ByteBar.Column;
    const uint After = Bar + 1;            // + bytes / 2: the cell after a bar (half a cell a byte)

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "43 bytes to 12" },
            new CodeBlock { X = Theme.Left, Y = Theme.BodyTop, Lines = [$"{K}static uint {T}Add({K}uint{T} a, {K}uint{T} b) => a + b;"] },

            new TextElement { X = Theme.Left, Y = Step1, Text = "One macro per IL", Color = Theme.Heading },
            new ByteBar { Row = Step1, Bytes = Naive, Color = Theme.Muted },
            new TextElement { X = After + Naive / 2, Y = Step1, Text = "43 B", Color = Theme.Heading },
            new TextElement { X = Bar, Y = Step1 + 1, Text = "#locals_push_value8, #add8", Face = TextFace.Code, Color = Theme.Accent },

            new TextElement { X = Theme.Left, Y = Step2, Text = "Static frames", Color = Theme.Heading, Wait = true },
            new ByteBar { Row = Step2, Bytes = Frames },
            new TextElement { X = After + Frames / 2, Y = Step2, Text = $"18 B  {Theme.Hi}-25", Color = Theme.Heading },
            new TextElement { X = Bar, Y = Step2 + 1, Text = "lda SF_TOP-511, rts: no prologue", Face = TextFace.Code, Color = Theme.Body },

            new TextElement { X = Theme.Left, Y = Step3, Text = "Top of stack in A", Color = Theme.Heading, Wait = true },
            new ByteBar { Row = Step3, Bytes = Tos },
            new TextElement { X = After + Tos / 2, Y = Step3, Text = $"14 B  {Theme.Hi}-4", Color = Theme.Heading },
            new TextElement { X = Bar, Y = Step3 + 1, Text = "pha, pla   pha, pla", Face = TextFace.Code, Color = Theme.Body, Strike = true },

            new TextElement { X = Theme.Left, Y = Step4, Text = "Operand fusion", Color = Theme.Heading, Wait = true },
            new ByteBar { Row = Step4, Bytes = Fused, Color = Theme.Accent },
            new TextElement { X = After + Fused / 2, Y = Step4, Text = $"12 B  {Theme.Hi}-2", Color = Theme.Heading },
            new TextElement { X = Bar, Y = Step4 + 1, Text = "lda SF_TOP-511, adc zp_param1_low", Face = TextFace.Code, Color = Theme.Body },
        ]
    };
}

// A bar of Bytes at 4 px a byte from Column, 6 px high in its row. Drawn
// after a press, it grows; the elements after it in the same step fade in
// once it has grown.
sealed class ByteBar : SlideElement
{
    public const uint Column = Theme.Left + 11;    // the names before it: 10 cells and a gap
    const ulong X = Column * Theme.CellPixels;
    const uint PixelsPerByte = 4;                  // 43 B: 172 px (a uint is 8 bits), its count inside the margin

    public uint Row { get; init; }
    public uint Bytes { get; init; }
    public Colors Color { get; init; }             // Auto = Theme.Body

    public override void Draw()
    {
        Deck.Show();
        bool live = Deck.Live;
        uint length = Bytes * PixelsPerByte;
        ulong y = Screen.Px(Row);
        Deck.Paint(Column, Row, (length + 7) / 8, 1, Theme.Or(Color, Theme.Body), Theme.Background);
        for (uint end = 0; end < length;)          // two cells a frame when live (Wait returns at once on a jump)
        {
            end = live && end + 16 < length ? end + 16 : length;
            Screen.Fill(X, y + 1, X + end - 1, y + 6);
            if (live)
                Deck.Wait(1);
        }
        Deck.Build(onSurface: false);
    }
}
