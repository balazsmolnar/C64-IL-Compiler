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
        }
    }
}
