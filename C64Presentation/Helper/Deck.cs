using C64Lib;

namespace C64Presentation.Helper;

// Slide flow: transitions between slides and builds within one.
//
// A slide is drawn while the screen is blanked (Begin), then faded in from
// black (Reveal); leaving it fades it out to black (FadeOut). Both fades
// run the screen's colours through the fade_Levels tables (Theme.cs) from a
// copy saved in the old text screen at $0400, which a bitmap program does
// not use: nothing is redrawn, no pixel changes, no garbage.
//
// Builds: Step() waits for "advance"; whatever is drawn after it is a build
// step: it is drawn invisibly (cells painted background on background) and
// faded in at the next Step(), Show() or the end of the slide. Elements do
// this by themselves as long as they colour cells with Paint and write
// with Text.
static class Deck
{
    public static uint Number;      // current slide, 0-based
    public static uint Count;       // slides in the deck

    static bool revealed_;
    static bool picture_;
    static bool building_;
    static uint marks_;
    static uint[] markX_ = new uint[24];
    static uint[] markY_ = new uint[24];
    static uint[] markW_ = new uint[24];
    static uint[] markH_ = new uint[24];
    static uint[] markFg_ = new uint[24];
    static uint[] markBg_ = new uint[24];
    static string[] markText_ = new string[24];   // text marks: redrawn in colour at the end
    static ulong[] markPx_ = new ulong[24];
    static ulong[] markFont_ = new ulong[24];

    const ulong Saved = 0x0400;

    public static bool Building => building_;

    // True when drawing shows at once (an animation can be watched).
    public static bool Live => revealed_ && !building_;

    // Once, at start: hi-res bitmap, everything black.
    public static void Init()
    {
        C64.Screen.SetBorderColor(Theme.Background);
        C64.Screen.SetBackgroundColor(Theme.Background);
        C64.Screen.SetBitmapColors(0);
        C64.Screen.ClearBitmap();
        C64.Screen.SetScreenMode(ScreenMode.Bitmap);
    }

    // Before a slide draws: the screen is black (FadeOut) and blanked.
    public static void Begin()
    {
        if (picture_)
        {
            C64.Screen.ClearSplit();
            C64.Screen.SetScreenMode(ScreenMode.Bitmap);
            picture_ = false;
        }
        C64.Screen.BeginUpdate();
        revealed_ = false;
        building_ = false;
        marks_ = 0;
    }

    // Fades the slide drawn so far in from black. Does nothing the second
    // time; a slide with live animation calls it before its loop.
    public static void Reveal()
    {
        if (revealed_)
            return;
        revealed_ = true;
        Save();
        C64.Screen.RemapCellColorsFrom(Saved, Level(0));
        C64.Screen.EndUpdate();
        for (uint k = 1; k < 5; k++)
        {
            C64.Screen.WaitForVBlank();
            C64.Screen.RemapCellColorsFrom(Saved, Level(k));
        }
    }

    // Fades the screen out to black and clears it for the next slide.
    public static void FadeOut()
    {
        Show();
        Save();
        for (uint k = 3; k < 4; k--)
        {
            C64.Screen.WaitForVBlank();
            C64.Screen.RemapCellColorsFrom(Saved, Level(k));
        }
        if (picture_)
        {
            // A picture's third colour is in colour RAM, which the fade
            // tables do not reach: cleared once the rest is dark.
            ulong colorRam = 0xD800;
            C64.FillMemory(colorRam, 0, 250);
            C64.FillMemory(colorRam + 250, 0, 250);
            C64.FillMemory(colorRam + 500, 0, 250);
            C64.FillMemory(colorRam + 750, 0, 250);
        }
        C64.Screen.BeginUpdate();
        C64.Screen.ClearBitmap();
    }

    // A full-bleed multicolour picture (tools/c64img.py, 40 x 21 cells,
    // --bg 0) over rows 0-20, with rows 21-24 hi-res below a raster split
    // for captions (Text works there as anywhere). The next slide switches
    // back by itself. A picture cuts in rather than fading (its colour RAM
    // colour is not in the saved copy) but fades out like any slide.
    public static void Picture(ulong image)
    {
        C64.Screen.SetScreenMode(ScreenMode.MultiColor);
        C64.Screen.SetSplit(21, ScreenMode.Bitmap);
        C64.Screen.BeginUpdate();
        C64.Screen.DrawImage(0, 0, image);
        picture_ = true;
    }

    // A build step: shows what is pending, waits for "advance" (not in a
    // SCENE_HOLD build, which shows every step at once), then draws what
    // follows invisibly until the next Step/Show.
    public static void Step()
    {
        Show();
        Reveal();
#if !SCENE_HOLD
        KeyBoard.WaitForKeys();
#endif
        building_ = true;
    }

    // Fades in what the current build step drew.
    public static void Show()
    {
        if (!building_)
            return;
        building_ = false;
        if (marks_ == 0)
            return;
        for (uint k = 1; k < 5; k++)
        {
            ulong table = Level(k);
            for (uint i = 0; i < marks_; i++)
            {
                uint m = (C64.GetMemory(table, markFg_[i]) << 4) | C64.GetMemory(table, markBg_[i]);
                // The last step redraws text in its own colours (inline
                // colour changes included); its pixels are already there.
                if (k == 4 && markText_[i] != null)
                    C64.Screen.DrawFontText(markPx_[i], markY_[i], markText_[i], markFont_[i], m);
                else
                    C64.Screen.SetCellColors(markX_[i], markY_[i], markW_[i], markH_[i], m, Colors.Black);
            }
            C64.Screen.WaitForVBlank();
            C64.Screen.WaitForVBlank();
        }
        marks_ = 0;
    }

    // Colours a rectangle of cells: fg for set pixels, bg for the rest.
    // During a build step the cells stay hidden (bg on bg) until Show.
    public static void Paint(uint column, uint row, uint width, uint height, Colors fg, Colors bg)
    {
        if (!building_)
        {
            C64.Screen.SetCellColors(column, row, width, height, Theme.Matrix(fg, bg), Colors.Black);
            return;
        }
        C64.Screen.SetCellColors(column, row, width, height, Theme.Matrix(bg, bg), Colors.Black);
        if (marks_ == 24)
            return;
        markX_[marks_] = column;
        markY_[marks_] = row;
        markW_[marks_] = width;
        markH_[marks_] = height;
        markFg_[marks_] = (uint)fg;
        markBg_[marks_] = (uint)bg;
        markText_[marks_] = null;
        marks_++;
    }

    // Text drawn in a build step (see Text): Paint, plus what to redraw.
    public static void PaintText(uint column, uint row, uint width, uint height, Colors fg, Colors bg, ulong x, string text, ulong font)
    {
        uint before = marks_;
        Paint(column, row, width, height, fg, bg);
        if (marks_ == before)
            return;
        markText_[before] = text;
        markPx_[before] = x;
        markFont_[before] = font;
    }

    static ulong Level(uint k) => C64Address.FromLabel("fade_Levels") + (ulong)(k << 4);

    static void Save()
    {
        ulong matrix = C64Address.FromLabel("Graphics_ColorMatrix");
        C64.CopyMemory(Saved, matrix, 250);
        C64.CopyMemory(Saved + 250, matrix + 250, 250);
        C64.CopyMemory(Saved + 500, matrix + 500, 250);
        C64.CopyMemory(Saved + 750, matrix + 750, 250);
    }
}
