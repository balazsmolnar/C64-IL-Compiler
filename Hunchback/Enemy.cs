using C64Lib;
namespace Hunchback;

// A missile: a fireball or an arrow crossing the screen high or low, two
// pixels every tick, fired again from the edge when it has crossed
// (Restructure/Enemy.asm's Enemy_FireballHiFront and its seven siblings).
class Enemy : GameObject
{
    const uint LowY = 117, HighY = 88;      // feet level; head level, to be ducked... or jumped into
    const ulong RightEdge = 344;

    private ulong x_;
    private uint y_;

    public ulong X
    {
        get => x_;
        set
        {
            x_ = value;
            sprite_.X = value;
        }
    }
    public uint Y
    {
        get => y_;
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
        set => sprite_ = value;
    }

    public EnemyType EnemyType;
    // Ticks before it first moves: a level's second missile waits at the
    // edge, so the two do not cross together (the original's missile2Delay).
    public uint MoveDelay;

    // Fires it: at first, and again every time it has crossed.
    public override void Init()
    {
        if (EnemyType == EnemyType.None)
        {
            sprite_.Visible = false;
            return;
        }
        sprite_.MultiColor = true;
        sprite_.Color = Colors.Violet;

        arrow_ = (EnemyType & EnemyType.Arrow) > 0;
        bool top = (EnemyType & EnemyType.Top) > 0;
        leftToRight_ = (EnemyType & EnemyType.LeftRight) > 0;
        bool coin = (C64.Random() & 1) != 0;
        if ((EnemyType & EnemyType.RandomHeight) > 0)
        {
            top = coin;
            if ((EnemyType & EnemyType.RandomDirection) > 0)
                leftToRight_ = coin;        // high from the left, low from the right
        }
        else if ((EnemyType & EnemyType.RandomDirection) > 0)
        {
            leftToRight_ = coin;
        }

        Y = top ? HighY : LowY;
        X = leftToRight_ ? 0UL : RightEdge;
        SetFrame();
        sprite_.Visible = true;
        C64.Sound.PlayEffectReg2(WaveForm.Noise, 0x2C64UL, 0UL, 128, 0, false);
    }

    public override void Move()
    {
        if (EnemyType == EnemyType.None)
            return;
        if (MoveDelay > 0)
        {
            MoveDelay--;
            return;
        }

        if (leftToRight_)
        {
            X += 2UL;
            if (x_ >= RightEdge)
                Init();
        }
        else
        {
            X -= 2UL;
            if (x_ == 0UL)
                Init();
        }
        if (!arrow_)
            SetFrame();
    }

    private void SetFrame()
    {
        if (arrow_)
        {
            if (leftToRight_)
                sprite_.DataBlock = C64Address.FromLabel("spt_arrow_right");
            else
                sprite_.DataBlock = C64Address.FromLabel("spt_arrow_left");
            return;
        }
        // the fireball turns as it flies
        switch (((uint)x_ >> 2) & 3)
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
