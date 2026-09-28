namespace C64Lib
{
    // Which of the VIC-II's screen modes Screen.SetScreenMode selects.
    // Character is plain text mode (SetChar/Write) -- also what a program
    // returns to after using Bitmap/MultiColor, replacing the old
    // DisableBitmapMode() call. Bitmap is hi-res (1 bit/pixel, on/off).
    // MultiColor is bitmap mode with 2 bits/pixel (4 colors) -- see
    // BitmapColorSource. Unrelated to Screen.SetMultiColor(), which is a
    // separate, existing text-mode (character) multicolor feature.
    public enum ScreenMode : uint
    {
        Character,
        Bitmap,
        MultiColor
    }

    // Which of multicolor bitmap mode's 4 sources a drawn pixel's 2 color
    // bits come from -- a different, smaller concept than the 16-color
    // Colors palette: each source is itself just a place a real Colors
    // value is stored, and which one a given pixel points at.
    // Background is screen-wide ($D021, Screen.SetBackgroundColor).
    // MatrixHigh/MatrixLow are per-cell (Screen.SetBitmapColors' high/low
    // nibble). ColorRam is also per-cell, but a separate 1000-byte area
    // ($D800) from the video matrix -- reachable today via
    // C64.FillMemory(0xD800, ..., 1000), no new API needed. Ignored (hi-res
    // mode's `on` keeps its current on/off meaning) unless the screen is
    // currently in MultiColor mode.
    public enum BitmapColorSource : uint
    {
        Background,
        MatrixHigh,
        MatrixLow,
        ColorRam
    }

    public static partial class C64
    {
        // Nested (not a separate top-level class returned by a C64.Screen
        // property) specifically so every call compiles to a plain static
        // call -- no instance is ever constructed or pushed/pulled through
        // the eval stack for these. Also keeps "Screen" out of the global
        // namespace: Hunchback and C64Presentation each already have their
        // own unrelated local `Screen` helper class, so an unqualified
        // top-level C64Lib.Screen would collide/shadow ambiguously for code
        // in those namespaces -- callers always write C64.Screen.X(...).
        //
        // Screen/color RAM has no second bank to flip to (color RAM in
        // particular is a single physical 1K chip fixed at $D800) -- so
        // BeginUpdate/EndUpdate aren't real double buffering. What they give
        // you instead: BeginUpdate blanks the physical display (clears the
        // VIC-II's DEN bit), so any number of SetChar/Write calls in between
        // happen invisibly, however many real 6502 cycles they take;
        // EndUpdate turns the display back on. For a full-screen redraw (the
        // case this was built for -- Hunchback/IntroScroll.cs), that's
        // visually equivalent to double buffering: no partial or
        // intermediate state is ever shown.
        public static class Screen
        {
            public static void BeginUpdate() { }
            public static void EndUpdate() { }

            // Waits for the raster beam to reach the start of the bottom
            // border (line 251), so what follows can't tear the visible
            // frame. Consecutive calls are one video frame (1/50 s PAL)
            // apart.
            public static void WaitForVBlank() { }
            public static void SetChar(uint x, uint y, uint ch, Colors colors = Colors.LightBlue) { }
            public static int GetChar(uint x, uint y) => 0;
            public static void Write(uint x, uint y, string s, Colors colors = Colors.LightBlue) { }
            public static void SetBorderColor(Colors color) { }
            public static void SetBackgroundColor(Colors color) { }
            public static Colors GetBorderColor() => Colors.Black;
            public static void SetCharSet(ulong address) { }
            public static void SetMultiColor() { }
            public static void SetCharBackgroundColor(uint colorIndex, Colors color) { }

            // Bitmap graphics (hi-res or multicolor) -- "if you don't use it
            // you don't pay for it" extends to the memory layout itself
            // here, not just code size: calling this at all reserves an
            // 8000-byte bitmap (Graphics_Bitmap, $2000) + a 1000-byte
            // per-cell color matrix (Graphics_ColorMatrix, $0c00) in the
            // compiled program; never calling it costs nothing. Coordinates
            // passed to SetPixel/DrawLine/DrawRectangle/DrawCircle are
            // ulong (this compiler's 16-bit integer type) because X needs
            // 0-319, past uint's 8-bit range in this compiler's own width
            // convention -- see Compiler/TypeExtensions.cs.
            //
            // The mode can change at runtime -- e.g. SetScreenMode(Bitmap),
            // draw a hi-res scene, later SetScreenMode(MultiColor) for a
            // different one, without recompiling -- SetPixel/DrawLine/etc.
            // check the current mode themselves, so the same draw calls
            // work under either bitmap mode (see BitmapColorSource for the
            // multicolor-only color parameter).
            //
            // No separate foreground/background color-setting API for
            // MatrixHigh/MatrixLow: the color matrix (one byte per 8x8
            // cell, high nibble = foreground, low nibble = background, same
            // row-major layout as the text screen) is reachable via the
            // existing C64Address.FromLabel("Graphics_ColorMatrix") +
            // C64.FillMemory/SetMemory/GetMemory, exactly like any other
            // named label -- see Demo/Program.cs for the pattern.
            // ColorRam ($D800) and Background ($D021, SetBackgroundColor)
            // are both fixed hardware addresses, reachable the same way
            // without any named label at all.
            public static void SetScreenMode(ScreenMode mode) { }

            // Double buffering, to avoid the flicker of erasing and redrawing
            // a shape on the visible bitmap. Using either of these two calls
            // reserves a second bitmap + color matrix in VIC bank 1
            // (Graphics_Bitmap2 $4000, Graphics_ColorMatrix2 $6000) and moves
            // where compiled code starts from $4000 to $6400 -- about 9 KB
            // more address space, paid only by programs that call them.
            // SetScreenMode(Bitmap/MultiColor) shows and draws buffer 0, as
            // without double buffering; SetDrawBuffer(1) directs
            // SetPixel/DrawLine/... into the hidden buffer 1, and
            // SwapBuffers shows whichever buffer was
            // just drawn into (waiting for the bottom border first, see
            // WaitForVBlank, so the flip can't tear) and makes the other one
            // the draw target. The hidden buffer still holds the frame from
            // two swaps ago -- erase or redraw it before drawing the next.
            // Sprites don't survive a flip (their data pointers sit at
            // matrix+$3f8, which differs per buffer): keep them hidden.
            public static void SetDrawBuffer(uint buffer) { }
            public static void SwapBuffers() { }

            // Fills every cell of the color matrix (both buffers' when
            // double buffering) with one byte: high nibble = foreground,
            // low nibble = background. Without this, buffer 1's matrix would
            // need filling separately via C64Address.FromLabel(
            // "Graphics_ColorMatrix2").
            public static void SetBitmapColors(uint foregroundBackground) { }

            // colorSource only matters under SetScreenMode(MultiColor) --
            // which of the 4 sources described on BitmapColorSource a drawn
            // (on: true) pixel's 2 color bits point at; a cleared (on:
            // false) pixel always becomes Background regardless of
            // colorSource. Under Bitmap (hi-res) mode colorSource is
            // ignored and `on` keeps its plain set/clear meaning, as
            // before.
            public static void SetPixel(ulong x, ulong y, bool on = true, BitmapColorSource colorSource = BitmapColorSource.ColorRam) { }
            public static void DrawLine(ulong x0, ulong y0, ulong x1, ulong y1, bool on = true, BitmapColorSource colorSource = BitmapColorSource.ColorRam) { }
            public static void DrawRectangle(ulong x0, ulong y0, ulong x1, ulong y1, bool filled = false, bool on = true, BitmapColorSource colorSource = BitmapColorSource.ColorRam) { }
            public static void DrawCircle(ulong cx, ulong cy, ulong radius, bool filled = false, bool on = true, BitmapColorSource colorSource = BitmapColorSource.ColorRam) { }
        }
    }
}
