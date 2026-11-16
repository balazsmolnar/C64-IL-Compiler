using C64Lib;

namespace Hunchback;

// The text screen, and a second one that is never shown: a level is built
// out of sight (tmpScreen / tmpColour in GameData.asm) and then either
// copied to the screen in one go or scrolled onto it a column at a time,
// as the original does with its scn_TemporaryScreen (Restructure/Screen.asm).
//
// Put and At address whichever of the two is being drawn on: the screen
// itself unless DrawOffScreen was called.
class Screen
{
    private static ulong chars_ = 0x0400;
    private static ulong colours_ = 0xD800;

    public static void DrawOffScreen()
    {
        chars_ = C64Address.FromLabel("tmpScreen");
        colours_ = C64Address.FromLabel("tmpColour");
    }

    public static void DrawOnScreen()
    {
        chars_ = 0x0400;
        colours_ = 0xD800;
    }

    // The address of the character / the colour at `offset` (row * 40 +
    // column) of what is being drawn on.
    public static ulong CharsAt(ulong offset) => chars_ + offset;

    public static ulong ColoursAt(ulong offset) => colours_ + offset;

    public static void Put(uint x, uint y, uint ch, Colors color)
    {
        ulong offset = C64.GetWord(C64Address.FromLabel("tbl_RowOffset"), y) + x;
        C64.FillMemory(chars_ + offset, ch, 1);
        C64.FillMemory(colours_ + offset, (uint)color, 1);
    }

    // Spaces in the given colour, all over what is being drawn on.
    public static void Clear(Colors charColor)
    {
        for (ulong page = 0; page < 0x400; page += 0x100)
        {
            // (the last page only up to the end of the screen: the eight
            // bytes after it are the sprite pointers)
            uint size = page == 0x300 ? 0xE8u : 0u;
            C64.FillMemory(chars_ + page, 32, size);
            C64.FillMemory(colours_ + page, (uint)charColor, size);
        }
    }

    // The picture built out of sight, onto the screen: everything below the
    // two status rows.
    public static void Show()
    {
        ulong chars = C64Address.FromLabel("tmpScreen");
        ulong colours = C64Address.FromLabel("tmpColour");
        for (ulong offset = 80; offset < 1000; offset += 230)
        {
            C64.CopyMemory(0x0400 + offset, chars + offset, 230);
            C64.CopyMemory(0xD800 + offset, colours + offset, 230);
        }
    }

    // One step of the scroll between two levels (Screen_LevelTransitionScroll):
    // everything below the status rows moves a column to the left, and
    // column `column` of the picture built out of sight comes in on the
    // right. The level marker's box in the bottom left corner stays.
    public static void ScrollLeft(uint column)
    {
        ulong chars = C64Address.FromLabel("tmpScreen") + column;
        ulong colours = C64Address.FromLabel("tmpColour") + column;
        // Behind the beam, so nothing tears (see C64.Screen.ScrollTextLeft):
        // it has just passed row 2.
        C64.Screen.WaitForRasterLine(76);
        C64.Screen.ScrollTextLeft(0, 2, 40, 19, chars + 80, colours + 80);
        C64.Screen.ScrollTextLeft(7, 21, 33, 4, chars + 840, colours + 840);
    }

    // One step of the intro's scroll (Screen_IntroScroll): the other way,
    // the new picture's column `column` coming in on the left.
    public static void ScrollRight(uint column)
    {
        ulong chars = C64Address.FromLabel("tmpScreen") + column;
        ulong colours = C64Address.FromLabel("tmpColour") + column;
        C64.Screen.WaitForRasterLine(76);
        C64.Screen.ScrollTextRight(0, 2, 40, 23, chars + 80, colours + 80);
    }

    // `length` screen codes from a table in GameData.asm, at (x, y) on the
    // screen, in one colour.
    public static void Text(uint x, uint y, ulong text, uint length, Colors color)
    {
        ulong offset = C64.GetWord(C64Address.FromLabel("tbl_RowOffset"), y) + x;
        C64.CopyMemory(0x0400 + offset, text, length);
        C64.FillMemory(0xD800 + offset, (uint)color, length);
    }

    // Lets `ticks` 60ths of a second pass.
    public static void Wait(uint ticks)
    {
        C64.Delay(ticks);
    }
}
