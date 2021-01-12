using C64Lib;

class Game
{
    PlatformEnemy _platformEnemy;
    Player _player;
    public void Init()
    {

        C64.SetBorderColor(Colors.Black);
        C64.SetBackgroundColor(Colors.Black);
        _platformEnemy = new PlatformEnemy()
        {
            Color = C64Lib.Colors.Red,
            MinX = 100,
            MaxX = 150,
            Y = 100,
            Sprite = C64Lib.C64.Sprites.Sprite3
        };

        _player = new Player()
        {
            Sprite = C64.Sprites.Sprite4,
            Y = 100
        };
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
        _platformEnemy.Move();
        _player.Move();
    }
}