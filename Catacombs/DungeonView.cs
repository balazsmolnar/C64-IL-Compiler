using C64Lib;

namespace Catacombs;

// Fixed compass orientation: every room is always drawn the same way --
// north is always the far wall, south the near (screen-edge) one, west the
// left wall, east the right wall -- regardless of which wall the player
// actually walked in through to get here. Walking through a door no longer
// rotates the view (there's no "facing" left to rotate -- see Maze.cs's own
// comment): crossing a room's east door and entering the next room's west
// wall always means arriving at ITS left edge, crossing its west door
// always means arriving at its right edge, and so on (Program.cs's own
// TryCrossDoor picks the matching entry edge for whichever wall was
// crossed).
//
// No outline is drawn at the near (screen-edge, south) frame -- that's
// where the player's own view cuts off, not a real surface -- only the far
// (north) frame gets its own 4-sided outline. A room has 4 possible doors
// -- north, south, east, west -- each either solid or, where the maze
// actually has an opening, a filled door (south's is a small marker at the
// bottom edge rather than a full panel, since there's no wall surface
// there to draw one on).
//
// Rendered ONCE per room entry, not per frame: Program.cs moves the player
// via a hardware sprite (PlayerSprite.cs) that repositions with plain VIC-II
// register pokes, no bitmap redraw needed for movement at all -- redrawing
// this whole vector scene (a dozen+ DrawLine/DrawTrapezoid calls) every
// single frame while double-buffered would mean a full clear+redraw every
// frame just to avoid stale walls, which measured far too slow for smooth
// movement (see conversation history -- this replaces an earlier per-frame
// continuous-depth version that made exactly that mistake).
//
// Walls, floor, ceiling and the screen background are all the current
// room's own color (Maze.RoomColor, applied in Program.cs), so nothing is
// filled: the room is just its lines and its doors on that one color.
static class DungeonView
{
    const ulong FX0Near = 4, FY0Near = 4, FX1Near = 315, FY1Near = 195;
    const ulong FX0Far = 50, FY0Far = 26, FX1Far = 269, FY1Far = 173;

    // Screen x of the mirror image of a left-wall point on the right wall:
    // both frames are symmetric about the screen's center (4+315 = 50+269 =
    // 319), so the right wall is the left wall with x -> 319 - x.
    const ulong MirrorSum = 319;

    // Side door, given for the LEFT wall (mirrored for the right). Two
    // vertical sides at x=29 (nearer the viewer) and x=43 (nearer the far
    // frame, leaving a gap before the far frame's own edge at x=50). Both
    // edges of the door follow the wall's own perspective: the bottom edge
    // is on the floor line (y=183 at x=29, rising to 176 at x=43), the top
    // edge is 70% of the local wall height above it (y=66 at x=29, 69 at
    // x=43) -- so the door is taller at its near side than its far side,
    // like the wall.
    const ulong DoorXNear = 29, DoorXFar = 43;
    const ulong DoorTopFirstRow = 66, DoorFullFirstRow = 69, DoorFullLastRow = 176, DoorBottomLastRow = 183;

    const ulong NorthDoorLeft = 131;
    const ulong NorthDoorRight = 187;
    const ulong NorthDoorTop = 92;

    // South has no wall surface to draw a panel on (see this file's own
    // header comment) -- just a short bar at the very bottom-center of the
    // screen signalling "there's an exit behind you." Centered on the same
    // axis the near/far frames themselves are symmetric about (4+315 =
    // MirrorSum = 319, center 159.5).
    const ulong SouthDoorLeft = 140, SouthDoorRight = 180;
    const ulong SouthDoorTop = 194, SouthDoorBottom = 199;

    // Where the player's/monster's/item's room-space position actually
    // ends up on screen is RoomProjection.cs's job, not this file's --
    // it mirrors this class's own FX0Near/FX1Near/FX0Far/FX1Far by hand
    // (see its own comment for why a shared constant isn't worth it).

    public static void Render(uint px, uint py)
    {
        RenderRoom(px, py, NorthDoorRight, NorthDoorTop, FY1Far);
    }

    // The north door opening: hinged at its LEFT edge (NorthDoorLeft,
    // fixed), its right edge given per-frame by the caller (Program.cs's
    // own AnimateNorthDoorOpening) -- an x (receding from the closed
    // door's right edge, NorthDoorRight, toward the hinge) AND a top/bottom
    // row pair, shrinking symmetrically in from the door's own top/bottom
    // toward its vertical center as the x recedes. The two right corners
    // trace inward AND toward the middle together -- a real 3D swing, not
    // a flat edge sliding sideways -- collapsing onto the hinge when fully
    // open. Passing the closed-door values (NorthDoorRight, NorthDoorTop,
    // FY1Far) reproduces Render's own plain rectangle exactly (see
    // DrawTaperedPanel's own comment on how those degenerate the tapers to
    // zero height) -- this is a strict generalization of it, not a
    // separate code path.
    public static void RenderNorthDoorOpening(uint px, uint py, ulong panelFarX, ulong panelFarTopRow, ulong panelFarBottomRow)
    {
        RenderRoom(px, py, panelFarX, panelFarTopRow, panelFarBottomRow);
    }

    static void RenderRoom(uint px, uint py, ulong northFarX, ulong northFarTopRow, ulong northFarBottomRow)
    {
        bool westOpen = !Maze.WestIsWall(px, py);
        bool eastOpen = !Maze.EastIsWall(px, py);
        bool northOpen = !Maze.NorthIsWall(px, py);
        bool southOpen = !Maze.SouthIsWall(px, py);

        // The two diagonal lines per side wall, always drawn.
        DrawWallLines(FX0Near, FX0Far);
        DrawWallLines(FX1Near, FX1Far);

        // Only the far frame's own outline -- not the near one. The near
        // frame sits right at the screen edge, which is where the
        // player's own view cuts off, not a real surface in front of
        // them; standing inside the room, there's nothing there to draw a
        // line on.
        DrawFrameOutline(FX0Far, FY0Far, FX1Far, FY1Far);

        if (westOpen)
            DrawSideDoor(false);
        if (eastOpen)
            DrawSideDoor(true);
        if (northOpen)
            DrawTaperedPanel(NorthDoorLeft, NorthDoorTop, northFarX, northFarTopRow, northFarBottomRow, FY1Far, BitmapColorSource.ColorRam);
        if (southOpen)
            C64.Screen.DrawRectangle(SouthDoorLeft, SouthDoorTop, SouthDoorRight, SouthDoorBottom, true, true, BitmapColorSource.ColorRam);
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
    // wall. nearX/farX are FX0Near/FX0Far for the left wall, FX1Near/
    // FX1Far for the right -- same shape either side.
    static void DrawWallLines(ulong nearX, ulong farX)
    {
        C64.Screen.DrawLine(nearX, FY0Near, farX, FY0Far, true, BitmapColorSource.MatrixHigh);
        C64.Screen.DrawLine(nearX, FY1Near, farX, FY1Far, true, BitmapColorSource.MatrixHigh);
    }

    static ulong X(ulong leftWallX, bool mirrored)
    {
        if (mirrored)
            return MirrorSum - leftWallX;
        return leftWallX;
    }

    // See the DoorX*/Door*Row constants for the geometry -- mirrored
    // (right-wall) calls just feed mirrored x's into DrawTaperedPanel,
    // which sorts left/right itself.
    static void DrawSideDoor(bool mirrored)
    {
        DrawTaperedPanel(X(DoorXNear, mirrored), DoorTopFirstRow, X(DoorXFar, mirrored), DoorFullFirstRow, DoorFullLastRow, DoorBottomLastRow, BitmapColorSource.ColorRam);
    }

    // The shared shape behind the side doors: a "near" edge at constant x,
    // spanning the full [topRow,bottomRow] height, and a "far" edge at
    // constant x, spanning a SHORTER, vertically-centered
    // [farTopRow,farBottomRow] sub-range -- connected by a triangular
    // taper above and below the far edge's own span. 3 calls: the top
    // taper (a point at (nearX,topRow) widening to the far edge's span by
    // farTopRow), the flat middle band (a plain rectangle from farTopRow
    // to farBottomRow), and the bottom taper (narrowing back to a point at
    // (nearX,bottomRow)).
    static void DrawTaperedPanel(ulong nearX, ulong topRow, ulong farX, ulong farTopRow, ulong farBottomRow, ulong bottomRow, BitmapColorSource color)
    {
        ulong left = nearX < farX ? nearX : farX;
        ulong right = nearX < farX ? farX : nearX;
        C64.Screen.DrawTrapezoid(nearX, nearX, topRow, left, right, farTopRow, true, color);
        C64.Screen.DrawRectangle(left, farTopRow, right, farBottomRow, true, true, color);
        C64.Screen.DrawTrapezoid(left, right, farBottomRow, nearX, nearX, bottomRow, true, color);
    }
}
