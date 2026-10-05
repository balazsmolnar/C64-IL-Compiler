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
        }
    }

    public static uint CollectedCount => collectedCount_;

    // Small distinct marker (yellow-ish via ColorRam's fixed Brown --
    // reuses the door color deliberately, both read as "things in the
    // world," distinct from Monster's MatrixLow/LightRed) drawn once as
    // part of DungeonView's static room scene, same idea as
    // Monster.RenderInRoom.
    public static void Render()
    {
        ulong x = RoomProjection.ScreenX(RoomX, RoomDepth);
        ulong y = RoomProjection.ScreenY(RoomDepth);
        C64.Screen.DrawRectangle(x - RoomRadius, y - RoomRadius, x + RoomRadius, y + RoomRadius, true, true, BitmapColorSource.ColorRam);
    }
}
