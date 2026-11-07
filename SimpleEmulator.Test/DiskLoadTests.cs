using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace SimpleEmulator.Test;

// Runs asm/C64Disk.asm's Disk_Load (C64.Disk.Load) on SimpleEmulator with
// the real KERNAL ROM: SETNAM, SETLFS and SETMSG run for real, and LOAD's
// own vector ($0330, ILOAD) points at a stub that records what LOAD was
// given and fails or succeeds on request -- the emulator has no drive.
// Checks the name's screen code -> PETSCII conversion, the device choice,
// the load address, the result and that $01 / the message flags come back.
// Skipped when 64tass isn't installed (TASS_EXE, else the usual c:\tools path).
[TestFixture]
public class DiskLoadTests
{
    const int IloadVector = 0x0330;
    const int FileNameLength = 0xB7;   // KERNAL FNLEN
    const int SecondaryAddress = 0xB9; // SA
    const int Device = 0xBA;           // FA
    const int FileNamePointer = 0xBB;  // FNADR
    const int MessageFlags = 0x9D;     // MSGFLG
    const int CpuPort = 0x01;

    static string prgPath;
    static Dictionary<string, int> labels;

    Emulator emulator;

    [OneTimeSetUp]
    public void AssembleHarness()
    {
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        var tass = Environment.GetEnvironmentVariable("TASS_EXE") ?? @"c:\tools\64tass-1.60.3243\64tass.exe";
        if (!File.Exists(tass))
            Assert.Ignore($"64tass not found at {tass} (set TASS_EXE).");

        var dir = Path.Combine(Path.GetTempPath(), "c64_disk_harness");
        Directory.CreateDirectory(dir);
        var asmPath = Path.Combine(dir, "harness.asm");
        prgPath = Path.Combine(dir, "harness.prg");
        var labelsPath = Path.Combine(dir, "harness.labels");

        string Asm(string relative) => Path.Combine(repoRoot, "asm", relative).Replace('\\', '/');
        File.WriteAllText(asmPath, $@"
.include ""{Asm("helper/zeropage.asm")}""
.include ""{Asm("helper/stack.asm")}""
.include ""{Asm("helper/8bit.asm")}""
Flag_Disk_Load = 1
* = $1000
Harness_Load
    lda #>Harness_Name
    pha
    lda #<Harness_Name
    pha
    lda Harness_Address+1
    pha
    lda Harness_Address
    pha
    jsr Disk_Load
    pla
    sta Harness_Result
    tsx
    stx Harness_StackAfter
    brk
Harness_Name        .fill 24
Harness_Address     .word 0
Harness_Result      .byte $ff
Harness_StackAfter  .byte 0

; Entered through ($0330) from the KERNAL's LOAD with A = verify flag and
; X/Y = the address (secondary address 0).
Stub_Load
    sta Stub_A
    stx Stub_X
    sty Stub_Y
    lda $9d
    sta Stub_Messages
    lda $01
    sta Stub_Port
    lda Stub_Fail
    lsr                 ; carry = fail
    lda #4              ; FILE NOT FOUND, if failing
    rts
Stub_A        .byte $ff
Stub_X        .byte 0
Stub_Y        .byte 0
Stub_Messages .byte $ff
Stub_Port     .byte 0
Stub_Fail     .byte 0
.include ""{Asm("C64Disk.asm")}""
");

        var psi = new ProcessStartInfo(tass)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var a in new[] { "-o", prgPath, "--long-branch", "--vice-labels", "-l", labelsPath, "--no-monitor", asmPath })
            psi.ArgumentList.Add(a);
        using (var p = Process.Start(psi))
        {
            var output = p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd();
            p.WaitForExit();
            Assert.That(p.ExitCode, Is.EqualTo(0), "64tass failed:\n" + output);
        }

        labels = new Dictionary<string, int>();
        foreach (var line in File.ReadAllLines(labelsPath))
        {
            var parts = line.Split(' ');
            if (parts.Length >= 3)
                labels[parts[2].TrimStart('.')] = Convert.ToInt32(parts[1], 16);
        }
    }

    [SetUp]
    public void LoadHarness()
    {
        emulator = new Emulator();
        emulator.LoadPrg(prgPath);
        var stub = Label("Stub_Load");
        emulator.SetMemory(IloadVector, (byte)(stub & 0xFF), (byte)(stub >> 8));
        emulator.SetMemory(CpuPort, 0x36);           // the program's normal mapping: BASIC out, KERNAL + I/O in
        emulator.SetMemory(MessageFlags, 0x80);      // direct mode: messages on
    }

    int Label(string name) =>
        labels.TryGetValue(name, out var address) ? address : throw new AssertionException($"label {name} missing from the harness");

    // Screen codes, as the compiler assembles string literals (.enc "screen").
    static byte[] ScreenCodes(string text)
    {
        var codes = new byte[text.Length + 1];
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            codes[i] = c switch
            {
                >= 'a' and <= 'z' => (byte)(c - 'a' + 1),
                >= 'A' and <= 'Z' => (byte)c,
                _ => (byte)c,
            };
        }
        return codes;   // ends with 0
    }

    bool RunLoad(string name, int address, int programDevice = 8, bool fail = false)
    {
        emulator.SetMemory(Label("Harness_Name"), ScreenCodes(name));
        emulator.SetMemory(Label("Harness_Address"), (byte)(address & 0xFF), (byte)(address >> 8));
        emulator.SetMemory(Device, (byte)programDevice);
        emulator.SetMemory(Label("Stub_Fail"), (byte)(fail ? 1 : 0));
        emulator.SetProgramCounter(Label("Harness_Load"));
        var stackBefore = emulator.HardwareStackPointer;
        var result = emulator.RunUntil(new HashSet<int>(), 1_000_000, out _, out _);
        Assert.That(result, Is.EqualTo(RunResult.Halted), "Disk_Load didn't return");
        Assert.That(emulator.GetMemory(Label("Harness_StackAfter")), Is.EqualTo(stackBefore), "eval stack unbalanced");
        var value = emulator.GetMemory(Label("Harness_Result"));
        Assert.That(value, Is.EqualTo(0).Or.EqualTo(1), "bool result");
        return value == 1;
    }

    string LoadedName()
    {
        int length = emulator.GetMemory(FileNameLength);
        int pointer = emulator.GetMemory(FileNamePointer) | (emulator.GetMemory(FileNamePointer + 1) << 8);
        var chars = new char[length];
        for (int i = 0; i < length; i++)
            chars[i] = (char)emulator.GetMemory(pointer + i);
        return new string(chars);
    }

    [TestCase("shotroom", "SHOTROOM")]
    [TestCase("SHOTROOM", "SHOTROOM")]
    [TestCase("Pic 2*", "PIC 2*")]
    public void Name_Becomes_Upper_Case_Petscii(string name, string expected)
    {
        Assert.That(RunLoad(name, 0x4000), Is.True);
        Assert.That(LoadedName(), Is.EqualTo(expected));
    }

    [Test]
    public void Name_Is_Cut_At_16_Characters()
    {
        RunLoad("abcdefghijklmnopqrst", 0x4000);
        Assert.That(LoadedName(), Is.EqualTo("ABCDEFGHIJKLMNOP"));
    }

    [Test]
    public void Loads_At_The_Given_Address_With_Secondary_Address_0()
    {
        RunLoad("file", 0x3456);
        Assert.That(emulator.GetMemory(Label("Stub_A")), Is.EqualTo(0), "load, not verify");
        Assert.That(emulator.GetMemory(Label("Stub_X")), Is.EqualTo(0x56));
        Assert.That(emulator.GetMemory(Label("Stub_Y")), Is.EqualTo(0x34));
        Assert.That(emulator.GetMemory(SecondaryAddress), Is.EqualTo(0));
    }

    [TestCase(8, 8)]
    [TestCase(9, 9)]
    [TestCase(1, 8)]    // started from tape: the first drive
    [TestCase(0, 8)]
    public void Uses_The_Programs_Own_Drive(int programDevice, int expected)
    {
        RunLoad("file", 0x4000, programDevice);
        Assert.That(emulator.GetMemory(Device), Is.EqualTo(expected));
    }

    [TestCase(false, true)]
    [TestCase(true, false)]
    public void Returns_Whether_It_Loaded(bool fail, bool expected) =>
        Assert.That(RunLoad("file", 0x4000, fail: fail), Is.EqualTo(expected));

    [Test]
    public void No_Messages_During_The_Load_And_State_Restored()
    {
        RunLoad("file", 0x4000);
        Assert.That(emulator.GetMemory(Label("Stub_Messages")), Is.EqualTo(0), "SEARCHING/LOADING off");
        Assert.That(emulator.GetMemory(Label("Stub_Port")) & 0x06, Is.EqualTo(0x06), "KERNAL and I/O in");
        Assert.That(emulator.GetMemory(MessageFlags), Is.EqualTo(0x80));
        Assert.That(emulator.GetMemory(CpuPort), Is.EqualTo(0x36));
    }
}
