using System;

namespace C64Lib
{
    public enum Colors : uint
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

    public enum Keys
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J,
        K,
        L,
        M,
        N,
        O,
        P,
        Q,
        R,
        S,
        T,
        U,
        V,
        W,
        X,
        Y,
        Z,
        Space
    };

    public static class C64
    {
        public static void SetChar(uint x, uint y, uint ch, Colors colors = Colors.LightBlue) { }
        public static int GetChar(uint x, uint y) => 0;
        public static void SetBorderColor(Colors color) { }
        public static void SetBackgroundColor(Colors color) { }
        public static Colors GetBorderColor() => Colors.Black;
        public static event EventHandler Interrupt;
        public static SpriteCollection Sprites => null;

        public static Debug Debug => null;
        public static bool IsKeyPressed(Keys key) => false;
        public static void SetCharSet(ulong address) { }
        public static void CopyMemory(ulong dest, ulong source, uint size) { }
        public static void FillMemory(ulong dest, uint value, uint size) { }
        public static uint GetMemory(ulong address, uint x) => 0;
        public static void Write(uint x, uint y, string s, Colors colors = Colors.LightBlue) { }
        public static void SetMultiColor() { }
        public static void SetCharBackgroundColor(uint colorIndex, Colors color) { }
    }
}
