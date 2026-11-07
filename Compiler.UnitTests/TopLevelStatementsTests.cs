using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using SimpleEmulator;

namespace Compiler.UnitTests;

// A program written with C# top-level statements: Roslyn compiles it to a
// class `Program` with the entry point `<Main>$(string[] args)` (label
// Program_x_Main__), its local functions to `<<Main>$>g__Name|0_0`. The
// program entry (ILEntryPointPass) calls the assembly's entry point,
// whatever its name, instead of a fixed Program_Main. Compiled with the
// real driver, assembled with 64tass (skipped when it isn't installed) and
// run on SimpleEmulator from the SYS target, in both frame conventions.
[TestFixture]
public class TopLevelStatementsTests
{
    const string TopLevel = @"using C64Lib;

var counter = new Counter();
counter.Add(3);
counter.Add(4);
C64.FillMemory(0x0340UL, Twice(counter.Value), 1);
C64.FillMemory(0x0341UL, Helper.Seven(), 1);

static uint Twice(uint x) => x * 2;

class Counter
{
    public uint Value;
    public void Add(uint n) { Value += n; }
}

static class Helper
{
    public static uint Seven() => 7;
}
";

    [Test]
    public void Top_Level_Statements_Compile_And_Call_The_Generated_Entry_Point()
    {
        var (exit, output) = DiagnosticsTests.CompileProgram(TopLevel);
        Assert.That(exit, Is.EqualTo(0), output);
        var entry = File.ReadAllText(Path.Combine(Path.GetDirectoryName(DiagnosticsTests.lastOutDir), "entry.asm"));
        Assert.That(entry, Does.Contain("jsr Program_x_Main__"), entry);
        Assert.That(entry, Does.Not.Contain("jsr Program_Main"));
    }

    [Test]
    public void A_Declared_Main_Is_Still_A_Plain_Jsr()
    {
        var (exit, output) = DiagnosticsTests.CompileProgram("using C64Lib;\nclass Program { static void Main() { C64.FillMemory(0x0340UL, 1, 1); } }");
        Assert.That(exit, Is.EqualTo(0), output);
        var entry = File.ReadAllText(Path.Combine(Path.GetDirectoryName(DiagnosticsTests.lastOutDir), "entry.asm"));
        Assert.That(entry, Does.Contain("\njsr Program_Main\n").Or.Contain("\njsr Program_Main\r\n"), entry);
    }

    [TestCase(false, TestName = "Top_Level_Statements_Run_With_Static_Frames")]
    [TestCase(true, TestName = "Top_Level_Statements_Run_Without_Static_Frames")]
    public void Top_Level_Statements_Run(bool noStaticFrames)
    {
        var emulator = AssembleAndLoad(TopLevel, noStaticFrames);
        Run(emulator);
        Assert.That(emulator.GetMemory(0x0340), Is.EqualTo(14), "Twice(3 + 4) via a local function and a class");
        Assert.That(emulator.GetMemory(0x0341), Is.EqualTo(7), "a static class in the same file");
    }

    [TestCase(false, TestName = "Int_Returning_Main_With_Args_Runs_With_Static_Frames")]
    [TestCase(true, TestName = "Int_Returning_Main_With_Args_Runs_Without_Static_Frames")]
    public void Int_Returning_Top_Level_Program_Leaves_The_Stack_Balanced(bool noStaticFrames)
    {
        // `return 5;` makes `<Main>$` return int; its result must not be
        // left on the stack under BASIC's SYS return address.
        var emulator = AssembleAndLoad("using C64Lib;\nC64.FillMemory(0x0340UL, 9, 1);\nreturn 5;\n", noStaticFrames);
        Run(emulator);
        Assert.That(emulator.GetMemory(0x0340), Is.EqualTo(9));
    }

    [Test]
    public void Reading_Args_Is_Reported()
    {
        var (exit, output) = DiagnosticsTests.CompileProgram("using C64Lib;\nC64.FillMemory(0x0340UL, args == null ? 1u : 2u, 1);\n");
        Assert.That(exit, Is.EqualTo(1), output);
        Assert.That(output, Does.Contain("error C64004"));
        Assert.That(output, Does.Contain("'args'"));
        Assert.That(output, Does.Contain("snippet.cs(2)"), output);
    }

    // Calls the program's SYS target ($1000) from a `jsr $1000; brk` stub,
    // as BASIC's SYS would, and checks it returns with the stack as it was.
    static void Run(Emulator emulator)
    {
        const int stub = 0x0334;
        emulator.SetMemory(stub, 0x20, 0x00, 0x10, 0x00);
        emulator.SetProgramCounter(stub);
        var stackBefore = emulator.HardwareStackPointer;
        var result = emulator.RunUntil(new HashSet<int>(), 2_000_000, out var stoppedAt, out _);
        Assert.That(result, Is.EqualTo(RunResult.Halted), $"the program did not return (stopped at ${stoppedAt:X4})");
        Assert.That(stoppedAt, Is.EqualTo(stub + 4), "returned somewhere other than the caller");
        Assert.That(emulator.HardwareStackPointer, Is.EqualTo(stackBefore), "stack unbalanced");
    }

    static Emulator AssembleAndLoad(string source, bool noStaticFrames)
    {
        var tass = Environment.GetEnvironmentVariable("TASS_EXE") ?? @"c:\tools\64tass-1.60.3243\64tass.exe";
        if (!File.Exists(tass))
            Assert.Ignore($"64tass not found at {tass} (set TASS_EXE).");

        var (exit, output) = noStaticFrames
            ? DiagnosticsTests.CompileProgram(source, "--no-static-frames")
            : DiagnosticsTests.CompileProgram(source);
        Assert.That(exit, Is.EqualTo(0), output);

        var dir = Path.GetDirectoryName(DiagnosticsTests.lastOutDir);
        var prg = Path.Combine(dir, "program.prg");
        var psi = new ProcessStartInfo(tass)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        // The entry .includes the runtime as ./helper/..., ./C64.asm etc.:
        // found through -I in the repository's asm folder.
        foreach (var a in new[] { "-q", "-o", prg, "--long-branch", "--no-monitor", "-I", AsmFolder(), Path.Combine(dir, "entry.asm") })
            psi.ArgumentList.Add(a);
        using (var p = Process.Start(psi))
        {
            var text = p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd();
            p.WaitForExit();
            Assert.That(p.ExitCode, Is.EqualTo(0), "64tass failed:\n" + text);
        }

        var emulator = new Emulator();
        emulator.LoadPrg(prg);
        return emulator;
    }

    static string AsmFolder()
    {
        for (var d = new DirectoryInfo(AppContext.BaseDirectory); d != null; d = d.Parent)
        {
            var candidate = Path.Combine(d.FullName, "asm");
            if (Directory.Exists(Path.Combine(candidate, "helper")))
                return candidate;
        }
        throw new DirectoryNotFoundException("the repository's asm folder");
    }
}
