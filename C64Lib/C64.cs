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

    // No parameters -- deliberately not System.EventHandler. There's only ever
    // one interrupt source here, so a sender/EventArgs pair would just be
    // ceremony: a fabricated sender object and a heap-allocated EventArgs on
    // every IRQ, for no payoff. Also sidesteps needing to compile
    // Delegate.Combine/real multicast semantics -- see OpNewObj's delegate
    // special case (Compiler/Operands/OperandBase.cs) and C64.asm's
    // C64_add_Interrupt/OnInterrupt, both written only for a single,
    // static-method subscriber (a second `+=` still just replaces the first,
    // same as before this was made to compile at all).
    public delegate void InterruptHandler();

    public static partial class C64
    {
        public static event InterruptHandler Interrupt;
        public static SpriteCollection Sprites => null;
        public static JoystickCollection Joysticks => null;
        public static Sound Sound => null;
        public static Debug Debug => null;
        public static bool IsKeyPressed(Keys key) => false;
        public static void CopyMemory(ulong dest, ulong source, uint size) { }
        public static void FillMemory(ulong dest, uint value, uint size) { }
        public static uint GetMemory(ulong address, uint x) => 0;
    }
}
