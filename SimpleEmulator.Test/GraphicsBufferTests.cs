using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace SimpleEmulator.Test;

// Double buffering in asm/C64Graphics.asm (Screen.SetDrawBuffer, SwapBuffers,
// SetBitmapColors, and how Screen.SetScreenMode(Bitmap/Character) treats the
// second buffer), assembled with 64tass into a harness around the REAL source
// file with GRAPHICS_DOUBLE_BUFFER = 1, at the addresses ProgramEntry.asm
// gives the buffers. SimpleEmulator has no VIC-II, so what is checked is
// exactly what the routines write: pixel bytes (which buffer they land in)
// and the $D018/$DD00/$D016 values that make the VIC show a buffer/format;
// whether the picture really flips (or multicolor really renders) is only
// observable on VICE. Skipped when 64tass isn't installed (TASS_EXE, else
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
Flag_Screen_SetScreenMode = 1
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
Harness_Bitmap
    lda #1
    pha
    jsr Screen_SetScreenMode
    brk
Harness_MultiColor
    lda #2
    pha
    jsr Screen_SetScreenMode
    brk
Harness_Character
    lda #0
    pha
    jsr Screen_SetScreenMode
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

    void SetPixel(int x, int y, int colorSource = 3)
    {
        emulator.SetMemory(Label("zp_gfx_x_low"), (byte)(x & 0xff), (byte)(x >> 8));
        emulator.SetMemory(Label("zp_gfx_y"), (byte)y);
        emulator.SetMemory(Label("zp_gfx_on"), 1);
        emulator.SetMemory(Label("zp_gfx_color"), (byte)colorSource);
        Run("Harness_Pixel");
    }

    void ClearPixel(int x, int y, int colorSource = 3)
    {
        emulator.SetMemory(Label("zp_gfx_x_low"), (byte)(x & 0xff), (byte)(x >> 8));
        emulator.SetMemory(Label("zp_gfx_y"), (byte)y);
        emulator.SetMemory(Label("zp_gfx_on"), 0);
        emulator.SetMemory(Label("zp_gfx_color"), (byte)colorSource);
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

    // Multicolor: same byte address as Pixel above, but each byte holds 4
    // 2-bit pairs (76/54/32/10, MSB-first) instead of 8 single bits -- x and
    // x+1 always share a pair. Returns the pair's raw 2-bit value (0-3).
    int PixelPair(int bitmap, int x, int y)
    {
        var offset = (y / 8) * 320 + (x & ~7) + (y & 7);
        var b = emulator.GetMemory(bitmap + offset);
        var shift = 6 - ((x & 6) >> 1) * 2;
        return (b >> shift) & 3;
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
        Run("Harness_Bitmap");

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
        Run("Harness_Bitmap");
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
    public void SetScreenMode_Bitmap_Clears_Both_Buffers_And_Both_Color_Matrices_And_Resets_State()
    {
        // Poison everything, and leave the draw target on buffer 1 and the VIC on bank 1.
        Fill(Bitmap0, BitmapBytes + 64, 0xFF);
        Fill(Bitmap1, BitmapBytes + 64, 0xFF);
        Fill(Matrix0, MatrixBytes + 24, 0xFF);
        Fill(Matrix1, MatrixBytes + 24, 0xFF);
        Run("Harness_DrawBuffer1");
        emulator.SetMemory(0xDD00, 0x02);

        Run("Harness_Bitmap");

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
    public void SetScreenMode_Character_Returns_To_VIC_Bank_0()
    {
        Run("Harness_Bitmap");
        Run("Harness_DrawBuffer1");
        Run("Harness_Swap");           // now showing buffer 1, in bank 1
        Assert.That(emulator.GetMemory(0xDD00) & 3, Is.EqualTo(2));

        Run("Harness_Character");

        Assert.That(emulator.GetMemory(0xDD00) & 3, Is.EqualTo(3));
        Assert.That(emulator.GetMemory(0xD011) & 0x20, Is.EqualTo(0), "bitmap mode off");
    }

    [Test]
    public void SetScreenMode_Bitmap_Leaves_D016_Untouched_Except_MCM()
    {
        // A Bitmap-only caller's other $d016 bits (screen width/scroll-X)
        // must survive -- SetScreenMode(Bitmap) only ever clears MCM (a
        // safe, idempotent RMW), never restores a saved byte over them.
        emulator.SetMemory(0xD016, 0xE8); // arbitrary non-MCM bits set, MCM off
        Run("Harness_Bitmap");
        Assert.That(emulator.GetMemory(0xD016), Is.EqualTo(0xE8), "other bits preserved, MCM still off");

        emulator.SetMemory(0xD016, 0xF8); // same, but MCM already on somehow
        Run("Harness_Bitmap");
        Assert.That(emulator.GetMemory(0xD016), Is.EqualTo(0xE8), "MCM cleared, other bits preserved");
    }

    [Test]
    public void SetScreenMode_Character_Without_MultiColor_Leaves_D016_Untouched()
    {
        // The old DisableBitmapMode never touched $d016 at all -- a program
        // that only ever used Bitmap (never MultiColor) must see the exact
        // same behavior, not a stray restore-to-zero.
        emulator.SetMemory(0xD016, 0xC8);
        Run("Harness_Bitmap");
        emulator.SetMemory(0xD016, 0xAB); // simulate something else changing it meanwhile
        Run("Harness_Character");
        Assert.That(emulator.GetMemory(0xD016), Is.EqualTo(0xAB), "untouched -- MultiColor was never entered");
    }

    [Test]
    public void SetScreenMode_Character_After_MultiColor_Restores_D016()
    {
        emulator.SetMemory(0xD016, 0xC8); // arbitrary non-MCM bits, MCM off
        Run("Harness_MultiColor");
        Assert.That(emulator.GetMemory(0xD016), Is.EqualTo(0xD8), "MCM on, other bits preserved");

        Run("Harness_Character");
        Assert.That(emulator.GetMemory(0xD016), Is.EqualTo(0xC8), "fully restored, including MCM back off");
    }

    [Test]
    public void MultiColorSetPixel_Sets_Correct_Bits_For_Each_Pair_Position()
    {
        // x and x+1 always share a pair -- one pixel set per pair (x=0,2,4,6)
        // within a single byte (y=0, cell row 0), each with a distinct
        // colorSource, confirms both the right pair position AND that
        // setting one pair's bits doesn't disturb any other pair's.
        ClearBuffers();
        Run("Harness_MultiColor");
        SetPixel(0, 0, colorSource: 1);
        SetPixel(2, 0, colorSource: 2);
        SetPixel(4, 0, colorSource: 3);
        SetPixel(6, 0, colorSource: 1);

        Assert.That(PixelPair(Bitmap0, 0, 0), Is.EqualTo(1));
        Assert.That(PixelPair(Bitmap0, 1, 0), Is.EqualTo(1), "x+1 shares x's pair");
        Assert.That(PixelPair(Bitmap0, 2, 0), Is.EqualTo(2));
        Assert.That(PixelPair(Bitmap0, 4, 0), Is.EqualTo(3));
        Assert.That(PixelPair(Bitmap0, 6, 0), Is.EqualTo(1));
    }

    [Test]
    public void MultiColorSetPixel_Encodes_All_Four_ColorSource_Values()
    {
        ClearBuffers();
        Run("Harness_MultiColor");
        for (int colorSource = 0; colorSource <= 3; colorSource++)
        {
            SetPixel(0, 0, colorSource);
            Assert.That(PixelPair(Bitmap0, 0, 0), Is.EqualTo(colorSource));
        }
    }

    [Test]
    public void MultiColorSetPixel_Off_Clears_To_Background_Regardless_Of_Prior_Color()
    {
        ClearBuffers();
        Run("Harness_MultiColor");
        SetPixel(0, 0, colorSource: 3);
        Assert.That(PixelPair(Bitmap0, 0, 0), Is.EqualTo(3));

        ClearPixel(0, 0, colorSource: 2); // colorSource ignored when clearing
        Assert.That(PixelPair(Bitmap0, 0, 0), Is.EqualTo(0));
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
