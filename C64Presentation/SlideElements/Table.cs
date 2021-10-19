
using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements
{
    class Table: SlideElement
    {
        public uint X, Y, RowNum, CollNum, CellWidth, CellHeight;
        public string[] Texts;
        public Colors TextColor;
        public Colors BorderColor;

        public bool WaitAfterRow;

        public override void Draw()
        {
            int textIndex = 0;
            uint y = Y;

            for (uint row = 0; row < RowNum; row++)
            {
                uint x = X;
                for (uint col = 0; col < CollNum; col++)
                {
                    var box = new Box()
                    {
                        X = x,
                        Y = y,
                        ConnectToTop = row != 0,
                        ConnectToLeft = col != 0,
                        TextColor = TextColor,
                        BorderColor = BorderColor,
                        Height = CellHeight,
                        Width = CellWidth,
                        Text = Texts[textIndex++]
                    };

                    box.Draw();
                    x += CellWidth-1;
                }

                if (WaitAfterRow)
                    KeyBoard.WaitForKeys();

                y += CellHeight-1;
            }
        }
    }
}
