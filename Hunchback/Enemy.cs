using C64Lib;
namespace Hunchback;

class Enemy : GameObject
{

    private ulong x_;
    private uint y_;
    private int frameCounter_;

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
    private bool leftToRight_;
    private bool arrow_;

    public Sprite Sprite
    {
        set
        {
            sprite_ = value;
        }
    }

    public EnemyType EnemyType;
    public override void Init()
    {
        sprite_.MultiColor = true;
        sprite_.Visible = EnemyType != EnemyType.None;
        sprite_.Color = Colors.Violet;

        Y = 117u;
        if ((EnemyType & EnemyType.Top) > 0)
            Y = 87u;
        leftToRight_ = (EnemyType & EnemyType.LeftRight) > 0;
        arrow_ = (EnemyType & EnemyType.Arrow) > 0;
        if (leftToRight_)
        {
            X = 0UL;
        }
        else
        {
            X = 316UL;
        }

        if (arrow_ && leftToRight_)
        {
            sprite_.DataBlock = C64Address.FromLabel("spt_arrow_right");
        }
        C64.Sound.PlayEffectReg2(WaveForm.Noise, 0x2C64UL, 0UL, 128, 0, false);
    }

    public override void Move()
    {
        if (EnemyType == EnemyType.None)
            return;
        frameCounter_++;
        if (frameCounter_ == 4)
            frameCounter_ = 0;

        if (leftToRight_)
        {
            X += 4UL;

            if (X == 316UL)
            {
                Init();
            }
        }
        else
        {
            X -= 4UL;

            if (X == 20UL)
            {
                Init();
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