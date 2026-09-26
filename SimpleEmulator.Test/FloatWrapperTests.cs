using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace SimpleEmulator.Test;

// asm/helper/float.asm's Float_Add/Sub/Mul/Div/Compare/FromInt/ToInt load and
// store FAC1 with inline code (flt_load_fac1/flt_store_fac1) instead of the
// ROM's MOVFM/MOVMF. This assembles the REAL float.asm next to reference
// routines that make the same ROM calls the old wrappers did (MOVFM ... op ...
// MOVMF) and checks both give byte-identical results on random and edge-case
// operands. Skipped when 64tass isn't installed (TASS_EXE, else the repo's
// usual c:\tools path).
[TestFixture]
public class FloatWrapperTests
{
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

        var dir = Path.Combine(Path.GetTempPath(), "c64_float_harness");
        Directory.CreateDirectory(dir);
        var asmPath = Path.Combine(dir, "harness.asm");
        prgPath = Path.Combine(dir, "harness.prg");
        var labelsPath = Path.Combine(dir, "harness.labels");

        string Asm(string relative) => Path.Combine(repoRoot, "asm", relative).Replace('\\', '/');
        File.WriteAllText(asmPath, $@"
.include ""{Asm("helper/zeropage.asm")}""
.include ""{Asm("helper/stack.asm")}""
.include ""{Asm("helper/floatBanking.asm")}""
.include ""{Asm("helper/8bit.asm")}""
tostring_buffer = $0200
* = $1000
Harness_Add:     jsr Float_Add
    brk
Harness_Sub:     jsr Float_Sub
    brk
Harness_Mul:     jsr Float_Mul
    brk
Harness_Div:     jsr Float_Div
    brk
Harness_Compare: jsr Float_Compare
    brk
Harness_FromInt: jsr Float_FromInt
    brk
Harness_ToInt:   jsr Float_ToInt
    brk
Ref_Add:     jsr Ref_Op_Add
    brk
Ref_Sub:     jsr Ref_Op_Sub
    brk
Ref_Mul:     jsr Ref_Op_Mul
    brk
Ref_Div:     jsr Ref_Op_Div
    brk
Ref_Compare: jsr Ref_Op_Compare
    brk
Ref_FromInt: jsr Ref_Op_FromInt
    brk
Ref_ToInt:   jsr Ref_Op_ToInt
    brk

; What the wrappers did before the inline load/store: the ROM's own MOVFM/MOVMF.
Ref_Op_Add
    #bank_in_basic_rom
    lda #<zp_flt_a
    ldy #>zp_flt_a
    jsr MOVFM
    lda #<zp_flt_b
    ldy #>zp_flt_b
    jsr FADD
    ldx #<zp_flt_a
    ldy #>zp_flt_a
    jsr MOVMF
    #bank_out_basic_rom
    rts
Ref_Op_Sub
    #bank_in_basic_rom
    lda #<zp_flt_b
    ldy #>zp_flt_b
    jsr MOVFM
    lda #<zp_flt_a
    ldy #>zp_flt_a
    jsr FSUB
    ldx #<zp_flt_a
    ldy #>zp_flt_a
    jsr MOVMF
    #bank_out_basic_rom
    rts
Ref_Op_Mul
    #bank_in_basic_rom
    lda #<zp_flt_a
    ldy #>zp_flt_a
    jsr MOVFM
    lda #<zp_flt_b
    ldy #>zp_flt_b
    jsr FMULT
    ldx #<zp_flt_a
    ldy #>zp_flt_a
    jsr MOVMF
    #bank_out_basic_rom
    rts
Ref_Op_Div
    #bank_in_basic_rom
    lda #<zp_flt_b
    ldy #>zp_flt_b
    jsr MOVFM
    lda #<zp_flt_a
    ldy #>zp_flt_a
    jsr FDIV
    ldx #<zp_flt_a
    ldy #>zp_flt_a
    jsr MOVMF
    #bank_out_basic_rom
    rts
Ref_Op_Compare
    #bank_in_basic_rom
    lda #<zp_flt_a
    ldy #>zp_flt_a
    jsr MOVFM
    lda #<zp_flt_b
    ldy #>zp_flt_b
    jsr FCOMP
    #bank_out_basic_rom
    rts
Ref_Op_FromInt
    #bank_in_basic_rom
    lda zp_flt_int_hi
    ldy zp_flt_int_lo
    jsr GIVAYF
    ldx #<zp_flt_a
    ldy #>zp_flt_a
    jsr MOVMF
    #bank_out_basic_rom
    rts
Ref_Op_ToInt
    #bank_in_basic_rom
    lda #<zp_flt_a
    ldy #>zp_flt_a
    jsr MOVFM
    jsr FAC1YA
    #bank_out_basic_rom
    tya
    rts

.include ""{Asm("helper/float.asm")}""
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
        foreach (var line in File.ReadAllLines(Path.Combine(repoRoot, "asm", "helper", "zeropage.asm")))
        {
            var m = Regex.Match(line, @"^(zp_flt_\w+)\s*=\s*\$([0-9a-fA-F]+)");
            if (m.Success)
                labels[m.Groups[1].Value] = Convert.ToInt32(m.Groups[2].Value, 16);
        }
    }

    [SetUp]
    public void LoadHarness()
    {
        emulator = new Emulator();
        emulator.LoadPrg(prgPath);
    }

    int Label(string name) =>
        labels.TryGetValue(name, out var address) ? address : throw new AssertionException($"label {name} missing from the harness");

    // Runs one entry point on fresh operands; returns zp_flt_a's 5 bytes and A.
    (byte[] result, byte a) Run(string entry, byte[] a, byte[] b, int intValue = 0)
    {
        emulator = new Emulator();
        emulator.LoadPrg(prgPath);
        emulator.SetMemory(Label("zp_flt_a"), a);
        if (b != null)
            emulator.SetMemory(Label("zp_flt_b"), b);
        if (entry.EndsWith("FromInt"))
            // zp_flt_int_lo = zp_flt_a, zp_flt_int_hi = zp_flt_a + 1 (zeropage.asm)
            emulator.SetMemory(Label("zp_flt_a"), (byte)intValue, (byte)(intValue >> 8));
        emulator.SetProgramCounter(Label(entry));
        var run = emulator.RunUntil(new HashSet<int>(), 200_000, out _, out _);
        Assert.That(run, Is.EqualTo(RunResult.Halted), $"{entry} didn't finish");
        var result = new byte[5];
        for (int i = 0; i < 5; i++)
            result[i] = emulator.GetMemory(Label("zp_flt_a") + i);
        return (result, emulator.Registers.A);
    }

    static string Hex(byte[] v) => BitConverter.ToString(v);

    void AssertSame(string op, byte[] a, byte[] b, int intValue = 0)
    {
        var (newResult, newA) = Run("Harness_" + op, a, b, intValue);
        var (refResult, refA) = Run("Ref_" + op, a, b, intValue);
        Assert.That(Hex(newResult), Is.EqualTo(Hex(refResult)), $"{op}: a={Hex(a)} b={(b == null ? "-" : Hex(b))} int={intValue}");
        // A only carries a result for Compare (FCOMP's sign) and ToInt (low byte);
        // for the arithmetic ops it is unspecified.
        if (op is "Compare" or "ToInt")
            Assert.That(newA, Is.EqualTo(refA), $"{op} A register: a={Hex(a)} b={(b == null ? "-" : Hex(b))}");
    }

    static byte[] Random5(Random r, int minExp, int maxExp)
    {
        var v = new byte[5];
        r.NextBytes(v);
        v[0] = (byte)r.Next(minExp, maxExp + 1);
        return v;
    }

    static readonly byte[] Zero = { 0, 0, 0, 0, 0 };

    [TestCase("Add")]
    [TestCase("Sub")]
    [TestCase("Mul")]
    public void Arithmetic_Matches_ROM_Path(string op)
    {
        var r = new Random(1234);
        for (int i = 0; i < 400; i++)
            AssertSame(op, Random5(r, 0x70, 0x90), Random5(r, 0x70, 0x90));
        // Widely different magnitudes (exercises alignment shifts and rounding).
        for (int i = 0; i < 100; i++)
            AssertSame(op, Random5(r, 0x40, 0x60), Random5(r, 0x90, 0xA0));
        // Zeros and identical operands.
        var x = Random5(r, 0x7f, 0x85);
        AssertSame(op, Zero, x);
        AssertSame(op, x, Zero);
        AssertSame(op, Zero, Zero);
        AssertSame(op, x, x);
    }

    [Test]
    public void Div_Matches_ROM_Path()
    {
        var r = new Random(99);
        for (int i = 0; i < 300; i++)
            AssertSame("Div", Random5(r, 0x70, 0x90), Random5(r, 0x70, 0x90));
        AssertSame("Div", Zero, Random5(r, 0x7f, 0x85));
    }

    [Test]
    public void Compare_Matches_ROM_Path()
    {
        var r = new Random(7);
        for (int i = 0; i < 300; i++)
            AssertSame("Compare", Random5(r, 0x70, 0x90), Random5(r, 0x70, 0x90));
        var x = Random5(r, 0x7f, 0x85);
        AssertSame("Compare", x, x);
        AssertSame("Compare", Zero, x);
        AssertSame("Compare", x, Zero);
    }

    [Test]
    public void FromInt_Matches_ROM_Path()
    {
        var r = new Random(5);
        foreach (var v in new[] { 0, 1, -1, 255, 256, 32767, -32768, 12345, -12345 })
            AssertSame("FromInt", Zero, null, v);
        for (int i = 0; i < 200; i++)
            AssertSame("FromInt", Zero, null, r.Next(-32768, 32768));
    }

    [Test]
    public void ToInt_Matches_ROM_Path()
    {
        var r = new Random(3);
        for (int i = 0; i < 300; i++)
            AssertSame("ToInt", Random5(r, 0x70, 0x8e), null);
        AssertSame("ToInt", Zero, null);
    }
}
