using C64Lib;

namespace Hunchback;

// Replicates the original's attract-mode title sequence (Restructure/Screen.asm's
// Screen_IntroScrollSelect/Screen_IntroScroll + Quasi.asm's Quasi_IntroMovement
// family, confirmed against that disassembly): three level layouts scroll into
// view left-to-right while the player sprite jump-bounces along the ground the
// entire time (not just for one of them -- Quasi_IntroMovement writes the jump
// frame every tick throughout all three levels), then turns and jumps away
// (Quasi_IntroJumpLeft) once the scroll finishes, before TitleScreen's static
// logo/tune loop takes over. The original never used VIC-II hardware
// smooth-scroll or a raster IRQ for the scroll itself -- it's a software
// character-cell scroll (shift each row's screen/color RAM left by one column
// per step, feed the incoming level's next column in from the right), which is
// what's reproduced here. Music plays continuously throughout, reusing
// TitleScreen's own tune (TitleScreen.GetTones()) instead of only starting once
// the static logo screen appears.
class IntroScroll
{
    private const uint Rows = 25;
    private const uint Cols = 40;

    // Static fields, not locals passed through ScrollToLevel's parameters --
    // two real compiler gaps made that the wrong call:
    //   - ScrollToLevel needed 7 parameters (description plus these 6), and
    //     Roslyn lowers any parameter past the 4th to Ldarg_s, which isn't
    //     mapped in CommandMap.cs (crashes Compiler.exe outright).
    //   - Independently, passing/returning several of these as normal
    //     locals hit real gaps in this compiler's reference-local
    //     root-count tracking at method exit (see NewRowBuffers' and
    //     ScrollToLevel's own comments below for the two variants found
    //     and fixed/worked around) -- a SimpleEmulator diagnostic showed
    //     roughly half of each level's row-buffer garbage staying stuck
    //     alive after GC.Collect() even after both fixes, heading toward
    //     the 255-slot object-table ceiling well before the intro even
    //     finished.
    // Static fields sidestep both: Debug_GetObjectId/IsAlive-based tests
    // elsewhere (Test/GCTest.cs's Two_Instances_First_GCd_Static_Field)
    // already confirm static-field root tracking works correctly in this
    // compiler, and each of these is allocated exactly once here anyway,
    // then reused (overwritten in place) across all three levels rather
    // than freed and reallocated -- exactly like Wall here (Draw() resets
    // every field of Wall's that matters; Move() is never called during
    // the intro, so nothing carries over that shouldn't).
    private static IntroPlayer s_player;
    private static Wall s_wall;

    // Two buffer pairs, swapped (reference-swapped, not re-snapshotted)
    // after each level instead of both being freshly read from the live
    // screen every time -- s_currentChar/Color always holds exactly
    // what's actually on screen right now (the previous level's fully
    // revealed content, or the initial blank screen for the very first
    // level), so ScrollToLevel no longer needs its own GetChar/GetMemory
    // readback of it. Cuts out 1000 of the roughly 2000-3000 read/write
    // calls the blanked prepare phase was doing per transition -- that
    // phase's own real 6502 time is what was showing up as a visible
    // pause between levels.
    private static uint[][] s_bufAChar, s_bufAColor, s_bufBChar, s_bufBColor;
    private static uint[][] s_currentChar, s_currentColor, s_nextChar, s_nextColor;

    private static TitleScreen.Tone[] s_tones;
    private static uint s_toneIndex;
    private static uint s_toneElapsed;

    // tones: passed in and reused from TitleScreen.Display(), rather than
    // fetched here via a second TitleScreen.GetTones() call -- that
    // method allocates a fresh ~64-object array every time it runs, and
    // two live copies at once (one held here, one for
    // TitleScreen.Display()'s own tune loop right after this returns)
    // pushed the object table past its 255-slot ceiling almost
    // immediately after the intro finished, confirmed live in VICE
    // (crashes back to BASIC's READY. prompt) even with zero player
    // input -- see TitleScreen.Display()'s own comment on this.
    public static void Play(TitleScreen.Tone[] tones)
    {
        Screen.Clear(Colors.Grey2);

        s_player = new IntroPlayer { Sprite = C64.Sprites.Sprite1 };
        s_player.Init();
        s_wall = new Wall();

        s_bufAChar = NewRowBuffers();
        s_bufAColor = NewRowBuffers();
        s_bufBChar = NewRowBuffers();
        s_bufBColor = NewRowBuffers();
        s_currentChar = s_bufAChar;
        s_currentColor = s_bufAColor;
        s_nextChar = s_bufBChar;
        s_nextColor = s_bufBColor;
        // One-time initial snapshot of the blank starting screen -- every
        // later "current" is carried over from the previous level's
        // reveal via the buffer swap at the end of ScrollToLevel instead.
        SnapshotScreen(s_currentChar, s_currentColor);

        s_tones = tones;
        s_toneIndex = 0;
        s_toneElapsed = 0;
        // TickMusic only plays a note once it advances past the current
        // one's Sustain -- play the very first note immediately instead
        // of leaving the intro's opening silent until then.
        C64.Sound.PlayEffectReg1(WaveForm.Saw, s_tones[0].Frequency, 0, 9, 0, false);

        var levels = LevelDescription.Levels;
        // Matches the original's Screen_IntroScrollSelect level picks (9, 8, 0)
        // exactly -- currentLevel is used identically as a direct 0-based
        // index into the level table in both codebases.
        ScrollToLevel(levels[9]);
        ScrollToLevel(levels[8]);
        ScrollToLevel(levels[0]);

        // Matches the original's Quasi_IntroJumpLeft: once all three levels
        // have scrolled by, the player turns to face left for one last
        // jump-away flourish over the now-static final level, instead of
        // just stopping -- a few more ticks here so it's actually visible,
        // not an instant flip.
        s_player.TurnLeft();
        for (uint step = 0; step < 24; step++)
        {
            for (uint k = 0; k < 8; k++)
                s_player.Move();
            TickMusic();
            Delay.Wait(2);
        }
    }

    private static void ScrollToLevel(LevelDescription description)
    {
        // Blanks the physical display for the whole prepare phase below.
        // Drawing the new level, reading it back, and restoring the old one
        // all take enough real 6502 cycles that the VIC-II -- which keeps
        // scanning out whatever is actually in screen/color RAM every
        // frame regardless of how "instant" this looks in C# -- was
        // visibly showing the fully-drawn *next* level before the scroll
        // even started. See C64Lib/Screen.cs for why this isn't real
        // double buffering (color RAM has no second bank to flip to) and
        // why that doesn't matter for a full-screen redraw like this one.
        C64.Screen.BeginUpdate();

        // Wall.Draw only paints its own decorated rows (they vary by
        // WallType -- e.g. KnightPits never touches rows 0-9 at all), the
        // same way every WallType's Draw does during normal gameplay,
        // where Game.cs always clears the screen immediately before each
        // one runs. Skipping that clear here would let whatever the
        // *previous* intro level left on those untouched rows (e.g.
        // RowOfBells' bell/rope glyphs) bleed into this level's
        // "new" snapshot below -- Wall.Draw draws on top of the live
        // screen, not onto a blank canvas.
        Screen.Clear(Colors.Grey2);

        s_wall.Draw(description.Color, description.WallType);
        SnapshotScreen(s_nextChar, s_nextColor);

        for (uint y = 0; y < Rows; y++)
            for (uint x = 0; x < Cols; x++)
                C64.SetChar(x, y, s_currentChar[y][x], (Colors)s_currentColor[y][x]);

        // Back on now that the live screen genuinely shows only the old
        // level again -- everything from here on is the real, intended
        // animated scroll.
        C64.Screen.EndUpdate();

        var screenBase = C64Address.FromLabel("screenMemory");
        var colorBase = C64Address.FromLabel("colorMemory");

        for (uint step = 0; step < Cols; step++)
        {
            var screenRow = screenBase;
            var colorRow = colorBase;
            for (uint y = 0; y < Rows; y++)
            {
                ShiftRowLeft(screenRow);
                ShiftRowLeft(colorRow);

                C64.SetChar(Cols - 1, y, s_nextChar[y][step], (Colors)s_nextColor[y][step]);

                screenRow += Cols;
                colorRow += Cols;
            }

            // Ticks the player's own jump-bounce cycle (see IntroPlayer.cs)
            // -- several ticks per scroll step so it plays out at a
            // reasonable pace across the intro's duration, rather than
            // barely twitching once.
            for (uint k = 0; k < 8; k++)
                s_player.Move();

            TickMusic();
            Delay.Wait(2);
        }

        // The live screen now shows exactly what s_nextChar/Color holds --
        // swap references (cheap: just 4 pointers) so the next
        // ScrollToLevel call's "current" is this without re-reading it.
        var swapChar = s_currentChar;
        s_currentChar = s_nextChar;
        s_nextChar = swapChar;
        var swapColor = s_currentColor;
        s_currentColor = s_nextColor;
        s_nextColor = swapColor;
    }

    // Plays through TitleScreen's own tune (TitleScreen.GetTones()), one
    // scroll step at a time, holding each note for its own Sustain value
    // (same field TitleScreen.Display()'s later loop uses) before
    // advancing -- fixes an earlier version that advanced one note per
    // scroll step unconditionally, which played the whole tune far faster
    // than intended. Loops back to the start if the intro runs longer
    // than the tune.
    private static void TickMusic()
    {
        s_toneElapsed++;
        if (s_toneElapsed < s_tones[s_toneIndex].Sustain)
            return;
        s_toneElapsed = 0;
        s_toneIndex++;
        if (s_toneIndex == s_tones.Length)
            s_toneIndex = 0;
        C64.Sound.PlayEffectReg1(WaveForm.Saw, s_tones[s_toneIndex].Frequency, 0, 9, 0, false);
    }

    // A flat uint[Rows*Cols] (1000 elements) silently corrupts past index
    // 255: objTableSize (asm/helper/objectTables.asm) is a single byte per
    // heap slot, so any array's real backing storage is capped at 255
    // bytes regardless of the requested length -- see
    // Test/ArrayTest.cs's Uint_Array_Index_Past_255 (confirmed failing) and
    // Jagged_Uint_Array (confirmed working) for how this was diagnosed.
    // An array-of-arrays sidesteps it: the outer array is 25 elements, each
    // inner one 40 -- both comfortably under the limit.
    //
    // Every access into an outer row-buffer array above is a direct
    // chars[y][x]/colors[y][x] double-index, deliberately never cached into
    // a reference-typed local reused across loop iterations (e.g. "var
    // charsRow = chars[y];" outside the inner loop) -- that pattern hit a
    // second, narrower compiler edge case (isolated, then found to be a
    // false positive -- see Test/GCTest.cs's
    // Reassigned_Ref_Local_In_Loop_Releases_Each_Previous_Value and its own
    // comment), but every access here still avoids it regardless.
    //
    // Called exactly 4 times total, from Play() -- not once per level, see
    // the static fields' own comment for why.
    private static uint[][] NewRowBuffers()
    {
        var buffers = new uint[Rows][];
        for (uint y = 0; y < Rows; y++)
            buffers[y] = new uint[Cols];
        return buffers;
    }

    private static void SnapshotScreen(uint[][] chars, uint[][] colors)
    {
        var colorRow = C64Address.FromLabel("colorMemory");
        for (uint y = 0; y < Rows; y++)
        {
            for (uint x = 0; x < Cols; x++)
            {
                chars[y][x] = (uint)C64.GetChar(x, y);
                colors[y][x] = C64.GetMemory(colorRow, x);
            }
            colorRow += Cols;
        }
    }

    // Shifts row[0..38] = row[1..39] (leaves row[39], the newly-vacated
    // rightmost column, untouched -- the caller overwrites it separately).
    // Can't do this as one direct in-place CopyMemory(row, row+1, 39): that
    // macro's copy loop walks its index from size down to 1, i.e. always
    // writes high-to-low, which is only safe for a right shift -- on this
    // left shift it would clobber not-yet-read source bytes. Routing
    // through the non-overlapping introScrollTempRow buffer sidesteps that
    // regardless of the copy direction. See GameData.asm's comment on that
    // buffer for the exact index math (CopyMemory(dest, source, size) only
    // ever touches offsets 1..size relative to each base, never offset 0 --
    // both calls below are based one address short of where the real data
    // starts to line that up).
    private static void ShiftRowLeft(ulong row)
    {
        var temp = C64Address.FromLabel("introScrollTempRow");
        C64.CopyMemory(temp - 1, row, Cols - 1);
        C64.CopyMemory(row - 1, temp - 1, Cols - 1);
    }
}
