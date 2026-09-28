using C64Lib;

namespace Catacombs;

class Program
{
    static uint px_, py_, dir_;    // dir: 0=N, 1=E, 2=S, 3=W (see Maze)
    static bool drawBuffer1_;      // which buffer Screen.SetDrawBuffer currently targets

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
        drawBuffer1_ = true;

        // Buffer 1 starts out blank (SetScreenMode clears both buffers),
        // so the very first draw needs no explicit clear first.
        DungeonView.Render(px_, py_, dir_);
        C64.Screen.SwapBuffers();
        drawBuffer1_ = false;

        for (;;)
        {
            if (HandleInput())
            {
                ClearDrawBuffer();
                DungeonView.Render(px_, py_, dir_);
                C64.Screen.SwapBuffers();
                drawBuffer1_ = !drawBuffer1_;
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
    }

    // The buffer SwapBuffers just showed still holds the view from two
    // moves ago -- clear it before drawing the new one on top, or the old
    // lines would show through. drawBuffer1_ is kept in sync with
    // SwapBuffers' own internal toggle (see its comment in
    // asm/C64Graphics.asm) purely in C#, since there's no "which buffer is
    // the draw target" query.
    static void ClearDrawBuffer()
    {
        ulong bitmap = drawBuffer1_ ? C64Address.FromLabel("Graphics_Bitmap2") : C64Address.FromLabel("Graphics_Bitmap");
        for (ulong offset = 0; offset < 8000UL; offset += 250UL)
            C64.FillMemory(bitmap + offset, 0, 250);
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
        if (!Maze.IsWall(nx, ny)) { px_ = nx; py_ = ny; }
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
