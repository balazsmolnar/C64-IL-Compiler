using C64Lib;

// The encounter screen's assets (art/encounter_art.py makes all of them).
// The portrait is symmetric: only its left half is stored, pixels and
// colours as two images so the pixels can be put up unseen.
[assembly: RawImage(Resource = "enc_base_px.img", Label = "EncPixels")]
[assembly: RawImage(Resource = "enc_base_col.img", Label = "EncColours")]
[assembly: RawImage(Resource = "enc_roar.img", Label = "EncRoar")]
[assembly: RawImage(Resource = "enc_shut.img", Label = "EncShut")]
[assembly: RawImage(Resource = "enc_name0.img", Label = "EncName0")]
[assembly: RawImage(Resource = "enc_name1.img", Label = "EncName1")]
[assembly: RawImage(Resource = "enc_name2.img", Label = "EncName2")]
[assembly: RawImage(Resource = "enc_name3.img", Label = "EncName3")]
// Eight hi-res frames for sprites 6-7: 0 eye, 1 eye narrowed, 2-3 the
// player's blade stroke, 4-5 the guardian's claws, 6 "MISS", 7 spare.
[assembly: RawSprites(Order = 20, Resource = "enc_sprites.spr", Label = "Enc")]

// Colour tables for Screen.RemapCellColors (table[colour] = new colour).
//   pal_EncKind  one per guardian: the bone's greys as they are, then as
//                greens, reds, violets. Each maps its results to
//                themselves, so a table can be applied again over cells it
//                already changed.
//   pal_EncTorch yellow <-> orange: the torches' flicker.
//   pal_EncFade  one step darker; applied over and over it ends in black.
[assembly: RawAssembly(Asm = @"
pal_EncKind  .byte 0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15
             .byte 0,1,2,3,4,5,9,7,8,9,10,5,13,13,14,7
             .byte 0,1,2,3,4,5,9,7,8,9,10,2,8,13,14,10
             .byte 0,1,2,3,4,5,6,7,8,9,10,4,14,13,14,3
pal_EncTorch .byte 0,1,2,3,4,5,6,8,7,9,10,11,12,13,14,15
pal_EncFade  .byte 0,15,9,14,6,11,0,8,9,0,2,6,11,5,4,12
")]

namespace Catacombs;

// The fight with a room's guardian, as a full-viewport scene (rows 0-20; the
// status panel below belongs to the HUD). Program.Combat calls Fight and
// gets the player's remaining health back.
//
// Sprites 6 and 7 are the guardian's burning eyes; for a moment at a time
// they become the player's blade stroke, the guardian's claws, or "MISS".
//
// Written for size: this compiler spends some 30 bytes on a method's entry
// and exit and 5 more per parameter, so there are few methods, with few
// parameters, and everything fits 8 bits (sprite X included).
static class Encounter
{
    const uint MaxHp = Monster.MaxHp;        // the bar is drawn for 30 points, 4 pixels each
    const uint EyeX = 149, EyeGap = 46, EyeY = 117;
    const uint Stroke = 2, Claws = 4, MissText = 6;     // frames (see the sheet above)

    static uint kind_;       // which guardian: 0-3
    static uint bar_;        // hit points the bar shows
    static uint tick_;
    static bool eyes_;       // the sprites are the eyes right now
    static ulong tint_;      // this guardian's colour table
    static bool big_;        // Pair: the frames are shown double size

    // Returns the player's health after the fight: 0 = the player died, the
    // guardian lives; anything else = the guardian is dead.
    public static uint Fight(uint mx, uint my, uint playerHp, uint playerDamage)
    {
        kind_ = Monster.FindAt(mx, my);
        bar_ = Monster.HpAt(mx, my);
        Enter();
        // One loop condition and no break: see Program.Combat's old note on
        // loops with several exits.
        bool over = false;
        while (!over)
        {
            while (!C64.IsKeyPressed(Keys.Space))
                Idle(1);
            while (C64.IsKeyPressed(Keys.Space))
                Idle(1);

            Audio.SwordHit();
            if ((C64.Random() & 7) == 0)
            {
                Miss();
            }
            else
            {
                Monster.DamageAt(mx, my, playerDamage);
                uint hp = 0;
                if (Monster.IsAliveAt(mx, my))
                    hp = Monster.HpAt(mx, my);
                else
                    Audio.MonsterDied();
                Struck(hp);
                over = hp == 0;
            }
            if (!over)
            {
                Idle(16);
                if (Lunge())
                {
                    Audio.MonsterAttack();
                    playerHp = playerHp <= Monster.MonsterDamage ? 0 : playerHp - Monster.MonsterDamage;
                    PlayerHealth(playerHp);
                    over = playerHp == 0;
                }
            }
        }
        Leave();
        return playerHp;
    }

    // The player's own health is shown by the HUD panel, not here. This is
    // the one place the encounter reports it: forward it to the HUD.
    static void PlayerHealth(uint hp)
    {
        Hud.SetHealth(hp);
    }

    // ---- entrance and exit ----

    // In: kind_, and bar_ = the guardian's health.
    static void Enter()
    {
        uint hp = bar_;
        tint_ = C64Address.FromLabel("pal_EncKind");
        for (uint i = 0; i < kind_; i++)
            tint_ = tint_ + 16;

        C64.Sprites.Sprite0.Visible = false;
        C64.Sprites.Sprite1.Visible = false;
        C64.Sprites.Sprite2.Visible = false;
        C64.Sprites.Sprite3.Visible = false;
        C64.Sprites.Sprite4.Visible = false;
        C64.Sprites.Sprite5.Visible = false;
        C64.Screen.SetBorderColor(Colors.Black);
        C64.Screen.SetBackgroundColor(Colors.Black);

        // The room closes like a pair of jaws, top and bottom rows first.
        for (uint i = 0; i < 11; i++)
        {
            C64.Screen.WaitForVBlank();
            C64.Screen.SetCellColors(0, i, 40, 1, 0, Colors.Black);
            C64.Screen.SetCellColors(0, 20 - i, 40, 1, 0, Colors.Black);
        }

        // Every cell is black on black now: the picture goes up unseen.
        C64.Screen.DrawImage(0, 0, C64Address.FromLabel("img_EncPixels"));
        C64.Screen.MirrorCells(0, 0, 40, 21);
        bar_ = MaxHp;
        Bar(hp);

        // Two eyes open in the dark...
        Pair(1, Colors.Red, EyeY);
        Idle(30);
        // ...a shape looms...
        C64.Screen.SetCellColors(0, 0, 40, 18, 0x66, Colors.Blue);
        Idle(14);
        // ...and the torches catch.
        C64.Screen.BeginUpdate();
        C64.Screen.DrawImage(0, 0, C64Address.FromLabel("img_EncColours"));
        C64.Screen.RemapCellColors(4, 0, 16, 18, tint_);
        C64.Screen.MirrorCellColors(0, 0, 40, 21);
        ulong name = C64Address.FromLabel("img_EncName0");
        if (kind_ == 1)
            name = C64Address.FromLabel("img_EncName1");
        if (kind_ == 2)
            name = C64Address.FromLabel("img_EncName2");
        if (kind_ == 3)
            name = C64Address.FromLabel("img_EncName3");
        C64.Screen.DrawImage(14, 18, name);
        C64.Screen.EndUpdate();
        eyes_ = true;
    }

    static void Leave()
    {
        eyes_ = false;
        C64.Sprites.Sprite6.Visible = false;
        C64.Sprites.Sprite7.Visible = false;
        C64.Screen.SetCellColors(0, 0, 40, 21, 0, Colors.Black);
    }

    // ---- idle life ----

    // Lets `frames` frames pass with the picture alive: the torches gutter;
    // the eyes (while the sprites are eyes) pulse and blink.
    static void Idle(uint frames)
    {
        for (uint i = 0; i < frames; i++)
        {
            C64.Screen.WaitForVBlank();
            tick_ = tick_ + 1;
            if ((C64.Random() & 7) < 2)
            {
                C64.Screen.RemapCellColors(0, 2, 4, 7, C64Address.FromLabel("pal_EncTorch"));
                C64.Screen.RemapCellColors(36, 2, 4, 7, C64Address.FromLabel("pal_EncTorch"));
            }
            if (eyes_ && (tick_ & 7) == 0)
            {
                Pair(0, (tick_ & 24) == 0 ? Colors.Orange : Colors.Yellow, EyeY);
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

    // Shows a frame pair on sprites 6 and 7 at height y. Frames 0-1 are the
    // eyes (the same frame twice, at the sockets); the others are two
    // halves side by side, at double size, across the guardian's face.
    static void Pair(uint frame, Colors color, uint y)
    {
        frame = frame + (uint)C64Address.FromLabel("frm_Enc");
        big_ = frame > (uint)C64Address.FromLabel("frm_Enc") + 1;
        Sprite s = C64.Sprites.Sprite6;
        s.Frame = frame;
        s.X = big_ ? 136u : EyeX;
        s.Y = y;
        s.Color = color;
        s.ExpandX = big_;
        s.ExpandY = big_;
        s.Visible = true;
        s = C64.Sprites.Sprite7;
        s.Frame = big_ ? frame + 1 : frame;
        s.X = big_ ? 184u : EyeX + EyeGap;
        s.Y = y;
        s.Color = color;
        s.ExpandX = big_;
        s.ExpandY = big_;
        s.Visible = true;
    }

    // ---- the readout ----

    // Drains the health bar down to hp, a point per frame once the
    // picture is showing.
    static void Bar(uint hp)
    {
        while (bar_ > hp)
        {
            bar_ = bar_ - 1;
            uint x = 100 + bar_ * 4;
            C64.Screen.DrawRectangle(x, 156, x + 3, 163, true, false, BitmapColorSource.Background);
            if (eyes_)
                Idle(1);
        }
    }

    // ---- blows ----

    static void Jaw(bool open)
    {
        C64.Screen.DrawImage(0, 0, open ? C64Address.FromLabel("img_EncRoar") : C64Address.FromLabel("img_EncShut"));
        C64.Screen.RemapCellColors(14, 12, 6, 6, tint_);
        C64.Screen.MirrorCells(14, 12, 12, 6);
    }

    // Shakes the picture for `frames` frames; the dark flashes `flash` for
    // the first three.
    static void Shake(uint frames, Colors flash)
    {
        for (uint i = 0; i < frames; i++)
        {
            C64.Screen.SetBackgroundColor(i < 3 ? flash : Colors.Black);
            C64.Screen.SetScrollX((i & 1) == 0 ? 4u : 0u);
            Idle(1);
        }
        C64.Screen.SetScrollX(0);
    }

    // "MISS" drifts up.
    static void Miss()
    {
        eyes_ = false;
        for (uint i = 0; i < 24; i++)
        {
            Pair(MissText, Colors.White, 140 - i);
            C64.Sprites.Sprite7.Visible = false;
            Idle(1);
        }
        eyes_ = true;
    }

    // The player's blade lands: stroke, flash, recoil, the bar drains; at
    // no health left the guardian dies.
    static void Struck(uint hp)
    {
        eyes_ = false;
        Pair(Stroke, Colors.White, 104);
        Jaw(true);
        Shake(8, Colors.White);
        Pair(1, Colors.Red, EyeY);
        eyes_ = true;
        Bar(hp);
        eyes_ = false;
        if (hp == 0)
        {
            // Its eyes fall out of its head like embers, and it sinks back
            // into the dark.
            for (uint i = 0; i < 30; i++)
            {
                Pair(0, i < 15 ? Colors.Orange : Colors.Red, EyeY + i * 2);
                Idle(1);
            }
            Hide();
            for (uint i = 0; i < 5; i++)
            {
                C64.Screen.RemapCellColors(0, 0, 40, 21, C64Address.FromLabel("pal_EncFade"));
                Idle(5);
            }
        }
        else
        {
            Jaw(false);
            eyes_ = true;
        }
    }

    // The guardian's answer. True when the claws connect.
    static bool Lunge()
    {
        eyes_ = false;
        Pair(1, Colors.White, EyeY);            // wind-up: the eyes narrow and blaze
        Idle(14);
        Jaw(true);
        bool hit = (C64.Random() & 3) != 0;
        if (hit)
        {
            Pair(Claws, Colors.Red, 100);
            Shake(9, Colors.Red);
        }
        else
        {
            Miss();
        }
        Jaw(false);
        eyes_ = true;
        return hit;
    }

#if SCENE_ENCOUNTER
    // Scene builds (tools/catacombs-shot.ps1 -Scene ENC...): every state of
    // the encounter, playing by itself. Never returns.
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
        bar_ = MaxHp;
            Enter();
        for (;;)
        {
#if SCENE_ENCHIT
            Idle(30);
            if (bar_ < 8)
            {
                Leave();
                bar_ = MaxHp;
            Enter();
            }
            else
            {
                Struck(bar_ - 7);
            }
#elif SCENE_ENCMISS
            Idle(30);
            Miss();
#elif SCENE_ENCATTACK
            Idle(40);
            Lunge();
#elif SCENE_ENCDEATH
            Idle(50);
            Struck(0);
            Leave();
            Idle(40);
            bar_ = MaxHp;
            Enter();
#elif SCENE_ENCENTER
            Idle(60);
            Leave();
            Idle(30);
            bar_ = MaxHp;
            Enter();
#else
            Idle(1);
#endif
        }
    }
#endif
}
