using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace TestDebugger;

// One incoming Debug Adapter Protocol message
// (https://microsoft.github.io/debug-adapter-protocol/specification),
// decoded just enough to dispatch on -- DapServer reads specific fields out
// of Arguments itself per-command.
class DapIncomingMessage
{
    public int Seq;
    public string Type;
    public string Command;
    public JsonElement Arguments;
}

// Reads/writes DAP's "Content-Length: N\r\n\r\n<json>" framing over raw
// stdio streams (not Console.In/Out, whose platform newline translation and
// buffering aren't appropriate for a byte-exact framed protocol). Kept
// minimal -- just enough request/response/event shape to drive VS Code, not
// a full DAP type library.
class DapIo
{
    private readonly Stream _input;
    private readonly Stream _output;
    private int _seq = 1;

    public DapIo(Stream input, Stream output)
    {
        _input = input;
        _output = output;
    }

    // Returns null when the client has disconnected (stdin closed).
    public DapIncomingMessage ReadMessage()
    {
        int contentLength = -1;
        while (true)
        {
            var line = ReadHeaderLine();
            if (line == null)
                return null;
            if (line.Length == 0)
                break;
            if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                contentLength = int.Parse(line.Substring("Content-Length:".Length).Trim());
        }
        if (contentLength < 0)
            throw new InvalidOperationException("DAP message missing Content-Length header.");

        var buffer = new byte[contentLength];
        int read = 0;
        while (read < contentLength)
        {
            int n = _input.Read(buffer, read, contentLength - read);
            if (n <= 0)
                return null;
            read += n;
        }

        using var doc = JsonDocument.Parse(buffer);
        var root = doc.RootElement;
        return new DapIncomingMessage
        {
            Seq = root.GetProperty("seq").GetInt32(),
            Type = root.GetProperty("type").GetString(),
            Command = root.TryGetProperty("command", out var c) ? c.GetString() : null,
            Arguments = root.TryGetProperty("arguments", out var a) ? a.Clone() : default,
        };
    }

    private string ReadHeaderLine()
    {
        var sb = new StringBuilder();
        int b;
        bool any = false;
        while ((b = _input.ReadByte()) != -1)
        {
            any = true;
            if (b == '\n')
            {
                if (sb.Length > 0 && sb[sb.Length - 1] == '\r')
                    sb.Length--;
                return sb.ToString();
            }
            sb.Append((char)b);
        }
        return any ? sb.ToString() : null;
    }

    public void WriteResponse(int requestSeq, string command, bool success, object body, string message = null)
    {
        var envelope = new Dictionary<string, object>
        {
            ["seq"] = _seq++,
            ["type"] = "response",
            ["request_seq"] = requestSeq,
            ["success"] = success,
            ["command"] = command,
        };
        if (message != null)
            envelope["message"] = message;
        if (body != null)
            envelope["body"] = body;
        Write(envelope);
    }

    public void WriteEvent(string eventName, object body)
    {
        var envelope = new Dictionary<string, object>
        {
            ["seq"] = _seq++,
            ["type"] = "event",
            ["event"] = eventName,
        };
        if (body != null)
            envelope["body"] = body;
        Write(envelope);
    }

    private void Write(object envelope)
    {
        var json = JsonSerializer.Serialize(envelope);
        var bytes = Encoding.UTF8.GetBytes(json);
        var header = Encoding.ASCII.GetBytes($"Content-Length: {bytes.Length}\r\n\r\n");
        lock (_output)
        {
            _output.Write(header, 0, header.Length);
            _output.Write(bytes, 0, bytes.Length);
            _output.Flush();
        }
    }
}
