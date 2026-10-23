using C64Lib;

namespace Catacombs;

// The panel's map (see Hud.cs): the maze's 10x10 rooms in the window of
// panel columns 31-36, one room = 2x2 multicolour pixels, from line 8 of the
// panel. Only rooms the player has been in are drawn (fog of war), dim; the
// room the player is in is the bright "you are here" mark, which blinks
// between bright and dim (never off).
//
// The window's 6x3 cells live in RAM (hud_Map, HudArt.cs) as tiles. A room
// is half a bitmap byte on two lines: revealing, marking or blinking one
// changes two bytes there and copies that one cell to the screen. In a
// room's four bits per line, %1010 is dim (colour 2) and %1111 is bright
// (colour 3), so a blink is an exclusive-or with %0101.
static class Minimap
{
    static uint index_;             // the current room's first byte in hud_Map
    static uint mask_, blink_;      // its half of that byte; the bits a blink flips
    static uint column_, row_;      // the screen cell holding it

    public static void Init()
    {
        C64.FillMemory(C64Address.FromLabel("hud_Map-1"), 0, 144); // FillMemory writes bytes 1..size
        mask_ = 0;
#if SCENE_TIMING || SCENE_TIMINGM
        C64.Screen.SetSplit(21, ScreenMode.Bitmap); // the timing readout is hi-res text
#endif
    }

    public static void Enter(uint x, uint y)
    {
        if (mask_ != 0)
            Store((C64.GetMemory(C64Address.FromLabel("hud_Map"), index_) | mask_) ^ blink_);        // the room left behind: dim
        index_ = C64.GetMemory(C64Address.FromLabel("hud_MapRow-1"), y) + (x & 0xFE) * 4;
        row_ = C64.GetMemory(C64Address.FromLabel("hud_MapCellRow-1"), y);
        column_ = 31 + (x >> 1);
        mask_ = 0xF0;
        blink_ = 0x50;
        if ((x & 1) != 0)
        {
            mask_ = 0x0F;
            blink_ = 0x05;
        }
        Store(C64.GetMemory(C64Address.FromLabel("hud_Map"), index_) | mask_);                       // this one: bright
    }

    // The mark's blink.
    public static void Flip()
    {
        Store(C64.GetMemory(C64Address.FromLabel("hud_Map"), index_) ^ blink_);
    }

    static void Store(uint value)
    {
        C64.FillMemory(C64Address.FromLabel("hud_Map-1") + index_, value, 2);
        C64.Screen.DrawTiles(column_, row_, 1, 1, C64Address.FromLabel("hud_Map"), index_ >> 3);
    }
}
