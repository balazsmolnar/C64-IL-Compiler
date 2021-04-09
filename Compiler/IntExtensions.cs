using System;
using System.Reflection;

namespace Compiler
{
    static class IntExtensions
    {
        static public byte ToByte(this int i)
        {
            return i < 0 ? (byte)(256 + i) : (byte)i;
        }
    }
}