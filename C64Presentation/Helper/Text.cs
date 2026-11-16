using C64Lib;

namespace C64Presentation.Helper;

// Typography. Four faces from art/font (see Theme for the colours):
//   Draw     body text: 8 px proportional sans, one cell row
//   Heading  titles and key messages: 16 px bold, two cell rows
//   Big      the title face doubled and smoothed: 32 px, four cell rows
//   Code     a fixed 6 px pitch with a blank top pixel line, so code lines
//            sit on consecutive rows; a colour change moves on to the next
//            cell edge (a cell has one colour), so pad with spaces to line
//            up columns after one
// x is in pixels, row in cells; each returns the x after the text. Strings
// may change colour inline (Theme.Hi ... Theme.Off). Special glyphs in every
// face: Theme.Arrow, Theme.Dot, Theme.Tick.
static class Text
{
    static ulong BodyFont => C64Address.FromLabel("font_Body");
    static ulong HeadingFont => C64Address.FromLabel("font_Title");
    static ulong BigFont => C64Address.FromLabel("font_TitleBig");
    static ulong CodeFont => C64Address.FromLabel("font_BodyMono");

    public static ulong Draw(ulong x, uint row, string text, Colors fg, Colors bg = Theme.Background)
        => Put(x, row, text, BodyFont, fg, bg, 1);

    public static ulong Heading(ulong x, uint row, string text, Colors fg = Theme.Heading, Colors bg = Theme.Background)
        => Put(x, row, text, HeadingFont, fg, bg, 2);

    public static ulong Big(ulong x, uint row, string text, Colors fg = Theme.Heading, Colors bg = Theme.Background)
        => Put(x, row, text, BigFont, fg, bg, 4);

    public static ulong Code(uint column, uint row, string text, Colors fg = Theme.Heading, Colors bg = Theme.Code)
        => Put(Screen.Px(column), row, text, CodeFont, fg, bg, 1);

    public static ulong Width(string text) => C64.Screen.FontTextWidth(text, BodyFont);

    public static ulong HeadingWidth(string text) => C64.Screen.FontTextWidth(text, HeadingFont);

    public static ulong BigWidth(string text) => C64.Screen.FontTextWidth(text, BigFont);

    // Centred between pixel x and x + width; text wider than that starts at
    // x. (Unsigned maths: an unchecked width - textWidth would wrap to some
    // 65,000 pixels and draw over memory.)
    public static void Center(ulong x, ulong width, uint row, string text, Colors fg, Colors bg = Theme.Background)
        => Draw(x + Margin(width, Width(text)), row, text, fg, bg);

    public static void CenterHeading(ulong x, ulong width, uint row, string text, Colors fg = Theme.Heading, Colors bg = Theme.Background)
        => Heading(x + Margin(width, HeadingWidth(text)), row, text, fg, bg);

    // Ends at pixel x (the letter spacing after the last glyph not counted);
    // text wider than x starts at 0.
    public static void Right(ulong x, uint row, string text, Colors fg, Colors bg = Theme.Background)
    {
        ulong width = Width(text);
        Draw(width <= x + 1 ? x + 1 - width : 0, row, text, fg, bg);
    }

    // Body text built at run time ("3 / 8", an interpolation): drawn at
    // once, never kept by a build step for its fade-in. Such a string lives
    // in the compiler's one scratch buffer, so a build mark holding it would
    // later show whatever text was built last (the compiler rejects that
    // with C64007). Call these outside a build step.
    public static ulong DrawNow(ulong x, uint row, string text, Colors fg, Colors bg = Theme.Background)
    {
        Deck.CheckRows(row, 1);
        return C64.Screen.DrawFontText(x, row, text, BodyFont, Theme.Matrix(fg, bg));
    }

    public static void CenterNow(ulong x, ulong width, uint row, string text, Colors fg, Colors bg = Theme.Background)
        => DrawNow(x + Margin(width, Width(text)), row, text, fg, bg);

    public static void RightNow(ulong x, uint row, string text, Colors fg, Colors bg = Theme.Background)
    {
        ulong width = Width(text);
        DrawNow(width <= x + 1 ? x + 1 - width : 0, row, text, fg, bg);
    }

    static ulong Margin(ulong space, ulong used) => used < space ? (space - used + 1) / 2 : 0;

    static ulong Put(ulong x, uint row, string text, ulong font, Colors fg, Colors bg, uint rows)
    {
        Deck.CheckRows(row, rows);
        if (Deck.Leaving())
            return x;
        if (!Deck.Building)
            return C64.Screen.DrawFontText(x, row, text, font, Theme.Matrix(fg, bg));

        // A build step: hide the cells, draw without colour, reveal later.
        uint first = (uint)(x / Theme.CellPixels);
        uint cells = (uint)((x + C64.Screen.FontTextWidth(text, font) + 6) / Theme.CellPixels) - first;
        if (cells != 0)
            Deck.PaintText(first, row, cells, rows, fg, bg, text, x, font);
        return C64.Screen.DrawFontText(x, row, text, font, 0);
    }
}
