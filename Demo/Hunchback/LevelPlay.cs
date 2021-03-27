using C64Lib;

namespace Hunchback
{
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
                Sprite = C64.Sprites.Sprite2
            };
            enemy.Init(description.EnemyType);

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
                    return false;
                if (player.Complete)
                    return true;
                knight.Move();
                enemy.Move();
                wall.Move();
                if (rope != null)
                    rope.Move();
                Delay.Wait(100);
                if (C64.IsKeyPressed(Keys.L))
                    return true;
            }
        }

        private static void GetReady()
        {
            C64.Write(16, 6, "GET READY", Colors.White);
            for (int i = 0; i < 40; i++)
                Delay.Wait(100);
            C64.Write(16, 6, "         ", Colors.White);
        }
    }
}