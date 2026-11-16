using C64Lib;

namespace Hunchback;

// One attempt at one level: from "GET READY" until the player has rung the
// bell at the end of the wall or lost a life.
//
// Sprites: 0 the player, 1 the climbing knight, 2 and 5 the missiles, 3 and
// 4 the rope, 6 the heart over Esmerelda's tower.
class LevelPlay
{
    // The game runs on the 60 Hz clock, as the original does: its missiles,
    // knight and rope move from the timer interrupt, every tick, and its
    // main loop -- the player -- comes round about every third.
    const uint PlayerTicks = 3;

    // scrollIn: the level before was completed, and this one scrolls in from
    // the right while the player walks back to its start; otherwise it is
    // simply there.
    public bool Play(LevelDescription description, PlayerStats playerStats, uint levelNumber, bool scrollIn)
    {
#if SCENE_TOWER
        // (the scene's own player cannot dodge: no missiles on the way to her)
        description.EnemyType = EnemyType.None;
        description.EnemyType2 = EnemyType.None;
#endif
        // The whole picture is drawn out of sight first.
        Wall wall = new Wall();
        Screen.DrawOffScreen();
        wall.Draw(description.Color, description.WallType, description.Tower);
        Screen.DrawOnScreen();

        // The missiles, the rope and the heart are not there yet. The
        // player, the knight and the missiles are multicolour sprites, the
        // rope and the heart are not.
        C64.Sprites.VisibleMask = C64.Sprites.VisibleMask & 0x83;
        C64.Sprites.MultiColorMask = 0x27;

        Player player = new Player()
        {
            Sprite = C64.Sprites.Sprite0
        };
        player.Init(wall);
        player.IsEsmereldaLevel = description.Tower;
        if (scrollIn)
        {
            ScrollIn(player);
        }
        else
        {
            C64.Sprites.Sprite1.Visible = false;
            Screen.Show();
        }
        playerStats.Draw(levelNumber);

        Knight knight = new Knight()
        {
            Sprite = C64.Sprites.Sprite1,
            Rate = levelNumber < 16 ? 48u : levelNumber < 32 ? 32u : 16u,
            Color = description.Color
        };
        knight.Init();
        GetReady();
        player.ClearCollisions();

        if (description.Tower)
        {
            C64.Sprites.Sprite6.DataBlock = C64Address.FromLabel("spt_heart");
            C64.Sprites.Sprite6.Color = Colors.Red;
            C64.Sprites.Sprite6.ExpandX = false;
            C64.Sprites.Sprite6.ExpandY = false;
            C64.Sprites.Sprite6.X = 240;
            C64.Sprites.Sprite6.Y = 74;
            C64.Sprites.Sprite6.Visible = true;
        }

        Rope rope = null;
        if (description.WallType == WallType.Rope)
        {
            rope = new Rope
            {
                Sprite1 = C64.Sprites.Sprite3,
                Sprite2 = C64.Sprites.Sprite4,
            };
            rope.Init();
        }

        Enemy enemy = new Enemy()
        {
            Sprite = C64.Sprites.Sprite2,
            EnemyType = description.EnemyType
        };
        enemy.Init();
        Enemy enemy2 = null;
        if (description.EnemyType2 != EnemyType.None)
        {
            enemy2 = new Enemy()
            {
                Sprite = C64.Sprites.Sprite5,
                EnemyType = description.EnemyType2
            };
            enemy2.Init();
            enemy2.MoveDelay = 96;
        }
        var gameObjects = new GameObject[] { enemy, enemy2, knight, rope, wall };

        uint phase = 0;
        uint heart = 0;
        for (; ; )
        {
            phase++;
            if (phase == PlayerTicks)
            {
                phase = 0;
                player.Move();

                var collisions = C64.Sprites.Collisions;
                if ((collisions & 1u) > 0)
                {
                    if ((collisions & 38u) > 0)  // the knight or a missile
                    {
                        player.Die();
                    }
                    else if ((collisions & 24u) > 0)
                    {
                        player.SetOnRope(rope);
                    }
                }
                if (player.Dead)
                {
                    playerStats.Lives--;
                    playerStats.Bonus = 0;
                    playerStats.DrawLives();
                    playerStats.DrawBonus();
                    Sfx.Dead();
                    C64.Sprites.VisibleMask = 0;
                    return false;
                }
                if (player.Complete)
                {
                    Completed(playerStats, knight, levelNumber);
                    return true;
                }
                // Debug/testing cheat: L completes the level.
                if (C64.IsKeyPressed(Keys.L))
                    player.Complete = true;
            }

            foreach (var go in gameObjects)
                go?.Move();

            if (description.Tower)
            {
                // the heart over the tower beats
                heart++;
                if (heart == 64)
                    C64.Sprites.Sprite6.DataBlock = C64Address.FromLabel("spt_heart_1");
                if (heart == 128)
                {
                    heart = 0;
                    C64.Sprites.Sprite6.DataBlock = C64Address.FromLabel("spt_heart");
                }
            }

            C64.Delay(1);
        }
    }

    // The bell at the end of the wall has been rung (the original's
    // Level_Complete): a bell for the bonus row, and five of them cashed in
    // one by one; the level's own score in a box where the knight got to;
    // the bonus man at 10,000 points.
    private static void Completed(PlayerStats playerStats, Knight knight, uint levelNumber)
    {
        playerStats.Bonus++;
        playerStats.DrawBonus();
        if (playerStats.Bonus == 5)
        {
            while (playerStats.Bonus != 0)
            {
                playerStats.Score += 50;        // 5,000 a bell
                playerStats.Bonus--;
                C64.Sound.PlayEffectReg2(WaveForm.Triangle, 0x7A64UL, 0UL, 0x0a, 0x00, false);
                playerStats.DrawBonus();
                playerStats.DrawScore();
                Screen.Wait(40);
            }
        }

        // the bell: high, then low
        C64.Sound.PlayEffectReg2(WaveForm.Triangle, 0x4864UL, 0x1000UL, 0x0a, 0x00, false);
        Screen.Wait(15);
        C64.Sound.PlayEffectReg2(WaveForm.Triangle, 0x1264UL, 0x1000UL, 0x0a, 0x00, false);

        // Hundreds of points: the sooner (by the knight's steps) the more,
        // times the number of the level.
        ulong points = LevelScoreMultiplier(knight.Counter) * (ulong)(levelNumber + 1);
        playerStats.Score += points;
        C64.Sprites.Sprite1.Visible = false;     // the box goes where he stands
        ShowLevelScoreBox(knight.Column, knight.Y, points);
        playerStats.DrawScore();
        Screen.Wait(160);
        C64.Sprites.VisibleMask = 1;

        if (playerStats.Score >= 100 && !playerStats.BonusLife)
        {
            playerStats.BonusLife = true;
            playerStats.Lives++;
            playerStats.DrawLives();
        }
    }

    // Esmerelda is reached (Quasi_RescueEsmerelda): "MY HERO!", seven
    // hearts beating around him, and 20,000 points.
    private static readonly ulong[] HeartX = { 72, 56, 148, 248, 292, 235, 136 };
    private static readonly uint[] HeartY = { 114, 182, 208, 195, 134, 96, 70 };

    public static void Rescue(PlayerStats playerStats)
    {
        Screen.Clear(Colors.Black);
        Screen.Text(16, 10, C64Address.FromLabel("txt_MyHero"), 8, Colors.Cyan);

        var hero = C64.Sprites.Sprite0;
        hero.DataBlock = C64Address.FromLabel("spt_player_right_1");
        hero.X = 170;
        hero.Y = 152;
        C64.Sprites.ExpandXMask = 0;
        C64.Sprites.ExpandYMask = 0;
        C64.Sprites.MultiColorMask = 1;
        for (uint i = 0; i < 7; i++)
        {
            var heart = C64.Sprites[i + 1];
            heart.Color = Colors.Red;
            heart.X = HeartX[i];
            heart.Y = HeartY[i];
        }
        Hearts(0);
        C64.Sprites.VisibleMask = 255;
        for (uint beat = 27; beat > 0; beat--)
        {
            Hearts(beat & 3);
            Screen.Wait(20);
        }
        Hearts(0);

        playerStats.Score += 200;
        Screen.Wait(240);
        C64.Sprites.VisibleMask = 0;
    }

    // All seven hearts in one of their frames: 0 whole, 1-3 the beat.
    private static void Hearts(uint frame)
    {
        ulong data = C64Address.FromLabel("spt_heart");
        if (frame == 1 || frame == 3)
            data = C64Address.FromLabel("spt_heart_3");
        if (frame == 2)
            data = C64Address.FromLabel("spt_heart_2");
        for (uint i = 1; i < 8; i++)
            C64.Sprites[i].DataBlock = data;
    }

    // The next wall comes in from the right, 40 columns of it, while the
    // player walks back to where it starts (Screen_LevelTransitionScroll and
    // Quasi_LevelTransitionMove). He arrives as the last column does: 135
    // steps of two pixels, three a column and a fourth on three columns in
    // every eight.
    private static void ScrollIn(Player player)
    {
        player.X = 310;
        for (uint column = 0; column < 40; column++)
        {
            Screen.ScrollLeft(column);
            uint steps = (column & 7) < 3 ? 4u : 3u;
            for (uint i = 0; i < steps; i++)
            {
                player.WalkLeft();
                C64.Delay(i == 0 ? 2u : 1u);
            }
        }
        player.X = 40;
    }

    private static void GetReady()
    {
        // What is under the text is put back afterwards (a rope of the row
        // of bells hangs through it).
        ulong text = C64Address.FromLabel("(screenMemory+256)");
        ulong colours = C64Address.FromLabel("(colorMemory+256)");
        ulong saved = C64Address.FromLabel("tmpScreen");
        C64.CopyMemory(saved, text, 9);
        C64.CopyMemory(saved + 9, colours, 9);
        Screen.Text(16, 6, C64Address.FromLabel("txt_GetReady"), 9, Colors.White);
        Screen.Wait(180);
        C64.CopyMemory(text, saved, 9);
        C64.CopyMemory(colours, saved + 9, 9);
    }

    // The original's tbl_ScoreMultiplier, read the way its
    // Player_LevelCompleteScore does: 8 with fewer than 20 of the knight's
    // steps taken, one less for every ten more, down to 1 from 80 on.
    private static readonly uint[] StepThresholds = { 20, 30, 40, 50, 60, 70, 80 };

    private static ulong LevelScoreMultiplier(uint steps)
    {
        for (uint i = 0; i < StepThresholds.Length; i++)
        {
            if (steps < StepThresholds[i])
                return 8UL - i;
        }
        return 1;
    }

    // The "SCORE" box, where the knight ended up (the original's
    // Player_LevelCompleteScore): two columns to his left, at his height.
    private static void ShowLevelScoreBox(uint col, uint knightY, ulong points)
    {
        const uint TopLeft = 91, TopFill = 95, TopRight = 92;
        const uint LeftSideWall = 0x62, RightSideWall = 0x61;
        const uint BottomLeft = 93, BottomFill = 96, BottomRight = 94;

        col = col < 2 ? 0 : col - 2;
        if (col > 33) col = 33;
        uint row = knightY > 53 ? (knightY - 53) >> 3 : 0;
        if (row > 20) row = 20;

        C64.Screen.SetChar(col, row, TopLeft, Colors.LightRed);
        for (uint x = 1; x < 6; x++)
            C64.Screen.SetChar(col + x, row, TopFill, Colors.LightRed);
        C64.Screen.SetChar(col + 6, row, TopRight, Colors.LightRed);

        C64.Screen.SetChar(col, row + 1, LeftSideWall, Colors.LightRed);
        C64.Screen.Write(col + 1, row + 1, "SCORE", Colors.White);
        C64.Screen.SetChar(col + 6, row + 1, RightSideWall, Colors.LightRed);

        C64.Screen.SetChar(col, row + 2, LeftSideWall, Colors.LightRed);
        C64.Screen.Write(col + 1, row + 2, points.ToString().PadLeft(3, '0'), Colors.White);
        C64.Screen.Write(col + 4, row + 2, "00", Colors.White);
        C64.Screen.SetChar(col + 6, row + 2, RightSideWall, Colors.LightRed);

        C64.Screen.SetChar(col, row + 3, BottomLeft, Colors.LightRed);
        for (uint x = 1; x < 6; x++)
            C64.Screen.SetChar(col + x, row + 3, BottomFill, Colors.LightRed);
        C64.Screen.SetChar(col + 6, row + 3, BottomRight, Colors.LightRed);
    }
}
