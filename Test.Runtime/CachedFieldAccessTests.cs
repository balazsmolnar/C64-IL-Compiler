using System.Runtime.CompilerServices;
using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.RuntimeTest;

// Runtime correctness for ILMethodCachedFieldAccessPass -- see
// Compiler.UnitTests/CachedFieldAccessTests.cs for the structural ("did
// it actually fire") side of this. These only check observable values
// stay correct when two fields of the same object get read back to back,
// both via a trivially-substituted getter pair (forced inline, same
// shape as Hunchback's own Player.X/Player.Y) and via two field reads
// inside one real, never-inlined method.
class Pair
{
    public uint A;
    public uint B;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint GetA() => A;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint GetB() => B;

    // Two call sites (see Sum2 below) so this is never itself a trivial-
    // substitution candidate on its own account -- isolates
    // ILPropertyGettterOptimizer's #pushfld fusion path specifically.
    public uint Sum() => A + B;
}

[TestFixture]
public class CachedFieldAccessTests
{
    [Test]
    public void Two_Forced_Inlined_Getters_On_Same_Object_Return_Correct_Values()
    {
        var p = new Pair { A = 10, B = 20 };
        uint sum = p.GetA() + p.GetB();
        if (sum != 30)
            Assert.Fail();
    }

    [Test]
    public void Two_This_Field_Reads_In_One_Method_Return_Correct_Values()
    {
        var p1 = new Pair { A = 3, B = 4 };
        var p2 = new Pair { A = 5, B = 6 };
        uint r = p1.Sum() + p2.Sum();
        if (r != 18)
            Assert.Fail();
    }

    [Test]
    public void Different_Objects_Read_Back_To_Back_Stay_Independent()
    {
        var p1 = new Pair { A = 100, B = 1 };
        var p2 = new Pair { A = 7, B = 1 };

        // p1.GetA() then p2.GetA() -- same shape, DIFFERENT object --
        // must never share a resolve.
        uint sum = p1.GetA() + p2.GetA();
        if (sum != 107)
            Assert.Fail();
    }
}
