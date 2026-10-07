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
        // A pseudo-random byte (0..255) from a software LFSR (asm/C64.asm's
        // C64_Random) -- not a hardware RNG (e.g. the SID's voice-3 noise
        // oscillator), deliberately: that would need SID voice 3 reserved
        // for noise generation, which could collide with a game's own
        // music/sound effects using all three voices. Combine with the
        // now-existing % operator for a bounded range, e.g.
        // `C64.Random() % 5` for 0..4.
        public static uint Random() => 0;

        // Waits until the KERNAL jiffy clock (asm/C64.asm's C64_Delay) has
        // advanced at least s50 ticks (50ths of a second on PAL) past
        // wherever it was at the end of the PREVIOUS Delay call -- if
        // that many ticks (or more) have already passed, e.g. because
        // other code ran a while between calls, this returns immediately
        // rather than waiting an extra s50 on top. The very first call
        // ever seeds that starting point from the real jiffy clock at
        // that moment (not a fixed 0), so whatever setup work ran before
        // it doesn't make that first call return instantly too.
        //
        // One shared timer across every call site in the whole program --
        // "the previous Delay call" means the previous call to Delay from
        // ANYWHERE, not just this call site's own last call. Calling it
        // from two unrelated places that both expect their own
        // independent pacing will make them interfere with each other.
        public static void Delay(uint s50) { }
    }
}
