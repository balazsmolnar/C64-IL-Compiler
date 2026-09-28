using C64Lib;

namespace Catacombs;

// First-person view of the single cell (room) the player is standing in.
// Two frames: the near one (screen edges -- right where you're standing)
// and the far one (one step ahead -- this room's own far boundary). A
// room has 3 visible walls -- left, right, front -- each either solid or,
// where the maze actually has an opening, marked with a door. No back
// wall (nothing renders behind the camera) and no depth beyond the far
// frame either, deliberately: this used to recede several cells into the
// distance, tapering smaller with each one, but that shape needed far
// more fill area (and far more per-row DrawRectangle calls) than a redraw
// after every move could afford -- see git history for that version if
// it's ever wanted back. This is deliberately less shape.
static class DungeonView
{
    const ulong FX0Near = 4, FY0Near = 4, FX1Near = 315, FY1Near = 195;
    const ulong FX0Far = 50, FY0Far = 26, FX1Far = 269, FY1Far = 173;

    // Door insets against the near/far frame corners -- the far edge is
    // flush with the far frame's own line (already drawn by the outline
    // below), only the near edge/top are hand-picked, same idea the
    // multi-depth version used.
    const ulong DoorTop = 63;
    const ulong DoorNearLeft = 35;
    const ulong DoorNearRight = 284;
    const ulong FrontDoorLeft = 131;
    const ulong FrontDoorRight = 187;
    const ulong FrontDoorTop = 92;

    public static void Render(uint px, uint py, uint dir)
    {
        bool leftOpen = !Maze.LeftIsWall(px, py, dir, 0);
        bool rightOpen = !Maze.RightIsWall(px, py, dir, 0);
        bool frontOpen = !Maze.AheadIsWall(px, py, dir, 1);

        // Fills (walls, floor, ceiling, front wall), drawn first so the
        // white outline/diagonal lines and brown doors below land on top
        // of them, not the other way around.
        FillWallPanel(FX0Near, FX0Far);
        FillWallPanel(FX1Near, FX1Far);
        FillFloorCeiling();
        C64.Screen.DrawRectangle(FX0Far, FY0Far, FX1Far, FY1Far, true, true, BitmapColorSource.MatrixLow);

        // Side walls: the two diagonal edges, always drawn, plus a door
        // overlay where that side is actually open.
        DrawWallWithDoor(FX0Near, FX0Far, DoorNearLeft, leftOpen);
        DrawWallWithDoor(FX1Near, FX1Far, DoorNearRight, rightOpen);

        // Both frames' own 4-sided outline.
        DrawFrameOutline(FX0Near, FY0Near, FX1Near, FY1Near);
        DrawFrameOutline(FX0Far, FY0Far, FX1Far, FY1Far);

        if (frontOpen)
            C64.Screen.DrawRectangle(FrontDoorLeft, FrontDoorTop, FrontDoorRight, FY1Far, false, true, BitmapColorSource.ColorRam);
    }

    static void DrawFrameOutline(ulong x0, ulong y0, ulong x1, ulong y1)
    {
        C64.Screen.DrawLine(x0, y0, x1, y0, true, BitmapColorSource.MatrixHigh);
        C64.Screen.DrawLine(x0, y1, x1, y1, true, BitmapColorSource.MatrixHigh);
        C64.Screen.DrawLine(x0, y0, x0, y1, true, BitmapColorSource.MatrixHigh);
        C64.Screen.DrawLine(x1, y0, x1, y1, true, BitmapColorSource.MatrixHigh);
    }

    // The two diagonal lines (near top corner to far top corner, near
    // bottom corner to far bottom corner) that suggest a receding side
    // wall, always drawn, plus a door overlay where open. nearX/farX are
    // FX0Near/FX0Far for the left wall, FX1Near/FX1Far for the right --
    // same shape either side, just the opposite edge of each frame.
    static void DrawWallWithDoor(ulong nearX, ulong farX, ulong doorNearX, bool isOpen)
    {
        C64.Screen.DrawLine(nearX, FY0Near, farX, FY0Far, true, BitmapColorSource.MatrixHigh);
        C64.Screen.DrawLine(nearX, FY1Near, farX, FY1Far, true, BitmapColorSource.MatrixHigh);

        if (!isOpen)
            return;

        // Far edge/bottom flush against farX/FY1Far -- the far frame's
        // own corner, already drawn by its own outline -- so the door
        // visibly joins onto real frame geometry instead of floating free.
        C64.Screen.DrawRectangle(doorNearX, DoorTop, farX, FY1Far, false, true, BitmapColorSource.ColorRam);
    }

    // Fills the wall panel between the near and far frame (nearX/farX =
    // FX0Near/FX0Far for the left wall, FX1Near/FX1Far for the right).
    // The near frame's height range strictly contains the far frame's, so
    // this bands into a top taper, a constant middle band -- one ordinary
    // multi-row fill, not a per-row loop, since the opposite x doesn't
    // change row to row there -- and a bottom taper.
    static void FillWallPanel(ulong nearX, ulong farX)
    {
        for (ulong y = FY0Near; y < FY0Far; y = y + 1)
        {
            ulong otherX = Lerp(nearX, farX, y - FY0Near, FY0Far - FY0Near);
            C64.Screen.DrawRectangle(nearX, y, otherX, y, true, true, BitmapColorSource.MatrixLow);
        }

        C64.Screen.DrawRectangle(nearX, FY0Far, farX, FY1Far, true, true, BitmapColorSource.MatrixLow);

        for (ulong y = FY1Far + 1; y <= FY1Near; y = y + 1)
        {
            ulong otherX = Lerp(farX, nearX, y - FY1Far, FY1Near - FY1Far);
            C64.Screen.DrawRectangle(nearX, y, otherX, y, true, true, BitmapColorSource.MatrixLow);
        }
    }

    // Fills the floor and ceiling trapezoids between the near and far
    // frame: unlike the wall panels, both left and right edges here are
    // diagonals running the whole row range, so each row is a single
    // Lerp, no banding needed.
    static void FillFloorCeiling()
    {
        for (ulong y = FY1Far; y <= FY1Near; y = y + 1)
        {
            ulong left = Lerp(FX0Far, FX0Near, y - FY1Far, FY1Near - FY1Far);
            ulong right = Lerp(FX1Far, FX1Near, y - FY1Far, FY1Near - FY1Far);
            C64.Screen.DrawRectangle(left, y, right, y, true, true, BitmapColorSource.MatrixLow);
        }
        for (ulong y = FY0Near; y <= FY0Far; y = y + 1)
        {
            ulong left = Lerp(FX0Near, FX0Far, y - FY0Near, FY0Far - FY0Near);
            ulong right = Lerp(FX1Near, FX1Far, y - FY0Near, FY0Far - FY0Near);
            C64.Screen.DrawRectangle(left, y, right, y, true, true, BitmapColorSource.MatrixLow);
        }
    }

    // Point a fraction (num/den) of the way from a to b. Unsigned-safe:
    // works out which direction to step before subtracting, since a and b
    // may fall either side of each other (left-wall x rises near to far,
    // right-wall x falls).
    static ulong Lerp(ulong a, ulong b, ulong num, ulong den)
    {
        if (b >= a)
            return a + (b - a) * num / den;
        return a - (a - b) * num / den;
    }
}
