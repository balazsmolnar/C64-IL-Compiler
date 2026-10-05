using C64Lib;

namespace Catacombs;

// Two weapon pickups scattered in the maze -- finding one replaces the
// player's current attack damage (Program.cs's playerDamage_) outright
// with the weapon's own value, a straight upgrade. Same spawn/collect
// pattern as Item.cs, plus a damage value to read BEFORE collecting
// (mirrors Monster.HpAt/DamageAt's own "read, then mutate" split --
// Item.CollectAt has nothing to read, so it isn't the right model here).
static class Weapon
{
    // A different fixed spot than Monster's (Center,50) and Item's
    // (25,75) so a room could in principle hold all three without the
    // markers overlapping.
    public const uint RoomX = 50, RoomDepth = 25;
    public const ulong RoomRadius = 10; // screen-space, not perspective-scaled

    const uint Count = 2;

    // Maze cells (see Maze.cs's Grid) confirmed as floor, distinct from
    // Monster's and Item's own spawn cells. Sword near the start room,
    // axe deeper in the maze -- a visible progression.
    static readonly uint[] SpawnX = { 3, 5 };
    static readonly uint[] SpawnY = { 1, 7 };
    static readonly uint[] Damage = { 12, 20 };

    static bool[] collected_;

    public static void Init()
    {
        collected_ = new bool[Count];
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

    // Only valid right after IsPresentAt(x,y) returned true -- same
    // precondition Monster.HpAt's own callers already have to satisfy.
    public static uint DamageAt(uint x, uint y)
    {
        return Damage[FindAt(x, y)];
    }

    public static void CollectAt(uint x, uint y)
    {
        uint i = FindAt(x, y);
        if (i < Count)
            collected_[i] = true;
    }

    // Small distinct marker (MatrixHigh/White -- the one BitmapColorSource
    // channel neither Monster's silhouette (MatrixLow) nor Item's marker
    // (ColorRam) already uses) drawn once as part of DungeonView's static
    // room scene, same idea as Item.Render/Monster.RenderInRoom.
    public static void Render()
    {
        ulong x = RoomProjection.ScreenX(RoomX, RoomDepth);
        ulong y = RoomProjection.ScreenY(RoomDepth);
        C64.Screen.DrawRectangle(x - RoomRadius, y - RoomRadius, x + RoomRadius, y + RoomRadius, true, true, BitmapColorSource.MatrixHigh);
    }
}
