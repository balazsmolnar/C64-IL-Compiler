using C64Lib;
namespace Hunchback
{
    class Enemy
    {

        private uint x_;
        private uint y_;
        private int frameCounter_;
        private int counter_;

        private bool highPosition;
        public uint X
        {
            get { return x_; }
            set
            {
                x_ = value;
                sprite_.X = value;
            }
        }
        public uint Y
        {
            get { return y_; }
            set
            {
                y_ = value;
                sprite_.Y = value;
            }
        }
        private Sprite sprite_;

        public Sprite Sprite
        {
            set
            {
                sprite_ = value;
            }
        }

        public void Init()
        {
            sprite_.MultiColor = true;
            sprite_.Visible = true;
            sprite_.Color = Colors.Violet;
            sprite_.HighPosition = true;
            highPosition = true;
            Y = 117;
            X = 56;
        }

        public void Move()
        {
            // counter_++;
            // if (counter_ < 32)
            //     return;
            // counter_ = 0;
            frameCounter_++;
            if (frameCounter_ == 4)
                frameCounter_ = 0;
            X -= 8;

            if (X == 0)
            {
                if (highPosition)
                {
                    highPosition = false;
                    sprite_.HighPosition = false;
                }
                else
                {
                    Init();
                }
            }
            SetFrame();
        }

        private void SetFrame()
        {

            if (frameCounter_ == 0)
                sprite_.DataBlock = C64Address.FromLabel("spt_fireball_0");
            else if (frameCounter_ == 1)
                sprite_.DataBlock = C64Address.FromLabel("spt_fireball_1");
            else if (frameCounter_ == 2)
                sprite_.DataBlock = C64Address.FromLabel("spt_fireball_2");
            else if (frameCounter_ == 3)
                sprite_.DataBlock = C64Address.FromLabel("spt_fireball_3");
        }
    }
}