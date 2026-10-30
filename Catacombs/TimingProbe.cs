#if SCENE_TIMING || SCENE_TIMINGM
using C64Lib;

namespace Catacombs;

// Timing scenes only (-Scene TIMING / TIMINGM, see Program.Main): how long
// each part of the main loop takes. Mark(k) after part k records the time
// since the loop started (in raster lines, 63 cycles each), keeps the worst
// over 64 iterations and prints the six numbers in the status panel.
//
// Start and Mark only take raw samples (the jiffy clock, then $D011/$D012 in
// one word, then the jiffy clock again: about 3 raster lines each); all the
// arithmetic happens in Report, after the last mark, outside the measured
// time. Mark k still includes the cost of the k marks before it.
static class TimingProbe
{
    const uint StartSlot = 6;
    static uint[] jiffyA_ = new uint[7];
    static uint[] jiffyB_ = new uint[7];
    static ulong[] raster_ = new ulong[7];      // low byte $D011 (bit 7 = raster bit 8), high byte $D012
    static ulong[] worst_ = new ulong[6];
    static uint count_;
    static ulong heap0_;

    public static void Start()
    {
        jiffyA_[StartSlot] = C64.GetMemory(0xA2, 0);
        raster_[StartSlot] = C64.GetWord(0xD011, 0);
        jiffyB_[StartSlot] = C64.GetMemory(0xA2, 0);
    }

    public static void Mark(uint k)
    {
        jiffyA_[k] = C64.GetMemory(0xA2, 0);
        raster_[k] = C64.GetWord(0xD011, 0);
        jiffyB_[k] = C64.GetMemory(0xA2, 0);
    }

    // Raster lines since the system interrupt (line 252), which also
    // advances the jiffy clock.
    static ulong Line(uint k)
    {
        ulong w = raster_[k];
        ulong line = w / 256;
        if ((w & 128) != 0)
            line = line + 256;
        return line >= 252 ? line - 252 : line + 60;
    }

    // The jiffy count that goes with sample k's raster line: if the clock
    // ticked between the two reads, the raster read came after the tick
    // when the line is early in the interrupt's frame.
    static uint Jiffy(uint k, ulong line)
    {
        if (jiffyA_[k] != jiffyB_[k] && line < 156)
            return jiffyB_[k];
        return jiffyA_[k];
    }

    public static void Report()
    {
        ulong line0 = Line(StartSlot);
        uint jiffy0 = Jiffy(StartSlot, line0);
        for (uint k = 0; k < 6; k++)
        {
            ulong line = Line(k);
            uint frames = Jiffy(k, line) - jiffy0;
            ulong t = line + 312UL * frames - line0;
            if (t > worst_[k])
                worst_[k] = t;
        }

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
            // The last one goes on the bottom row: at column 31 the HUD's
            // own drawing covers it.
            if (k == 5)
                C64.Screen.DrawText(15, 24, w.ToString() + "  ", 0x10);
            else
                C64.Screen.DrawText(k * 6 + 1, 23, w.ToString() + "  ", 0x10);
            worst_[k] = 0;
        }
    }
}
#endif
