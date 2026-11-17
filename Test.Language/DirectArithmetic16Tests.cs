using NUnit.Framework;

namespace Compiler.LanguageTest;

// 16-bit add and subtract done straight from the operands
// (ILMethodIntOperandFusionOptimizer: arith16_direct, arith16_store and
// arith16_stack in asm/helper/arithmetic.asm) -- every shape those take,
// with values that carry or borrow between the two bytes.
class Holder16
{
    public ulong Value;

    public ulong Get() => Value;
}

// The same shapes in methods that are called, not test entry points: those
// get static frames (ILStaticFramePass), where a local has a fixed address
// and `x += 2` is done in place.
static class Calc16
{
    public static ulong AddConst(ulong a) => a + 300;

    public static ulong SubVar(ulong a, ulong b) => a - b;

    public static ulong InPlace(ulong a)
    {
        ulong x = a;
        x += 2;
        x += 255;
        x -= 1;
        x -= 255;
        x += 1000;
        return x;
    }

    public static ulong Stored(ulong a, ulong b)
    {
        ulong sum = a + b;
        ulong diff = sum - 7;
        return diff + a;
    }

    public static ulong Rows(uint rows)
    {
        ulong address = 0x0400;
        for (uint y = 0; y < rows; y++)
            address += 40;
        return address;
    }

    public static ulong FieldMinusVar(Holder16 h, ulong b) => h.Value - b;

    public static ulong CallPlusConst(Holder16 h) => h.Get() + 0x0102;
}

[TestFixture]
public class DirectArithmetic16Tests
{
    [TestCase(1u, ExpectedResult = 301)]
    [TestCase(65500u, ExpectedResult = 264)]
    public ulong StaticFrame_AddConst(ulong a)
    {
        return Calc16.AddConst(a);
    }

    [TestCase(0x0100u, 1u, ExpectedResult = 0x00FF)]
    [TestCase(3u, 5u, ExpectedResult = 65534)]
    public ulong StaticFrame_SubVar(ulong a, ulong b)
    {
        return Calc16.SubVar(a, b);
    }

    [TestCase(0u, ExpectedResult = 1001)]
    [TestCase(254u, ExpectedResult = 1255)]
    [TestCase(65535u, ExpectedResult = 1000)]
    public ulong StaticFrame_InPlace(ulong a)
    {
        return Calc16.InPlace(a);
    }

    [TestCase(100u, 200u, ExpectedResult = 393)]
    [TestCase(0xFFu, 0x08u, ExpectedResult = 0x01FF)]
    public ulong StaticFrame_Stored(ulong a, ulong b)
    {
        return Calc16.Stored(a, b);
    }

    [TestCase(7u, ExpectedResult = 1304)]
    [TestCase(25u, ExpectedResult = 2024)]
    public ulong StaticFrame_Rows(uint rows)
    {
        return Calc16.Rows(rows);
    }

    // (a second caller of each: a method called from one place is inlined)
    [TestCase(10u, 3u, ExpectedResult = 1340)]
    public ulong StaticFrame_Again(ulong a, ulong b)
    {
        ulong r = Calc16.AddConst(a);           // 310
        r = Calc16.SubVar(r, b);                // 307
        r = Calc16.InPlace(r) - 1002;           // 306
        r = Calc16.Stored(r, b);                // 306 + 3 - 7 + 306 = 608
        return r + Calc16.Rows((uint)b) - 0x0400 + 612;    // 608 + 120 + 612
    }

    [TestCase(0x0200u, 0x0101u, ExpectedResult = 0x00FF)]
    public ulong StaticFrame_FieldMinusVar(ulong a, ulong b)
    {
        return Calc16.FieldMinusVar(new Holder16 { Value = a }, b);
    }

    [TestCase(0x00FFu, ExpectedResult = 0x0201)]
    public ulong StaticFrame_CallPlusConst(ulong a)
    {
        return Calc16.CallPlusConst(new Holder16 { Value = a });
    }

    // variable + constant, pushed (returned)
    [TestCase(250u, ExpectedResult = 260)]
    [TestCase(65530u, ExpectedResult = 4)]
    [TestCase(1000u, ExpectedResult = 1010)]
    public ulong VarPlusSmallConst(ulong a)
    {
        return a + 10;
    }

    [TestCase(1u, ExpectedResult = 4661)]
    [TestCase(60000u, ExpectedResult = 64660)]
    [TestCase(0xFFu, ExpectedResult = 4915)]
    public ulong VarPlusLargeConst(ulong a)
    {
        return a + 0x1234;
    }

    [TestCase(5u, ExpectedResult = 1029)]
    [TestCase(0xFF00u, ExpectedResult = 0x0300)]
    public ulong ConstPlusVar(ulong a)
    {
        return 0x0400 + a;
    }

    [TestCase(300u, 500u, ExpectedResult = 800)]
    [TestCase(0x00FFu, 0x0001u, ExpectedResult = 0x0100)]
    [TestCase(40000u, 30000u, ExpectedResult = 4464)]
    public ulong VarPlusVar(ulong a, ulong b)
    {
        return a + b;
    }

    [TestCase(256u, ExpectedResult = 254)]
    [TestCase(1u, ExpectedResult = 65535)]
    [TestCase(1000u, ExpectedResult = 998)]
    public ulong VarMinusSmallConst(ulong a)
    {
        return a - 2;
    }

    [TestCase(0x2000u, ExpectedResult = 0x0DCC)]
    [TestCase(0x1234u, ExpectedResult = 0)]
    public ulong VarMinusLargeConst(ulong a)
    {
        return a - 0x1234;
    }

    [TestCase(1u, ExpectedResult = 999)]
    [TestCase(744u, ExpectedResult = 256)]
    [TestCase(745u, ExpectedResult = 255)]
    public ulong ConstMinusVar(ulong a)
    {
        return 1000 - a;
    }

    [TestCase(0x0100u, 0x0001u, ExpectedResult = 0x00FF)]
    [TestCase(500u, 300u, ExpectedResult = 200)]
    [TestCase(5u, 6u, ExpectedResult = 65535)]
    public ulong VarMinusVar(ulong a, ulong b)
    {
        return a - b;
    }

    // stored: into the same local (x += c, x -= c), into another one
    [TestCase(254u, ExpectedResult = 258)]
    [TestCase(10u, ExpectedResult = 14)]
    [TestCase(65534u, ExpectedResult = 2)]
    public ulong InPlaceAdd(ulong a)
    {
        ulong x = a;
        x += 2;
        x += 2;
        return x;
    }

    [TestCase(257u, ExpectedResult = 253)]
    [TestCase(10u, ExpectedResult = 6)]
    [TestCase(1u, ExpectedResult = 65533)]
    public ulong InPlaceSubtract(ulong a)
    {
        ulong x = a;
        x -= 2;
        x -= 2;
        return x;
    }

    [TestCase(200u, ExpectedResult = 1200)]
    [TestCase(65000u, ExpectedResult = 464)]
    public ulong InPlaceAddLarge(ulong a)
    {
        ulong x = a;
        x += 1000;
        return x;
    }

    [TestCase(100u, 200u, ExpectedResult = 403)]
    [TestCase(0x80u, 0x80u, ExpectedResult = 359)]
    public ulong StoreToOtherLocal(ulong a, ulong b)
    {
        ulong sum = a + b;
        ulong more = sum + 3;
        ulong both = more + a;
        return both - a + 100;
    }

    // a loop: the address arithmetic of a screen row
    [TestCase(3u, ExpectedResult = 1144)]
    [TestCase(24u, ExpectedResult = 1984)]
    public ulong RunningAddress(uint rows)
    {
        ulong address = 0x0400;
        for (uint y = 0; y < rows; y++)
            address += 40;
        return address;
    }

    // the first operand already on the stack: a field, a call's result
    [TestCase(0x01F0u, ExpectedResult = 0x0200)]
    [TestCase(0xFFFFu, ExpectedResult = 0x000F)]
    public ulong FieldPlusConst(ulong a)
    {
        var h = new Holder16 { Value = a };
        return h.Value + 16;
    }

    [TestCase(0x0200u, ExpectedResult = 0x01F0)]
    [TestCase(5u, ExpectedResult = 65525)]
    public ulong CallMinusConst(ulong a)
    {
        var h = new Holder16 { Value = a };
        return h.Get() - 16;
    }

    [TestCase(0x00FFu, 0x0101u, ExpectedResult = 0x0200)]
    [TestCase(1000u, 24u, ExpectedResult = 1024)]
    public ulong CallPlusVar(ulong a, ulong b)
    {
        var h = new Holder16 { Value = a };
        return h.Get() + b;
    }

    [TestCase(0x0200u, 0x0101u, ExpectedResult = 0x00FF)]
    [TestCase(1024u, 24u, ExpectedResult = 1000)]
    public ulong CallMinusVar(ulong a, ulong b)
    {
        var h = new Holder16 { Value = a };
        return h.Get() - b;
    }

    // either branch pushes the first operand; the constant is where they join
    // (the harness passes no bool arguments: a number)
    [TestCase(1u, ExpectedResult = 1100)]
    [TestCase(0u, ExpectedResult = 2100)]
    public ulong JoinPlusConst(uint first)
    {
        ulong a = 1000, b = 2000;
        return (first != 0 ? a : b) + 100;
    }

    // signed: a negative constant, a negative result
    [TestCase(10, ExpectedResult = 7)]
    [TestCase(1, ExpectedResult = -2)]
    [TestCase(-300, ExpectedResult = -303)]
    public long SignedPlusNegativeConst(long a)
    {
        return a + -3;
    }

    [TestCase(10, 20, ExpectedResult = -10)]
    [TestCase(-10, -20, ExpectedResult = 10)]
    public long SignedMinus(long a, long b)
    {
        return a - b;
    }
}
