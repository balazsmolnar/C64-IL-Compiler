#if SCENE_TECHDEMO
using C64Lib;

// Colour tables for Screen.RemapCellColors: 16 bytes, table[colour] = the
// colour it becomes.
//   pal_TechMoss    a palette-swapped theme: the cold stone ramp
//                   (blue / greys) becomes brown / greens / yellow.
//   pal_TechFlicker rotates yellow -> orange -> light red -> yellow; applied
//                   every few frames it cycles a torch's glow.
[assembly: RawAssembly(Asm = @"
pal_TechMoss    .byte 0,1,2,3,4,5,9,7,8,9,10,5,13,13,14,7
pal_TechFlicker .byte 0,1,2,3,4,5,6,8,10,9,7,11,12,13,14,15
")]

namespace Catacombs;

// "dotnet build -p:Scene=TECHDEMO" (tools/catacombs-shot.ps1 -Scene TECHDEMO):
// a worked example of every piece of graphics tech in TECH.md, not part of
// the game. Art: art/techdemo_art.py.
static class TechDemo
{
    public static void Run()
    {
        C64.Screen.SetBorderColor(Colors.Black);
        C64.Screen.SetBackgroundColor(Colors.Black);
        C64.Screen.SetScreenMode(ScreenMode.MultiColor);

        // Blank the screen while the picture is built (a big image takes a
        // few frames and its pixels arrive before its colours).
        C64.Screen.BeginUpdate();

        // A full-colour 40x21-cell backdrop, then an overlay with transparent
        // cells on top of it.
        C64.Screen.DrawImage(0, 0, C64Address.FromLabel("img_TechRoom"));
        C64.Screen.DrawImage(17, 15, C64Address.FromLabel("img_TechChest"));

        // The right half of the same picture in another palette.
        C64.Screen.RemapCellColors(24, 0, 16, 21, C64Address.FromLabel("pal_TechMoss"));

        // Status panel: rows 21-24 are shown hi-res (a raster split), with a
        // hi-res picture and text drawn into them.
        C64.Screen.DrawImage(0, 21, C64Address.FromLabel("img_TechPanel"));
        C64.Screen.SetSplit(21, ScreenMode.Bitmap);
        C64.Screen.DrawText(2, 22, "HEALTH 100   GOLD 0042", 0x10);
        C64.Screen.DrawText(2, 23, "FRAME STEP", 0xF0);
        // Recolour part of the panel's frame: cells 30-37 of its top row, orange on black.
        C64.Screen.SetCellColors(30, 21, 8, 1, 0x80, Colors.Black);
        C64.Screen.EndUpdate();

        // Eight sprites showing eight different frames; the frames advance,
        // so over time all 16 + 8 are seen. Sprites 0-3: hi-res coin frames.
        // Sprites 4-7: multicolour walker frames.
        uint coin = (uint)C64Address.FromLabel("frm_TechCoin");
        uint walker = (uint)C64Address.FromLabel("frm_TechWalker");
        C64.Sprites.CommonColor1 = Colors.Black;
        C64.Sprites.CommonColor2 = Colors.Blue;
        for (uint i = 0; i < 8; i++)
        {
            Sprite s = SpriteAt(i);
            ulong x = 44 + (ulong)i * 34;
            s.X = x;
            s.Y = 196;
            s.Color = i < 4 ? Colors.Yellow : Colors.LightRed;
            if (i >= 4)
                s.MultiColor = true;
            s.Visible = true;
        }

        ulong flicker = C64Address.FromLabel("pal_TechFlicker");
        uint tick = 0;
        uint step = 0;
        for (;;)
        {
            C64.Screen.WaitForVBlank();
            tick = tick + 1;
            if (tick == 6)
            {
                tick = 0;
                step = step + 1;
                // Colour-cycle the glow around the left torch: no pixel is
                // redrawn. First thing after the vertical blank, so it is
                // done before the beam gets there.
                C64.Screen.RemapCellColors(4, 1, 12, 12, flicker);

                for (uint i = 0; i < 4; i++)
                    SpriteAt(i).Frame = coin + ((step + i * 4) & 15);
                for (uint i = 4; i < 8; i++)
                    SpriteAt(i).Frame = walker + ((step + i) & 7);

                C64.Screen.DrawText(13, 23, step.ToString(), 0x70);
            }
        }
    }

    static Sprite SpriteAt(uint i)
    {
        if (i == 0) return C64.Sprites.Sprite0;
        if (i == 1) return C64.Sprites.Sprite1;
        if (i == 2) return C64.Sprites.Sprite2;
        if (i == 3) return C64.Sprites.Sprite3;
        if (i == 4) return C64.Sprites.Sprite4;
        if (i == 5) return C64.Sprites.Sprite5;
        if (i == 6) return C64.Sprites.Sprite6;
        return C64.Sprites.Sprite7;
    }
}
#endif
