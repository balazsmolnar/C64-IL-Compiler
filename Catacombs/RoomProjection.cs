namespace Catacombs;

// Converts a logical position within a room into the screen coordinates to
// actually draw/position something at. Program.cs and Monster/Item only
// ever deal in room-space:
//   roomX:     0 (against the left wall) .. Width (against the right
//              wall), Width/2 = center -- ALWAYS this same range,
//              regardless of depth.
//   roomDepth: 0 (at the entrance, near the viewer) .. DepthMax (at the
//              far wall/door).
// The room's walls converge toward the far frame (see DungeonView's own
// near/far frame constants, mirrored below), so projecting roomX through
// the bounds AT THE CURRENT DEPTH means someone hugging the side wall
// (roomX at its extreme) is walked visually toward the screen's center as
// they go deeper -- exactly like a real perspective view -- without
// Program.cs ever needing to know the room's vector geometry itself.
// Both roomX and roomDepth are plain uint (unsigned): using 0..Width
// instead of a signed +/-HalfWidth range sidesteps needing signed
// arithmetic anywhere in the conversion.
static class RoomProjection
{
    public const uint Width = 100;     // 0=left wall, 50=center, 100=right wall
    public const uint Center = Width / 2;
    public const uint DepthMax = 100;  // 0=entrance, DepthMax=far wall

    // CHARACTERS BRANCH STAND-IN (take the Rooms part's version of this
    // file on merge): the fixed room geometry of AAA-BRIEF.md, so that
    // sprites stand where the repainted room's floor will be. The walkable
    // floor: depth 0 at y 160, DepthMax at y 124 (the foot of the far wall
    // is y 120), and at each depth the floor trapezoid's width less a
    // margin for half a sprite. ScreenX / ScreenY give the bitmap pixel
    // between a character's feet.
    const ulong FX0Near = 26, FX1Near = 293;
    const ulong FX0Far = 74, FX1Far = 245;
    const ulong ScreenYNear = 160, ScreenYFar = 124;

    public static ulong ScreenX(uint roomX, uint depth)
    {
        ulong left = Lerp(FX0Near, FX0Far, depth);
        ulong right = Lerp(FX1Near, FX1Far, depth);
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
