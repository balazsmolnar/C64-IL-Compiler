using C64Lib;

// The encounter screen's assets (art/encounter_art.py makes all of them).
// The portrait is symmetric: only its left half is stored, and the jaw is
// apart from the rest so it can open. Pixels and colours are separate
// images so the pixels can be put up unseen.
[assembly: RawImage(Resource = "enc_base_px.img", Label = "EncPixels")]
[assembly: RawImage(Resource = "enc_base_col.img", Label = "EncColours")]
[assembly: RawImage(Resource = "enc_shut_px.img", Label = "EncShutPx")]
[assembly: RawImage(Resource = "enc_shut_col.img", Label = "EncShutCol")]
[assembly: RawImage(Resource = "enc_roar_px.img", Label = "EncRoarPx")]
[assembly: RawImage(Resource = "enc_roar_col.img", Label = "EncRoarCol")]
// Each guardian's own one-sided marks (full width, drawn after the mirror)
// and name plate.
[assembly: RawImage(Resource = "enc_kind0.img", Label = "EncKind0")]
[assembly: RawImage(Resource = "enc_kind1.img", Label = "EncKind1")]
[assembly: RawImage(Resource = "enc_kind2.img", Label = "EncKind2")]
[assembly: RawImage(Resource = "enc_kind3.img", Label = "EncKind3")]
[assembly: RawImage(Resource = "enc_name0.img", Label = "EncName0")]
[assembly: RawImage(Resource = "enc_name1.img", Label = "EncName1")]
[assembly: RawImage(Resource = "enc_name2.img", Label = "EncName2")]
[assembly: RawImage(Resource = "enc_name3.img", Label = "EncName3")]
// Eight hi-res frames for sprites 6-7: 0 eye, 1 eye narrowed, 2-3 the
// player's blade stroke, 4-5 the guardian's claws, 6 "MISS", 7 a spark.
[assembly: RawSprites(Order = 20, Resource = "enc_sprites.spr", Label = "Enc")]

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
             .byte 0,1,5,3,4,5,6,7,9,9,10,11,5,13,14,13
             .byte 0,1,2,3,4,5,9,7,8,9,10,2,12,13,10,15
             .byte 0,1,4,3,4,5,6,15,4,6,10,11,12,13,14,15
pal_EncEyes  .byte 2,8,7,1, 5,13,7,1, 2,10,7,1, 4,14,3,1
pal_EncTorch .byte 0,7,2,3,4,5,6,8,1,9,10,11,12,13,14,15
pal_EncFlash .byte 0,9,10,11,5,4,14,12,15,1,2,3,7,13,6,8
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
// and exit and 6 on each read of a parameter or local (4 for a static
// field), so there are few methods, with few parameters.
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
    static ulong eyeRamp_;   // its eye colours

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
                Struck(hp);
                over = hp == 0;
            }
            if (!over)
            {
                Idle(16);
                if (Lunge())
                {
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
    }

    // ---- entrance and exit ----

    // In: kind_, and bar_ = the guardian's health.
    static void Enter()
    {
        uint hp = bar_;
        tint_ = C64Address.FromLabel("pal_EncKind") + (ulong)(kind_ << 4);
        eyeRamp_ = C64Address.FromLabel("pal_EncEyes") + (ulong)(kind_ << 2);
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

        // Two eyes burn in the dark while the picture goes up unseen (every
        // cell is black on black).
        Pair(0, (Colors)C64.GetMemory(eyeRamp_, 0), EyeX, EyeY);
        C64.Screen.DrawImage(0, 0, C64Address.FromLabel("img_EncPixels"));
        C64.Screen.DrawImage(0, 0, C64Address.FromLabel("img_EncShutPx"));
        C64.Screen.MirrorCells(0, 0, 40, 21);
        bar_ = MaxHp;
        Bar(hp);
        Idle(20);
        // A shape looms out of the dark, and comes nearer...
        C64.Screen.SetCellColors(4, 0, 32, 18, 0x66, Colors.Blue);
        Idle(16);
        C64.Screen.SetCellColors(4, 0, 32, 18, 0xB6, Colors.Grey1);
        Idle(16);
        // ...and the torches catch.
        C64.Screen.BeginUpdate();
        C64.Screen.DrawImage(0, 0, C64Address.FromLabel("img_EncColours"));
        C64.Screen.DrawImage(0, 0, C64Address.FromLabel("img_EncShutCol"));
        C64.Screen.RemapCellColors(4, 0, 16, 18, tint_);
        C64.Screen.MirrorCellColors(0, 0, 40, 21);
        Marks();
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

    // This guardian's one-sided marks: cracks, a broken horn, a crown.
    static void Marks()
    {
        ulong marks = C64Address.FromLabel("img_EncKind0");
        if (kind_ == 1)
            marks = C64Address.FromLabel("img_EncKind1");
        if (kind_ == 2)
            marks = C64Address.FromLabel("img_EncKind2");
        if (kind_ == 3)
            marks = C64Address.FromLabel("img_EncKind3");
        C64.Screen.DrawImage(0, 0, marks);
    }

    // The dark closes over it again, from the middle out.
    static void Leave()
    {
        eyes_ = false;
        Hide();
        for (uint i = 0; i < 11; i++)
        {
            C64.Screen.WaitForVBlank();
            C64.Screen.SetCellColors(0, 10 - i, 40, 1, 0, Colors.Black);
            C64.Screen.SetCellColors(0, 10 + i, 40, 1, 0, Colors.Black);
        }
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
            if ((C64.Random() & 3) == 0)
            {
                C64.Screen.RemapCellColors(1, 3, 2, 2, C64Address.FromLabel("pal_EncTorch"));
                C64.Screen.RemapCellColors(37, 3, 2, 2, C64Address.FromLabel("pal_EncTorch"));
            }
            if (eyes_ && (tick_ & 7) == 0)
            {
                // dim, mid, hot, white-hot, hot, mid...
                uint step = (tick_ >> 3) & 7;
                if (step > 3)
                    step = 7 - step;
                Pair(0, (Colors)C64.GetMemory(eyeRamp_, step), EyeX, EyeY);
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
    // the eyes: the same frame twice, at the sockets. MissText: sprite 7 is
    // its black shadow. The others are two halves side by side, at double
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
        if (frame == MissText)
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
        while (bar_ != hp)          // not ">": branch_greater_unsigned8 is ">=" until the integration fix lands
        {
            bar_ = bar_ - 1;
            uint x = 100 + bar_ * 4;
            C64.Screen.DrawRectangle(x, 156, x + 3, 163, true, false, BitmapColorSource.Background);
            if (eyes_)
                Idle(1);
        }
    }

    // ---- blows ----

    // Opens or shuts the jaw. Its cells go a flat colour first (white: the
    // flash of a blow landing; black), the new pixels go up behind that,
    // then their colours, so no half-drawn jaw is ever shown.
    static void Jaw(bool open, uint flash)
    {
        C64.Screen.SetCellColors(14, 12, 12, 6, flash, (Colors)flash);
        C64.Screen.DrawImage(0, 0, open ? C64Address.FromLabel("img_EncRoarPx") : C64Address.FromLabel("img_EncShutPx"));
        C64.Screen.MirrorCells(14, 12, 12, 6);
        C64.Screen.DrawImage(0, 0, open ? C64Address.FromLabel("img_EncRoarCol") : C64Address.FromLabel("img_EncShutCol"));
        C64.Screen.RemapCellColors(14, 12, 6, 6, tint_);
        C64.Screen.MirrorCellColors(14, 12, 12, 6);
    }

    // Shakes the picture (above the HUD's split) for `frames` frames, dying
    // away; the face flashes light-for-dark and the dark flashes `flash`
    // for the first two.
    static void Shake(uint frames, Colors flash)
    {
        for (uint i = 0; i < frames; i++)
        {
            C64.Screen.WaitForVBlank();
            if (i < 2)
                C64.Screen.RemapCellColors(12, 5, 16, 7, C64Address.FromLabel("pal_EncFlash"));
            C64.Screen.SetBackgroundColor(i < 3 ? flash : Colors.Black);
            C64.Screen.SetScrollX((i & 1) == 0 ? (frames - i) >> 1 : 0u);
            Idle(1);
        }
        C64.Screen.SetScrollX(0);
    }

    // The player missed: "MISS" rises over the guardian's head.
    static void Miss()
    {
        eyes_ = false;
        for (uint i = 0; i < 24; i++)
        {
            Pair(MissText, Colors.White, 136, 86 - i);
            Idle(1);
        }
        eyes_ = true;
    }

    // The player's blade lands: stroke, flash, recoil, the bar drains; at
    // no health left the guardian dies.
    static void Struck(uint hp)
    {
        eyes_ = false;
        Pair(Stroke, Colors.Yellow, 148, 92);
        Idle(1);
        Pair(Stroke, Colors.White, 136, 104);
        Jaw(true, 0x11);
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
        else
        {
            Jaw(false, 0);
            eyes_ = true;
        }
    }

    // The guardian's answer: the eyes narrow and blaze, the jaw gapes, the
    // claws rake down across the whole view. True when they connect.
    static bool Lunge()
    {
        eyes_ = false;
        for (uint i = 0; i < 4; i++)
        {
            Pair(1, (Colors)C64.GetMemory(eyeRamp_, i), EyeX, EyeY);
            Idle(6);
        }
        Jaw(true, 0);
        bool hit = (C64.Random() & 3) != 0;
        for (uint i = 0; i < 6; i++)
        {
            Pair(Claws, hit ? (i < 3 ? Colors.White : Colors.LightRed) : Colors.Grey1, 40 + i * 20, 52 + i * 14);
            if (hit)
                Shake(2, Colors.Red);
            else
                Idle(2);
        }
        Hide();
        Jaw(false, 0);
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
