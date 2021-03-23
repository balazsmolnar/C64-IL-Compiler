using C64Lib;
namespace Hunchback
{
    class Player
    {

        private uint x_;
        private uint y_;
        private int frameCounter_;
        private int jumpFrameCounter_;
        private bool left_;
        private bool jump_;
        private int[] jumpOffsets_;
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
            sprite_.DataBlock = C64Address.FromLabel("spt_player_right_0");
            sprite_.MultiColor = true;
            sprite_.Visible = true;
            sprite_.Color = Colors.Green;
            jump_ = false;
            InitJumpOffsets();
            Y = 117;
            X = 40;
        }

        public void Move()
        {
            if (!jump_ && C64.IsKeyPressed(Keys.W))
            {
                jump_ = true;
                jumpFrameCounter_ = 0;
            }

            if (jump_)
            {
                Y = 117u - (uint)jumpOffsets_[jumpFrameCounter_];
                jumpFrameCounter_++;
                if (jumpFrameCounter_ == 16)
                {
                    jump_ = false;
                }
            }

            if (C64.IsKeyPressed(Keys.A))
            {
                left_ = true;
                X -= 2;
                frameCounter_++;
            }
            if (C64.IsKeyPressed(Keys.D))
            {
                left_ = false;
                X += 2;
                frameCounter_++;
            }
            if (frameCounter_ == 4)
                frameCounter_ = 0;
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
            if (jump_)
            {
                if (left_)
                {
                    if (jumpFrameCounter_ < 6)
                        sprite_.DataBlock = C64Address.FromLabel("spt_player_jump_left_0");
                    else if (jumpFrameCounter_ < 10)
                        sprite_.DataBlock = C64Address.FromLabel("spt_player_jump_left_1");
                    else
                        sprite_.DataBlock = C64Address.FromLabel("spt_player_jump_left_2");
                }
                else
                {
                    if (jumpFrameCounter_ < 6)
                        sprite_.DataBlock = C64Address.FromLabel("spt_player_jump_right_0");
                    else if (jumpFrameCounter_ < 10)
                        sprite_.DataBlock = C64Address.FromLabel("spt_player_jump_right_1");
                    else
                        sprite_.DataBlock = C64Address.FromLabel("spt_player_jump_right_2");
                }
                return;
            }
            if (left_)
            {
                if (frameCounter_ == 0)
                    sprite_.DataBlock = C64Address.FromLabel("spt_player_left_0");
                else if (frameCounter_ == 1)
                    sprite_.DataBlock = C64Address.FromLabel("spt_player_left_3");
                else if (frameCounter_ == 2)
                    sprite_.DataBlock = C64Address.FromLabel("spt_player_left_1");
                else if (frameCounter_ == 3)
                    sprite_.DataBlock = C64Address.FromLabel("spt_player_left_2");
            }
            else
            {
                if (frameCounter_ == 0)
                    sprite_.DataBlock = C64Address.FromLabel("spt_player_right_0");
                else if (frameCounter_ == 1)
                    sprite_.DataBlock = C64Address.FromLabel("spt_player_right_3");
                else if (frameCounter_ == 2)
                    sprite_.DataBlock = C64Address.FromLabel("spt_player_right_1");
                else if (frameCounter_ == 3)
                    sprite_.DataBlock = C64Address.FromLabel("spt_player_right_2");
            }
        }

        private void InitJumpOffsets()
        {
            if (jumpOffsets_ != null)
                return;
            // byte $00, $06, $0B, $0E, $10, $12, $13, $14
            // byte $14, $13, $12, $10, $0E, $0B, $06, $00

            jumpOffsets_ = new int[16];
            jumpOffsets_[0] = jumpOffsets_[15] = 0;
            jumpOffsets_[1] = jumpOffsets_[14] = 6;
            jumpOffsets_[2] = jumpOffsets_[13] = 11;
            jumpOffsets_[3] = jumpOffsets_[12] = 14;
            jumpOffsets_[4] = jumpOffsets_[11] = 16;
            jumpOffsets_[5] = jumpOffsets_[10] = 18;
            jumpOffsets_[6] = jumpOffsets_[9] = 19;
            jumpOffsets_[7] = jumpOffsets_[8] = 20;
        }
    }
}