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
        C64.SetChar(5, 10, 65, Colors.White);
        Assert.AreEqual(C64.GetChar(5, 10), 65);
    }

    [Test]
    public void SetChar_Different_Positions_Are_Independent()
    {
        C64.SetChar(0, 0, 1, Colors.White);
        C64.SetChar(1, 0, 2, Colors.White);
        Assert.AreEqual(C64.GetChar(0, 0), 1);
        Assert.AreEqual(C64.GetChar(1, 0), 2);
    }

    // C64_SetChar_Core (asm/C64.asm) treats a character code of $FF as
    // "only touch the color, leave the existing character alone" -- used
    // throughout Hunchback (e.g. PlayerStats' bonus markers keep the
    // header's decorative color showing through). Verify that sentinel
    // directly rather than assuming every code is written literally.
    [Test]
    public void SetChar_0xFF_Leaves_Character_Unchanged()
    {
        C64.SetChar(3, 3, 42, Colors.White);
        C64.SetChar(3, 3, 0xFF, Colors.Red);
        Assert.AreEqual(C64.GetChar(3, 3), 42);
    }

    [Test]
    public void Write_Sets_Characters_From_String()
    {
        // 'A' == $41 under .enc "screen" for this project's custom
        // charset -- confirmed by assembling ".enc \"screen\" / .text
        // \"A\",0" standalone and inspecting the emitted byte, not assumed
        // from the classic PETSCII screen-code layout (which would be 1).
        C64.Write(2, 2, "A", Colors.White);
        Assert.AreEqual(C64.GetChar(2, 2), 65);
    }

    [Test]
    public void Write_Stops_At_Terminator()
    {
        C64.SetChar(6, 6, 99, Colors.White);
        C64.Write(5, 6, "A", Colors.White); // one real char, then a $00 terminator
        Assert.AreEqual(C64.GetChar(5, 6), 65);
        Assert.AreEqual(C64.GetChar(6, 6), 99); // untouched by the terminated write
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

    [Test]
    public void SetBorderColor_GetBorderColor_RoundTrip()
    {
        C64.SetBorderColor(Colors.Red);
        Assert.AreEqual((uint)C64.GetBorderColor(), (uint)Colors.Red);
        C64.SetBorderColor(Colors.Cyan);
        Assert.AreEqual((uint)C64.GetBorderColor(), (uint)Colors.Cyan);
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

        C64.SetMultiColor();
        C64.SetCharBackgroundColor(0, Colors.Grey1);
        C64.SetCharSet(0x2000UL);

        // Success is simply reaching here without the emulator faulting --
        // every property above is write-only (no getter exists in the asm
        // for any of them, see the class comment), so there's nothing to
        // read back and assert on directly.
        Assert.IsTrue(true);
    }
}
