using C64Lib;
namespace Hunchback;

// The intro's moving character, ported from Restructure/Quasi.asm's
// Quasi_IntroMovement (the main scroll's continuous jump-bounce -- it
// writes SPRPTR0 from tbl_QuasiLeftJumpFrame every tick, i.e. the player
// is jumping/bouncing the *entire* time the three levels scroll by, not
// just for one of them) and Quasi_IntroJumpLeft (a final turn-and-jump-
// away flourish once the scroll finishes). Both write directly to the
// PLAYER sprite (SPRY0/SPRX0/SPRPTR0 = sprite 0, green, the same sprite
// normal gameplay's Player.cs controls) -- there is no separate
// decorative NPC and no wall-climb in the original's intro at all. This
// class reuses Player.cs's own sprite assets (spt_player_jump_*) and
// green color, driven by IntroScroll.cs's own simplified jump-bounce state
// rather than Player.cs's full input/collision-driven Move() (which
// assumes a real keyboard/joystick and a real wall to collide with,
// neither of which apply during the intro).
class IntroPlayer
{
    private static readonly uint[] JumpOffsets =
        { 0, 6, 11, 14, 16, 18, 19, 20, 20, 19, 18, 16, 14, 11, 6, 0 };

    // Not Player.cs's own ground level (117) -- that's calibrated for
    // normal gameplay's specific room layouts, where it lines up with a
    // ledge partway up the screen. Wall.cs's actual ground strip (the
    // jagged line at the base of every wall texture, confirmed visually
    // in VICE) sits much lower, near the bottom of the visible playfield.
    private const uint GroundY = 225;

    private uint x_;
    private uint y_;
    private uint jumpFrameCounter_;
    private bool facingLeft_;
    private Sprite sprite_;
    private uint counter_;

    public Sprite Sprite
    {
        set => sprite_ = value;
    }

    public uint X
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

    public void Init()
    {
        sprite_.MultiColor = true;
        sprite_.Visible = true;
        sprite_.Color = Colors.Green;
        facingLeft_ = false;
        jumpFrameCounter_ = 0;
        Y = GroundY;
        X = 40;
        SetFrame();
    }

    // Matches the original's Quasi_IntroJumpLeft: once the three-level
    // scroll finishes, the player turns to face left for one last
    // jump-away flourish before the static title logo takes over.
    public void TurnLeft()
    {
        facingLeft_ = true;
    }

    // IntroScroll.cs ticks Move() far more often per visible step than
    // Knight.cs's original once-per-game-frame cadence (needed to make
    // the scroll's own pacing and this character's pacing independently
    // tunable) -- throttled the same way Knight.cs throttles its own
    // Move(), just at a different rate, both to look reasonable and
    // because X/Y are uint (1 byte in this compiler -- see
    // Test/ArrayTest.cs's notes on GetStorageBytes) and would wrap well
    // before the sequence finished without a throttle capping how often
    // they actually change.
    public void Move()
    {
        counter_++;
        if (counter_ < 4)
            return;
        counter_ = 0;

        Y = GroundY - JumpOffsets[jumpFrameCounter_];
        jumpFrameCounter_++;
        if (jumpFrameCounter_ == 16)
        {
            jumpFrameCounter_ = 0;
            // X only advances once per full jump arc (not every tick) --
            // a slow, controlled drift across dozens of jump cycles over
            // the whole intro, comfortably staying inside uint's 1-byte
            // range starting from X=40.
            if (facingLeft_)
            {
                if (X > 24)
                    X -= 1;
            }
            else
            {
                X += 1;
            }
        }
        SetFrame();
    }

    private void SetFrame()
    {
        if (facingLeft_)
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
    }
}
