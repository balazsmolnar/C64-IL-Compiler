using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// A table: a header row on a card (white text), then the rows, each
// followed by a hairline. Texts are row by row, the header first. Column
// widths in cells: ColumnWidths, or CellWidth (default 9) for all. The
// first column is left aligned (Body colour), the others right aligned
// (numbers; white). WaitAfterRow builds it row by row. X default Theme.Left.
//
//     new Table { Y = 10, CollNum = 3, RowNum = 3, ColumnWidths = new uint[] { 16, 10, 10 },
//                 Texts = new[] { "", "Before", "After", "Hunchback", "36,847", "28,157", ... } }
class Table : SlideElement
{
    public uint X, Y, RowNum, CollNum;
    public uint CellWidth;
    public uint[] ColumnWidths;
    public string[] Texts;
    public bool WaitAfterRow;

    public override void Draw()
    {
        uint left = X == 0 ? Theme.Left : X;
        uint total = 0;
        for (uint c = 0; c < CollNum; c++)
            total += Column(c);

        uint index = 0;
        uint y = Y;
        for (uint r = 0; r < RowNum; r++)
        {
            if (r != 0 && WaitAfterRow)
                Deck.Step();
            bool header = r == 0;
            Colors bg = header ? Theme.Card : Theme.Background;
            if (header)
                Deck.Paint(left, y, total, 1, Theme.Accent, Theme.Card);
            ulong x = Helper.Screen.X(left);
            for (uint c = 0; c < CollNum; c++)
            {
                ulong w = Helper.Screen.X(Column(c));
                string text = Texts[index++];
                Colors fg = header ? Theme.Heading : (c == 0 ? Theme.Body : Theme.Heading);
                if (c == 0)
                    Text.Draw(x + 6, y, text, fg, bg);
                else
                    Text.Right(x + w - 7, y, text, fg, bg);
                x += w;
            }
            if (!header)
            {
                Deck.Paint(left, y + 1, total, 1, Theme.Muted, Theme.Background);
                ulong ly = Helper.Screen.X(y + 1) + 3;
                Helper.Screen.Line(Helper.Screen.X(left), ly, Helper.Screen.X(left + total) - 1, ly);
            }
            y += 2;
        }
    }

    uint Column(uint c) => ColumnWidths != null ? ColumnWidths[c] : (CellWidth == 0 ? 9 : CellWidth);
}
