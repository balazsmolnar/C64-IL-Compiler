using NUnit.Framework;

namespace Compiler.LanguageTest;

// An IL switch whose index is out of range must fall through to the default.
// Roslyn compiles a switch over an enum whose first case is 1 to
// `switch (value - 1)`, so value 0 arrives as index 255; #switch used to
// jump through whatever followed the jump table (a BRK, ?FLOAT ERROR).
enum SwitchFace { Plain, Bold, Big, Mono }

[TestFixture]
public class SwitchDefaultTests
{
    static uint Width(SwitchFace face) => face switch
    {
        SwitchFace.Bold => 2,
        SwitchFace.Big => 4,
        SwitchFace.Mono => 3,
        _ => 1,
    };

    [TestCase(0u, ExpectedResult = 1u)]
    [TestCase(1u, ExpectedResult = 2u)]
    [TestCase(2u, ExpectedResult = 4u)]
    [TestCase(3u, ExpectedResult = 3u)]
    public uint SwitchFromOneDefaultsBelow(uint face) => Width((SwitchFace)face);

    [TestCase(0u, ExpectedResult = 9u)]
    [TestCase(1u, ExpectedResult = 10u)]
    [TestCase(3u, ExpectedResult = 30u)]
    [TestCase(4u, ExpectedResult = 9u)]
    [TestCase(200u, ExpectedResult = 9u)]
    public uint SwitchDefaultsAbove(uint value)
    {
        switch (value)
        {
            case 1: return 10;
            case 2: return 20;
            case 3: return 30;
            default: return 9;
        }
    }
}
