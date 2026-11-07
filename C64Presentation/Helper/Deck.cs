using C64Lib;

namespace C64Presentation.Helper;

// Slide flow: transitions between slides and builds within one.
//
// A slide is drawn while the screen is blanked (Begin), then faded in from
// black (Reveal); leaving it fades it out to black (FadeOut). Both fades
// run the screen's colours through the fade_Levels tables (Theme.cs) from a
// copy saved in the old text screen ($0418-$07FF), which a bitmap program
// does not show: nothing is redrawn, no pixel changes, no garbage.
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
    static bool onSurface_;     // the step is drawn onto a visible surface
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

    // The colour save buffer: 1000 bytes at $0418-$07FF, in the old text
    // screen a bitmap program does not show. Not from $0400: the KERNAL
    // keeps writing the character under its (home) cursor there. The last 8
    // bytes are the text screen's sprite pointers, unused in bitmap mode.
    const ulong Saved = 0x0418;
    const ulong ColorRam = 0xD800;

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
            C64.Screen.SetBackgroundColor(Theme.Background);
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
            ScreenBytes.Fill(ColorRam, 0);
        }
        C64.Screen.BeginUpdate();
        C64.Screen.ClearBitmap();
    }

    // A multicolour picture (tools/c64img.py, --bg 0) at (column, row),
    // with the rows from `split` down hi-res below a raster split (for
    // captions); split 25 = the whole screen multicolour. The next slide
    // switches back by itself. Its colour RAM colour is not in the saved
    // copy, so that one colour cuts in and out instead of fading.
    public static void Picture(ulong image, uint column, uint row, uint split)
    {
        C64.Screen.SetScreenMode(ScreenMode.MultiColor);
        if (split < 25)
            C64.Screen.SetSplit(split, ScreenMode.Bitmap);
        C64.Screen.BeginUpdate();
        C64.Screen.DrawImage(column, row, image);
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
        onSurface_ = false;
    }

    // Starts hidden drawing without waiting: what follows fades in at the
    // next Show/Step. onSurface: it goes onto a surface that stays visible
    // (new code on a code card), so its background does not fade.
    public static void Build(bool onSurface)
    {
        Show();
        building_ = true;
        onSurface_ = onSurface;
    }

    // Fades the foreground of a rectangle down to its background (text on a
    // card going out before new text comes in).
    public static void Dim(uint column, uint row, uint width, uint height, Colors fg, Colors bg)
    {
        uint paper = (uint)bg;
        for (uint k = 3; k > 0; k--)
        {
            C64.Screen.SetCellColors(column, row, width, height,
                (C64.GetMemory(Level(k), (uint)fg) << 4) | paper, Colors.Black);
            C64.Screen.WaitForVBlank();
            C64.Screen.WaitForVBlank();
        }
        C64.Screen.SetCellColors(column, row, width, height, Theme.Matrix(bg, bg), Colors.Black);
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
                uint bg = onSurface_ ? markBg_[i] : C64.GetMemory(table, markBg_[i]);
                uint m = (C64.GetMemory(table, markFg_[i]) << 4) | bg;
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
    // During a build step the cells stay hidden until Show: black, or the
    // surface's own colour when drawing onto one (Build(true)).
    public static void Paint(uint column, uint row, uint width, uint height, Colors fg, Colors bg)
    {
        if (!building_)
        {
            C64.Screen.SetCellColors(column, row, width, height, Theme.Matrix(fg, bg), Colors.Black);
            return;
        }
        C64.Screen.SetCellColors(column, row, width, height, onSurface_ ? Theme.Matrix(bg, bg) : 0, Colors.Black);
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
        ScreenBytes.Copy(Saved, C64Address.FromLabel("Graphics_ColorMatrix"));
    }
}

// 1000-byte (one per cell) copies and fills. THE ONE PLACE that works around
// C64.CopyMemory/FillMemory writing bytes 1..size of the address given (or
// 0..255 for size 0): three whole pages, then the last 232 bytes from one
// below. When those calls write address..address+size-1, this becomes four
// plain 250-byte calls.
static class ScreenBytes
{
    const uint Tail = 232;
    const ulong TailStart = 767;    // 768 - 1: bytes 768..999

    public static void Copy(ulong dest, ulong source)
    {
        C64.CopyMemory(dest, source, 0);
        C64.CopyMemory(dest + 256, source + 256, 0);
        C64.CopyMemory(dest + 512, source + 512, 0);
        C64.CopyMemory(dest + TailStart, source + TailStart, Tail);
    }

    public static void Fill(ulong dest, uint value)
    {
        C64.FillMemory(dest, value, 0);
        C64.FillMemory(dest + 256, value, 0);
        C64.FillMemory(dest + 512, value, 0);
        C64.FillMemory(dest + TailStart, value, Tail);
    }
}
