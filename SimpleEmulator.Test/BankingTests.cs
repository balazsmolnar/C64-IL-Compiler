using NUnit.Framework;

namespace SimpleEmulator.Test;

// Confirms SimpleEmulator's ROM/RAM banking model itself, independent of
// any actual 6502 code -- these only need GetMemory/SetMemory. Written
// after discovering the *previous* model was read-blind (every read of
// $a000-$bfff/$e000-$ffff always saw ROM, no matter what was written to
// $01) and shared a single array for ROM and RAM (so a write while banked
// out could permanently clobber the ROM image). See Emulator.cs's
// ReadByte/SetMemory comments for the fix.
[TestFixture]
public class BankingTests
{
    [Test]
    public void DefaultState_BasicAndKernalRomAreMapped()
    {
        var e = new Emulator();

        // Real hardware/VICE default at power-on, before anything writes to
        // $01: LORAM=1, HIRAM=1 (both ROMs visible). $a000 is BASIC ROM's
        // first byte, $e000 KERNAL's -- neither should be zero, since a
        // freshly-allocated RAM array is all zero and the real ROM image
        // is not.
        Assert.That(e.GetMemory(0xa000), Is.Not.EqualTo(0), "expected BASIC ROM content, not zeroed RAM");
        Assert.That(e.GetMemory(0xe000), Is.Not.EqualTo(0), "expected KERNAL ROM content, not zeroed RAM");
    }

    [Test]
    public void BankingOutBasicRom_RevealsRam()
    {
        var e = new Emulator();

        // LORAM=0, HIRAM=1, CHAREN=1 -- this project's own #disable_basic_rom
        // value (asm/helper/banking.asm).
        e.SetMemory(0x01, 0x06);

        // Freshly allocated RAM is zero-initialized, so with BASIC ROM
        // banked out, $a000 must now read as 0, not the ROM's real first
        // byte (confirmed non-zero in DefaultState_... above).
        Assert.That(e.GetMemory(0xa000), Is.EqualTo(0));
        // KERNAL stays mapped (HIRAM still 1) -- unaffected.
        Assert.That(e.GetMemory(0xe000), Is.Not.EqualTo(0));
    }

    [Test]
    public void BankingOutKernalRom_RevealsRam()
    {
        var e = new Emulator();

        // LORAM=1, HIRAM=0: bank out KERNAL specifically, leave BASIC mapped.
        e.SetMemory(0x01, 0x05);

        Assert.That(e.GetMemory(0xe000), Is.EqualTo(0));
        Assert.That(e.GetMemory(0xa000), Is.Not.EqualTo(0));
    }

    [Test]
    public void WritingUnderMappedRom_IsVisibleOnlyAfterBankingOut()
    {
        var e = new Emulator();

        // BASIC ROM is mapped in by default (see DefaultState test) -- a
        // write to $a000 right now must still land in the RAM underneath
        // it, exactly like real hardware's "hide data under ROM" trick,
        // not get silently discarded (the previous model's behavior) and
        // not corrupt the ROM image itself.
        e.SetMemory(0xa123, 0x42);

        // Still reads as ROM while mapped in -- the write didn't overwrite
        // the ROM image, and isn't visible through the ROM view.
        Assert.That(e.GetMemory(0xa123), Is.Not.EqualTo(0x42));

        // Bank BASIC ROM out: the write becomes visible.
        e.SetMemory(0x01, 0x06);
        Assert.That(e.GetMemory(0xa123), Is.EqualTo(0x42));

        // Bank it back in: ROM is intact, shows its own real content again
        // (not the 0x42 that was written while it was hidden).
        e.SetMemory(0x01, 0x07);
        Assert.That(e.GetMemory(0xa123), Is.Not.EqualTo(0x42));
    }

    [Test]
    public void WritingWhileBankedOut_SurvivesBankingBackInAndOutAgain()
    {
        var e = new Emulator();

        e.SetMemory(0x01, 0x06); // bank BASIC ROM out
        e.SetMemory(0xb000, 0x99);
        Assert.That(e.GetMemory(0xb000), Is.EqualTo(0x99));

        e.SetMemory(0x01, 0x07); // bank it back in -- $b000 now shows ROM
        Assert.That(e.GetMemory(0xb000), Is.Not.EqualTo(0x99));

        e.SetMemory(0x01, 0x06); // bank it back out -- the earlier write is
        Assert.That(e.GetMemory(0xb000), Is.EqualTo(0x99)); // still there, untouched by the round trip
    }

    [Test]
    public void RomBankingDoesNotAffectOrdinaryRam()
    {
        var e = new Emulator();

        e.SetMemory(0x2000, 0x55); // ordinary RAM, well below $a000
        e.SetMemory(0x01, 0x06);   // bank BASIC ROM out
        Assert.That(e.GetMemory(0x2000), Is.EqualTo(0x55));
        e.SetMemory(0x01, 0x07);   // bank it back in
        Assert.That(e.GetMemory(0x2000), Is.EqualTo(0x55));
    }

    [Test]
    public void ExecutingFromRam_WhileBasicRomIsBankedIn_RunsTheRamCodeNotRom()
    {
        var e = new Emulator();
        // Code lives at $0900, well below $a000 -- must run correctly
        // regardless of $01's state.
        var code = new Asm().LdaImm(0x42).StaAbs(0x2000).Brk().ToArray();
        e.SetMemory(0x0900, code);
        e.SetMemory(0x01, 0x07); // BASIC ROM mapped in -- must not affect $0900

        e.Start(0x0900);

        Assert.That(e.GetMemory(0x2000), Is.EqualTo(0x42));
    }
}
