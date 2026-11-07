using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// A table: a Header row (white on black, a 2 px cyan rule under it), then
// the Rows, each followed by a Grey2 hairline; two cell rows per row.
// Widths: each column's width in cells. The first column is left aligned
// in Grey3, the others right aligned in white (numbers). WaitAfterRow:
// each row after the header is a build step. X: Auto = Theme.Left.
//
//     new Table
//     {
//         Y = 10, Widths = [9, 8, 9, 10], WaitAfterRow = true,
//         Header = ["", "C64", "PC", "Ratio"],
//         Rows = [["Memory", "64 KB", "32 GB", "x524,288"], ["CPU", "1 MHz", "8x1.8 GHz", "x14,400"]],
//     }
sealed class Table : SlideElement
{
    const ulong Padding = 6;

    public uint X { get; init; }
    public uint Y { get; init; }
    public required uint[] Widths { get; init; }
    public required string[] Header { get; init; }
    public required string[][] Rows { get; init; }
    public bool WaitAfterRow { get; init; }

    public override void Draw()
    {
        uint left = Theme.OrCells(X, Theme.Left);
        uint total = 0;
        foreach (var width in Widths)
            total += width;
        uint row = Y;
        DrawRow(left, row, Header, Theme.Heading);
        Rule(left, row + 1, total, Theme.Accent, 2, 0);
        foreach (var cells in Rows)
        {
            row += 2;
            if (WaitAfterRow)
                Deck.Step();
            DrawRow(left, row, cells, Theme.Body);
            Rule(left, row + 1, total, Theme.Muted, 1, 3);
        }
    }

    void DrawRow(uint left, uint row, string[] cells, Colors firstColumn)
    {
        ulong x = Screen.Px(left);
        for (uint c = 0; c < cells.Length; c++)
        {
            ulong width = Screen.Px(Widths[c]);
            if (c == 0)
                Text.Draw(x + Padding, row, cells[c], firstColumn);
            else
                Text.Right(x + width - Padding - 1, row, cells[c], Theme.Heading);
            x += width;
        }
    }

    // A rule `thickness` px tall, `offset` px down into its cell row.
    static void Rule(uint left, uint row, uint width, Colors color, ulong thickness, ulong offset)
    {
        Deck.Paint(left, row, width, 1, color, Theme.Background);
        ulong y = Screen.Px(row) + offset;
        Screen.Fill(Screen.Px(left), y, Screen.Px(left + width) - 1, y + thickness - 1);
    }
}
