using C64Lib;

namespace Catacombs;

// The panel's map (see Hud.cs): the whole 12x12 maze in panel columns 33-38,
// one maze cell = 4x2 hi-res pixels (48x24 pixels = 6x3 screen cells). Only
// rooms the player has been in are drawn (fog of war); the room the player
// is in blinks.
//
// The map lives in hud_Map (HudArt.cs): the 18 screen cells' bitmap bytes,
// as tiles. A room is half a byte on two pixel rows, so revealing or
// blinking one changes two bytes there; the map is then copied to the
// screen in one go. The maze's solid outer ring is never a room: its pixels
// hold the map's frame.
static class Minimap
{
    static uint index_, mask_;  // the current room: its first byte in hud_Map, its half of that byte
    static bool hidden_;        // the blink's dark half

    public static bool Hidden => hidden_;

    public static void Enter(uint x, uint y)
    {
        if (hidden_)
            Flip();             // leave the old room drawn
        index_ = C64.GetMemory(C64Address.FromLabel("hud_MapRow"), y) + (x & 0xFE) * 4;
        mask_ = 0xF0;
        if ((x & 1) != 0)
            mask_ = 0x0F;
        Store(C64.GetMemory(C64Address.FromLabel("hud_Map"), index_) | mask_);
    }

    // Blinks the current room.
    public static void Flip()
    {
        hidden_ = !hidden_;
        Store(C64.GetMemory(C64Address.FromLabel("hud_Map"), index_) ^ mask_);
    }

    public static void Draw()
    {
        C64.Screen.DrawTiles(33, 22, 6, 3, C64Address.FromLabel("hud_Map"), 0);
    }

    static void Store(uint value)
    {
        C64.FillMemory(C64Address.FromLabel("hud_Map-1") + index_, value, 2); // FillMemory writes bytes 1..size
        Draw();
    }
}
