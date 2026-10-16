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
            // Silences the SID and starts the player ticking once per video
            // frame from the KERNAL's interrupt, so it keeps time no matter
            // what the main program is doing. Call once, before Play or
            // PlayEffect. It re-times that interrupt to the frame (50 Hz on
            // PAL, in the bottom border), so the jiffy clock and C64.Delay
            // count 50ths from then on; works with or without
            // Screen.SetSplit. (A program that drives its own interrupts
            // calls Tick() once per frame instead and never calls Install.)
            public static void Install() { }

            // Starts a song from its beginning, replacing the current one
            // (and forgetting any song Interrupt put aside).
            public static void Play(uint song) { }

            // Puts the playing song aside exactly where it is and plays
            // this one instead -- a fight breaking into the level's tune.
            // Resume() (or a song that ends with 'resume' in the score)
            // carries on with the first one from that very note. Only one
            // song is kept: a second Interrupt before a Resume just
            // replaces what is playing.
            public static void Interrupt(uint song) { }
            public static void Resume() { }

            // Stops the song and releases its notes. Effects still play.
            public static void Stop() { }

            // Plays a sound effect over the music. While an effect is still
            // sounding, only one with an equal or higher number replaces it,
            // so number effects from least to most important. An effect
            // marked polite in the score only plays when the music's note
            // on the borrowed voice is released (footsteps).
            public static void PlayEffect(uint effect) { }

            // Ticks the player has made so far, 0-255 and round again: a
            // frame counter (50 a second) that keeps counting however long
            // the main loop takes.
            public static uint Frame => 0;

            // True while a sound effect is sounding (for example to know
            // whether a polite effect was actually played).
            public static bool EffectPlaying => false;

            // One step of the player -- only for programs that do not call
            // Install() and tick it themselves.
            public static void Tick() { }
        }
    }
}
