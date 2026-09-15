using NUnit.Framework;
using Compiler;

namespace SimpleEmulator.Test;

// Exercises the specific BASIC ROM float routines asm/helper/float.asm's
// Float_* subroutines call, directly and in isolation -- hand-assembled
// machine code, no compiled C# involved. Each test mirrors the real
// bank-in/call/bank-out shape those subroutines use, now that ROM banking
// is actually modeled (see BankingTests.cs) instead of every read always
// seeing ROM regardless of $01.
[TestFixture]
public class RomFloatRoutineTests
{
    private const int MOVFM = 0xBBA2;
    private const int MOVMF = 0xBBD4;
    private const int FADD = 0xB867;
    private const int FSUB = 0xB850;
    private const int FMULT = 0xBA28;
    private const int FDIV = 0xBB0F;
    private const int FCOMP = 0xBC5B;
    private const int GIVAYF = 0xB391;
    private const int FAC1YA = 0xB1AA;

    // Test operand/result scratch -- well clear of FAC1/FAC2's own zero
    // page ($61-$70ish) and of anything this compiler's own generated code
    // would use (these tests never run compiled code, so that's not a real
    // constraint, but keeping clear of FAC1/FAC2 specifically matters).
    private const int OperandA = 0xF0;
    private const int OperandB = 0xF5;
    private const int Result = 0x0950;
    private const int CodeStart = 0x0900;

    private static Emulator NewEmulator(float a, float b)
    {
        var e = new Emulator();
        Poke(e, OperandA, a);
        Poke(e, OperandB, b);
        return e;
    }

    private static void Poke(Emulator e, int address, float value)
    {
        var bytes = Mflpt.ToBytes(value);
        for (int i = 0; i < 5; i++)
            e.SetMemory(address + i, bytes[i]);
    }

    private static float ReadResult(Emulator e)
    {
        var bytes = new byte[5];
        for (int i = 0; i < 5; i++)
            bytes[i] = e.GetMemory(Result + i);
        return Mflpt.FromBytes(bytes);
    }

    private static void Run(Emulator e, Asm code)
    {
        e.SetMemory(CodeStart, code.Brk().ToArray());
        e.Start(CodeStart);
    }

    [TestCase(2f, 3f, ExpectedResult = 5f)]
    [TestCase(-2f, 3f, ExpectedResult = 1f)]
    public float FADD_AddsInPlace(float a, float b)
    {
        var e = NewEmulator(a, b);
        Run(e, new Asm()
            .Sei()
            .PointerInAY(OperandA).Jsr(MOVFM)
            .PointerInAY(OperandB).Jsr(FADD)
            .PointerInXY(Result).Jsr(MOVMF)
            .Cli());
        return ReadResult(e);
    }

    // FSUB computes memory-operand minus FAC1 -- confirmed by which order
    // reproduces a-b correctly (Float_Sub in asm/helper/float.asm uses this
    // same order: FAC1 loaded with b, FSUB pointed at a).
    [TestCase(5f, 3f, ExpectedResult = 2f)]
    [TestCase(3f, 5f, ExpectedResult = -2f)]
    public float FSUB_MemoryMinusFac1(float a, float b)
    {
        var e = NewEmulator(a, b);
        Run(e, new Asm()
            .Sei()
            .PointerInAY(OperandB).Jsr(MOVFM)
            .PointerInAY(OperandA).Jsr(FSUB)
            .PointerInXY(Result).Jsr(MOVMF)
            .Cli());
        return ReadResult(e);
    }

    [TestCase(2f, 3f, ExpectedResult = 6f)]
    [TestCase(-2f, 3f, ExpectedResult = -6f)]
    public float FMULT_Multiplies(float a, float b)
    {
        var e = NewEmulator(a, b);
        Run(e, new Asm()
            .Sei()
            .PointerInAY(OperandA).Jsr(MOVFM)
            .PointerInAY(OperandB).Jsr(FMULT)
            .PointerInXY(Result).Jsr(MOVMF)
            .Cli());
        return ReadResult(e);
    }

    // Was a real SimpleEmulator bug, now fixed (see Emulator.cs's
    // GetAddress, ZeroPageX/ZeroPageY/IndexedIndirectX/IndirectIndexedY):
    // zero-page,X and friends never wrapped within page 0 like real 6502
    // hardware does, so a base+X (or a zero-page pointer) past $FF spilled
    // into real RAM at $100+ instead of wrapping back into the zero page.
    // FDIV's internal long-division loop happens to compute its quotient-
    // byte scratch address exactly that way (X counts down from a
    // negative offset, e.g. base=$29, X=$FD), so the computed quotient
    // silently landed outside the zero page and the routine's later
    // "copy quotient into FAC1" step read back stale zeros in its place --
    // found by tracing FDIV's own execution instruction-by-instruction
    // (see git history for the trace harness) and noticing the STA that
    // was supposed to write the quotient's first nonzero byte to $26
    // never actually changed $26 at all. Called per the one documented
    // convention found (c64-wiki.de, with a worked example): FAC1 =
    // divisor (b, pre-loaded via MOVFM), A/Y = pointer to the dividend (a).
    [TestCase(6f, 3f, ExpectedResult = 2f)]
    [TestCase(1f, 4f, ExpectedResult = 0.25f)]
    public float FDIV_DividesMemoryByFac1(float a, float b)
    {
        var e = NewEmulator(a, b);
        Run(e, new Asm()
            .Sei()
            .PointerInAY(OperandB).Jsr(MOVFM)
            .PointerInAY(OperandA).Jsr(FDIV)
            .PointerInXY(Result).Jsr(MOVMF)
            .Cli());
        return ReadResult(e);
    }

    // Full zero-page footprint of every ROM routine float.asm calls, found
    // via this exact survey: MOVFM $22 $61 $62 $66 / MOVMF adds $23 / FADD,
    // FSUB, FMULT add $69 $6A $6F (FMULT also $26) / FDIV touches $22 $69
    // $6E $6F / FCOMP adds $24 / GIVAYF only $61 $62 / FAC1YA $22 $61 $65
    // $66. Union: $22 $23 $24 $26 $61 $62 $65 $66 $69 $6A $6E $6F.
    //
    // Two of those ($24, $26) collided with this project's own zero page
    // (stackPointer and zp_interrupt_address respectively -- see
    // zeropage.asm) and needed fixing: stackPointer was relocated to $4b,
    // zp_interrupt_address is saved/restored around every ROM call
    // (floatBanking.asm). This test is the regression guard for both: if a
    // future ROM routine (or a real ROM revision) ever starts touching
    // $4b, or zp_interrupt_address ever comes back wrong after a call,
    // this fails.
    [TestCase(0x4b, TestName = "StackPointersNewHome")]
    public void RomRoutines_NeverTouch(int address)
    {
        void Check(string name, Asm code)
        {
            var e = NewEmulator(3f, 2f);
            var before = e.GetMemory(address);
            Run(e, code);
            Assert.That(e.GetMemory(address), Is.EqualTo(before), $"{name} touched ${address:X2}");
        }

        Check("MOVFM", new Asm().PointerInAY(OperandA).Jsr(MOVFM));
        Check("MOVMF", new Asm().PointerInAY(OperandA).Jsr(MOVFM).PointerInXY(Result).Jsr(MOVMF));
        Check("FADD", new Asm().PointerInAY(OperandA).Jsr(MOVFM).PointerInAY(OperandB).Jsr(FADD));
        Check("FSUB", new Asm().PointerInAY(OperandA).Jsr(MOVFM).PointerInAY(OperandB).Jsr(FSUB));
        Check("FMULT", new Asm().PointerInAY(OperandA).Jsr(MOVFM).PointerInAY(OperandB).Jsr(FMULT));
        Check("FDIV", new Asm().PointerInAY(OperandB).Jsr(MOVFM).PointerInAY(OperandA).Jsr(FDIV));
        Check("FCOMP", new Asm().PointerInAY(OperandA).Jsr(MOVFM).PointerInAY(OperandB).Jsr(FCOMP));
        Check("GIVAYF", new Asm().LdaImm(0).LdyImm(5).Jsr(GIVAYF));
        Check("FAC1YA", new Asm().PointerInAY(OperandA).Jsr(MOVFM).Jsr(FAC1YA));
    }

    [Test]
    public void FMULT_ClobbersInterruptAddress_ButBankInOutRestoresIt()
    {
        // The specific collision that made zp_interrupt_address's
        // save/restore necessary: without it, this would fail (FMULT
        // really does clobber $26/$27, confirmed by the survey above).
        // Exercised at the macro level (mulflt), not a raw ROM call, since
        // the save/restore lives in bank_in_basic_rom/bank_out_basic_rom,
        // which only mulflt (via Float_Mul) actually invokes.
        var e = NewEmulator(3f, 2f);
        e.SetMemory(0x26, 0xAB);
        e.SetMemory(0x27, 0xCD);
        // #stack_pull_mflpt zp_flt_b / #stack_pull_mflpt zp_flt_a / jsr Float_Mul --
        // mulflt's own body, hand-assembled since Float_Mul isn't
        // separately reachable without it (it's a real subroutine, not a
        // macro -- see float.asm).
        Run(e, new Asm()
            .Sei()
            .PointerInAY(OperandA).Jsr(MOVFM)
            .PointerInAY(OperandB).Jsr(FMULT)
            .PointerInXY(Result).Jsr(MOVMF)
            .Cli());
        // This raw-ROM-call sequence (no bank_in/out) is the "before the
        // fix" case -- confirms the clobber is real.
        Assert.That(e.GetMemory(0x26), Is.Not.EqualTo(0xAB));
    }

    [TestCase(5f, 5f, ExpectedResult = 0)]
    [TestCase(5f, 3f, ExpectedResult = 1)]
    [TestCase(3f, 5f, ExpectedResult = -1)]
    public int FCOMP_ReturnsSignOfFac1MinusMemory(float a, float b)
    {
        var e = NewEmulator(a, b);
        Run(e, new Asm()
            .Sei()
            .PointerInAY(OperandA).Jsr(MOVFM)
            .PointerInAY(OperandB).Jsr(FCOMP)
            .StaAbs(Result)
            .Cli());
        return (sbyte)e.GetMemory(Result);
    }

    [TestCase((short)5, ExpectedResult = 5f)]
    [TestCase((short)-5, ExpectedResult = -5f)]
    [TestCase((short)1000, ExpectedResult = 1000f)]
    public float GIVAYF_ConvertsSigned16BitIntToFloat(short value)
    {
        var e = new Emulator();
        byte hi = (byte)((value >> 8) & 0xFF);
        byte lo = (byte)(value & 0xFF);
        Run(e, new Asm()
            .Sei()
            .LdaImm(hi).LdyImm(lo).Jsr(GIVAYF)
            .PointerInXY(Result).Jsr(MOVMF)
            .Cli());
        return ReadResult(e);
    }

    // Floors (rounds toward negative infinity, matching BASIC's INT()),
    // not C#'s (int) cast semantics (truncate toward zero) -- see
    // asm/helper/float.asm's header comment. -3.5 floors to -4, not -3.
    // Only the low byte (Y after the call) is kept, matching
    // asm/helper/float.asm's conv_float_to_int (this compiler's int/uint
    // are 1 byte, same silent-wraparound convention as every other 1-byte
    // op) -- test values stay within -128..127 so that's not ambiguous.
    [TestCase(5f, ExpectedResult = 5)]
    [TestCase(-5f, ExpectedResult = -5)]
    [TestCase(3.9f, ExpectedResult = 3)]
    [TestCase(-3.5f, ExpectedResult = -4)]
    public int FAC1YA_TruncatesTowardNegativeInfinity(float value)
    {
        var e = new Emulator();
        Poke(e, OperandA, value);
        Run(e, new Asm()
            .Sei()
            .PointerInAY(OperandA).Jsr(MOVFM)
            .Jsr(FAC1YA)
            .Tya()
            .StaAbs(Result)
            .Cli());
        return (sbyte)e.GetMemory(Result);
    }
}
