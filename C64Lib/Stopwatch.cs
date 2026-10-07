namespace C64Lib
{
    public static partial class C64
    {
        // Nested (not a separate top-level class returned by a C64.Stopwatch
        // property) for the same reason as Screen -- see its own comment:
        // every call compiles to a plain static call, no instance ever
        // constructed or pushed through the eval stack.
        //
        // A precise, cycle-accurate stopwatch for measuring real elapsed
        // time from code (e.g. timing N iterations of a routine under
        // VICE, whose emulation is cycle-exact) -- unlike measuring off
        // the KERNAL jiffy clock ($A0-$A2), which only has 1/50s (PAL)
        // resolution, this counts real CPU cycles directly. Backed by CIA2
        // Timer A (asm/C64.asm's Stopwatch_Start/Stopwatch_Elapsed)
        // running free in continuous mode -- CIA1's own Timer A is left
        // alone, since it's already what drives the KERNAL's jiffy-clock
        // IRQ (see C64.Delay's own comment); reconfiguring it would break
        // that.
        //
        // Wraps every ~65536 cycles (~66.5ms at PAL speed): Elapsed() is
        // only meaningful for the time since the MOST RECENT Start() call,
        // and only if that's under ~66.5ms -- call Start() again before
        // each measured region rather than relying on one Start() for a
        // long-running or repeated total. For anything that might run
        // longer than that, use the jiffy clock instead (~21.8 minute
        // range, but only 1/50s resolution).
        public static class Stopwatch
        {
            public static void Start() { }
            public static ulong Elapsed() => 0;
        }
    }
}
