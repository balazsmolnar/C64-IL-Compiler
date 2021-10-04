using System;
using System.Reflection;

namespace Compiler
{
    static class StringExtensions
    {
        static public string ToValidName(this string s)
        {
            var result = s
                ?.Replace('.', '_')
                ?.Replace('<', '_')
                ?.Replace('>', '_')
                ?.Replace('`', '_');

            return result.StartsWith('_') ? "x" + result : result;
        }
    }
}