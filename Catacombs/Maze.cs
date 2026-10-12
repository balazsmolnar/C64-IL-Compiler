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
          0, 32, 40, 36,  0,113,106,102,162,160,168,  0,
          0, 34,  0, 40,  0, 97,  0,  0,  0,  0,160,  0,
          0, 38,  0, 32,104,100,  0,113,170,166,162,  0,
          0, 74,  0,  0,  0,136,  0,129,  0,  0, 38,  0,
          0, 81, 74, 70,  0,128,136,132,  0, 49, 42,  0,
          0,  0,  0, 74,  0,  0,  0,136,  0, 33,  0,  0,
          0, 68, 65, 81,138,134,  0,128, 40, 36, 33,  0,
          0,104,  0,  0,  0,170,  0,  0,  0,  0, 68,  0,
          0, 96,104,100,  0,177,170,166, 66, 64, 72,  0,
          0,  0,  0,104,164,161,  0,  0,  0,  0, 64,  0,
          0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    };

    // What a floor cell's Grid value means: bits 5-7 are the room's palette
    // theme + 1 (themes 0 moss, 1 cold, 2 rust, 3 crypt, 4 bone -- see
    // art/room_art.py; the + 1 keeps every floor cell nonzero), bits 0-4 its
    // props (DungeonView's prop bits: 1 chains, 2 banner, 4 bones, 8 puddle,
    // 16 grate). Themes go by region, 4x4 cells each, so walking the maze
    // passes through differently coloured parts of the catacomb; props vary
    // from room to room within a region. To carve a new room, give it its
    // region's theme and any props.
    public static uint Info(uint x, uint y)
    {
        return Grid[y * Width + x];
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
