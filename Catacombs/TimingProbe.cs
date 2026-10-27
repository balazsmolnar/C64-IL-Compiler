#if SCENE_TIMING || SCENE_TIMINGM
using C64Lib;

namespace Catacombs;

// Timing scenes only (-Scene TIMING / TIMINGM, see Program.Main): how long
// each part of the main loop takes. Mark(k) after part k records the time
// since the loop started (in raster lines, 63 cycles each), keeps the worst
// over 64 iterations and prints the six numbers in the status panel.
static class TimingProbe
{
    static ulong line0_;
    static uint jiffy0_;
    static ulong[] worst_ = new ulong[8];
    static uint count_;
    static ulong heap0_;

    // Raster lines since the system interrupt (line 252), which also
    // advances the jiffy clock.
    static ulong Line()
    {
        ulong line = C64.GetMemory(0xD012, 0);
        if ((C64.GetMemory(0xD011, 0) & 128) != 0)
            line = line + 256;
        return line >= 252 ? line - 252 : line + 60;
    }

    public static void Start()
    {
        jiffy0_ = C64.GetMemory(0xA2, 0);
        line0_ = Line();
    }

    public static void Mark(uint k)
    {
        // No multiply (it cost 4 lines a call). Every Mark's own time is counted
        // in all the later parts. Measured: ~18 raster lines a call, so the
        // printed total (all six marks) reads ~90 lines high; for the real
        // frame time keep only Mark(5).
        uint frames = C64.GetMemory(0xA2, 0) - jiffy0_;
        ulong t = Line() - line0_;
        for (; frames != 0; frames--)
            t = t + 312;
        if (t >= 0x8000) // read just after line 252, before the interrupt advanced the jiffy clock
            t = t + 312;
        if (t > worst_[k])
            worst_[k] = t;
    }

    public static void Report()
    {
        count_ = count_ + 1;
        if ((count_ & 63) != 0)
            return;
        // The heap's bump pointer (zero page $FB/$FC), read before this
        // report makes any strings: the first one is what a run needs.
        ulong heap = C64.GetWord(0xFB, 0);
        if (heap0_ == 0)
            heap0_ = heap;
        C64.Screen.DrawText(1, 24, heap0_.ToString(), 0x10);
        C64.Screen.DrawText(8, 24, heap.ToString(), 0x10);
        for (uint k = 0; k < 6; k++)
        {
            ulong w = worst_[k];
            C64.Screen.DrawText(k * 6 + 1, 23, w.ToString() + "  ", 0x10);
            worst_[k] = 0;
        }
    }
}
#endif
