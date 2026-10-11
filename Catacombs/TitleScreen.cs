using C64Lib;

// The ossuary gate: one picture for the title, game over and victory screens
// (art/title_art.py). Eight sprite frames: 0-2 flame, 3 hooded figure, 4 eyes,
// 5 glint, 6 embers, 7 relic.
[assembly: RawImage(Resource = "title.img", Label = "Title")]
[assembly: RawSprites(Order = 10, Resource = "title.spr", Label = "Title")]

// Colour tables for Screen.RemapCellColors (table[colour] = new colour),
// printed by art/title_art.py -- see its comment for why they work:
//   pal_Dim / pal_Bright  one step darker, and its exact inverse on the
//                         colours the picture uses (fade in, torch flicker)
//   pal_Dead              dim picture -> the game over palette (cold, blood red)
//   pal_Dawn              dim picture -> the victory palette (gold)
//   pal_Darken            towards black; applied five times it blacks out any
//                         screen, the game's rooms included
//
// Sprite set-up, copied straight into the VIC registers (C64.CopyMemory
// copies bytes 1..n, hence the "- 1" labels):
//   ttl_Pos   $D000-$D010  x, y of sprites 0-7, then the x high bits
//   ttl_Look  $D01B-$D02E  priority, multicolour mask, x expansion, (two
//                          read-only registers), border, background, (three
//                          unused), the two shared sprite colours, the eight
//                          sprite colours
//   ttl_Ptr   the eight sprite pointers
// Sprites: 0 glint, 1-2 the flames, 3-4 their embers, 5 the keystone skull's
// eyes, 6 the figure on the stair, 7 the relic it holds up.
[assembly: RawAssembly(Asm = @"
pal_Dim     .byte 0,15,2,3,4,5,0,10,9,2,8,6,11,13,14,12
pal_Bright  .byte 6,1,9,3,4,5,11,7,10,8,7,12,15,13,14,1
pal_Dead    .byte 6,1,2,3,2,5,11,15,12,11,15,12,15,13,14,1
pal_Dawn    .byte 9,1,9,3,7,5,8,7,10,8,7,10,7,13,14,1
pal_Darken  .byte 0,15,0,14,6,11,0,10,9,0,8,6,11,5,6,12
ttl_Pos     .byte 0,54, 92,110, 252,110, 92,90, 252,100, 172,93, 174,168, 174,150, 0
ttl_Look    .byte 0,$ff,0, 0,0, 0,0, 0,0,0, 7,8, 1,2,2,8,10,2,11,1
ttl_Ptr     .byte frm_Title+5,frm_Title,frm_Title+1,frm_Title+6,frm_Title+6,frm_Title+4,frm_Title+3,frm_Title+7
ttl_PosSrc  = ttl_Pos - 1
ttl_LookSrc = ttl_Look - 1
ttl_PtrSrc  = ttl_Ptr - 1
ttl_PosDst  = $d000 - 1
ttl_LookDst = $d01b - 1
ttl_PtrDst  = Graphics_ColorMatrix + $3f8 - 1
")]

namespace Catacombs;

// The three full-screen pictures around the game, all drawn by Show():
// the picture goes up blanked and dimmed, then light spreads over it from
// the gate outwards -- to torchlight (title), to cold stone (game over) or
// to gold (victory). Rows 0-20 are the multicolour picture, rows 21-24
// hi-res text below a raster split. FadeOut() takes any screen to black.
//
// State is in static fields, not locals: they compile to much shorter code.
static class TitleScreen
{
    public const uint Title = 0, GameOver = 1, Victory = 2;

    const uint Rows = 21;               // cell rows of the picture
    const uint PromptRow = 22, InfoRow = 24;
    const ulong SpriteEnable = 0xD014, SpriteExpandY = 0xD016;   // register - 1: FillMemory fills bytes 1..n

    static uint kind_;
    static uint tick_;                  // video frames, wraps at 256
    static uint flame_;                 // the flame's first sprite frame
    static uint phase_;                 // flame frame 0-2
    static uint ember_;
    static uint lit_;                   // torch flicker: 1 = full light
    static uint armed_;                 // fire has been seen released
    static uint info_;
    static uint column_;
    static ulong glint_;
    static ulong table_;
#if SCENE
    static ulong timer_;
#endif

    // Shows a screen and returns when fire / SPACE is pressed. `value` is the
    // number shown: relics recovered (game over), health left (victory).
    // Scene builds: also returns after `frames` video frames, if not 0.
    public static void Show(uint kind, uint value, ulong frames)
    {
        kind_ = kind;
        flame_ = (uint)C64Address.FromLabel("frm_Title");

        // Everything up to EndUpdate happens with the display off: the
        // picture is never seen being drawn.
        C64.Screen.BeginUpdate();
        C64.Screen.SetScreenMode(ScreenMode.MultiColor);
        C64.Screen.ClearBitmap();
        C64.Screen.SetCellColors(0, Rows, 40, 4, 0, Colors.Black);
        C64.Screen.DrawImage(0, 0, C64Address.FromLabel("img_Title"));
        C64.Screen.RemapCellColors(0, 0, 40, Rows, C64Address.FromLabel("pal_Dim"));
        C64.Screen.SetSplit(Rows, ScreenMode.Bitmap);

        // Sprites: positions, colours (and a black border and background),
        // pointers; none expanded, none switched on yet.
        C64.FillMemory(SpriteEnable, 0, 1);
        C64.FillMemory(SpriteExpandY, 0, 1);
        C64.CopyMemory(C64Address.FromLabel("ttl_PosDst"), C64Address.FromLabel("ttl_PosSrc"), 17);
        C64.CopyMemory(C64Address.FromLabel("ttl_LookDst"), C64Address.FromLabel("ttl_LookSrc"), 20);
        C64.CopyMemory(C64Address.FromLabel("ttl_PtrDst"), C64Address.FromLabel("ttl_PtrSrc"), 8);
        C64.Screen.EndUpdate();

        // Light spreads from the gate outwards.
        table_ = C64Address.FromLabel("pal_Bright");
        if (kind == GameOver)
            table_ = C64Address.FromLabel("pal_Dead");
        if (kind == Victory)
            table_ = C64Address.FromLabel("pal_Dawn");
        for (column_ = 0; column_ < 20; column_++)
        {
            C64.Screen.WaitForVBlank();
            C64.Screen.WaitForVBlank();
            C64.Screen.RemapCellColors(19 - column_, 0, 1, Rows, table_);
            C64.Screen.RemapCellColors(20 + column_, 0, 1, Rows, table_);
        }

        // on: title = glint, flames, embers, figure; game over = embers,
        // eyes; victory = the title's plus the relic.
        if (kind == Title)
        {
            C64.FillMemory(SpriteEnable, 0x5F, 1);
            C64.Screen.DrawText(10, PromptRow, "PRESS FIRE OR SPACE", 0x10);
        }
        else if (kind == GameOver)
        {
            C64.FillMemory(SpriteEnable, 0x38, 1);
            C64.Screen.DrawText(11, PromptRow, "YOU HAVE PERISHED", 0xA0);
            C64.Screen.DrawText(8, InfoRow, "RELICS RECOVERED:   OF 3", 0xC0);
            C64.Screen.DrawText(26, InfoRow, value.ToString(), 0x10);
        }
        else
        {
            C64.FillMemory(SpriteEnable, 0xDF, 1);
            C64.Screen.DrawText(7, PromptRow, "YOU ESCAPED THE CATACOMBS", 0x70);
            C64.Screen.DrawText(6, InfoRow, "ALL 3 RELICS  -  HEALTH", 0xF0);
            C64.Screen.DrawText(30, InfoRow, value.ToString(), 0x10);
        }

        lit_ = 1;
        armed_ = 0;
        glint_ = 400;
#if SCENE
        timer_ = frames;
#endif
        while (Animate())
        {
        }
        C64.FillMemory(SpriteEnable, 0, 1);
    }

    // One video frame of the screen's life. False when it is time to leave.
    static bool Animate()
    {
        C64.Screen.WaitForVBlank();
        tick_++;

        // Torch light: the cells around each flame drop a step and come back,
        // at random. First thing in the frame, ahead of the beam.
        if (kind_ != GameOver && (C64.Random() & 15) == 0)
        {
            lit_ = lit_ ^ 1;
            table_ = C64Address.FromLabel("pal_Bright");
            if (lit_ == 0)
                table_ = C64Address.FromLabel("pal_Dim");
            C64.Screen.RemapCellColors(7, 7, 6, 6, table_);
            C64.Screen.RemapCellColors(27, 7, 6, 6, table_);
        }

        if ((tick_ & 7) == 0)
        {
            // The two flames, out of step.
            phase_++;
            if (phase_ == 3)
                phase_ = 0;
            C64.Sprites.Sprite1.Frame = flame_ + phase_;
            C64.Sprites.Sprite2.Frame = flame_ + 2 - phase_;
            if (kind_ == Title)
            {
                // The prompt pulses white / yellow.
                column_ = 0x10;
                if ((tick_ & 32) != 0)
                    column_ = 0x70;
                C64.Screen.SetCellColors(10, PromptRow, 19, 1, column_, Colors.Black);
            }
        }

        // Embers climb from the flames and start over.
        ember_--;
        if (ember_ < 60)
            ember_ = 100;
        C64.Sprites.Sprite3.Y = ember_;
        C64.Sprites.Sprite4.Y = ember_ ^ 16;

        // A glint runs along the logo every five seconds.
        if (glint_ < 344)
        {
            glint_ = glint_ + 6;
            C64.Sprites.Sprite0.X = glint_;
        }
        else if (tick_ == 0)
            glint_ = 30;

        if (kind_ == Title && tick_ == 128)
        {
            info_++;
            if (info_ == 3)
                info_ = 0;
            if (info_ == 0)
                C64.Screen.DrawText(2, InfoRow, "RECOVER THE THREE RELICS AND ESCAPE ", 0xF0);
            if (info_ == 1)
                C64.Screen.DrawText(2, InfoRow, "  W A S D TO WALK - SPACE TO ACT    ", 0xF0);
            if (info_ == 2)
                C64.Screen.DrawText(2, InfoRow, " A C# GAME FOR THE C64 - B. MOLNAR  ", 0xF0);
        }

#if SCENE
        if (timer_ != 0)
        {
            timer_ = timer_ - 1;
            if (timer_ == 0)
                return false;
        }
#endif
        // Fire on a joystick in port 2, or SPACE -- once it has been seen
        // released, so the press that ended the last screen does not count.
        if (!C64.IsKeyPressed(Keys.Space) && ((uint)C64.Joysticks.Joystick2.Pressed & 16) == 0)
        {
            armed_ = 1;
            return true;
        }
        return armed_ == 0;
    }

    // Any screen to black: five darkening waves close in from both sides.
    // Leaves an empty bitmap with black colours, no split and no sprites.
    public static void FadeOut()
    {
        C64.FillMemory(SpriteEnable, 0, 1);
        table_ = C64Address.FromLabel("pal_Darken");
        for (tick_ = 0; tick_ < 28; tick_++)
        {
            C64.Screen.WaitForVBlank();
            column_ = tick_;
            for (phase_ = 0; phase_ < 5; phase_++)
            {
                if (column_ < 20)   // (it wraps past zero: that wave has not started)
                {
                    C64.Screen.RemapCellColors(column_, 0, 1, 25, table_);
                    C64.Screen.RemapCellColors(39 - column_, 0, 1, 25, table_);
                }
                column_ = column_ - 2;
            }
        }
        C64.Screen.SetBorderColor(Colors.Black);
        C64.Screen.SetBackgroundColor(Colors.Black);
        C64.Screen.ClearSplit();
        C64.Screen.ClearBitmap();
    }
}
