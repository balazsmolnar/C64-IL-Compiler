using C64Lib;

namespace Catacombs;

// The panel's map (see Hud.cs): the maze's 10x10 interior, one room = 4x2
// hi-res pixels, in panel columns 34-38. Only rooms the player has been in
// are drawn (fog of war); the room the player is in blinks.
//
// A room is half a bitmap byte on two pixel rows, so a pair of rooms is
// exactly one byte value written twice: nothing is read back from the
// screen, and a change redraws two bytes.
static class Minimap
{
    static bool[] seen_;
    static uint x_, y_;
    static bool lit_;

    public static bool Lit => lit_;

    public static void Init()
    {
        seen_ = new bool[Maze.Width * Maze.Height];
        x_ = 1;
        y_ = 1;
    }

    public static void Enter(uint x, uint y)
    {
        uint oldX = x_, oldY = y_;
        x_ = x;
        y_ = y;
        seen_[y * Maze.Width + x] = true;
        lit_ = true;
        DrawPair(oldX, oldY);
        DrawPair(x, y);
    }

    public static void Blink()
    {
        lit_ = !lit_;
        DrawPair(x_, y_);
    }

    public static void Render()
    {
        for (uint y = 1; y <= 10; y++)
            for (uint x = 1; x <= 10; x += 2)
                DrawPair(x, y);
    }

    // Redraws the byte pair holding room (x, y) and its neighbour.
    static void DrawPair(uint x, uint y)
    {
        uint left = (x - 1) | 1;
        uint i = y * Maze.Width + left;
        uint value = 0;
        if (seen_[i] && (lit_ || left != x_ || y != y_))
            value = 0xF0;
        if (seen_[i + 1] && (lit_ || left + 1 != x_ || y != y_))
            value = value | 0x0F;

        uint row = y - 1;
        ulong address = C64Address.FromLabel("Graphics_Bitmap+7311") + (left - 1) * 4 + (row & 3) * 2;
        if (row >= 4)
            address = address + 320;
        if (row >= 8)
            address = address + 320;
        C64.FillMemory(address, value, 2);
    }
}
