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

    // Per-monster patrol position within its room (room-space X, see
    // RoomProjection.cs) -- bounces between PatrolMin/PatrolMax, same
    // "flip direction at the edge" shape Spider.Animate uses. Depth stays
    // fixed (see RoomDepth below), so this only ever drifts sideways,
    // roughly guarding its spot rather than wandering to a door.
    const uint PatrolCenter = 50, PatrolMin = 30, PatrolMax = 70;
    static uint[] roomX_;
    static bool[] movingRight_;

    public static void Init()
    {
        alive_ = new bool[Count];
        hp_ = new uint[Count];
        roomX_ = new uint[Count];
        movingRight_ = new bool[Count];
        for (uint i = 0; i < Count; i++)
        {
            alive_[i] = true;
            hp_[i] = MaxHp;
            roomX_[i] = PatrolCenter;
            movingRight_[i] = true;
        }
    }

    // Index of the live monster guarding (x,y), or Count (never a valid
    // index) if there isn't one -- callers compare against Count directly
    // instead of needing a separate "found" bool.
    public static uint FindAt(uint x, uint y)
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
        return i < Count ? roomX_[i] : PatrolCenter;
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
    const uint GiantSpider = 3;
    const uint Front = SpriteStage.Feature, Back = SpriteStage.Feature + 4;    // its two sprites
    static readonly uint[] KindColor = { 0x81, 0x85, 0x8E, 0x82 };  // white, green, light blue, red (+ multicolour)

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

        uint i = guard_;
        if ((tick_ & 3) == 0)
        {
            if (movingRight_[i])
            {
                roomX_[i] = roomX_[i] + 1;
                if (roomX_[i] > PatrolMax)
                    movingRight_[i] = false;
            }
            else
            {
                roomX_[i] = roomX_[i] - 1;
                if (roomX_[i] < PatrolMin)
                    movingRight_[i] = true;
            }
            SpriteStage.X[1] = RoomProjection.ScreenX(roomX_[i], RoomDepth);
        }

        uint step = (tick_ >> 3) & 1;
        uint first = SpriteStage.Mc + Frame.Guardian + i + i + i;
        if (i == GiantSpider)
        {
            SpriteStage.Parts[Front + SpriteStage.Frame] = first + step;
            SpriteStage.Parts[Back + SpriteStage.Frame] = first + 2 + step;
        }
        else
        {
            SpriteStage.Parts[Front + SpriteStage.Frame] = first;
            SpriteStage.Parts[Back + SpriteStage.Frame] = first + 1 + step;
        }
    }

    static void Setup(uint px, uint py)
    {
        feature_ = Nothing;
        SpriteStage.Y[1] = 0;
        guard_ = FindAt(px, py);
        if (guard_ < Count)
        {
            // Stacked: top above the legs. The spider: left and right halves.
            uint dx = 12, dy = 12, dx2 = 12;
            if (guard_ == GiantSpider)
            {
                dx = 0;
                dy = 33;
                dx2 = 24;
            }
            SpriteStage.Part(Front, KindColor[guard_], dx, dy);
            SpriteStage.Part(Back, KindColor[guard_], dx2, 33);
            SpriteStage.X[1] = RoomProjection.ScreenX(roomX_[guard_], RoomDepth);
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

    // The encounter picture and its health bar are Encounter.cs.
}
