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
[assembly: RawSprites(Order = 20, Resource = "enc_sprites.spr", Label = "Enc")]

// Colour tables for Screen.RemapCellColors (table[colour] = new colour).
//   pal_EncKind  the three other guardians: the bone's greys become greens,
//                reds, violets. Each maps its results to themselves, so a
//                table can be applied again over cells it already changed.
//   pal_EncTorch yellow <-> orange: the torches' flicker.
//   pal_EncFade  one step darker; applied over and over it ends in black.
[assembly: RawAssembly(Asm = @"
pal_EncKind  .byte 0,1,2,3,4,5,9,7,8,9,10,5,13,13,14,7
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
static class Encounter
{
    const uint MaxHp = Monster.MaxHp;        // the bar is drawn for 30 points, 4 pixels each
    const ulong BarX = 100;
    const ulong EyeLeft = 149, EyeRight = 195;
    const uint EyeY = 117;

    static uint kind_;       // which guardian: 0-3
    static uint bar_;        // hit points the bar shows
    static uint tick_;
    static bool eyes_;       // the sprites are the eyes right now

    // Returns the player's health after the fight: 0 = the player died, the
    // guardian lives; anything else = the guardian is dead.
    public static uint Fight(uint mx, uint my, uint playerHp, uint playerDamage)
    {
        kind_ = Monster.FindAt(mx, my);
        Enter(Monster.HpAt(mx, my));
        // One loop condition and no break: see Program.Combat's old note on
        // loops with several exits.
        bool over = false;
        while (!over)
        {
            while (!C64.IsKeyPressed(Keys.Space))
                Tick();
            while (C64.IsKeyPressed(Keys.Space))
                Tick();

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
                if (hp == 0)
                {
                    Die();
                    over = true;
                }
            }
            if (!over)
            {
                Idle(16);
                if (Lunge())
                {
                    playerHp = playerHp <= Monster.MonsterDamage ? 0 : playerHp - Monster.MonsterDamage;
                    PlayerHealth(playerHp);
                    if (playerHp == 0)
                        over = true;
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

    static void Enter(uint hp)
    {
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
        Bar(hp, false);

        // Two eyes open in the dark...
        tick_ = 0;
        Eyes(1, Colors.Red);
        Wait(30);
        // ...a shape looms...
        C64.Screen.SetCellColors(0, 0, 40, 18, 0x66, Colors.Blue);
        Eyes(0, Colors.Orange);
        Wait(14);
        // ...and the torches catch.
        C64.Screen.BeginUpdate();
        C64.Screen.DrawImage(0, 0, C64Address.FromLabel("img_EncColours"));
        Tint(4, 0, 16, 18);
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
        Eyes(0, Colors.Yellow);
    }

    static void Leave()
    {
        eyes_ = false;
        C64.Sprites.Sprite6.Visible = false;
        C64.Sprites.Sprite7.Visible = false;
        C64.Screen.SetScrollX(0);
        C64.Screen.SetCellColors(0, 0, 40, 21, 0, Colors.Black);
    }

    // Gives a freshly drawn part of the left half this guardian's colours.
    static void Tint(uint column, uint row, uint width, uint height)
    {
        if (kind_ != 0)
            C64.Screen.RemapCellColors(column, row, width, height, C64Address.FromLabel("pal_EncKind") + (ulong)(kind_ - 1) * 16);
    }

    // ---- idle life ----

    // One frame of the picture being alive: the torches gutter, the eyes
    // pulse, glance aside and blink.
    static void Tick()
    {
        C64.Screen.WaitForVBlank();
        tick_ = tick_ + 1;
        if ((C64.Random() & 7) < 2)
        {
            ulong torch = C64Address.FromLabel("pal_EncTorch");
            C64.Screen.RemapCellColors(0, 2, 4, 7, torch);
            C64.Screen.RemapCellColors(36, 2, 4, 7, torch);
        }
        if (eyes_ && (tick_ & 7) == 0)
        {
            uint phase = tick_ >> 3;
            if ((phase & 31) == 31)
            {
                C64.Sprites.Sprite6.Visible = false;       // a blink
                C64.Sprites.Sprite7.Visible = false;
            }
            else
            {
                Eyes(0, (phase & 3) == 0 ? Colors.Orange : Colors.Yellow);
            }
        }
    }

    static void Idle(uint frames)
    {
        for (uint i = 0; i < frames; i++)
            Tick();
    }

    static void Wait(uint frames)
    {
        for (uint i = 0; i < frames; i++)
            C64.Screen.WaitForVBlank();
    }

    static void Eyes(uint frame, Colors color)
    {
        eyes_ = true;
        Pair(frame, frame, EyeLeft, EyeRight, EyeY, color, false);
    }

    // Shows two frames of the sheet on sprites 6 and 7.
    static void Pair(uint left, uint right, ulong leftX, ulong rightX, uint y, Colors color, bool big)
    {
        uint sheet = (uint)C64Address.FromLabel("frm_Enc");
        Sprite s = C64.Sprites.Sprite6;
        s.Frame = sheet + left;
        s.X = leftX;
        s.Y = y;
        s.Color = color;
        s.ExpandX = big;
        s.ExpandY = big;
        s.Visible = true;
        s = C64.Sprites.Sprite7;
        s.Frame = sheet + right;
        s.X = rightX;
        s.Y = y;
        s.Color = color;
        s.ExpandX = big;
        s.ExpandY = big;
        s.Visible = true;
    }

    // ---- the readout ----

    // Drains the health bar down to hp, a point per frame when `slowly`.
    static void Bar(uint hp, bool slowly)
    {
        while (bar_ > hp)
        {
            bar_ = bar_ - 1;
            ulong x = BarX + (ulong)bar_ * 4;
            C64.Screen.DrawRectangle(x, 156, x + 3, 163, true, false, BitmapColorSource.Background);
            if (slowly)
                Tick();
        }
    }

    // ---- blows ----

    static void Jaw(bool open)
    {
        C64.Screen.DrawImage(0, 0, open ? C64Address.FromLabel("img_EncRoar") : C64Address.FromLabel("img_EncShut"));
        Tint(14, 12, 6, 6);
        C64.Screen.MirrorCells(14, 12, 12, 6);
    }

    // The words "MISS" drift up and fade.
    static void Miss()
    {
        eyes_ = false;
        for (uint i = 0; i < 24; i++)
        {
            Pair(6, 6, 160, 160, 150 - i, i < 16 ? Colors.White : Colors.Grey2, false);
            C64.Sprites.Sprite7.Visible = false;
            Tick();
        }
        Eyes(0, Colors.Yellow);
    }

    // The player's blade lands: stroke, flash, recoil, the bar drains.
    static void Struck(uint hp)
    {
        eyes_ = false;
        Pair(2, 3, 136, 184, 104, Colors.White, true);
        C64.Screen.SetBackgroundColor(Colors.White);
        Wait(2);
        C64.Screen.SetBackgroundColor(Colors.Black);
        Jaw(true);
        for (uint i = 0; i < 8; i++)
        {
            C64.Screen.WaitForVBlank();
            C64.Screen.SetScrollX((i & 1) == 0 ? 3u : 0u);
            if (i == 3)
                Pair(2, 3, 136, 184, 104, Colors.Yellow, true);
        }
        C64.Screen.SetScrollX(0);
        Eyes(1, Colors.Red);
        Bar(hp, true);
        Jaw(false);
        Eyes(0, Colors.Yellow);
    }

    // The guardian's answer. True when the claws connect.
    static bool Lunge()
    {
        Eyes(1, Colors.White);                 // wind-up: the eyes narrow and blaze
        Wait(14);
        Jaw(true);
        bool hit = (C64.Random() & 3) != 0;
        if (hit)
        {
            eyes_ = false;
            for (uint i = 0; i < 9; i++)
            {
                Pair(4, 5, 124, 172, 92 + i * 5, i < 5 ? Colors.White : Colors.Red, true);
                C64.Screen.WaitForVBlank();
                C64.Screen.SetScrollX((i & 1) == 0 ? 5u : 0u);
                C64.Screen.SetBackgroundColor(i < 3 ? Colors.Red : Colors.Black);
            }
            C64.Screen.SetScrollX(0);
        }
        else
        {
            Idle(6);
            Miss();
        }
        Jaw(false);
        Eyes(0, Colors.Yellow);
        return hit;
    }

    // The guardian dies: a last roar, the eyes fall out of its head like
    // embers, and it sinks back into the dark.
    static void Die()
    {
        Jaw(true);
        eyes_ = false;
        for (uint i = 0; i < 30; i++)
        {
            Pair(0, 0, EyeLeft - i, EyeRight + i, EyeY + i * 2, i < 10 ? Colors.Yellow : i < 20 ? Colors.Orange : Colors.Red, false);
            C64.Screen.WaitForVBlank();
            C64.Screen.SetScrollX(i < 12 ? (i & 3) : 0);
        }
        C64.Sprites.Sprite6.Visible = false;
        C64.Sprites.Sprite7.Visible = false;
        for (uint i = 0; i < 5; i++)
        {
            C64.Screen.RemapCellColors(0, 0, 40, 21, C64Address.FromLabel("pal_EncFade"));
            Wait(5);
        }
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
        Enter(MaxHp);
        for (;;)
        {
#if SCENE_ENCHIT
            Idle(30);
            if (bar_ < 7)
                Enter(MaxHp);
            else
                Struck(bar_ - 7);
#elif SCENE_ENCMISS
            Idle(30);
            Miss();
#elif SCENE_ENCATTACK
            Idle(40);
            Lunge();
#elif SCENE_ENCDEATH
            Idle(50);
            Struck(0);
            Die();
            Leave();
            Wait(40);
            Enter(MaxHp);
#elif SCENE_ENCENTER
            Idle(60);
            Leave();
            Wait(30);
            Enter(MaxHp);
#else
            Tick();
#endif
        }
    }
#endif
}
