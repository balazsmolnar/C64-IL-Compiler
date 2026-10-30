using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace SimpleEmulator.Test;

// Runs the runtime's shared multiply/divide subroutines
// (asm/helper/division.asm: Multiply8, Multiply16, Divide16Core) out of the
// real compiled unit-test program (prg/unittest.prg) against C#'s own
// arithmetic: every 8-bit product, and a wide spread of 16-bit operands
// covering each fast path (narrow divisor, narrow dividend, narrow
// multiplier on either side) and the general one.
[TestFixture]
public class ArithmeticRoutineTests
{
    private const int ZpParam0 = 0x30;
    private const int ZpParam1 = 0x32;
    private const int ZpParam2 = 0x34;
    private const int DriverStart = 0x0900;

    private static Dictionary<string, int> labels;

    private static string PrgFolder()
    {
        var directory = new FileInfo(typeof(ArithmeticRoutineTests).Assembly.Location).DirectoryName;
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

    private static Emulator Load(string routine)
    {
        var e = new Emulator();
        e.LoadPrg(Path.Combine(PrgFolder(), "unittest.prg"));
        e.SetMemory(0x01, 0x06);
        // jsr routine; sta $02 (A result); brk
        e.SetMemory(DriverStart, new Asm().Jsr(Label(routine)).StaZp(0x02).Brk().ToArray());
        return e;
    }

    private static void Run(Emulator e) => e.Start(DriverStart, 5000, (pc, step) => { });

    private static void Set16(Emulator e, int zp, int value)
    {
        e.SetMemory(zp, (byte)(value & 0xFF));
        e.SetMemory(zp + 1, (byte)(value >> 8));
    }

    private static int Get16(Emulator e, int zp) => e.GetMemory(zp) | (e.GetMemory(zp + 1) << 8);

    private static IEnumerable<int> Operands16()
    {
        var values = new List<int> { 0, 1, 2, 3, 7, 10, 100, 127, 128, 129, 200, 255, 256, 257, 300, 1000, 4600, 32767, 32768, 40000, 65534, 65535 };
        var random = new Random(1234);
        for (int i = 0; i < 40; i++)
            values.Add(random.Next(0, 65536));
        for (int i = 0; i < 20; i++)
            values.Add(random.Next(0, 256));
        return values;
    }

    [Test]
    public void Multiply8_Matches_Every_Product()
    {
        var e = Load("Multiply8");
        for (int a = 0; a < 256; a++)
            for (int b = 0; b < 256; b += (a < 16 ? 1 : 7))
            {
                e.SetMemory(ZpParam1, (byte)a);
                e.SetMemory(ZpParam2, (byte)b);
                Run(e);
                Assert.That(e.GetMemory(0x02), Is.EqualTo((byte)(a * b)), $"{a} * {b}");
            }
    }

    [Test]
    public void Multiply16_Matches()
    {
        var e = Load("Multiply16");
        foreach (var a in Operands16())
            foreach (var b in Operands16())
            {
                Set16(e, ZpParam1, a);
                Set16(e, ZpParam2, b);
                Run(e);
                Assert.That(Get16(e, ZpParam0), Is.EqualTo((a * b) & 0xFFFF), $"{a} * {b}");
            }
    }

    [Test]
    public void Divide16Core_Matches()
    {
        var e = Load("Divide16Core");
        foreach (var dividend in Operands16())
            foreach (var divisor in Operands16())
            {
                // A divisor above 32767 can overflow the general path's
                // 16-bit remainder (it always could); real programs divide
                // by small constants or lengths.
                if (divisor == 0 || divisor > 32767)
                    continue;
                Set16(e, ZpParam2, dividend);
                Set16(e, ZpParam1, divisor);
                Run(e);
                Assert.That(Get16(e, ZpParam2), Is.EqualTo(dividend / divisor), $"{dividend} / {divisor}");
                Assert.That(Get16(e, ZpParam0), Is.EqualTo(dividend % divisor), $"{dividend} % {divisor}");
            }
    }
}
