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
    const uint Glint = SpriteStage.Feature, Thing = SpriteStage.Feature + 4;
    // The glint runs along the item in four steps, then rests.
    static readonly uint[] GlintDx = { 4, 7, 10, 13 };
    static readonly uint[] GlintDy = { 37, 36, 35, 34 };
    static readonly uint[] GlintColor = { (uint)Colors.Grey3, (uint)Colors.White, (uint)Colors.Yellow, (uint)Colors.White };

    public static bool Setup(uint px, uint py)
    {
        uint frame = Frame.Treasure;
        uint color = SpriteStage.Multi | (uint)Colors.Yellow;
        uint rx = RoomX;
        uint depth = RoomDepth;
        if (!IsPresentAt(px, py))
        {
            if (!Weapon.IsPresentAt(px, py))
                return false;
            frame = Frame.Sword + Weapon.FindAt(px, py);
            color = SpriteStage.Multi | (uint)Colors.Grey3;
            rx = Weapon.RoomX;
            depth = Weapon.RoomDepth;
        }
        SpriteStage.Part(Glint, (uint)Colors.White, 12, 0);
        SpriteStage.Parts[Glint + SpriteStage.Frame] = SpriteStage.Hi + Frame.Spark;
        SpriteStage.Part(Thing, color, 12, 33);
        SpriteStage.Parts[Thing + SpriteStage.Frame] = SpriteStage.Mc + frame;
        SpriteStage.X[1] = RoomProjection.ScreenX(rx, depth);
        SpriteStage.Y[1] = (uint)RoomProjection.ScreenY(depth);
        return true;
    }

    // A glint travelling along the blade or across the coins every second
    // or so; hidden in between (sprite Y 0 is above the screen).
    public static void Twinkle(uint tick)
    {
        uint step = (tick >> 2) & 15;
        uint dy = 0 - SpriteStage.Y[1];
        if (step < 4)
        {
            SpriteStage.Parts[Glint] = GlintDx[step];
            SpriteStage.Parts[Glint + SpriteStage.Color] = GlintColor[step];
            dy = GlintDy[step];
        }
        SpriteStage.Parts[Glint + SpriteStage.Dy] = dy;
    }
}
