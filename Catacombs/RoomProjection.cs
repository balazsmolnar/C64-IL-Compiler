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

    // Mirrors DungeonView's own FX0Near/FX1Near/FX0Far/FX1Far, inset by
    // half the player sprite's width (48px now that PlayerSprite doubles
    // it via ExpandX, see Sprites.cs/PlayerSprite.cs) plus a little slack
    // -- the wall LINE position itself isn't a safe bound for where a
    // sprite's own X can go: a sprite is drawn with that X as its left
    // edge, so sitting right at the true wall line put roughly the left
    // half of it past the visible screen edge and off-screen (confirmed
    // empirically via VICE -- the sprite vanished at roomX=0, back when
    // the sprite was still single-width). Insetting both near and far by
    // the same fixed amount is simpler than scaling the inset with the
    // frame's own width, and the far frame (219px wide) still has room
    // left after it.
    const ulong Margin = 40;
    const ulong FX0Near = 4 + Margin, FX1Near = 315 - Margin;
    const ulong FX0Far = 50 + Margin, FX1Far = 269 - Margin;

    // Where the player's FEET (see PlayerSprite's own bottom-edge
    // positioning) should be, not the sprite's top: 199 is the very
    // bottom of the visible 200-line picture (depth=0, standing right at
    // the front of your own view) down to 173 -- DungeonView's own
    // FY1Far, the far frame's actual floor line -- at DepthMax, so
    // reaching the far wall visually plants the character's feet exactly
    // on that line instead of floating above or sinking below it.
    const ulong ScreenYNear = 199, ScreenYFar = 173;

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
