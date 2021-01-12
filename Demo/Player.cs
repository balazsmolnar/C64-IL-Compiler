using C64Lib;

class Player
{

    private uint x_;
    private uint y_;

    private int data_;
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
            C64.Sprites.CommonColor1 = Colors.LightRed;
            C64.Sprites.CommonColor2 = Colors.Blue;

            _sprite = value;
            _sprite.DataBlock = 3;
            data_ = 4;
            _sprite.MultiColor = true;
            _sprite.Visible = true;
            _sprite.Color = Colors.Brown;
        }
    }

    public void Move()
    {
        if (C64.IsKeyPressed(Keys.W))
            Y--;
        if (C64.IsKeyPressed(Keys.A))
            X--;
        if (C64.IsKeyPressed(Keys.S))
        {
            Y++;
        }
        if (C64.IsKeyPressed(Keys.D))
        {
            X++;
            data_++;
            if (data_ == 6)
                data_ = 2;
            _sprite.DataBlock = data_;
        }
    }
}