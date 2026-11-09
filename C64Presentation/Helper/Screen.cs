using C64Lib;

namespace C64Presentation.Helper;

// Pixel drawing on the hi-res bitmap. These set or clear pixels only; the
// cells' colours come from Deck.Paint. (ulong maths: * 8 and / 8, the
// compiler has no 16-bit shifts.)
class Screen
{
    public const int Width = 40;
    public const int Height = 25;

    // Clears the pixels of a rectangle of cells (its colours stay).
    public static void ClearRect(uint column, uint row, uint width, uint height)
    {
        ulong x = (ulong)column * 8;
        ulong y = (ulong)row * 8;
        C64.Screen.DrawRectangle(x, y, x + (ulong)width * 8 - 1, y + (ulong)height * 8 - 1, true, false);
    }

    // A solid block of pixels, corners inclusive.
    public static void Fill(ulong x0, ulong y0, ulong x1, ulong y1)
        => C64.Screen.DrawRectangle(x0, y0, x1, y1, true);

    public static void Line(ulong x0, ulong y0, ulong x1, ulong y1)
        => C64.Screen.DrawLine(x0, y0, x1, y1);

    // Pixel x of a cell column (and y of a cell row).
    public static ulong X(uint column) => (ulong)column * 8;
}
