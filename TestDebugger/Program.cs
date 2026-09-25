using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace TestDebugger;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--dap")
        {
            var repoRootForDap = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
            var server = new DapServer(repoRootForDap, Console.OpenStandardInput(), Console.OpenStandardOutput());
            server.Run();
            return 0;
        }

        if (args.Length > 0 && args[0] == "vice-probe")
            return ViceProbe.Run(args);

        string testSelector = null;
        var breakSpecs = new List<string>();
        var forceRecompile = false;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--break")
            {
                breakSpecs.Add(args[++i]);
            }
            else if (args[i] == "--force-recompile")
            {
                forceRecompile = true;
            }
            else if (testSelector == null)
            {
                testSelector = args[i];
            }
            else
            {
                Console.Error.WriteLine($"Unrecognized argument: {args[i]}");
                return 1;
            }
        }

        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        var compiler = new TestCompiler(repoRoot);

        try
        {
            compiler.EnsureCompiled(forceRecompile);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Compile/assemble failed: {ex.Message}");
            return 1;
        }

        var model = DebugMapModel.Load(compiler.DebugMapPath, compiler.LabelsPath);
        var listing = DisassemblyListing.Load(compiler.DumpListingPath);
        var testAssembly = Assembly.LoadFrom(compiler.TestDllPath);
        var session = new TestSession(testAssembly, model, compiler.PrgPath);
        var resolver = new BreakpointResolver(model);
        var repl = new ReplLoop(session, model, resolver, listing);

        foreach (var spec in breakSpecs)
            repl.AddBreakpoint(spec);

        if (testSelector != null)
            repl.RunTest(testSelector);

        repl.Loop();
        return 0;
    }
}
