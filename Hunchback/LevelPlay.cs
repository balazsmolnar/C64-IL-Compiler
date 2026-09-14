using C64Lib;

namespace Hunchback;

class LevelPlay
{
    public bool Play(LevelDescription description, PlayerStats playerStats, uint levelNumber)
    {
        Wall wall = new Wall();
        wall.Draw(description.Color, description.WallType);
        playerStats.Draw();
        DrawDebugLevelNumber(levelNumber);

        // Rope/Enemy/Enemy2 aren't set up until after GetReady() below (they
        // shouldn't appear until gameplay actually starts) -- but Screen.Clear()
        // only wiped the sprite POINTER bytes for the new level, not the
        // separate hardware enable bits, so whatever was left enabled from
        // the previous level's rope/enemies would otherwise still show,
        // now pointing at this level's (blank) pointer value, for the whole
        // Get Ready pause. Disable them upfront so nothing stale is visible
        // until each one's own Init() turns it back on with real data.
        C64.Sprites.Sprite2.Visible = false;
        C64.Sprites.Sprite3.Visible = false;
        C64.Sprites.Sprite4.Visible = false;
        C64.Sprites.Sprite5.Visible = false;

        Player player = new Player()
        {
            Sprite = C64.Sprites.Sprite0
        };
        player.Init(wall);
        Knight knight = new Knight()
        {
            Sprite = C64.Sprites.Sprite1
        };
        knight.Init();
        GetReady();
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
        else
        {
            C64.Sprites.Sprite3.Visible = false;
            C64.Sprites.Sprite4.Visible = false;
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
            enemy2.MoveDelay = 20;
        }
        else
        {
            C64.Sprites.Sprite5.Visible = false;
        }
        var gameObjects = new GameObject[] { enemy, enemy2, knight, rope, wall };
        for (; ; )
        {
            player.Move();

            var collisions = C64.Sprites.Collisions;
            if ((collisions & 1u) > 0)
            {
                if ((collisions & 38u) > 0)  // Player-enemy collision (Knight, Enemy, or Enemy2)
                {
                    player.Die();
                }
                else
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
                return false;
            }
            if (player.Complete)
            {
                playerStats.Bonus++;
                if (playerStats.Bonus == 5)
                {
                    BonusFanfare();
                    playerStats.Bonus = 0;
                    playerStats.Lives++;
                    playerStats.DrawLives();
                }
                playerStats.DrawBonus();
                return true;
            }
            foreach (var go in gameObjects)
                go?.Move();

            Delay.Wait(100);
            // Debug/testing cheat: force the level complete instead of playing
            // it out. Sets Complete rather than returning true directly so it
            // still goes through the normal bonus/fanfare handling above.
            if (C64.IsKeyPressed(Keys.L))
                player.Complete = true;
        }
    }

    private static void GetReady()
    {
        // Save what's actually under the text (e.g. a row-of-bells rope
        // strand) instead of just blanking to spaces afterward, which used
        // to punch a visible gap through it.
        var c0 = (uint)C64.GetChar(16, 6);
        var c1 = (uint)C64.GetChar(17, 6);
        var c2 = (uint)C64.GetChar(18, 6);
        var c3 = (uint)C64.GetChar(19, 6);
        var c4 = (uint)C64.GetChar(20, 6);
        var c5 = (uint)C64.GetChar(21, 6);
        var c6 = (uint)C64.GetChar(22, 6);
        var c7 = (uint)C64.GetChar(23, 6);
        var c8 = (uint)C64.GetChar(24, 6);

        C64.Write(16, 6, "GET READY", Colors.White);
        for (int i = 0; i < 40; i++)
            Delay.Wait(100);

        // Grey2 matches the only decoration that can be here today (the
        // row-of-bells rope); harmless elsewhere since a space's color
        // isn't visible.
        C64.SetChar(16, 6, c0, Colors.Grey2);
        C64.SetChar(17, 6, c1, Colors.Grey2);
        C64.SetChar(18, 6, c2, Colors.Grey2);
        C64.SetChar(19, 6, c3, Colors.Grey2);
        C64.SetChar(20, 6, c4, Colors.Grey2);
        C64.SetChar(21, 6, c5, Colors.Grey2);
        C64.SetChar(22, 6, c6, Colors.Grey2);
        C64.SetChar(23, 6, c7, Colors.Grey2);
        C64.SetChar(24, 6, c8, Colors.Grey2);
    }

    // Debug aid: show the (0-based) level index at the top right of the
    // header, in the free columns after the lives markers. No division/
    // modulo support in the compiler, so tens/ones are split by repeated
    // subtraction instead.
    private static void DrawDebugLevelNumber(uint levelNumber)
    {
        uint tens = 0;
        uint ones = levelNumber;
        while (ones >= 10)
        {
            ones -= 10;
            tens++;
        }
        C64.SetChar(36, 1, 76, Colors.Yellow); // 'L'
        C64.SetChar(37, 1, 48 + tens, Colors.Yellow);
        C64.SetChar(38, 1, 48 + ones, Colors.Yellow);
    }

    private static void BonusFanfare()
    {
        for (uint i = 0; i < 5; i++)
        {
            C64.Sound.PlayEffectReg2(WaveForm.Triangle, 0x2864UL, 0x1000UL, 0x0a, 0x00, false);
            Delay.Wait(150);
        }
    }
}