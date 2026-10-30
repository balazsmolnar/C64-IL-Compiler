using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.Test;

// And/Or/Xor with a constant operand (ILMethodIntOperandFusionOptimizer):
// a value already on the stack becomes `pla; and #c; pha` (and8_const and
// friends, asm/helper/arithmetic.asm), a simple leaf is loaded straight
// into A, and each byte whose constant is an identity (and #$ff, ora #0,
// eor #0) or absorbing (and #0, ora #$ff) takes a shorter form. Operands
// come in as parameters so Roslyn cannot fold anything. Few methods with
// several checks each: this program is close to its memory ceiling.
[TestFixture]
public class LogicConstFusionTests
{
    static uint field8_;

    // A leaf (local, parameter, static field) and a computed value (already
    // on the evaluation stack) with a constant, every operator, including
    // the identity and absorbing constants.
    [TestCase(0xB7u, 0x30u, 0x17u)]
    public void Byte_With_Constant(uint x, uint y, uint z)
    {
        uint local = x;
        Assert.AreEqual(local & 0x0F, 0x07u);
        Assert.AreEqual(local | 0xF0, 0xF7u);
        Assert.AreEqual(x ^ 0xFF, 0x48u);
        Assert.AreEqual((y + z) & 0xF0, 0x40u);
        Assert.AreEqual((y + z) | 0x81, 0xC7u);
        Assert.AreEqual((y + z) ^ 0xFF, 0xB8u);
        Assert.AreEqual((y + z) & 0xFF, 0x47u);
        Assert.AreEqual((y + z) & 0x00, 0x00u);
        Assert.AreEqual((y + z) | 0xFF, 0xFFu);
        Assert.AreEqual((y + z) ^ 0x00, 0x47u);
        field8_ = x;
        Assert.AreEqual(field8_ & 0x0F, 0x07u);
    }

    // The constant is a branch target: the operand arrives from either arm
    // of the conditional, the constant is pushed after the join.
    [TestCase(1u, 0x3Du, 0x52u, ExpectedResult = 0x05u)]
    [TestCase(0u, 0x3Du, 0x52u, ExpectedResult = 0x02u)]
    public uint Conditional_Byte_And_Constant(uint pick, uint x, uint y) => (pick != 0 ? x : y) & 0x07;

    // Two leaves (no constant): the fused and8_tail/or8_tail/xor8_tail forms.
    [TestCase(0xCAu, 0x5Fu)]
    public void Byte_With_Byte(uint x, uint y)
    {
        uint a = x;
        uint b = y;
        Assert.AreEqual(a & b, 0x4Au);
        Assert.AreEqual(a | b, 0xDFu);
        Assert.AreEqual(a ^ b, 0x95u);
    }

    // The second leaf is a branch target: on one path the first operand was
    // pushed by the other arm of the conditional, so the two leaves must
    // not be fused (that would always read the arm just before the join).
    [TestCase(1u, 0x1Fu, 0x2Eu, 0x11u, ExpectedResult = 0x11u + 0x30u)]
    [TestCase(0u, 0x1Fu, 0x2Eu, 0x11u, ExpectedResult = 0x00u + 0x3Fu)]
    public uint Conditional_Byte_Join_With_Leaf(uint pick, uint x, uint y, uint z)
    {
        uint r = (pick != 0 ? x : y) & z;
        return r + ((pick != 0 ? x : y) + z);
    }

    // 16-bit: each byte of the constant chooses its own form. Returns the
    // number of the first failing check (0: all passed) -- cheaper than an
    // Assert per check.
    [TestCase(0xA5C3UL, 0x10F0UL, 0x0111UL, ExpectedResult = 0u)]
    public uint Word_With_Constant(ulong w, ulong a, ulong b)
    {
        if ((w & 0x00FFUL) != 0x00C3UL) return 1;
        if ((w & 0xFF00UL) != 0xA500UL) return 2;
        if ((w & 0x3F81UL) != 0x2581UL) return 3;
        if ((w | 0x0F00UL) != 0xAFC3UL) return 4;
        if ((w ^ 0x0FF0UL) != 0xAA33UL) return 5;
        if (((a + b) & 0x0F0FUL) != 0x0201UL) return 6;
        return 0;
    }

    [TestCase(1u, 0x1234UL, 0x5678UL, ExpectedResult = 0x0204UL)]
    [TestCase(0u, 0x1234UL, 0x5678UL, ExpectedResult = 0x0608UL)]
    public ulong Conditional_Word_And_Constant(uint pick, ulong a, ulong b) => (pick != 0 ? a : b) & 0x0F0FUL;

    [TestCase(0xCA35UL, 0x5F0FUL, ExpectedResult = 0u)]
    public uint Word_With_Word(ulong a, ulong b)
    {
        ulong x = a;
        ulong y = b;
        if ((x & y) != 0x4A05UL) return 1;
        if ((x | y) != 0xDF3FUL) return 2;
        if ((x ^ y) != 0x953AUL) return 3;
        return 0;
    }
}
