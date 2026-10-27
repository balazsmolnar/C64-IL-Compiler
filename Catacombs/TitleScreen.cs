using C64Lib;

// The ossuary gate: one picture for the title, game over and victory screens
// (art/title_art.py).
// Sprite frames: 0-2 flame, 3 and 5 the hooded figure (lantern swinging),
// 4 eyes, 6 embers, 7 relic.
[assembly: RawImage(Resource = "title.img", Label = "Title")]
[assembly: RawSprites(Order = 10, Resource = "title.spr", Label = "Title")]

// Colour tables for Screen.RemapCellColors (table[colour] = new colour),
// written by art/title_art.py -- see its comments:
//   pal_Dim / pal_Bright     one step darker, and its exact inverse (fade in,
//                            torch flicker)
//   pal_GoDim / pal_GoBright the picture to the dim game over palette, and
//                            the step back up
//   pal_Dawn                 dim picture -> victory light (the gate's columns)
//   pal_Darken               towards black, for any screen (the game's too)
//   pal_Prompt               the prompt's colour cycle
[assembly: RawAssembly(Resource = "title_tables.asm")]

// Sprite set-up, copied straight into the VIC registers (C64.CopyMemory
// copies bytes 1..n, hence the "- 1" labels):
//   ttl_Pos   $D000-$D010  x, y of sprites 0-7, then the x high bits
//   ttl_Look  $D01B-$D02E  priority, multicolour mask, x expansion, (two
//                          read-only registers), border, background, (three
//                          unused), the two shared sprite colours, the eight
//                          sprite colours
//   ttl_Ptr   the eight sprite pointers
// Sprites: 0 the relic, 1-2 the flames, 3-4 their embers, 5 the keystone
// skull's eyes, 6 the figure on the stair.
[assembly: RawAssembly(Asm = @"
ttl_Pos     .byte 180,148, 92,113, 252,113, 90,100, 250,100, 172,91, 174,168, 0,0, 0
ttl_Look    .byte 0,$ff,0, 0,0, 0,0, 0,0,0, 7,8, 0,2,2,7,7,2,0,0
ttl_Ptr     .byte frm_Title+7,frm_Title,frm_Title+1,frm_Title+6,frm_Title+6,frm_Title+4,frm_Title+3,frm_Title+7
ttl_Eyes    .byte 2,10,8,10
ttl_Cool    .byte 2,2,8,8,7,7
ttl_PosSrc  = ttl_Pos - 1
ttl_LookSrc = ttl_Look - 1
ttl_PtrSrc  = ttl_Ptr - 1
ttl_PosDst  = $d000 - 1
ttl_LookDst = $d01b - 1
ttl_PtrDst  = Graphics_ColorMatrix + $3f8 - 1
")]

namespace Catacombs;

// The three full-screen pictures around the game, all drawn by Show(): the
// picture goes up blanked and dimmed, then light spreads over it from the
// gate outwards -- to torchlight (title), to cold stone (game over) or to
// daylight in the gate (victory). Rows 0-20 are the multicolour picture,
// rows 21-24 hi-res text below a raster split. FadeOut() takes any screen
// to black.
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
    static uint flame_;                 // the first sprite frame of the sheet
    static uint phase_;                 // flame frame 0-2
    static uint ember_;                 // embers' height above their start, counts down
    static uint flick_;                 // frames to the next torch flicker
    static uint lit_;                   // torch flicker: 1 = full light
    static uint fire_;                  // frames fire has been held; 255 = not yet released
    static uint info_;
    static uint column_;
    static uint sweep_;                 // columns either side the light spreads over
    static ulong inner_, table_;
#if SCENE
    // Scene builds: leave the screen by itself after this many video frames
    // (0 = stay). Set before Show().
    public static ulong Timer;
#endif

    // Shows a screen and returns when fire / SPACE is pressed (held for two
    // frames, after a release). `value` is the number of relics shown on the
    // game over screen.
    public static void Show(uint kind, uint value)
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
        inner_ = C64Address.FromLabel("pal_Bright");
        sweep_ = 20;
        if (kind == GameOver)
        {
            C64.Screen.RemapCellColors(0, 0, 40, Rows, C64Address.FromLabel("pal_GoDim"));
            inner_ = C64Address.FromLabel("pal_GoBright");
        }
        else
            C64.Screen.RemapCellColors(0, 0, 40, Rows, C64Address.FromLabel("pal_Dim"));
        if (kind == Victory)
        {
            inner_ = C64Address.FromLabel("pal_Dawn");
            sweep_ = 8;
        }
        C64.Screen.SetSplit(Rows, ScreenMode.Bitmap);

        // Sprites: positions, colours (and a black border and background),
        // pointers; none expanded, none switched on yet.
        C64.FillMemory(SpriteEnable, 0, 1);
        C64.FillMemory(SpriteExpandY, 0, 1);
        C64.CopyMemory(C64Address.FromLabel("ttl_PosDst"), C64Address.FromLabel("ttl_PosSrc"), 17);
        C64.CopyMemory(C64Address.FromLabel("ttl_LookDst"), C64Address.FromLabel("ttl_LookSrc"), 20);
        C64.CopyMemory(C64Address.FromLabel("ttl_PtrDst"), C64Address.FromLabel("ttl_PtrSrc"), 8);
        C64.Screen.EndUpdate();

        // Light spreads from the gate outwards. On the victory screen only
        // the gate's own 16 columns fill with daylight: the burnt-out torches
        // leave the rest of the wall dim.
        for (column_ = 0; column_ < sweep_; column_++)
        {
            C64.Screen.WaitForVBlank();
            C64.Screen.WaitForVBlank();
            C64.Screen.RemapCellColors(19 - column_, 0, 1, Rows, inner_);
            C64.Screen.RemapCellColors(20 + column_, 0, 1, Rows, inner_);
        }

        // on: title = flames, embers, figure; game over = embers, eyes;
        // victory = the figure and the relic.
        if (kind == Title)
        {
            C64.FillMemory(SpriteEnable, 0x5E, 1);
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
            C64.FillMemory(SpriteEnable, 0x41, 1);   // the torches have burnt out
            C64.Screen.RemapCellColors(0, 0, 12, 4, C64Address.FromLabel("pal_Bright"));   // the logo in full gold
            C64.Screen.RemapCellColors(28, 0, 12, 4, C64Address.FromLabel("pal_Bright"));
            C64.Screen.DrawText(7, PromptRow, "YOU ESCAPED THE CATACOMBS", 0x70);
            C64.Screen.DrawText(9, InfoRow, "ALL 3 RELICS RECOVERED", 0xF0);
        }

        lit_ = 1;
        fire_ = 255;
        flick_ = 20;
        while (Animate())
        {
        }
        FadeOut(2);
    }

    // One video frame of the screen's life. False when it is time to leave.
    static bool Animate()
    {
        C64.Screen.WaitForVBlank();
        tick_++;

        if (kind_ == Title)
        {
            // Torch light: the cells hugging a flame's bowl drop a step and come back, at random intervals.
            // First thing in the frame, ahead of the beam.
            flick_--;
            if (flick_ == 0)
            {
                flick_ = (C64.Random() & 7) + 4;
                lit_ = lit_ ^ 1;
                table_ = C64Address.FromLabel("pal_Bright");
                if (lit_ == 0)
                    table_ = C64Address.FromLabel("pal_Dim");
                C64.Screen.RemapCellColors(9, 9, 2, 2, table_);
                C64.Screen.RemapCellColors(29, 9, 2, 2, table_);
            }
        }

        if ((tick_ & 7) == 0)
        {
            // The two flames, out of step; the figure's lantern swings.
            phase_++;
            if (phase_ == 3)
                phase_ = 0;
            C64.Sprites.Sprite1.Frame = flame_ + phase_;
            C64.Sprites.Sprite2.Frame = flame_ + 2 - phase_;
            C64.Sprites.Sprite6.Frame = flame_ + 3 + ((tick_ >> 3) & 2);
            // The prompt cycles white - yellow - orange - brown; the dead
            // skull's eyes smoulder.
            if (kind_ == Title)
                C64.Screen.SetCellColors(10, PromptRow, 19, 1, C64.GetMemory(C64Address.FromLabel("pal_Prompt"), (tick_ >> 3) & 7), Colors.Black);
            C64.FillMemory(0xD02BUL, C64.GetMemory(C64Address.FromLabel("ttl_Eyes"), (tick_ >> 3) & 3), 1);   // sprite 5's colour
        }

        // Embers rise from the flames, cooling from yellow through orange to
        // red (sprites 3 and 4 share the colour), and start over.
        ember_--;
        if (ember_ > 40)
            ember_ = 40;
        C64.Sprites.Sprite3.Y = 60 + ember_;
        C64.Sprites.Sprite4.Y = 64 + ember_;
        C64.FillMemory(0xD029UL, C64.GetMemory(C64Address.FromLabel("ttl_Cool"), ember_ >> 3), 2);

        if (kind_ == Title && tick_ == 128)
        {
            info_++;
            if (info_ == 3)
                info_ = 0;
            if (info_ == 0)
                C64.Screen.DrawText(2, InfoRow, " FIND 3 RELICS, BRING THEM BACK OUT ", 0xF0);
            if (info_ == 1)
                C64.Screen.DrawText(2, InfoRow, "  WALK: W A S D     ACT: SPACE      ", 0xF0);
            if (info_ == 2)
                C64.Screen.DrawText(2, InfoRow, "  WRITTEN IN C# - B. MOLNAR 2026    ", 0xB0);
        }

#if SCENE
        if (Timer != 0)
        {
            Timer = Timer - 1;
            if (Timer == 0)
                return false;
        }
#endif
        // Fire on a joystick in port 2, or SPACE: held for two frames, and
        // only after it has been seen released (the press that ended the
        // last screen does not count).
        if (C64.IsKeyPressed(Keys.Space) || ((uint)C64.Joysticks.Joystick2.Pressed & 16) != 0)
        {
            if (fire_ == 255)
                return true;
            fire_++;
            return fire_ < 2;
        }
        fire_ = 0;
        return true;
    }

    // Any screen to black, the game's rooms included: five darkening passes
    // (the game keeps border and background black). Leaves the display blanked, with an
    // empty bitmap, no split and no sprites: whatever comes next draws
    // unseen and shows itself.
    // `frames` video frames per step: 2 between screens, 20 for the death.
    // Sprites stay up until the screen is black.
    public static void FadeOut(uint frames)
    {
        table_ = C64Address.FromLabel("pal_Darken");
        for (column_ = 0; column_ < 5; column_++)
        {
            for (tick_ = 0; tick_ < frames; tick_++)
                C64.Screen.WaitForVBlank();
            C64.Screen.RemapCellColors(0, 0, 40, 25, table_);
        }
        C64.FillMemory(SpriteEnable, 0, 1);
        C64.Screen.BeginUpdate();
        C64.Screen.SetBorderColor(Colors.Black);
        C64.Screen.SetBackgroundColor(Colors.Black);
        C64.Screen.ClearSplit();
        C64.Screen.ClearBitmap();
    }
}
