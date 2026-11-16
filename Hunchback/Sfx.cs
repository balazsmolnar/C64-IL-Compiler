using C64Lib;

namespace Hunchback;

// The two sounds of a lost life, which need more of the SID than
// C64.Sound's effects reach: written to its registers directly, as the
// original does (Restructure/Sound.asm's Sound_QuasiDying and
// Sound_QuasiDead).
class Sfx
{
    private static void Poke(uint register, uint value)
    {
        C64.FillMemory(0xD400UL + register, value, 1);
    }

    // The cry: a low held note on voice 1 whose pitch flutters.
    public static void Dying()
    {
        Poke(0, 0);
        Poke(1, 2);
        Poke(5, 0);
        Poke(6, 0xF0);
        Poke(4, 0x31);
        for (uint i = 18; i > 0; i--)
        {
            Poke(0, C64.GetMemory(C64Address.FromLabel("tbl_DeathSoundP2FreqLo"), i - 1));
            C64.Delay(2 + (i & 1));
        }
        Poke(4, 8);
    }

    // The end: noise rising through the low-pass filter, while the wall
    // flashes.
    public static void Dead()
    {
        Poke(0x17, 0xF1);
        Poke(0x18, 0x1F);
        Poke(0x15, 0);
        Poke(0, 0);
        Poke(5, 0);
        Poke(6, 0xF0);
        Poke(4, 0x81);
        bool flash = false;
        for (uint pitch = 40; pitch < 200; pitch += 10)
        {
            Poke(1, pitch);
            Poke(0x16, pitch);
            flash = !flash;
            C64.Screen.SetCharBackgroundColor(0, flash ? Colors.Grey2 : Colors.Grey1);
            C64.Delay(1);
        }
        Poke(4, 8);
        Poke(0x18, 15);
        Poke(0x17, 0);
        C64.Screen.SetCharBackgroundColor(0, Colors.Grey1);
    }
}
