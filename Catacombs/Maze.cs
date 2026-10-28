using C64Lib;

namespace Catacombs;

// A grid of solid/floor cells (nonzero = floor, see Info; 0 = solid rock), with a full
// border of solid cells so a lookahead from any legal player position never
// needs bounds-checking beyond what IsWall already does (see its own
// comment). Facing direction: 0 = north (-Y), 1 = east (+X), 2 = south
// (+Y), 3 = west (-X) -- matches a normal screen coordinate system (Y grows
// downward), not compass math.
static class Maze
{
    public const uint Width = 12;
    public const uint Height = 12;

    static readonly byte[] Grid =
    {
          0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
          0, 42,105,140,  0,138,162,130,162, 41, 98,  0,
          0, 67,  0,169,  0,165,  0,  0,  0,  0,138,  0,
          0, 34,  0, 42, 96, 36,  0,132,102, 35, 76,  0,
          0, 76,  0,  0,  0,136,  0, 75,  0,  0, 40,  0,
          0, 33,101, 67,  0, 78,143, 38,  0,132, 78,  0,
          0,  0,  0,129,  0,  0,  0,137,  0,166,  0,  0,
          0, 97,175, 32, 79, 98,  0,109, 64,110,140,  0,
          0,130,  0,  0,  0,138,  0,  0,  0,  0, 47,  0,
          0, 69, 42,130,  0, 43, 67,130,171, 42,142,  0,
          0,  0,  0, 99,173, 70,  0,  0,  0,  0,163,  0,
          0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    };

    // What a floor cell's Grid value means: bits 5-7 are the room's palette
    // theme + 1 (themes 0 moss, 1 cold, 2 rust, 3 crypt, 4 bone -- see
    // art/room_art.py; the + 1 keeps every floor cell nonzero), bits 0-4 an
    // index into PropSets, the room's props (DungeonView's prop bits).
    // Neighbouring rooms have different themes. To carve a new room, give
    // it a theme and a prop set.
    public static uint Info(uint x, uint y)
    {
        return Grid[y * Width + x];
    }

    static readonly uint[] PropSets =
    {
        0,
        DungeonView.Niche,
        DungeonView.Bones | DungeonView.Puddle,
        DungeonView.Bones | DungeonView.Banner,
        DungeonView.Puddle,
        DungeonView.Grate | DungeonView.Banner,
        DungeonView.Banner | DungeonView.Grate,
        DungeonView.Bones | DungeonView.Niche,
        DungeonView.Dark | DungeonView.Puddle,
        DungeonView.Puddle | DungeonView.Banner,
        DungeonView.DarkRight | DungeonView.Bones,
        DungeonView.Grate,
        DungeonView.Grate | DungeonView.Puddle,
        DungeonView.Bones | DungeonView.Grate,
        DungeonView.DarkRight | DungeonView.Banner,
        DungeonView.Banner | DungeonView.Niche,
    };

    public static uint Props(uint x, uint y)
    {
        return PropSets[Info(x, y) & 15];
    }

    // The screen background and border are black in every room (the room
    // picture brings its own colours). Program.cs still asks per room.
    public static Colors RoomColor(uint x, uint y)
    {
        return Colors.Black;
    }

    public static uint RoomColorValue(uint x, uint y)
    {
        return 0;
    }

    // Out-of-range coordinates read as solid: a step that would take x or y
    // negative wraps to a large uint (this compiler's int/uint are 8-bit,
    // see CLAUDE.md), which this same ">=" check still correctly rejects --
    // no separate underflow handling needed anywhere that calls this.
    public static bool IsWall(uint x, uint y)
    {
        if (x >= Width || y >= Height)
            return true;
        return Grid[y * Width + x] == 0;
    }

    // Absolute compass neighbors of (x,y) -- rooms are always rendered
    // north-up (see DungeonView's own comment), so there's no more
    // "facing"-relative left/right/ahead: north is always the far wall,
    // south the near one, west/east the left/right walls, regardless of
    // which wall the player actually walked in through.
    public static bool NorthIsWall(uint x, uint y)
    {
        return IsWall(x, y - 1);
    }

    public static bool SouthIsWall(uint x, uint y)
    {
        return IsWall(x, y + 1);
    }

    public static bool WestIsWall(uint x, uint y)
    {
        return IsWall(x - 1, y);
    }

    public static bool EastIsWall(uint x, uint y)
    {
        return IsWall(x + 1, y);
    }
}
