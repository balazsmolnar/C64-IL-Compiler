using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides.Part3;

// Section 6, slides 17-21: what the optimizations bought (DECK.md).
static class SmallAndFast
{
    // 17: the rule every optimization follows.
    public static Slide Section() => new SimpleSlide
    {
        Elements =
        [
            new SectionElement { Number = "03", Title = "Small and fast", Subtitle = "Pay only for what you use", Icon = C64Address.FromLabel("img_SecChart") },
            new TextElement { X = Theme.Left, Y = 17, Text = ".weak Flag_X", Color = Theme.Muted },
        ]
    };

    // 18: slide 7's Add again, compiled today.
    public static Slide BytesTo12() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "39 bytes to 12" },
            AddSize(29, "before", 39, "39 B", Theme.Muted),
            new CodeBlock
            {
                X = Theme.Left, Y = 5, Width = 25, Wait = true,
                Lines =
                [
                    $"Program_Add         {Theme.Com}; 12 bytes",
                    $"  {Theme.Kw}lda {Theme.Pl}SF_TOP-512   {Theme.Com}; b",
                    $"  {Theme.Kw}sta {Theme.Pl}zp_param1_low",
                    $"  {Theme.Kw}lda {Theme.Pl}SF_TOP-511   {Theme.Com}; a",
                    $"  {Theme.Kw}clc",
                    $"  {Theme.Kw}adc {Theme.Pl}zp_param1_low",
                    $"  {Theme.Kw}rts              {Theme.Com}; sum in A",
                ],
            },
            AddSize(34, "after", 12, "12 B", Theme.Accent),
            new BulletList { Top = 15, Wait = true, Items = ["Static frame: no prologue", "Caller stores args: lda, sta", "Result comes back in A"] },
        ]
    };

    // 53 px for the 39 bytes: the two columns share one scale.
    static Column AddSize(uint x, string label, ulong bytes, string text, Colors color)
        => new() { X = x, Bottom = 12, Pixels = bytes * 53 / 39, Text = text, Label = label, Color = color };

    // 19 (C2): the same game, two compilers two weeks apart.
    public static Slide Hunchback() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Hunchback: -24%" },
            new BarChart
            {
                X = Theme.Left, Y = 7, Width = Theme.Width, LabelWidth = 7, ValueWidth = 9, Pitch = 3, Max = 36847, Wait = true,
                Bars =
                [
                    new() { Label = "Oct 17", Value = 36847, Text = "36,847 B", Color = Theme.Muted },
                    new() { Label = "Oct 31", Value = 28157, Text = "28,157 B" },
                ],
                Gap = new() { From = 0, To = 1, Text = "-8,690 B" },
            },
            new TextElement { X = Theme.Left, Y = 15, Text = "Same game, same C#: -8,690 B" },
        ]
    };

    // 20 (C3): which optimization pays depends on the program.
    public static Slide NoSingleWinner() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "No single winner" },
            new ButterflyChart
            {
                Max = 3420, Left = "Hunchback", Right = "Catacombs", LegendRow = 2, Y = 4, Wait = true,
                Rows =
                [
                    Saved("Object arrays", 3357, "3,357", 0, ""),
                    Saved("Static frames", 2304, "2,304", 1818, "1,818"),
                    Saved("Exit tails", 1983, "1,983", 2579, "2,579"),
                    Saved("Top of stack", 824, "824", 1168, "1,168"),
                    Saved("Static arrays", 0, "", 616, "616"),
                    Saved("Skip Y reload", 311, "311", 495, "495"),
                    Saved("++ on statics", 28, "28", 340, "340"),
                    Saved("Logic + const", 85, "85", 326, "326"),
                ],
            },
            new TextElement { X = Theme.Left, Y = 20, Text = "Bytes saved, each at its own commit", Color = Theme.Muted },
        ]
    };

    static ButterflyRow Saved(string label, ulong hunchback, string hunchbackText, ulong catacombs, string catacombsText)
        => new() { Label = label, LeftValue = hunchback, LeftText = hunchbackText, RightValue = catacombs, RightText = catacombsText };

    // 21 (C4): smaller code is faster code.
    public static Slide WorstFrame() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Worst frame: -19%" },
            new TextElement { X = Theme.Left, Y = 5, Text = "Catacombs, raster lines per frame", Color = Theme.Muted },
            new BarChart
            {
                X = Theme.Left, Y = 9, Width = Theme.Width, LabelWidth = 7, ValueWidth = 5, Pitch = 4, Max = 311, Outline = true, TickEvery = 50, Wait = true,
                Bars =
                [
                    new() { Label = "before", Value = 257, Text = "257", Color = Theme.Muted },
                    new() { Label = "after", Value = 208, Text = "208" },
                ],
                Gap = new() { From = 0, To = 1, Text = "-49 lines" },
            },
            new TextElement { X = Theme.Left, Y = 17, Text = "311 lines = one frame; the rest is free for the game", Color = Theme.Muted },
        ]
    };
}
