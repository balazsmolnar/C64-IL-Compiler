#if SCENE_DISK
using C64Lib;

namespace C64Presentation;

// "-Scene DISK" (tools/presentation-shot.ps1): a hi-res slide stays up while
// a picture loads from the disk (-Slide 0, 1, 2: A1, A2, A3), then it is
// drawn with the load time underneath.
// Shows what the presenter sees, and measures the drive. Not part of the deck.
static class DiskScene
{
    const uint Caption = 0xF0;    // light grey on black
    const uint Accent = 0x30;     // cyan on black
    const uint ShotRows = 21;

    static DiskPicture Picture => SceneConfig.StartSlide switch
    {
        1 => DiskPicture.ShotTitle,
        2 => DiskPicture.ShotBoss,
        _ => DiskPicture.ShotRoom,
    };

    public static void Run()
    {
        C64.Screen.SetBorderColor(Colors.Black);
        C64.Screen.SetBackgroundColor(Colors.Black);
        C64.Screen.BeginUpdate();
        C64.Screen.SetScreenMode(ScreenMode.Bitmap);
        C64.Screen.ClearBitmap();
        C64.Screen.SetCellColors(0, 0, 40, 25, Caption, Colors.Black);
        C64.Screen.DrawText(2, 10, "THE PREVIOUS SLIDE", Accent);
        C64.Screen.DrawText(2, 12, "STAYS UP WHILE THE NEXT", Caption);
        C64.Screen.DrawText(2, 13, "PICTURE LOADS FROM DISK", Caption);
        C64.Screen.EndUpdate();

        TimeOfDay.Restart();
        ulong picture = ArtDisk.Load(Picture);
        uint tenths = TimeOfDay.ElapsedTenths;

        C64.Screen.BeginUpdate();
        C64.Screen.SetScreenMode(ScreenMode.MultiColor);
        C64.Screen.ClearBitmap();
        C64.Screen.DrawImage(0, 0, picture);
        C64.Screen.SetSplit(ShotRows, ScreenMode.Bitmap);
        C64.Screen.DrawText(2, 22, $"LOADED FROM DISK IN {tenths / 10}.{tenths % 10} S", Caption);
        C64.Screen.DrawText(2, 23, "CATACOMBS, WRITTEN IN C#", Accent);
        C64.Screen.EndUpdate();
        while (true)
        {
        }
    }
}

// CIA 1's time-of-day clock: counts tenths of a second from the mains
// frequency, whatever the interrupts are doing (the KERNAL holds them off
// for much of a disk load, so the jiffy clock falls behind).
static class TimeOfDay
{
    const ulong Tenths = 0xDC08;
    const ulong Seconds = 0xDC09;
    const ulong Minutes = 0xDC0A;
    const ulong Hours = 0xDC0B;
    const ulong ControlA = 0xDC0E;
    const uint FiftyHertz = 0x80;   // control register A bit 7: PAL mains

    // Writing the hours stops the clock, writing the tenths starts it.
    public static void Restart()
    {
        Set(ControlA, C64.GetMemory(ControlA, 0) | FiftyHertz);
        Set(Hours, 0);
        Set(Minutes, 0);
        Set(Seconds, 0);
        Set(Tenths, 0);
    }

    // Up to 25.5 seconds.
    public static uint ElapsedTenths => FromBcd(C64.GetMemory(Seconds, 0)) * 10 + FromBcd(C64.GetMemory(Tenths, 0));

    static uint FromBcd(uint value) => (value >> 4) * 10 + (value & 0x0F);
    static void Set(ulong register, uint value) => C64.FillMemory(register, value, 1);
}
#endif
