using System;
using System.IO;
using System.Linq;

namespace TestDebugger;

// Builds a debug copy of one of the repo's C64 programs (Demo, Hunchback,
// C64Presentation): `dotnet build` of the project, then Compiler.exe with
// the debug flag and 64tass, into a SEPARATE output tree
// (asm/<name>_debug/, asm/<name>_debug.asm, prg/<name>_debug.*) so the
// normal asm/generated + prg/main.prg build is never touched. Program
// counterpart of TestCompiler.
class ProgramCompiler
{
    public string RepoRoot { get; }
    public string Project { get; }
    public string DllPath { get; private set; }

    private string Name => Project.ToLowerInvariant() + "_debug";
    public string AsmOutDir => Path.Combine(RepoRoot, "asm", Name);
    public string AsmEntryFile => Path.Combine(RepoRoot, "asm", Name + ".asm");
    public string PrgPath => Path.Combine(RepoRoot, "prg", Name + ".prg");
    public string LabelsPath => Path.Combine(RepoRoot, "prg", Name + ".labels");
    public string DumpListingPath => Path.Combine(RepoRoot, "prg", "dump_" + Name + ".asm");
    public string DebugMapPath => Path.Combine(AsmOutDir, "debugmap.txt");
    public string SourceDir => Path.Combine(RepoRoot, Project);

    public ProgramCompiler(string repoRoot, string project)
    {
        RepoRoot = repoRoot;
        Project = project;
    }

    public void Build()
    {
        var csproj = Path.Combine(RepoRoot, Project, Project + ".csproj");
        if (!File.Exists(csproj))
            throw new InvalidOperationException($"No project {csproj}.");

        // These projects have no post-build step of their own (the .bat
        // files do the compile/assemble/launch), so building is side-effect
        // free.
        TestCompiler.RunProcess("dotnet", $"build \"{csproj}\" -nologo -v q");

        var binDir = Path.Combine(RepoRoot, Project, "bin", "Debug");
        DllPath = Directory.GetFiles(binDir, Project + ".dll", SearchOption.AllDirectories)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault()
            ?? throw new InvalidOperationException($"{Project}.dll not found under {binDir} after building.");

        Directory.CreateDirectory(AsmOutDir);
        Directory.CreateDirectory(Path.GetDirectoryName(PrgPath));

        var compilerExe = Environment.GetEnvironmentVariable("COMPILER_EXE")
            ?? Path.Combine(RepoRoot, "Compiler", "bin", "Debug", "Compiler.exe");
        var tassExe = Environment.GetEnvironmentVariable("TASS_EXE")
            ?? @"c:\tools\64tass-1.60.3243\64tass.exe";

        TestCompiler.RunProcess(compilerExe, $"\"{DllPath}\" \"{AsmOutDir}\" \"{AsmEntryFile}\" program debug");

        if (File.Exists(PrgPath))
            File.Delete(PrgPath);
        TestCompiler.RunProcess(tassExe,
            $"-o \"{PrgPath}\" --long-branch --vice-labels -l \"{LabelsPath}\" --list \"{DumpListingPath}\" --no-monitor \"{AsmEntryFile}\"");
    }
}
