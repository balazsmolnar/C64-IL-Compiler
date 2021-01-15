using C64Lib;

class Game
{
    PlatformEnemy platformEnemy_;
    Player player_;
    Platform platform1_, platform2_, platform3_, platform4_, platform5_, platform6_, platform7_;
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
        platform1_ = new Platform()
        {
            X = 10,
            Y = 10,
            Color = Colors.Yellow,
            Width = 10
        };
        platform1_.Draw();

        platform2_ = new Platform()
        {
            X = 1,
            Y = 23,
            Color = Colors.Green,
            Width = 10
        };
        platform2_.Draw();

        platform3_ = new Platform()
        {
            X = 12,
            Y = 20,
            Color = Colors.Green,
            Width = 3
        };
        platform3_.Draw();

        platform4_ = new Platform()
        {
            X = 17,
            Y = 20,
            Color = Colors.Green,
            Width = 3
        };
        platform4_.Draw();

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
        var d = platform1_.DistanceToPlatform(player_.X, player_.Y);
        if (d < min)
            min = d;
        d = platform2_.DistanceToPlatform(player_.X, player_.Y);
        if (d < min)
            min = d;
        d = platform3_.DistanceToPlatform(player_.X, player_.Y);
        if (d < min)
            min = d;
        d = platform4_.DistanceToPlatform(player_.X, player_.Y);
        if (d < min)
            min = d;
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