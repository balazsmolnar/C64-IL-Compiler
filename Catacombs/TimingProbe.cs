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
        uint frames = C64.GetMemory(0xA2, 0) - jiffy0_;
        ulong t = Line() + 312UL * frames - line0_;
        if (t > worst_[k])
            worst_[k] = t;
    }

    public static void Report()
    {
        count_ = count_ + 1;
        if ((count_ & 63) != 0)
            return;
        for (uint k = 0; k < 6; k++)
        {
            ulong w = worst_[k];
            C64.Screen.DrawText(k * 6 + 1, 23, w.ToString() + "  ", 0x10);
            worst_[k] = 0;
        }
    }
}
#endif
