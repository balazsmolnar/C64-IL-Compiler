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
        C64.Interrupt += MoveBall;

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

        for (;;) { }
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
