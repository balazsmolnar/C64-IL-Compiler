using System;
using System.Collections.Generic;

namespace TestDebugger;

// Parses a REPL "print" argument like "arr[3].Child.Id" into a root local
// name and a list of segments to walk via ObjectInspector.Expand. Each
// segment string is left exactly as ExpandArray/ExpandObject name their
// children ("[3]" for an array index, "Child"/"Id" for a field), so
// looking one up is a plain string-equality scan -- no separate
// index-vs-field-name branch needed downstream.
static class VariablePath
{
    public static (string Root, List<string> Segments) Parse(string path)
    {
        var segments = new List<string>();
        var rootEnd = path.IndexOfAny(new[] { '.', '[' });
        var root = rootEnd < 0 ? path : path.Substring(0, rootEnd);
        var i = rootEnd < 0 ? path.Length : rootEnd;

        while (i < path.Length)
        {
            if (path[i] == '.')
            {
                i++;
                var start = i;
                while (i < path.Length && path[i] != '.' && path[i] != '[')
                    i++;
                if (i == start)
                    throw new ArgumentException($"Empty field name in \"{path}\"");
                segments.Add(path.Substring(start, i - start));
            }
            else if (path[i] == '[')
            {
                var start = i;
                while (i < path.Length && path[i] != ']')
                    i++;
                if (i >= path.Length)
                    throw new ArgumentException($"Unclosed '[' in \"{path}\"");
                i++; // include ']'
                segments.Add(path.Substring(start, i - start));
            }
            else
            {
                throw new ArgumentException($"Unexpected character '{path[i]}' in \"{path}\"");
            }
        }

        return (root, segments);
    }
}
