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

    // The cell 'dist' steps ahead of (x,y) facing dir (dist 0 = the cell the
    // viewer is standing in).
    public static bool AheadIsWall(uint x, uint y, uint dir, uint dist)
    {
        if (dir == 0) return IsWall(x, y - dist);
        if (dir == 1) return IsWall(x + dist, y);
        if (dir == 2) return IsWall(x, y + dist);
        return IsWall(x - dist, y);
    }

    // The cell to the LEFT of the cell 'dist' steps ahead -- "left" meaning
    // the forward vector rotated 90 degrees so a walker facing that way has
    // it on their left hand, e.g. facing north (-Y), left is west (-X).
    public static bool LeftIsWall(uint x, uint y, uint dir, uint dist)
    {
        if (dir == 0) return IsWall(x - 1, y - dist);
        if (dir == 1) return IsWall(x + dist, y - 1);
        if (dir == 2) return IsWall(x + 1, y + dist);
        return IsWall(x - dist, y + 1);
    }

    public static bool RightIsWall(uint x, uint y, uint dir, uint dist)
    {
        if (dir == 0) return IsWall(x + 1, y - dist);
        if (dir == 1) return IsWall(x + dist, y + 1);
        if (dir == 2) return IsWall(x - 1, y + dist);
        return IsWall(x - dist, y - 1);
    }
}
