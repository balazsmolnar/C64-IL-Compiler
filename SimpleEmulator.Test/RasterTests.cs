using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace SimpleEmulator.Test;

// The emulator's raster model (every program read of $D012 advances the beam
// one line, $D011 bit 7 = line bit 8) and asm/C64Graphics.asm's
// Screen_WaitForVBlank, which polls it. The real source file is assembled, so this
// tests what the compiler links. The routine tests are skipped when 64tass
// isn't installed (TASS_EXE, else the repo's usual c:\tools path).
[TestFixture]
public class RasterTests
{
    const int CodeStart = 0x0900;

    [Test]
    public void D012_Read_Advances_The_Beam_One_Line_And_Wraps()
    {
        // ldx #0 / loop: lda $d012 / sta $0a00,x / inx / bne loop / brk
        var code = new byte[]
        {
            0xA2, 0x00,
            0xAD, 0x12, 0xD0,
            0x9D, 0x00, 0x0A,
            0xE8,
            0xD0, 0xF7,
            0x00,
        };
        var emulator = new Emulator();
        emulator.SetMemory(CodeStart, code);
        emulator.Start(CodeStart);

        for (int i = 0; i < 256; i++)
            Assert.That(emulator.GetMemory(0x0A00 + i), Is.EqualTo((i + 1) & 0xff), $"read {i}");
        Assert.That(emulator.RasterLine, Is.EqualTo(256));
    }

    [Test]
    public void D011_Bit7_Is_Raster_Bit8()
    {
        // lda $d011 / sta $0a00 / brk, with the beam moved past line 255 first
        var emulator = new Emulator();
        var read = new byte[] { 0xAD, 0x11, 0xD0, 0x8D, 0x00, 0x0A, 0x00 };

        emulator.SetMemory(CodeStart, read);
        emulator.Start(CodeStart);
        Assert.That(emulator.GetMemory(0x0A00) & 0x80, Is.EqualTo(0), "line 0");

        // 256 reads of $d012 -> line 256
        var advance = new byte[]
        {
            0xA2, 0x00, 0xAD, 0x12, 0xD0, 0xE8, 0xD0, 0xFA, 0x00,
        };
        emulator.SetMemory(CodeStart, advance);
        emulator.Start(CodeStart);
        Assert.That(emulator.RasterLine, Is.EqualTo(256));

        emulator.SetMemory(CodeStart, read);
        emulator.Start(CodeStart);
        Assert.That(emulator.GetMemory(0x0A00) & 0x80, Is.EqualTo(0x80), "line 256");
    }

    [Test]
    public void GetMemory_Does_Not_Move_The_Beam()
    {
        var emulator = new Emulator();
        for (int i = 0; i < 10; i++)
            emulator.GetMemory(0xD012);
        Assert.That(emulator.RasterLine, Is.EqualTo(0));
    }

    // --- Screen_WaitForVBlank, assembled from asm/C64Graphics.asm ---

    static string prgPath;
    static Dictionary<string, int> labels;

    static void AssembleHarness()
    {
        if (prgPath != null)
            return;
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        var tass = Environment.GetEnvironmentVariable("TASS_EXE") ?? @"c:\tools\64tass-1.60.3243\64tass.exe";
        if (!File.Exists(tass))
            Assert.Ignore($"64tass not found at {tass} (set TASS_EXE).");

        var dir = Path.Combine(Path.GetTempPath(), "c64_raster_harness");
        Directory.CreateDirectory(dir);
        var asmPath = Path.Combine(dir, "harness.asm");
        var prg = Path.Combine(dir, "harness.prg");
        var labelsPath = Path.Combine(dir, "harness.labels");
        string Asm(string relative) => Path.Combine(repoRoot, "asm", relative).Replace('\\', '/');
        File.WriteAllText(asmPath, $@"
.include ""{Asm("helper/zeropage.asm")}""
.include ""{Asm("helper/stack.asm")}""
Flag_Screen_WaitForVBlank = 1
* = $1000
Harness_Once
    jsr Screen_WaitForVBlank
    brk
Harness_Twice
    jsr Screen_WaitForVBlank
    jsr Screen_WaitForVBlank
    brk
.include ""{Asm("C64Graphics.asm")}""
");
        var psi = new ProcessStartInfo(tass)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var a in new[] { "-o", prg, "--long-branch", "--vice-labels", "-l", labelsPath, "--no-monitor", asmPath })
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
        prgPath = prg;
    }

    static (Emulator emulator, long steps) RunHarness(string entry)
    {
        AssembleHarness();
        var emulator = new Emulator();
        emulator.LoadPrg(prgPath);
        emulator.SetProgramCounter(labels[entry]);
        var result = emulator.RunUntil(new HashSet<int>(), 100_000, out _, out var steps);
        Assert.That(result, Is.EqualTo(RunResult.Halted), $"{entry} did not finish");
        return (emulator, steps);
    }

    [Test]
    public void WaitForVBlank_Returns_With_The_Beam_On_Line_251()
    {
        var (emulator, _) = RunHarness("Harness_Once");
        Assert.That(emulator.RasterLine, Is.EqualTo(0xFB));
    }

    [Test]
    public void WaitForVBlank_Called_Twice_Waits_A_Whole_Frame_The_Second_Time()
    {
        var (once, onceSteps) = RunHarness("Harness_Once");
        var (twice, twiceSteps) = RunHarness("Harness_Twice");
        Assert.That(twice.RasterLine, Is.EqualTo(0xFB));
        // A whole frame is 312 polls of 3 instructions each; a second call
        // that returned immediately (beam still on line 251) would add ~3.
        Assert.That(twiceSteps - onceSteps, Is.GreaterThan(3 * (Emulator.RasterLines - 5)));
    }
}
