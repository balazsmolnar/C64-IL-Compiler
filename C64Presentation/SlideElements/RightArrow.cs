using System;
using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements
{
    class RightArrow : SlideElement
    {
        public uint X, Y, Width;
        public string Text1;
        public string Text2;

        private const uint H_LINE = 0x43;
        private const uint ARROW_HEAD = 0x3E;

        public override void Draw()
        {
            // Compiler bug
            uint a = X;
            for (uint x = a; x <= X + Width - 1; x++)
            {
                C64.SetChar(x, Y, H_LINE);
                Delay.Wait(100);
            }
            C64.SetChar(X + Width - 1, Y, ARROW_HEAD);

            if (Text1 != null)
            {
                int length = Text1.Length;
                uint startX = ((Width - (uint)length) >> 1) + (uint)X;

                C64.Write(startX, Y - 1, Text1);
            }

            if (Text2 != null)
            {
                int length = Text2.Length;
                uint startX = ((Width - (uint)length) >> 1) + (uint)X;

                C64.Write(startX, Y + 1, Text2);
            }
        }
    }
}