using C64Lib;

namespace Catacombs;

// A few pickups scattered around the maze, collected when the player
// sprite touches its fixed spot within the room, in ROOM-space (see
// RoomProjection.cs and Monster.RoomX/RoomDepth's own comment).
static class Item
{
    // A different fixed spot than Monster's (Center,50) so a room could in
    // principle hold both without the markers overlapping -- left of
    // center, closer to the viewer.
    public const uint RoomX = 25, RoomDepth = 75;
    public const ulong RoomRadius = 10; // screen-space, not perspective-scaled

    public const uint Count = 3; // exposed for Hud.cs's pip row

    // Maze cells (see Maze.cs's Grid) confirmed as floor, distinct from
    // Monster's own spawn cells.
    static readonly uint[] SpawnX = { 7, 3, 9 };
    static readonly uint[] SpawnY = { 3, 5, 9 };

    static bool[] collected_;
    static uint collectedCount_;

    public static void Init()
    {
        collected_ = new bool[Count];
        collectedCount_ = 0;
    }

    static uint FindAt(uint x, uint y)
    {
        for (uint i = 0; i < Count; i++)
        {
            if (!collected_[i] && SpawnX[i] == x && SpawnY[i] == y)
                return i;
        }
        return Count;
    }

    public static bool IsPresentAt(uint x, uint y)
    {
        return FindAt(x, y) < Count;
    }

    public static void CollectAt(uint x, uint y)
    {
        uint i = FindAt(x, y);
        if (i < Count)
        {
            collected_[i] = true;
            collectedCount_ = collectedCount_ + 1;
            Monster.Refresh();
        }
    }

    public static uint CollectedCount => collectedCount_;

    // The treasure and the weapons used to be drawn into the room's bitmap;
    // they are sprites now (below), so there is nothing to draw. Kept for
    // Program.EnterRoom, which still calls it.
    public static void Render()
    {
    }

    // --- The pickup lying in this room, treasure or weapon, as the room's
    // Feature (SpriteStage actor 1; see Monster.Setup, which calls this when
    // the room has no guardian): the item itself with its shadow, and in
    // front of it a hi-res glint that flares and jumps about on it.
    static readonly uint[] GlintDx = { 8, 15, 11, 17 };
    static readonly uint[] GlintDy = { 28, 25, 33, 30 };

    public static bool Setup(uint px, uint py)
    {
        uint frame = Frame.Treasure;
        uint color = (uint)Colors.Yellow;
        uint rx = RoomX;
        uint depth = RoomDepth;
        if (!IsPresentAt(px, py))
        {
            if (!Weapon.IsPresentAt(px, py))
                return false;
            frame = Frame.Sword + Weapon.FindAt(px, py);
            color = (uint)Colors.Grey3;
            rx = Weapon.RoomX;
            depth = Weapon.RoomDepth;
        }
        SpriteStage.Part(3, SpriteStage.HiRes | (uint)Colors.White, 12, 28);
        SpriteStage.Part(4, color, 12, 33);
        SpriteStage.Frame[4] = SpriteStage.Mc + frame;
        SpriteStage.X[SpriteStage.Feature] = RoomProjection.ScreenX(rx, depth);
        SpriteStage.Y[SpriteStage.Feature] = (uint)RoomProjection.ScreenY(depth);
        return true;
    }

    // Small, large, small, then somewhere else on the item.
    public static void Twinkle(uint tick)
    {
        uint phase = (tick >> 3) & 3;
        uint flare = 0;
        if (phase == 1)
            flare = 1;
        SpriteStage.Frame[3] = SpriteStage.Hi + Frame.Spark + flare;
        if ((tick & 31) == 0)
        {
            uint spot = (tick >> 5) & 3;
            SpriteStage.Dx[3] = GlintDx[spot];
            SpriteStage.Dy[3] = GlintDy[spot];
        }
    }
}
