using C64Lib;

namespace Hunchback;

// Replicates the original's attract-mode title sequence (Restructure/Screen.asm's
// Screen_IntroScrollSelect/Screen_IntroScroll + Quasi.asm's Quasi_IntroMovement
// family, confirmed against that disassembly): three level layouts scroll into
// view left-to-right while the Knight sprite climbs then runs across the
// screen, before TitleScreen's static logo/tune loop takes over. The original
// never used VIC-II hardware smooth-scroll or a raster IRQ for this -- it's a
// software character-cell scroll (shift each row's screen/color RAM left by
// one column per step, feed the incoming level's next column in from the
// right), which is what's reproduced here.
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
    // compiler, and each of these six is allocated exactly once here
    // anyway, then reused (overwritten in place) across all three levels
    // rather than freed and reallocated -- exactly like Wall here (Draw()
    // resets every field of Wall's that matters; Move() is never called
    // during the intro, so nothing carries over that shouldn't).
    private static Knight s_knight;
    private static Wall s_wall;
    private static uint[][] s_oldChar;
    private static uint[][] s_oldColor;
    private static uint[][] s_newChar;
    private static uint[][] s_newColor;

    public static void Play()
    {
        Screen.Clear(Colors.Grey2);

        s_knight = new Knight { Sprite = C64.Sprites.Sprite1 };
        s_knight.Init();
        s_wall = new Wall();

        s_oldChar = NewRowBuffers();
        s_oldColor = NewRowBuffers();
        s_newChar = NewRowBuffers();
        s_newColor = NewRowBuffers();

        var levels = LevelDescription.Levels;
        // Matches the original's Screen_IntroScrollSelect level picks (9, 8, 0)
        // exactly -- currentLevel is used identically as a direct 0-based
        // index into the level table in both codebases.
        ScrollToLevel(levels[9]);
        ScrollToLevel(levels[8]);
        ScrollToLevel(levels[0]);
    }

    private static void ScrollToLevel(LevelDescription description)
    {
        SnapshotScreen(s_oldChar, s_oldColor);

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
        SnapshotScreen(s_newChar, s_newColor);

        for (uint y = 0; y < Rows; y++)
            for (uint x = 0; x < Cols; x++)
                C64.SetChar(x, y, s_oldChar[y][x], (Colors)s_oldColor[y][x]);

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

                C64.SetChar(Cols - 1, y, s_newChar[y][step], (Colors)s_newColor[y][step]);

                screenRow += Cols;
                colorRow += Cols;
            }

            // Ticks the Knight's own climb/walk cycle (same sprite/animation
            // used as gameplay's background decoration -- see Knight.cs) --
            // several ticks per scroll step so it visibly gets through the
            // climb and a real run across the screen within the intro's
            // duration, rather than barely twitching once.
            for (uint k = 0; k < 8; k++)
                s_knight.Move();

            Delay.Wait(2);
        }
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
    // second real, narrower compiler bug: the *last* value such a local
    // is reassigned to inside a loop never gets its root-count decremented
    // at method exit (every reassignment before the last one releases the
    // previous value correctly). See
    // Test/GCTest.cs's Reassigned_Ref_Local_In_Loop_Releases_Each_Previous_Value
    // (confirmed failing on exactly the last iteration) and
    // Jagged_Uint_Array_Fully_Collected (confirmed passing, since it only
    // ever indexes directly) for how this was isolated.
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
