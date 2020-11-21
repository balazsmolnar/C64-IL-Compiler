using System;

namespace C64Lib
{
    public enum Colors
    {
        Black = 0,
        White,
        Red,
        Cyan,
        Violet,
        Green,
        Blue,
        Yellow,
        Orange,
        Brown,
        LightRed,
        Grey1,
        Grey2,
        LightGreen,
        LightBlue,
        Grey3
    }
    public static class C64
    {
        public static void SetChar(int x, int y, int ch, Colors colors=Colors.LightBlue) {}
    }
}
