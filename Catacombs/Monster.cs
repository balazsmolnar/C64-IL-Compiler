using C64Lib;

namespace Catacombs;

// A handful of monsters guarding specific rooms, patrolling side to side
// within them -- unlike Spider (pure ambient decoration, no player
// interaction at all), these block the player's path: HandleInput
// (Program.cs) diverts into Combat instead of just walking in, and the
// room stays blocked until its monster is killed.
//
// Rendered as an overlay on top of DungeonView's own corridor/room render,
// not integrated into its perspective depth scaling -- an encounter always
// shows the monster the same size, front and center, regardless of which
// room it's guarding. Simpler, and correctness here matters more than a
// receding-with-distance effect for a first pass.
static class Monster
{
    public const uint MaxHp = 30;
    public const uint MonsterDamage = 4;

    const uint Count = 4;

    // Maze cells (see Maze.cs's Grid) confirmed as floor, not wall, and
    // spread away from the player's start room (1,1).
    static readonly uint[] SpawnX = { 5, 9, 1, 7 };
    static readonly uint[] SpawnY = { 3, 5, 8, 9 };

    static bool[] alive_;
    static uint[] hp_;

    // The guardian paces across the middle of its room, in screen pixels
    // (see Animate), at a fixed depth.
    const uint PatrolCenter = 50;
    const ulong PatrolLeft = 116, PatrolRight = 204;
    static ulong guardX_;
    static bool guardRight_;

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
            Refresh();
        }
        else
        {
            hp_[i] = hp_[i] - amount;
        }
    }

    // Fixed depth within a room, in ROOM-space (see RoomProjection.cs) --
    // only X patrols (above); Program.cs compares the player's own
    // room-space position against CurrentRoomX(px,py)/RoomDepth to detect
    // touching. Partway toward the far wall.
    public const uint RoomDepth = 50;
    public const ulong RoomRadius = 20; // screen-space, not perspective-scaled (a simplification)

    // Valid any time -- callers that don't already know a monster is here
    // just get PatrolCenter back (harmless, since Program.cs only ever
    // uses this guarded by monsterHere_, same precondition HpAt/DamageAt
    // already have).
    public static uint CurrentRoomX(uint px, uint py)
    {
        uint i = FindAt(px, py);
        return PatrolCenter;
    }

    // --- The room's Feature (SpriteStage actor 1): its guardian or, in a
    // room without one, its pickup (Item.cs). No room has both.
    //
    // Guardians, one kind per guarded room, two sprites each: skeleton
    // warrior, ghoul, wraith (stacked top and legs) and the giant spider
    // (two sprites side by side). They pace their patrol slowly; the legs
    // (or the wraith's rags, or the spider's eight) step as they go.
    const uint NoRoom = 255;
    const uint Nothing = 0, Guardian = 1, Pickup = 2;
    const uint Front = SpriteStage.Feature, Back = SpriteStage.Feature + 4;    // its two sprites
    static readonly uint[] KindColor = { 0x8F, 0x85, 0x8E, 0x84 };  // light grey, green, light blue, violet (+ multicolour)

    static uint room_ = NoRoom;     // the room the Feature was set up for
    static uint feature_;
    static uint guard_;
    static uint tick_;

    // Makes the next Animate look at the room again: something in it was
    // killed or collected.
    public static void Refresh()
    {
        room_ = NoRoom;
    }

    // Takes the guardian off the stage (the combat screen shows its own).
    public static void HideSprite()
    {
        SpriteStage.Y[1] = 0;
        room_ = NoRoom;
        feature_ = Nothing;
    }

    // Once per video frame, from Program.cs's main loop (and once more from
    // EnterRoom, so a fresh room is right at once).
    public static void Animate(uint px, uint py)
    {
        tick_ = tick_ + 1;
        uint key = (px << 4) + py;
        if (key != room_)
        {
            room_ = key;
            Setup(px, py);
        }

        if (feature_ == Pickup)
            Item.Twinkle(tick_);
        if (feature_ != Guardian)
            return;

        // A slow, heavy pace: a pixel every other frame, a step every 8
        // frames, the body dipping as each foot lands.
        if ((tick_ & 1) == 0)
        {
            if (guardRight_)
            {
                guardX_ = guardX_ + 1;
                if (guardX_ > PatrolRight)
                    guardRight_ = false;
            }
            else
            {
                guardX_ = guardX_ - 1;
                if (guardX_ < PatrolLeft)
                    guardRight_ = true;
            }
            SpriteStage.X[1] = guardX_;
        }
        uint step = (tick_ >> 3) & 1;
        uint first = SpriteStage.Mc + Frame.Guardian + guard_ + guard_ + guard_;
        SpriteStage.Parts[Front + SpriteStage.Frame] = first;
        SpriteStage.Parts[Front + SpriteStage.Dy] = 12 + step;
        SpriteStage.Parts[Back + SpriteStage.Frame] = first + 1 + step;
    }

    static void Setup(uint px, uint py)
    {
        feature_ = Nothing;
        SpriteStage.Y[1] = 0;
        guard_ = FindAt(px, py);
        if (guard_ < Count)
        {
            // Two sprites stacked: the top above the legs.
            SpriteStage.Part(Front, KindColor[guard_], 12, 12);
            SpriteStage.Part(Back, KindColor[guard_], 12, 33);
            guardX_ = 160;
            SpriteStage.X[1] = guardX_;
            SpriteStage.Y[1] = (uint)RoomProjection.ScreenY(RoomDepth);
            feature_ = Guardian;
        }
        else if (Item.Setup(px, py))
            feature_ = Pickup;

        if (feature_ != Nothing)
        {
            SpriteStage.Count[1] = 2;
            SpriteStage.Y[3] = 0;   // the bat: its sprite is needed here
        }
    }

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
