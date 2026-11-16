using C64Lib;

namespace Hunchback;

// The intro: three walls scroll past while the hunchback bounds along the
// foot of the castle to the intro tune, then he climbs the first wall. It
// runs before every game, and as the title screens' demo, where it goes on
// with "FEATURING THE BELLS" (Restructure/Menu.asm's Menu_DisplayIntro,
// Screen.asm's Screen_IntroScrollSelect and Screen_DisplayEndOfDemoMode,
// Quasi.asm's Quasi_Intro... routines).
//
// The original moves him and plays the tune from the timer interrupt while
// its main program scrolls. Here everything is in one loop that asks the
// clock how many 60ths of a second have passed (Advance) and gives the tune
// and the hunchback that many.
class Intro
{
    const uint GroundY = 216;       // the foot of the castle
    const uint WallY = 117;         // the top of the wall

    private static ulong x_;        // the sprite's X
    private static uint y_;
    private static uint stage_;     // 0 running in, 1 on the spot while the walls go by, 2 running out
    private static uint jump_;      // where in his 16-step bound
    private static uint moveClock_;
    private static uint note_;
    private static uint noteLeft_;
    private static uint last_;      // the clock when Advance last looked
    private static uint key_;       // 1 = F1, 2 = SPACE or fire: the intro was cut short
    private static bool demo_;

    // Returns 0, or the key that cut it short: as the demo F1 (1) or SPACE
    // (2); before a game SPACE or fire skip to its end, and it returns 0.
    public static uint Play(bool demo)
    {
        demo_ = demo;
        key_ = 0;
        var wall = new Wall();

        // The last wall of the sixteen, with Esmerelda's tower, to begin with.
        Build(wall, 15);
        Screen.Clear(Colors.White);
        PlayerStats.DrawHeader();
        Screen.Show();

        var sprite = C64.Sprites.Sprite0;
        sprite.MultiColor = true;
        sprite.Color = Colors.Green;
        x_ = 336;
        y_ = GroundY;
        stage_ = 0;
        jump_ = 0;
        moveClock_ = 0;
        note_ = 0;
        noteLeft_ = 1;
        Move();
        C64.Sprites.VisibleMask = 1;
        last_ = C64.GetMemory(0xA2UL, 0);

        while (stage_ == 0 && key_ == 0)
            Advance();
        Scroll(wall, 9);
        Scroll(wall, 8);
        Scroll(wall, 0);
        stage_ = 2;
        while (x_ >= 41 && key_ == 0)
            Advance();

        if (key_ != 0)
        {
            if (demo)
                return key_;
            // skipped: the first wall is simply there
            Build(wall, 0);
            Screen.Show();
            y_ = WallY;
        }

        // up the wall
        x_ = 40;
        sprite.X = x_;
        while (y_ > WallY)
        {
            y_ = y_ > WallY + 2 ? y_ - 2 : WallY;
            sprite.Y = y_;
            C64.Delay(1);
        }
        sprite.Y = WallY;
        sprite.DataBlock = C64Address.FromLabel("spt_player_right_0");
        if (!demo)
            return 0;
        EndOfDemo();
        C64.Sprites.VisibleMask = 0;
        return key_;
    }

    private static void Build(Wall wall, uint level)
    {
        var description = LevelDescription.Get(level);
        Screen.DrawOffScreen();
        wall.Draw(description.Color, description.WallType, description.Tower);
        Screen.DrawOnScreen();
    }

    // The next wall comes in from the left, a column about every five ticks.
    private static void Scroll(Wall wall, uint level)
    {
        if (key_ != 0)
            return;
        Build(wall, level);
        for (uint step = 0; step < 40; step++)
        {
            uint waited = 0;
            while (waited < 5 && key_ == 0)
                waited = waited + Advance();
            if (key_ != 0)
                return;
            Screen.ScrollRight(39 - step);
        }
    }

    // Lets time pass: at least a tick. The tune and the hunchback get the
    // ticks that have passed since the last call (more than one after a
    // scroll step); returns how many.
    private static uint Advance()
    {
        C64.Delay(1);
        uint now = C64.GetMemory(0xA2UL, 0);
        uint elapsed = now - last_;
        last_ = now;

        // the tune: 64 notes, round and round (Sound_PlayIntroTune)
        if (noteLeft_ > elapsed)
        {
            noteLeft_ = noteLeft_ - elapsed;
        }
        else
        {
            noteLeft_ = C64.GetMemory(C64Address.FromLabel("tbl_IntroTuneDuration"), note_);
            C64.Sound.PlayEffectReg1(WaveForm.Saw, C64.GetWord(C64Address.FromLabel("tbl_IntroTuneFreq"), note_), 0, 9, 0, false);
            note_ = (note_ + 1) & 63;
        }

        // the hunchback: a step every second tick
        moveClock_ = moveClock_ + elapsed;
        while (moveClock_ >= 2)
        {
            moveClock_ = moveClock_ - 2;
            Move();
        }

        CheckKeys();
        return elapsed;
    }

    private static bool CheckKeys()
    {
        if (C64.IsKeyPressed(Keys.Space) || Input.Fire)
            key_ = 2;
        else if (demo_ && C64.IsKeyPressed(Keys.F1))
            key_ = 1;
        return key_ != 0;
    }

    // One step of his bounding run to the left (Quasi_IntroMovement).
    private static void Move()
    {
        var sprite = C64.Sprites.Sprite0;
        y_ = GroundY - C64.GetMemory(C64Address.FromLabel("tbl_QuasiJumpYOffset"), jump_);
        sprite.Y = y_;
        JumpFrame();
        if (stage_ != 1)
        {
            x_ = x_ - 2;
            if (stage_ == 0 && x_ == 168)
                stage_ = 1;
            sprite.X = x_;
        }
        jump_ = (jump_ + 1) & 15;
    }

    private static void JumpFrame()
    {
        var sprite = C64.Sprites.Sprite0;
        if (jump_ < 6)
            sprite.DataBlock = C64Address.FromLabel("spt_player_jump_left_0");
        else if (jump_ < 10)
            sprite.DataBlock = C64Address.FromLabel("spt_player_jump_left_1");
        else
            sprite.DataBlock = C64Address.FromLabel("spt_player_jump_left_2");
    }

    // "FEATURING": he walks the wall to its end, then bounds back, and every
    // bound leaves a letter of THE BELLS behind; they flash.
    private static void EndOfDemo()
    {
        var sprite = C64.Sprites.Sprite0;
        while (x_ < 312)
        {
            if (CheckKeys())
                return;
            x_ = x_ + 2;
            sprite.X = x_;
            switch (((uint)x_ >> 1) & 3)
            {
                case 0:
                    sprite.DataBlock = C64Address.FromLabel("spt_player_right_0");
                    break;
                case 1:
                    sprite.DataBlock = C64Address.FromLabel("spt_player_right_3");
                    break;
                case 2:
                    sprite.DataBlock = C64Address.FromLabel("spt_player_right_1");
                    break;
                case 3:
                    sprite.DataBlock = C64Address.FromLabel("spt_player_right_2");
                    break;
            }
            C64.Delay(4);
        }

        Screen.Text(16, 6, C64Address.FromLabel("txt_Featuring"), 9, Colors.White);
        jump_ = 0;
        while (x_ >= 28)
        {
            if (CheckKeys())
                return;
            sprite.Y = WallY - C64.GetMemory(C64Address.FromLabel("tbl_QuasiJumpYOffset"), jump_);
            JumpFrame();
            x_ = x_ - 2;
            sprite.X = x_;
            if (x_ < 256)
                Letter((uint)x_ - 24);
            jump_ = (jump_ + 1) & 15;
            C64.Delay(2 + (jump_ & 1));
        }

        uint color = (uint)Colors.LightBlue;
        for (uint i = 0; i < 64; i++)
        {
            if (CheckKeys())
                return;
            color = color ^ 4;
            C64.FillMemory(C64Address.FromLabel("(colorMemory+371)"), color, 18);
            C64.FillMemory(C64Address.FromLabel("(colorMemory+411)"), color, 18);
            C64.Delay(5);
        }
        for (uint i = 0; i < 120; i++)
        {
            if (CheckKeys())
                return;
            C64.Delay(5);
        }
    }

    // The letter of THE BELLS he has just reached, if any: two characters
    // by two, on rows 9 and 10 (Screen_DisplayTheBells).
    private static void Letter(uint x)
    {
        for (uint i = 0; i < 8; i++)
        {
            if (C64.GetMemory(C64Address.FromLabel("tbl_TheBellsQuasiX"), i) == x)
            {
                uint column = C64.GetMemory(C64Address.FromLabel("tbl_TheBellsScreenLo"), i) - 0x68;
                ulong chars = C64Address.FromLabel("tbl_TheBellsChars");
                uint c = i << 2;
                C64.Screen.SetChar(column, 9, C64.GetMemory(chars, c), Colors.LightBlue);
                C64.Screen.SetChar(column + 1, 9, C64.GetMemory(chars, c + 1), Colors.LightBlue);
                C64.Screen.SetChar(column, 10, C64.GetMemory(chars, c + 2), Colors.LightBlue);
                C64.Screen.SetChar(column + 1, 10, C64.GetMemory(chars, c + 3), Colors.LightBlue);
            }
        }
    }
}
