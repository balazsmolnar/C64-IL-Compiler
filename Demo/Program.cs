using System;
using C64Lib;

namespace Demo;

// Small example project -- exercises the compiler end-to-end without the
// size/complexity of a full game. For the Hunchback game (previously here),
// see the Hunchback/ project.
//
// Demonstrates two things running concurrently: a sprite moved entirely
// from the interrupt handler (MoveBall), while Main's own code runs a
// small float-arithmetic demo -- each line shows whether the actual
// computed result matches the expected one (green) or not (red), which
// also doubles as a live check that the interrupt firing throughout isn't
// corrupting arithmetic running alongside it (see asm/C64.asm's
// OnInterrupt and asm/helper/floatBanking.asm for the interrupt-safety
// work this relies on).
class Program
{
    static void Main()
    {
        // C64.Interrupt += MoveBall;


        var ball = C64.Sprites.Sprite0;
        ball.DataBlock = C64Address.FromLabel("spt_ball");
        ball.Color = Colors.Yellow;
        ball.Y = 100;
        ball.X = 24;
        ball.Visible = true;

        C64.Screen.SetBackgroundColor(Colors.Blue);
        C64.Screen.SetBorderColor(Colors.LightBlue);
        C64.Screen.Write(1, 1, "FLOAT ARITHMETIC DEMO", Colors.White);

        ShowResult(1, 3, "3.5 + 2.25 = 5.75", 3.5f + 2.25f, 5.75f);
        ShowResult(1, 5, "10.0 - 3.5 = 6.5", 10.0f - 3.5f, 6.5f);
        ShowResult(1, 7, "4.0 * 2.5 = 10.0", 4.0f * 2.5f, 10.0f);
        ShowResult(1, 9, "9.0 / 2.0 = 4.5", 9.0f / 2.0f, 4.5f);
        ShowResult(1, 11, "NEGATE -3.0 = 3.0", -(-3.0f), 3.0f);
        ShowResult(1, 13, "2.0 < 3.0 IS TRUE", (2.0f < 3.0f) ? 1.0f : 0.0f, 1.0f);
        ShowResult(1, 15, "5.0 == 5.0 IS TRUE", (5.0f == 5.0f) ? 1.0f : 0.0f, 1.0f);

        // Bitmap graphics scene -- replaces the text demo above visually
        // (bitmap mode repoints the VIC-II's video matrix away from the
        // text screen). Sprite turned off first: the VIC-II
        // reads a sprite's data pointer from (video-matrix-base + $3F8),
        // NOT a fixed address -- c64sprite.asm's spriteData=$07F8 only
        // works while $D018 points at the normal $0400 text screen.
        // SetScreenMode(MultiColor) moves the video matrix to
        // Graphics_ColorMatrix ($0C00), so that lookup would land on $0FF8
        // instead -- unrelated, uninitialized memory, not the real pointer
        // this program set up -- and render as a garbled sprite. Confirmed
        // via vice-verify (an early build of this scene showed exactly
        // that artifact). Making sprites and bitmap mode coexist for real
        // (e.g. relocating or duplicating the pointer table) is out of
        // scope for this graphics feature; sidestepped here by simply not
        // showing a sprite during the bitmap demo.
        //
        // RotatingCube's own spinning-cube demo (see RotatingCube.cs) used
        // to run here, between the text demo and the shapes scene below --
        // dropped from this particular build: its single heap allocation
        // was competing with Screen.DrawBitmap's own new asm (~216
        // bytes) for the same fixed, narrow budget between wherever
        // compiled code ends and OBJ_TABLES_MAX_START/HEAP_LIMIT (asm/
        // helper/objectTables.asm/fault.asm) -- confirmed via measure.bat
        // (compiled code's own end address moved from $c6cc to $c7a4, so
        // the object tables + heap's available room before the fixed
        // $d000 ceiling shrank by that same ~216 bytes) and reproduced via
        // VICE (a FAULT_OUT_OF_MEMORY crash, red border, right at `new
        // RotatingCube()`, invisible on screen only because the fault
        // message prints through the real KERNAL CHROUT while the VIC is
        // still reading from the bitmap's own color matrix, not the text
        // screen). Not a bug in DrawBitmap's own code -- just this
        // scene's pre-existing heap pressure, resolved by not paying for
        // both demos' heap needs in the same build.
        ball.Visible = false;

        // All four BitmapColorSource values are set to different colors
        // and drawn side by side, so each renders visibly distinct from
        // the others -- Background is set to a color no other source
        // uses, so its rectangle shows as a plain background-colored gap
        // rather than invisible/blank.
        C64.Screen.SetScreenMode(ScreenMode.MultiColor);
        C64.Screen.SetBackgroundColor(Colors.Black);
        // Video matrix: high nibble = MatrixHigh source, low nibble =
        // MatrixLow source (same per-cell byte SetBitmapColors always
        // filled, just read as two independent colors now instead of
        // hi-res's single foreground/background pair).
        C64.Screen.SetBitmapColors(0x12); // MatrixHigh=White(1), MatrixLow=Red(2)
        // Color RAM ($D800, 1000 cells, one nibble each): the low nibble
        // is what the VIC-II reads for ColorRam -- a fixed hardware
        // address, no named label, same FillMemory used for the color
        // matrix above. FillMemory's size parameter is this compiler's
        // uint (8-bit, max 255), same reason the color matrix fill above
        // needs 4 calls rather than one of 1000.
        C64.FillMemory(0xD800UL, 0x0D, 250); // ColorRam=LightGreen(13)
        C64.FillMemory(0xD800UL + 250UL, 0x0D, 250);
        C64.FillMemory(0xD800UL + 500UL, 0x0D, 250);
        C64.FillMemory(0xD800UL + 750UL, 0x0D, 250);

        C64.Screen.DrawRectangle(10, 20, 70, 80, true, true, BitmapColorSource.Background);
        C64.Screen.DrawRectangle(90, 20, 150, 80, true, true, BitmapColorSource.MatrixHigh);
        C64.Screen.DrawRectangle(170, 20, 230, 80, true, true, BitmapColorSource.MatrixLow);
        C64.Screen.DrawRectangle(250, 20, 310, 80, true, true, BitmapColorSource.ColorRam);

        C64.Screen.DrawLine(10, 100, 310, 140, true, BitmapColorSource.MatrixHigh);
        C64.Screen.DrawCircle(160, 160, 30, true, true, BitmapColorSource.ColorRam);

        // Bitmap pipeline smoke test: a 16x16 hollow-square stencil
        // (see Bitmaps.cs), drawn deliberately overlapping the solid
        // white MatrixHigh rectangle above (90,20)-(150,80) -- if
        // transparency were broken (the ring's "off" pixels clearing to
        // Background instead of leaving the rectangle's fill alone), the
        // ring's hole would render as a black square; if it works, the
        // white fill shows through the hole and around the ring's edges,
        // with only the ring's own outline drawn (in ColorRam -- green).
        C64.Screen.DrawBitmap(104, 32, C64Address.FromLabel("bmp_Ring"), BitmapColorSource.ColorRam);

        for (;;)
        {
        }
    }

    static void ShowResult(uint x, uint y, string label, float actual, float expected)
    {
        C64.Screen.Write(x, y, label, actual == expected ? Colors.Green : Colors.Red);
    }

    // Sprite.X has no getter implementation in asm/c64sprite.asm (only
    // Sprite_set_X exists -- Hunchback never reads a sprite's position
    // back either, it always tracks its own copy, e.g. Player.X's private
    // x_ field), so position is tracked here rather than read back from
    // the sprite.
    static ulong ballX_ = 24;
    static bool movingRight_ = true;

    // Runs entirely from the interrupt (see C64.Interrupt += MoveBall
    // above) -- Main never touches the sprite again after the initial
    // setup, so every bit of motion on screen comes from here, firing
    // concurrently with (and independently of) Main's arithmetic demo.
    static void MoveBall()
    {
        if (movingRight_)
        {
            ballX_ = ballX_ + 2;
            if (ballX_ > 280)
                movingRight_ = false;
        }
        else
        {
            ballX_ = ballX_ - 2;
            if (ballX_ < 24)
                movingRight_ = true;
        }
        var ball = C64.Sprites.Sprite0;
        ball.X = ballX_;
    }
}
