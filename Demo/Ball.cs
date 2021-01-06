using C64Lib;

class Ball
{

    const uint MINX = 40;
    const uint MAXX = 250;
    const uint MINY = 40;
    const uint MAXY = 200;

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
}