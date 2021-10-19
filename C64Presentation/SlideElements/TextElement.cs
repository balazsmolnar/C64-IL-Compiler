using System.ComponentModel;
using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements
{
    class TextElement : SlideElement
    {
        public string Text;
        public uint X, Y;
        public Colors Color;
        public override void Draw()
        {
            C64.Write(X, Y, Text, Color);
        }
    }
}