using C64Lib;

class Player
{

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
    private Sprite _sprite;

    public Sprite Sprite
    {
        set
        {
            _sprite = value;
            _sprite.DataBlock = 0;
            _sprite.Visible = true;
        }
    }

    public Colors PlayerColor
    {
        set
        {
            _sprite.Color = value;
        }
    }

    public void Move()
    {
        if (C64.IsKeyPressed(Keys.W))
            Y += 255;
        if (C64.IsKeyPressed(Keys.A))
            X += 255;
        if (C64.IsKeyPressed(Keys.S))
            Y++;
        if (C64.IsKeyPressed(Keys.D))
            X++;
    }
}