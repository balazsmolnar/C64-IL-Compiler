using C64Lib;
using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.Test;

// C64Lib's methods are empty C# stubs (see CLAUDE.md) -- the real behavior
// lives in asm/helper/*.asm's macros and asm/C64.asm/c64sprite.asm/etc.
// Unlike Test/*.cs's other files (which exercise general compiler/language
// features), these tests specifically target that hand-written library
// surface, verifying its actual runtime behavior through SimpleEmulator --
// including a few real, non-obvious quirks in the implementation that
// existing game code (Hunchback/C64Presentation) already silently relies
// on, confirmed by reading the asm directly rather than assumed.
[TestFixture]
public class C64LibTest
{
    [Test]
    public void SetChar_GetChar_RoundTrip()
    {
        C64.Screen.SetChar(5, 10, 65, Colors.White);
        Assert.AreEqual(C64.Screen.GetChar(5, 10), 65);
    }

    [Test]
    public void SetChar_Different_Positions_Are_Independent()
    {
        C64.Screen.SetChar(0, 0, 1, Colors.White);
        C64.Screen.SetChar(1, 0, 2, Colors.White);
        Assert.AreEqual(C64.Screen.GetChar(0, 0), 1);
        Assert.AreEqual(C64.Screen.GetChar(1, 0), 2);
    }

    // C64_SetChar_Core (asm/C64.asm) treats a character code of $FF as
    // "only touch the color, leave the existing character alone" -- used
    // throughout Hunchback (e.g. PlayerStats' bonus markers keep the
    // header's decorative color showing through). Verify that sentinel
    // directly rather than assuming every code is written literally.
    [Test]
    public void SetChar_0xFF_Leaves_Character_Unchanged()
    {
        C64.Screen.SetChar(3, 3, 42, Colors.White);
        C64.Screen.SetChar(3, 3, 0xFF, Colors.Red);
        Assert.AreEqual(C64.Screen.GetChar(3, 3), 42);
    }

    [Test]
    public void Write_Sets_Characters_From_String()
    {
        // 'A' == $41 under .enc "screen" for this project's custom
        // charset -- confirmed by assembling ".enc \"screen\" / .text
        // \"A\",0" standalone and inspecting the emitted byte, not assumed
        // from the classic PETSCII screen-code layout (which would be 1).
        C64.Screen.Write(2, 2, "A", Colors.White);
        Assert.AreEqual(C64.Screen.GetChar(2, 2), 65);
    }

    // No Chars.A-style constants needed -- a char literal already converts
    // to the exact same uint constant (65) at compile time with zero new
    // library code, so SetChar(x, y, 'A', ...) works directly.
    [Test]
    public void SetChar_Accepts_Char_Literal()
    {
        C64.Screen.SetChar(4, 4, 'A', Colors.White);
        Assert.AreEqual(C64.Screen.GetChar(4, 4), 65);
    }

    [Test]
    public void Write_Stops_At_Terminator()
    {
        C64.Screen.SetChar(6, 6, 99, Colors.White);
        C64.Screen.Write(5, 6, "A", Colors.White); // one real char, then a $00 terminator
        Assert.AreEqual(C64.Screen.GetChar(5, 6), 65);
        Assert.AreEqual(C64.Screen.GetChar(6, 6), 99); // untouched by the terminated write
    }

    // C64_FillMemory's loop (asm/C64.asm) counts Y down from the size
    // parameter with "dey/bne -", which wraps a size of 0 through all 256
    // values before stopping -- the idiomatic "fill a whole 256-byte page"
    // this codebase relies on throughout (Screen.Clear etc.), not "fill
    // zero bytes".
    [Test]
    public void FillMemory_Zero_Size_Fills_Whole_Page()
    {
        C64.FillMemory(0x0400UL, 7, 0);
        Assert.AreEqual(C64.GetMemory(0x0400UL, 0), 7u);
        Assert.AreEqual(C64.GetMemory(0x0400UL, 255), 7u);
    }

    // Same dey/bne loop: for a nonzero size N, it writes offsets N down to
    // 1 -- never offset 0. Real call sites already depend on this
    // (Wall.BuildBasicWall's 199-byte CopyMemory, C64Presentation's
    // RandomDisappearAnimation), so it's documented here directly rather
    // than left as an easy-to-get-wrong implicit assumption.
    [Test]
    public void FillMemory_Nonzero_Size_Starts_At_Offset_One()
    {
        C64.FillMemory(0x0500UL, 3, 0); // clear the page first
        C64.FillMemory(0x0500UL, 9, 5);
        Assert.AreEqual(C64.GetMemory(0x0500UL, 0), 3u); // offset 0: untouched
        Assert.AreEqual(C64.GetMemory(0x0500UL, 1), 9u);
        Assert.AreEqual(C64.GetMemory(0x0500UL, 5), 9u);
        Assert.AreEqual(C64.GetMemory(0x0500UL, 6), 3u); // past the filled range
    }

    [Test]
    public void CopyMemory_Copies_Source_To_Dest()
    {
        C64.FillMemory(0x0600UL, 0, 0);
        C64.FillMemory(0x0600UL, 42, 3); // source offsets 1..3 = 42, per the quirk above
        C64.FillMemory(0x0700UL, 0, 0);
        C64.CopyMemory(0x0700UL, 0x0600UL, 3);
        Assert.AreEqual(C64.GetMemory(0x0700UL, 0), 0u);
        Assert.AreEqual(C64.GetMemory(0x0700UL, 1), 42u);
        Assert.AreEqual(C64.GetMemory(0x0700UL, 3), 42u);
    }

    // SimpleEmulator has no IRQ simulation at all (Emulator.cs's own
    // Interrupt() is a deliberate no-op -- see its comment), so the KERNAL
    // jiffy clock ($A0-$A2) C64_Delay polls never advances on its own
    // here: only the "deadline already passed" branch is testable this
    // way. The actual-waiting behavior (the poll loop really exits once
    // the clock reaches the target, and not before), and that
    // c64_delay_last really is set to the literal jiffy value at each
    // return (not some other computed value), were both verified
    // manually via TestDebugger instead -- stepping through a real call
    // and reading c64_delay_init/c64_delay_last directly, plus a
    // breakpoint inside the poll loop with `set` to advance $a2 mid-wait
    // and confirm it exits at exactly the right tick.
    //
    // FillMemory(dest, value, 1) writes to dest+1, never dest itself (see
    // CopyMemory_Copies_Source_To_Dest's own "offsets 1..3" comment above
    // for the same quirk) -- 0x00A1, not 0x00A2, pokes the jiffy clock's
    // own low byte.
    [Test]
    public void Delay_Returns_Immediately_When_Enough_Time_Already_Elapsed_Since_Last_Call()
    {
        C64.Delay(0); // first call ever: seeds c64_delay_last from "now" (0 in a fresh emulator); s50=0 always returns immediately
        C64.FillMemory(0x00A1UL, 10, 1); // pretend 10 jiffies have passed since that first call
        C64.Delay(5); // elapsed (10-0=10) already >= 5 -- must not hang
        Assert.IsTrue(true); // reaching here (no emulator step-limit timeout) is the pass condition
    }

    // Exercises the high byte ($A1) specifically: a gap of exactly 256
    // ticks (middle byte 1, low byte 0) wraps the low byte alone back to
    // 0, which an 8-bit-only elapsed calculation would misread as "no
    // time has passed at all" -- confirmed as a real source of felt
    // non-determinism (the ball's own pacing getting coupled to an
    // unrelated, shared-timer call site elsewhere that can itself run
    // for several seconds), not just a theoretical risk. The high byte
    // being nonzero must be enough on its own to recognize "already far
    // more than s50 have passed," regardless of what the low byte reads.
    [Test]
    public void Delay_Returns_Immediately_Across_A_256_Tick_Gap()
    {
        C64.Delay(0); // seeds c64_delay_last = (hi=0, lo=0)
        C64.FillMemory(0x00A0UL, 1, 1); // middle byte ($A1) = 1
        C64.FillMemory(0x00A1UL, 0, 1); // low byte ($A2) = 0 -- now = 256 ticks since last
        C64.Delay(5); // elapsed (256-0=256) already >= 5 -- must not hang
        Assert.IsTrue(true);
    }

    // SimpleEmulator doesn't model CIA timers actually counting down at
    // all (confirmed: no $DD0x handling in Emulator.cs, so they behave as
    // plain RAM here) -- this only confirms the call compiles/runs and
    // returns a small value right after Start(), not real timing
    // accuracy. The real, cycle-accurate behavior (does it count the
    // right number of cycles, does the tear-safe read actually matter)
    // was verified manually against a live VICE instance instead -- see
    // the conversation this was built from.
    [Test]
    public void Stopwatch_Elapsed_Is_Small_Right_After_Start()
    {
        C64.Stopwatch.Start();
        var elapsed = C64.Stopwatch.Elapsed();
        Assert.IsTrue(elapsed < 100UL);
    }

    [Test]
    public void SetBorderColor_GetBorderColor_RoundTrip()
    {
        C64.Screen.SetBorderColor(Colors.Red);
        Assert.AreEqual((uint)C64.Screen.GetBorderColor(), (uint)Colors.Red);
        C64.Screen.SetBorderColor(Colors.Cyan);
        Assert.AreEqual((uint)C64.Screen.GetBorderColor(), (uint)Colors.Cyan);
    }

    // WaitForVBlank polls the raster beam ($D012); SimpleEmulator's raster
    // model moves the beam on with every read, so the wait terminates --
    // this is the end-to-end check (C# call -> compiler flag -> asm ->
    // emulator). The routine's own behavior (ends on line 251, back-to-back
    // calls a frame apart) is covered by SimpleEmulator.Test's RasterTests.
    [Test]
    public void WaitForVBlank_Returns_And_Leaves_Following_Code_Running()
    {
        C64.Screen.WaitForVBlank();
        C64.Screen.WaitForVBlank();
        C64.Screen.SetBorderColor(Colors.Green);
        Assert.AreEqual((uint)C64.Screen.GetBorderColor(), (uint)Colors.Green);
    }

    // SpriteCollection.Collisions reads the real VIC-II sprite-collision
    // register ($D01E) directly -- with no sprites ever drawn in this
    // headless harness, it should read back reliably zero.
    [Test]
    public void Sprites_Collisions_Is_Zero_With_No_Sprites()
    {
        Assert.AreEqual(C64.Sprites.Collisions, 0u);
    }

    // Every Sprite/Sound/Joystick property in this library is a write-only
    // mirror of a hardware register -- no getter exists in the asm for any
    // of them except IsInCollision/IsInBackgroundCollision/Collisions (see
    // asm/c64sprite.asm), so they can't be round-trip verified the way
    // SetChar/FillMemory/SetBorderColor above can be. This is a smoke test
    // instead: every write-only call in the library, exercised once, to
    // catch a gross regression (a bad macro, wrong parameter order/count)
    // even without a way to observe the result from C#.
    [Test]
    public void Write_Only_Hardware_Setters_Do_Not_Crash()
    {
        var sprite = C64.Sprites.Sprite0;
        sprite.Visible = true;
        sprite.Color = Colors.Green;
        sprite.MultiColor = true;
        sprite.ExpandX = true;
        sprite.ExpandY = true;
        sprite.DataBlock = 0x2000UL;
        sprite.X = 100UL;
        sprite.Y = 100U;
        sprite.Visible = false;

        C64.Sprites.CommonColor1 = Colors.Brown;
        C64.Sprites.CommonColor2 = Colors.Grey3;

        C64.Sound.Volume = 15;
        C64.Sound.PlayEffectReg1(WaveForm.Triangle, 1000UL, 0UL, 9, 0, false);
        C64.Sound.PlayEffectReg2(WaveForm.Noise, 2000UL, 0UL, 9, 0, false);

        C64.Screen.SetMultiColor();
        C64.Screen.SetCharBackgroundColor(0, Colors.Grey1);
        C64.Screen.SetCharSet(0x2000UL);

        // No assertion needed: the test harness (asm/unittest.asm) marks
        // result = success as soon as this method returns normally, so
        // simply reaching here without the emulator faulting already is
        // the pass condition -- there's nothing to read back and compare
        // against for a write-only property in the first place.
    }

    // NOTE: no NUnit/SimpleEmulator-level test for Screen.SetPixel/graphics
    // here, despite that being the natural, fast, exact-byte-verification
    // choice for its addressing math (see asm/C64Graphics.asm's
    // Graphics_ComputePixelAddress) -- confirmed empirically that it
    // doesn't fit. This project's unit-test harness compiles ALL 560+
    // Test/*.cs tests as ONE combined program (asm/unittest.asm), which
    // already has only ~1.9KB headroom before OBJ_TABLES_MAX_START ($d000,
    // the hardest possible ceiling for this program type -- I/O registers
    // start there, and it's already at that limit, nowhere higher to
    // raise it). The graphics feature's memory reservation
    // (Compiler/Templates/UnitTestEntry.asm's GRAPHICS_USED block) costs
    // ~9.2KB the moment ANY test calls a graphics method -- confirmed via
    // a real build attempt: `.cerror * >= OBJ_TABLES_MAX_START` (asm/
    // helper/objectTables.asm) fired exactly as designed, rather than
    // silently corrupting memory. Graphics correctness is instead verified
    // visually via VICE (vice-verify skill) against Demo/Program.cs -- see
    // that file and the plan this feature was built from.
}
