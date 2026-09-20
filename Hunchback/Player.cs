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
    private uint bellGrabCooldown_;
    public bool Dead;
    public bool OnRope;
    public Rope Rope;
    public bool Complete;
    private Wall wall_;
    private int[] jumpOffsets_;

    // Set once by LevelPlay.Play() when levelNumber == 47 -- Esmeralda's
    // Tower's decorative tile block (LevelPlay.cs's DrawEsmereldaTower)
    // sits at columns 32-39 (screen chars), rows 2-10 -- squarely inside
    // the player's own jump-arc height (Y ranges ~97 at a jump's peak to
    // 117 standing, both within that span) near the level's right side.
    // Without this exception, brushing the tower's own (very real,
    // hardware-collidable) character graphics triggered the same "touched
    // a wall" death as any other background collision.
    //
    // X range recheck: the first cut of this fix used X>=276 as the
    // tower's left edge, guessed from the column count rather than derived
    // -- still died walking into the tower from the left, confirmed live.
    // The correct column-to-X conversion is already established elsewhere
    // in this file's own sibling (Wall.cs's "quasiX = bellColumn*8 - 10",
    // verified there against all 4 tbl_BellRopeXOffset entries with zero
    // rounding error). Applying it to the tower's own leftmost column (32):
    // 32*8-10 = 246, not 280 -- the earlier guess was 30px too far right,
    // leaving a real gap (X 246-276) where touching the tower still killed
    // the player. 240 below gives a few pixels of margin for the sprite's
    // own hitbox extending slightly left of its X coordinate.
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
            if (onBell_)
            {
                // A jump that doesn't carry far enough horizontally can
                // still be overlapping the same bell's rope once it lands
                // (background collision is still true there), which would
                // otherwise re-grab it immediately -- looks like "bouncing
                // back" from the bar you just tried to leave. Block
                // re-grabbing anything for a short window after release so
                // there's always a real, visible gap in the grab.
                bellGrabCooldown_ = 10;
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

        // Grabbing a bell overrides normal left/right walking -- the player
        // hangs at a fixed X until they jump (handled above, via
        // onBell_ = false). Matches Quasi_RowOfBellsCollisionCheck exactly:
        // staying grabbed only needs continued background collision (the
        // snap below keeps the sprite touching the bell, which keeps
        // collision true, which keeps it grabbed -- self-sustaining); the
        // grab *zone* is only checked to start a new grab, not to remain in
        // one, so grazing slightly outside it mid-hang doesn't drop you.
        if (bellGrabCooldown_ > 0)
            bellGrabCooldown_--;

        if (onBellWall && !jump_ && backgroundCollision)
        {
            if (!onBell_ && bellGrabCooldown_ == 0 && wall_.IsRowOfBellsGrabZone(x_))
            {
                onBell_ = true;
                wall_.PlayBellSound(x_);
            }
            if (onBell_)
                X = wall_.GetBellSnapX(x_);
        }
        else
        {
            onBell_ = false;

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
        }
        if (frameCounter_ == 4)
            frameCounter_ = 0;

        if (Y == 117 && !jumpFromRope_ && wall_.IsHole(x_))
        {
            Die();
        }
        // The original also kills the player for walking (not jumping)
        // through the gaps between bell ropes without being grabbed onto
        // one (Quasi_CheckCollision's .CheckBellPitFall) -- disabled again
        // for now. The architecture matches what I can verify from the
        // disassembly (position + onBell_ state, independent of live
        // background collision), but the exact 11px "near enough to a rope"
        // windows leave only a razor-thin, hard-to-verify-blind timing
        // window to jump the ~14px approach gap, and it's made the level
        // unplayable twice in a row now. Not worth guessing a third time --
        // needs to be tuned against actual play. The gap is real (visible,
        // via BuildRopePit) and the grab mechanic works; only this
        // additional harsh fall-through-the-gap hazard is off.
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
        // Same data as IntroPlayer.cs's JumpOffsets (a static field there,
        // which is why it could already be a plain array literal before
        // this compiler supported array literals on instance fields).
        jumpOffsets_ = new int[] { 0, 6, 11, 14, 16, 18, 19, 20, 20, 19, 18, 16, 14, 11, 6, 0 };
    }

    private bool IsLeft =>
        C64.IsKeyPressed(Keys.A) || (C64.Joysticks.Joystick2.Pressed & JoystickButtons.Left) == JoystickButtons.Left;

    private bool IsRight =>
        C64.IsKeyPressed(Keys.D) || (C64.Joysticks.Joystick2.Pressed & JoystickButtons.Right) == JoystickButtons.Right;

    private bool IsJump =>
        C64.IsKeyPressed(Keys.W) || (C64.Joysticks.Joystick2.Pressed & JoystickButtons.Fire) == JoystickButtons.Fire;
}