using C64Lib;

namespace C64Presentation.Helper;

// Typography. Three faces from art/font (see Theme for the colours):
//   Draw     body text: 8 px proportional sans, one cell row
//   Heading  titles: 16 px bold, two cell rows
//   Code     the body glyphs at a fixed 8 px pitch, one glyph per cell
// x is in pixels, row in cells; each returns the x after the text. Strings
// may change colour inline (Theme.Hi ... Theme.Off). Special glyphs in both
// fonts: Theme.Arrow "~", Theme.Dot "\\", Theme.Tick "^".
static class Text
{
    public static ulong Draw(ulong x, uint row, string text, Colors fg, Colors bg = Theme.Background)
        => Put(x, row, text, C64Address.FromLabel("font_Body"), fg, bg, 1);

    public static ulong Heading(ulong x, uint row, string text, Colors fg = Theme.Heading, Colors bg = Theme.Background)
        => Put(x, row, text, C64Address.FromLabel("font_Title"), fg, bg, 2);

    public static ulong Code(uint column, uint row, string text, Colors fg = Theme.Body, Colors bg = Theme.Code)
        => Put((ulong)column * 8, row, text, C64Address.FromLabel("font_BodyMono"), fg, bg, 1);

    public static ulong Width(string text) => C64.Screen.FontTextWidth(text, C64Address.FromLabel("font_Body"));

    public static ulong HeadingWidth(string text) => C64.Screen.FontTextWidth(text, C64Address.FromLabel("font_Title"));

    // Centred between pixel x and x + width.
    public static void Center(ulong x, ulong width, uint row, string text, Colors fg, Colors bg = Theme.Background)
        => Draw(x + (width - Width(text) + 1) / 2, row, text, fg, bg);

    public static void CenterHeading(ulong x, ulong width, uint row, string text, Colors fg = Theme.Heading, Colors bg = Theme.Background)
        => Heading(x + (width - HeadingWidth(text) + 2) / 2, row, text, fg, bg);

    // Ends at pixel x (the letter spacing after the last glyph not counted).
    public static void Right(ulong x, uint row, string text, Colors fg, Colors bg = Theme.Background)
        => Draw(x + 1 - Width(text), row, text, fg, bg);

    static ulong Put(ulong x, uint row, string text, ulong font, Colors fg, Colors bg, uint rows)
    {
        if (!Deck.Building)
            return C64.Screen.DrawFontText(x, row, text, font, Theme.Matrix(fg, bg));

        // A build step: hide the cells, draw without colour, reveal later.
        uint first = (uint)(x / 8);
        uint cells = (uint)((x + C64.Screen.FontTextWidth(text, font) + 6) / 8) - first;
        if (cells != 0)
            Deck.PaintText(first, row, cells, rows, fg, bg, x, text, font);
        return C64.Screen.DrawFontText(x, row, text, font, 0);
    }
}
