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
    public uint X { get; init; }
    public uint Y { get; init; }
    public uint RowNum { get; init; }
    public uint CollNum { get; init; }
    public uint CellWidth { get; init; }
    public uint[] ColumnWidths { get; init; }
    public string[] Texts { get; init; }
    public bool WaitAfterRow { get; init; }

    public override void Draw()
    {
        uint columns = CollNum;
        uint[] widths = ColumnWidths;
        if (widths == null)
        {
            widths = new uint[columns];
            for (uint c = 0; c < columns; c++)
                widths[c] = Theme.OrCells(CellWidth, 9);
        }
        Render(Theme.OrCells(X, Theme.Left), Y, RowNum, columns, widths, Texts, WaitAfterRow);
    }

    static void Render(uint left, uint y, uint rows, uint columns, uint[] widths, string[] texts, bool wait)
    {
        uint total = 0;
        for (uint c = 0; c < columns; c++)
            total += widths[c];
        uint index = 0;
        for (uint r = 0; r < rows; r++)
        {
            bool header = r == 0;
            if (!header && wait)
                Deck.Step();
            Colors bg = header ? Theme.Card : Theme.Background;
            if (header)
                Deck.Paint(left, y, total, 1, Theme.Heading, Theme.Card);
            ulong x = Helper.Screen.X(left);
            for (uint c = 0; c < columns; c++)
            {
                ulong w = Helper.Screen.X(widths[c]);
                if (c == 0)
                    Text.Draw(x + 6, y, texts[index], header ? Theme.Heading : Theme.Body, bg);
                else
                    Text.Right(x + w - 7, y, texts[index], Theme.Heading, bg);
                index++;
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
}
