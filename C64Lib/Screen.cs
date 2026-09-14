namespace C64Lib
{
    // Screen/color RAM has no second bank to flip to (color RAM in particular
    // is a single physical 1K chip fixed at $D800) -- so this isn't real
    // double buffering. What it gives you instead: BeginUpdate blanks the
    // physical display (clears the VIC-II's DEN bit), so any number of
    // SetChar/FillMemory/CopyMemory/Write calls in between happen invisibly,
    // however many real 6502 cycles they take; EndUpdate turns the display
    // back on. For a full-screen redraw (the case this was built for --
    // Hunchback/IntroScroll.cs), that's visually equivalent to double
    // buffering: no partial or intermediate state is ever shown.
    public class Screen
    {
        public void BeginUpdate() { }
        public void EndUpdate() { }
    }
}
