using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace SimpleEmulator.Test;

// Runs asm/C64Graphics.asm's Graphics_HLine_Core directly on SimpleEmulator
// and checks its byte-level fast fill (the interior-byte blit added to
// speed up Screen.DrawRectangle/DrawCircle's filled mode -- see the
// partitioned-doodling-sun plan) against a plain reference: every pixel in
// [x0,x1] at row y, for both hi-res and multicolor, especially at
// byte-boundary-crossing spans, since that's exactly where a head/
// interior/tail off-by-one would show up. Assembled with 64tass into a
// tiny harness around the REAL source file, so this tests what the
// compiler actually links. Skipped when 64tass isn't installed (TASS_EXE,
// else the repo's usual c:\tools path).
[TestFixture]
public class GraphicsFillTests
{
    const int BitmapAddress = 0x2000;
    const int BitmapBytes = 8000;

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

        var dir = Path.Combine(Path.GetTempPath(), "c64_fill_harness");
        Directory.CreateDirectory(dir);
        var asmPath = Path.Combine(dir, "harness.asm");
        prgPath = Path.Combine(dir, "harness.prg");
        var labelsPath = Path.Combine(dir, "harness.labels");

        string Asm(string relative) => Path.Combine(repoRoot, "asm", relative).Replace('\\', '/');
        File.WriteAllText(asmPath, $@"
.include ""{Asm("helper/zeropage.asm")}""
.include ""{Asm("helper/stack.asm")}""
.include ""{Asm("helper/8bit.asm")}""
Flag_Screen_DrawRectangle = 1
Flag_Screen_DrawLine = 1
Flag_Screen_DrawCircle = 1
Flag_Screen_DrawTrapezoid = 1
Flag_Screen_ClearBitmap = 1
Graphics_Bitmap = ${BitmapAddress:x4}
* = $1000
.include ""{Asm("helper/division.asm")}""
Harness_HLine
    jsr Graphics_HLine_Core
    brk
Harness_FillRect
    jsr Graphics_FillRect_Core
    brk
Harness_SetPixel
    jsr Graphics_SetPixel_Core
    brk
Harness_Outline
    jsr Graphics_RectOutline_Core
    brk
Harness_Clear
    lda Harness_ClearPage
    jsr Graphics_ClearBitmap
    brk
Harness_ClearPage .byte 0
Harness_Trapezoid
    jsr Graphics_Trapezoid_Core
    brk
Harness_Circle
    jsr Graphics_DrawCircle_Core
    brk
Harness_Line
    jsr Graphics_DrawLine_Core
    brk
.include ""{Asm("C64Graphics.asm")}""
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
            var m = System.Text.RegularExpressions.Regex.Match(line, @"^(zp_gfx_\w+)\s*=\s*\$([0-9a-fA-F]+)");
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

    void FillBitmap(byte value)
    {
        var data = new byte[BitmapBytes];
        Array.Fill(data, value);
        emulator.SetMemory(BitmapAddress, data);
    }

    void SetMultiColor(bool on) => emulator.SetMemory(Label("graphics_multicolor_active"), (byte)(on ? 1 : 0));

    void RunHLine(int x0, int x1, int y, bool on, int colorSource = 0)
    {
        emulator.SetMemory(Label("zp_gfx_x_low"), (byte)(x0 & 0xff), (byte)(x0 >> 8));
        emulator.SetMemory(Label("zp_gfx_endx_low"), (byte)(x1 & 0xff), (byte)(x1 >> 8));
        emulator.SetMemory(Label("zp_gfx_y"), (byte)y);
        emulator.SetMemory(Label("zp_gfx_on"), (byte)(on ? 1 : 0));
        emulator.SetMemory(Label("zp_gfx_color"), (byte)colorSource);
        emulator.SetProgramCounter(Label("Harness_HLine"));
        var result = emulator.RunUntil(new HashSet<int>(), 5_000_000, out _, out _);
        Assert.That(result, Is.EqualTo(RunResult.Halted), $"HLine ({x0}-{x1}) at y={y} didn't finish");
    }

    // Hi-res pixel (x,y): byte (y/8)*320 + (x&~7) + (y&7), bit 7-(x&7).
    bool Pixel(int x, int y)
    {
        var offset = (y / 8) * 320 + (x & ~7) + (y & 7);
        return (emulator.GetMemory(BitmapAddress + offset) & (0x80 >> (x & 7))) != 0;
    }

    // Multicolor pair (x,y): same byte, but 2 bits per pixel (76/54/32/10).
    int PixelPair(int x, int y)
    {
        var offset = (y / 8) * 320 + (x & ~7) + (y & 7);
        var b = emulator.GetMemory(BitmapAddress + offset);
        var shift = 6 - ((x & 6) >> 1) * 2;
        return (b >> shift) & 3;
    }

    // x and x+1 always share one 2-bit pair (hardware, not a bug) -- a
    // pixel's pair is affected by a fill if EITHER x-value in its pair
    // falls inside [x0,x1], not only if x itself does. E.g. filling just
    // [1,1] also touches x=0's own value, since 0 and 1 share a pair.
    static bool PairAffected(int x, int x0, int x1)
    {
        int pairStart = x & ~1, pairEnd = pairStart | 1;
        return pairEnd >= x0 && pairStart <= x1;
    }

    // ---- Hi-res correctness ----

    [TestCase(0, 0)]        // single pixel, byte-aligned
    [TestCase(3, 3)]        // single pixel, unaligned
    [TestCase(0, 7)]        // exactly one byte, no interior/tail
    [TestCase(2, 9)]        // partial-head + partial-tail, no interior byte
    [TestCase(0, 15)]       // exactly two bytes, no partial at all
    [TestCase(3, 20)]       // partial-head + 2 interior bytes + partial-tail
    [TestCase(0, 319)]      // full row width
    [TestCase(1, 319)]      // full row minus first pixel
    [TestCase(0, 318)]      // full row minus last pixel
    public void HiRes_Set_Matches_Every_Pixel_In_Range(int x0, int x1)
    {
        FillBitmap(0);
        SetMultiColor(false);
        RunHLine(x0, x1, 100, true);
        for (int x = 0; x < 320; x++)
            Assert.That(Pixel(x, 100), Is.EqualTo(x >= x0 && x <= x1), $"x={x}");
    }

    [Test]
    public void HiRes_Clear_Matches_Every_Pixel_In_Range()
    {
        FillBitmap(0xFF);
        SetMultiColor(false);
        RunHLine(10, 37, 50, false);   // crosses several byte boundaries
        for (int x = 0; x < 320; x++)
            Assert.That(Pixel(x, 50), Is.EqualTo(!(x >= 10 && x <= 37)), $"x={x}");
    }

    [Test]
    public void HiRes_Random_Spans_Match_Reference()
    {
        var random = new Random(64);
        for (int i = 0; i < 500; i++)
        {
            int a = random.Next(320), b = random.Next(320);
            int x0 = Math.Min(a, b), x1 = Math.Max(a, b);
            bool on = random.Next(2) == 0;
            FillBitmap((byte)(on ? 0x00 : 0xFF));
            SetMultiColor(false);
            RunHLine(x0, x1, 77, on);
            for (int x = 0; x < 320; x++)
            {
                bool expectedSet = on ? (x >= x0 && x <= x1) : !(x >= x0 && x <= x1);
                Assert.That(Pixel(x, 77), Is.EqualTo(expectedSet), $"span [{x0},{x1}] on={on} x={x}");
            }
        }
    }

    [Test]
    public void HiRes_Leaves_Other_Rows_Alone()
    {
        FillBitmap(0xAA);
        SetMultiColor(false);
        RunHLine(5, 100, 50, true);
        // 0xAA = bits 7,5,3,1 set: pixels at even x within each byte --
        // a different row's own bytes must be untouched by this call.
        for (int x = 0; x < 320; x++)
            Assert.That(Pixel(x, 51), Is.EqualTo((x & 1) == 0), $"row 51, x={x}");
    }

    // ---- Multicolor correctness ----

    [TestCase(0, 0)]
    [TestCase(2, 5)]
    [TestCase(0, 7)]
    [TestCase(1, 9)]
    [TestCase(0, 15)]
    [TestCase(3, 20)]
    [TestCase(0, 319)]
    public void MultiColor_Set_Matches_Every_Pair_In_Range(int x0, int x1)
    {
        FillBitmap(0);
        SetMultiColor(true);
        RunHLine(x0, x1, 100, true, colorSource: 3);
        for (int x = 0; x < 320; x++)
            Assert.That(PixelPair(x, 100), Is.EqualTo(PairAffected(x, x0, x1) ? 3 : 0), $"x={x}");
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void MultiColor_Encodes_Every_ColorSource_Across_A_Wide_Span(int colorSource)
    {
        FillBitmap(0);
        SetMultiColor(true);
        RunHLine(0, 63, 100, true, colorSource);   // several full interior bytes
        for (int x = 0; x <= 63; x++)
            Assert.That(PixelPair(x, 100), Is.EqualTo(colorSource), $"x={x}");
    }

    [Test]
    public void MultiColor_Clear_Always_Goes_To_Background_Regardless_Of_ColorSource()
    {
        FillBitmap(0xFF);
        SetMultiColor(true);
        RunHLine(10, 37, 50, false, colorSource: 2);
        for (int x = 0; x < 320; x++)
            Assert.That(PixelPair(x, 50), Is.EqualTo(PairAffected(x, 10, 37) ? 0 : 3), $"x={x}");
    }

    [Test]
    public void MultiColor_Random_Spans_Match_Reference()
    {
        var random = new Random(65);
        for (int i = 0; i < 500; i++)
        {
            int a = random.Next(320), b = random.Next(320);
            int x0 = Math.Min(a, b), x1 = Math.Max(a, b);
            int colorSource = random.Next(4);
            FillBitmap(0);
            SetMultiColor(true);
            RunHLine(x0, x1, 77, true, colorSource);
            for (int x = 0; x < 320; x++)
            {
                int expected = PairAffected(x, x0, x1) ? colorSource : 0;
                Assert.That(PixelPair(x, 77), Is.EqualTo(expected), $"span [{x0},{x1}] color={colorSource} x={x}");
            }
        }
    }

    // ---- Performance (measured, not hand-counted -- see GraphicsLineTests.
    // Report_Cycles_Per_Pixel for the same base-cycle-table + PC-trace
    // pattern this reuses) ----

    static readonly int[] BaseCycles =
    {
        7,6,0,0,0,3,5,0,3,2,2,0,0,4,6,0,  2,5,0,0,0,4,6,0,2,4,0,0,0,4,7,0,
        6,6,0,0,3,3,5,0,4,2,2,0,4,4,6,0,  2,5,0,0,0,4,6,0,2,4,0,0,0,4,7,0,
        6,6,0,0,0,3,5,0,3,2,2,0,3,4,6,0,  2,5,0,0,0,4,6,0,2,4,0,0,0,4,7,0,
        6,6,0,0,0,3,5,0,4,2,2,0,5,4,6,0,  2,5,0,0,0,4,6,0,2,4,0,0,0,4,7,0,
        0,6,0,0,3,3,3,0,2,0,2,0,4,4,4,0,  2,6,0,0,4,4,4,0,2,5,2,0,0,5,0,0,
        2,6,2,0,3,3,3,0,2,2,2,0,4,4,4,0,  2,5,0,0,4,4,4,0,2,4,2,0,4,4,4,0,
        2,6,0,0,3,3,5,0,2,2,2,0,4,4,6,0,  2,5,0,0,0,4,6,0,2,4,0,0,0,4,7,0,
        2,6,0,0,3,3,5,0,2,2,2,0,4,4,6,0,  2,5,0,0,0,4,6,0,2,4,0,0,0,4,7,0,
    };

    long MeasureCycles(int x0, int x1, int y, bool multicolor)
    {
        FillBitmap(0);
        SetMultiColor(multicolor);
        emulator.SetMemory(Label("zp_gfx_x_low"), (byte)(x0 & 0xff), (byte)(x0 >> 8));
        emulator.SetMemory(Label("zp_gfx_endx_low"), (byte)(x1 & 0xff), (byte)(x1 >> 8));
        emulator.SetMemory(Label("zp_gfx_y"), (byte)y);
        emulator.SetMemory(Label("zp_gfx_on"), 1);
        emulator.SetMemory(Label("zp_gfx_color"), 3);

        long cycles = 0;
        int previousPc = -1, previousOpcode = 0;
        emulator.Start(Label("Harness_HLine"), 5_000_000, (pc, _) =>
        {
            if (previousPc >= 0)
            {
                bool branch = (previousOpcode & 0x1f) == 0x10;
                cycles += BaseCycles[previousOpcode] + (branch && pc != previousPc + 2 ? 1 : 0);
            }
            previousPc = pc;
            previousOpcode = emulator.GetMemory(pc);
        });
        return cycles;
    }

    // Not exact (page-crossing penalties ignored, same caveat
    // GraphicsLineTests.Report_Cycles_Per_Pixel already documents), but
    // asserts the interior byte-blit lands well below the old per-pixel
    // cost (hand-counted at ~151/~189 cycles/pixel hi-res/multicolor in
    // the partitioned-doodling-sun plan's own research) for a wide span
    // with plenty of fully-interior bytes.
    [TestCase(false)]
    [TestCase(true)]
    public void Report_And_Assert_Cycles_Per_Pixel_For_Wide_Span(bool multicolor)
    {
        int pixels = 256;
        long cycles = MeasureCycles(0, pixels - 1, 100, multicolor);
        double perPixel = (double)cycles / pixels;
        TestContext.WriteLine($"{(multicolor ? "multicolor" : "hi-res")} {pixels}px fill: {cycles} cycles, {perPixel:F1} cycles/px");
        Assert.That(perPixel, Is.LessThan(50), "interior byte-blit should be far below the old ~151-189 cycles/pixel");
    }

    // ---- Filled rectangles and horizontal lines ----

    void SetupRect(int x0, int y0, int x1, int y1, bool on, int colorSource)
    {
        emulator.SetMemory(Label("zp_gfx_x_low"), (byte)(x0 & 0xff), (byte)(x0 >> 8));
        emulator.SetMemory(Label("zp_gfx_y"), (byte)y0);
        emulator.SetMemory(Label("zp_gfx_endx_low"), (byte)(x1 & 0xff), (byte)(x1 >> 8));
        emulator.SetMemory(Label("zp_gfx_endy"), (byte)y1);
        emulator.SetMemory(Label("zp_gfx_on"), (byte)(on ? 1 : 0));
        emulator.SetMemory(Label("zp_gfx_color"), (byte)colorSource);
    }

    void RunFillRect(int x0, int y0, int x1, int y1, bool on, int colorSource = 0)
    {
        SetupRect(x0, y0, x1, y1, on, colorSource);
        emulator.SetProgramCounter(Label("Harness_FillRect"));
        var result = emulator.RunUntil(new HashSet<int>(), 20_000_000, out _, out _);
        Assert.That(result, Is.EqualTo(RunResult.Halted), $"FillRect ({x0},{y0})-({x1},{y1}) didn't finish");
    }

    void RunLine(int x0, int y0, int x1, int y1, bool on, int colorSource = 0)
    {
        SetupRect(x0, y0, x1, y1, on, colorSource);
        emulator.SetProgramCounter(Label("Harness_Line"));
        var result = emulator.RunUntil(new HashSet<int>(), 20_000_000, out _, out _);
        Assert.That(result, Is.EqualTo(RunResult.Halted), $"Line ({x0},{y0})-({x1},{y1}) didn't finish");
    }

    long TraceCycles(string entry)
    {
        long cycles = 0;
        int previousPc = -1, previousOpcode = 0;
        emulator.Start(Label(entry), 20_000_000, (pc, _) =>
        {
            if (previousPc >= 0)
            {
                bool branch = (previousOpcode & 0x1f) == 0x10;
                cycles += BaseCycles[previousOpcode] + (branch && pc != previousPc + 2 ? 1 : 0);
            }
            previousPc = pc;
            previousOpcode = emulator.GetMemory(pc);
        });
        return cycles;
    }

    // Every pixel/pair of the whole bitmap against a model of what a filled
    // rectangle (or horizontal line) must have done to a background of
    // `initial`: inside -> the fill (hi-res: set/clear; multicolor: the
    // color source / background), outside -> untouched.
    void AssertRect(int x0, int y0, int x1, int y1, bool on, int colorSource, bool multicolor, byte initial)
    {
        for (int y = 0; y < 200; y++)
            for (int x = 0; x < 320; x++)
            {
                bool inside = y >= y0 && y <= y1 &&
                              (multicolor ? PairAffected(x, x0, x1) : (x >= x0 && x <= x1));
                if (multicolor)
                {
                    int initialPair = (initial >> (6 - ((x & 6) >> 1) * 2)) & 3;
                    int expected = inside ? (on ? colorSource : 0) : initialPair;
                    Assert.That(PixelPair(x, y), Is.EqualTo(expected), $"rect ({x0},{y0})-({x1},{y1}) on={on} c={colorSource} at ({x},{y})");
                }
                else
                {
                    bool initialBit = (initial & (0x80 >> (x & 7))) != 0;
                    bool expected = inside ? on : initialBit;
                    Assert.That(Pixel(x, y), Is.EqualTo(expected), $"rect ({x0},{y0})-({x1},{y1}) on={on} at ({x},{y})");
                }
            }
    }

    [Test]
    public void FillRect_HiRes_Random_Rectangles_Match_Model()
    {
        var random = new Random(70);
        for (int i = 0; i < 60; i++)
        {
            int xa = random.Next(320), xb = random.Next(320), ya = random.Next(200), yb = random.Next(200);
            int x0 = Math.Min(xa, xb), x1 = Math.Max(xa, xb), y0 = Math.Min(ya, yb), y1 = Math.Max(ya, yb);
            bool on = random.Next(2) == 0;
            byte initial = (byte)(random.Next(2) == 0 ? 0xAA : 0x55);
            FillBitmap(initial);
            SetMultiColor(false);
            RunFillRect(x0, y0, x1, y1, on);
            AssertRect(x0, y0, x1, y1, on, 0, false, initial);
        }
    }

    [Test]
    public void FillRect_MultiColor_Random_Rectangles_Match_Model()
    {
        var random = new Random(71);
        for (int i = 0; i < 60; i++)
        {
            int xa = random.Next(320), xb = random.Next(320), ya = random.Next(200), yb = random.Next(200);
            int x0 = Math.Min(xa, xb), x1 = Math.Max(xa, xb), y0 = Math.Min(ya, yb), y1 = Math.Max(ya, yb);
            bool on = random.Next(4) != 0;
            int colorSource = random.Next(4);
            byte initial = (byte)(random.Next(2) == 0 ? 0x1B : 0xE4);
            FillBitmap(initial);
            SetMultiColor(true);
            RunFillRect(x0, y0, x1, y1, on, colorSource);
            AssertRect(x0, y0, x1, y1, on, colorSource, true, initial);
        }
    }

    [TestCase(0, 0, 0, 0)]
    [TestCase(0, 0, 319, 199)]
    [TestCase(7, 7, 8, 8)]              // 2x2 straddling a cell corner
    [TestCase(131, 92, 187, 173)]       // the dungeon's front door
    [TestCase(255, 63, 256, 176)]       // across the 255/256 x boundary
    public void FillRect_Special_Rectangles(int x0, int y0, int x1, int y1)
    {
        foreach (var multicolor in new[] { false, true })
        {
            FillBitmap(0);
            SetMultiColor(multicolor);
            RunFillRect(x0, y0, x1, y1, true, 3);
            AssertRect(x0, y0, x1, y1, true, 3, multicolor, 0);
        }
    }

    [Test]
    public void Horizontal_DrawLine_Either_Direction_Matches_HLine_MultiColor()
    {
        foreach (var (a, b) in new[] { (10, 200), (200, 10), (0, 319), (319, 0), (33, 33), (5, 6) })
        {
            FillBitmap(0);
            SetMultiColor(true);
            RunLine(a, 77, b, 77, true, 2);
            AssertRect(Math.Min(a, b), 77, Math.Max(a, b), 77, true, 2, true, 0);
        }
    }

    [Test]
    public void Vertical_DrawLine_Either_Direction_Matches_A_One_Column_Rectangle()
    {
        foreach (var multicolor in new[] { false, true })
            foreach (var (a, b) in new[] { (10, 190), (190, 10), (0, 199), (199, 0), (77, 77), (7, 8) })
                foreach (var x in new[] { 0, 1, 50, 255, 256, 319 })
                {
                    FillBitmap(0);
                    SetMultiColor(multicolor);
                    RunLine(x, a, x, b, true, 2);
                    AssertRect(x, Math.Min(a, b), x, Math.Max(a, b), true, 2, multicolor, 0);
                }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Report_Cycles_For_The_Dungeons_Shapes(bool multicolor)
    {
        var shapes = new[]
        {
            ("front door 57x82", 131, 92, 187, 173),
            ("side door 15x108", 29, 69, 43, 176),
            ("wide 300x40", 10, 50, 309, 89),
        };
        foreach (var (name, x0, y0, x1, y1) in shapes)
        {
            FillBitmap(0);
            SetMultiColor(multicolor);
            SetupRect(x0, y0, x1, y1, true, 3);
            long cycles = TraceCycles("Harness_FillRect");
            TestContext.WriteLine($"{(multicolor ? "multicolor" : "hi-res")} {name}: {cycles} cycles");
        }

        // Frame lines like DungeonView draws.
        foreach (var (name, x0, y0, x1, y1) in new[] { ("horizontal line 220px", 50, 26, 269, 26), ("vertical line 148px", 50, 26, 50, 173) })
        {
            FillBitmap(0);
            SetMultiColor(multicolor);
            SetupRect(x0, y0, x1, y1, true, 1);
            long cycles = TraceCycles("Harness_Line");
            TestContext.WriteLine($"{(multicolor ? "multicolor" : "hi-res")} {name}: {cycles} cycles");
        }
    }

    // ---- The other routines: lines (both modes), rectangle outlines,
    // circles, single pixels. Each is checked pixel-for-pixel against a
    // model of the plotted points. ----

    static HashSet<(int, int)> LinePoints(int x0, int y0, int x1, int y1)
    {
        var points = new HashSet<(int, int)>();
        int dx = Math.Abs(x1 - x0), dy = Math.Abs(y1 - y0);
        int sx = x1 >= x0 ? 1 : -1, sy = y1 >= y0 ? 1 : -1;
        int x = x0, y = y0;
        if (dx >= dy)
        {
            int err = dx >> 1;
            while (true)
            {
                points.Add((x, y));
                if (x == x1) break;
                x += sx;
                err -= dy;
                if (err < 0) { y += sy; err += dx; }
            }
        }
        else
        {
            int err = dy >> 1;
            while (true)
            {
                points.Add((x, y));
                if (y == y1) break;
                y += sy;
                err -= dx;
                if (err < 0) { x += sx; err += dy; }
            }
        }
        return points;
    }

    static HashSet<(int, int)> OutlinePoints(int x0, int y0, int x1, int y1)
    {
        var points = new HashSet<(int, int)>();
        for (int x = x0; x <= x1; x++) { points.Add((x, y0)); points.Add((x, y1)); }
        for (int y = y0; y <= y1; y++) { points.Add((x0, y)); points.Add((x1, y)); }
        return points;
    }

    // The midpoint algorithm exactly as Graphics_DrawCircle_Core steps it.
    static HashSet<(int, int)> CirclePoints(int cx, int cy, int radius, bool filled)
    {
        var points = new HashSet<(int, int)>();
        void Span(int y, int a, int b) { for (int x = a; x <= b; x++) points.Add((x, y)); }
        void Plot4(int a, int b)
        {
            points.Add((cx + a, cy + b)); points.Add((cx - a, cy + b));
            points.Add((cx - a, cy - b)); points.Add((cx + a, cy - b));
        }
        int px = radius, py = 0, d = 1 - radius;
        while (py <= px)
        {
            if (filled)
            {
                Span(cy + py, cx - px, cx + px); Span(cy - py, cx - px, cx + px);
                Span(cy + px, cx - py, cx + py); Span(cy - px, cx - py, cx + py);
            }
            else
            {
                Plot4(px, py);
                Plot4(py, px);
            }
            py++;
            if (d < 0) d += 2 * py + 1;
            else { px--; d += 2 * (py - px) + 1; }
        }
        return points;
    }

    void AssertPoints(HashSet<(int, int)> points, bool on, int colorSource, bool multicolor, byte initial, string what)
    {
        for (int y = 0; y < 200; y++)
            for (int x = 0; x < 320; x++)
            {
                if (multicolor)
                {
                    bool touched = points.Contains((x & ~1, y)) || points.Contains((x | 1, y));
                    int initialPair = (initial >> (6 - ((x & 6) >> 1) * 2)) & 3;
                    int expected = touched ? (on ? colorSource : 0) : initialPair;
                    if (PixelPair(x, y) != expected)
                        Assert.Fail($"{what}: pair at ({x},{y}) is {PixelPair(x, y)}, expected {expected}");
                }
                else
                {
                    bool initialBit = (initial & (0x80 >> (x & 7))) != 0;
                    bool expected = points.Contains((x, y)) ? on : initialBit;
                    if (Pixel(x, y) != expected)
                        Assert.Fail($"{what}: pixel at ({x},{y}) is {Pixel(x, y)}, expected {expected}");
                }
            }
    }

    void SetupCircle(int cx, int cy, int radius, bool filled, bool on, int colorSource)
    {
        emulator.SetMemory(Label("zp_gfx_cx_low"), (byte)(cx & 0xff), (byte)(cx >> 8));
        emulator.SetMemory(Label("zp_gfx_cy"), (byte)cy);
        emulator.SetMemory(Label("zp_gfx_radius"), (byte)radius);
        emulator.SetMemory(Label("zp_gfx_filled"), (byte)(filled ? 1 : 0));
        emulator.SetMemory(Label("zp_gfx_on"), (byte)(on ? 1 : 0));
        emulator.SetMemory(Label("zp_gfx_color"), (byte)colorSource);
    }

    void RunCircle(int cx, int cy, int radius, bool filled, bool on, int colorSource)
    {
        SetupCircle(cx, cy, radius, filled, on, colorSource);
        emulator.SetProgramCounter(Label("Harness_Circle"));
        var result = emulator.RunUntil(new HashSet<int>(), 50_000_000, out _, out _);
        Assert.That(result, Is.EqualTo(RunResult.Halted), $"circle ({cx},{cy}) r={radius} didn't finish");
    }

    void RunOutline(int x0, int y0, int x1, int y1, bool on, int colorSource)
    {
        SetupRect(x0, y0, x1, y1, on, colorSource);
        emulator.SetProgramCounter(Label("Harness_Outline"));
        var result = emulator.RunUntil(new HashSet<int>(), 20_000_000, out _, out _);
        Assert.That(result, Is.EqualTo(RunResult.Halted), $"outline ({x0},{y0})-({x1},{y1}) didn't finish");
    }

    void SetupPixel(int x, int y, bool on, int colorSource)
    {
        emulator.SetMemory(Label("zp_gfx_x_low"), (byte)(x & 0xff), (byte)(x >> 8));
        emulator.SetMemory(Label("zp_gfx_y"), (byte)y);
        emulator.SetMemory(Label("zp_gfx_on"), (byte)(on ? 1 : 0));
        emulator.SetMemory(Label("zp_gfx_color"), (byte)colorSource);
    }

    void RunPixel(int x, int y, bool on, int colorSource)
    {
        SetupPixel(x, y, on, colorSource);
        emulator.SetProgramCounter(Label("Harness_SetPixel"));
        var result = emulator.RunUntil(new HashSet<int>(), 100_000, out _, out _);
        Assert.That(result, Is.EqualTo(RunResult.Halted), $"pixel ({x},{y}) didn't finish");
    }

    [Test]
    public void Lines_Grid_Match_Model_Both_Modes()
    {
        var xs = new[] { 0, 1, 7, 8, 9, 254, 255, 256, 300, 319 };
        var ys = new[] { 0, 1, 7, 8, 100, 199 };
        foreach (var multicolor in new[] { false, true })
            foreach (var x0 in xs) foreach (var y0 in ys) foreach (var x1 in xs) foreach (var y1 in ys)
            {
                FillBitmap(0);
                SetMultiColor(multicolor);
                RunLine(x0, y0, x1, y1, true, 2);
                AssertPoints(LinePoints(x0, y0, x1, y1), true, 2, multicolor, 0, $"line ({x0},{y0})-({x1},{y1}) mc={multicolor}");
            }
    }

    [Test]
    public void Lines_Random_Match_Model_Both_Modes()
    {
        var random = new Random(80);
        for (int i = 0; i < 400; i++)
        {
            bool multicolor = i % 2 == 1;
            int x0, y0, x1, y1;
            if (i % 4 < 2)
            {
                x0 = random.Next(320); y0 = random.Next(200); x1 = random.Next(320); y1 = random.Next(200);
            }
            else
            {
                x0 = random.Next(20, 300); y0 = random.Next(20, 180);
                x1 = x0 + random.Next(-15, 16); y1 = y0 + random.Next(-15, 16);
            }
            bool on = random.Next(4) != 0;
            int color = random.Next(4);
            byte initial = (byte)(multicolor ? (random.Next(2) == 0 ? 0x1B : 0xE4) : (random.Next(2) == 0 ? 0xAA : 0x55));
            FillBitmap(initial);
            SetMultiColor(multicolor);
            RunLine(x0, y0, x1, y1, on, color);
            AssertPoints(LinePoints(x0, y0, x1, y1), on, color, multicolor, initial, $"line ({x0},{y0})-({x1},{y1}) on={on} c={color} mc={multicolor}");
        }
    }

    [Test]
    public void Outlines_Random_Match_Model_Both_Modes()
    {
        var random = new Random(81);
        for (int i = 0; i < 80; i++)
        {
            bool multicolor = i % 2 == 1;
            int xa = random.Next(320), xb = random.Next(320), ya = random.Next(200), yb = random.Next(200);
            int x0 = Math.Min(xa, xb), x1 = Math.Max(xa, xb), y0 = Math.Min(ya, yb), y1 = Math.Max(ya, yb);
            bool on = random.Next(4) != 0;
            int color = random.Next(4);
            byte initial = (byte)(multicolor ? 0x1B : 0xAA);
            FillBitmap(initial);
            SetMultiColor(multicolor);
            RunOutline(x0, y0, x1, y1, on, color);
            AssertPoints(OutlinePoints(x0, y0, x1, y1), on, color, multicolor, initial, $"outline ({x0},{y0})-({x1},{y1}) on={on} c={color} mc={multicolor}");
        }
    }

    [Test]
    public void Circles_Match_Model_Both_Modes()
    {
        var random = new Random(82);
        for (int i = 0; i < 60; i++)
        {
            bool multicolor = i % 2 == 1;
            int radius = i < 6 ? i : random.Next(1, 90);
            int cx = random.Next(radius, 320 - radius), cy = random.Next(radius, 200 - radius);
            bool filled = (i / 2) % 2 == 0;
            bool on = random.Next(4) != 0;
            int color = random.Next(4);
            byte initial = (byte)(multicolor ? 0x1B : 0xAA);
            FillBitmap(initial);
            SetMultiColor(multicolor);
            RunCircle(cx, cy, radius, filled, on, color);
            AssertPoints(CirclePoints(cx, cy, radius, filled), on, color, multicolor, initial,
                $"circle ({cx},{cy}) r={radius} filled={filled} on={on} c={color} mc={multicolor}");
        }
    }

    [Test]
    public void SetPixel_Matches_Model_Both_Modes()
    {
        var random = new Random(83);
        foreach (var multicolor in new[] { false, true })
        {
            byte initial = (byte)(multicolor ? 0x1B : 0xAA);
            SetMultiColor(multicolor);
            for (int i = 0; i < 300; i++)
            {
                int x = random.Next(320), y = random.Next(200);
                int color = random.Next(4);
                bool on = random.Next(4) != 0;
                FillBitmap(initial);
                RunPixel(x, y, on, color);
                AssertPoints(new HashSet<(int, int)> { (x, y) }, on, color, multicolor, initial, $"pixel ({x},{y}) on={on} c={color} mc={multicolor}");
            }
        }
    }

    // Graphics_ClearBitmap (SetScreenMode's clear, and Screen.ClearBitmap):
    // exactly 8000 bytes from the page it is given, nothing before or after
    // -- the bytes right after the bitmap hold Catacombs' sprite data.
    [TestCase(0x20)]
    [TestCase(0x40)]
    public void ClearBitmap_Clears_Exactly_The_8000_Bytes(int page)
    {
        int start = page << 8;
        emulator.SetMemory(start - 256, Enumerable.Repeat((byte)0xA5, 256 + 8192 + 256).ToArray());
        emulator.SetMemory(Label("Harness_ClearPage"), (byte)page);
        emulator.SetProgramCounter(Label("Harness_Clear"));
        var result = emulator.RunUntil(new HashSet<int>(), 5_000_000, out _, out _);
        Assert.That(result, Is.EqualTo(RunResult.Halted));
        for (int a = start - 256; a < start + 8192 + 256; a++)
        {
            byte expected = a >= start && a < start + 8000 ? (byte)0 : (byte)0xA5;
            if (emulator.GetMemory(a) != expected)
                Assert.Fail($"byte ${a:X4} is ${emulator.GetMemory(a):X2}, expected ${expected:X2}");
        }
    }

    // ---- Screen.DrawTrapezoid ----

    void SetupTrapezoid(int x0Left, int x0Right, int y0, int x1Left, int x1Right, int y1, bool on, int colorSource)
    {
        emulator.SetMemory(Label("zp_gfx_x_low"), (byte)(x0Left & 0xff), (byte)(x0Left >> 8));
        emulator.SetMemory(Label("zp_gfx_cx_low"), (byte)(x0Right & 0xff), (byte)(x0Right >> 8));
        emulator.SetMemory(Label("zp_gfx_y"), (byte)y0);
        emulator.SetMemory(Label("zp_gfx_endx_low"), (byte)(x1Left & 0xff), (byte)(x1Left >> 8));
        emulator.SetMemory(Label("zp_gfx_dx_low"), (byte)(x1Right & 0xff), (byte)(x1Right >> 8));
        emulator.SetMemory(Label("zp_gfx_endy"), (byte)y1);
        emulator.SetMemory(Label("zp_gfx_on"), (byte)(on ? 1 : 0));
        emulator.SetMemory(Label("zp_gfx_color"), (byte)colorSource);
    }

    void RunTrapezoid(int x0Left, int x0Right, int y0, int x1Left, int x1Right, int y1, bool on, int colorSource)
    {
        SetupTrapezoid(x0Left, x0Right, y0, x1Left, x1Right, y1, on, colorSource);
        emulator.SetProgramCounter(Label("Harness_Trapezoid"));
        var result = emulator.RunUntil(new HashSet<int>(), 20_000_000, out _, out _);
        Assert.That(result, Is.EqualTo(RunResult.Halted),
            $"trapezoid ({x0Left},{x0Right})@{y0} -> ({x1Left},{x1Right})@{y1} didn't finish");
    }

    // Reference model: reproduces Graphics_Trapezoid_Core's own integer DDA
    // exactly (one-time truncating division into a per-row step + a
    // remainder folded in via a bounded error accumulator), not a
    // continuous float interpolation -- same reasoning GraphicsLineTests'
    // own Reference() uses the exact Bresenham algorithm rather than a
    // rounded float line: an integer DDA's per-row position can legitimately
    // lag a "smooth" interpolation by more than one pixel while its
    // accumulated error is still building up (e.g. a slowly-converging
    // edge steps 0 for several rows in a row, then jumps 1), so only an
    // identical integer model gives an exact, tolerance-free comparison.
    static int[] TrapezoidEdge(int topX, int bottomX, int height)
    {
        var xs = new int[height + 1];
        xs[0] = topX;
        int delta = bottomX - topX;
        int sign = delta < 0 ? -1 : 1;
        int adx = Math.Abs(delta);
        int step = height == 0 ? 0 : adx / height;
        int rem = height == 0 ? 0 : adx % height;
        int x = topX, error = 0;
        for (int row = 1; row <= height; row++)
        {
            x += sign * step;
            error += rem;
            if (error >= height)
            {
                error -= height;
                x += sign;
            }
            xs[row] = x;
        }
        return xs;
    }

    // Every row's exact [left,right] span, keyed by row index (0 = y0).
    static (int[] left, int[] right, int loY, int hiY) TrapezoidModel(int x0Left, int x0Right, int y0, int x1Left, int x1Right, int y1)
    {
        if (y0 > y1)
            (x0Left, x0Right, y0, x1Left, x1Right, y1) = (x1Left, x1Right, y1, x0Left, x0Right, y0);
        int height = y1 - y0;
        return (TrapezoidEdge(x0Left, x1Left, height), TrapezoidEdge(x0Right, x1Right, height), y0, y1);
    }

    void AssertTrapezoid(int x0Left, int x0Right, int y0, int x1Left, int x1Right, int y1, bool on, int colorSource, bool multicolor, byte initial)
    {
        var (left, right, loY, hiY) = TrapezoidModel(x0Left, x0Right, y0, x1Left, x1Right, y1);
        for (int y = 0; y < 200; y++)
        {
            bool inRange = y >= loY && y <= hiY;
            int l = inRange ? left[y - loY] : 0, r = inRange ? right[y - loY] : -1;
            for (int x = 0; x < 320; x++)
            {
                if (multicolor)
                {
                    bool touched = inRange && PairAffected(x, l, r);
                    int initialPair = (initial >> (6 - ((x & 6) >> 1) * 2)) & 3;
                    int expected = touched ? (on ? colorSource : 0) : initialPair;
                    if (PixelPair(x, y) != expected)
                        Assert.Fail($"trapezoid row y={y}: pair at x={x} is {PixelPair(x, y)}, expected {expected} (row span [{l},{r}])");
                }
                else
                {
                    bool inside = inRange && x >= l && x <= r;
                    bool initialBit = (initial & (0x80 >> (x & 7))) != 0;
                    bool expected = inside ? on : initialBit;
                    if (Pixel(x, y) != expected)
                        Assert.Fail($"trapezoid row y={y}: pixel at x={x} is {Pixel(x, y)}, expected {expected} (row span [{l},{r}])");
                }
            }
        }
    }

    [Test]
    public void Trapezoid_Random_Match_Model_Both_Modes()
    {
        var random = new Random(90);
        for (int i = 0; i < 100; i++)
        {
            bool multicolor = i % 2 == 1;
            int y0 = random.Next(200), y1 = random.Next(200);
            // Left <= right at each row is this primitive's precondition
            // (same as Graphics_HLine_Core's own x0<=x1) -- "left"/"right"
            // are directional labels, not corners DrawTrapezoid sorts for
            // the caller the way DrawRectangle's 2 symmetric corners are.
            int a = random.Next(320), b = random.Next(320);
            int x0Left = Math.Min(a, b), x0Right = Math.Max(a, b);
            a = random.Next(320); b = random.Next(320);
            int x1Left = Math.Min(a, b), x1Right = Math.Max(a, b);
            bool on = random.Next(4) != 0;
            int color = random.Next(4);
            byte initial = (byte)(multicolor ? (random.Next(2) == 0 ? 0x1B : 0xE4) : (random.Next(2) == 0 ? 0xAA : 0x55));
            FillBitmap(initial);
            SetMultiColor(multicolor);
            RunTrapezoid(x0Left, x0Right, y0, x1Left, x1Right, y1, on, color);
            AssertTrapezoid(x0Left, x0Right, y0, x1Left, x1Right, y1, on, color, multicolor, initial);
        }
    }

    [Test]
    public void Trapezoid_Degenerate_Shapes_Match_Model()
    {
        var shapes = new (string name, int x0L, int x0R, int y0, int x1L, int x1R, int y1)[]
        {
            ("rectangle", 20, 80, 10, 20, 80, 60),
            ("triangle apex at top", 50, 50, 10, 20, 80, 60),
            ("triangle apex at bottom", 20, 80, 10, 50, 50, 60),
            ("single row", 20, 80, 30, 60, 90, 30),
            ("y0 > y1 (reversed rows)", 20, 80, 60, 30, 70, 10),
            ("zero-width both rows (a line)", 50, 50, 10, 90, 90, 60),
        };
        foreach (var (name, x0L, x0R, y0, x1L, x1R, y1) in shapes)
            foreach (var multicolor in new[] { false, true })
            {
                FillBitmap(0);
                SetMultiColor(multicolor);
                RunTrapezoid(x0L, x0R, y0, x1L, x1R, y1, true, 3);
                AssertTrapezoid(x0L, x0R, y0, x1L, x1R, y1, true, 3, multicolor, 0, name);
            }
    }

    void AssertTrapezoid(int x0Left, int x0Right, int y0, int x1Left, int x1Right, int y1, bool on, int colorSource, bool multicolor, byte initial, string what)
    {
        try
        {
            AssertTrapezoid(x0Left, x0Right, y0, x1Left, x1Right, y1, on, colorSource, multicolor, initial);
        }
        catch (AssertionException e)
        {
            throw new AssertionException($"{what}: {e.Message}");
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Report_Cycles_For_Trapezoid(bool multicolor)
    {
        var shapes = new[]
        {
            ("wall panel taper 4x22 (door top)", 29, 29, 66, 29, 43, 69),
            ("wall panel taper 8x7 (door bottom)", 29, 43, 176, 29, 29, 183),
            ("wide floor span 259x22", 4, 315, 4, 50, 269, 26),
            ("cube face ~30x30", 100, 130, 70, 90, 150, 100),
        };
        foreach (var (name, x0L, x0R, y0, x1L, x1R, y1) in shapes)
        {
            FillBitmap(0);
            SetMultiColor(multicolor);
            SetupTrapezoid(x0L, x0R, y0, x1L, x1R, y1, true, 3);
            long cycles = TraceCycles("Harness_Trapezoid");
            TestContext.WriteLine($"{(multicolor ? "multicolor" : "hi-res")} {name}: {cycles} cycles");
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Report_Cycles_For_The_Other_Routines(bool multicolor)
    {
        string mode = multicolor ? "multicolor" : "hi-res";
        void Report(string name, string entry, Action setup, int pixels)
        {
            FillBitmap(0);
            SetMultiColor(multicolor);
            setup();
            long cycles = TraceCycles(entry);
            TestContext.WriteLine($"{mode,-10} {name,-28} {cycles,8} cycles  {(pixels > 0 ? (double)cycles / pixels : 0),7:F1}/px");
        }
        Report("pixel", "Harness_SetPixel", () => SetupPixel(100, 50, true, 3), 1);
        Report("diagonal 45deg 150px", "Harness_Line", () => SetupRect(10, 10, 159, 159, true, 3), 150);
        Report("shallow 200x40", "Harness_Line", () => SetupRect(10, 10, 209, 49, true, 3), 200);
        Report("shallow 250x60 (up-left)", "Harness_Line", () => SetupRect(260, 90, 10, 30, true, 3), 251);
        Report("steep 40x180", "Harness_Line", () => SetupRect(10, 10, 49, 189, true, 3), 180);
        Report("steep 30x150 (up-right)", "Harness_Line", () => SetupRect(20, 180, 49, 31, true, 3), 150);
        Report("wall diagonal 46x22", "Harness_Line", () => SetupRect(4, 4, 50, 26, true, 3), 47);
        Report("vertical 148px", "Harness_Line", () => SetupRect(50, 26, 50, 173, true, 3), 148);
        Report("wide 300x100", "Harness_Line", () => SetupRect(5, 5, 304, 104, true, 3), 300);
        Report("outline 220x148", "Harness_Outline", () => SetupRect(50, 26, 269, 173, true, 3), 0);
        Report("clear bitmap (8000 bytes)", "Harness_Clear", () => emulator.SetMemory(Label("Harness_ClearPage"), 0x20), 8000);
        Report("circle outline r=40", "Harness_Circle", () => SetupCircle(160, 100, 40, false, true, 3), 0);
        Report("circle filled r=40", "Harness_Circle", () => SetupCircle(160, 100, 40, true, true, 3), 0);
    }
}
