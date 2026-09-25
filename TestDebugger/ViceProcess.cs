using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace TestDebugger;

// Starts x64sc with its binary monitor listening on a free localhost port and
// the given .prg autostarted. The machine runs on its own; the debugger
// attaches to the monitor port (ViceMonitorClient) and takes control from
// there.
class ViceProcess : IDisposable
{
    private Process _process;

    public int Port { get; private set; }
    public bool HasExited => _process == null || _process.HasExited;

    public event Action Exited;

    public static ViceProcess Start(string viceExe, string prgPath)
    {
        viceExe ??= Environment.GetEnvironmentVariable("VICE_EXE") ?? @"c:\tools\VICE\bin\x64sc.exe";

        var vice = new ViceProcess { Port = FreePort() };
        var psi = new ProcessStartInfo(viceExe)
        {
            UseShellExecute = false,
            // VICE logs to stdout, which here is the DAP channel -- letting
            // it inherit that handle would corrupt the protocol stream.
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        // Refresh the display when entering the monitor and after each monitor
        // command, so a program stopped at a breakpoint shows its current
        // screen ("-" enables a boolean VICE option, "+" disables it).
        psi.ArgumentList.Add("-refreshonbreak");
        psi.ArgumentList.Add("-binarymonitor");
        psi.ArgumentList.Add("-binarymonitoraddress");
        psi.ArgumentList.Add($"ip4://127.0.0.1:{vice.Port}");
        psi.ArgumentList.Add(prgPath);

        vice._process = Process.Start(psi) ?? throw new InvalidOperationException($"Could not start {viceExe}.");
        vice._process.OutputDataReceived += (_, e) => { if (e.Data != null) Console.Error.WriteLine("[vice] " + e.Data); };
        vice._process.ErrorDataReceived += (_, e) => { if (e.Data != null) Console.Error.WriteLine("[vice] " + e.Data); };
        vice._process.BeginOutputReadLine();
        vice._process.BeginErrorReadLine();
        vice._process.EnableRaisingEvents = true;
        vice._process.Exited += (_, _) => vice.Exited?.Invoke();
        return vice;
    }

    private static int FreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public void Dispose()
    {
        try
        {
            if (_process != null && !_process.HasExited)
                _process.Kill();
        }
        catch { }
    }
}
