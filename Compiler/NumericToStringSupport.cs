using System;

namespace Compiler;

// The one mapping from a numeric CLR type to its ToString() conversion
// routine label (asm/helper/tostring.asm's four integer routines, plus
// asm/helper/float.asm's always-included float one). Shared by
// ILNumericToStringPass (rewrites x.ToString() calls) and
// ILStringInterpolationPass (rewrites an interpolation hole's implicit
// formatting the same way) -- same "single source of truth" reasoning as
// MathFSupport.cs, just for this mapping instead of a method allowlist.
static class NumericToStringSupport
{
    public static bool IsSupportedNumericType(Type type) =>
        type == typeof(uint) || type == typeof(byte) ||
        type == typeof(int) || type == typeof(sbyte) ||
        type == typeof(ulong) || type == typeof(long) ||
        type == typeof(float);

    public static string ConversionLabel(Type numericType)
    {
        if (numericType == typeof(uint) || numericType == typeof(byte))
            return "NumberFormat_UInt8ToString";
        if (numericType == typeof(int) || numericType == typeof(sbyte))
            return "NumberFormat_Int8ToString";
        if (numericType == typeof(ulong))
            return "NumberFormat_UInt16ToString";
        if (numericType == typeof(long))
            return "NumberFormat_Int16ToString";
        return "NumberFormat_FloatToString";
    }
}
