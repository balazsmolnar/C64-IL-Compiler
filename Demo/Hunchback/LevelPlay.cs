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
            Enemy enemy = new Enemy()
            {
                Sprite = C64.Sprites.Sprite2
            };
            enemy.Init(description.EnemyType);

            for (; ; )
            {
                player.Move();
                if (player.Dead)
                    return false;
                if (player.Complete)
                    return true;
                knight.Move();
                enemy.Move();
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