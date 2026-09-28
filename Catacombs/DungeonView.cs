using C64Lib;

namespace Catacombs;

// First-person maze renderer: nested rectangles shrinking toward the
// screen's vanishing point, one per visible depth, connected by diagonal
// side-wall lines -- the classic Wizardry/Bard's Tale-style corridor view.
//
// Frame index i is the boundary after (i+1) cells traveled -- frame[0], the
// near-full-screen frame, is the CLOSEST a wall can ever be: one step
// ahead, filling almost the whole view, the way standing right in front of
// a wall actually looks. There is deliberately no frame for "distance 0":
// that's the camera/the player's own position, not a rendered surface, so
// nothing is ever drawn for it -- the nearest thing on screen is always
// exactly one step away, which is what makes the view read as "you are
// standing here" rather than "you are one phantom half-step behind here".
// MaxDepth is as far as the view ever looks (in cells), whether or not the
// corridor is actually that long; frame[MaxDepth] (one past the last
// checked distance) is used only as the vanishing edge when the corridor
// stays open through every checked distance.
//
// Frame sizes shrink by roughly a consistent ratio each step (width ~65%,
// height ~72%) so the perspective reads as a single, even vanishing point
// rather than a lopsided taper.
//
// Side walls are always drawn, whether or not the maze actually has a wall
// there: the tunnel should always look like a continuous, fully enclosed
// corridor. Where a side is actually open (a passage you could turn into),
// a door -- a smaller rectangle inset into that wall segment, its corners
// hand-picked (not computed) per depth -- marks it, rather than leaving a
// gap in the wall geometry itself.
//
// The wall you're facing, whenever you're facing one, always gets a
// "front door" too -- a doorway with a smaller rectangle nested inside it,
// hinting at more space (and maybe another door) glimpsed through it,
// rather than a flat dead end.
static class DungeonView
{
    const uint MaxDepth = 4;

    static readonly ulong[] FX0 = { 4, 50, 88, 116, 136 };
    static readonly ulong[] FY0 = { 4, 26, 46, 60, 72 };
    static readonly ulong[] FX1 = { 315, 269, 231, 203, 183 };
    static readonly ulong[] FY1 = { 195, 173, 153, 139, 127 };

    // Side-door corners, one entry per wall panel (s = 0..MaxDepth-1, the
    // panel between frame[s] and frame[s+1]). The far edge/bottom of each
    // door is exactly that panel's own far frame corner (FX0[s+1]/
    // FX1[s+1]/FY1[s+1] -- no separate table needed, it already flushes
    // against the frame line the outline loop draws), so only the door's
    // near edge and top need their own hand-picked values here.
    static readonly ulong[] DoorTop = { 63, 73, 80, 86 };
    static readonly ulong[] DoorNearX0 = { 35, 75, 107, 129 };
    static readonly ulong[] DoorNearX1 = { 284, 244, 212, 190 };

    // Front-door corners, one entry per possible stop depth (0..MaxDepth):
    // a doorway centered in that frame, sized as a fraction of it. Bottom
    // is that frame's own floor line (FY1), so no separate table for it.
    static readonly ulong[] FrontDoorLeft = { 119, 131, 140, 148, 153 };
    static readonly ulong[] FrontDoorRight = { 199, 187, 178, 170, 165 };
    static readonly ulong[] FrontDoorTop = { 90, 92, 94, 95, 97 };

    public static void Render(uint px, uint py, uint dir)
    {
        // Maze.AheadIsWall(dist) checks the cell `dist` steps ahead (1-based:
        // dist=1 is the very next cell). That maps to frame index dist-1, so
        // the nearest possible wall (dist=1) lands on frame[0].
        uint lastIndex = MaxDepth;
        for (uint d = 1; d <= MaxDepth; d++)
        {
            if (Maze.AheadIsWall(px, py, dir, d))
            {
                lastIndex = d - 1;
                break;
            }
        }

        // Segment s (frame[s] to frame[s+1]) represents the cell reached
        // after s+1 steps -- one more than the frame's own array index,
        // since frame[0] itself already corresponds to dist=1.
        for (uint s = 0; s < lastIndex; s++)
        {
            DrawWallWithDoor(s, FX0, DoorNearX0, !Maze.LeftIsWall(px, py, dir, s + 1));
            DrawWallWithDoor(s, FX1, DoorNearX1, !Maze.RightIsWall(px, py, dir, s + 1));
        }

        // The cross-section outline (ceiling, floor, both vertical edges) at
        // every visible depth, drawn unconditionally. Without this, a wall
        // that spans several segments has no marked corner where one
        // segment's diagonal hands off to the next -- it just changes
        // slope, with nothing drawn AT that depth. This also IS the stop
        // wall's rectangle at the deepest depth (f == lastIndex draws all 4
        // of its sides), so that needs no separate DrawRectangle call.
        for (uint f = 0; f <= lastIndex; f++)
        {
            C64.Screen.DrawLine(FX0[f], FY0[f], FX1[f], FY0[f], true, BitmapColorSource.MatrixHigh);
            C64.Screen.DrawLine(FX0[f], FY1[f], FX1[f], FY1[f], true, BitmapColorSource.MatrixHigh);
            C64.Screen.DrawLine(FX0[f], FY0[f], FX0[f], FY1[f], true, BitmapColorSource.MatrixHigh);
            C64.Screen.DrawLine(FX1[f], FY0[f], FX1[f], FY1[f], true, BitmapColorSource.MatrixHigh);
        }

        DrawFrontDoor(lastIndex);
    }

    // The two lines (top-corner-to-top-corner, bottom-corner-to-bottom-
    // corner) that suggest a receding side wall between frame s and frame
    // s+1. xEdge is FX0 for the left wall, FX1 for the right wall -- same
    // shape either side, just the opposite edge of each frame. Always
    // drawn; isOpen additionally overlays a door, without ever removing
    // the wall geometry itself.
    static void DrawWallWithDoor(uint s, ulong[] xEdge, ulong[] doorNearX, bool isOpen)
    {
        C64.Screen.DrawLine(xEdge[s], FY0[s], xEdge[s + 1], FY0[s + 1], true, BitmapColorSource.MatrixHigh);
        C64.Screen.DrawLine(xEdge[s], FY1[s], xEdge[s + 1], FY1[s + 1], true, BitmapColorSource.MatrixHigh);

        if (!isOpen)
            return;

        // Far edge/bottom flush against xEdge[s+1]/FY1[s+1] -- the frame's
        // own corner, already drawn by the outline loop -- so the door
        // visibly joins onto real frame geometry instead of floating free
        // in open space. ColorRam (not MatrixHigh, like the walls) so a
        // door reads as visibly distinct from the wall it's set into.
        C64.Screen.DrawRectangle(doorNearX[s], DoorTop[s], xEdge[s + 1], FY1[s + 1], false, true, BitmapColorSource.ColorRam);
    }

    // The doorway on the wall directly ahead, whenever there's a wall
    // directly ahead to put one on (index is either the stop depth, or
    // MaxDepth's own vanishing edge when the corridor stays open the whole
    // way -- both already get a full 4-sided frame from the outline loop
    // above, so a door reads naturally on either). A smaller rectangle
    // nested inside it hints at more space glimpsed through the doorway,
    // rather than it being a flat dead end.
    static void DrawFrontDoor(uint index)
    {
        ulong left = FrontDoorLeft[index];
        ulong right = FrontDoorRight[index];
        ulong top = FrontDoorTop[index];
        ulong bottom = FY1[index];
        C64.Screen.DrawRectangle(left, top, right, bottom, false, true, BitmapColorSource.ColorRam);

        ulong innerLeft = Lerp(left, right, 1, 4);
        ulong innerRight = Lerp(right, left, 1, 4);
        ulong innerTop = Lerp(top, bottom, 1, 4);
        C64.Screen.DrawRectangle(innerLeft, innerTop, innerRight, bottom, false, true, BitmapColorSource.ColorRam);
    }

    // Point a fraction (num/den) of the way from a to b. Unsigned-safe:
    // works out which direction to step before subtracting, since a and b
    // may fall either side of each other (FX0 rises with depth, FX1 falls).
    static ulong Lerp(ulong a, ulong b, ulong num, ulong den)
    {
        if (b >= a)
            return a + (b - a) * num / den;
        return a - (a - b) * num / den;
    }
}
