using System;
using System.Reflection;

namespace Compiler;

static class StringExtensions
{
    static public string ToValidName(this string s)
    {
        // Anything that isn't a plain identifier character becomes '_': '.', '+'
        // (nested types), '<' '>' '`' (generics, closures) and, for local
        // functions, the '|' and '$' Roslyn puts in their generated names
        // ("<Run>g__Add|0_0"), which the assembler rejects in a label.
        if (s == null)
            return null;
        var chars = s.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            if (!(char.IsAsciiLetterOrDigit(chars[i]) || chars[i] == '_'))
                chars[i] = '_';
        }
        var result = new string(chars);

        return result.StartsWith('_') ? "x" + result : result;
    }
}