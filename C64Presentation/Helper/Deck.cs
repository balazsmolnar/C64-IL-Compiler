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
// step: it is drawn hidden and faded in at the next Step(), Show() or the
// end of the slide. Elements do this by themselves as long as they colour
// cells with Paint and write with Text.
//
// Jumps (N / B, KeyBoard.Pending): the first Step, Wait, Show, Reveal or
// Paint after the press fades the slide out at once and blanks the screen;
// from then on they return at once, so the rest of the slide runs through
// unseen, and FadeOut (Presenter) only clears up.
static class Deck
{
    public static uint Number { get; private set; }     // current slide, 0-based
    public static uint Count { get; private set; }      // slides in the deck
    public static bool Hidden { get; private set; }     // a hidden slide: no number, no progress

    // Fade levels: fade_Levels holds Black (0) .. Full (4).
    const uint Black = 0;
    const uint Full = 4;
    const uint FramesPerBuildLevel = 2;
    const uint MaxMarks = 24;

    // The colour save buffer: 1000 bytes at $0418-$07FF, in the old text
    // screen a bitmap program does not show. Not from $0400: the KERNAL
    // keeps writing the character under its (home) cursor there. The last 8
    // bytes are the text screen's sprite pointers, unused in bitmap mode.
    const ulong Saved = 0x0418;
    const ulong ColorRam = 0xD800;

    static bool _revealed;
    static bool _picture;
    static bool _building;
    static bool _onSurface;     // the step is drawn onto a visible surface
    static bool _leaving;       // a jump faded the slide out; the rest draws unseen
    static uint _contentBottom; // rows from here to the footer stay empty (Master.Content)
    static uint _markCount;
    static readonly Mark[] _marks = new Mark[MaxMarks];

    public static bool Building => _building;

    // True when drawing shows at once (an animation can be watched).
    public static bool Live => _revealed && !_building && !KeyBoard.Jumping;

    // Once, at start: hi-res bitmap, everything black.
    public static void Init(uint count)
    {
        Count = count;
        C64.Screen.SetBorderColor(Theme.Background);
        C64.Screen.SetBackgroundColor(Theme.Background);
        C64.Screen.SetBitmapColors(0);
        C64.Screen.ClearBitmap();
        C64.Screen.SetScreenMode(ScreenMode.Bitmap);
    }

    // Before slide `number` draws: the screen is black (FadeOut) and blanked.
    public static void Begin(uint number)
    {
        Number = number;
        Hidden = false;
        _contentBottom = Screen.Rows;
        if (_picture)
        {
            C64.Screen.ClearSplit();
            C64.Screen.SetScreenMode(ScreenMode.Bitmap);
            C64.Screen.SetBackgroundColor(Theme.Background);
            _picture = false;
        }
        C64.Screen.BeginUpdate();
        _revealed = false;
        _building = false;
        _leaving = false;
        _markCount = 0;
    }

    // Swaps the current slide for a hidden one (an appendix shown on a key):
    // fades out, then starts a slide whose footer has no number or progress.
    public static void BeginHidden()
    {
        FadeOut();
        Begin(Number);
        Hidden = true;
    }

    // True once a jump is pending; the first time, fades what is on screen
    // out (what a build step had hidden stays hidden) and blanks the screen.
    public static bool Leaving()
    {
        if (!KeyBoard.Jumping)
            return false;
        if (!_leaving)
        {
            _leaving = true;
            FadeToBlack();
        }
        return true;
    }

    // Fades the slide drawn so far in from black. Does nothing the second
    // time; a slide with live animation calls it before its loop.
    public static void Reveal()
    {
        if (_revealed || Leaving())
            return;
        _revealed = true;
        Save();
        C64.Screen.RemapCellColorsFrom(Saved, Level(Black));
        C64.Screen.EndUpdate();
        for (uint level = Black + 1; level <= Full; level++)
        {
            C64.Screen.WaitForVBlank();
            C64.Screen.RemapCellColorsFrom(Saved, Level(level));
        }
    }

    // Fades the screen out to black and clears it for the next slide. After
    // a jump the fade is done: what the slide drew since, colours included,
    // is cleared unseen.
    public static void FadeOut()
    {
        if (_leaving)
            ScreenBytes.Fill(C64Address.FromLabel("Graphics_ColorMatrix"), 0);
        else
        {
            Show();
            FadeToBlack();
        }
        C64.Screen.ClearBitmap();
    }

    static void FadeToBlack()
    {
        C64.Sprites.VisibleMask = 0;     // sprites have no fade levels: they go first
        if (_revealed)
        {
            Save();
            uint level = Full;
            while (level > Black)
            {
                level--;
                C64.Screen.WaitForVBlank();
                C64.Screen.RemapCellColorsFrom(Saved, Level(level));
            }
        }
        if (_picture)
        {
            // A picture's third colour is in colour RAM, which the fade
            // tables do not reach: cleared once the rest is dark.
            ScreenBytes.Fill(ColorRam, 0);
        }
        C64.Screen.BeginUpdate();
    }

    // A multicolour picture (tools/c64img.py, --bg 0) at (column, row),
    // with the rows from `split` down hi-res below a raster split (for
    // captions); split 25 = the whole screen multicolour. The next slide
    // switches back by itself. Its colour RAM colour is not in the saved
    // copy, so that one colour cuts in and out instead of fading.
    public static void Picture(ulong image, uint column, uint row, uint split)
    {
        C64.Screen.SetScreenMode(ScreenMode.MultiColor);
        if (split < Screen.Rows)
            C64.Screen.SetSplit(split, ScreenMode.Bitmap);
        C64.Screen.BeginUpdate();
        C64.Screen.DrawImage(column, row, image);
        _picture = true;
    }

    // A build step: shows what is pending, waits for "advance" (not in a
    // SCENE_HOLD build, which shows every step at once), then draws what
    // follows hidden until the next Step/Show.
    public static void Step()
    {
        if (Leaving())
            return;
        Show();
        Reveal();
#if !SCENE_HOLD
        KeyBoard.WaitForAdvance();
        if (Leaving())
            return;
#endif
        _building = true;
        _onSurface = false;
    }

    // Waits `frames` frames, or less once a jump is pending (then whatever
    // follows is drawn hidden). For a slide's own timed animation.
    public static void Wait(uint frames)
    {
        for (uint frame = 0; frame < frames; frame++)
        {
            KeyBoard.Poll();
            if (Leaving())
                return;
            C64.Screen.WaitForVBlank();
        }
    }

    // Starts hidden drawing without waiting: what follows fades in at the
    // next Show/Step. onSurface: it goes onto a surface that stays visible,
    // so its background does not fade.
    public static void Build(bool onSurface)
    {
        Show();
        _building = true;
        _onSurface = onSurface;
    }

    // Fades in what the current build step drew.
    public static void Show()
    {
        if (!_building || Leaving())
            return;
        _building = false;
        if (_markCount == 0)
            return;
        for (uint level = Black + 1; level <= Full; level++)
        {
            ulong table = Level(level);
            for (uint i = 0; i < _markCount; i++)
                _marks[i].Show(table, level == Full, _onSurface);
            for (uint frame = 0; frame < FramesPerBuildLevel; frame++)
                C64.Screen.WaitForVBlank();
        }
        _markCount = 0;
    }

    // A content slide: rows from `bottom` to the footer must stay empty.
    public static void KeepEmptyFrom(uint bottom) => _contentBottom = bottom;

    // In a scene build, drawing into the rows kept empty above the footer,
    // or drawing a slide before its fade-in while the display is on (VIC
    // DEN, $D011 bit 4), turns the border red, so a screenshot shows it.
    // Compiled out of the real deck ([Conditional]: the calls go too).
    [System.Diagnostics.Conditional("SCENE")]
    public static void CheckRows(uint row, uint height)
    {
        if (row + height > _contentBottom && row < Theme.FooterRow)
            C64.Screen.SetBorderColor(Theme.Bad);
        if (!_revealed && (C64.GetMemory(0xD011, 0) & 0x10) != 0)
            C64.Screen.SetBorderColor(Theme.Bad);
    }

    // Colours a rectangle of cells: fg for set pixels, bg for the rest.
    // During a build step the cells stay hidden until Show: black, or the
    // surface's own colour when drawing onto one (Build(true)).
    public static void Paint(uint column, uint row, uint width, uint height, Colors fg, Colors bg)
        => PaintText(column, row, width, height, fg, bg, "", 0, 0);

    // Paint for text drawn in a build step (see Text): at the last fade
    // level the text is redrawn in its own colours, inline changes included.
    // (A name of its own: the compiler's labels do not tell overloads apart.
    // "" = no text: the compiler cannot pass a bare null as a string.)
    public static void PaintText(uint column, uint row, uint width, uint height, Colors fg, Colors bg, string text, ulong x, ulong font)
    {
        if (Leaving())
            return;
        CheckRows(row, height);
        if (!_building)
        {
            C64.Screen.SetCellColors(column, row, width, height, Theme.Matrix(fg, bg), Colors.Black);
            return;
        }
        C64.Screen.SetCellColors(column, row, width, height, _onSurface ? Theme.Matrix(bg, bg) : 0, Colors.Black);
        if (_markCount == MaxMarks)
            return;
        var mark = _marks[_markCount];
        if (mark == null)              // the pool fills on first use
        {
            mark = new Mark();
            _marks[_markCount] = mark;
        }
        mark.Column = column;
        mark.Row = row;
        mark.Width = width;
        mark.Height = height;
        mark.Fg = fg;
        mark.Bg = bg;
        mark.Text = text;
        mark.X = x;
        mark.Font = font;
        _markCount++;
    }

    static ulong Level(uint level) => C64Address.FromLabel("fade_Levels") + (ulong)(level << 4);

    static void Save() => ScreenBytes.Copy(Saved, C64Address.FromLabel("Graphics_ColorMatrix"));
}

// A rectangle a build step painted, to fade in (and the text to redraw in it).
// A small class rather than a record struct: the compiler has no structs.
sealed class Mark
{
    public uint Column { get; set; }
    public uint Row { get; set; }
    public uint Width { get; set; }
    public uint Height { get; set; }
    public Colors Fg { get; set; }
    public Colors Bg { get; set; }
    public string? Text { get; set; }        // "" = a rectangle, not text
    public ulong X { get; set; }
    public ulong Font { get; set; }

    public void Show(ulong table, bool last, bool keepBackground)
    {
        uint ink = C64.GetMemory(table, (uint)Fg);
        uint paper = keepBackground ? (uint)Bg : C64.GetMemory(table, (uint)Bg);
        uint matrix = (ink << 4) | paper;
        if (last && Text != null && Text.Length != 0)
            C64.Screen.DrawFontText(X, Row, Text, Font, matrix);
        else
            C64.Screen.SetCellColors(Column, Row, Width, Height, matrix, Colors.Black);
    }
}

// One byte per cell of the screen: 1000 bytes, as four 250-byte runs.
static class ScreenBytes
{
    const uint Run = 250;
    const ulong Cells = Screen.Columns * Screen.Rows;

    public static void Copy(ulong dest, ulong source)
    {
        for (ulong offset = 0; offset < Cells; offset += Run)
            C64.CopyMemory(dest + offset, source + offset, Run);
    }

    public static void Fill(ulong dest, uint value)
    {
        for (ulong offset = 0; offset < Cells; offset += Run)
            C64.FillMemory(dest + offset, value, Run);
    }
}
