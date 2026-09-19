using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.Test;

// Exercises Compiler/CommandMap.cs's Div/Div_un/Rem/Rem_un mappings
// against asm/helper/division.asm's shift-subtract routines. Signed cases
// specifically check truncate-toward-zero division and a remainder that
// takes the dividend's sign (or is zero), matching C#/.NET's own int/long
// semantics -- not floor division, and not always-positive remainder.
[TestFixture]
public class DivisionTests
{
    [TestCase(7u, 2u, ExpectedResult = 3u)]
    [TestCase(100u, 10u, ExpectedResult = 10u)]
    [TestCase(5u, 5u, ExpectedResult = 1u)]
    [TestCase(1u, 7u, ExpectedResult = 0u)]
    [TestCase(255u, 1u, ExpectedResult = 255u)]
    [TestCase(255u, 255u, ExpectedResult = 1u)]
    [TestCase(0u, 5u, ExpectedResult = 0u)]
    public uint TestDivideUInt(uint a, uint b)
    {
        return a / b;
    }

    [TestCase(7u, 2u, ExpectedResult = 1u)]
    [TestCase(100u, 10u, ExpectedResult = 0u)]
    [TestCase(5u, 3u, ExpectedResult = 2u)]
    [TestCase(255u, 255u, ExpectedResult = 0u)]
    [TestCase(1u, 7u, ExpectedResult = 1u)]
    public uint TestModuloUInt(uint a, uint b)
    {
        return a % b;
    }

    // -128 (this compiler's int minimum) is deliberately not exercised here
    // as a TestCase argument -- RunInEmulatorAspect.CopyMethodArgumentsToEmulator
    // only accepts -127..127 for an int argument, a pre-existing test-harness
    // limitation unrelated to division itself (see asm/helper/division.asm's
    // own header comment on -128 self-negation for the division-side behavior).
    [TestCase(7, 2, ExpectedResult = 3)]
    [TestCase(-7, 2, ExpectedResult = -3)]
    [TestCase(7, -2, ExpectedResult = -3)]
    [TestCase(-7, -2, ExpectedResult = 3)]
    [TestCase(0, 5, ExpectedResult = 0)]
    [TestCase(-1, 1, ExpectedResult = -1)]
    public int TestDivideInt(int a, int b)
    {
        return a / b;
    }

    // Remainder takes the dividend's sign, or is zero -- e.g. -7 % 2 = -1,
    // not the mathematical-mod +1 a floor-division language would give.
    [TestCase(7, 2, ExpectedResult = 1)]
    [TestCase(-7, 2, ExpectedResult = -1)]
    [TestCase(7, -2, ExpectedResult = 1)]
    [TestCase(-7, -2, ExpectedResult = -1)]
    [TestCase(0, 5, ExpectedResult = 0)]
    public int TestModuloInt(int a, int b)
    {
        return a % b;
    }

    [TestCase(1000ul, 7ul, ExpectedResult = 142ul)]
    [TestCase(65535ul, 3ul, ExpectedResult = 21845ul)]
    [TestCase(65535ul, 65535ul, ExpectedResult = 1ul)]
    [TestCase(0ul, 17ul, ExpectedResult = 0ul)]
    [TestCase(5ul, 65535ul, ExpectedResult = 0ul)]
    public ulong TestDivideULong(ulong a, ulong b)
    {
        return a / b;
    }

    [TestCase(1000ul, 7ul, ExpectedResult = 6ul)]
    [TestCase(65535ul, 3ul, ExpectedResult = 0ul)]
    [TestCase(100ul, 30ul, ExpectedResult = 10ul)]
    public ulong TestModuloULong(ulong a, ulong b)
    {
        return a % b;
    }

    [TestCase(1000L, 7L, ExpectedResult = 142L)]
    [TestCase(-1000L, 7L, ExpectedResult = -142L)]
    [TestCase(1000L, -7L, ExpectedResult = -142L)]
    [TestCase(-1000L, -7L, ExpectedResult = 142L)]
    [TestCase(-32768L, 1L, ExpectedResult = -32768L)]
    public long TestDivideLong(long a, long b)
    {
        return a / b;
    }

    [TestCase(1000L, 7L, ExpectedResult = 6L)]
    [TestCase(-1000L, 7L, ExpectedResult = -6L)]
    [TestCase(1000L, -7L, ExpectedResult = 6L)]
    [TestCase(-1000L, -7L, ExpectedResult = -6L)]
    public long TestModuloLong(long a, long b)
    {
        return a % b;
    }

    // Same expressions the codebase's own hand-rolled digit-splitting code
    // (e.g. Hunchback/LevelPlay.cs's DrawDebugLevelNumber, written before
    // division existed at all) could now use directly.
    [TestCase(47u, ExpectedResult = 4u)]
    [TestCase(5u, ExpectedResult = 0u)]
    [TestCase(99u, ExpectedResult = 9u)]
    public uint TestDivide_TensDigit(uint value)
    {
        return value / 10;
    }

    [TestCase(47u, ExpectedResult = 7u)]
    [TestCase(5u, ExpectedResult = 5u)]
    [TestCase(99u, ExpectedResult = 9u)]
    public uint TestModulo_OnesDigit(uint value)
    {
        return value % 10;
    }
}
