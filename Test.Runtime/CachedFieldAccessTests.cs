using System.Runtime.CompilerServices;
using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.RuntimeTest;

// Runtime correctness for ILMethodCachedFieldAccessPass -- see
// Compiler.UnitTests/CachedFieldAccessTests.cs for the structural ("did
// it actually fire, and did it correctly NOT fire") side of this, and
// that file's own comment for the real investigation behind it (how
// often resolveObjPtr gets called in Hunchback, and why the pass had to
// become a real CFG dataflow instead of a strictly-adjacent-only scan).
//
// Pair/FieldChain below mirror the field-read shapes those structural tests
// use; FloatPair specifically guards the real regression this pass
// shipped with once already: float arithmetic (Add/Sub/Mul/Div) routes
// through a real `jsr` into the BASIC ROM (asm/helper/float.asm), which
// an earlier version of this pass's dataflow wrongly treated as
// transparent, corrupting the SECOND of two cached reads separated by
// any float op. See ILMethodCachedFieldAccessPass's own comment for the
// full story.
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

class FieldChain
{
    public uint A;
    public uint B;
    public uint C;

    // Reads A, then B separated only by arithmetic, then C after a
    // branch -- the non-adjacent, "across real code" shape the
    // generalized dataflow exists to still cache correctly.
    public uint SumWithBranch(bool flag)
    {
        uint mid = A + 1;
        if (flag)
            mid = mid + B;
        else
            mid = mid + B + 1;
        return mid + C;
    }

    public uint SumInLoop(uint n)
    {
        uint sum = 0;
        for (uint i = 0; i < n; i++)
        {
            sum = sum + A + B;
        }
        return sum;
    }
}

class FloatPair
{
    public float A;
    public float B;
    float Last;

    // Same shape as the real regression: reads of A/B separated by
    // float arithmetic (Mul/Sub/Div), not just adjacent reads.
    public void Compute(float x)
    {
        float sum = A + B;
        float scaled = x * A - B;
        Last = scaled / sum;
    }

    public float GetLast() => Last;
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

    [Test]
    public void Reads_Separated_By_Arithmetic_And_A_Branch_Return_Correct_Values()
    {
        var c = new FieldChain { A = 10, B = 20, C = 5 };
        // flag=true: mid = (10+1) + 20 = 31; + C(5) = 36
        if (c.SumWithBranch(true) != 36)
            Assert.Fail();

        c = new FieldChain { A = 10, B = 20, C = 5 };
        // flag=false: mid = (10+1) + 20 + 1 = 32; + C(5) = 37
        if (c.SumWithBranch(false) != 37)
            Assert.Fail();
    }

    [Test]
    public void Reads_Inside_A_Loop_Return_Correct_Values()
    {
        var c = new FieldChain { A = 3, B = 4 };
        // 5 iterations of (3+4) = 35
        if (c.SumInLoop(5) != 35)
            Assert.Fail();
    }

    [Test]
    public void Float_Reads_Separated_By_Float_Arithmetic_Return_Correct_Values()
    {
        // Real regression guard: a_=1, b_=3, x=8 -> sum=4, scaled=8*1-3=5,
        // last=5/4=1.25 (exact in binary). A second call on the same
        // instance with different x re-reads A/B again.
        var m = new FloatPair { A = 1f, B = 3f };
        m.Compute(8f);
        if (m.GetLast() != 1.25f)
            Assert.Fail();

        m.Compute(2f);
        // x=2 -> scaled=2*1-3=-1 -> last=-1/4=-0.25 (exact).
        if (m.GetLast() != -0.25f)
            Assert.Fail();
    }
}
