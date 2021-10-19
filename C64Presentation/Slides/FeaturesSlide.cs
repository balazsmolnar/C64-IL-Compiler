using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C64Lib;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides
{
    class FeaturesSlide
    {
        public static Slide Create()
        {
            return new SimpleSlide()
            {
                Elements = new SlideElement[]
                {
                    new Title() { Text = "FEATURES" },
                    new BulletList() { Items = new [] { "DATA TYPES" }, Top = 6, Left = 0, Color = Colors.White },
                    new BulletList() { Items = new [] { "INT, UINT (8 BIT)", "LONG, ULONG (16 BIT)", "STRING (MAX 255 BYTES)", "OBJECT (255)", "ARRAY (MAX 255 ITEMS)" }, Top = 6, Left = 14, Wait = true, Color = Colors.Green, Symbol = 0x57},
                    new BulletList() { Items = new [] { "OO" }, Top = 17, Left = 0, Color = Colors.White },
                    new BulletList() { Items = new [] { "INHERITANCE", "VIRTUAL METHODS", "ABSTRACT METHODS" }, Top = 17, Left = 14, Wait = true, Color = Colors.Green, Symbol = 0x57 },
                    new BulletList() { Items = new [] { "GARBAGE COLLECTOR" }, Top = 24, Left = 0, Color = Colors.White },
                }
            };

        }
    }
}
