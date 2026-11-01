using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Two columns: data types left, object orientation and memory right.
class FeaturesSlide
{
    public static Slide Create()
    {
        return new SimpleSlide()
        {
            Elements = new SlideElement[]
            {
                new Title() { Text = "Features" },
                new TextElement() { X = Theme.LeftColumn, Y = 5, Text = "Data types", Color = Theme.Accent },
                new BulletList() { Top = 7, Wait = true, Items = new [] { "int, uint: 8 bits", "long, ulong: 16 bits", "float", "string (max 255 bytes)", "object (255)", "array (max 255 items)" } },
                new TextElement() { X = Theme.RightColumn, Y = 5, Text = "Object-oriented", Color = Theme.Accent, Wait = true },
                new BulletList() { Left = Theme.RightColumn, Top = 7, Wait = true, Items = new [] { "Inheritance", "Virtual methods", "Abstract methods" } },
                new TextElement() { X = Theme.RightColumn, Y = 14, Text = "Memory", Color = Theme.Accent, Wait = true },
                new BulletList() { Left = Theme.RightColumn, Top = 16, Items = new [] { "Garbage collector" } },
            }
        };
    }
}
