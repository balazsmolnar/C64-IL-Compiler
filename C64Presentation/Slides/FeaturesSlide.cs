using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Two columns: data types left, object orientation and memory right.
static class FeaturesSlide
{
    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "Features" },
            Heading(Theme.LeftColumn, 5, "Data types", wait: false),
            new BulletList
            {
                Top = 7, Wait = true,
                Items = ["int, uint: 8 bits", "long, ulong: 16 bits", "float", "string (max 255 bytes)", "object (255)", "array (max 255 items)"],
            },
            Heading(Theme.RightColumn, 5, "Object-oriented"),
            new BulletList { Left = Theme.RightColumn, Top = 7, Wait = true, Items = ["Inheritance", "Virtual methods", "Abstract methods"] },
            Heading(Theme.RightColumn, 14, "Memory"),
            new BulletList { Left = Theme.RightColumn, Top = 16, Items = ["Garbage collector"] },
        ]
    };

    static TextElement Heading(uint column, uint row, string text, bool wait = true)
        => new() { X = column, Y = row, Text = text, Color = Theme.Accent, Wait = wait };
}
