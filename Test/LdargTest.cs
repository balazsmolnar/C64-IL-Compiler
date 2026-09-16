using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.Test;

// Ldarg_0..Ldarg_3 bake the argument index into the opcode itself (no IL
// operand); a method's 5th+ parameter needs Ldarg_s instead, which carries
// the index as a real 1-byte operand -- unexercised until a Demo/Program.cs
// sprite+arithmetic demo needed a 5-parameter helper method, at which point
// it turned out Ldarg_s wasn't mapped in CommandMap.cs at all. These pin
// down that fix (OpLdarg_s in Compiler/Operands/OperandBase.cs) actually
// reads back the *right* parameter, not just that it compiles.
[TestFixture]
class LdargTest
{
    private static int FifthParam(int a, int b, int c, int d, int e) => e;
    private static int FourthParam(int a, int b, int c, int d, int e) => d;

    [TestCase(1, 2, 3, 4, 5, ExpectedResult = 5)]
    [TestCase(10, 20, 30, 40, 50, ExpectedResult = 50)]
    public int Ldarg_s_ReadsFifthParameter(int a, int b, int c, int d, int e)
    {
        return FifthParam(a, b, c, d, e);
    }

    [TestCase(1, 2, 3, 4, 5, ExpectedResult = 4)]
    public int Ldarg_s_ReadsFourthParameter(int a, int b, int c, int d, int e)
    {
        return FourthParam(a, b, c, d, e);
    }

    // Matches Demo/Program.cs's ShowResult(uint, uint, string, float, float)
    // shape exactly: two float parameters past the Ldarg_s boundary, each
    // needing the "flt" SizeSuffix path, not just the plain 8/16-bit one.
    private static bool FloatParamsMatch(uint x, uint y, string label, float actual, float expected)
    {
        return actual == expected;
    }

    [TestCase(ExpectedResult = true)]
    public bool Ldarg_s_ReadsFloatParametersCorrectly()
    {
        return FloatParamsMatch(1, 2, "label", 3.5f, 3.5f);
    }

    [TestCase(ExpectedResult = false)]
    public bool Ldarg_s_DistinguishesFloatParameters()
    {
        return FloatParamsMatch(1, 2, "label", 3.5f, 6.5f);
    }
}
