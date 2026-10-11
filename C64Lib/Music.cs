namespace C64Lib
{
    public static partial class C64
    {
        // A data-driven SID music and sound-effect player (asm/C64Music.asm).
        // Nested static class for the same reason as Screen/Stopwatch: every
        // call compiles to a plain static call.
        //
        // The songs, instruments and effects are data the program supplies:
        // write a score in Python against tools/sidmusic.py, which generates
        // an .asm file to embed with [assembly: RawAssembly(Resource = ...)]
        // (and, optionally, a C# file of song/effect numbers). See
        // Catacombs/audio/ for a complete example.
        //
        // Three voices play the song; one effect at a time borrows a voice
        // (which one is chosen in the score) and the music takes it back when
        // the effect is over. A program that calls none of these methods
        // pays nothing for the player.
        public static class Music
        {
            // Silences the SID and starts the player ticking 60 times a
            // second from the KERNAL's interrupt, so it keeps time no matter
            // what the main program is doing. Call once, before Play or
            // PlayEffect. (A program that drives its own interrupts calls
            // Tick() once per frame instead and never calls Install.)
            public static void Install() { }

            // Starts a song from its beginning, replacing the current one.
            public static void Play(uint song) { }

            // Stops the song and releases its notes. Effects still play.
            public static void Stop() { }

            // Plays a sound effect over the music. While an effect is still
            // sounding, only one with an equal or higher number replaces it,
            // so number effects from least to most important.
            public static void PlayEffect(uint effect) { }

            // One step of the player -- only for programs that do not call
            // Install() and tick it themselves.
            public static void Tick() { }
        }
    }
}
