using C64Lib;

namespace Catacombs;

// A handful of stationary monsters guarding specific rooms -- unlike Bats
// (pure ambient decoration, no player interaction at all), these block the
// player's path: MoveForward (Program.cs) diverts into Combat instead of
// just walking in, and the room stays blocked until its monster is killed.
//
// Rendered as an overlay on top of DungeonView's own corridor/room render,
// not integrated into its perspective depth scaling -- an encounter always
// shows the monster the same size, front and center, regardless of which
// room it's guarding. Simpler, and correctness here matters more than a
// receding-with-distance effect for a first pass.
static class Monster
{
    public const uint MaxHp = 30;
    public const uint PlayerDamage = 6;
    public const uint MonsterDamage = 4;

    const uint Count = 4;

    // Maze cells (see Maze.cs's Grid) confirmed as floor, not wall, and
    // spread away from the player's start room (1,1).
    static readonly uint[] SpawnX = { 5, 9, 1, 7 };
    static readonly uint[] SpawnY = { 3, 5, 8, 9 };

    static bool[] alive_;
    static uint[] hp_;

    public static void Init()
    {
        alive_ = new bool[Count];
        hp_ = new uint[Count];
        for (uint i = 0; i < Count; i++)
        {
            alive_[i] = true;
            hp_[i] = MaxHp;
        }
    }

    // Index of the live monster guarding (x,y), or Count (never a valid
    // index) if there isn't one -- callers compare against Count directly
    // instead of needing a separate "found" bool.
    static uint FindAt(uint x, uint y)
    {
        for (uint i = 0; i < Count; i++)
        {
            if (alive_[i] && SpawnX[i] == x && SpawnY[i] == y)
                return i;
        }
        return Count;
    }

    public static bool IsAliveAt(uint x, uint y)
    {
        return FindAt(x, y) < Count;
    }

    // Only valid right after IsAliveAt(x,y) returned true -- same
    // precondition FindAt's callers already have to satisfy.
    public static uint HpAt(uint x, uint y)
    {
        return hp_[FindAt(x, y)];
    }

    public static void DamageAt(uint x, uint y, uint amount)
    {
        uint i = FindAt(x, y);
        if (amount >= hp_[i])
        {
            hp_[i] = 0;
            alive_[i] = false;
        }
        else
        {
            hp_[i] = hp_[i] - amount;
        }
    }

    // Fixed spot within a room, in ROOM-space (see RoomProjection.cs) --
    // Program.cs compares the player's own room-space position against
    // this directly (cheap, no projection needed just to detect touching);
    // RenderInRoom below projects it to screen-space only when actually
    // drawing. Center-ish X, partway toward the far wall.
    public const uint RoomX = RoomProjection.Center, RoomDepth = 50;
    public const ulong RoomRadius = 20; // screen-space, not perspective-scaled (a simplification)

    // Simple silhouette: a body and two eyes. MatrixLow is a fixed color
    // (LightRed) set once in Program.Main and never changed, so it's
    // always safe to draw with here regardless of the current room's own
    // Background color.
    public static void Render()
    {
        C64.Screen.DrawCircle(160, 95, 35, true, true, BitmapColorSource.MatrixLow);
        C64.Screen.DrawCircle(148, 85, 5, true, true, BitmapColorSource.ColorRam);
        C64.Screen.DrawCircle(172, 85, 5, true, true, BitmapColorSource.ColorRam);
    }

    // The smaller, static-scene version -- drawn once as part of
    // DungeonView's own one-time room render, not full-screen like Render()
    // (which is only used for the Combat overlay once the player actually
    // reaches it).
    public static void RenderInRoom()
    {
        ulong x = RoomProjection.ScreenX(RoomX, RoomDepth);
        ulong y = RoomProjection.ScreenY(RoomDepth);
        C64.Screen.DrawCircle(x, y, RoomRadius, true, true, BitmapColorSource.MatrixLow);
        C64.Screen.DrawCircle(x - 7, y - 6, 3, true, true, BitmapColorSource.ColorRam);
        C64.Screen.DrawCircle(x + 7, y - 6, 3, true, true, BitmapColorSource.ColorRam);
    }

    // ulong, not uint: this compiler's uint/int are 8-bit (see CLAUDE.md),
    // and the right-hand bar's screen X (310) already doesn't fit one --
    // every coordinate here follows DungeonView.cs's own convention of
    // ulong for anything that touches Screen.Draw*.
    const ulong BarWidth = 100;
    const ulong BarHeight = 8;

    // Player's bar top-left, the current monster's top-right -- both
    // outlined (MatrixHigh, matching the walls) with a filled portion
    // (MatrixLow) proportional to remaining HP. HP arrives as uint (that's
    // what Program.cs's playerHp_ and HpAt above are), widened to ulong
    // immediately: playerHp*BarWidth can reach 10000, which overflows an
    // 8-bit uint multiply but fits comfortably in this compiler's 16-bit
    // ulong.
    public static void DrawHealthBars(uint playerHp, uint playerMaxHp, uint monsterHp)
    {
        ulong pHp = playerHp;
        ulong pMax = playerMaxHp;
        ulong mHp = monsterHp;

        C64.Screen.DrawRectangle(10, 8, 10 + BarWidth, 8 + BarHeight, false, true, BitmapColorSource.MatrixHigh);
        ulong playerFill = (pHp * BarWidth) / pMax;
        if (playerFill > 0)
            C64.Screen.DrawRectangle(10, 8, 10 + playerFill, 8 + BarHeight, true, true, BitmapColorSource.MatrixLow);

        ulong right = 310;
        C64.Screen.DrawRectangle(right - BarWidth, 8, right, 8 + BarHeight, false, true, BitmapColorSource.MatrixHigh);
        ulong monsterFill = (mHp * BarWidth) / MaxHp;
        if (monsterFill > 0)
            C64.Screen.DrawRectangle(right - monsterFill, 8, right, 8 + BarHeight, true, true, BitmapColorSource.MatrixLow);
    }
}
