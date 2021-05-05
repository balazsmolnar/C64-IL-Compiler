using C64Lib;
namespace Hunchback
{
    class Enemy
    {

        private uint x_;
        private uint y_;
        private int frameCounter_;

        private EnemyType enemyType_;

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
        private bool leftToRight_;
        private bool arrow_;

        public Sprite Sprite
        {
            set
            {
                sprite_ = value;
            }
        }

        public void Init(EnemyType enemyType)
        {
            enemyType_ = enemyType;
            sprite_.MultiColor = true;
            sprite_.Visible = enemyType_ != EnemyType.None;
            sprite_.Color = Colors.Violet;

            Y = 117u;
            if ((enemyType_ & EnemyType.Top) > 0)
                Y = 87u;
            leftToRight_ = (enemyType_ & EnemyType.LeftRight) > 0;
            arrow_ = (enemyType_ & EnemyType.Arrow) > 0;
            if (leftToRight_)
            {
                X = 0;
                highPosition = false;
                sprite_.HighPosition = false;
            }
            else
            {
                highPosition = true;
                sprite_.HighPosition = true;
                X = 60;
            }

            if (arrow_ && leftToRight_)
            {
                sprite_.DataBlock = C64Address.FromLabel("spt_arrow_right");
            }
            C64.Sound.PlayEffectReg2(WaveForm.Noise, 0x2C64UL, 0UL, 128, 0, false);
        }

        public void Move()
        {
            if (enemyType_ == EnemyType.None)
                return;
            frameCounter_++;
            if (frameCounter_ == 4)
                frameCounter_ = 0;

            if (leftToRight_)
            {
                X += 4;
                if (X == 0)
                {
                    highPosition = true;
                    sprite_.HighPosition = true;
                }

                if (X == 60 && highPosition)
                {
                    Init(enemyType_);
                }
            }
            else
            {
                X -= 4;

                if (X == 0)
                {
                    if (highPosition)
                    {
                        highPosition = false;
                        sprite_.HighPosition = false;
                    }
                    else
                    {
                        Init(enemyType_);
                    }
                }
            }
            SetFrame();
        }

        private void SetFrame()
        {
            if (arrow_)
                return;
            switch (frameCounter_)
            {
                case 0:
                    sprite_.DataBlock = C64Address.FromLabel("spt_fireball_0");
                    break;
                case 1:
                    sprite_.DataBlock = C64Address.FromLabel("spt_fireball_1");
                    break;
                case 2:
                    sprite_.DataBlock = C64Address.FromLabel("spt_fireball_2");
                    break;
                case 3:
                    sprite_.DataBlock = C64Address.FromLabel("spt_fireball_3");
                    break;
            }
        }
    }
}