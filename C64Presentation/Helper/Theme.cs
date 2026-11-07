using C64Lib;

// The fonts (art/font/make_fonts.py), the logo sprites (art/engine/make_logos.py)
// and the fade tables: level k (0-4) of colour c is fade_Levels[k*16 + c];
// level 0 is black, level 4 the colour itself (see Deck). Every colour steps
// down by brightness through the greys (no hue shifts: cyan goes
// 15 -> 12 -> 11), and the Grey1 surface is always darker than any text on
// it: 0 at level 1, 11 from level 2, while text is 11 / 12 / 12-15.
[assembly: RawAssembly(Order = 1, Resource = "fonts.asm")]
[assembly: RawSprites(Resource = "logos.spr", Label = "Logo")]
[assembly: RawAssembly(Order = 2, Asm = """
fade_Levels
    .byte 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
    .byte 0,11,0,11,0,0,0,11,0,0,11,0,0,11,0,11
    .byte 0,12,0,12,11,11,0,12,11,0,11,11,11,12,11,12
    .byte 0,15,11,15,11,12,11,15,11,11,12,11,12,15,12,12
    .byte 0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15
""")]

namespace C64Presentation.Helper;

// The deck's look: one palette and one grid, used by every element. The
// palette is art/ART.md's: black, Grey1 surface, Grey2 rules, Grey3 body,
// white titles, cyan accent, yellow second accent, violet for C#/.NET.
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
// Text on a Grey1 surface is white (code comments excepted).
static class Theme
{
    // Palette.
    public const Colors Background = Colors.Black;
    public const Colors Card = Colors.Grey1;        // surface: cards, panels, table header
    public const Colors Code = Colors.Black;        // code panels: black in a Grey1 frame, for contrast
    public const Colors Frame = Colors.Grey1;       // frames, tracks
    public const Colors Heading = Colors.White;     // titles, text on cards
    public const Colors Body = Colors.Grey3;        // running text
    public const Colors Muted = Colors.Grey2;       // rules, captions, footer, inactive
    public const Colors Accent = Colors.Cyan;       // rules, bullets, highlights, bars
    public const Colors Accent2 = Colors.Yellow;    // a second accent, sparingly
    public const Colors Brand = Colors.Violet;      // C# / .NET marks only
    public const Colors Bad = Colors.LightRed;      // errors

    // Inline colour codes ("`" + colour, see C64.Screen.DrawFontText). C#
    // folds constant strings: $"a {Theme.Hi}b" costs nothing at run time.
    // In proportional text a colour change moves on to the next cell.
    public const string Hi = "`3";     // Accent
    public const string Hi2 = "`7";    // Accent2
    public const string White = "`1";
    public const string Off = "`?";    // back to Body
    public const string Dim = "`<";    // Muted
    public const string Red = "`:";    // Bad

    // Syntax colours for code (free at the fixed pitch), on the black code panel.
    public const string Kw = "`3";     // keywords, mnemonics: cyan
    public const string Ty = "`1";     // types, names, plain code: white
    public const string Str = "`7";    // strings, numbers: yellow
    public const string Num = "`7";
    public const string Com = "`?";    // comments, addresses: Grey3
    public const string Pl = "`1";     // plain: white

    // Glyphs in every face: an arrow, a bullet dot, a tick.
    public const string Arrow = "~";
    public const string Dot = "\\";
    public const string Tick = "^";

    // Grid (cells).
    public const uint Left = 2;
    public const uint Right = 38;          // first column past the content
    public const uint Width = Right - Left;
    public const uint TitleRow = 1;
    public const uint RuleRow = 3;
    public const uint BodyTop = 5;
    public const uint BodyBottom = 21;
    public const uint FooterRow = 23;
    public const uint ProgressRow = 24;
    public const uint LeftColumn = Left;   // two-column layout
    public const uint RightColumn = 21;
    public const uint ColumnWidth = 17;
    public const uint CellPixels = 8;

    // A cell's colour byte: fg for set pixels, bg for the rest.
    // (Colours go through locals: the compiler does no arithmetic on enums.)
    public static uint Matrix(Colors fg, Colors bg)
    {
        uint ink = (uint)fg;
        uint paper = (uint)bg;
        return (ink << 4) | paper;
    }

    // Element defaults. The compiler runs no field initializers or
    // constructor bodies, and has no Nullable<T>, so an element property left
    // at its default (0, or Colors.Black) means "the theme's value": write
    // Theme.Auto to say so explicitly. (Position 0 therefore cannot be asked
    // for; nothing in the grid uses column or row 0 for content.)
    public const uint Auto = 0;

    // A colour property left at its default (black) means "the theme's".
    public static Colors Or(Colors color, Colors fallback) => color == Colors.Black ? fallback : color;

    // The same for counts and positions left at 0. (A name of its own: the
    // compiler's labels do not tell overloads apart.)
    public static uint OrCells(uint value, uint fallback) => value == 0 ? fallback : value;
}
