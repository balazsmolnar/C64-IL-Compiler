using C64Lib;

namespace Catacombs;

// A small thumbnail showing where the player is within the whole maze
// (see Maze.cs's own Width/Height) -- a filled square spanning the
// maze's bounds, with the player's current cell marked on top as a small
// square in a different color. Deliberately NOT a per-cell floor/wall
// rendering (an
// earlier version drew every floor cell individually -- at this
// thumbnail's small scale it read as visual noise rather than a legible
// map, and wall cells had nothing to draw, so they just showed whatever
// the live room's own background color happened to be underneath, which
// was confusing). Drawn once per room entry in the screen's top-right
// corner, the mirror of Hud.cs's own top-left placement.
static class Minimap
{
    const ulong OriginX = 270, OriginY = 4;
    const ulong CellSize = 2; // 12*2 = 24px square footprint
    const ulong MarkerSize = 2;

    public static void Render(uint px, uint py)
    {
        ulong width = Maze.Width * CellSize;
        ulong height = Maze.Height * CellSize;
        C64.Screen.DrawRectangle(OriginX, OriginY, OriginX + width, OriginY + height, true, true, BitmapColorSource.MatrixHigh);

        ulong px2 = OriginX + px * CellSize;
        ulong py2 = OriginY + py * CellSize;
        C64.Screen.DrawRectangle(px2, py2, px2 + MarkerSize, py2 + MarkerSize, true, true, BitmapColorSource.MatrixLow);
    }
}
