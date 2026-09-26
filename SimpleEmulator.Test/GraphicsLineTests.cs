using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace SimpleEmulator.Test;

// Runs asm/C64Graphics.asm's Graphics_DrawLine_Core on SimpleEmulator and
// compares every pixel it sets against a plain C# Bresenham. The routine is
// assembled with 64tass into a tiny harness around the REAL source file (not
// a copy), so this tests what the compiler actually links. Skipped when
// 64tass isn't installed (TASS_EXE, else the repo's usual c:\tools path).
//
// The reference isn't just "some line": it's the exact algorithm and
// tie-breaking the routine has always had (X-major when dx >= dy, error
// starts at half the major delta, y steps when the error goes negative), so
// any rewrite of the routine has to stay pixel-identical to it, not merely
// close to a straight line.
[TestFixture]
public class GraphicsLineTests
{
    const int BitmapAddress = 0x2000;
    const int BitmapBytes = 8000;

    static string repoRoot;
    static string prgPath;
    static Dictionary<string, int> labels;

    Emulator emulator;

    [OneTimeSetUp]
    public void AssembleHarness()
    {
        repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        var tass = Environment.GetEnvironmentVariable("TASS_EXE") ?? @"c:\tools\64tass-1.60.3243\64tass.exe";
        if (!File.Exists(tass))
            Assert.Ignore($"64tass not found at {tass} (set TASS_EXE).");

        var dir = Path.Combine(Path.GetTempPath(), "c64_line_harness");
        Directory.CreateDirectory(dir);
        var asmPath = Path.Combine(dir, "harness.asm");
        prgPath = Path.Combine(dir, "harness.prg");
        var labelsPath = Path.Combine(dir, "harness.labels");

        string Asm(string relative) => Path.Combine(repoRoot, "asm", relative).Replace('\\', '/');
        // GRAPHICS_ASM=<file> assembles that copy of C64Graphics.asm instead
        // (e.g. an older revision, to compare speeds).
        var graphicsAsm = Environment.GetEnvironmentVariable("GRAPHICS_ASM")?.Replace('\\', '/') ?? Asm("C64Graphics.asm");
        File.WriteAllText(asmPath, $@"
.include ""{Asm("helper/zeropage.asm")}""
.include ""{Asm("helper/stack.asm")}""
.include ""{Asm("helper/8bit.asm")}""
Flag_Screen_DrawLine = 1
Graphics_Bitmap = ${BitmapAddress:x4}
* = $1000
Harness_Entry
    jsr Graphics_DrawLine_Core
    brk
.include ""{graphicsAsm}""
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

        // Constants (name = $xx) aren't exported as labels; read the zero-page
        // addresses from the source they're defined in.
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

    // Draws one line and returns the number of instructions the routine ran.
    long Draw(int x0, int y0, int x1, int y1, bool on)
    {
        emulator.SetMemory(Label("zp_gfx_x_low"), (byte)(x0 & 0xff), (byte)(x0 >> 8));
        emulator.SetMemory(Label("zp_gfx_y"), (byte)y0);
        emulator.SetMemory(Label("zp_gfx_endx_low"), (byte)(x1 & 0xff), (byte)(x1 >> 8));
        emulator.SetMemory(Label("zp_gfx_endy"), (byte)y1);
        emulator.SetMemory(Label("zp_gfx_on"), (byte)(on ? 1 : 0));
        emulator.SetProgramCounter(Label("Harness_Entry"));
        var result = emulator.RunUntil(new HashSet<int>(), 5_000_000, out _, out var steps);
        Assert.That(result, Is.EqualTo(RunResult.Halted), $"line ({x0},{y0})-({x1},{y1}) didn't finish");
        return steps;
    }

    void FillBitmap(byte value)
    {
        var data = new byte[BitmapBytes];
        Array.Fill(data, value);
        emulator.SetMemory(BitmapAddress, data);
    }

    // Pixel (x,y) in the bitmap: byte (y/8)*320 + (x&~7) + (y&7), bit 7-(x&7).
    bool Pixel(int x, int y)
    {
        var offset = (y / 8) * 320 + (x & ~7) + (y & 7);
        return (emulator.GetMemory(BitmapAddress + offset) & (0x80 >> (x & 7))) != 0;
    }

    HashSet<(int, int)> SetPixels()
    {
        var result = new HashSet<(int, int)>();
        for (int y = 0; y < 200; y++)
            for (int x = 0; x < 320; x++)
                if (Pixel(x, y))
                    result.Add((x, y));
        return result;
    }

    static HashSet<(int, int)> Reference(int x0, int y0, int x1, int y1)
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
                if (x == x1)
                    break;
                x += sx;
                err -= dy;
                if (err < 0)
                {
                    y += sy;
                    err += dx;
                }
            }
        }
        else
        {
            int err = dy >> 1;
            while (true)
            {
                points.Add((x, y));
                if (y == y1)
                    break;
                y += sy;
                err -= dx;
                if (err < 0)
                {
                    x += sx;
                    err += dy;
                }
            }
        }
        return points;
    }

    void AssertLine(int x0, int y0, int x1, int y1)
    {
        FillBitmap(0);
        Draw(x0, y0, x1, y1, true);
        var actual = SetPixels();
        var expected = Reference(x0, y0, x1, y1);
        if (!actual.SetEquals(expected))
        {
            var missing = expected.Except(actual).Take(5).ToList();
            var extra = actual.Except(expected).Take(5).ToList();
            Assert.Fail($"line ({x0},{y0})-({x1},{y1}): {actual.Count} pixels set, expected {expected.Count}; " +
                        $"missing {string.Join(" ", missing)}; unexpected {string.Join(" ", extra)}");
        }
    }

    [Test]
    public void Endpoints_Grid_MatchesReference()
    {
        // Every pair from a set of coordinates chosen to hit cell (8-pixel)
        // boundaries, the 255/256 byte boundary of X, and the screen edges.
        var xs = new[] { 0, 1, 8, 9, 255, 256, 300, 319 };
        var ys = new[] { 0, 1, 7, 8, 100, 199 };
        foreach (var x0 in xs)
            foreach (var y0 in ys)
                foreach (var x1 in xs)
                    foreach (var y1 in ys)
                        AssertLine(x0, y0, x1, y1);
    }

    [Test]
    public void Random_Lines_MatchReference()
    {
        var random = new Random(64);
        for (int i = 0; i < 1500; i++)
            AssertLine(random.Next(320), random.Next(200), random.Next(320), random.Next(200));
    }

    [Test]
    public void Random_Short_Lines_MatchReference()
    {
        // Short lines in every direction exercise the octant/tie-break logic
        // far more densely than long ones.
        var random = new Random(65);
        for (int i = 0; i < 1500; i++)
        {
            int x0 = random.Next(20, 300), y0 = random.Next(20, 180);
            AssertLine(x0, y0, x0 + random.Next(-15, 16), y0 + random.Next(-15, 16));
        }
    }

    [TestCase(0, 0, 319, 0)]
    [TestCase(319, 199, 0, 199)]
    [TestCase(0, 0, 0, 199)]
    [TestCase(319, 0, 319, 199)]
    [TestCase(0, 0, 199, 199)]
    [TestCase(319, 199, 120, 0)]
    [TestCase(0, 0, 319, 1)]
    [TestCase(0, 0, 1, 199)]
    [TestCase(5, 5, 5, 5)]
    public void Special_Lines_MatchReference(int x0, int y0, int x1, int y1)
    {
        AssertLine(x0, y0, x1, y1);
    }

    [Test]
    public void Drawing_Off_Clears_Exactly_The_Lines_Pixels()
    {
        var random = new Random(66);
        for (int i = 0; i < 300; i++)
        {
            int x0 = random.Next(320), y0 = random.Next(200), x1 = random.Next(320), y1 = random.Next(200);
            FillBitmap(0xFF);
            Draw(x0, y0, x1, y1, false);
            var cleared = new HashSet<(int, int)>();
            for (int y = 0; y < 200; y++)
                for (int x = 0; x < 320; x++)
                    if (!Pixel(x, y))
                        cleared.Add((x, y));
            Assert.That(cleared.SetEquals(Reference(x0, y0, x1, y1)), Is.True, $"line ({x0},{y0})-({x1},{y1})");
        }
    }

    [Test]
    public void Drawing_Leaves_Other_Pixels_Alone()
    {
        FillBitmap(0xAA);
        Draw(10, 10, 100, 60, true);
        var after = SetPixels();
        var line = Reference(10, 10, 100, 60);
        for (int y = 0; y < 200; y++)
            for (int x = 0; x < 320; x++)
            {
                // 0xAA = bits 7,5,3,1 set: pixels at even x within each byte.
                bool background = (x & 1) == 0;
                bool expected = background || line.Contains((x, y));
                Assert.That(after.Contains((x, y)), Is.EqualTo(expected), $"pixel ({x},{y})");
            }
    }

    // Base cycle counts of the documented 6502 opcodes (0 = not a documented
    // opcode). Branches add 1 when taken (see Cycles); page-crossing penalties
    // on indexed reads are ignored, so totals are a good relative measure, not
    // exact.
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

    // Runs the routine for one line and returns (instructions, approximate cycles).
    (long instructions, long cycles) Measure(int x0, int y0, int x1, int y1)
    {
        FillBitmap(0);
        emulator.SetMemory(Label("zp_gfx_x_low"), (byte)(x0 & 0xff), (byte)(x0 >> 8));
        emulator.SetMemory(Label("zp_gfx_y"), (byte)y0);
        emulator.SetMemory(Label("zp_gfx_endx_low"), (byte)(x1 & 0xff), (byte)(x1 >> 8));
        emulator.SetMemory(Label("zp_gfx_endy"), (byte)y1);
        emulator.SetMemory(Label("zp_gfx_on"), 1);

        long cycles = 0, count = 0;
        int previousPc = -1, previousOpcode = 0;
        emulator.Start(Label("Harness_Entry"), 5_000_000, (pc, _) =>
        {
            if (previousPc >= 0)
            {
                bool branch = (previousOpcode & 0x1f) == 0x10;
                cycles += BaseCycles[previousOpcode] + (branch && pc != previousPc + 2 ? 1 : 0);
            }
            previousPc = pc;
            previousOpcode = emulator.GetMemory(pc);
            count++;
        });
        return (count, cycles);
    }

    // Not an assertion on speed: prints instructions and approximate cycles
    // per pixel for lines in each direction/slope class, so a change to the
    // routine's cost shows in the test output. Lines here are all under 256
    // pixels wide, i.e. the fast paths.
    [Test]
    public void Report_Cycles_Per_Pixel()
    {
        var lines = new[]
        {
            ("horizontal right 255", 0, 50, 255, 50),
            ("horizontal left 255", 255, 50, 0, 50),
            ("vertical down 199", 100, 0, 100, 199),
            ("vertical up 199", 100, 199, 100, 0),
            ("diagonal 199", 0, 0, 199, 199),
            ("shallow 255x60", 0, 0, 255, 60),
            ("steep 60x199", 0, 0, 60, 199),
            ("shallow up-left 200x40", 220, 100, 20, 60),
            ("steep up-right 40x150", 20, 180, 60, 30),
            ("short 20x8", 50, 50, 70, 58),
        };
        long totalCycles = 0, totalInstructions = 0, totalPixels = 0;
        foreach (var (name, x0, y0, x1, y1) in lines)
        {
            var (instructions, cycles) = Measure(x0, y0, x1, y1);
            int pixels = Math.Max(Math.Abs(x1 - x0), Math.Abs(y1 - y0)) + 1;
            TestContext.WriteLine($"{name,-26} {pixels,4} px  {(double)instructions / pixels,5:F1} instr/px  {(double)cycles / pixels,6:F1} cycles/px");
            totalCycles += cycles;
            totalInstructions += instructions;
            totalPixels += pixels;
        }
        TestContext.WriteLine($"{"TOTAL",-26} {totalPixels,4} px  {(double)totalInstructions / totalPixels,5:F1} instr/px  {(double)totalCycles / totalPixels,6:F1} cycles/px");
    }
}
