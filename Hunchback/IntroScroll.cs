using C64Lib;
using System;

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

    public static void Play()
    {
        Screen.Clear(Colors.Grey2);

        var knight = new Knight { Sprite = C64.Sprites.Sprite1 };
        knight.Init();

        var levels = LevelDescription.Levels;
        // Matches the original's Screen_IntroScrollSelect level picks (9, 8, 0)
        // exactly -- currentLevel is used identically as a direct 0-based
        // index into the level table in both codebases.
        //
        // Each ScrollToLevel call allocates six 1000-element uint[] snapshot
        // arrays (oldChar/oldColor/newChar/newColor here, plus their locals
        // inside SnapshotScreen's own frame) that all go dead the moment it
        // returns -- GC.Collect() after each one reclaims them before the
        // next call allocates its own batch, the same way Game.cs's
        // RunGame loop collects after every real level. Skipping this was
        // a real bug, not just tidiness: without it, three levels' worth of
        // never-reclaimed arrays walked the heap far enough to eventually
        // corrupt something else and crash the whole emulator partway
        // through the third transition (reproduced live in VICE).
        ScrollToLevel(levels[9], knight);
        GC.Collect();
        ScrollToLevel(levels[8], knight);
        GC.Collect();
        ScrollToLevel(levels[0], knight);
        GC.Collect();
    }

    private static void ScrollToLevel(LevelDescription description, Knight knight)
    {
        // Rows*Cols would be a runtime Mul on non-constant operands anywhere
        // else in this file (the compiler has no Mul opcode support at all --
        // discovered while writing this), but both operands are compile-time
        // consts, so Roslyn folds this to a literal 1000 before it ever
        // reaches the compiler. Every other row/column offset below is a
        // running accumulator instead, incremented by Cols per row, to avoid
        // needing a real multiply.
        var oldChar = new uint[Rows * Cols];
        var oldColor = new uint[Rows * Cols];
        SnapshotScreen(oldChar, oldColor);

        // Blanks the physical display (clears the VIC-II's DEN bit in
        // $d011) for the whole prepare phase below. Drawing the new level,
        // reading it back, and restoring the old one all take enough real
        // 6502 cycles that the VIC-II -- which keeps scanning out whatever
        // is actually in screen/color RAM every frame regardless of how
        // "instant" this looks in C# -- was visibly showing the fully-drawn
        // *next* level before the scroll even started. This is the
        // standard C64 "turn off the display during a slow update" trick;
        // real VIC-bank double buffering can't fully solve this anyway,
        // since color RAM (unlike screen RAM) has only one physical bank,
        // with nothing to flip to. $1B is this program's untouched KERNAL
        // boot default for $d011 (confirmed nothing else in this codebase
        // ever writes it); $0B is the same value with just DEN (bit 4)
        // cleared.
        C64.FillMemory(C64Address.FromLabel("$d011") - 1, 0x0B, 1);

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

        var wall = new Wall();
        wall.Draw(description.Color, description.WallType);
        var newChar = new uint[Rows * Cols];
        var newColor = new uint[Rows * Cols];
        SnapshotScreen(newChar, newColor);

        uint i = 0;
        for (uint y = 0; y < Rows; y++)
            for (uint x = 0; x < Cols; x++)
            {
                C64.SetChar(x, y, oldChar[i], (Colors)oldColor[i]);
                i++;
            }

        // Back on now that the live screen genuinely shows only the old
        // level again -- everything from here on is the real, intended
        // animated scroll.
        C64.FillMemory(C64Address.FromLabel("$d011") - 1, 0x1B, 1);

        var screenBase = C64Address.FromLabel("screenMemory");
        var colorBase = C64Address.FromLabel("colorMemory");

        for (uint step = 0; step < Cols; step++)
        {
            var screenRow = screenBase;
            var colorRow = colorBase;
            i = 0;
            for (uint y = 0; y < Rows; y++)
            {
                ShiftRowLeft(screenRow);
                ShiftRowLeft(colorRow);

                C64.SetChar(Cols - 1, y, newChar[i + step], (Colors)newColor[i + step]);

                screenRow += Cols;
                colorRow += Cols;
                i += Cols;
            }

            // Ticks the Knight's own climb/walk cycle (same sprite/animation
            // used as gameplay's background decoration -- see Knight.cs) --
            // several ticks per scroll step so it visibly gets through the
            // climb and a real run across the screen within the intro's
            // duration, rather than barely twitching once.
            for (uint k = 0; k < 8; k++)
                knight.Move();

            Delay.Wait(2);
        }
    }

    private static void SnapshotScreen(uint[] chars, uint[] colors)
    {
        var colorRow = C64Address.FromLabel("colorMemory");
        uint i = 0;
        for (uint y = 0; y < Rows; y++)
        {
            for (uint x = 0; x < Cols; x++)
            {
                chars[i] = (uint)C64.GetChar(x, y);
                colors[i] = C64.GetMemory(colorRow, x);
                i++;
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
