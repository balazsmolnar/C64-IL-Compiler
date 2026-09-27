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
            if (Maze.LeftIsWall(px, py, dir, s))
                DrawSideSegment(s, FX0);
            if (Maze.RightIsWall(px, py, dir, s))
                DrawSideSegment(s, FX1);
        }

        // The cross-section outline (ceiling, floor, both vertical edges) at
        // every visible depth, drawn unconditionally -- not just where a
        // wall or an opening happens to be. Without this, a wall that spans
        // several segments has no marked corner where one segment's diagonal
        // hands off to the next (it just changes slope, with nothing drawn
        // AT that depth), and an open side is an unmarked gap. With it,
        // every depth step reads as a real boundary either way: where a
        // wall exists, the diagonal plus this outline reads as a proper
        // corner; where it's open, the outline alone reads as a door frame
        // with a passage beyond it. This also IS the stop-wall rectangle at
        // the deepest depth (f == stopDepth draws all 4 of its sides), so
        // that no longer needs its own separate DrawRectangle call.
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
    // shape either side, just the opposite edge of each frame.
    static void DrawSideSegment(uint s, ulong[] xEdge)
    {
        C64.Screen.DrawLine(xEdge[s], FY0[s], xEdge[s + 1], FY0[s + 1]);
        C64.Screen.DrawLine(xEdge[s], FY1[s], xEdge[s + 1], FY1[s + 1]);
    }
}
