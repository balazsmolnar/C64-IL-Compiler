using C64Lib;
namespace Hunchback;

class Player
{

    private ulong x_;
    private uint y_;
    private int frameCounter_;
    private int jumpFrameCounter_;
    private bool left_;
    private bool jump_;
    private bool jumpFromRope_;
    private bool onBell_;
    public bool Dead;
    public bool OnRope;
    public Rope Rope;
    public bool Complete;
    private Wall wall_;
    private int[] jumpOffsets_;

    // On a level that ends at Esmerelda's tower: its stones are background
    // like any other, but touching them is not death (the original stops
    // testing the background 24 pixels before the tower's first column).
    public bool IsEsmereldaLevel;

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
        var backgroundCollision = sprite_.IsInBackgroundCollision;
        var onBellWall = wall_.IsRowOfBells;

        if (backgroundCollision)
        {
            // Reaching the exit-door decoration is detected via background
            // collision too (BuildBasicWall draws it for every wall type,
            // including RowOfBells), so this has to run regardless of wall
            // type -- only the *death* half of this check is RowOfBells-
            // specific.
            if (X > 300)
            {
                LevelComplete();
                return;
            }
            // Row-of-bells levels never reach the normal "any other
            // background collision is death" rule -- Restructure/Quasi.asm's
            // Quasi_CheckCollision dispatches LEVEL_ROWOFBELLS straight to
            // its own pit-fall check (further below), never falling through
            // to the default SPRCBG-is-death path other wall types use.
            // Ordinary levels are unaffected by this -- it only changes what
            // background collision means specifically for RowOfBells walls.
            // Same kind of exception onBellWall already is, scoped to
            // exactly the tower's own column span so it doesn't weaken
            // normal wall-collision death anywhere else on the level (see
            // IsEsmereldaLevel's own comment above -- X>=240 is the
            // corrected, column-formula-derived threshold).
            bool nearTower = IsEsmereldaLevel && X >= 240UL;
            if (!onBellWall && !nearTower)
            {
                Die();
                return;
            }
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
            onBell_ = false;
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

        // The row of bells, as the original plays it (Quasi.asm's
        // Quasi_RowOfBellsCollisionCheck and Quasi_CheckCollision's
        // .CheckBellPitFall). Touching a bell rope while not jumping grabs
        // it: he hangs at that rope's own position, whatever the stick
        // says, and its bell rings. A jump lets go, and carries him 32
        // pixels -- exactly one rope on -- if the stick is held all the
        // way. Between the ropes there is nothing to stand on: not
        // jumping, not hanging and not within reach of a rope (the 11
        // pixels to its right, where a jump from the last rope comes
        // down), he falls.
        if (onBellWall)
        {
            if (backgroundCollision && !jump_ && wall_.IsBellZone(x_))
            {
                if (!onBell_)
                {
                    onBell_ = true;
                    wall_.PlayBellSound(x_);
                }
                X = wall_.GetBellSnapX(x_);
            }
            else
            {
                onBell_ = false;
            }
            if (!jump_ && !onBell_ && wall_.IsBellPit(x_))
            {
                Die();
                return;
            }
        }

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
        // He cries out, then falls (Quasi_Dying).
        Sfx.Dying();
        while (Y < 244)
        {
            Y += 6;
            C64.Delay(1);
        }
        Dead = true;
        sprite_.Visible = false;
    }

    private void SetFrame()
    {
        if (OnRope || onBell_)
        {
            // No dedicated "hanging from a bell" art exists -- reuse the
            // rope-hang sprite, visually close enough (both are "gripping
            // something overhead with both arms").
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
    }

    // Forgets what the sprite has touched so far. The VIC remembers every
    // collision until it is asked: the wall that scrolled in through him,
    // the "GET READY" text over a bell rope.
    public void ClearCollisions()
    {
        var touched = sprite_.IsInCollision;
        touched = sprite_.IsInBackgroundCollision;
    }

    // A step to the left with nothing else going on: back to the start of
    // the next wall while it scrolls in. He keeps facing the way he is
    // going to go, as in the original.
    public void WalkLeft()
    {
        left_ = false;
        X -= 2;
        frameCounter_++;
        if (frameCounter_ == 4)
            frameCounter_ = 0;
        SetFrame();
    }

    private void InitJumpOffsets()
    {
        if (jumpOffsets_ != null)
            return;
        // Same data as IntroPlayer.cs's JumpOffsets (a static field there,
        // which is why it could already be a plain array literal before
        // this compiler supported array literals on instance fields).
        jumpOffsets_ = new int[] { 0, 6, 11, 14, 16, 18, 19, 20, 20, 19, 18, 16, 14, 11, 6, 0 };
    }

#if SCENE
    // Scene builds play by themselves (see Game.cs): always to the right,
    // jumping where the level needs it.
    private bool IsLeft => false;

    private bool IsRight => true;

    private bool IsJump => OnRope || onBell_ || wall_.IsJumpPoint(x_);
#else
    private bool IsLeft => Input.Left;

    private bool IsRight => Input.Right;

    private bool IsJump => Input.Fire;
#endif
}