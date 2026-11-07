using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// An arrow from the middle of one cell to the middle of another, in any
// direction: straight, or an L (along the row first, then the column; or
// column first with VerticalFirst). The head points along the last leg.
// Text labels it: above a horizontal first leg, else beside the vertical
// one, on whichever side stays inside the content columns. After a build
// step it is drawn live and grows from its start.
//
//     new Arrow { FromCol = 10, FromRow = 7, ToCol = 14, ToRow = 7, Text = "dotnet build", Wait = true }
//     new Arrow { FromCol = 33, FromRow = 9, ToCol = 26, ToRow = 14 }   // down, then left
class Arrow : SlideElement
{
    public uint FromCol { get; init; }
    public uint FromRow { get; init; }
    public uint ToCol { get; init; }
    public uint ToRow { get; init; }
    public bool VerticalFirst { get; init; }
    public string? Text { get; init; }
    public Colors Color { get; init; }
    public bool Wait { get; init; }

    public override void Draw()
    {
        if (Wait)
            Deck.Step();
        Deck.Show();                // after a build step it is drawn live
        uint cornerCol = VerticalFirst ? FromCol : ToCol;
        uint cornerRow = VerticalFirst ? ToRow : FromRow;
        var color = Theme.Or(Color, Theme.Accent);
        Leg(FromCol, FromRow, cornerCol, cornerRow, color);
        Leg(cornerCol, cornerRow, ToCol, ToRow, color);
        // The head points away from the last leg's start: the corner, or
        // the arrow's start when it is straight.
        bool straight = cornerCol == ToCol && cornerRow == ToRow;
        Head(straight ? FromCol : cornerCol, straight ? FromRow : cornerRow, ToCol, ToRow);
        if (Text != null)
            Label(Text, FromCol, FromRow, cornerCol, cornerRow, ToRow);
    }

    static ulong X(uint column) => Helper.Screen.Px(column) + 3;

    static ulong Y(uint row) => Helper.Screen.Px(row) + 3;   // 4 px under a label in the row above

    static uint Min(uint a, uint b) => a < b ? a : b;

    static uint Span(uint a, uint b) => (a < b ? b - a : a - b) + 1;

    // One straight leg, a cell per frame when it can be seen.
    static void Leg(uint c0, uint r0, uint c1, uint r1, Colors color)
    {
        if (c0 == c1 && r0 == r1)
            return;
        Deck.Paint(Min(c0, c1), Min(r0, r1), Span(c0, c1), Span(r0, r1), color, Theme.Background);
        ulong x = X(c0), y = Y(r0), x1 = X(c1), y1 = Y(r1);
        while (x != x1 || y != y1)
        {
            ulong nx = Toward(x, x1, 8), ny = Toward(y, y1, 8);
            Helper.Screen.Line(x, y, nx, ny);
            x = nx;
            y = ny;
            if (Deck.Live)
                C64.Screen.WaitForVBlank();
        }
    }

    // The head at (c1, r1), pointing away from (c0, r0).
    static void Head(uint c0, uint r0, uint c1, uint r1)
    {
        ulong x = X(c1), y = Y(r1);
        if (r0 == r1)
        {
            ulong back = Toward(x, X(c0), 3);
            Helper.Screen.Line(back, y - 3, x, y);
            Helper.Screen.Line(back, y + 3, x, y);
        }
        else
        {
            ulong back = Toward(y, Y(r0), 3);
            Helper.Screen.Line(x - 3, back, x, y);
            Helper.Screen.Line(x + 3, back, x, y);
        }
    }

    static void Label(string text, uint fromCol, uint fromRow, uint cornerCol, uint cornerRow, uint toRow)
    {
        if (cornerRow == fromRow && cornerCol != fromCol)
        {
            uint left = Min(fromCol, cornerCol);
            Helper.Text.Center(Helper.Screen.Px(left), Helper.Screen.Px(Span(fromCol, cornerCol)), fromRow - 1, text, Theme.Muted);
            return;
        }
        uint row = Min(fromRow, toRow) + (Span(fromRow, toRow) >> 1);
        ulong beside = Helper.Screen.Px(cornerCol + 1) + 4;
        if (beside + Helper.Text.Width(text) > Helper.Screen.Px(Theme.Right))
            Helper.Text.Right(Helper.Screen.Px(cornerCol) - 5, row, text, Theme.Muted);
        else
            Helper.Text.Draw(beside, row, text, Theme.Muted);
    }

    // `from` moved toward `to` by at most `step`.
    static ulong Toward(ulong from, ulong to, ulong step) => from < to
        ? (to - from > step ? from + step : to)
        : (from - to > step ? from - step : to);
}
