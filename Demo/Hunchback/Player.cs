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
        public bool Dead;
        public bool Complete;
        private Wall wall_;

        private bool highPosition_;
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

        public void Init(Wall wall)
        {
            sprite_.DataBlock = C64Address.FromLabel("spt_player_right_0");
            sprite_.MultiColor = true;
            sprite_.Visible = true;
            sprite_.Color = Colors.Green;
            sprite_.HighPosition = false;
            var tmp = sprite_.IsInCollision;
            tmp = sprite_.IsInBackgroundCollision;
            highPosition_ = false;
            jump_ = false;
            Dead = false;
            wall_ = wall;
            InitJumpOffsets();
            Y = 117;
            X = 40;

        }

        public void Move()
        {

            if (sprite_.IsInCollision)
            {
                Die();
                return;
            }

            if (sprite_.IsInBackgroundCollision)
            {
                if (highPosition_)
                    Complete = true;
                else
                    Die();
                return;
            }


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
                if (highPosition_ && X == 0)
                {
                    sprite_.HighPosition = false;
                    highPosition_ = false;
                }

                frameCounter_++;
            }
            if (C64.IsKeyPressed(Keys.D))
            {
                left_ = false;
                X += 2;
                if (!highPosition_ && X == 0)
                {
                    sprite_.HighPosition = true;
                    highPosition_ = true;
                }
                frameCounter_++;
            }
            if (frameCounter_ == 4)
                frameCounter_ = 0;

            if (Y == 117 && wall_.IsHole(x_))
            {
                Die();
            }
            SetFrame();

        }

        private void Die()
        {
            while (Y < 250)
            {
                Y++;
                Delay.Wait(2);
            }
            Dead = true;
            sprite_.Visible = false;
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
                switch (frameCounter_)
                {
                    case 0:
                        sprite_.DataBlock = C64Address.FromLabel("spt_player_left_0");
                        break;
                    case 1:
                        sprite_.DataBlock = C64Address.FromLabel("spt_player_left_3");
                        break;
                    case 2:
                        sprite_.DataBlock = C64Address.FromLabel("spt_player_left_1");
                        break;
                    case 3:
                        sprite_.DataBlock = C64Address.FromLabel("spt_player_left_2");
                        break;
                }
            }
            else
            {
                switch (frameCounter_)
                {
                    case 0:
                        sprite_.DataBlock = C64Address.FromLabel("spt_player_right_0");
                        break;
                    case 1:
                        sprite_.DataBlock = C64Address.FromLabel("spt_player_right_3");
                        break;
                    case 2:
                        sprite_.DataBlock = C64Address.FromLabel("spt_player_right_1");
                        break;
                    case 3:
                        sprite_.DataBlock = C64Address.FromLabel("spt_player_right_2");
                        break;
                }
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