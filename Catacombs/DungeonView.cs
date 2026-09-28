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

        // Solid fills for every segment's 4 surfaces (2 wall panels, floor,
        // ceiling), drawn FIRST so the white outline/diagonal lines and
        // brown doors (below) render crisply on top of them, not the other
        // way around. Frame[s] strictly contains frame[s+1] (both FX/FY
        // pairs move monotonically with depth), so the space between them
        // is a rectangular "picture frame" ring; these 4 surfaces are the
        // natural trapezoidal pieces you get splitting that ring along its
        // own corner-to-corner diagonals -- the SAME diagonals
        // DrawWallWithDoor already draws below, so no new outline is
        // needed, only the fill.
        for (uint s = 0; s < lastIndex; s++)
        {
            FillWallPanel(s, FX0);
            FillWallPanel(s, FX1);
            FillFloorCeiling(s);
        }

        // The wall you're facing (or the vanishing edge, if the corridor
        // stays open) has no "next frame" to fill a panel against -- fill
        // its own interior directly as one flat surface, same color as
        // every other wall/floor/ceiling. The front door drawn below
        // still lands on top of it, same as any other door on a filled
        // panel.
        C64.Screen.DrawRectangle(FX0[lastIndex], FY0[lastIndex], FX1[lastIndex], FY1[lastIndex], true, true, BitmapColorSource.MatrixLow);

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

    // Fills the wall panel between frame s and frame s+1 (xEdge = FX0 for
    // the left wall, FX1 for the right -- same shape either side, just the
    // opposite edge of each frame). xEdge[s] is constant for the whole row
    // range (frame s's own vertical edge); the opposite x is banded by
    // row, because frame s's height range strictly contains frame s+1's:
    // a top taper and a bottom taper (each along the diagonal to frame
    // s+1's corresponding near corner), and in between, a MIDDLE band
    // where the opposite x is ALSO constant (frame s+1's own edge, for as
    // long as y stays within frame s+1's own height range) -- usually most
    // of the panel's height. That middle band is a single ordinary
    // (multi-row) filled DrawRectangle, not a per-row loop -- each row of
    // a real multi-row fill is one asm-side Graphics_HLine_Core call
    // inside ONE C# call, instead of one C# call (with its own argument-
    // marshalling cost) per row; only the two tapers, where the width
    // genuinely changes row to row, need the per-row treatment. MatrixLow
    // -- distinct from the walls' own MatrixHigh outline and the doors'
    // ColorRam -- so a solid wall doesn't just look like a thicker white
    // line.
    static void FillWallPanel(uint s, ulong[] xEdge)
    {
        for (ulong y = FY0[s]; y < FY0[s + 1]; y = y + 1)
        {
            ulong otherX = Lerp(xEdge[s], xEdge[s + 1], y - FY0[s], FY0[s + 1] - FY0[s]);
            C64.Screen.DrawRectangle(xEdge[s], y, otherX, y, true, true, BitmapColorSource.MatrixLow);
        }

        C64.Screen.DrawRectangle(xEdge[s], FY0[s + 1], xEdge[s + 1], FY1[s + 1], true, true, BitmapColorSource.MatrixLow);

        for (ulong y = FY1[s + 1] + 1; y <= FY1[s]; y = y + 1)
        {
            ulong otherX = Lerp(xEdge[s + 1], xEdge[s], y - FY1[s + 1], FY1[s] - FY1[s + 1]);
            C64.Screen.DrawRectangle(xEdge[s], y, otherX, y, true, true, BitmapColorSource.MatrixLow);
        }
    }

    // Fills the floor and ceiling trapezoids between frame s and frame
    // s+1: unlike the wall panels, both left and right edges here are
    // diagonals running the whole row range (FX0[s] to FX0[s+1] on the
    // left, FX1[s] to FX1[s+1] on the right), so each row is a single
    // Lerp, no banding needed.
    static void FillFloorCeiling(uint s)
    {
        for (ulong y = FY1[s + 1]; y <= FY1[s]; y = y + 1)
        {
            ulong left = Lerp(FX0[s + 1], FX0[s], y - FY1[s + 1], FY1[s] - FY1[s + 1]);
            ulong right = Lerp(FX1[s + 1], FX1[s], y - FY1[s + 1], FY1[s] - FY1[s + 1]);
            C64.Screen.DrawRectangle(left, y, right, y, true, true, BitmapColorSource.MatrixLow);
        }
        for (ulong y = FY0[s]; y <= FY0[s + 1]; y = y + 1)
        {
            ulong left = Lerp(FX0[s], FX0[s + 1], y - FY0[s], FY0[s + 1] - FY0[s]);
            ulong right = Lerp(FX1[s], FX1[s + 1], y - FY0[s], FY0[s + 1] - FY0[s]);
            C64.Screen.DrawRectangle(left, y, right, y, true, true, BitmapColorSource.MatrixLow);
        }
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
