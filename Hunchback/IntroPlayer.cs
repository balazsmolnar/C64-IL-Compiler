using C64Lib;
namespace Hunchback;

// The intro's moving character, ported from Restructure/Quasi.asm's
// Quasi_IntroClimbWall/Quasi_IntroMovementRight/Quasi_IntroJumpLeft: all
// three write directly to the PLAYER sprite (SPRY0/SPRX0/SPRPTR0 = sprite
// 0, green, the same sprite normal gameplay's Player.cs controls) -- there
// is no separate decorative NPC in the original's intro. This class
// reuses Player's own sprite assets (spt_player_*) and green color to
// match, driven by IntroScroll.cs's own simplified climb/walk/jump state
// machine rather than Player.cs's full input/collision-driven Move()
// (which assumes a real keyboard/joystick and a real wall to collide
// with, neither of which apply during the intro).
class IntroPlayer
{
    private static readonly uint[] JumpOffsets =
        { 0, 6, 11, 14, 16, 18, 19, 20, 20, 19, 18, 16, 14, 11, 6, 0 };

    private uint x_;
    private uint y_;
    private int frameCounter_;
    private uint jumpFrameCounter_;
    private bool climbing_;
    private bool jumping_;
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
        climbing_ = true;
        jumping_ = false;
        Y = 197;
        X = 40;
        SetFrame();
    }

    // Matches the original's Quasi_IntroJumpLeft: the finale/tower level
    // (the last of the three intro levels here) shows the player jumping,
    // not climbing or walking -- there's no wall left to climb by then.
    public void StartJumping()
    {
        climbing_ = false;
        jumping_ = true;
        jumpFrameCounter_ = 0;
    }

    // IntroScroll.cs ticks Move() far more often per visible step than
    // Knight.cs's original once-per-game-frame cadence (needed to make
    // the scroll's own pacing and this character's pacing independently
    // tunable) -- throttled the same way Knight.cs throttles its own
    // Move(), just at a different rate, both to look reasonable and
    // because X/Y are uint (1 byte in this compiler -- see
    // Test/ArrayTest.cs's notes on GetStorageBytes) and would wrap well
    // before either phase finishes without a throttle capping how often
    // they actually change.
    public void Move()
    {
        counter_++;
        if (counter_ < 4)
            return;
        counter_ = 0;

        if (climbing_)
        {
            Y -= 2;
            if (Y <= 117)
            {
                Y = 117;
                climbing_ = false;
            }
            return;
        }

        if (jumping_)
        {
            // A full 16-frame jump arc (Player.cs's own InitJumpOffsets
            // table) repeating in place while drifting left, matching the
            // original's Quasi_IntroJumpLeft -- "happily jumping", not a
            // single hop.
            Y = 117 - JumpOffsets[jumpFrameCounter_];
            if (X > 24)
                X -= 1;
            jumpFrameCounter_++;
            if (jumpFrameCounter_ == 16)
                jumpFrameCounter_ = 0;
            SetFrame();
            return;
        }

        frameCounter_++;
        if (frameCounter_ == 4)
            frameCounter_ = 0;
        X += 1;
        SetFrame();
    }

    private void SetFrame()
    {
        if (jumping_)
        {
            if (jumpFrameCounter_ < 6)
                sprite_.DataBlock = C64Address.FromLabel("spt_player_jump_left_0");
            else if (jumpFrameCounter_ < 10)
                sprite_.DataBlock = C64Address.FromLabel("spt_player_jump_left_1");
            else
                sprite_.DataBlock = C64Address.FromLabel("spt_player_jump_left_2");
            return;
        }

        if (climbing_)
        {
            sprite_.DataBlock = C64Address.FromLabel("spt_player_left_0");
            return;
        }

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
