using C64Lib;

class Ball
{

    const uint MINX = 20;
    const uint MAXX = 250;
    const uint MINY = 45;
    const uint MAXY = 228;

    private uint x_;
    private uint y_;
    public uint X
    {
        get { return x_; }
        set
        {
            x_ = value;
            _sprite.X = value;
        }
    }
    public uint Y
    {
        get { return y_; }
        set
        {
            y_ = value;
            _sprite.Y = value;
        }
    }
    public int VX { get; set; }
    public int VY { get; set; }
    private Sprite _sprite;

    public Sprite Sprite
    {
        set
        {
            _sprite = value;
            _sprite.DataBlock = 0;

            X = MINX;
            Y = MINY;
            _sprite.Visible = true;
        }
    }

    public Colors BallColor
    {
        set
        {
            _sprite.Color = value;
        }
    }

    public void Move()
    {
        X = (uint)(VX + X);
        Y = (uint)(VY + Y);
        if (X > MAXX)
            VX = -VX;
        if (X < MINX)
            VX = -VX;
        if (Y > MAXY)
            VY = -VY;
        if (Y < MINY)
            VY = -VY;
    }

    public static void RunBalls()
    {
        var ball1 = new Ball();
        ball1.Sprite = C64.Sprites.Sprite0;
        ball1.VX = 1;
        ball1.VY = 1;
        ball1.Y = 70;
        ball1.BallColor = Colors.Red;

        var ball2 = new Ball();
        ball2.Sprite = C64.Sprites.Sprite1;
        ball2.VX = 1;
        ball2.VY = 2;
        ball2.X = 100;
        ball2.BallColor = Colors.Yellow;

        var ball3 = new Ball();
        ball3.Sprite = C64.Sprites.Sprite2;
        ball3.VX = 2;
        ball3.VY = 1;
        ball3.X = 120;
        ball3.BallColor = Colors.Violet;

        var player = new Player();
        player.Sprite = C64.Sprites.Sprite3;
        player.PlayerColor = Colors.LightRed;

        for (; ; )
        {
            ball1.Move();
            ball2.Move();
            ball3.Move();
            player.Move();
            for (uint i = 0; i < 255; i++) ;
        }
    }


}