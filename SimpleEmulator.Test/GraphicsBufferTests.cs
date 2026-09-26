using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace SimpleEmulator.Test;

// Double buffering in asm/C64Graphics.asm (Screen.SetDrawBuffer, SwapBuffers,
// SetBitmapColors, and how EnableBitmapMode/DisableBitmapMode treat the second
// buffer), assembled with 64tass into a harness around the REAL source file
// with GRAPHICS_DOUBLE_BUFFER = 1, at the addresses ProgramEntry.asm gives the
// buffers. SimpleEmulator has no VIC-II, so what is checked is exactly what the
// routines write: pixel bytes (which buffer they land in) and the $D018/$DD00
// values that make the VIC show a buffer; whether the picture really flips is
// only observable on VICE. Skipped when 64tass isn't installed (TASS_EXE, else
// the repo's usual c:\tools path).
[TestFixture]
public class GraphicsBufferTests
{
    const int Bitmap0 = 0x2000;
    const int Bitmap1 = 0x4000;
    const int Matrix0 = 0x0C00;
    const int Matrix1 = 0x6000;
    const int BitmapBytes = 8000;
    const int MatrixBytes = 1000;

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

        var dir = Path.Combine(Path.GetTempPath(), "c64_buffer_harness");
        Directory.CreateDirectory(dir);
        var asmPath = Path.Combine(dir, "harness.asm");
        prgPath = Path.Combine(dir, "harness.prg");
        var labelsPath = Path.Combine(dir, "harness.labels");

        string Asm(string relative) => Path.Combine(repoRoot, "asm", relative).Replace('\\', '/');
        File.WriteAllText(asmPath, $@"
.include ""{Asm("helper/zeropage.asm")}""
.include ""{Asm("helper/stack.asm")}""
.include ""{Asm("helper/8bit.asm")}""
Flag_Screen_SetPixel = 1
Flag_Screen_DrawLine = 1
Flag_Screen_EnableBitmapMode = 1
Flag_Screen_DisableBitmapMode = 1
Flag_Screen_SetDrawBuffer = 1
Flag_Screen_SwapBuffers = 1
Flag_Screen_SetBitmapColors = 1
GRAPHICS_DOUBLE_BUFFER = 1
Graphics_ColorMatrix = ${Matrix0:x4}
Graphics_Bitmap = ${Bitmap0:x4}
Graphics_Bitmap2 = ${Bitmap1:x4}
Graphics_ColorMatrix2 = ${Matrix1:x4}
* = $1000
Harness_Pixel
    jsr Graphics_SetPixel_Core
    brk
Harness_Line
    jsr Graphics_DrawLine_Core
    brk
Harness_Enable
    jsr Screen_EnableBitmapMode
    brk
Harness_Disable
    jsr Screen_DisableBitmapMode
    brk
Harness_Swap
    jsr Screen_SwapBuffers
    brk
Harness_DrawBuffer0
    lda #0
    pha
    jsr Screen_SetDrawBuffer
    brk
Harness_DrawBuffer1
    lda #1
    pha
    jsr Screen_SetDrawBuffer
    brk
Harness_Colors
    lda #$1e
    pha
    jsr Screen_SetBitmapColors
    brk
.include ""{Asm("C64Graphics.asm")}""
Harness_End
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
            var m = Regex.Match(line, @"^(zp_gfx_\w+)\s*=\s*\$([0-9a-fA-F]+)");
            if (m.Success)
                labels[m.Groups[1].Value] = Convert.ToInt32(m.Groups[2].Value, 16);
        }

        // The harness code must end below the first bitmap, or it would be
        // sitting on top of the memory under test.
        Assert.That(labels["Harness_End"], Is.LessThan(Bitmap0), "harness code overlaps the bitmap");
    }

    [SetUp]
    public void LoadHarness()
    {
        emulator = new Emulator();
        emulator.LoadPrg(prgPath);
    }

    int Label(string name) =>
        labels.TryGetValue(name, out var address) ? address : throw new AssertionException($"label {name} missing from the harness");

    void Run(string entry)
    {
        emulator.SetProgramCounter(Label(entry));
        var result = emulator.RunUntil(new HashSet<int>(), 5_000_000, out _, out _);
        Assert.That(result, Is.EqualTo(RunResult.Halted), $"{entry} did not finish");
    }

    void SetPixel(int x, int y)
    {
        emulator.SetMemory(Label("zp_gfx_x_low"), (byte)(x & 0xff), (byte)(x >> 8));
        emulator.SetMemory(Label("zp_gfx_y"), (byte)y);
        emulator.SetMemory(Label("zp_gfx_on"), 1);
        Run("Harness_Pixel");
    }

    void DrawLine(int x0, int y0, int x1, int y1)
    {
        emulator.SetMemory(Label("zp_gfx_x_low"), (byte)(x0 & 0xff), (byte)(x0 >> 8));
        emulator.SetMemory(Label("zp_gfx_y"), (byte)y0);
        emulator.SetMemory(Label("zp_gfx_endx_low"), (byte)(x1 & 0xff), (byte)(x1 >> 8));
        emulator.SetMemory(Label("zp_gfx_endy"), (byte)y1);
        emulator.SetMemory(Label("zp_gfx_on"), 1);
        Run("Harness_Line");
    }

    void Fill(int address, int count, byte value) => emulator.SetMemory(address, Enumerable.Repeat(value, count).ToArray());

    void ClearBuffers()
    {
        Fill(Bitmap0, BitmapBytes, 0);
        Fill(Bitmap1, BitmapBytes, 0);
    }

    // Pixel (x,y): byte (y/8)*320 + (x&~7) + (y&7), bit 7-(x&7).
    bool Pixel(int bitmap, int x, int y)
    {
        var offset = (y / 8) * 320 + (x & ~7) + (y & 7);
        return (emulator.GetMemory(bitmap + offset) & (0x80 >> (x & 7))) != 0;
    }

    HashSet<(int, int)> SetPixels(int bitmap)
    {
        var result = new HashSet<(int, int)>();
        for (int y = 0; y < 200; y++)
            for (int x = 0; x < 320; x++)
                if (Pixel(bitmap, x, y))
                    result.Add((x, y));
        return result;
    }

    bool AllEqual(int address, int count, byte value)
    {
        for (int i = 0; i < count; i++)
            if (emulator.GetMemory(address + i) != value)
                return false;
        return true;
    }

    [Test]
    public void Pixels_Go_Into_Buffer_0_By_Default()
    {
        ClearBuffers();
        SetPixel(100, 50);
        Assert.That(Pixel(Bitmap0, 100, 50), Is.True);
        Assert.That(SetPixels(Bitmap1), Is.Empty);
    }

    [Test]
    public void SetDrawBuffer_1_Redirects_Pixels_To_Buffer_1_Only()
    {
        ClearBuffers();
        Run("Harness_DrawBuffer1");
        SetPixel(100, 50);
        SetPixel(319, 199);
        Assert.That(SetPixels(Bitmap1), Is.EquivalentTo(new[] { (100, 50), (319, 199) }));
        Assert.That(SetPixels(Bitmap0), Is.Empty);
    }

    [Test]
    public void SetDrawBuffer_0_Goes_Back_To_Buffer_0()
    {
        ClearBuffers();
        Run("Harness_DrawBuffer1");
        Run("Harness_DrawBuffer0");
        SetPixel(7, 7);
        Assert.That(Pixel(Bitmap0, 7, 7), Is.True);
        Assert.That(SetPixels(Bitmap1), Is.Empty);
    }

    [TestCase(0, 0, 319, 199)]
    [TestCase(10, 150, 300, 20)]
    [TestCase(200, 5, 40, 190)]
    [TestCase(0, 99, 319, 99)]
    public void A_Line_Drawn_Into_Buffer_1_Is_The_Same_Line_As_In_Buffer_0(int x0, int y0, int x1, int y1)
    {
        ClearBuffers();
        DrawLine(x0, y0, x1, y1);
        var inBuffer0 = SetPixels(Bitmap0);
        Assert.That(inBuffer0, Is.Not.Empty);
        Assert.That(SetPixels(Bitmap1), Is.Empty);

        ClearBuffers();
        Run("Harness_DrawBuffer1");
        DrawLine(x0, y0, x1, y1);
        Assert.That(SetPixels(Bitmap1), Is.EquivalentTo(inBuffer0));
        Assert.That(SetPixels(Bitmap0), Is.Empty);
    }

    [Test]
    public void SwapBuffers_Shows_The_Drawn_Buffer_And_Alternates_The_Draw_Target()
    {
        ClearBuffers();
        Run("Harness_Enable");

        // Drawing into buffer 0; the swap shows it and moves drawing to buffer 1.
        Run("Harness_Swap");
        Assert.That(emulator.GetMemory(0xD018), Is.EqualTo(0x38));
        Assert.That(emulator.GetMemory(0xDD00) & 3, Is.EqualTo(3), "bank 0");
        SetPixel(1, 1);
        Assert.That(SetPixels(Bitmap1), Is.EquivalentTo(new[] { (1, 1) }));

        // Buffer 1 was drawn into: show it, draw into buffer 0 again.
        Run("Harness_Swap");
        Assert.That(emulator.GetMemory(0xD018), Is.EqualTo(0x80));
        Assert.That(emulator.GetMemory(0xDD00) & 3, Is.EqualTo(2), "bank 1");
        SetPixel(2, 2);
        Assert.That(SetPixels(Bitmap0), Is.EquivalentTo(new[] { (2, 2) }));

        Run("Harness_Swap");
        Assert.That(emulator.GetMemory(0xD018), Is.EqualTo(0x38));
        Assert.That(emulator.GetMemory(0xDD00) & 3, Is.EqualTo(3));
    }

    [Test]
    public void SwapBuffers_Only_Touches_The_VIC_Bank_Bits_Of_DD00()
    {
        Run("Harness_Enable");
        foreach (var upper in new byte[] { 0xA4, 0x5C, 0xFC, 0x00 })
        {
            emulator.SetMemory(0xDD00, (byte)(upper | 3));
            Run("Harness_DrawBuffer1");
            Run("Harness_Swap");       // shows buffer 1 -> bank 1
            Assert.That(emulator.GetMemory(0xDD00), Is.EqualTo(upper | 2), $"upper bits {upper:X2}, to bank 1");
            Run("Harness_Swap");       // shows buffer 0 -> bank 0
            Assert.That(emulator.GetMemory(0xDD00), Is.EqualTo(upper | 3), $"upper bits {upper:X2}, to bank 0");
            Run("Harness_DrawBuffer0");
        }
    }

    [Test]
    public void EnableBitmapMode_Clears_Both_Buffers_And_Both_Color_Matrices_And_Resets_State()
    {
        // Poison everything, and leave the draw target on buffer 1 and the VIC on bank 1.
        Fill(Bitmap0, BitmapBytes + 64, 0xFF);
        Fill(Bitmap1, BitmapBytes + 64, 0xFF);
        Fill(Matrix0, MatrixBytes + 24, 0xFF);
        Fill(Matrix1, MatrixBytes + 24, 0xFF);
        Run("Harness_DrawBuffer1");
        emulator.SetMemory(0xDD00, 0x02);

        Run("Harness_Enable");

        Assert.That(AllEqual(Bitmap0, BitmapBytes, 0), "bitmap 0");
        Assert.That(AllEqual(Bitmap1, BitmapBytes, 0), "bitmap 1");
        Assert.That(AllEqual(Matrix0, MatrixBytes, 0), "matrix 0");
        Assert.That(AllEqual(Matrix1, MatrixBytes, 0), "matrix 1");
        // ... and nothing past their ends.
        Assert.That(AllEqual(Bitmap0 + BitmapBytes, 64, 0xFF), "past bitmap 0");
        Assert.That(AllEqual(Bitmap1 + BitmapBytes, 64, 0xFF), "past bitmap 1");
        Assert.That(AllEqual(Matrix0 + MatrixBytes, 24, 0xFF), "past matrix 0");
        Assert.That(AllEqual(Matrix1 + MatrixBytes, 24, 0xFF), "past matrix 1");

        Assert.That(emulator.GetMemory(0xD011) & 0x20, Is.EqualTo(0x20), "bitmap mode on");
        Assert.That(emulator.GetMemory(0xD018), Is.EqualTo(0x38));
        Assert.That(emulator.GetMemory(0xDD00) & 3, Is.EqualTo(3), "back on bank 0");
        SetPixel(3, 3);
        Assert.That(SetPixels(Bitmap0), Is.EquivalentTo(new[] { (3, 3) }), "draw target reset to buffer 0");
    }

    [Test]
    public void DisableBitmapMode_Returns_To_VIC_Bank_0()
    {
        Run("Harness_Enable");
        Run("Harness_DrawBuffer1");
        Run("Harness_Swap");           // now showing buffer 1, in bank 1
        Assert.That(emulator.GetMemory(0xDD00) & 3, Is.EqualTo(2));

        Run("Harness_Disable");

        Assert.That(emulator.GetMemory(0xDD00) & 3, Is.EqualTo(3));
        Assert.That(emulator.GetMemory(0xD011) & 0x20, Is.EqualTo(0), "bitmap mode off");
    }

    [Test]
    public void SetBitmapColors_Fills_Both_Color_Matrices_And_Nothing_Else()
    {
        Fill(Matrix0, MatrixBytes + 24, 0xFF);
        Fill(Matrix1, MatrixBytes + 24, 0xFF);

        Run("Harness_Colors");

        Assert.That(AllEqual(Matrix0, MatrixBytes, 0x1E), "matrix 0");
        Assert.That(AllEqual(Matrix1, MatrixBytes, 0x1E), "matrix 1");
        Assert.That(AllEqual(Matrix0 + MatrixBytes, 24, 0xFF), "past matrix 0");
        Assert.That(AllEqual(Matrix1 + MatrixBytes, 24, 0xFF), "past matrix 1");
    }
}
