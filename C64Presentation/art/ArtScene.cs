#if SCENE_ART
using C64Lib;

namespace C64Presentation;

// "-Scene ART -Slide N" (tools/presentation-shot.ps1): shows art page N and
// holds it, for reviewing the assets on the real screen. Not part of the deck.
//   0 title   1 logos + section icons   2 pipeline diagram
//   3 A1 room   4 A3 boss   5 A2 title screen   6 A5 breadbox + A6 icons
// (pages 3-5 multicolour with a split, 0-2 and 6 hi-res)
static class ArtScene
{
    public static void Run()
    {
        uint page = SceneConfig.StartSlide;
        C64.Screen.SetBorderColor(Colors.Black);
        C64.Screen.SetBackgroundColor(Colors.Black);
        C64.Screen.BeginUpdate();
        if (page >= 3 && page <= 5)
            C64.Screen.SetScreenMode(ScreenMode.MultiColor);
        else
            C64.Screen.SetScreenMode(ScreenMode.Bitmap);
        C64.Screen.ClearBitmap();
        C64.Screen.SetCellColors(0, 0, 40, 25, 0x10, Colors.Black);

        if (page == 0)
            C64.Screen.DrawImage(0, 0, C64Address.FromLabel("img_Title"));
        else if (page == 1)
            Logos();
        else if (page == 2)
            Pipeline(4);
        else if (page == 3)
            Shot(0, 0, ArtDisk.Load(DiskPicture.ShotRoom), "A1: THE ROOM, ROWS 0-20");
        else if (page == 4)
            Boss();
        else if (page == 5)
            C64.Screen.DrawImage(2, 3, ArtDisk.Load(DiskPicture.ShotTitle));   // A2: 36x18, centred, rows 3-20
        else
            Machine();
        C64.Screen.EndUpdate();
        while (true)
        {
        }
    }

    static void Logos()
    {
        C64.Screen.DrawImage(2, 2, C64Address.FromLabel("img_LogoC64"));
        C64.Screen.DrawImage(9, 2, C64Address.FromLabel("img_LogoCSharp"));
        C64.Screen.DrawImage(16, 2, C64Address.FromLabel("img_LogoDotNet"));
        C64.Screen.DrawImage(23, 2, C64Address.FromLabel("img_Chip6502"));
        C64.Screen.DrawText(2, 8, "C64", 0xF0);
        C64.Screen.DrawText(9, 8, "C#", 0xF0);
        C64.Screen.DrawText(16, 8, ".NET", 0xF0);
        C64.Screen.DrawText(23, 8, "6502", 0xF0);
        C64.Screen.DrawImage(1, 12, C64Address.FromLabel("img_SecGear"));
        C64.Screen.DrawImage(9, 12, C64Address.FromLabel("img_SecChip"));
        C64.Screen.DrawImage(17, 12, C64Address.FromLabel("img_SecMemory"));
        C64.Screen.DrawImage(25, 12, C64Address.FromLabel("img_SecJoystick"));
        C64.Screen.DrawImage(33, 12, C64Address.FromLabel("img_SecChart"));
        C64.Screen.DrawText(1, 19, "PASSES", 0xF0);
        C64.Screen.DrawText(9, 19, "6502", 0xF0);
        C64.Screen.DrawText(17, 19, "GC", 0xF0);
        C64.Screen.DrawText(25, 19, "GAME", 0xF0);
        C64.Screen.DrawText(33, 19, "SIZE", 0xF0);
    }

    // The whole diagram, top-left cell at row `top` (14 rows high).
    static void Pipeline(uint top)
    {
        ulong arrow = C64Address.FromLabel("img_PipeArrow");
        C64.Screen.DrawImage(1, top, C64Address.FromLabel("img_PipeCs"));
        C64.Screen.DrawImage(9, top + 2, arrow);
        C64.Screen.DrawImage(11, top, C64Address.FromLabel("img_PipeIl"));
        C64.Screen.DrawImage(19, top + 2, arrow);
        C64.Screen.DrawImage(21, top, C64Address.FromLabel("img_PipeCompiler"));
        C64.Screen.DrawImage(4, top + 6, C64Address.FromLabel("img_PipeElbow"));
        C64.Screen.DrawImage(1, top + 8, C64Address.FromLabel("img_PipeAsm"));
        C64.Screen.DrawImage(9, top + 10, arrow);
        C64.Screen.DrawImage(11, top + 8, C64Address.FromLabel("img_PipeTass"));
        C64.Screen.DrawImage(19, top + 10, arrow);
        C64.Screen.DrawImage(21, top + 8, C64Address.FromLabel("img_PipePrg"));
        C64.Screen.DrawImage(29, top + 10, arrow);
        C64.Screen.DrawImage(31, top + 8, C64Address.FromLabel("img_PipeC64"));
        C64.Screen.DrawText(3, top + 4, "C#", 0x1B);
        C64.Screen.DrawText(14, top + 4, "IL", 0x1B);
        C64.Screen.DrawText(26, top + 4, "COMPILER", 0x1B);
        C64.Screen.DrawText(4, top + 12, "ASM", 0x1B);
        C64.Screen.DrawText(12, top + 12, "64TASS", 0x1B);
        C64.Screen.DrawText(23, top + 12, ".PRG", 0x1B);
        C64.Screen.DrawText(33, top + 12, "C64", 0x1B);
    }

    // A3: rows 0-14 multicolour, hi-res from row 15.
    static void Boss()
    {
        C64.Screen.DrawImage(0, 0, ArtDisk.Load(DiskPicture.ShotBoss));
        C64.Screen.SetSplit(15, ScreenMode.Bitmap);
        C64.Screen.DrawText(12, 16, "THE BONE WARDEN", 0x30);   // its name, in hi-res
        C64.Screen.DrawText(2, 19, "A3: ROWS 0-14, SPLIT AT 15", 0xF0);
    }

    // A5 at the storyboard's spot (cells 1-18, rows 7-14), A6 below it:
    // pixels only, so the cells' colours (cyan on black) are set first.
    static void Machine()
    {
        C64.Screen.DrawImage(1, 7, C64Address.FromLabel("img_Breadbox"));
        C64.Screen.DrawText(21, 8, "64 KB RAM", 0x10);
        C64.Screen.DrawText(21, 10, "1 MHZ 6510 CPU", 0x10);
        C64.Screen.SetCellColors(0, 17, 40, 4, 0x30, Colors.Black);
        C64.Screen.DrawImage(2, 18, C64Address.FromLabel("img_IconInt8"));
        C64.Screen.DrawImage(8, 18, C64Address.FromLabel("img_IconStack"));
        C64.Screen.DrawImage(14, 18, C64Address.FromLabel("img_IconObject"));
        C64.Screen.DrawImage(20, 18, C64Address.FromLabel("img_IconGc"));
        C64.Screen.DrawImage(26, 18, C64Address.FromLabel("img_IconString"));
        C64.Screen.DrawImage(32, 18, C64Address.FromLabel("img_IconBitmap"));
        C64.Screen.DrawText(1, 22, "8-BIT STACK OBJECTS  GC   TEXT BITMAP", 0xF0);
    }

    // A full-width screenshot over rows 0-20, a hi-res caption band below.
    static void Shot(uint col, uint row, ulong image, string caption)
    {
        C64.Screen.DrawImage(col, row, image);
        C64.Screen.SetSplit(21, ScreenMode.Bitmap);
        C64.Screen.DrawText(2, 22, caption, 0xF0);
        C64.Screen.DrawText(2, 23, "CATACOMBS, WRITTEN IN C#", 0x30);
    }
}
#endif
