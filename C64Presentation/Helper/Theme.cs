using C64Lib;

// The fonts (art/font/make_fonts.py), the logo sprites (art/engine/make_logos.py)
// and the fade tables: level k (0-4) of colour c is fade_Levels[k*16 + c];
// level 0 is black, level 4 the colour itself (see Deck).
[assembly: RawAssembly(Order = 1, Resource = "fonts.asm")]
[assembly: RawSprites(Resource = "logos.spr", Label = "Logo")]
[assembly: RawAssembly(Order = 2, Asm = @"
fade_Levels
    .byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
    .byte 0,11,0,6,0,0,0,9,0,0,9,11,11,11,0,11
    .byte 0,12,9,11,6,11,0,12,9,0,2,11,11,5,6,11
    .byte 0,15,9,14,11,12,6,15,9,9,8,11,11,3,4,12
    .byte 0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15
")]

namespace C64Presentation.Helper;

// The deck's look: one palette and one grid, used by every element.
//
// Screen: hi-res bitmap, 40 x 25 cells of 8 x 8 pixels, two colours per cell.
//
//   rows 1-2    title (Title font, 16 px)          Master.Content
//   row  3      accent rule
//   rows 5-20   body                               BodyTop .. BodyBottom-1
//   row  23     footer: deck title, "n / total"
//   row  24     progress bar
//   columns 2-37 content (16 px margins); two columns: 2-18 and 21-37
//
// Element fields left at 0 / Colors.Black / null take the theme's default
// (the compiler has no field initializers).
static class Theme
{
    // Palette.
    public const Colors Background = Colors.Black;
    public const Colors Card = Colors.Grey1;        // panels, boxes, table header
    public const Colors Code = Colors.Blue;         // code panels: the C64's own blue, on purpose
    public const Colors Heading = Colors.White;     // titles, emphasis
    public const Colors Body = Colors.Grey3;        // running text
    public const Colors Muted = Colors.Grey2;       // captions, footer, secondary
    public const Colors Accent = Colors.Cyan;       // rules, bullets, highlights, bars
    public const Colors Accent2 = Colors.Yellow;    // a second highlight (numbers, "after")
    public const Colors Good = Colors.LightGreen;
    public const Colors Bad = Colors.LightRed;

    // Inline colour codes for any text ("`" + colour digit, see
    // C64.Screen.DrawFontText); C# folds constant strings, so
    // Theme.Hi + "word" + Theme.Off costs nothing at run time. In
    // proportional text a colour change moves on to the next cell.
    public const string Hi = "`3";     // Accent
    public const string Hi2 = "`7";    // Accent2
    public const string White = "`1";
    public const string Off = "`?";    // back to Body
    public const string Dim = "`<";    // Muted
    public const string Red = "`:";    // Bad
    public const string Green = "`=";  // Good

    // Syntax colours for CodeBlock lines (free at a fixed pitch), chosen
    // for the blue code panel: comments in the C64's light blue.
    public const string Kw = "`3";     // keywords: cyan
    public const string Ty = "`1";     // types, names: white
    public const string Str = "`=";    // strings: light green
    public const string Num = "`7";    // numbers: yellow
    public const string Com = "`>";    // comments: light blue
    public const string Pl = "`?";     // plain: light grey

    // Glyphs: "~" is an arrow, "\\" a bullet dot, "^" a tick (both fonts).
    public const string Arrow = "~";
    public const string Dot = "\\";
    public const string Tick = "^";

    // Grid (cells).
    public const uint Left = 2;
    public const uint Right = 38;          // first column past the content
    public const uint Width = 36;
    public const uint TitleRow = 1;
    public const uint RuleRow = 3;
    public const uint BodyTop = 5;
    public const uint BodyBottom = 21;
    public const uint FooterRow = 23;
    public const uint LeftColumn = 2;      // two-column layout
    public const uint RightColumn = 21;
    public const uint ColumnWidth = 17;

    // A cell's colour byte: fg for set pixels, bg for the rest.
    public static uint Matrix(Colors fg, Colors bg)
    {
        uint f = (uint)fg;
        uint b = (uint)bg;
        return (f << 4) | b;
    }

    // A colour field left at 0 (black) means "the default".
    public static Colors Or(Colors color, Colors fallback) => color == Colors.Black ? fallback : color;
}
