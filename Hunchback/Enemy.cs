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
    // For a level's second enemy (see LevelPlay.Play's dual-enemy handling):
    // both enemies otherwise spawn at the exact same edge X and move in
    // lockstep, looking like a single mirrored pair rather than two
    // independent hazards. Counted down each Move() before it starts moving,
    // so it visibly lags behind the first.
    public uint MoveDelay;
    public override void Init()
    {
        sprite_.MultiColor = true;
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

        // Screen.Clear() at the top of every level wipes the whole screen
        // matrix, including the sprite data pointers living in its last 8
        // bytes -- so every sprite points at a stale/blank block until its
        // owner's Init() writes a real one. Do that here, before Visible is
        // set, the same way Player/Knight/Rope already do -- otherwise this
        // sprite is visible for a frame pointing at whatever the blank fill
        // value resolves to, rendering garbage.
        if (arrow_ && leftToRight_)
        {
            sprite_.DataBlock = C64Address.FromLabel("spt_arrow_right");
        }
        else if (!arrow_)
        {
            SetFrame();
        }
        sprite_.Visible = EnemyType != EnemyType.None;
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