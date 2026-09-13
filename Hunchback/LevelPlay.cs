using C64Lib;

namespace Hunchback;

class LevelPlay
{
    public bool Play(LevelDescription description, PlayerStats playerStats)
    {
        Wall wall = new Wall();
        wall.Draw(description.Color, description.WallType);
        playerStats.Draw();

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
        var gameObjects = new GameObject[] { enemy, knight, rope, wall };
        for (; ; )
        {
            player.Move();

            var collisions = C64.Sprites.Collisions;
            if ((collisions & 1u) > 0)
            {
                if ((collisions & 6u) > 0)  // Player-enemy collision
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
        C64.Write(16, 6, "GET READY", Colors.White);
        for (int i = 0; i < 40; i++)
            Delay.Wait(100);
        C64.Write(16, 6, "         ", Colors.White);
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