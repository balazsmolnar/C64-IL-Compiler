using C64Lib;

namespace C64Presentation.Helper;

// Pixel drawing on the hi-res bitmap. These set or clear pixels only; the
// cells' colours come from Deck.Paint.
static class Screen
{
    public const uint Columns = 40;
    public const uint Rows = 25;

    // The pixel coordinate of a cell column or row.
    public static ulong Px(uint cells) => (ulong)cells * Theme.CellPixels;

    // The first cell edge at least gap pixels after pixel x: where something
    // in another colour can start (a cell has one foreground colour).
    public static ulong CellAfter(ulong x, ulong gap) => (x + gap + Theme.CellPixels - 1) / Theme.CellPixels * Theme.CellPixels;

    // Clears the pixels of a rectangle of cells (its colours stay).
    public static void ClearRect(uint column, uint row, uint width, uint height)
    {
        ulong x = Px(column), y = Px(row);
        C64.Screen.DrawRectangle(x, y, x + Px(width) - 1, y + Px(height) - 1, true, false);
    }

    // A solid block of pixels, corners inclusive.
    public static void Fill(ulong x0, ulong y0, ulong x1, ulong y1) => C64.Screen.DrawRectangle(x0, y0, x1, y1, true);

    // A 1 px frame, corners inclusive.
    public static void Frame(ulong x0, ulong y0, ulong x1, ulong y1) => C64.Screen.DrawRectangle(x0, y0, x1, y1);

    public static void Line(ulong x0, ulong y0, ulong x1, ulong y1) => C64.Screen.DrawLine(x0, y0, x1, y1);
}
