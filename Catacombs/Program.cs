using C64Lib;

namespace Catacombs;

class Program
{
    static uint px_, py_, dir_;    // dir: 0=N, 1=E, 2=S, 3=W (see Maze)

    static void Main()
    {
        px_ = 1;
        py_ = 1;
        dir_ = 1; // facing east into the first corridor

        // MultiColor (not plain Bitmap) so doors can be a color distinct
        // from the lines: MatrixHigh=White (wall/outline lines),
        // ColorRam=Brown (doors -- DungeonView passes
        // BitmapColorSource.ColorRam on every door rectangle, MatrixHigh
        // on every line). Everything else -- the screen background
        // ($D021, what every unset pixel shows: walls, floor, ceiling)
        // and the border ($D020, a separate VIC register) -- is the
        // current room's own color (Maze.RoomColor, see ApplyRoomColor),
        // so the room reads as lines and doors on one solid color, with
        // no visible frame around it.
        C64.Screen.SetScreenMode(ScreenMode.MultiColor);
        ApplyRoomColor();
        C64.Screen.SetBitmapColors(0x10); // MatrixHigh=White(1); MatrixLow unused
        var colorRam = 0xD800UL;
        for (ulong offset = 0; offset < 1000UL; offset += 250UL)
            C64.FillMemory(colorRam + offset, (uint)Colors.Brown, 250);
        C64.Screen.SetDrawBuffer(1);
        Bats.Init();
        Bats.SetRoomColor(Maze.RoomColorValue(px_, py_));

        // Buffer 1 starts out blank (SetScreenMode clears both buffers),
        // so the very first draw needs no explicit clear first.
        DungeonView.Render(px_, py_, dir_);
        C64.Screen.SwapBuffers();

        for (;;)
        {
            if (HandleInput())
            {
                // The buffer SwapBuffers just showed still holds the view from
                // two moves ago -- clear it before drawing the new one on
                // top, or the old lines would show through.
                C64.Screen.ClearBitmap();
                DungeonView.Render(px_, py_, dir_);
                C64.Screen.SwapBuffers();
                // After the swap, not before: background/border are single
                // global registers, not per buffer, so changing them while
                // the new view is still being drawn would recolor the OLD
                // room's picture mid-redraw.
                ApplyRoomColor();
            }
        }
    }

    // Background and border both take the current room's color.
    static void ApplyRoomColor()
    {
        Colors c = Maze.RoomColor(px_, py_);
        C64.Screen.SetBorderColor(c);
        C64.Screen.SetBackgroundColor(c);
        Bats.SetRoomColor(Maze.RoomColorValue(px_, py_));
    }

    // Blocks until W/A/S/D is pressed, applies it, and waits for release
    // (same wait-then-debounce shape as C64Presentation's KeyBoard.
    // WaitForKeys) so one press moves once, not dozens of times. Always
    // returns true (there's nothing else this loop does while waiting) --
    // the bool keeps the call site reading as "did something happen."
    static bool HandleInput()
    {
        // A single loop condition, no `break` -- a for(;;) with several
        // "if (...) { ...; break; }" exits in a row hits a real limitation
        // in this compiler (ILMethodBuildEvaluationStackPass's traversal
        // order over a loop with more than one exit edge, unrelated to
        // this project); this shape avoids it.
        Keys key = Keys.W;
        bool found = false;
        while (!found)
        {
            // One video frame per pass: paces the bats' animation, and
            // key polling runs at 50 Hz, plenty responsive.
            C64.Screen.WaitForVBlank();
            Bats.Animate();
            if (C64.IsKeyPressed(Keys.W)) { key = Keys.W; found = true; }
            else if (C64.IsKeyPressed(Keys.S)) { key = Keys.S; found = true; }
            else if (C64.IsKeyPressed(Keys.A)) { key = Keys.A; found = true; }
            else if (C64.IsKeyPressed(Keys.D)) { key = Keys.D; found = true; }
        }

        if (key == Keys.W) MoveForward();
        else if (key == Keys.S) MoveBackward();
        else if (key == Keys.A) dir_ = (dir_ + 3) % 4;
        else dir_ = (dir_ + 1) % 4;

        while (C64.IsKeyPressed(key))
            Delay.Wait(500);
        return true;
    }

    static void MoveForward()
    {
        uint nx = px_, ny = py_;
        if (dir_ == 0) ny = py_ - 1;
        else if (dir_ == 1) nx = px_ + 1;
        else if (dir_ == 2) ny = py_ + 1;
        else nx = px_ - 1;
        if (!Maze.IsWall(nx, ny))
        {
            AnimateDoorOpening(nx, ny);
            px_ = nx;
            py_ = ny;
        }
    }

    // EXPERIMENT: right edge of the door panel at each animation frame --
    // top and bottom given separately (see DungeonView.RenderOpening) so
    // the panel is a trapezoid, not a flat rectangle. The bottom edge
    // recedes from the closed door's right edge (187) to the hinge (131)
    // the same as before; the top edge recedes faster (halved distance
    // from the hinge at every frame), so the panel reads as a door
    // swinging away top-first rather than a panel sliding sideways. Both
    // reach the hinge together on the last frame (fully open).
    static readonly ulong[] DoorOpenPanelRightBottom = { 171, 155, 143, 135, 131 };
    static readonly ulong[] DoorOpenPanelRightTop = { 151, 143, 137, 133, 131 };

    // Opens the front door of the CURRENT room, with the doorway showing
    // the next room's color (MatrixLow, which nothing else draws with, is
    // set to it), before the caller moves into it. Each frame goes through
    // the same clear/render/swap as a normal redraw, so it stays
    // double-buffered.
    static void AnimateDoorOpening(uint nx, uint ny)
    {
        C64.Screen.SetBitmapColors(0x10 + Maze.RoomColorValue(nx, ny));
        for (uint i = 0; i < 5; i++)
        {
            C64.Screen.ClearBitmap();
            DungeonView.RenderOpening(px_, py_, dir_, DoorOpenPanelRightTop[i], DoorOpenPanelRightBottom[i]);
            C64.Screen.SwapBuffers();
            Bats.Animate();
        }
    }

    static void MoveBackward()
    {
        uint nx = px_, ny = py_;
        if (dir_ == 0) ny = py_ + 1;
        else if (dir_ == 1) nx = px_ - 1;
        else if (dir_ == 2) ny = py_ - 1;
        else nx = px_ + 1;
        if (!Maze.IsWall(nx, ny)) { px_ = nx; py_ = ny; }
    }
}
