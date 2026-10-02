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

    // Each cell's room color -- a Colors value (0-15), same 12x12 layout as
    // Grid, used for both the screen background and the border while the
    // player is in that room (see Program.cs). Only the floor cells' entries
    // are ever read; solid cells just carry the pattern along. The palette
    // is deliberately limited to colors that stay legible with the white
    // lines and brown doors drawn on top: no White, Brown, Yellow, Cyan or
    // LightGreen (too close to the lines or the doors), leaving Black(0),
    // Red(2), Violet(4), Green(5), Blue(6), LightRed(10), Grey1(11),
    // Grey2(12), LightBlue(14). Laid out so that any two orthogonally
    // adjacent cells always differ.
    static readonly uint[] RoomColors =
    {
        0,5,11,0,5,11,0,5,11,0,5,11,
        10,14,4,10,14,4,10,14,4,10,14,4,
        2,6,12,2,6,12,2,6,12,2,6,12,
        11,0,5,11,0,5,11,0,5,11,0,5,
        4,10,14,4,10,14,4,10,14,4,10,14,
        12,2,6,12,2,6,12,2,6,12,2,6,
        5,11,0,5,11,0,5,11,0,5,11,0,
        14,4,10,14,4,10,14,4,10,14,4,10,
        6,12,2,6,12,2,6,12,2,6,12,2,
        0,5,11,0,5,11,0,5,11,0,5,11,
        10,14,4,10,14,4,10,14,4,10,14,4,
        2,6,12,2,6,12,2,6,12,2,6,12,
    };

    public static Colors RoomColor(uint x, uint y)
    {
        return (Colors)RoomColors[y * Width + x];
    }

    // The same value as a plain uint, for callers that need to do
    // arithmetic on it (this compiler has no arithmetic on enum types).
    public static uint RoomColorValue(uint x, uint y)
    {
        return RoomColors[y * Width + x];
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
