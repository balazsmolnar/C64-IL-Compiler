using C64Lib;

namespace Demo
{

    class PlatformEnemy
    {

        private uint _x;
        private uint _y;
        public int SpriteData;
        public uint VRight;
        public uint VLeft;
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
                _sprite.DataBlock = SpriteData;

                _right = true;
                X = MinX;
                Y = Y;
                _sprite.Visible = true;
            }
        }

        public Platform Platform
        {
            set
            {
                MinX = value.XMinPixel + 20;
                MaxX = value.XMaxPixel - 20;
                _y = value.YPixel - 2;
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
                X += VRight;
                if (X > MaxX)
                    _right = false;
            }
            else
            {
                X -= VLeft;
                if (X < MinX)
                    _right = true;
            }
        }
    }
}