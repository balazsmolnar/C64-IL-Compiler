#if SCENE_ENGINE
using System;
using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// The engine's elements on DECK.md's layouts, for review (-Scene ENGINE;
// Program.cs shows these instead of the deck). Figures are DECK.md's.
static class EngineGallery
{
    public static readonly Func<Slide>[] Slides =
    [
        MemoryMapSlide, HunchbackChart, NoSingleWinner, WorstFrame, TestLoop,
        () => new PictureSlide { Picture = DiskPicture.ShotRoom, Split = 21, Caption = $"Every line of game logic: {Theme.Hi}C#" },
        () => new PictureSlide { Picture = DiskPicture.ShotBoss, Split = 15, Caption = "The Bone Warden" },
        KeyMessage, TwoColumns, Section,
    ];

    // D7: the memory map, text in the right column.
    static Slide MemoryMapSlide() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Hide under the ROM" },
            new MemoryMap
            {
                X = 8, Y = 5, Width = 8, Note = "not to scale",
                Segments =
                [
                    new() { Name = "bitmap", Address = "$E000", Height = 3, Color = Theme.Accent2 },
                    new() { Name = "I/O", Address = "$D000", Height = 2, Color = Theme.Muted },
                    new() { Name = "colours", Address = "$CC00", Height = 2, Color = Theme.Accent },
                    new() { Name = "program", Address = "$1000", Height = 3, Color = Theme.Body },
                    new() { Name = "tables", Address = "$0800", Height = 2, Color = Theme.Muted },
                    new() { Name = "zero page", Address = "$0000", Height = 2, Color = Theme.Muted },
                ],
            },
            Fact(5, "Bitmap under the KERNAL ROM", wait: true),
            Fact(7, "Sprites under I/O", wait: true),
            Fact(9, $"{Theme.Hi}Frees 9.4 KB", wait: true),
            Fact(11, "Pictures packed:", wait: true),
            Fact(13, $"8,400 {Theme.Arrow} 2,118 B", wait: false),
        ]
    };

    static TextElement Fact(uint row, string text, bool wait) => new() { X = 19, Y = row, Text = text, Wait = wait };

    // C2: two bars and the dashed gap.
    static Slide HunchbackChart() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Hunchback: -24%" },
            new BarChart
            {
                X = 2, Y = 7, Width = 36, LabelWidth = 7, ValueWidth = 9, Pitch = 3, Max = 36847, Wait = true,
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

    // C3: eight optimizations, two programs: a butterfly chart (DECK.md's C3
    // was 8 x 2 bars on consecutive rows: now Hunchback grows left, Catacombs
    // right, from a centred label column, one item every 2 rows).
    static Slide NoSingleWinner() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "No single winner" },
            new ButterflyChart
            {
                Max = 3420, Left = "Hunchback", Right = "Catacombs", LegendRow = 2,
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
        ]
    };

    static ButterflyRow Saved(string label, ulong hunchback, string hunchbackText, ulong catacombs, string catacombsText)
        => new() { Label = label, LeftValue = hunchback, LeftText = hunchbackText, RightValue = catacombs, RightText = catacombsText };

    // C4: bars against a full-frame outline, ticked every 50 lines.
    static Slide WorstFrame() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Worst frame: -19%" },
            new TextElement { X = Theme.Left, Y = 5, Text = "Catacombs, raster lines per frame", Color = Theme.Muted },
            new BarChart
            {
                X = 2, Y = 9, Width = 36, LabelWidth = 7, ValueWidth = 5, Pitch = 4, Max = 311, Outline = true, TickEvery = 50, Wait = true,
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

    // D8-style loop: four steps clockwise, with up and left arrows.
    static Slide TestLoop() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Tests run on 6502" },
            Step(2, 6, "NUnit", "calls the test", Theme.Brand, wait: false),
            new Arrow { FromCol = 15, FromRow = 7, ToCol = 22, ToRow = 7, Text = "intercepts", Wait = true },
            Step(23, 6, "PostSharp aspect", "OnInvoke", Theme.Brand),
            new Arrow { FromCol = 30, FromRow = 11, ToCol = 30, ToRow = 14, Text = "loads .prg", Wait = true },
            Step(23, 15, "SimpleEmulator", "runs 6502", Theme.Accent),
            new Arrow { FromCol = 22, FromRow = 16, ToCol = 15, ToRow = 16, Text = "result byte", Wait = true },
            Step(2, 15, "$20 result", $"{Theme.Tick} 0 = pass", Theme.Accent),
            new Arrow { FromCol = 8, FromRow = 14, ToCol = 8, ToRow = 11, Text = "back to NUnit", Wait = true },
        ]
    };

    static Box Step(uint x, uint y, string name, string detail, Colors color, bool wait = true)
        => new() { X = x, Y = y, Width = 13, Height = 4, Text = name, Caption = detail, Color = color, Wait = wait };

    // A key message.
    static Slide KeyMessage() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Four lessons" },
            new BulletList { Items = ["Structure beats peepholes", "Measure every change", "Unsupported = compile error"], Wait = true },
            new Callout { Y = 11, Lines = ["Every slide you have seen", "was C#, on a 6502."], Attribution = "this deck, compiled by this compiler", Wait = true },
        ]
    };

    // Two columns: a picture left, facts right.
    static Slide TwoColumns() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "64 KB at 1 MHz" },
            new ImageElement { Column = 1, Row = 7, Width = 18, Height = 8, Image = C64Address.FromLabel("img_Breadbox"), Caption = "Commodore 64, 1982" },
            new BulletList { Left = Theme.RightColumn, Top = 6, Wait = true, Items = ["64 KB RAM", "1 MHz 6510 CPU", "A, X, Y: 8 bits", "256-byte stack", "320x200, 16 colours"] },
        ]
    };

    static Slide Section() => new SimpleSlide
    {
        Elements =
        [
            new SectionElement { Number = "03", Title = "Small and fast", Subtitle = "Pay only for what you use", Icon = C64Address.FromLabel("img_SecGear") },
        ]
    };
}

// A picture from the disk over a split, its caption in the hi-res band.
sealed class PictureSlide : Slide
{
    public DiskPicture Picture { get; init; }
    public uint Split { get; init; }
    public required string Caption { get; init; }

    public override void Present() => Master.Picture(ArtDisk.Load(Picture), 0, 0, Split, Caption);
}
#endif
