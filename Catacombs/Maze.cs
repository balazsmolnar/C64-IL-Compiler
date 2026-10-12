using C64Lib;

namespace Catacombs;

// A grid of solid/floor cells (1 = floor, 0 = solid rock), with a full
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
        0,0,0,0,0,0,0,0,0,0,0,0,
        0,1,1,1,0,1,1,1,1,1,1,0,
        0,1,0,1,0,1,0,0,0,0,1,0,
        0,1,0,1,1,1,0,1,1,1,1,0,
        0,1,0,0,0,1,0,1,0,0,1,0,
        0,1,1,1,0,1,1,1,0,1,1,0,
        0,0,0,1,0,0,0,1,0,1,0,0,
        0,1,1,1,1,1,0,1,1,1,1,0,
        0,1,0,0,0,1,0,0,0,0,1,0,
        0,1,1,1,0,1,1,1,1,1,1,0,
        0,0,0,1,1,1,0,0,0,0,1,0,
        0,0,0,0,0,0,0,0,0,0,0,0,
    };

    // The room at (x, y) has a palette theme (RoomArt.ThemeCount of them:
    // 0 moss, 1 cold, 2 rust, 3 crypt, 4 bone -- see art/room_art.py) and a
    // set of props (DungeonView's prop bits). Themes go by region, 4x4
    // cells each, so walking the maze passes through differently coloured
    // parts of the catacomb; props vary from room to room within a region.
    public static uint Theme(uint x, uint y)
    {
        return ((x / 4 + (y / 4) * 3) * 2) % RoomArt.ThemeCount;
    }

    static readonly uint[] PropSets =
    {
        0,
        DungeonView.Chains,
        DungeonView.Bones | DungeonView.Banner,
        DungeonView.Puddle,
        DungeonView.Grate | DungeonView.Chains,
        DungeonView.Banner,
        DungeonView.Bones,
        DungeonView.Puddle | DungeonView.Banner,
    };

    public static uint Props(uint x, uint y)
    {
        return PropSets[(x * 3 + y * 5) & 7];
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
