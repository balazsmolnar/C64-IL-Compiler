using C64Lib;

namespace Catacombs;

// First-person maze renderer: nested rectangles shrinking toward the
// screen's vanishing point, one per visible depth, connected by diagonal
// side-wall lines -- the classic Wizardry/Bard's Tale-style corridor view.
// Depth 0 is the cell the viewer stands in; MaxDepth is as far as the view
// ever looks, whether or not the corridor is actually that long.
//
// Frame sizes shrink by roughly a consistent ratio each step (width ~65%,
// height ~72%) so the perspective reads as a single, even vanishing point
// rather than a lopsided taper.
//
// Side walls are always drawn, whether or not the maze actually has a wall
// there: the tunnel should always look like a continuous, fully enclosed
// corridor. Where a side is actually open (a passage you could turn into),
// a door -- a smaller rectangle inset into that wall segment -- marks it,
// rather than leaving a gap in the wall geometry itself.
static class DungeonView
{
    const uint MaxDepth = 4;

    static readonly ulong[] FX0 = { 4, 50, 88, 116, 136 };
    static readonly ulong[] FY0 = { 4, 26, 46, 60, 72 };
    static readonly ulong[] FX1 = { 315, 269, 231, 203, 183 };
    static readonly ulong[] FY1 = { 195, 173, 153, 139, 127 };

    public static void Render(uint px, uint py, uint dir)
    {
        uint stopDepth = MaxDepth;
        bool stopped = false;
        for (uint d = 1; d <= MaxDepth; d++)
        {
            if (Maze.AheadIsWall(px, py, dir, d))
            {
                stopDepth = d;
                stopped = true;
                break;
            }
        }

        uint openSegments = stopped ? stopDepth : MaxDepth;
        for (uint s = 0; s < openSegments; s++)
        {
            DrawWallWithDoor(s, FX0, !Maze.LeftIsWall(px, py, dir, s));
            DrawWallWithDoor(s, FX1, !Maze.RightIsWall(px, py, dir, s));
        }

        // The cross-section outline (ceiling, floor, both vertical edges) at
        // every visible depth, drawn unconditionally. Without this, a wall
        // that spans several segments has no marked corner where one
        // segment's diagonal hands off to the next -- it just changes
        // slope, with nothing drawn AT that depth. This also IS the stop
        // wall's rectangle at the deepest depth (f == stopDepth draws all 4
        // of its sides), so that needs no separate DrawRectangle call.
        uint visibleDepth = stopped ? stopDepth : MaxDepth;
        for (uint f = 0; f <= visibleDepth; f++)
        {
            C64.Screen.DrawLine(FX0[f], FY0[f], FX1[f], FY0[f]);
            C64.Screen.DrawLine(FX0[f], FY1[f], FX1[f], FY1[f]);
            C64.Screen.DrawLine(FX0[f], FY0[f], FX0[f], FY1[f]);
            C64.Screen.DrawLine(FX1[f], FY0[f], FX1[f], FY1[f]);
        }
    }

    // The two lines (top-corner-to-top-corner, bottom-corner-to-bottom-
    // corner) that suggest a receding side wall between frame s and frame
    // s+1. xEdge is FX0 for the left wall, FX1 for the right wall -- same
    // shape either side, just the opposite edge of each frame. Always
    // drawn; isOpen additionally overlays a door, without ever removing
    // the wall geometry itself.
    static void DrawWallWithDoor(uint s, ulong[] xEdge, bool isOpen)
    {
        C64.Screen.DrawLine(xEdge[s], FY0[s], xEdge[s + 1], FY0[s + 1]);
        C64.Screen.DrawLine(xEdge[s], FY1[s], xEdge[s + 1], FY1[s + 1]);

        if (!isOpen)
            return;

        // A doorway cut into the wall, sized and centered on the segment's
        // FAR corner (frame s+1's own edge) rather than this segment's
        // near/far midpoint. Using the far corner specifically -- an
        // already-known-safe frame coordinate -- keeps the door inside the
        // screen at every depth, including the outermost segment, where
        // the NEAR corner sits right at the screen's own edge (a midpoint
        // there previously pushed the door partly or wholly off-screen).
        // It spans from the floor up to 3/4 of that frame's own height
        // (leaving a "lintel" strip at the top), sized as a fraction of the
        // same height, so it reads at a sensible size at every depth.
        ulong height = FY1[s + 1] - FY0[s + 1];
        ulong doorTop = FY0[s + 1] + height / 4;
        ulong halfWidth = height / 6;
        if (halfWidth < 2)
            halfWidth = 2;
        ulong x = xEdge[s + 1];
        ulong doorLeft = x > halfWidth ? x - halfWidth : 0;
        C64.Screen.DrawRectangle(doorLeft, doorTop, x + halfWidth, FY1[s + 1]);
    }
}
