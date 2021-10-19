using System.Security.Cryptography.X509Certificates;
using C64Lib;
using C64Presentation.Animations;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides
{
    class MemoryLayoutSlide
    {

        public static Slide Create()
        {
            uint x = 0;
            uint y = 0;
            return new SimpleSlide()
            {
                Elements = new SlideElement[]
                {
                    new Title() {Text = "MEMORY LAYOUT"},

                    new TextElement() { X = 15, Y = 5, Text = "OBJECT TABLE", Color = Colors.Cyan },
                    new Box() { X = x, Y = y=7, Width = 5, Height = 3, Text = "  1", Color = Colors.Cyan },
                    new Box() { X = x+=4, Y = y, Width = 10, Height = 3, Text = "HEAP PTR", Color = Colors.Cyan, ConnectToLeft = true },
                    new Box() { X = x+=9, Y = y, Width = 6, Height = 3, Text = "SIZE", Color = Colors.Cyan, ConnectToLeft = true },
                    new Box() { X = x+=5, Y = y, Width = 6, Height = 3, Text = "REF.", Color = Colors.Cyan, ConnectToLeft = true },
                    new Box() { X = x+=5, Y = y, Width = 6, Height = 3, Text = "ROOT", Color = Colors.Cyan, ConnectToLeft = true },
                    new Box() { X = x+=5, Y = y, Width = 10, Height = 3, Text = "VTABLE", Color = Colors.Cyan, ConnectToLeft = true },

                    new Box() { X = x=0, Y = y=10, Width = 5, Height = 3, Text = "  2", Color = Colors.Cyan },
                    new Box() { X = x+=4, Y = y, Width = 10, Height = 3, Text = "", Color = Colors.Cyan, ConnectToLeft = true },
                    new Box() { X = x+=9, Y = y, Width = 6, Height = 3, Text = "", Color = Colors.Cyan, ConnectToLeft = true },
                    new Box() { X = x+=5, Y = y, Width = 6, Height = 3, Text = "", Color = Colors.Cyan, ConnectToLeft = true },
                    new Box() { X = x+=5, Y = y, Width = 6, Height = 3, Text = "", Color = Colors.Cyan, ConnectToLeft = true },
                    new Box() { X = x+=5, Y = y, Width = 10, Height = 3, Text = "", Color = Colors.Cyan, ConnectToLeft = true },

                    new TextElement() { X = 17, Y = 13, Text = ". . .", Color = Colors.Cyan },

                    new Box() { X = x=0, Y = y=14, Width = 5, Height = 3, Text = "255", Color = Colors.Cyan },

                    new Box() { X = x+=4, Y = y, Width = 10, Height = 3, Text = "", Color = Colors.Cyan, ConnectToLeft = true },
                    new Box() { X = x+=9, Y = y, Width = 6, Height = 3, Text = "", Color = Colors.Cyan, ConnectToLeft = true },
                    new Box() { X = x+=5, Y = y, Width = 6, Height = 3, Text = "", Color = Colors.Cyan, ConnectToLeft = true },
                    new Box() { X = x+=5, Y = y, Width = 6, Height = 3, Text = "", Color = Colors.Cyan, ConnectToLeft = true },
                    new Box() { X = x+=5, Y = y, Width = 10, Height = 3, Text = "", Color = Colors.Cyan, ConnectToLeft = true },

                    new TextElement() { X = 18, Y = 18, Text = "HEAP", Color = Colors.Yellow },
                    new Box() { X = x=0, Y = y=20, Width = 6, Height = 5, Text = "1", Color = Colors.Yellow },
                    new Box() { X = x+=5, Y = y, Width = 10, Height = 5, Text = "3", Color = Colors.Yellow, ConnectToLeft = true },
                    new Box() { X = x+=9, Y = y, Width = 6, Height = 5, Text = "2", Color = Colors.Yellow, ConnectToLeft = true },
                    new Box() { X = x+=5, Y = y, Width = 3, Height = 5, Text = "5", Color = Colors.Yellow, ConnectToLeft = true },
                    new Box() { X = x+=2, Y = y, Width = 19, Height = 5, Text = "FREE", Color = Colors.Yellow, ConnectToLeft = true },

                },
                // Animation = new RandomDisappearAnimation()
            };
        }
    }
}
