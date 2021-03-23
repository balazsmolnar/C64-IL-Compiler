using C64Lib;
namespace Hunchback
{
    class Knight
    {

        private uint x_;
        private uint y_;
        private int frameCounter_;
        private int counter_;
        private bool climb_;
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
            sprite_.DataBlock = C64Address.FromLabel("spt_knight_climb_0");
            sprite_.MultiColor = true;
            sprite_.Visible = true;
            sprite_.Color = Colors.Grey1;
            climb_ = true;
            Y = 197;
            X = 16;
        }

        public void Move()
        {
            counter_++;
            if (counter_ < 32)
                return;
            counter_ = 0;
            frameCounter_ = frameCounter_ == 0 ? 1 : 0;
            if (climb_)
            {
                Y -= 8;
                if (Y == 117)
                    climb_ = false;
            }
            else
            {
                X += 8;
            }
            SetFrame();
        }

        private void Die()
        {
            C64.SetBorderColor(Colors.Red);
            for (int i = 0; i < 100; i++)
                for (int j = 0; j < 100; j++) ;
            C64.SetBorderColor(Colors.Black);

            Y = 200;
            X = 20;
        }

        private void SetFrame()
        {

            if (climb_)
            {
                if (frameCounter_ == 0)
                    sprite_.DataBlock = C64Address.FromLabel("spt_knight_climb_0");
                else
                    sprite_.DataBlock = C64Address.FromLabel("spt_knight_climb_1");
            }
            else
            {
                if (frameCounter_ == 0)
                    sprite_.DataBlock = C64Address.FromLabel("spt_knight_walk_0");
                else
                    sprite_.DataBlock = C64Address.FromLabel("spt_knight_walk_1");
            }
        }
    }
}