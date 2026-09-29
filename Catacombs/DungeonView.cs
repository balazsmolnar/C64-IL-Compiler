using C64Lib;

namespace Catacombs;

// First-person view of the single cell (room) the player is standing in,
// as if actually standing inside it: no outline is drawn at the near
// (screen-edge) frame -- that's where the player's own view cuts off, not
// a real surface -- only the far frame (one step ahead) gets its own
// 4-sided outline. A room has 3 visible walls -- left, right, front --
// each either solid or, where the maze actually has an opening, a filled
// door. The side doors are perspective quads (vertical sides, the bottom
// edge running along the floor line, the top edge converging like the
// wall itself); the front door, faced head-on, is a plain rectangle.
//
// Walls, floor, ceiling and the screen background are all the current
// room's own color (Maze.RoomColor, applied in Program.cs), so nothing is
// filled: the room is just its lines and its doors on that one color. (An
// earlier version filled each surface in its own color, which needed
// hundreds of per-row DrawRectangle calls per redraw -- see git history if
// distinct surface shades are ever wanted back.) No back wall and no depth
// beyond the far frame either: this used to recede several cells into the
// distance; this is deliberately less shape.
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
    // edges of the door follow the wall's own perspective, worked out as
    // fractions of the wall panel: the bottom edge is on the floor line
    // (y=183 at x=29, rising to 176 at x=43), the top edge is 70% of the
    // local wall height above it (y=66 at x=29, 69 at x=43) -- so the door
    // is taller at its near side than its far side, like the wall.
    //
    // Filled as 3 DrawTrapezoid calls -- a shape whose left edge is the
    // constant column x=29 for the door's full height, and whose right
    // edge is 3-banded (taper in from a point, a flat run at x=43, taper
    // back out to a point): top taper (a triangle, x0Left=x0Right=29
    // degenerating to a point at the top), the flat middle band (a plain
    // rectangle, still expressed as a trapezoid with matching top/bottom
    // x's), and the bottom taper (a triangle again). Each trapezoid does
    // its own row interpolation in asm (Graphics_Trapezoid_Core), so this
    // no longer hand-computes a per-row endpoint in C#.
    const ulong DoorXNear = 29, DoorXFar = 43;
    const ulong DoorTopFirstRow = 66, DoorFullFirstRow = 69, DoorFullLastRow = 176, DoorBottomLastRow = 183;

    const ulong FrontDoorLeft = 131;
    const ulong FrontDoorRight = 187;
    const ulong FrontDoorTop = 92;

    public static void Render(uint px, uint py, uint dir)
    {
        RenderRoom(px, py, dir, FrontDoorRight, FrontDoorTop, FY1Far);
    }

    // EXPERIMENT: the front door used to open as a flat rectangle sliding
    // its right edge toward the hinge -- a uniform shrink, no sense of
    // swinging in 3D. Now it opens with the same "constant near edge,
    // shorter far edge, tapered top/bottom bands" shape DrawSideDoor
    // already uses (see DrawTaperedPanel) -- the far (right) edge's x
    // recedes toward the hinge AND its top/bottom span shrinks toward the
    // door's own vertical center, symmetrically, so the two right corners
    // trace inward AND toward the middle as the door swings away, instead
    // of a flat edge just sliding sideways. farTopRow==FrontDoorTop and
    // farBottomRow==FY1Far (the closed-door state Render() passes) leaves
    // the top/bottom tapers degenerate (zero height) and the whole panel
    // is just the closed flat rectangle, same as before. The doorway
    // behind it is filled with MatrixLow -- Program.cs sets that to the
    // NEXT room's color for the animation, so you see into the room
    // you're about to enter -- drawn as a plain rectangle first, so the
    // panel (any shape) just needs drawing on top of it; wherever the
    // panel doesn't reach, the doorway fill already shows through.
    // panelFarX == FrontDoorLeft leaves no panel at all (fully open). Has
    // no effect on a room with no front door.
    public static void RenderOpening(uint px, uint py, uint dir, ulong panelFarX, ulong panelFarTopRow, ulong panelFarBottomRow)
    {
        RenderRoom(px, py, dir, panelFarX, panelFarTopRow, panelFarBottomRow);
    }

    static void RenderRoom(uint px, uint py, uint dir, ulong panelFarX, ulong panelFarTopRow, ulong panelFarBottomRow)
    {
        bool leftOpen = !Maze.LeftIsWall(px, py, dir, 0);
        bool rightOpen = !Maze.RightIsWall(px, py, dir, 0);
        bool frontOpen = !Maze.AheadIsWall(px, py, dir, 1);

        // The two diagonal lines per side wall, always drawn.
        DrawWallLines(FX0Near, FX0Far);
        DrawWallLines(FX1Near, FX1Far);

        // Only the far frame's own outline -- not the near one. The near
        // frame sits right at the screen edge, which is where the
        // player's own view cuts off, not a real surface in front of
        // them; standing inside the room, there's nothing there to draw a
        // line on.
        DrawFrameOutline(FX0Far, FY0Far, FX1Far, FY1Far);

        if (leftOpen)
            DrawSideDoor(false);
        if (rightOpen)
            DrawSideDoor(true);
        if (frontOpen)
        {
            if (panelFarX < FrontDoorRight)
                C64.Screen.DrawRectangle(FrontDoorLeft, FrontDoorTop, FrontDoorRight, FY1Far, true, true, BitmapColorSource.MatrixLow);
            DrawTaperedPanel(FrontDoorLeft, FrontDoorTop, panelFarX, panelFarTopRow, panelFarBottomRow, FY1Far, BitmapColorSource.ColorRam);
        }
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

    // The shared shape behind both the side doors and the front door's
    // opening animation: a "near" edge at constant x, spanning the full
    // [topRow,bottomRow] height, and a "far" edge at constant x, spanning
    // a SHORTER, vertically-centered [farTopRow,farBottomRow] sub-range --
    // connected by a triangular taper above and below the far edge's own
    // span. 3 calls: the top taper (a point at (nearX,topRow) widening to
    // the far edge's span by farTopRow), the flat middle band (a plain
    // rectangle from farTopRow to farBottomRow), and the bottom taper
    // (narrowing back to a point at (nearX,bottomRow)). farTopRow==topRow
    // and/or farBottomRow==bottomRow degenerate that taper to zero height
    // (nothing drawn there), so farTopRow=topRow/farBottomRow=bottomRow
    // together is just a plain rectangle nearX..farX. nearX/farX don't
    // need nearX<farX -- sorted into left/right here, once, since both
    // tapers and the middle band share the same two x's.
    static void DrawTaperedPanel(ulong nearX, ulong topRow, ulong farX, ulong farTopRow, ulong farBottomRow, ulong bottomRow, BitmapColorSource color)
    {
        ulong left = nearX < farX ? nearX : farX;
        ulong right = nearX < farX ? farX : nearX;
        C64.Screen.DrawTrapezoid(nearX, nearX, topRow, left, right, farTopRow, true, color);
        C64.Screen.DrawRectangle(left, farTopRow, right, farBottomRow, true, true, color);
        C64.Screen.DrawTrapezoid(left, right, farBottomRow, nearX, nearX, bottomRow, true, color);
    }
}
