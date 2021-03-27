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
            sprite_.HighPosition = true;
            highPosition = true;
            Y = 117;
            X = 60;
        }

        public bool IsInCollision => sprite_.IsInCollision;

        public void Move()
        {
            if (enemyType_ == EnemyType.None)
                return;
            frameCounter_++;
            if (frameCounter_ == 4)
                frameCounter_ = 0;
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
            SetFrame();
        }

        private void SetFrame()
        {
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