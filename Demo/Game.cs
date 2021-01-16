using C64Lib;

class Game
{
    PlatformEnemy platformEnemy_;
    Player player_;
    Platform[] platforms_;
    public void Init()
    {
        ClearScreen();
        C64.SetBorderColor(Colors.Black);
        C64.SetBackgroundColor(Colors.Black);
        InitPlatforms();
        platformEnemy_ = new PlatformEnemy()
        {
            Color = C64Lib.Colors.Red,
            MinX = 100,
            MaxX = 150,
            Y = 112,
            Sprite = C64Lib.C64.Sprites.Sprite3
        };

        player_ = new Player()
        {
            Sprite = C64.Sprites.Sprite4,
            Y = 216,
            X = 20
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
                        Width = 3
                    },
                    new Platform()
                    {
                        X = 17,
                        Y = 20,
                        Color = Colors.Green,
                        Width = 3
                    }
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
                for (uint j = 0; j < 20; j++) ;
        }
    }
    public void Step()
    {
        platformEnemy_.Move();
        uint min = 255;
        for (int i = 0; i < platforms_.Length; i++)
        {
            var d = platforms_[i].DistanceToPlatform(player_.X, player_.Y);
            if (d < min)
                min = d;
        }
        d
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