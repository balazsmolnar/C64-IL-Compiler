using C64Lib;

namespace Catacombs;

// Two spiders scurrying side to side on the floor of every room. Y is FIXED
// at floor level (no bobbing) -- running, not flying.
//
// Like PlayerSprite, this only tracks LOGICAL state (X position, color,
// which never changes visually except X) -- it no longer writes any
// hardware register itself. Which physical sprite (0/1/2) shows which
// spider changes frame to frame depending on depth ranking against the
// player (see SpriteStage.cs/Program.cs's own UpdateZOrder).
static class Spider
{
    const uint PosePointer = 0xFD;

    // Floor-level Y, not bobbing -- same "position by the bottom edge"
    // idea as PlayerSprite (see its own comment): bitmapY+50-21 puts a
    // single-height (21px) sprite's FEET at bitmapY, not its top. Two
    // slightly different floor depths (185/195, both near the room's own
    // near-floor line at bitmap Y=195 -- DungeonView's FY1Near) so the two
    // spiders don't run in lockstep at the exact same row -- this also
    // gives them a real (if small and fixed) depth difference from each
    // other, which is what makes SpriteStage's z-ordering ever matter
    // between the two of them, not just against the player.
    const uint SpriteHeight = 21;
    const uint SpriteYOffset = 50 - SpriteHeight;
    public const uint BitmapY0 = 185, BitmapY1 = 195;

    static ulong x0_, x1_;
    static bool right0_, right1_;
    static uint tick_;
    static Colors color_;

    public static void Init()
    {
        x0_ = 60;
        x1_ = 250;
        right0_ = true;
        right1_ = false;
        tick_ = 0;
        color_ = Colors.Black;
    }

    // Black spiders, except in the black room, where they'd vanish.
    public static void SetRoomColor(uint roomColor)
    {
        color_ = roomColor == 0 ? Colors.White : Colors.Black;
    }

    // One animation tick: scurry both spiders side to side (Y never
    // changes -- they stay on the floor). Meant to be called once per video
    // frame, from wherever the game loop currently is (Program.cs's main
    // loop and Combat's own wait loop both call this) -- the actual
    // on-screen sprite writes happen separately, via SpriteStage, so
    // callers that animate must also re-run z-ordering the same frame or
    // the visible sprite positions go stale.
    public static void Animate()
    {
        tick_ = tick_ + 1;

        if (right0_)
        {
            x0_ = x0_ + 2;
            if (x0_ > 290)
                right0_ = false;
        }
        else
        {
            x0_ = x0_ - 2;
            if (x0_ < 40)
                right0_ = true;
        }

        if (right1_)
        {
            x1_ = x1_ + 3;
            if (x1_ > 290)
                right1_ = false;
        }
        else
        {
            x1_ = x1_ - 3;
            if (x1_ < 40)
                right1_ = true;
        }
    }

    public static ulong X0 => x0_;
    public static ulong X1 => x1_;
    public static Colors Color => color_;
    public static uint RawY0 => BitmapY0 + SpriteYOffset;
    public static uint RawY1 => BitmapY1 + SpriteYOffset;
    public static uint Pointer => PosePointer;

    // Calculated collision, not VIC-II hardware collision: hardware
    // sprite-sprite collision (Sprite.IsInCollision / SpriteCollection's
    // own Collisions register) reports overlap in raw screen pixels only,
    // which doesn't account for the perspective this game is built on --
    // Monster/Item's own encounters are already deliberately calculated in
    // room-space (see RoomProjection.cs and Program.Touching) rather than
    // trusting anything hardware-hit-tested, and this follows the same
    // principle for the player's own hardware sprite against the spiders'.
    // Compares the SAME bitmap-space coordinates both the player
    // (PlayerSprite.X/BitmapY) and these spiders are already positioned
    // from, so it's an exact screen-space distance check, not an
    // approximation.
    public static bool IsCollidingWith(ulong x, ulong y, ulong radius)
    {
        return Near(x0_, BitmapY0, x, y, radius) || Near(x1_, BitmapY1, x, y, radius);
    }

    static bool Near(ulong ex, ulong ey, ulong px, ulong py, ulong radius)
    {
        ulong dx = ex > px ? ex - px : px - ex;
        ulong dy = ey > py ? ey - py : py - ey;
        return dx < radius && dy < radius;
    }
}
