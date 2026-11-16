using C64Lib;

// The encounter screen's assets (art/encounter_art.py makes all of them).
// The portrait is symmetric: only its left half is stored, pixels and
// colours as two images so the pixels can be put up unseen.
[assembly: RawImage(Resource = "enc_base_px.img", Label = "EncPixels")]
[assembly: RawImage(Resource = "enc_base_col.img", Label = "EncColours")]
// Each guardian's name plate and its own one-sided marks (full width, drawn
// after the mirror).
[assembly: RawImage(Resource = "enc_kind0.img", Label = "EncKind0")]
[assembly: RawImage(Resource = "enc_kind1.img", Label = "EncKind1")]
[assembly: RawImage(Resource = "enc_kind2.img", Label = "EncKind2")]
[assembly: RawImage(Resource = "enc_kind3.img", Label = "EncKind3")]
// Hi-res sprite frames, in two sheets (the first is the last that fits in
// the 64 frames that cost no memory). Enc: 0 eye, 1 eye narrowed, 2-3 the
// player's blade stroke, 4-5 the guardian's claws, 6 "CRIT!", 7 a spark.
// EncArm: 0 the player's gauntlet and forearm, 1-2 the sword and the axe it
// holds, 3-4 the claws mirrored, 5-7 the key that steps out of a blow's way
// with an arrow (D right, A left, S back).
[assembly: RawSprites(Order = 20, Resource = "enc_sprites.spr", Label = "Enc")]
[assembly: RawSprites(Order = 21, Resource = "enc_arm.spr", Label = "EncArm")]

// Colour tables (art/src/enc_tables.txt lists what encounter_art.py made):
//   pal_EncKind  one per guardian for Screen.RemapCellColors: the bone as
//                it is, rotten green, the iron run red, cold and violet.
//                Each maps its results to themselves, so it can be applied
//                again over cells it already changed.
//   pal_EncEyes  per guardian, four eye colours from dim to white-hot.
//   pal_EncTorch the flame's three colours rotated (white, yellow, orange).
//   pal_EncFlash light and dark swapped; applied twice it is undone.
//   pal_EncFade  one step darker; applied over and over it ends in black.
[assembly: RawAssembly(Asm = @"
pal_EncKind  .byte 0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15
             .byte 0,1,5,3,4,5,9,13,9,9,10,11,12,13,5,15
             .byte 0,1,2,3,4,5,9,7,8,9,10,2,12,13,10,15
             .byte 0,1,4,3,4,5,6,15,4,6,10,11,12,13,4,15
pal_EncEyes  .byte 2,8,7,1, 5,13,7,1, 2,10,7,1, 4,14,3,1
pal_EncTorch .byte 0,7,2,3,4,5,6,8,1,9,10,11,12,13,14,15
pal_EncFlash .byte 0,9,10,11,5,4,14,12,15,1,2,3,7,13,6,8
pal_EncFade  .byte 0,15,9,14,6,11,0,8,9,0,2,0,11,5,4,12
")]

namespace Catacombs;

// The fight with a room's guardian, as a full-viewport scene (rows 0-20; the
// status panel below belongs to the HUD). Program.Combat calls Fight and
// gets the player's remaining health back.
//
// A duel in real time, decided by the player's timing and nothing else:
//
//   The blade   Hold SPACE and the weapon arm rises; let go and it strikes.
//               Let go too early and the guardian turns the blow aside. Let
//               go in the few frames the steel flashes white (a ping sounds)
//               and the blow lands, lightly: the guardian is on its guard.
//               Hold on too long and the arm sinks.
//   The claws   The guardian strikes when it chooses: it growls, its eyes
//               narrow and heat up from dim to white, and its claws rise
//               where they will come from. Step away from them -- D from
//               claws on the left, A from claws on the right, S back from
//               claws overhead -- but not too soon: a step lasts a third of
//               a second, and after one the player stands flat-footed (the
//               weapon arm dark) for as long again. The screen says all of
//               this: the key to press is shown with an arrow on the side
//               to step to, grey while it is too soon and flashing from the
//               moment a step will carry the player clear.
//   The opening A guardian that missed reels, its eyes guttering: now ANY
//               blow lands with the weapon's whole weight, and one let go on
//               the flash is a critical hit for twice that.
//
// The four guardians strike faster and harder in turn; from the third on
// they also strike from overhead, and the last one feints. All of them
// speed up when nearly dead.
//
// Sprites: 0-1 the player's weapon arm; 2-3 and 4-5 the guardian's claws
// and the key prompt (whichever pair the claws are not using), and the
// spark of a parried blow; 6-7 its burning eyes, which for a moment at a
// time become the player's blade stroke or "CRIT!".
//
// Written for size: this compiler spends some 30 bytes on a method's entry
// and exit and 6 on each read of a parameter or local (4 for a static
// field), so there are few methods, with few parameters.
static class Encounter
{
    const uint MaxHp = Monster.MaxHp;        // the bar is drawn for 30 points, 4 pixels each
    const uint EyeX = 149, EyeGap = 46, EyeY = 117;
    // frames (see the sheets above)
    const uint Stroke = 2, Claws = 4, CritText = 6, Spark = 7;
    const uint ClawsMirrored = 3;       // in EncArm; the hand is 0, + weapon_ = what it holds
    const uint KeyPrompt = 4;           // in EncArm, + side_: D, A, S
    const uint StepNow = 16;            // a step from this many frames before the blow on is in time

    // The blade: how long SPACE has been held, in frames.
    const uint Feeble = 12;                  // let go before this: no blow at all
    const uint GlintLo = 26, GlintHi = 33;   // the flash: let go now
    const uint Spent = 44;                   // the arm sinks back
    const uint ArmX = 36, ArmY = 150;        // the weapon arm at rest: sprite x - 256, sprite y

    // A step aside: StepFrames from the key press until the next one may
    // start; the player is out of the way while more than StepOver remain.
    const uint StepFrames = 32, StepOver = 16;

    const uint Watch = 0, WindUp = 1, Open = 2;     // what the guardian is doing

    static uint kind_;       // which guardian: 0-3
    static uint bar_;        // hit points the bar shows
    static uint tick_;
    static bool eyes_;       // the sprites are the eyes right now, and Idle keeps them burning
    static ulong tint_;      // this guardian's colour table
    static ulong eyeRamp_;   // its eye colours

    static uint hp_;         // the guardian's health
    static uint player_;     // the player's
    static uint power_;      // damage of a blow on the flash; a blow into an opening does twice that
    static uint weapon_;     // 0 fist, 1 sword, 2 axe
    static uint arm_;        // the weapon arm's sprites: 3, or 2 with nothing in the hand
    static uint charge_;     // frames SPACE has been held
    static uint step_;       // frames left of the step aside (and its recovery)
    static uint stepDir_;    // where to: 1 right, 2 left, 3 back -- the claws' side_ it avoids
    static uint lastKeys_;
    static uint phase_;      // Watch, WindUp, Open
    static uint clock_;      // frames left of it
    static uint side_;       // the blow being wound up: 1 from the left, 2 from the right, 3 from overhead
    static bool feint_;      // ...and it will change sides at the last moment
    static bool done_;
    static uint[] parts_;    // Sprites.PlaceGroup data: the weapon arm (0-7), a pair of claws (8-15)
#if SCENE
    static uint bot_;
#endif

    // Returns the player's health after the fight: 0 = the player died, the
    // guardian lives; anything else = the guardian is dead.
    public static uint Fight(uint mx, uint my, uint playerHp, uint playerDamage)
    {
        kind_ = Monster.FindAt(mx, my);
        hp_ = Monster.HpAt(mx, my);
        bar_ = hp_;
        player_ = playerHp;
        weapon_ = playerDamage / 10;            // 6, 12, 20: as the HUD picks its icon
        power_ = 2 + weapon_;                   // 2, 3, 4 of the guardian's 30
        Enter();
        Duel();
        if (hp_ == 0)
            Monster.DamageAt(mx, my, MaxHp);
        Leave();
        return player_;
    }

    // ---- the duel ----

    // What the player is pressing: 1 D (step right), 2 A (left), 3 S (back),
    // plus 8 for SPACE.
    static uint Input()
    {
        uint k = 0;
#if SCENE
        // Scene builds fight by themselves (no keypresses): a fair player,
        // who lets go too early one time in four and misses one step in
        // four.
        if (charge_ < bot_)
            k = 8;
        else
            bot_ = (C64.Random() & 3) == 0 ? 18u : GlintLo + 2;
#if !SCENE_ENCLOSE
        if (phase_ == WindUp && clock_ == 9 && (C64.Random() & 3) != 0)
            k = k | side_;
#endif
#else
        if (C64.IsKeyPressed(Keys.D))
            k = 1;
        if (C64.IsKeyPressed(Keys.A))
            k = 2;
        if (C64.IsKeyPressed(Keys.S))
            k = 3;
        if (C64.IsKeyPressed(Keys.Space))
            k = k | 8;
#endif
        return k;
    }

    // Until one of the two is dead. (One loop condition and no break: this
    // compiler's loops have one exit.)
    static void Duel()
    {
        lastKeys_ = 3;      // a key still held from walking in is not a step
        done_ = false;
        phase_ = Watch;
        clock_ = 70;
        while (!done_)
        {
            Idle(1);
            uint keys = Input();

            // a step aside: on a fresh key press, once the last one is over
            uint dir = keys & 3;
            if (step_ != 0)
            {
                step_ = step_ - 1;
            }
            else if (dir != 0 && (lastKeys_ & 3) == 0)
            {
                stepDir_ = dir;
                step_ = StepFrames;
                Audio.Fx(AudioIds.FxDodge);
            }
            lastKeys_ = keys;

            // the blade
            if (keys >= 8)
            {
                charge_ = charge_ + 1;
                if (charge_ == GlintLo)
                    Audio.Fx(AudioIds.FxReady);
                if (charge_ == Spent)
                    charge_ = 0;
            }
            else if (charge_ != 0)
            {
                Strike();
            }
            Arm();

            if (!done_)
                Guardian();
        }
    }

    // The weapon arm (sprites 0-1): it rises with the charge and leans with
    // a step aside; the steel is dull, then bright, then flashes white for
    // the frames that count, then is spent.
    static void Arm()
    {
        uint c = charge_;
        uint color = (uint)Colors.Grey2;
        if (c >= Feeble)
            color = (uint)Colors.Grey3;
        if (c >= GlintLo)
            color = (c & 2) == 0 ? (uint)Colors.White : (uint)Colors.Yellow;
        if (c > GlintHi)
            color = (uint)Colors.Orange;
        if (step_ != 0 && step_ <= StepOver && c < GlintLo)
            color = (uint)Colors.Grey1;         // flat-footed: no step yet
        parts_[3] = color;
        parts_[7] = color;
        uint x = ArmX;
        uint y = ArmY - c;
        if (c > GlintHi)
            x = x + (c & 2);            // held too long: the arm shakes
        if (step_ > StepOver)
        {
            if (stepDir_ == 1)
                x = x + 12;
            if (stepDir_ == 2)
                x = x - 8;
            if (stepDir_ == 3)
                y = y + 14;
        }
        C64.Sprites.PlaceGroup(0, 2, 256UL + x, y, parts_, 0);
    }

    // Shows / hides sprites: those in `keep` stay as they are, those in `on`
    // are shown.
    static void Show(uint keep, uint on)
    {
        C64.Sprites.VisibleMask = (C64.Sprites.VisibleMask & keep) | on;
    }

    // SPACE was let go with the arm raised.
    static void Strike()
    {
        uint c = charge_;
        charge_ = 0;
        if (c < Feeble)
            return;
        bool keen = c >= GlintLo && c <= GlintHi;

        // the arm flies at the skull
        for (uint i = 1; i < 4; i++)
        {
            C64.Sprites.PlaceGroup(0, 2, 292UL - i * 30, ArmY - c + i * 6, parts_, 0);
            Idle(1);
        }
        Show(0xFC, 0);

        if (phase_ != Open && !keen)
        {
            // Turned aside: a spark off the bone, and nothing else.
            Audio.Fx(AudioIds.FxClang);
            parts_[10] = (uint)C64Address.FromLabel("frm_Enc") + Spark;
            parts_[11] = (uint)Colors.Yellow;
            C64.Sprites.PlaceGroup(2, 1, 160, 109, parts_, 8);
            Show(0xFF, 4);
            Shake(6, Colors.Black);
            Show(0xFB, 0);
        }
        else
        {
            // On its guard it only takes a scratch. Into an opening the
            // blow has the weapon's whole weight (4, 6, 8), and twice
            // that on the flash.
            uint damage = power_;
            if (phase_ == Open)
            {
                damage = damage + damage;
                if (keen)
                    damage = damage + damage;
                Rest();
            }
            else
            {
                keen = false;           // a hit, no more
            }
            Audio.Fx(keen ? AudioIds.FxCrit : AudioIds.FxSword);
            hp_ = damage >= hp_ ? 0 : hp_ - damage;
            if (hp_ == 0)
            {
                Audio.MonsterDied();
                done_ = true;
                Show(0xC0, 0);          // only the eyes are left, to fall
            }
            Struck(hp_);
            if (keen && hp_ != 0)
                Crit();
            eyes_ = phase_ != WindUp;   // (a blow it is winding up still comes)
        }
        if (hp_ != 0)
            Show(0xFF, arm_);
    }

    // The guardian's turn of the frame.
    static void Guardian()
    {
        clock_ = clock_ - 1;
        if (phase_ == WindUp)
        {
            if (feint_ && clock_ == 15)
            {
                side_ = 3 - side_;      // the other claw after all
                Show(0xC3, 0);
            }
            // The eyes narrow and heat up, the claws rise: dim at first,
            // yellow for frames 15-8 (from here on the side is certain, and
            // a step aside lasts long enough), white for the last eight.
            uint heat = clock_ >> 3;
            heat = heat > 3 ? 0u : 3 - heat;
            uint color = C64.GetMemory(eyeRamp_, heat);
            Pair(1, (Colors)color, EyeX, EyeY);
            Claw(color, 0, 46 - (heat << 2));
            // The key to press, on the side to step to (below, for a step
            // back): grey while a step would be over too soon, flashing
            // once it will not.
            color = (uint)Colors.Grey1;
            if (clock_ < StepNow)
                color = (clock_ & 2) == 0 ? (uint)Colors.White : (uint)Colors.LightGreen;
            parts_[10] = (uint)C64Address.FromLabel("frm_EncArm") + KeyPrompt + side_;
            parts_[11] = color;
            if (side_ == 2)
            {
                C64.Sprites.PlaceGroup(2, 1, 68, 112, parts_, 8);
                Show(0xFF, 0x04);
            }
            else
            {
                C64.Sprites.PlaceGroup(4, 1, side_ == 1 ? 244u : 160u, side_ == 1 ? 112u : 150u, parts_, 8);
                Show(0xFF, 0x10);
            }
            if (clock_ == 0)
                Lunge();
        }
        else if (phase_ == Open)
        {
            // reeling: the eyes gutter
            if ((clock_ & 3) == 0)
            {
                if ((clock_ & 4) == 0)
                    Hide();
                else
                    Pair(0, (Colors)C64.GetMemory(eyeRamp_, 0), EyeX, EyeY + 2);
            }
            if (clock_ == 0)
                Rest();
        }
        else if (clock_ == 0)
        {
            // It makes up its mind: which claw (from the third guardian on
            // sometimes both, and the last one sometimes changes sides).
            uint r = C64.Random();
            side_ = (r & 1) + 1;
            feint_ = kind_ == 3 && (r & 8) != 0;
            if (kind_ >= 2 && (r & 6) == 0)
            {
                side_ = 3;
                feint_ = false;
            }
            phase_ = WindUp;
            eyes_ = false;
            clock_ = 36 - (kind_ << 2) - kind_;     // 36, 31, 26, 21 frames of warning
            Audio.Fx(AudioIds.FxGrowl);
        }
    }

    // Back to watching, for a while it does not tell: shorter for the later
    // guardians, and half as long once it is nearly dead.
    static void Rest()
    {
        phase_ = Watch;
        eyes_ = true;
        uint t = 48 + (C64.Random() & 63) - (kind_ << 3);
        if (hp_ < 11)
            t = t >> 1;
        clock_ = t;
    }

    // The claws of the blow side_: sprites 2-3 from the left or 4-5 from
    // the right, x pixels in from the viewport's edge; from overhead sprites
    // 2-3 above the skull, and x is how far down they have come.
    static void Claw(uint color, uint x, uint y)
    {
        uint frame = (uint)C64Address.FromLabel("frm_Enc") + Claws;
        uint mirrored = (uint)C64Address.FromLabel("frm_EncArm") + ClawsMirrored;
        parts_[11] = color;
        parts_[15] = color;
        if (side_ != 2)
        {
            parts_[10] = frame;
            parts_[14] = frame + 1;
            if (side_ == 3)
                C64.Sprites.PlaceGroup(2, 2, 136, y + (x >> 1), parts_, 8);
            else
                C64.Sprites.PlaceGroup(2, 2, 24 + x, y, parts_, 8);
            Show(0xFF, 0x0C);
        }
        else
        {
            parts_[10] = mirrored;
            parts_[14] = mirrored + 1;
            C64.Sprites.PlaceGroup(4, 2, 248 - x, y, parts_, 8);
            Show(0xFF, 0x30);
        }
    }

    // The claws come down. If the player is out of their way they rake the
    // air and the guardian is left open; if not, they connect.
    static void Lunge()
    {
        bool hit = step_ <= StepOver || stepDir_ != side_;
        Show(0xC3, 0);          // the key prompt goes
        if (hit)
            Audio.Fx(AudioIds.FxHurt);
        for (uint i = 0; i < 6; i++)
        {
            // A blow rakes from the corner across the brow and horns and out
            // the other side (white on the face is the player's blade); a
            // dodged one swipes past in grey, beside the skull.
            if (hit)
            {
                Claw((uint)Colors.White, i * 36, 30 + i * 8);
                Shake(2, i < 2 ? Colors.Red : Colors.Black);
            }
            else
            {
                Claw((uint)Colors.Grey1, i * 12, 50 + i * 8);
                Idle(2);
            }
        }
        Show(0xC3, 0);
        if (hit)
        {
            uint damage = 8 + kind_ + kind_;
            player_ = player_ <= damage ? 0 : player_ - damage;
            Hud.SetHealth(player_);
            charge_ = 0;                // the arm is knocked down
            done_ = player_ == 0;
            Rest();
        }
        else
        {
            phase_ = Open;
            clock_ = 66 - (kind_ << 3);
        }
    }

    // ---- entrance and exit ----

    // In: kind_, weapon_, and bar_ = the guardian's health.
    static void Enter()
    {
        uint hp = bar_;
        tint_ = C64Address.FromLabel("pal_EncKind") + (ulong)(kind_ << 4);
        eyeRamp_ = C64Address.FromLabel("pal_EncEyes") + (ulong)(kind_ << 2);
        // Sprites 0-5 are the weapon arm and the claws here, all at double
        // size (Leave gives them back as they were).
        C64.Sprites.VisibleMask = 0;
        C64.Sprites.ExpandXMask = 0x3F;
        C64.Sprites.ExpandYMask = 0x3F;
        uint sheet = (uint)C64Address.FromLabel("frm_EncArm");
        parts_ = new uint[16];
        parts_[2] = sheet + weapon_;            // (bare-handed: not shown)
        parts_[4] = 12;                         // the gauntlet, under the weapon's grip
        parts_[5] = 38;
        parts_[6] = sheet;
        parts_[12] = 48;
        arm_ = weapon_ == 0 ? 2u : 3u;
        C64.Screen.SetBorderColor(Colors.Black);
        C64.Screen.SetBackgroundColor(Colors.Black);

        // The room closes like a pair of jaws, top and bottom rows first.
        for (uint i = 0; i < 11; i++)
        {
            C64.Screen.WaitForVBlank();
            C64.Screen.SetCellColors(0, i, 40, 1, 0, Colors.Black);
            C64.Screen.SetCellColors(0, 20 - i, 40, 1, 0, Colors.Black);
        }

        // Two eyes burn in the dark while the picture goes up unseen (every
        // cell is black on black).
        C64.Screen.DrawImage(0, 0, C64Address.FromLabel("img_EncPixels"));
        Pair(1, (Colors)C64.GetMemory(eyeRamp_, 0), EyeX, EyeY);
        C64.Screen.MirrorCells(0, 0, 40, 21);
        bar_ = MaxHp;
        Bar(hp);
        Idle(2);
        // A shape looms out of the dark...
        C64.Screen.SetCellColors(4, 0, 32, 18, 0xB6, Colors.Blue);
        Idle(30);
        // ...and the torches catch: the colours sweep in, left half then
        // right (no screen blank: the HUD panel below must not blink).
        C64.Screen.DrawImage(0, 0, C64Address.FromLabel("img_EncColours"));
        C64.Screen.RemapCellColors(4, 0, 16, 18, tint_);
        C64.Screen.MirrorCellColors(0, 0, 40, 21);
        // this guardian's name and its own marks (a broken horn, a third
        // eye weeping blood), drawn over both halves
        ulong marks = C64Address.FromLabel("img_EncKind0");
        if (kind_ == 1)
            marks = C64Address.FromLabel("img_EncKind1");
        if (kind_ == 2)
            marks = C64Address.FromLabel("img_EncKind2");
        if (kind_ == 3)
            marks = C64Address.FromLabel("img_EncKind3");
        C64.Screen.DrawImage(0, 0, marks);
        eyes_ = true;
        // the player's weapon comes up
        charge_ = 0;
        step_ = 0;
        Arm();
        Show(0xFF, arm_);
    }

    // The dark closes over it again.
    static void Leave()
    {
        eyes_ = false;
        Hide();
        C64.Sprites.VisibleMask = 0;
        C64.Sprites.ExpandXMask = 0;
        C64.Sprites.ExpandYMask = 0;
        C64.Screen.SetCellColors(0, 0, 40, 21, 0, Colors.Black);
    }

    // ---- idle life ----

    // Lets `frames` frames pass with the picture alive: the flames shift;
    // the eyes (while the sprites are eyes) pulse and blink.
    static void Idle(uint frames)
    {
        for (uint i = 0; i < frames; i++)
        {
            C64.Screen.WaitForVBlank();
            tick_ = tick_ + 1;
            if ((C64.Random() & 3) == 0 && bar_ != 0)       // (still while it dies)
            {
                C64.Screen.RemapCellColors(1, 3, 2, 2, C64Address.FromLabel("pal_EncTorch"));
                C64.Screen.RemapCellColors(37, 3, 2, 2, C64Address.FromLabel("pal_EncTorch"));
            }
            if (eyes_ && (tick_ & 7) == 0)
            {
                // mid, hot, white-hot, hot: the fire breathes
                uint step = (tick_ >> 3) & 3;
                Pair(0, (Colors)C64.GetMemory(eyeRamp_, step == 0 ? 1u : step == 3 ? 2u : step), EyeX, EyeY);
                if (tick_ > 247)                       // a blink, every five seconds
                    Hide();
            }
        }
    }

    static void Hide()
    {
        C64.Sprites.Sprite6.Visible = false;
        C64.Sprites.Sprite7.Visible = false;
    }

    // Shows a frame on sprites 6 and 7, sprite 6 at (x, y). Frames 0-1 are
    // the eyes: the same frame twice, at the sockets. CritText: sprite 7
    // shows it again in black, 2 pixels down and right, as its shadow (it is
    // behind sprite 6). The others are two halves side by side, at double
    // size.
    static void Pair(uint frame, Colors color, uint x, uint y)
    {
        uint sheet = (uint)C64Address.FromLabel("frm_Enc");
        bool big = frame > 1;
        Sprite s = C64.Sprites.Sprite6;
        s.Frame = sheet + frame;
        s.X = x;
        s.Y = y;
        s.Color = color;
        s.ExpandX = big;
        s.ExpandY = big;
        s.Visible = true;
        s = C64.Sprites.Sprite7;
        if (frame == CritText)
        {
            x = x + 2;
            y = y + 2;
            color = Colors.Black;
        }
        else
        {
            x = x + (big ? 48u : EyeGap);
            if (big)
                frame = frame + 1;
        }
        s.Frame = sheet + frame;
        s.X = x;
        s.Y = y;
        s.Color = color;
        s.ExpandX = big;
        s.ExpandY = big;
        s.Visible = true;
    }

    // ---- the readout ----

    // Drains the health bar down to hp, a point per frame once the
    // picture is showing.
    static void Bar(uint hp)
    {
        while (bar_ != hp)
        {
            bar_ = bar_ - 1;
            uint x = 100 + bar_ * 4;
            C64.Screen.DrawRectangle(x, 156, x + 3, 163, true, false, BitmapColorSource.Background);
            if (eyes_)
                Idle(1);
        }
    }

    // ---- blows ----

    // Shakes the picture for `frames` frames, dying away; the dark flashes
    // `flash` for the first three, and on a white flash (a blow landing)
    // the face flashes light-for-dark for two.
    static void Shake(uint frames, Colors flash)
    {
        for (uint i = 0; i < frames; i++)
        {
            C64.Screen.WaitForVBlank();
            if (i < 2 && flash == Colors.White)
                C64.Screen.RemapCellColors(12, 5, 16, 7, C64Address.FromLabel("pal_EncFlash"));
            C64.Screen.SetBackgroundColor(i < 3 ? flash : Colors.Black);
            C64.Screen.SetScrollX((i & 1) == 0 ? (frames - i) >> 1 : 0u);
            Idle(1);
        }
        C64.Screen.SetScrollX(0);
        C64.Screen.SetBackgroundColor(Colors.Black);
    }

    // A critical hit: "CRIT!", with a drop shadow, rises up over the
    // guardian's head into the dark.
    static void Crit()
    {
        for (uint i = 0; i < 24; i++)
        {
            Pair(CritText, (i & 4) == 0 ? Colors.White : Colors.Yellow, 160, 70 - i);
            Idle(1);
        }
        Hide();
    }

    // The player's blade lands: stroke, flash, recoil, the bar drains; at
    // no health left the guardian dies.
    static void Struck(uint hp)
    {
        eyes_ = false;
        Pair(Stroke, Colors.Yellow, 148, 92);
        Idle(1);
        Pair(Stroke, Colors.White, 136, 104);
        Shake(10, Colors.White);
        Pair(Stroke, Colors.Grey2, 124, 116);
        Idle(2);
        Pair(1, (Colors)C64.GetMemory(eyeRamp_, 0), EyeX, EyeY);
        Bar(hp);
        if (hp == 0)
        {
            // Its eyes fall out of its head like embers, and it sinks back
            // into the dark.
            for (uint i = 0; i < 40; i++)
            {
                Pair(0, (Colors)C64.GetMemory(eyeRamp_, i < 20 ? 1u : 0u), EyeX, EyeY + i * 2);
                Idle(1);
            }
            Hide();
            for (uint i = 0; i < 5; i++)
            {
                C64.Screen.RemapCellColors(0, 0, 40, 21, C64Address.FromLabel("pal_EncFade"));
                Idle(8);
            }
        }
    }

#if SCENE_ENCOUNTER
    // Scene builds (tools/catacombs-shot.ps1 -Scene ENC, ENC1, ENC2, ENC3:
    // the four guardians): duel after duel, fought by Input's own player.
    // ENCLOSE: against the first guardian by a player who never steps
    // aside, and dies. Never returns.
    public static void Scene()
    {
#if SCENE_ENC1
        kind_ = 1;
#elif SCENE_ENC2
        kind_ = 2;
#elif SCENE_ENC3
        kind_ = 3;
#else
        kind_ = 0;
#endif
        for (;;)
        {
            hp_ = MaxHp;
            bar_ = MaxHp;
            player_ = 100;
            Hud.SetHealth(100);
            weapon_ = kind_ == 3 ? 1u : kind_;     // fist, sword, axe, sword
            power_ = 2 + weapon_;
            Enter();
            Duel();
            Leave();
        }
    }
#endif
}
