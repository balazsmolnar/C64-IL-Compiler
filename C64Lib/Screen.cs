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
            // here, not just code size: never calling a graphics method
            // costs nothing. Calling one selects a graphics memory layout
            // (Compiler/Templates/ProgramEntry.asm): the bitmap
            // (Graphics_Bitmap) is the RAM underneath the KERNAL ROM at
            // $E000, which a program could not use otherwise, the 1000-byte
            // per-cell color matrix (Graphics_ColorMatrix) is at $CC00, and
            // the VIC is pointed at that bank while a bitmap mode is on.
            // (Bitmap bytes can be written from C# with C64.FillMemory, but
            // not read back with C64.GetMemory, which sees the ROM.)
            // Programs that double-buffer (SetDrawBuffer/SwapBuffers) get
            // the older layout instead: bitmap $2000, matrix $0c00. Coordinates
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
            // switches the program to the low graphics layout: buffer 0 in
            // VIC bank 0 (bitmap $2000, matrix $0c00), a second bitmap +
            // color matrix in bank 1 (Graphics_Bitmap2 $4000,
            // Graphics_ColorMatrix2 $6000), compiled code from $6400 --
            // about 18 KB of address space, paid only by programs that call
            // them. ([RawSprites] and DrawImage's buffer selection are not
            // available there.)
            // SetScreenMode(Bitmap/MultiColor) shows and draws buffer 0, as
            // without double buffering; SetDrawBuffer(1) directs
            // SetPixel/DrawLine/... into the hidden buffer 1, and
            // SwapBuffers shows whichever buffer was
            // just drawn into (waiting for the bottom border first, see
            // WaitForVBlank, so the flip can't tear) and makes the other one
            // the draw target. The hidden buffer still holds the frame from
            // two swaps ago -- erase or redraw it before drawing the next.
            // Sprites need care under a flip: their data pointers sit at
            // matrix+$3f8, which differs per buffer, and each buffer is in
            // its own VIC bank, so a sprite's art must exist in both banks
            // and its pointer be written into both matrices' tables (the
            // DataBlock setter only knows the text screen's $07f8). See
            // Catacombs/Bats.cs and Sprites.cs for a working example; a
            // program that doesn't want to deal with that keeps them hidden.
            public static void SetDrawBuffer(uint buffer) { }
            public static void SwapBuffers() { }

            // Whether this program should render double-buffered rather
            // than a direct, single-buffered erase+redraw path -- true by
            // default (the byte this reads is never written by anything
            // in a normal build). Lets one build be compared both ways by
            // externally patching that byte to nonzero (e.g. in a hex
            // editor), without the calling program needing to know or
            // declare any address itself -- see asm/C64Graphics.asm's own
            // Screen_DoubleBuffered for exactly which byte and why it's
            // safe to use this way.
            public static bool DoubleBuffered() => true;

            // Erases the bitmap of the current draw target (buffer 0, or
            // whichever buffer SetDrawBuffer/SwapBuffers last made the draw
            // target) to the background: what to call before redrawing a
            // frame into a hidden buffer. One call, about 45,000 cycles --
            // clearing with C64.FillMemory in a loop takes several times as
            // long. Leaves the color matrix alone.
            public static void ClearBitmap() { }

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

            // A filled, always-4-sided trapezoid: row y0 spans x0Left..
            // x0Right, row y1 spans x1Left..x1Right, and every row between
            // linearly interpolates each edge -- a rectangle is the special
            // case x0Left=x1Left/x0Right=x1Right, a triangle is x0Left=
            // x0Right or x1Left=x1Right (that corner degenerates to a
            // point). y0/y1 (and the two x pairs together) can be given in
            // either order. Always filled -- there is no outline-only mode,
            // unlike DrawRectangle/DrawCircle (draw the two slanted edges
            // with DrawLine and the two rows with DrawLine/DrawRectangle
            // instead, if that's what's wanted).
            //
            // Costs one one-time division per edge (not a division per
            // row), then the same byte-level span fill as DrawRectangle's
            // filled mode per row -- see asm/C64Graphics.asm's
            // Graphics_Trapezoid_Core for the DDA this runs.
            public static void DrawTrapezoid(ulong x0Left, ulong x0Right, ulong y0, ulong x1Left, ulong x1Right, ulong y1, bool on = true, BitmapColorSource colorSource = BitmapColorSource.ColorRam) { }

            // Decompresses and blits a stencil bitmap imported via
            // [assembly: RawBitmap(...)] -- a single-color (1 bit/pixel)
            // stencil: "on" pixels are painted in colorSource (same
            // BitmapColorSource convention as SetPixel/DrawLine/
            // DrawRectangle), "off" pixels are left COMPLETELY untouched --
            // whatever was already on the bitmap at that position (walls,
            // floor, another bitmap) shows through, unlike DrawRectangle/
            // DrawLine's `on: false`, which clears to Background. X and Y
            // must both be multiples of 8 (bitmaps are cell-aligned) -- a
            // caller contract, not checked at runtime, same as every other
            // coordinate this API takes.
            //
            // bitmap is the asset's generated data-block address --
            // C64Address.FromLabel("bmp_<Label>"), the same way a sprite's
            // art is referenced via C64Address.FromLabel("spt_<Label>").
            public static void DrawBitmap(ulong x, ulong y, ulong bitmap, BitmapColorSource colorSource = BitmapColorSource.ColorRam) { }

            // ---- Cells: full-colour images and per-cell colours ----
            //
            // A cell is one 8x8 character position: column 0-39, row 0-24.
            // Each cell has its 8 bitmap bytes and its own colours: the
            // video matrix byte (two colours: high nibble / low nibble) and
            // the colour RAM byte (a third colour, multicolour only). The
            // three methods below work in whole cells, and take cell
            // coordinates (not pixels). Rectangles are not clipped: keep
            // them on the screen.

            // Decompresses a picture embedded with [assembly: RawImage(...)]
            // (made from a PNG by tools/c64img.py) onto the screen, pixels
            // and colours, its top-left cell at (column, row):
            //
            //     C64.Screen.DrawImage(0, 0, C64Address.FromLabel("img_Room"));
            //
            // Cells the image does not contain (transparent in the PNG) are
            // left exactly as they were. An image stored without its bitmap
            // plane only recolours. A full 40x21-cell picture takes about
            // 0.15-0.2 s. Draws into buffer 0 (ignores SetDrawBuffer).
            public static void DrawImage(uint column, uint row, ulong image) { }

            // Sets the colours of every cell in a rectangle, without
            // touching a pixel: matrix = (high nibble colour << 4) | low
            // nibble colour, colorRam = the third (multicolour) colour.
            public static void SetCellColors(uint column, uint row, uint width, uint height, uint matrix, Colors colorRam) { }

            // Replaces every colour in a rectangle of cells by table[colour]
            // -- both matrix nibbles and the colour RAM value -- without
            // touching a pixel: a palette swap (room themes from one
            // picture), a colour cycle or flicker (apply a table that
            // rotates a few colours, every few frames), a flash (swap, then
            // swap back). table is the address of 16 bytes, one per C64
            // colour, each 0-15, e.g.
            //
            //     [assembly: RawAssembly(Asm = "pal_Moss .byte 0,1,2,3,4,5,5,7,8,9,10,11,12,13,13,15")]
            //     C64.Screen.RemapCellColors(0, 0, 40, 21, C64Address.FromLabel("pal_Moss"));
            //
            // About 75 cycles per cell: the whole 40x21 viewport is some 3
            // frames, a small area is nothing.
            public static void RemapCellColors(uint column, uint row, uint width, uint height, ulong table) { }

            // Copies the left half of a cell rectangle onto its right half,
            // mirrored, pixels and colours: a symmetric picture (a face, a
            // doorway, a frame) needs only its left half stored and drawn.
            //
            //     C64.Screen.DrawImage(0, 0, C64Address.FromLabel("img_LeftHalf"));   // 20 cells wide
            //     C64.Screen.MirrorCells(0, 0, 40, 21);
            //
            // Pixels are reversed as multicolour pixel pairs or as hi-res
            // bits, whichever SetScreenMode last set. The middle column of an
            // odd width is left alone. About 600 cycles per copied cell: half
            // the viewport takes some 13 frames.
            public static void MirrorCells(uint column, uint row, uint width, uint height) { }

            // The VIC's horizontal fine scroll: shows the picture 0-7 pixels
            // to the right (a screen shake; the fine part of a scroller).
            // The pixels that leave on the right are hidden, the gap on the
            // left shows the background colour. While a raster split is on
            // (SetSplit), only the part above the split moves, starting with
            // the next frame.
            public static void SetScrollX(uint pixels) { }

            // Writes text into the bitmap, one character per cell from
            // (column, row), using the built-in 8x8 font, and gives those
            // cells the colours `matrix` = (text colour << 4) | background.
            // The font is hi-res (one bit per pixel), so use it on a hi-res
            // screen or below a split (SetSplit) whose lower part is hi-res;
            // in a multicolour area the letters come out as coloured
            // blocks. Works with number.ToString(). The character '@' cannot
            // be drawn (its code, 0, ends the string).
            //
            // The font is the ROM's upper-case/graphics set, copied to RAM at
            // startup: 128 characters of 8 bytes at
            // C64Address.FromLabel("Graphics_Font"), indexed by screen code
            // -- copy other glyphs over it for a custom font.
            public static void DrawText(uint column, uint row, string text, uint matrix) { }

            // Raster split: from cell row `row` to the bottom, the screen is
            // shown as lowerMode (Bitmap = hi-res, or MultiColor) while the
            // rows above stay in the mode set with SetScreenMode. It is the
            // same bitmap and the same per-cell colours either way; only how
            // the bits are read changes. Typical use: a multicolour picture
            // above a crisp hi-res status panel.
            //
            //     C64.Screen.SetScreenMode(ScreenMode.MultiColor);
            //     C64.Screen.SetSplit(21, ScreenMode.Bitmap);
            //
            // Call it after SetScreenMode; call ClearSplit before changing
            // the screen mode again. Keep the first pixel line of the lower
            // part empty (background): on frames where several sprites
            // cross the split, that one line may still be shown in the upper
            // part's mode.
            //
            // While a split is on, the system interrupt (keyboard scan,
            // jiffy clock, and a C64.Interrupt subscriber) runs once per
            // video frame -- 50 times a second on PAL, in the bottom border
            // -- instead of 60 times a second from the CIA timer.
            // Floating-point maths holds interrupts off and can make the
            // split arrive late for a frame.
            public static void SetSplit(uint row, ScreenMode lowerMode) { }
            public static void ClearSplit() { }
        }
    }
}
