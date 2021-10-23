using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements
{
    class BulletList : SlideElement
    {
        public bool Wait;
        public string[] Items;
        public uint Top;
        public uint Left;
        public Colors Color;
        public uint Symbol;

        public override void Draw()
        {
            var y = Top;
            if (Symbol == 0)
                Symbol = 0x51;

            foreach (var item in Items)
            {
                C64.SetChar(Left, y, Symbol, Color);
                C64.Write(Left + 2, y, item, Color);
                y += 2;

                if (Wait)
                    KeyBoard.WaitForKeys();

            }
        }
    }
}