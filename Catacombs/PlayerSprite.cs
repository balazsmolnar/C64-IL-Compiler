using C64Lib;

namespace Catacombs;

// The player, shown as a hardware sprite that moves via plain X/Y register
// pokes, no bitmap redraw at all -- see DungeonView's own comment for why
// the room itself is only ever drawn once per entry.
//
// This class only tracks the player's own LOGICAL sprite state (position,
// walk-cycle frame) -- it no longer writes any hardware register itself.
// Which physical sprite (0/1/2) actually shows the player changes frame to
// frame depending on depth ranking against the two spiders (see
// SpriteStage.cs/Program.cs's own UpdateZOrder), so the actual register
// writes happen there, once per frame, for whichever creature currently
// ranks into each slot.
//
// 2-frame walk cycle copied from Hunchback (see Sprites.cs's own comment on
// where the art came from). Multicolor, unlike the spiders -- needs both
// the per-sprite MultiColor flag (set per-frame by SpriteStage, since
// MultiColor is a per-PHYSICAL-sprite register that has to follow whichever
// creature currently occupies that slot) and the two SHARED palette
// registers ($D025/$D026, not exposed as a C64Lib property -- poked
// directly). Double-sized (ExpandX/ExpandY) so the character reads clearly
// against the room's own scale.
static class PlayerSprite
{
    // Blocks 1/2 of the sprite data gap ($3F80/$3FC0 -- see Sprites.cs).
    static readonly uint[] FramePointer = { 0xFE, 0xFF };

    // Sprite_set_Y (asm/c64sprite.asm) writes straight to the VIC-II's raw
    // hardware Y register with no conversion -- unlike bitmap-drawn Y
    // coordinates (DrawCircle/DrawRectangle/DungeonView's own FY0/FY1
    // constants, and RoomProjection's own ScreenY), which are relative to
    // the visible picture's own top edge (bitmap Y=0). The standard C64
    // offset between the two is +50 (the visible 200-line picture starts
    // at raster line 50).
    //
    // Y is also the sprite's TOP edge, not its center or bottom -- and this
    // sprite is double-height (42px, ExpandY). Positioning by bitmapY+50
    // would push a 42px-tall sprite's BOTTOM well past the visible area, so
    // this subtracts the doubled height instead, positioning by the
    // sprite's BOTTOM edge (bitmapY+50-42 = bitmapY+8), which is what
    // should actually line up with the floor.
    const uint SpriteHeight = 42; // 21px base, doubled by ExpandY
    const uint SpriteYOffset = 50 - SpriteHeight;

    static ulong x_;
    static uint bitmapY_;
    static uint walkTick_;
    static uint frame_;

    public static void Init(ulong x, ulong y)
    {
        walkTick_ = 0;
        frame_ = 0;

        // Blue/Yellow match the original sprite art's own mcolour1=6/
        // mcolour2=7 -- shared regs, safe to set once regardless of which
        // physical sprite slot ends up showing the player on any given
        // frame.
        Poke(0xD025UL, (uint)Colors.Blue);
        Poke(0xD026UL, (uint)Colors.Yellow);

        SetPosition(x, y);
    }

    // Advances the walk-cycle frame roughly every 6 calls (Program.cs calls
    // this once per frame the player actually moves).
    public static void SetPosition(ulong x, ulong y)
    {
        x_ = x;
        bitmapY_ = (uint)y;

        walkTick_ = walkTick_ + 1;
        if (walkTick_ % 6 == 0)
            frame_ = (frame_ + 1) % 2;
    }

    public static ulong X => x_;

    // Bitmap-space Y (pre-hardware-offset) -- this doubles as the player's
    // depth-sort key for SpriteStage's z-ordering and for Spider's own
    // collision check, both of which need the same coordinate space the
    // spiders' own floor Y constants are already given in.
    public static uint BitmapY => bitmapY_;

    public static uint RawY => bitmapY_ + SpriteYOffset;
    public static uint Pointer => FramePointer[frame_];

    // FillMemory writes offsets 1..size of its target (its loop never
    // touches offset 0), so writing one byte at `address` means filling one
    // byte starting one below it.
    static void Poke(ulong address, uint value)
    {
        C64.FillMemory(address - 1UL, value, 1);
    }
}
