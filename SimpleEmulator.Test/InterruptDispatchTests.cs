using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace SimpleEmulator.Test;

// Directly drives asm/C64.asm's C64_add_Interrupt/OnInterrupt against a
// real compiled program (prg/unittest.prg, the same file
// C64TestFramework's RunInEmulatorAspect uses) rather than through a
// compiled C# `C64.Interrupt += ...;` call -- SimpleEmulator has no real
// interrupt-timer model to fire an actual IRQ (see Emulator.cs's
// Interrupt(), a deliberate no-op), so an interrupt has to be synthesized
// here: register a handler, poke known sentinel values into bytes the
// handler's own execution is known to clobber, jump straight into
// OnInterrupt, and confirm those bytes come back exactly as they were
// once the handler (a real, compiled method -- see
// Test/InterruptTests.cs's Handler) has run.
//
// Complements Test/InterruptTests.cs's Subscribe_Compiles_And_Runs (proves
// the delegate-construction fix works through the real compiler pipeline,
// but can't observe the interrupt actually firing) and the live-VICE smoke
// test (the only place a real, asynchronously-timed interrupt can be
// exercised at all).
[TestFixture]
public class InterruptDispatchTests
{
    // zp_tmp1_low -- clobbered by every C64Lib API macro's return-address
    // relocation (#stack_save_return_adress), including
    // C64_SetBorderColor, which the compiled handler calls. If OnInterrupt's
    // save/restore didn't work, this would come back holding that call's
    // own return address instead of the sentinel poked below.
    private const int ZpTmp1Low = 0x20;
    private const int RomBankingRegister = 0x01;
    private const int BorderColorRegister = 0xD020;
    private const byte GreenColor = 5; // C64Lib.Colors.Green

    // Bit 0 (LORAM) must stay 0 -- banking BASIC ROM back in ($a000-$bfff
    // reading as ROM again) would break localsStack, which this project
    // places in that same ROM-shadowed range once ROM is banked out (see
    // LoadUnitTestProgram) -- every compiled method call, including the
    // handler's own prologue/epilogue, depends on it being real RAM. Bit
    // 7 is just an arbitrary marker distinguishing this from any value
    // bank_in/bank_out_basic_rom might otherwise leave behind.
    private const byte Zp01Sentinel = 0x86;

    private const int DriverStart = 0x0900;

    private static Dictionary<string, int> labels;

    private static string PrgFolder()
    {
        var directory = new FileInfo(typeof(InterruptDispatchTests).Assembly.Location).DirectoryName;
        return Path.Combine(directory, "..\\..\\..\\prg");
    }

    private static int Label(string name)
    {
        if (labels == null)
        {
            labels = new Dictionary<string, int>();
            foreach (var line in File.ReadAllLines(Path.Combine(PrgFolder(), "unittest.labels")))
            {
                var parts = line.Split(' ');
                labels[parts[2]] = Convert.ToInt32(parts[1], 16);
            }
        }
        return labels[$".{name}"];
    }

    // Mirrors the real program bootstrap's #disable_basic_rom
    // (asm/helper/banking.asm): banks BASIC ROM out so $a000-$bfff reads
    // as RAM. This test never runs that bootstrap (it jumps straight into
    // a hand-assembled driver, skipping Program_Main/Run_Test entirely),
    // but every compiled method call -- including the handler's own
    // prologue/epilogue -- depends on localsStack (which lives in that
    // range) being real, readable-back RAM, not BASIC ROM's own bytes.
    private static Emulator LoadUnitTestProgram()
    {
        var e = new Emulator();
        e.LoadPrg(Path.Combine(PrgFolder(), "unittest.prg"));
        e.SetMemory(RomBankingRegister, 0x06);
        return e;
    }

    [Test]
    public void OnInterrupt_RegistersAndDispatchesToACompiledHandler()
    {
        var e = LoadUnitTestProgram();
        int handlerAddress = Label("InterruptTests_Handler");
        int addInterrupt = Label("C64_add_Interrupt");
        int onInterrupt = Label("OnInterrupt");

        e.SetMemory(BorderColorRegister, 0x0E); // anything but green, so a no-op dispatch can't pass by accident
        e.SetMemory(DriverStart, new Asm()
            .PushPointer(handlerAddress)
            .Jsr(addInterrupt)
            .Jsr(onInterrupt)
            .ToArray());

        // OnInterrupt ends by chaining to whatever was previously at
        // $0314/$0315 (jmp (zp_interrupt_saved_low)) -- C64_add_Interrupt
        // saved that from fresh, zero-initialized RAM here, so it chains
        // to $0000, which is also zero-initialized -- opcode $00 is BRK,
        // so execution halts there on its own. maxSteps is just a safety
        // net against that reasoning being wrong (an infinite loop instead
        // of the expected halt), not the thing actually ending the test.
        e.Start(DriverStart, 5000, (pc, step) => { });

        Assert.That(e.GetMemory(BorderColorRegister), Is.EqualTo(GreenColor),
            "the compiled handler (SetBorderColor(Colors.Green)) should have run");
    }

    [Test]
    public void OnInterrupt_SavesAndRestoresZeroPageScratchAndBankingRegister()
    {
        var e = LoadUnitTestProgram();
        int handlerAddress = Label("InterruptTests_Handler");
        int addInterrupt = Label("C64_add_Interrupt");
        int onInterrupt = Label("OnInterrupt");

        e.SetMemory(DriverStart, new Asm()
            .PushPointer(handlerAddress)
            .Jsr(addInterrupt)
            // Poked AFTER registration (C64_add_Interrupt itself also uses
            // zp_tmp1_low, so sentinels only make sense once it's done)
            // and immediately before the synthesized dispatch, so this is
            // testing OnInterrupt's own save/restore specifically.
            .LdaImm(0x11).StaZp(ZpTmp1Low)
            .LdaImm(Zp01Sentinel).StaZp(RomBankingRegister)
            .Jsr(onInterrupt)
            .ToArray());

        e.Start(DriverStart, 5000, (pc, step) => { });

        Assert.That(e.GetMemory(ZpTmp1Low), Is.EqualTo(0x11),
            "zp_tmp1_low should be restored after the handler's own C64Lib call clobbered it");
        Assert.That(e.GetMemory(RomBankingRegister), Is.EqualTo(Zp01Sentinel),
            "$01 (ROM banking register) should be restored");
    }
}
