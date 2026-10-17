namespace Catacombs;

// Converts a logical position within a room into the screen position of
// that point of the floor. Program.cs and Monster/Item only ever deal in
// room-space:
//   roomX:     0 (against the left wall) .. Width (against the right
//              wall), Width/2 = center -- ALWAYS this same range,
//              regardless of depth.
//   roomDepth: 0 (the near edge of the floor, by the south exit) ..
//              DepthMax (the foot of the far wall / the north door).
// The result is in bitmap pixels (x 0-319, y 0-199): where something
// standing on that spot touches the floor, i.e. the bottom centre of a
// sprite. (Sprite registers: X = ScreenX + 24 - half the sprite's width,
// Y = ScreenY + 50 - its height.)
//
// The floor of the room picture (DungeonView / art/room_art.py) is the
// trapezoid between the foot of the far wall (y 120, x 64..255) and the
// bottom of the viewport, its sides running along the lines through the
// vanishing point (160, 56). The walkable area is that trapezoid from y 124
// to y 160, kept a little inside the side walls so a sprite of ordinary
// width does not overlap them. Both edges are straight lines, so a plain
// linear interpolation in depth is exact.
static class RoomProjection
{
    public const uint Width = 100;     // 0=left wall, 50=center, 100=right wall
    public const uint Center = Width / 2;
    public const uint DepthMax = 100;  // 0=near edge, DepthMax=far wall

    // The floor's own edges are x = 4 / 315 at y 160 and x = 58 / 261 at
    // y 124; the walkable band is inset by 24 pixels near and 16 far.
    const ulong XLeftNear = 28, XRightNear = 291;
    const ulong XLeftFar = 74, XRightFar = 245;
    const ulong ScreenYNear = 160, ScreenYFar = 124;

    public static ulong ScreenX(uint roomX, uint depth)
    {
        ulong left = Lerp(XLeftNear, XLeftFar, depth);
        ulong right = Lerp(XRightNear, XRightFar, depth);
        return left + (right - left) * roomX / Width;
    }

    public static ulong ScreenY(uint depth)
    {
        return Lerp(ScreenYNear, ScreenYFar, depth);
    }

    // from at depth=0, to at depth=DepthMax, straight line between --
    // ulong-safe (no underflow) regardless of whether from<to or from>to.
    static ulong Lerp(ulong from, ulong to, uint depth)
    {
        if (depth >= DepthMax)
            return to;
        if (to > from)
            return from + (to - from) * depth / DepthMax;
        return from - (from - to) * depth / DepthMax;
    }
}
