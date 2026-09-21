using System;
using System.Diagnostics;
using System.IO;

namespace TestDebugger;

// Compiles Test's already-built DLL with Compiler.exe's debug flag into a
// SEPARATE output tree (asm/unittest_debug/, prg/unittest_debug.*) so this
// never touches prg/unittest.prg/asm/unittest.asm that the real NUnit run
// depends on. Mirrors tools/compile-and-assemble.bat's --unittest flow, but
// invoked directly (not through the .bat) since that script doesn't know
// about the new 5th "debug" arg.
class TestCompiler
{
    public string RepoRoot { get; }
    public string TestDllPath { get; }
    public string AsmOutDir { get; }
    public string AsmEntryFile { get; }
    public string PrgDir { get; }
    public string PrgName => "unittest_debug";
    public string DebugMapPath => Path.Combine(AsmOutDir, "debugmap.txt");
    public string LabelsPath => Path.Combine(PrgDir, PrgName + ".labels");
    public string PrgPath => Path.Combine(PrgDir, PrgName + ".prg");
    public string DumpListingPath => Path.Combine(PrgDir, "dump_debug.asm");

    public TestCompiler(string repoRoot)
    {
        RepoRoot = repoRoot;
        TestDllPath = Path.Combine(repoRoot, "Test", "obj", "Debug", "Before-PostSharp", "Compiler.Test.dll");
        AsmOutDir = Path.Combine(repoRoot, "asm", "unittest_debug");
        AsmEntryFile = Path.Combine(repoRoot, "asm", "unittest_debug.asm");
        PrgDir = Path.Combine(repoRoot, "prg");
    }

    // Skips recompiling if the debug map is already newer than the test
    // DLL, unless forceRecompile is set.
    public void EnsureCompiled(bool forceRecompile)
    {
        if (!File.Exists(TestDllPath))
            throw new InvalidOperationException(
                $"Test DLL not found at {TestDllPath}. Build Test/Compiler.Test.csproj first (dotnet build).");

        if (!forceRecompile && File.Exists(DebugMapPath) && File.Exists(PrgPath) &&
            File.GetLastWriteTimeUtc(DebugMapPath) >= File.GetLastWriteTimeUtc(TestDllPath))
        {
            Console.WriteLine("(using cached debug build)");
            return;
        }

        Directory.CreateDirectory(AsmOutDir);
        Directory.CreateDirectory(PrgDir);

        var compilerExe = Environment.GetEnvironmentVariable("COMPILER_EXE")
            ?? Path.Combine(RepoRoot, "Compiler", "bin", "Debug", "Compiler.exe");
        var tassExe = Environment.GetEnvironmentVariable("TASS_EXE")
            ?? @"c:\tools\64tass-1.60.3243\64tass.exe";

        Console.Error.WriteLine("Compiling test assembly with debug info...");
        RunProcess(compilerExe, $"\"{TestDllPath}\" \"{AsmOutDir}\" \"{AsmEntryFile}\" unittest debug");

        if (File.Exists(PrgPath))
            File.Delete(PrgPath);

        Console.Error.WriteLine("Assembling...");
        RunProcess(tassExe,
            $"-o \"{PrgPath}\" --long-branch --vice-labels -l \"{LabelsPath}\" --list \"{DumpListingPath}\" --no-monitor \"{AsmEntryFile}\"");
    }

    // Redirects the child's stdout/stderr to OUR stderr, never our stdout --
    // critical when running as a DAP adapter (DapServer.cs), where stdout is
    // a JSON-RPC-ish protocol stream that a stray "Assembling file: ..." line
    // from Compiler.exe/64tass would corrupt. Harmless for the plain REPL
    // too (stderr shows in the same terminal).
    private static void RunProcess(string exe, string arguments)
    {
        var psi = new ProcessStartInfo(exe, arguments)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        using var process = Process.Start(psi);
        process.OutputDataReceived += (_, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"{Path.GetFileName(exe)} exited with code {process.ExitCode}");
    }
}
