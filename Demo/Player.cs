using C64Lib;
namespace Demo
{

    class Player
    {

        private uint x_;
        private uint y_;

        private int data_;
        private bool jump_;
        private int jumpCounter_;
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
                C64.Sprites.CommonColor1 = Colors.LightRed;
                C64.Sprites.CommonColor2 = Colors.Blue;

                sprite_ = value;
                sprite_.DataBlock = 3;
                data_ = 4;
                sprite_.MultiColor = true;
                sprite_.Visible = true;
                sprite_.Color = Colors.Brown;
                jump_ = false;
                Y = 200;
                X = 20;
            }
        }

        public void Move(uint distanceToPlatform)
        {
            if (sprite_.IsInCollision || Y > 240)
            {
                Die();
                return;
            }

            var freefall = false;
            if (!jump_ && distanceToPlatform > 4)
                freefall = true;

            if (!jump_ && !freefall && C64.IsKeyPressed(Keys.W))
            {
                jump_ = true;
                jumpCounter_ = 0;
            }
            if (C64.IsKeyPressed(Keys.A))
            {
                X--;
                if (data_ < 5)
                    data_ = 5;
                data_++;
                if (data_ == 9)
                    data_ = 6;
                if (jump_)
                {
                    X--;
                    data_ = 7;
                }
                if (freefall)
                {
                    X--;
                    data_ = 7;
                }
                sprite_.DataBlock = data_;

            }
            if (C64.IsKeyPressed(Keys.D))
            {
                X++;
                data_++;
                if (data_ > 4)
                    data_ = 1;
                if (jump_)
                {
                    X++;
                    data_ = 2;
                }
                if (freefall)
                {
                    X++;
                    data_ = 2;
                }
                sprite_.DataBlock = data_;
            }

            if (jump_)
            {
                jumpCounter_++;

                Y -= jumpCounter_ < 10 ? 3 : 1;
                if (jumpCounter_ == 16)
                {
                    jump_ = false;
                }
            }

            if (freefall)
            {
                Y += 3;
            }
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
    }
}