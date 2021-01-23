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
        Z
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
    }
}
