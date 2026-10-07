using System;
using System.IO;
using System.Linq;

namespace TestDebugger;

// `TestDebugger vice-probe <port> [<labels file> <label>...]` -- connects to
// a VICE started with -binarymonitor and prints what the ViceTarget sees,
// optionally running to each given label in turn. A manual smoke test of
// the monitor client (there's no VICE in CI).
static class ViceProbe
{
    public static int Run(string[] args)
    {
        var port = args.Length > 1 ? int.Parse(args[1]) : 6502;
        var labelsFile = args.Length > 2 ? args[2] : null;
        var labelNames = args.Skip(3).ToList();

        Console.WriteLine($"Connecting to VICE on port {port}...");
        using var client = ViceMonitorClient.Connect("127.0.0.1", port);
        using var target = new ViceTarget(client);

        var r = target.Registers;
        Console.WriteLine($"Stopped: A=${r.A:X2} X=${r.X:X2} Y=${r.Y:X2} PC=${target.ProgramCounter:X4} SP=${target.HardwareStackPointer:X2} P=${r.P:X2}");
        Console.WriteLine("$0801: " + string.Join(" ", Enumerable.Range(0x0801, 8).Select(a => target.GetMemory(a).ToString("X2"))));
        Console.WriteLine("$0001: " + target.GetMemory(1).ToString("X2"));

        // SetMemory round-trip -- the debugger's own "set"/setVariable
        // write path (ObjectInspector.TryWrite), confirming VICE's binary
        // monitor MemSet command actually lands and that ViceTarget's own
        // read cache doesn't serve a stale value back afterward.
        const int probeAddr = 0xC000;
        var probeBefore = target.GetMemory(probeAddr);
        target.SetMemory(probeAddr, (byte)0x42);
        var probeAfter = target.GetMemory(probeAddr);
        Console.WriteLine($"SetMemory round-trip at ${probeAddr:X4}: wrote 0x42, read back 0x{probeAfter:X2} ({(probeAfter == 0x42 ? "OK" : "MISMATCH")})");
        target.SetMemory(probeAddr, probeBefore);

        if (labelsFile == null)
            return 0;

        var labels = File.ReadAllLines(labelsFile)
            .Select(l => l.Split(' '))
            .Where(p => p.Length >= 3)
            .ToDictionary(p => p[2].TrimStart('.'), p => Convert.ToInt32(p[1], 16), StringComparer.Ordinal);

        foreach (var name in labelNames)
        {
            if (!labels.TryGetValue(name, out var address))
            {
                Console.WriteLine($"No label {name}");
                continue;
            }

            var result = target.RunUntil(new System.Collections.Generic.HashSet<int> { address }, 0, out var stoppedAt, out _);
            var regs = target.Registers;
            Console.WriteLine($"{name} (${address:X4}): {result} at PC=${stoppedAt:X4}  A=${regs.A:X2} SP=${target.HardwareStackPointer:X2}  localsSP($4b)={target.GetMemory(0x4b)}");
        }

        // One raw single step, to check Advance.
        var before = target.ProgramCounter;
        target.StepOne();
        Console.WriteLine($"StepOne: ${before:X4} -> ${target.ProgramCounter:X4}");
        return 0;
    }
}
