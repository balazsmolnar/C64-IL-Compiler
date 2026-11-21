using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides.Part3;

// Slide 12 (DECK.md): slide 7's Add, compiled again with three passes
// switched on one after another, one press each. Every number is measured
// (DECK.md, "Measurements", snippet S1): a bar is Add's own bytes with this
// pass and the ones above it on; the line under it is the 6502 it changed.
static class PassesSlide
{
    const string K = Theme.Kw, T = Theme.Ty, C = Theme.Com;
    const uint Naive = 43, Frames = 18, Tos = 14, Fused = 12;     // Add's bytes after each step
    const uint Pitch = 3;                                         // bar, code, a blank row

    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "43 bytes to 12" },
            new CodeBlock { X = Theme.Left, Y = Theme.BodyTop, Lines = [$"{K}static uint {T}Add({K}uint{T} a, {K}uint{T} b) => a + b;"] },
            new PassRow { Row = 9, Label = "One macro per IL", Bytes = Naive, Code = "#locals_push_value8, #add8", CodeColor = Theme.Accent },
            new PassRow { Row = 9 + Pitch, Label = "+ Static frames", Bytes = Frames, Before = Naive, Code = $"lda SF_TOP-511 {C}; no prologue", Wait = true },
            new PassRow { Row = 9 + 2 * Pitch, Label = "+ Stack top in A", Bytes = Tos, Before = Frames, Code = $"{C}; pha, pla x2: gone", Wait = true },
            new PassRow { Row = 9 + 3 * Pitch, Label = "+ Operand fusion", Bytes = Fused, Before = Tos, Code = $"lda SF_TOP-511 {C}; a: no pha/pla", Color = Theme.Accent, Wait = true },
        ]
    };
}

// One step of a shrinking routine: its name, a bar of Bytes at 4 px a byte
// (grey; Color for the result), the count a few pixels after the bar's end,
// the saving against Before in cyan, and under the bar a line of 6502 in the
// code face (white; comments `Theme.Com`). Drawn after a press, the name
// fades in, the bar grows, the count and saving appear as it stops, and the
// code line fades in.
//
//     new PassRow { Row = 12, Label = "+ Static frames", Bytes = 18, Before = 43, Code = "lda SF_TOP-511" }
sealed class PassRow : SlideElement
{
    const uint BarColumn = Theme.Left + 11;     // the names before it: 10 cells and a gap
    const ulong BarX = BarColumn * Theme.CellPixels;
    const uint PixelsPerByte = 4;               // 43 B: 172 px (a uint is 8 bits), its count inside the margin
    const ulong CountGap = 6;                   // past the bar's last pixel, so the count clears its cell

    public uint Row { get; init; }
    public required string Label { get; init; }
    public uint Bytes { get; init; }
    public uint Before { get; init; }           // the step above's bytes; 0 = the first step, no saving
    public required string Code { get; init; }
    public Colors Color { get; init; }          // the bar; Auto = Theme.Body
    public Colors CodeColor { get; init; }      // Auto = Theme.Heading (white, as in a code panel)
    public bool Wait { get; init; }

    public override void Draw()
    {
        if (Wait)
            Deck.Step();
        Helper.Text.Draw(Screen.Px(Theme.Left), Row, Label, Theme.Heading);
        Deck.Show();
        bool live = Deck.Live;
        uint length = Bytes * PixelsPerByte;
        ulong y = Screen.Px(Row);
        Deck.Paint(BarColumn, Row, (length + 7) / 8, 1, Theme.Or(Color, Theme.Body), Theme.Background);
        for (uint end = 0; end < length;)       // two cells a frame when live (Wait returns at once on a jump)
        {
            end = live && end + 16 < length ? end + 16 : length;
            Screen.Fill(BarX, y + 1, BarX + end - 1, y + 6);
            if (live)
                Deck.Wait(1);
        }
        // Built at run time from the numbers, so drawn at once (Text.DrawNow),
        // as the bar stops; the code line then fades in.
        ulong countEnd = Helper.Text.DrawNow(BarX + length + CountGap, Row, $"{Bytes} B", Theme.Heading);
        if (Before != 0)
            Helper.Text.DrawNow(Screen.CellAfter(countEnd, 8), Row, $"-{Before - Bytes}", Theme.Accent);
        Deck.Build(onSurface: false);
        Helper.Text.Code(BarColumn, Row + 1, Code, Theme.Or(CodeColor, Theme.Heading), Theme.Background);
    }
}
