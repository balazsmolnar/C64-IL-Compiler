using C64Lib;

namespace Demo
{

    class Game
    {
        PlatformEnemy[] platformEnemies_;
        Player player_;
        Platform[] platforms_;
        public void Init()
        {

            ClearScreen();
            C64.SetBorderColor(Colors.Black);
            C64.SetBackgroundColor(Colors.Black);
            InitPlatforms();

            platformEnemies_ = new PlatformEnemy[] {
            new PlatformEnemy()
            {
                Platform = platforms_[0],
                VLeft = 2,
                VRight = 2,
                SpriteData = 0,
                Sprite = C64Lib.C64.Sprites.Sprite2,
                Color = Colors.Red
            },
            new PlatformEnemy()
            {
                Platform = platforms_[2],
                VLeft = 8,
                VRight = 1,
                SpriteData = 0,
                Sprite = C64Lib.C64.Sprites.Sprite5,
                Color = C64Lib.Colors.Red,
            },
            // new PlatformEnemy()
            // {
            //     MaxX = 250,
            //     MinX = 50,
            //     Y = 60,
            //     VLeft = 4,
            //     VRight = 4,
            //     SpriteData = 9,
            //     Sprite = C64Lib.C64.Sprites.Sprite1,
            //     Color = C64Lib.Colors.Yellow,
            // },
            // new PlatformEnemy()
            // {
            //     Platform = platforms_[5],
            //     VLeft = 5,
            //     VRight = 5,
            //     SpriteData = 9,
            //     Sprite = C64Lib.C64.Sprites.Sprite3,
            //     Color = C64Lib.Colors.White,
            // }
        };

            player_ = new Player()
            {
                Sprite = C64.Sprites.Sprite4,
            };
        }

        private void InitPlatforms()
        {
            platforms_ = new Platform[] {
                    new Platform()
                    {
                        X = 10,
                        Y = 10,
                        Color = Colors.Yellow,
                        Width = 10
                    },
                    new Platform()
                    {
                        X = 1,
                        Y = 23,
                        Color = Colors.Green,
                        Width = 10
                    },
                    new Platform()
                    {
                        X = 12,
                        Y = 20,
                        Color = Colors.Green,
                        Width = 13
                    },
                    new Platform()
                    {
                        X = 23,
                        Y = 16,
                        Color = Colors.LightBlue,
                        Width = 6
                    },
                    new Platform()
                    {
                        X = 26,
                        Y = 12,
                        Color = Colors.LightBlue,
                        Width = 1
                    },
                    new Platform()
                    {
                        X = 0,
                        Y = 9,
                        Color = Colors.Yellow,
                        Width = 9
                    },
                    new Platform()
                    {
                        X = 9,
                        Y = 5,
                        Color = Colors.Red,
                        Width = 1
                    },
                    new Platform()
                    {
                        X = 15,
                        Y = 5,
                        Color = Colors.Red,
                        Width = 1
                    },
                    new Platform()
                    {
                        X = 21,
                        Y = 5,
                        Color = Colors.Red,
                        Width = 1
                    },
                    new Platform()
                    {
                        X = 27,
                        Y = 5,
                        Color = Colors.Red,
                        Width = 1
                    },
                    new Platform()
                    {
                        X = 33,
                        Y = 5,
                        Color = Colors.Red,
                        Width = 1
                    },

        };

            for (uint i = 0; i < platforms_.Length; i++)
                platforms_[i].Draw();
        }
        public void Run()
        {
            for (; ; )
            {
                Step();
                for (uint i = 0; i < 100; i++)
                    for (uint j = 0; j < 5; j++) ;
            }
        }
        public void Step()
        {
            for (int i = 0; i < platformEnemies_.Length; i++)
                platformEnemies_[i].Move();
            uint min = 255;
            for (int i = 0; i < platforms_.Length; i++)
            {
                var d = platforms_[i].DistanceToPlatform(player_.X, player_.Y);
                if (d < min)
                    min = d;
            }

            player_.Move(min);
        }

        private static void ClearScreen()
        {
            for (uint x = 0; x < 40; x++)
            {
                for (uint y = 0; y < 25; y++)
                {
                    C64.SetChar(x, y, 32);
                }
            }
        }
    }
}