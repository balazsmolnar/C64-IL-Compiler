using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

// Engine placeholder art (art/engine/make_chip.py).
[assembly: RawImage(Resource = "chip.img", Label = "Chip")]

namespace C64Presentation.Slides;

// Examples of every engine element, with true figures, for the Slides phase
// to copy from (and replace): section divider, two columns with a picture
// and a callout, a bar chart, a diagram with arrows.
static class EngineShowcaseSlides
{
    public static Slide Section() => new SimpleSlide
    {
        Elements = new SlideElement[] { new SectionElement { Part = "Part 3", Title = "Small and fast", Subtitle = "Pay only for what you use" } }
    };

    // Two columns: a picture left, facts right, a callout across the bottom.
    public static Slide Machine() => new SimpleSlide
    {
        Elements = new SlideElement[]
        {
            new Title { Text = "The machine" },
            new ImageElement { Column = 2, Row = 6, Width = 16, Height = 8, Image = C64Address.FromLabel("img_Chip"), Caption = "MOS 6510, about 1 MHz" },
            new BulletList { Left = Theme.RightColumn, Top = 6, Wait = true, Items = new[] { "64 KB RAM", "A, X, Y: 8 bits", "256-byte stack", "320x200, 16 colours" } },
            new Callout { Y = 17, Wait = true, Lines = new[] { "Everything that follows is about fitting", "C# into " + Theme.Hi + "that." } },
        }
    };

    // A chart: before/after bars that grow, with the values at their ends.
    public static Slide Chart() => new SimpleSlide
    {
        Elements = new SlideElement[]
        {
            new Title { Text = "Hunchback: -24%" },
            new BarChart
            {
                Y = 6, LabelWidth = 7, Max = 37000, Wait = true,
                Labels = new[] { "Oct 17", "Oct 31" },
                Values = new ulong[] { 36847, 28157 },
                ValueTexts = new[] { "36,847 B", "28,157 B" },
                BarColors = new[] { Theme.Muted, Theme.Accent },
            },
            new TextElement { X = 2, Y = 12, Wait = true, Text = "Same game, same C#: " + Theme.Hi + "-8,690 B" },
            new TextElement { X = 2, Y = 14, Wait = true, Text = "Catacombs: 7,177 ~ 10,478 B free", Color = Theme.Muted },
        }
    };

    // A diagram: stages as cards joined by arrows that draw themselves.
    public static Slide Diagram() => new SimpleSlide
    {
        Elements = new SlideElement[]
        {
            new Title { Text = "The pipeline" },
            new Box { X = 2, Y = 6, Width = 8, Height = 3, Text = "C#", Color = Theme.Accent },
            new RightArrow { X = 10, Y = 7, Width = 5, Text1 = "dotnet", Wait = true },
            new Box { X = 15, Y = 6, Width = 8, Height = 3, Text = "IL .dll", Color = Colors.Violet },
            new RightArrow { X = 23, Y = 7, Width = 7, Text1 = "Compiler", Wait = true },
            new Box { X = 30, Y = 6, Width = 8, Height = 3, Text = ".asm", Color = Theme.Good },
            new DownArrow { X = 33, Y = 9, Height = 4, Text = "64tass", Wait = true },
            new Box { X = 30, Y = 13, Width = 8, Height = 4, Text = ".prg", Caption = "one file", Color = Theme.Accent2 },
            new Box { X = 2, Y = 13, Width = 12, Height = 4, Text = "VICE or a C64", Caption = "runs it", Color = Theme.Muted, Wait = true },
        }
    };
}

// The section divider as an element.
class SectionElement : SlideElement
{
    public string Part, Title, Subtitle;

    public override void Draw() => Master.Section(Part, Title, Subtitle);
}
