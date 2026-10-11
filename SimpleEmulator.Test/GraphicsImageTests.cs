using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using NUnit.Framework;

namespace SimpleEmulator.Test;

// The cell-level routines of asm/C64Graphics.asm -- Screen.DrawImage (and
// its decompressor), SetCellColors, RemapCellColors -- plus the "high"
// memory layout they normally run in: assembled with 64tass around the REAL
// source file with GRAPHICS_HIGH = 1 and the addresses ProgramEntry.asm
// uses there (bitmap $E000, underneath the KERNAL ROM; matrix $CC00).
//
// The images come from tools/c64img.py ("testvectors": random images run
// through the real compressor, with the bytes each plane must end up
// holding), so compressor and decompressor are checked against each other.
// SimpleEmulator maps the KERNAL ROM over $E000-$FFFF like the real machine,
// which is what makes the banking testable: a routine that read the bitmap
// without banking the ROM out would get ROM bytes back.
//
// Skipped when 64tass or Python is not installed.
[TestFixture]
public class GraphicsImageTests
{
    const int Bitmap = 0xE000;
    const int Matrix = 0xCC00;
    const int ColorRam = 0xD800;
    const int ImageData = 0x4000;
    const int Table = 0x3F00;

    static string prgPath;
    static Dictionary<string, int> labels;
    static List<ImageCase> cases;

    Emulator emulator;

    class ImageCase
    {
        public int X, Y, Cols, Rows;
        public byte[] Payload;
        public Dictionary<int, byte> BitmapBytes, MatrixBytes, ColourBytes;
    }

    [OneTimeSetUp]
    public void AssembleHarness()
    {
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        var tass = Environment.GetEnvironmentVariable("TASS_EXE") ?? @"c:\tools\64tass-1.60.3243\64tass.exe";
        if (!File.Exists(tass))
            Assert.Ignore($"64tass not found at {tass} (set TASS_EXE).");

        var dir = Path.Combine(Path.GetTempPath(), "c64_image_harness");
        Directory.CreateDirectory(dir);
        var asmPath = Path.Combine(dir, "harness.asm");
        prgPath = Path.Combine(dir, "harness.prg");
        var labelsPath = Path.Combine(dir, "harness.labels");
        var vectorsPath = Path.Combine(dir, "vectors.json");

        string python = Environment.GetEnvironmentVariable("PYTHON_EXE") ?? "python";
        try
        {
            var output = RunProcess(python, Path.Combine(repoRoot, "tools", "c64img.py"), "testvectors", vectorsPath);
            Assert.That(output.ExitCode, Is.EqualTo(0), "c64img.py testvectors failed:\n" + output.Text);
        }
        catch (System.ComponentModel.Win32Exception)
        {
            Assert.Ignore("python not found (set PYTHON_EXE).");
        }
        cases = LoadCases(vectorsPath);

        string Asm(string relative) => Path.Combine(repoRoot, "asm", relative).Replace('\\', '/');
        File.WriteAllText(asmPath, $@"
.include ""{Asm("helper/zeropage.asm")}""
.include ""{Asm("helper/stack.asm")}""
.include ""{Asm("helper/8bit.asm")}""
Flag_Screen_SetPixel = 1
Flag_Screen_DrawImage = 1
Flag_Screen_SetCellColors = 1
Flag_Screen_RemapCellColors = 1
Flag_Screen_MirrorCells = 1
Flag_Screen_SetScrollX = 1
GRAPHICS_HIGH = 1
Graphics_ColorMatrix = ${Matrix:x4}
Graphics_Bitmap = ${Bitmap:x4}
* = $1000
Harness_Args .fill 8
Harness_DrawImage
    lda #$06                ; the runtime's normal banking: KERNAL in, BASIC out
    sta $01
    lda Harness_Args
    pha
    lda Harness_Args+1
    pha
    lda #>${ImageData:x4}
    pha
    lda #<${ImageData:x4}
    pha
    jsr Screen_DrawImage
    brk
Harness_SetCellColors
    ldx #0
-   lda Harness_Args,x
    pha
    inx
    cpx #6
    bne -
    jsr Screen_SetCellColors
    brk
Harness_RemapCellColors
    ldx #0
-   lda Harness_Args,x
    pha
    inx
    cpx #4
    bne -
    lda #>${Table:x4}
    pha
    lda #<${Table:x4}
    pha
    jsr Screen_RemapCellColors
    brk
Harness_MirrorCells
    lda #$06
    sta $01
    ldx #0
-   lda Harness_Args,x
    pha
    inx
    cpx #4
    bne -
    jsr Screen_MirrorCells
    brk
Harness_SetScrollX
    lda Harness_Args
    pha
    jsr Screen_SetScrollX
    brk
Harness_SetPixel
    lda #$06
    sta $01
    lda Harness_Args        ; x, 16 bit
    pha
    lda #0
    pha
    lda Harness_Args+1      ; y, 16 bit
    pha
    lda #1                  ; on
    pha
    lda #3                  ; colorSource
    pha
    jsr Screen_SetPixel_Entry
    brk
; Screen.SetPixel(ulong x, ulong y, bool on, BitmapColorSource colorSource):
; the 16-bit arguments are pushed high byte first.
Screen_SetPixel_Entry
    jmp Screen_SetPixel
.include ""{Asm("C64Graphics.asm")}""
Harness_End
");

        var result = RunProcess(tass, "-o", prgPath, "--long-branch", "--vice-labels", "-l", labelsPath, "--no-monitor", asmPath);
        Assert.That(result.ExitCode, Is.EqualTo(0), "64tass failed:\n" + result.Text);

        labels = new Dictionary<string, int>();
        foreach (var line in File.ReadAllLines(labelsPath))
        {
            var parts = line.Split(' ');
            if (parts.Length >= 3)
                labels[parts[2].TrimStart('.')] = Convert.ToInt32(parts[1], 16);
        }
        Assert.That(labels["Harness_End"], Is.LessThan(Table), "harness code overlaps the test data");
    }

    static (int ExitCode, string Text) RunProcess(string file, params string[] args)
    {
        var psi = new ProcessStartInfo(file)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var a in args)
            psi.ArgumentList.Add(a);
        using var p = Process.Start(psi);
        var text = p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd();
        p.WaitForExit();
        return (p.ExitCode, text);
    }

    static List<ImageCase> LoadCases(string path)
    {
        var result = new List<ImageCase>();
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        foreach (var item in doc.RootElement.EnumerateArray())
        {
            Dictionary<int, byte> Plane(string name) =>
                item.GetProperty(name).EnumerateArray().ToDictionary(e => e[0].GetInt32(), e => (byte)e[1].GetInt32());
            result.Add(new ImageCase
            {
                X = item.GetProperty("x").GetInt32(),
                Y = item.GetProperty("y").GetInt32(),
                Cols = item.GetProperty("cols").GetInt32(),
                Rows = item.GetProperty("rows").GetInt32(),
                Payload = item.GetProperty("payload").EnumerateArray().Select(e => (byte)e.GetInt32()).ToArray(),
                BitmapBytes = Plane("bitmap"),
                MatrixBytes = Plane("matrix"),
                ColourBytes = Plane("colour"),
            });
        }
        return result;
    }

    [SetUp]
    public void LoadHarness()
    {
        emulator = new Emulator();
        emulator.LoadPrg(prgPath);
    }

    int Label(string name) =>
        labels.TryGetValue(name, out var address) ? address : throw new AssertionException($"label {name} missing from the harness");

    void Run(string entry, params byte[] args)
    {
        emulator.SetMemory(Label("Harness_Args"), args);
        emulator.SetProgramCounter(Label(entry));
        var result = emulator.RunUntil(new HashSet<int>(), 20_000_000, out _, out _);
        Assert.That(result, Is.EqualTo(RunResult.Halted), $"{entry} did not finish");
    }

    void Fill(int address, int count, byte value) => emulator.SetMemory(address, Enumerable.Repeat(value, count).ToArray());

    // What is in RAM at an address, whatever ROM is mapped over it.
    byte Ram(int address)
    {
        var port = emulator.GetMemory(1);
        emulator.SetMemory(1, 0x04);
        var value = emulator.GetMemory(address);
        emulator.SetMemory(1, port);
        return value;
    }

    void AssertPlane(string name, int baseAddress, int length, Dictionary<int, byte> expected, byte untouched, byte mask, int caseIndex)
    {
        for (int i = 0; i < length; i++)
        {
            byte want = expected.TryGetValue(i, out var v) ? v : untouched;
            byte got = Ram(baseAddress + i);
            if (((got ^ want) & mask) != 0)
                Assert.Fail($"case {caseIndex}: {name} byte {i} (${baseAddress + i:X4}) is ${got:X2}, expected ${want:X2}" +
                    (expected.ContainsKey(i) ? "" : " (a byte the image must not touch)"));
        }
    }

    [Test]
    public void DrawImage_Decodes_Every_Plane_Exactly_And_Touches_Nothing_Else()
    {
        Assert.That(cases.Count, Is.GreaterThan(8));
        for (int index = 0; index < cases.Count; index++)
        {
            var c = cases[index];
            emulator = new Emulator();
            emulator.LoadPrg(prgPath);
            Fill(Bitmap, 8000, 0xA5);
            Fill(Matrix, 1000, 0x5A);
            Fill(ColorRam, 1000, 0x05);
            emulator.SetMemory(ImageData, c.Payload);
            Run("Harness_DrawImage", (byte)c.X, (byte)c.Y);

            AssertPlane("bitmap", Bitmap, 8000, c.BitmapBytes, 0xA5, 0xFF, index);
            AssertPlane("matrix", Matrix, 1000, c.MatrixBytes, 0x5A, 0xFF, index);
            // Colour RAM holds 4 bits per cell; the decoder may write junk above them.
            AssertPlane("colour RAM", ColorRam, 1000, c.ColourBytes, 0x05, 0x0F, index);
            Assert.That(emulator.GetMemory(1) & 7, Is.EqualTo(6), $"case {index}: the KERNAL ROM must be banked back in afterwards");
        }
    }

    [Test]
    public void Test_Vectors_Exercise_Matches_Skips_And_Long_Offsets()
    {
        // Guards the test itself: the images must make the compressor use
        // every token kind, or the decoder's paths are not all covered.
        bool literal = false, repeat = false, skip = false, shortMatch = false, longMatch = false;
        foreach (var c in cases)
        {
            int planes = c.Payload[0];
            int pos = 1;
            for (int plane = 0; plane < 3; plane++)
            {
                if ((planes & (1 << plane)) == 0)
                    continue;
                while (true)
                {
                    int t = c.Payload[pos++];
                    if (t == 0) break;
                    if (t < 0x40) { literal = true; pos += t; }
                    else if (t < 0x60) repeat = true;
                    else if (t < 0x80) { skip = true; pos++; }
                    else if (t < 0xC0) { shortMatch = true; pos++; }
                    else { longMatch = true; pos += 2; }
                }
            }
            Assert.That(pos, Is.EqualTo(c.Payload.Length));
        }
        Assert.That(new[] { literal, repeat, skip, shortMatch, longMatch }, Is.All.True,
            "literal / repeat / skip / short match / long match");
    }

    [Test]
    public void SetPixel_Keeps_The_Other_Bits_Of_A_Byte_Under_The_Kernal_Rom()
    {
        Fill(Bitmap, 8000, 0);
        Run("Harness_SetPixel", 0, 1);     // multicolour is off in the harness: hi-res bits
        Run("Harness_SetPixel", 5, 1);
        Run("Harness_SetPixel", 7, 1);
        // Without banking the ROM out, each read-modify-write would have
        // started from the ROM's byte at $E001 instead of the bits set so far.
        Assert.That(Ram(Bitmap + 1), Is.EqualTo(0x85));
        Assert.That(emulator.GetMemory(Bitmap + 1), Is.Not.EqualTo(0x85), "the emulator should be showing ROM here again");
        Assert.That(emulator.GetMemory(1) & 7, Is.EqualTo(6));
    }

    [Test]
    public void SetCellColors_Fills_Exactly_The_Rectangle()
    {
        Fill(Matrix, 1000, 0x11);
        Fill(ColorRam, 1000, 0x02);
        Run("Harness_SetCellColors", 37, 22, 3, 3, 0xAB, 0x0C);
        for (int row = 0; row < 25; row++)
            for (int col = 0; col < 40; col++)
            {
                bool inside = col >= 37 && row >= 22;
                Assert.That(emulator.GetMemory(Matrix + row * 40 + col), Is.EqualTo(inside ? 0xAB : 0x11), $"matrix ({col},{row})");
                Assert.That(emulator.GetMemory(ColorRam + row * 40 + col), Is.EqualTo(inside ? 0x0C : 0x02), $"colour RAM ({col},{row})");
            }
    }

    [Test]
    public void SetCellColors_With_An_Empty_Rectangle_Does_Nothing()
    {
        Fill(Matrix, 1000, 0x11);
        Fill(ColorRam, 1000, 0x02);
        Run("Harness_SetCellColors", 5, 5, 0, 3, 0xAB, 0x0C);
        Run("Harness_SetCellColors", 5, 5, 3, 0, 0xAB, 0x0C);
        for (int i = 0; i < 1000; i++)
        {
            Assert.That(emulator.GetMemory(Matrix + i), Is.EqualTo(0x11));
            Assert.That(emulator.GetMemory(ColorRam + i), Is.EqualTo(0x02));
        }
    }

    [Test]
    public void RemapCellColors_Maps_Both_Matrix_Nibbles_And_Colour_Ram()
    {
        var table = new byte[16];
        for (int i = 0; i < 16; i++)
            table[i] = (byte)((i * 7 + 3) & 15);       // a permutation of 0..15
        emulator.SetMemory(Table, table);
        var rnd = new Random(7);
        var matrix = new byte[1000];
        var colour = new byte[1000];
        rnd.NextBytes(matrix);
        rnd.NextBytes(colour);                         // high nibbles: junk, as on the real chip
        emulator.SetMemory(Matrix, matrix);
        emulator.SetMemory(ColorRam, colour);

        Run("Harness_RemapCellColors", 2, 1, 38, 24);
        for (int row = 0; row < 25; row++)
            for (int col = 0; col < 40; col++)
            {
                int i = row * 40 + col;
                bool inside = col >= 2 && row >= 1;
                byte m = matrix[i];
                byte wantM = inside ? (byte)((table[m >> 4] << 4) | table[m & 15]) : m;
                Assert.That(emulator.GetMemory(Matrix + i), Is.EqualTo(wantM), $"matrix ({col},{row})");
                int wantC = inside ? table[colour[i] & 15] : colour[i] & 15;
                Assert.That(emulator.GetMemory(ColorRam + i) & 15, Is.EqualTo(wantC), $"colour RAM ({col},{row})");
            }
    }

    // Reverses a byte's pixels: four multicolour pixel pairs, or eight hi-res bits.
    static byte Reversed(byte b, bool multiColor)
    {
        int r = 0;
        if (multiColor)
            for (int i = 0; i < 4; i++)
                r |= ((b >> (i * 2)) & 3) << (6 - i * 2);
        else
            for (int i = 0; i < 8; i++)
                r |= ((b >> i) & 1) << (7 - i);
        return (byte)r;
    }

    [TestCase(true, 0, 0, 40, 21)]
    [TestCase(false, 0, 0, 40, 21)]
    [TestCase(true, 3, 2, 7, 5)]       // odd width: the middle column stays
    [TestCase(false, 30, 20, 10, 5)]
    [TestCase(true, 38, 24, 2, 1)]
    public void MirrorCells_Copies_The_Left_Half_Reversed_And_Touches_Nothing_Else(bool multiColor, int x, int y, int w, int h)
    {
        var rnd = new Random(11);
        var bitmap = new byte[8000];
        var matrix = new byte[1000];
        var colour = new byte[1000];
        rnd.NextBytes(bitmap);
        rnd.NextBytes(matrix);
        rnd.NextBytes(colour);
        emulator.SetMemory(Bitmap, bitmap);
        emulator.SetMemory(Matrix, matrix);
        emulator.SetMemory(ColorRam, colour);
        emulator.SetMemory(Label("graphics_multicolor_active"), new byte[] { (byte)(multiColor ? 1 : 0) });

        Run("Harness_MirrorCells", (byte)x, (byte)y, (byte)w, (byte)h);

        for (int row = 0; row < 25; row++)
            for (int col = 0; col < 40; col++)
            {
                int i = row * 40 + col;
                bool written = row >= y && row < y + h && col >= x + (w + 1) / 2 && col < x + w;
                int from = written ? row * 40 + (x + (x + w - 1 - col)) : i;
                Assert.That(emulator.GetMemory(Matrix + i), Is.EqualTo(matrix[from]), $"matrix ({col},{row})");
                Assert.That(emulator.GetMemory(ColorRam + i) & 15, Is.EqualTo(colour[from] & 15), $"colour RAM ({col},{row})");
                for (int k = 0; k < 8; k++)
                {
                    byte want = written ? Reversed(bitmap[from * 8 + k], multiColor) : bitmap[i * 8 + k];
                    Assert.That(Ram(Bitmap + i * 8 + k), Is.EqualTo(want), $"bitmap ({col},{row}) byte {k}");
                }
            }
        Assert.That(emulator.GetMemory(1) & 7, Is.EqualTo(6), "the KERNAL ROM must be banked back in afterwards");
    }

    [Test]
    public void MirrorCells_With_Nothing_To_Copy_Does_Nothing()
    {
        Fill(Bitmap, 8000, 0x1B);
        Fill(Matrix, 1000, 0x12);
        Run("Harness_MirrorCells", 5, 5, 1, 3);
        Run("Harness_MirrorCells", 5, 5, 4, 0);
        for (int i = 0; i < 8000; i++)
            Assert.That(Ram(Bitmap + i), Is.EqualTo(0x1B));
        for (int i = 0; i < 1000; i++)
            Assert.That(emulator.GetMemory(Matrix + i), Is.EqualTo(0x12));
    }

    [Test]
    public void SetScrollX_Changes_Only_The_Fine_Scroll_Bits()
    {
        emulator.SetMemory(0xD016, new byte[] { 0x18 });
        Run("Harness_SetScrollX", 5);
        Assert.That(emulator.GetMemory(0xD016), Is.EqualTo(0x1D));
        Run("Harness_SetScrollX", 0);
        Assert.That(emulator.GetMemory(0xD016), Is.EqualTo(0x18));
        Run("Harness_SetScrollX", 0xFA);     // only 0-7 means anything
        Assert.That(emulator.GetMemory(0xD016), Is.EqualTo(0x1A));
    }
}
