using C64Lib;
namespace Hunchback;

// The knight who climbs the wall behind the player and then marches along
// its top after him (Restructure/Enemy.asm's Enemy_MoveKnight and
// Enemy_AnimateKnight): the level's clock. He bridges every pit he crosses,
// and the sooner the level is finished the fewer steps he has taken and the
// more it scores (LevelPlay's LevelScoreMultiplier reads Counter).
class Knight : GameObject
{
    const uint WallChar = 62;

    private ulong x_;
    private uint y_;
    private uint frame_;
    private uint ticks_;
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

    public Sprite Sprite
    {
        set => sprite_ = value;
    }

    // Ticks between his steps: 48 on the first sixteen levels, 32 on the
    // next, 16 on the last (tbl_KnightMoveRates).
    public uint Rate;
    // The steps he has taken on this level.
    public uint Counter;
    // The screen column his left edge is in.
    public uint Column;
    // The wall's colour, for the bridge.
    public Colors Color;

    public override void Init()
    {
        sprite_.DataBlock = C64Address.FromLabel("spt_knight_climb_0");
        sprite_.MultiColor = true;
        sprite_.Visible = true;
        sprite_.Color = Colors.Grey1;
        Y = 197;
        X = 40;
        Column = 2;
        frame_ = 1;
        ticks_ = 0;
        Counter = 0;
    }

    public override void Move()
    {
        ticks_++;
        if (ticks_ < Rate)
            return;
        ticks_ = 0;
        if (Counter < 255)
            Counter++;

        // Every step is a change of frame; every second one moves him: up
        // the wall, then along its top.
        frame_ = frame_ ^ 1;
        bool climbing = y_ != 117;
        if (frame_ == 0)
        {
            if (climbing)
            {
                Y -= 8;
            }
            else
            {
                X += 8;
                Column++;
            }
        }
        if (climbing)
        {
            if (frame_ == 0)
                sprite_.DataBlock = C64Address.FromLabel("spt_knight_climb_0");
            else
                sprite_.DataBlock = C64Address.FromLabel("spt_knight_climb_1");
        }
        else
        {
            if (frame_ == 0)
                sprite_.DataBlock = C64Address.FromLabel("spt_knight_walk_0");
            else
                sprite_.DataBlock = C64Address.FromLabel("spt_knight_walk_1");
        }
        if (y_ == 117)
            Bridge();
    }

    // On top of the wall he walks on wall whatever was there: the three
    // columns under and ahead of him become wall at its top row, and are
    // cleared above it (a pit knight's lance, a bell rope).
    private void Bridge()
    {
        uint column = Column;
        if (column == 0 || column > 37)
            return;
        column--;
        for (uint i = 0; i < 3; i++)
        {
            C64.Screen.SetChar(column + i, 8, 32, Color);
            C64.Screen.SetChar(column + i, 9, 32, Color);
            C64.Screen.SetChar(column + i, 10, 32, Color);
            C64.Screen.SetChar(column + i, 11, WallChar, Color);
        }
    }
}
