using System;
using System.Collections.Generic;
using System.Linq;
using SimpleEmulator;

namespace TestDebugger;

// IDebugTarget backed by a running VICE, over its binary remote monitor.
// Memory is read from the machine's RAM bank (not the CPU view), so ROM
// banking ($01) never hides program data -- the compiled program keeps its
// heap, locals stack and object tables in RAM regardless of what BASIC/
// KERNAL ROM is currently mapped. Reads are cached by 256-byte page and the
// cache is dropped whenever the machine runs.
class ViceTarget : IDebugTarget, IDisposable
{
    private readonly ViceMonitorClient _client;
    private readonly ushort _ramBank;
    private readonly byte _regA, _regX, _regY, _regPc, _regSp, _regFlags;
    private readonly Dictionary<int, uint> _breakpointNumbers = new();
    private readonly byte[][] _pages = new byte[256][];
    private Dictionary<byte, ushort> _registers;

    // How long RunUntil waits for a breakpoint before pausing the machine and
    // reporting StepLimitReached. VICE runs in real time, so there's no
    // instruction count to bound; this is the equivalent guard against a
    // program that never reaches any breakpoint. Negative = wait forever
    // (an interactive debug session, where the user pauses or stops it).
    public int RunTimeoutMs { get; set; } = 30000;

    public ViceTarget(ViceMonitorClient client)
    {
        _client = client;

        var banks = client.GetBanks();
        _ramBank = banks.TryGetValue("ram", out var ram) ? ram
                 : banks.TryGetValue("default", out var def) ? def
                 : (ushort)0;

        var regs = client.GetRegisterIds();
        _regA = regs["A"];
        _regX = regs["X"];
        _regY = regs["Y"];
        _regPc = regs["PC"];
        _regSp = regs["SP"];
        _regFlags = regs.TryGetValue("FL", out var fl) ? fl : regs["P"];

        // A running machine stops (and reports it) when we send a command; one
        // that was already sitting in the monitor never reports anything, so
        // fall back to reading the PC directly.
        if (!client.Stop(1500))
            client.AssumeStopped(client.GetRegisters()[_regPc]);
    }

    public byte GetMemory(int address)
    {
        address &= 0xffff;
        var page = _pages[address >> 8];
        if (page == null)
        {
            int start = address & 0xff00;
            page = _client.ReadMemory(start, start + 0xff, _ramBank);
            _pages[address >> 8] = page;
        }
        return page[address & 0xff];
    }

    // Invalidates every page the write touches (not just the start page --
    // a multi-byte write can cross a 256-byte boundary) so a later
    // GetMemory re-fetches from VICE instead of serving a stale cached
    // read.
    public void SetMemory(int address, params byte[] value)
    {
        address &= 0xffff;
        _client.WriteMemory(address, value, _ramBank);
        for (int a = address; a < address + value.Length; a++)
            _pages[(a & 0xffff) >> 8] = null;
    }

    public int ProgramCounter => _client.Pc;

    public byte HardwareStackPointer => (byte)Registers16()[_regSp];

    public EmulatorRegisters Registers
    {
        get
        {
            var r = Registers16();
            byte p = (byte)r[_regFlags];
            return new EmulatorRegisters(
                (byte)r[_regA], (byte)r[_regX], (byte)r[_regY], p,
                n: (p & 0x80) != 0, c: (p & 0x01) != 0, z: (p & 0x02) != 0,
                v: (p & 0x40) != 0, i: (p & 0x04) != 0);
        }
    }

    public RunResult RunUntil(HashSet<int> breakpointAddresses, long maxSteps, out int stoppedAtAddress, out long stepsExecuted)
    {
        SyncBreakpoints(breakpointAddresses);
        Invalidate();
        stepsExecuted = 0; // VICE doesn't report an instruction count

        _client.Resume();

        if (!_client.WaitForStop(RunTimeoutMs))
        {
            _client.Stop();
            stoppedAtAddress = _client.Pc;
            return RunResult.StepLimitReached;
        }

        stoppedAtAddress = _client.Pc;
        if (_client.Jammed)
            return RunResult.Halted;
        return breakpointAddresses.Contains(stoppedAtAddress) ? RunResult.Breakpoint : RunResult.StepLimitReached;
    }

    // Stops a running machine (from another thread than the one blocked in
    // RunUntil, which then returns). No-op if it is already stopped.
    public void Pause() => _client.Stop();

    public bool StepOne()
    {
        Invalidate();
        _client.Advance(1, stepOverSubroutines: false);
        _client.WaitForStop(5000);
        return !_client.Jammed;
    }

    // Makes VICE's checkpoints match `wanted` exactly: deletes ones no
    // longer wanted, adds new ones. Only touches the difference, since each
    // change is a round trip to VICE.
    private void SyncBreakpoints(HashSet<int> wanted)
    {
        foreach (var address in _breakpointNumbers.Keys.Where(a => !wanted.Contains(a)).ToList())
        {
            _client.DeleteBreakpoint(_breakpointNumbers[address]);
            _breakpointNumbers.Remove(address);
        }
        foreach (var address in wanted)
        {
            if (!_breakpointNumbers.ContainsKey(address))
                _breakpointNumbers[address] = _client.SetBreakpoint(address);
        }
    }

    private Dictionary<byte, ushort> Registers16()
    {
        return _registers ??= _client.GetRegisters();
    }

    private void Invalidate()
    {
        Array.Clear(_pages, 0, _pages.Length);
        _registers = null;
    }

    public void Dispose() => _client.Dispose();
}
