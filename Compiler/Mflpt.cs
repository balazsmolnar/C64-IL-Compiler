using System;

namespace Compiler;

// Converts between .NET's IEEE754 single (what Roslyn puts in the IL for a
// float literal / Ldc_r4) and the Commodore 64's own 5-byte "MFLPT" float
// format -- the format this compiler stores `float` values in everywhere
// (see Compiler/TypeExtensions.cs's GetStorageBytes), chosen specifically so
// runtime arithmetic can hand values straight to the BASIC ROM's float
// routines (MOVFM/FADD/etc.) with zero conversion. This class is the ONE
// place that conversion happens, and it happens at compile time in C#, not
// in hand-written 6502 -- see OpLdc_r4.
//
// MFLPT layout (5 bytes, big-endian, verified against c64-wiki.com's MOVFM/
// GIVAYF pages and the worked "1.0 -> 81 00 00 00 00" example on
// c64os.com/post/floatingpointmath):
//   byte[0]      = exponent byte. 0 means the value is exactly zero
//                  (mantissa/sign bytes are then also all zero -- "canonical
//                  zero"). Otherwise byte[0] = E + 128, where the value is
//                  sign * mantissa * 2^E with mantissa normalized to
//                  [0.5, 1.0).
//   byte[1]      = sign bit (bit 7, 1 = negative) in the top bit, then the
//                  top 7 bits of the 31-bit mantissa fraction.
//   byte[2..4]   = the remaining 24 bits of the mantissa fraction.
//   (The mantissa's implicit leading 1 bit -- always set once normalized to
//   [0.5, 1.0) -- is never stored, exactly like IEEE754's own implicit bit.)
//
// IEEE754 single layout: 1 sign bit, 8-bit exponent (bias 127, mantissa
// normalized to [1.0, 2.0) with an implicit leading 1), 23 explicit mantissa
// bits. Renormalizing IEEE's [1.0,2.0) mantissa to MFLPT's [0.5,1.0) shifts
// the exponent by exactly 1 (1.xxxxx * 2^e = 0.1xxxxx * 2^(e+1)), which is
// where the "+2" / "-2" relating the two biases (127 and 128) below comes
// from: mflptExp = ieeeExp - 127 + 1 + 128 = ieeeExp + 2.
public static class Mflpt
{
    // IEEE754's 23 explicit mantissa bits become MFLPT's top 23 (of 31)
    // explicit mantissa bits, zero-padded in the remaining low 8 -- exact,
    // no rounding, since MFLPT has strictly more mantissa precision.
    public static byte[] ToBytes(float value)
    {
        uint bits = BitConverter.SingleToUInt32Bits(value);
        uint sign = (bits >> 31) & 1;
        int ieeeExp = (int)((bits >> 23) & 0xFF);
        uint ieeeMantissa = bits & 0x7FFFFF;

        if (ieeeExp == 0)
        {
            // Zero (mantissa == 0) or a subnormal. MFLPT has no subnormal
            // range this compact -- flush either to canonical zero rather
            // than attempt a partial/incorrect representation. Not expected
            // to matter for hand-written game-code float literals.
            return new byte[5];
        }
        if (ieeeExp == 0xFF)
            throw new InvalidOperationException($"Float literal {value} is Infinity/NaN, which the C64's MFLPT format cannot represent.");

        int mflptExp = ieeeExp + 2;
        if (mflptExp < 1 || mflptExp > 255)
            throw new InvalidOperationException($"Float literal {value} is out of MFLPT's representable exponent range.");

        uint mantissa31 = ieeeMantissa << 8; // 23 bits -> top 23 of 31, zero-padded

        var result = new byte[5];
        result[0] = (byte)mflptExp;
        result[1] = (byte)(((sign << 7) | ((mantissa31 >> 24) & 0x7F)));
        result[2] = (byte)((mantissa31 >> 16) & 0xFF);
        result[3] = (byte)((mantissa31 >> 8) & 0xFF);
        result[4] = (byte)(mantissa31 & 0xFF);
        return result;
    }

    // Reverse of ToBytes -- only needed to round-trip-verify ToBytes in
    // tests, never at runtime (the compiler never converts an MFLPT value
    // back to IEEE754; runtime float values stay in MFLPT end to end).
    public static float FromBytes(byte[] mflpt)
    {
        if (mflpt.Length != 5)
            throw new ArgumentException("MFLPT value must be exactly 5 bytes.", nameof(mflpt));

        if (mflpt[0] == 0)
            return 0f;

        int mflptExp = mflpt[0];
        uint sign = (uint)((mflpt[1] >> 7) & 1);
        uint mantissa31 = ((uint)(mflpt[1] & 0x7F) << 24) | ((uint)mflpt[2] << 16) | ((uint)mflpt[3] << 8) | mflpt[4];

        int ieeeExp = mflptExp - 2;
        uint ieeeMantissa = mantissa31 >> 8; // drop the low 8 bits ToBytes always zero-padded

        uint bits = (sign << 31) | ((uint)(ieeeExp & 0xFF) << 23) | (ieeeMantissa & 0x7FFFFF);
        return BitConverter.UInt32BitsToSingle(bits);
    }
}
