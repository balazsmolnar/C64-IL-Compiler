using C64Lib;
namespace Hunchback
{
    class Player
    {

        private ulong x_;
        private uint y_;
        private int frameCounter_;
        private int jumpFrameCounter_;
        private bool left_;
        private bool jump_;
        private bool jumpFromRope_;
        public bool Dead;
        public bool OnRope;
        public Rope Rope;
        public bool Complete;
        private Wall wall_;
        private int[] jumpOffsets_;

        public ulong X
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
            var tmp = sprite_.IsInCollision;
            tmp = sprite_.IsInBackgroundCollision;
            jump_ = false;
            Dead = false;
            wall_ = wall;
            InitJumpOffsets();
            Y = 117;
            X = 40;

        }

        public void Move()
        {

            if (sprite_.IsInBackgroundCollision)
            {
                if (X > 300)
                {
                    LevelComplete();
                }
                else
                    Die();
                return;
            }

            if (OnRope)
            {
                jump_ = false;
                X = Rope.PlayerX;
                Y = 116u;
            }

            if (!jump_ && IsJump)
            {
                C64.Sound.PlayEffectReg1(WaveForm.Rectangle, 1024UL, 0x4028UL, 0, 9, true);
                if (OnRope)
                {
                    jumpFromRope_ = true;
                    OnRope = false;
                }
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
                    jumpFromRope_ = false;
                }
            }

            if (IsLeft)
            {
                left_ = true;
                if (!OnRope)
                {
                    X -= 2;
                    frameCounter_++;
                }

            }
            if (IsRight)
            {
                left_ = false;
                if (!OnRope)
                {
                    X += 2;
                    frameCounter_++;
                }
            }
            if (frameCounter_ == 4)
                frameCounter_ = 0;

            if (Y == 117 && !jumpFromRope_ && wall_.IsHole(x_))
            {
                Die();
            }
            SetFrame();

        }

        public void SetOnRope(Rope rope)
        {
            if (jumpFromRope_)
                return;
            Rope = rope;
            OnRope = true;
        }
        public void Die()
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
            if (OnRope)
            {
                if (left_)
                {
                    sprite_.DataBlock = C64Address.FromLabel("spt_player_rope_left");
                }
                else
                {
                    sprite_.DataBlock = C64Address.FromLabel("spt_player_rope_right");
                }
                return;
            }
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

        private void LevelComplete()
        {
            Complete = true;
            C64.Sound.PlayEffectReg2(WaveForm.Triangle, 0x4864UL, 0x1000UL, 0x0a, 0x00, false);
            for (int x = 0; x < 127; x++)
            {
                for (int y = 0; y < 50; y++)
                { }
            }
            C64.Sound.PlayEffectReg2(WaveForm.Triangle, 0x1264UL, 0x1000UL, 0x0a, 0x00, false);
        }
        private void InitJumpOffsets()
        {
            if (jumpOffsets_ != null)
                return;
            // byte 0x00, 0x06, 0x0B, 0x0E, 0x10, 0x12, 0x13, 0x14
            // byte 0x14, 0x13, 0x12, 0x10, 0x0E, 0x0B, 0x06, 0x00

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

        private bool IsLeft
        {
            get
            {
                return C64.IsKeyPressed(Keys.A) || (C64.Joysticks.Joystick2.Pressed & JoystickButtons.Left) == JoystickButtons.Left;
            }
        }

        private bool IsRight
        {
            get
            {
                return C64.IsKeyPressed(Keys.D) || (C64.Joysticks.Joystick2.Pressed & JoystickButtons.Right) == JoystickButtons.Right;
            }
        }

        private bool IsJump
        {
            get
            {
                return C64.IsKeyPressed(Keys.W) || (C64.Joysticks.Joystick2.Pressed & JoystickButtons.Fire) == JoystickButtons.Fire;
            }
        }
    }
}