using System.Runtime.CompilerServices;
using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.RuntimeTest;

// Runtime correctness for constant-argument specialization -- see
// Compiler.UnitTests/ConstantArgumentSpecializationTests.cs for the
// structural ("did it actually skip materialization") side of this and
// that file's own comment for the verified (not assumed) finding that it
// does NOT also unlock further const-folding inside the callee's own
// body. These only check the observable VALUE stays correct.
class ConstHolder
{
    public uint value_;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint AddN(uint n) => value_ + n;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint MulBy(uint v, uint n) => v * n;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float AddF(float v, float n) => v + n;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong AddWide(ulong v, ulong n) => v + n;
}

[TestFixture]
public class ConstantArgumentSpecializationTests
{
    [Test]
    public void Constant_Argument_Computes_Correct_Value()
    {
        var h = new ConstHolder { value_ = 5 };
        if (h.AddN(10) != 15)
            Assert.Fail();
    }

    [Test]
    public void Constant_Multiply_Computes_Correct_Value()
    {
        if (ConstHolder.MulBy(3, 4) != 12)
            Assert.Fail();
    }

    [Test]
    public void Float_Constant_Argument_Computes_Correct_Value()
    {
        if (ConstHolder.AddF(1.0f, 2.5f) != 3.5f)
            Assert.Fail();
    }

    [Test]
    public void Wide_Typed_Parameter_With_Literal_Argument_Computes_Correct_Value()
    {
        // Forces the materializing (not specialized) path -- see the
        // structural test of the same name -- still must be correct.
        if (ConstHolder.AddWide(1, 10) != 11)
            Assert.Fail();
    }

    [Test]
    public void Mix_Of_Variable_And_Constant_Arguments_Computes_Correct_Value()
    {
        uint x = 7;
        if (ConstHolder.MulBy(x, 4) != 28)
            Assert.Fail();
    }
}
