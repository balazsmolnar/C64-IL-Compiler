using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace TestDebugger;

class ViceProtocolException : Exception
{
    public ViceProtocolException(string message) : base(message) { }
}

class ViceResponse
{
    public byte Type;
    public byte Error;
    public uint RequestId;
    public byte[] Body;
}

// Client for VICE's binary remote monitor (x64sc -binarymonitor
// -binarymonitoraddress ip4://127.0.0.1:6502; VICE manual chapter 13, API
// version 2). A reader thread splits the TCP stream into packets: a packet
// whose request id matches a pending command completes that command;
// packets with request id 0xffffffff are events (registers, stopped,
// resumed, jam) and update the stop state instead.
//
// Any command sent while the machine is running stops it (the monitor is
// entered), which VICE reports as a registers event followed by a Stopped
// event -- so "pause" is just "send any command", see Stop().
class ViceMonitorClient : IDisposable
{
    const byte Stx = 0x02;
    const byte ApiVersion = 0x02;
    const uint EventId = 0xffffffff;

    const byte CmdMemGet = 0x01;
    const byte CmdMemSet = 0x02;
    const byte CmdCheckpointSet = 0x12;
    const byte CmdCheckpointDelete = 0x13;
    const byte CmdRegistersGet = 0x31;
    const byte CmdAdvance = 0x71;
    const byte CmdPing = 0x81;
    const byte CmdBanksAvailable = 0x82;
    const byte CmdRegistersAvailable = 0x83;
    const byte CmdExit = 0xaa;

    const byte RespCheckpointInfo = 0x11;
    const byte RespRegisterInfo = 0x31;
    const byte RespJam = 0x61;
    const byte RespStopped = 0x62;
    const byte RespResumed = 0x63;

    private class Pending
    {
        public byte Terminal;
        public ViceResponse Result;
        public Exception Failure;
        public readonly ManualResetEventSlim Done = new(false);
    }

    private readonly TcpClient _tcp;
    private readonly NetworkStream _stream;
    private readonly Thread _reader;
    private readonly object _writeLock = new();
    private readonly object _stateLock = new();
    private readonly Dictionary<uint, Pending> _pending = new();
    private uint _nextRequestId = 1;
    private bool _closed;

    private bool _stopped;
    private bool _jammed;
    private int _pc;
    private Dictionary<byte, ushort> _registersAtStop;

    private ViceMonitorClient(TcpClient tcp)
    {
        _tcp = tcp;
        _stream = tcp.GetStream();
        _reader = new Thread(ReadLoop) { IsBackground = true, Name = "VICE monitor reader" };
        _reader.Start();
    }

    // Retries until VICE's monitor port accepts, since VICE may still be
    // starting up when this is called.
    public static ViceMonitorClient Connect(string host, int port, int timeoutMs = 10000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (true)
        {
            try
            {
                var tcp = new TcpClient();
                tcp.NoDelay = true;
                tcp.Connect(host, port);
                return new ViceMonitorClient(tcp);
            }
            catch (SocketException) when (DateTime.UtcNow < deadline)
            {
                Thread.Sleep(200);
            }
        }
    }

    public bool IsStopped { get { lock (_stateLock) return _stopped; } }
    public bool Jammed { get { lock (_stateLock) return _jammed; } }
    public int Pc { get { lock (_stateLock) return _pc; } }

    // --- Commands -------------------------------------------------------

    public void Ping() => Command(CmdPing, Array.Empty<byte>(), CmdPing);

    // Bank name -> bank id (e.g. "cpu", "ram", "rom", "io").
    public Dictionary<string, ushort> GetBanks()
    {
        var body = Command(CmdBanksAvailable, Array.Empty<byte>(), CmdBanksAvailable).Body;
        var result = new Dictionary<string, ushort>();
        int count = BinaryPrimitives.ReadUInt16LittleEndian(body);
        int p = 2;
        for (int i = 0; i < count; i++)
        {
            int size = body[p];
            ushort id = BinaryPrimitives.ReadUInt16LittleEndian(body.AsSpan(p + 1));
            int nameLength = body[p + 3];
            result[System.Text.Encoding.ASCII.GetString(body, p + 4, nameLength)] = id;
            p += 1 + size;
        }
        return result;
    }

    // Register name (e.g. "A", "PC", "SP", "FL") -> register id.
    public Dictionary<string, byte> GetRegisterIds()
    {
        var body = Command(CmdRegistersAvailable, new byte[] { 0 }, CmdRegistersAvailable).Body;
        var result = new Dictionary<string, byte>();
        int count = BinaryPrimitives.ReadUInt16LittleEndian(body);
        int p = 2;
        for (int i = 0; i < count; i++)
        {
            int size = body[p];
            byte id = body[p + 1];
            int nameLength = body[p + 3];
            result[System.Text.Encoding.ASCII.GetString(body, p + 4, nameLength)] = id;
            p += 1 + size;
        }
        return result;
    }

    // Register id -> value.
    public Dictionary<byte, ushort> GetRegisters()
    {
        var body = Command(CmdRegistersGet, new byte[] { 0 }, RespRegisterInfo).Body;
        return ParseRegisters(body);
    }

    // Reads start..endInclusive from the given bank (main memory space).
    public byte[] ReadMemory(int start, int endInclusive, ushort bank)
    {
        var request = new byte[8];
        request[0] = 0; // no side effects
        BinaryPrimitives.WriteUInt16LittleEndian(request.AsSpan(1), (ushort)start);
        BinaryPrimitives.WriteUInt16LittleEndian(request.AsSpan(3), (ushort)endInclusive);
        request[5] = 0; // main memory
        BinaryPrimitives.WriteUInt16LittleEndian(request.AsSpan(6), bank);
        var body = Command(CmdMemGet, request, CmdMemGet).Body;
        int length = BinaryPrimitives.ReadUInt16LittleEndian(body);
        var result = new byte[length];
        Array.Copy(body, 2, result, 0, length);
        return result;
    }

    public void WriteMemory(int start, byte[] data, ushort bank)
    {
        var request = new byte[8 + data.Length];
        request[0] = 0;
        BinaryPrimitives.WriteUInt16LittleEndian(request.AsSpan(1), (ushort)start);
        BinaryPrimitives.WriteUInt16LittleEndian(request.AsSpan(3), (ushort)(start + data.Length - 1));
        request[5] = 0;
        BinaryPrimitives.WriteUInt16LittleEndian(request.AsSpan(6), bank);
        Array.Copy(data, 0, request, 8, data.Length);
        Command(CmdMemSet, request, CmdMemSet);
    }

    // Sets an execute checkpoint that stops the machine; returns its number.
    public uint SetBreakpoint(int address)
    {
        var request = new byte[8];
        BinaryPrimitives.WriteUInt16LittleEndian(request.AsSpan(0), (ushort)address);
        BinaryPrimitives.WriteUInt16LittleEndian(request.AsSpan(2), (ushort)address);
        request[4] = 1; // stop when hit
        request[5] = 1; // enabled
        request[6] = 4; // exec
        request[7] = 0; // not temporary
        var body = Command(CmdCheckpointSet, request, RespCheckpointInfo).Body;
        return BinaryPrimitives.ReadUInt32LittleEndian(body);
    }

    public void DeleteBreakpoint(uint number)
    {
        var request = new byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(request, number);
        Command(CmdCheckpointDelete, request, CmdCheckpointDelete);
    }

    // Resumes execution until the next checkpoint (or until Stop()).
    public void Resume()
    {
        MarkRunning();
        Command(CmdExit, Array.Empty<byte>(), CmdExit);
    }

    // Executes `count` instructions, then the machine stops again.
    public void Advance(int count, bool stepOverSubroutines)
    {
        var request = new byte[3];
        request[0] = (byte)(stepOverSubroutines ? 1 : 0);
        BinaryPrimitives.WriteUInt16LittleEndian(request.AsSpan(1), (ushort)count);
        MarkRunning();
        Command(CmdAdvance, request, CmdAdvance);
    }

    // Enters the monitor if the machine is running: any command does that,
    // so a ping is enough. Returns false if it doesn't report stopped in time.
    public bool Stop(int timeoutMs = 5000)
    {
        if (IsStopped)
            return true;
        Ping();
        return WaitForStop(timeoutMs);
    }

    // For attaching to a machine that was already stopped in the monitor
    // before we connected: no Stopped event will ever arrive for that, so the
    // caller establishes the state itself from a registers read.
    public void AssumeStopped(int pc)
    {
        lock (_stateLock)
        {
            _stopped = true;
            _pc = pc;
            Monitor.PulseAll(_stateLock);
        }
    }

    // timeoutMs < 0 waits indefinitely (until stopped or the connection closes).
    public bool WaitForStop(int timeoutMs)
    {
        var deadline = timeoutMs < 0 ? DateTime.MaxValue : DateTime.UtcNow.AddMilliseconds(timeoutMs);
        lock (_stateLock)
        {
            while (!_stopped)
            {
                if (_closed)
                    throw new ViceProtocolException("Connection to VICE is closed.");
                var remaining = deadline - DateTime.UtcNow;
                if (remaining <= TimeSpan.Zero)
                    return false;
                // Wake up periodically: Monitor.Wait can't take an
                // unbounded TimeSpan, and this also re-checks _closed.
                Monitor.Wait(_stateLock, remaining > TimeSpan.FromSeconds(30) ? TimeSpan.FromSeconds(30) : remaining);
            }
            return true;
        }
    }

    // --- Plumbing -------------------------------------------------------

    private void MarkRunning()
    {
        lock (_stateLock)
        {
            _stopped = false;
            _jammed = false;
            _registersAtStop = null;
        }
    }

    private ViceResponse Command(byte command, byte[] body, byte terminalType, int timeoutMs = 10000)
    {
        var pending = new Pending { Terminal = terminalType };
        uint id;
        lock (_writeLock)
        {
            if (_closed)
                throw new ViceProtocolException("Connection to VICE is closed.");
            id = _nextRequestId++;
            if (id == EventId)
                id = _nextRequestId++;
            lock (_pending)
                _pending[id] = pending;

            var packet = new byte[11 + body.Length];
            packet[0] = Stx;
            packet[1] = ApiVersion;
            BinaryPrimitives.WriteUInt32LittleEndian(packet.AsSpan(2), (uint)body.Length);
            BinaryPrimitives.WriteUInt32LittleEndian(packet.AsSpan(6), id);
            packet[10] = command;
            Array.Copy(body, 0, packet, 11, body.Length);
            _stream.Write(packet, 0, packet.Length);
        }

        if (!pending.Done.Wait(timeoutMs))
        {
            lock (_pending)
                _pending.Remove(id);
            throw new ViceProtocolException($"Timed out waiting for VICE to answer command 0x{command:X2}.");
        }
        if (pending.Failure != null)
            throw pending.Failure;

        var response = pending.Result;
        if (response.Error != 0)
            throw new ViceProtocolException($"VICE rejected command 0x{command:X2} with error 0x{response.Error:X2}.");
        return response;
    }

    private void ReadLoop()
    {
        var header = new byte[12];
        try
        {
            while (true)
            {
                ReadExactly(header, 12);
                if (header[0] != Stx)
                    throw new ViceProtocolException($"Bad packet start byte 0x{header[0]:X2}.");
                int length = (int)BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(2));
                var response = new ViceResponse
                {
                    Type = header[6],
                    Error = header[7],
                    RequestId = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(8)),
                    Body = new byte[length],
                };
                ReadExactly(response.Body, length);
                Dispatch(response);
            }
        }
        catch (Exception ex)
        {
            Fail(ex is ViceProtocolException ? ex : new ViceProtocolException("Connection to VICE lost: " + ex.Message));
        }
    }

    private void ReadExactly(byte[] buffer, int count)
    {
        int read = 0;
        while (read < count)
        {
            int n = _stream.Read(buffer, read, count - read);
            if (n == 0)
                throw new EndOfStreamException("VICE closed the connection.");
            read += n;
        }
    }

    private void Dispatch(ViceResponse response)
    {
        if (response.RequestId == EventId)
        {
            HandleEvent(response);
            return;
        }

        Pending pending;
        lock (_pending)
            _pending.TryGetValue(response.RequestId, out pending);
        if (pending == null)
            return;
        if (response.Error != 0 || response.Type == pending.Terminal)
        {
            lock (_pending)
                _pending.Remove(response.RequestId);
            pending.Result = response;
            pending.Done.Set();
        }
        // else: an intermediate response for the same request (e.g. one of
        // several checkpoint infos) -- keep waiting for the terminal one.
    }

    private void HandleEvent(ViceResponse response)
    {
        lock (_stateLock)
        {
            switch (response.Type)
            {
                case RespRegisterInfo:
                    _registersAtStop = ParseRegisters(response.Body);
                    break;
                case RespStopped:
                    _stopped = true;
                    _pc = BinaryPrimitives.ReadUInt16LittleEndian(response.Body);
                    Monitor.PulseAll(_stateLock);
                    break;
                case RespJam:
                    _jammed = true;
                    _stopped = true;
                    _pc = BinaryPrimitives.ReadUInt16LittleEndian(response.Body);
                    Monitor.PulseAll(_stateLock);
                    break;
                case RespResumed:
                    _stopped = false;
                    break;
            }
        }
    }

    private static Dictionary<byte, ushort> ParseRegisters(byte[] body)
    {
        var result = new Dictionary<byte, ushort>();
        int count = BinaryPrimitives.ReadUInt16LittleEndian(body);
        int p = 2;
        for (int i = 0; i < count; i++)
        {
            int size = body[p];
            byte id = body[p + 1];
            result[id] = BinaryPrimitives.ReadUInt16LittleEndian(body.AsSpan(p + 2));
            p += 1 + size;
        }
        return result;
    }

    private void Fail(Exception ex)
    {
        lock (_stateLock)
        {
            _closed = true;
            Monitor.PulseAll(_stateLock);
        }
        List<Pending> pendings;
        lock (_pending)
        {
            pendings = new List<Pending>(_pending.Values);
            _pending.Clear();
        }
        foreach (var p in pendings)
        {
            p.Failure = ex;
            p.Done.Set();
        }
    }

    public void Dispose()
    {
        try { _tcp.Close(); } catch { }
    }
}
