using C64Lib;

class PlatformEnemy
{

    private uint _x;
    private uint _y;
    private int _spriteData;
    public uint X
    {
        get { return _x; }
        set
        {
            _x = value;
            _sprite.X = value;
        }
    }
    public uint Y
    {
        get { return _y; }
        set
        {
            _y = value;
            _sprite.Y = value;
        }
    }
    public uint MinX { get; set; }
    public uint MaxX { get; set; }

    public bool _right;
    private Sprite _sprite;

    public Sprite Sprite
    {
        set
        {
            _sprite = value;
            _sprite.DataBlock = 1;

            _right = true;
            X = MinX;
            Y = Y;
            _sprite.Visible = true;
        }
    }

    public Colors Color
    {
        set
        {
            _sprite.Color = value;
        }
    }

    public void Move()
    {
        if (_right)
        {
            X++;
            if (X > MaxX)
                _right = false;
        }
        else
        {
            X--;
            if (X < MinX)
                _right = true;
        }

        // _spriteData = _spriteData == 1 ? 2 : 1;
        // _sprite.DataBlock = _spriteData;
    }
}