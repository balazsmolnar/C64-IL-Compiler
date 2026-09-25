namespace C64Lib
{
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
            public static void SetChar(uint x, uint y, uint ch, Colors colors = Colors.LightBlue) { }
            public static int GetChar(uint x, uint y) => 0;
            public static void Write(uint x, uint y, string s, Colors colors = Colors.LightBlue) { }
            public static void SetBorderColor(Colors color) { }
            public static void SetBackgroundColor(Colors color) { }
            public static Colors GetBorderColor() => Colors.Black;
            public static void SetCharSet(ulong address) { }
            public static void SetMultiColor() { }
            public static void SetCharBackgroundColor(uint colorIndex, Colors color) { }

            // Hi-res bitmap graphics -- "if you don't use it you don't pay
            // for it" extends to the memory layout itself here, not just
            // code size: calling any one of these reserves an 8000-byte
            // bitmap (Graphics_Bitmap, $2000) + a 1000-byte per-cell color
            // matrix (Graphics_ColorMatrix, $0c00) in the compiled program;
            // never calling any of them costs nothing. Coordinates are
            // ulong (this compiler's 16-bit integer type) because X needs
            // 0-319, past uint's 8-bit range in this compiler's own width
            // convention -- see Compiler/TypeExtensions.cs.
            //
            // No separate color-setting API: the color matrix (one byte per
            // 8x8 cell, high nibble = foreground shown where a bit is set,
            // low nibble = background shown where clear, same row-major
            // layout as the text screen) is reachable via the existing
            // C64Address.FromLabel("Graphics_ColorMatrix") + C64.FillMemory/
            // SetMemory/GetMemory, exactly like any other named label --
            // see Demo/Program.cs for the pattern.
            public static void EnableBitmapMode() { }
            public static void DisableBitmapMode() { }
            public static void SetPixel(ulong x, ulong y, bool on = true) { }
            public static void DrawLine(ulong x0, ulong y0, ulong x1, ulong y1, bool on = true) { }
            public static void DrawRectangle(ulong x0, ulong y0, ulong x1, ulong y1, bool filled = false, bool on = true) { }
            public static void DrawCircle(ulong cx, ulong cy, ulong radius, bool filled = false, bool on = true) { }
        }
    }
}
