using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements
{
    class Box : SlideElement
    {
        public uint X, Y, Width, Height;
        public string Text;
        public Colors TextColor;
        public Colors BorderColor;

        public Colors Color
        {
            get => TextColor;
            set
            {
                TextColor = value;
                BorderColor = value;
            }
        }

        public bool WaitAfter;
        public bool ConnectToLeft;
        public bool ConnectToTop;

        private const uint TL_CORNER = 0x70;
        private const uint TL_CORNER_CONNECT_LEFT = 0x72;
        private const uint TL_CORNER_CONNECT_TOP = 0x6B;
        private const uint TR_CORNER = 0x6E;
        private const uint TR_CORNER_TOP = 0x73;
        private const uint BL_CORNER = 0x6D;
        private const uint BL_CORNER_CONNECT = 0x71;
        private const uint BR_CORNER = 0x7D;
        private const uint CROSS = 0x5B;
        private const uint H_LINE = 0x43;
        private const uint V_LINE = 0x5D;

        public override void Draw()
        {
            var tlCorner = TL_CORNER;
            if (ConnectToTop && ConnectToLeft)
                tlCorner = CROSS;
            else if (ConnectToLeft)
                tlCorner = TL_CORNER_CONNECT_LEFT;
            else if (ConnectToTop)
                tlCorner = TL_CORNER_CONNECT_TOP;

            C64.SetChar(X, Y, tlCorner, BorderColor);
            C64.SetChar(X + Width - 1, Y,  ConnectToTop ? TR_CORNER_TOP : TR_CORNER, BorderColor);
            C64.SetChar(X, Y + Height - 1,  ConnectToLeft ? BL_CORNER_CONNECT : BL_CORNER, BorderColor);
            C64.SetChar(X + Width - 1, Y + Height - 1, BR_CORNER, BorderColor);

            for (uint x = X + 1; x < Width - 1 + X; x++)
            {
                C64.SetChar(x, Y, H_LINE, BorderColor);
                C64.SetChar(x, Y + Height - 1, H_LINE, BorderColor);
            }

            for (uint y = Y + 1; y < Height - 1 + Y; y++)
            {
                C64.SetChar(X, y, V_LINE, BorderColor);
                C64.SetChar(X + Width - 1, y, V_LINE, BorderColor);
            }

            if (Text != null)
            {
                int length = Text.Length;
                uint startX = ((Width - (uint)length) >> 1) + (uint)X;
                uint startY = Y + ((Height-1) >> 1);

                C64.Write(startX, startY, Text, TextColor);
            }

            if (WaitAfter)
            {
                KeyBoard.WaitForKeys();
            }

        }
    }
}

