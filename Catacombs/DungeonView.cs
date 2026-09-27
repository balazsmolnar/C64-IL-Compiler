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
            else
                DrawOpeningEdge(s, FX0);
            if (Maze.RightIsWall(px, py, dir, s))
                DrawSideSegment(s, FX1);
            else
                DrawOpeningEdge(s, FX1);
        }

        // Floor and ceiling: a horizontal line at every visible depth,
        // drawn regardless of whether a side wall exists there. Without
        // these the corridor has no top/bottom boundary at all wherever
        // both sides happen to be open, and looks like it's floating in
        // empty space instead of being an enclosed tunnel; with them, each
        // depth reads as a visible "step" even along an open stretch,
        // which is what actually sells the recession into the screen.
        uint visibleDepth = stopped ? stopDepth : MaxDepth;
        for (uint f = 0; f <= visibleDepth; f++)
        {
            C64.Screen.DrawLine(FX0[f], FY0[f], FX1[f], FY0[f]);
            C64.Screen.DrawLine(FX0[f], FY1[f], FX1[f], FY1[f]);
        }

        if (stopped)
            C64.Screen.DrawRectangle(FX0[stopDepth], FY0[stopDepth], FX1[stopDepth], FY1[stopDepth]);
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

    // Marks a side opening (a passage branching off, or just the corridor
    // continuing straight with nothing walling this side) with a short
    // vertical line at frame s's own edge, connecting the ceiling line to
    // the floor line at that depth -- like a door jamb. Without this, "no
    // wall" and "nothing drawn there" look identical, so an opening just
    // reads as a gap in the picture rather than a clearly bounded passage.
    static void DrawOpeningEdge(uint s, ulong[] xEdge)
    {
        C64.Screen.DrawLine(xEdge[s], FY0[s], xEdge[s], FY1[s]);
    }
}
