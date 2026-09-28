using C64Lib;

namespace Catacombs;

// Two bats flying around every room, flapping their wings (Sprites.cs has
// the art: three wing poses, in both VIC banks).
//
// Sprites work differently under bitmap double buffering than the normal
// C64.Sprites.SpriteN.DataBlock setter assumes: the VIC-II reads a sprite's
// data pointer from (video matrix + $3F8), not the text screen's $07F8, and
// each buffer has its own matrix (Graphics_ColorMatrix / Graphics_ColorMatrix2)
// in its own VIC bank. So the pointer bytes are poked directly, into BOTH
// matrices' pointer tables, and the animation frame is just which pointer
// value gets written. Position, color and visibility are ordinary VIC
// registers (not per bank), so those use the normal Sprite properties.
static class Bats
{
    // Pointer value per animation step: mid/up/down poses are blocks 0/1/2
    // of each bank's data (see Sprites.cs); the flap cycle is up, mid, down,
    // mid. Bank 0 blocks are at $3F40/$3F80/$3FC0, bank 1 at $5F40/...:
    // block number within the bank's 16K is address/64 = $FD..$FF and $7D..$7F.
    static readonly uint[] FlapPointer0 = { 0xFE, 0xFD, 0xFF, 0xFD };
    static readonly uint[] FlapPointer1 = { 0x7E, 0x7D, 0x7F, 0x7D };

    // Vertical bobbing, one full cycle over 16 ticks.
    static readonly uint[] Wobble = { 6, 8, 10, 11, 12, 11, 10, 8, 6, 4, 2, 1, 0, 1, 2, 4 };

    static ulong pointers0_, pointers1_;   // sprite pointer tables of the two matrices
    static ulong x0_, x1_;
    static bool right0_, right1_;
    static uint tick_;

    public static void Init()
    {
        pointers0_ = C64Address.FromLabel("Graphics_ColorMatrix") + 0x3F8UL;
        pointers1_ = C64Address.FromLabel("Graphics_ColorMatrix2") + 0x3F8UL;
        x0_ = 60;
        x1_ = 250;
        right0_ = true;
        right1_ = false;
        tick_ = 0;

        var bat0 = C64.Sprites.Sprite0;
        bat0.MultiColor = false;
        bat0.X = x0_;
        bat0.Y = 85;
        bat0.Visible = true;

        var bat1 = C64.Sprites.Sprite1;
        bat1.MultiColor = false;
        bat1.X = x1_;
        bat1.Y = 165;
        bat1.Visible = true;

        SetPointers();
    }

    // Black bats, except in the black room, where they'd vanish.
    public static void SetRoomColor(uint roomColor)
    {
        var bat0 = C64.Sprites.Sprite0;
        var bat1 = C64.Sprites.Sprite1;
        if (roomColor == 0)
        {
            bat0.Color = Colors.White;
            bat1.Color = Colors.White;
        }
        else
        {
            bat0.Color = Colors.Black;
            bat1.Color = Colors.Black;
        }
    }

    // One animation tick: move both bats, and every 4th tick advance the
    // wing pose. Meant to be called once per video frame.
    public static void Animate()
    {
        tick_ = tick_ + 1;

        if (right0_)
        {
            x0_ = x0_ + 2;
            if (x0_ > 290)
                right0_ = false;
        }
        else
        {
            x0_ = x0_ - 2;
            if (x0_ < 40)
                right0_ = true;
        }

        if (right1_)
        {
            x1_ = x1_ + 3;
            if (x1_ > 290)
                right1_ = false;
        }
        else
        {
            x1_ = x1_ - 3;
            if (x1_ < 40)
                right1_ = true;
        }

        var bat0 = C64.Sprites.Sprite0;
        bat0.X = x0_;
        bat0.Y = 85 + Wobble[tick_ % 16];

        var bat1 = C64.Sprites.Sprite1;
        bat1.X = x1_;
        bat1.Y = 165 + Wobble[(tick_ + 8) % 16];

        if (tick_ % 4 == 0)
            SetPointers();
    }

    // Writes the current wing pose of both bats into both buffers' pointer
    // tables (whichever buffer is on screen decides which one the VIC reads;
    // the bats are out of step by half a flap).
    static void SetPointers()
    {
        uint frame0 = (tick_ / 4) % 4;
        uint frame1 = (tick_ / 4 + 2) % 4;
        Poke(pointers0_, FlapPointer0[frame0]);
        Poke(pointers0_ + 1UL, FlapPointer0[frame1]);
        Poke(pointers1_, FlapPointer1[frame0]);
        Poke(pointers1_ + 1UL, FlapPointer1[frame1]);
    }

    // FillMemory writes offsets 1..size of its target (its loop never
    // touches offset 0), so writing one byte at `address` means filling one
    // byte starting one below it.
    static void Poke(ulong address, uint value)
    {
        C64.FillMemory(address - 1UL, value, 1);
    }
}
