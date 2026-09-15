using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.Test;

[TestFixture]
public class FloatTests
{
    [TestCase(2f, 3f, ExpectedResult = 5f)]
    [TestCase(-2f, 3f, ExpectedResult = 1f)]
    [TestCase(0.5f, 0.25f, ExpectedResult = 0.75f)]
    public float TestAdd(float a, float b)
    {
        return a + b;
    }

    [TestCase(5f, 3f, ExpectedResult = 2f)]
    [TestCase(3f, 5f, ExpectedResult = -2f)]
    [TestCase(0.75f, 0.25f, ExpectedResult = 0.5f)]
    public float TestSub(float a, float b)
    {
        return a - b;
    }

    [TestCase(2f, 3f, ExpectedResult = 6f)]
    [TestCase(-2f, 3f, ExpectedResult = -6f)]
    [TestCase(0.5f, 0.5f, ExpectedResult = 0.25f)]
    public float TestMul(float a, float b)
    {
        return a * b;
    }

    [TestCase(6f, 3f, ExpectedResult = 2f)]
    [TestCase(1f, 4f, ExpectedResult = 0.25f)]
    [TestCase(-6f, 3f, ExpectedResult = -2f)]
    public float TestDiv(float a, float b)
    {
        return a / b;
    }

    [TestCase(5f, ExpectedResult = -5f)]
    [TestCase(-5f, ExpectedResult = 5f)]
    [TestCase(0f, ExpectedResult = 0f)]
    public float TestNegate(float a)
    {
        return -a;
    }

    [TestCase(5f, 5f, ExpectedResult = true)]
    [TestCase(5f, 4f, ExpectedResult = false)]
    public bool TestEquality(float a, float b)
    {
        return a == b;
    }

    [TestCase(3f, 5f, ExpectedResult = true)]
    [TestCase(5f, 3f, ExpectedResult = false)]
    [TestCase(5f, 5f, ExpectedResult = false)]
    [TestCase(-5f, 3f, ExpectedResult = true)]
    public bool TestLess(float a, float b)
    {
        return a < b;
    }

    [TestCase(5f, 3f, ExpectedResult = true)]
    [TestCase(3f, 5f, ExpectedResult = false)]
    [TestCase(5f, 5f, ExpectedResult = false)]
    public bool TestGreater(float a, float b)
    {
        return a > b;
    }

    // Roslyn fuses a simple "if (a < b)" straight into a compare-and-branch
    // IL opcode instead of a separate Clt + Brtrue -- this is the shape
    // that exercises branch.asm's branch_lessflt etc, not compareLessflt.
    [TestCase(3f, 5f, ExpectedResult = true)]
    [TestCase(5f, 3f, ExpectedResult = false)]
    public bool TestLess_If(float a, float b)
    {
        if (a < b)
            return true;
        return false;
    }

    [TestCase(ExpectedResult = 3.5f)]
    public float TestConst()
    {
        return 3.5f;
    }

    [TestCase(5, ExpectedResult = 5f)]
    [TestCase(-5, ExpectedResult = -5f)]
    public float TestConvFromInt(int a)
    {
        return a;
    }

    [TestCase(5u, ExpectedResult = 5f)]
    [TestCase(200u, ExpectedResult = 200f)]
    public float TestConvFromUint(uint a)
    {
        return a;
    }

    [TestCase(5f, ExpectedResult = 5)]
    [TestCase(-5f, ExpectedResult = -5)]
    public int TestConvToInt(float a)
    {
        return (int)a;
    }

    [TestCase(2f, 3f, ExpectedResult = 11f)]
    public float TestLocal(float a, float b)
    {
        var sum = a + b;
        sum = sum * 2f;
        return sum + 1f;
    }

    [TestCase(ExpectedResult = 4.5f)]
    public float TestField()
    {
        var obj = new FloatHolder();
        obj.Value = 3.5f;
        obj.Value = obj.Value + 1f;
        return obj.Value;
    }

    [TestCase(ExpectedResult = 4f)]
    public float TestArray()
    {
        var arr = new float[3];
        arr[0] = 1.5f;
        arr[1] = 2.5f;
        arr[2] = arr[0] + arr[1];
        return arr[2];
    }

    [TestCase(ExpectedResult = 3)]
    public int TestArrayLength()
    {
        var arr = new float[3];
        return arr.Length;
    }

    [TestCase(ExpectedResult = -1.25f)]
    public float TestArray_NegativeAndFractionalElements()
    {
        var arr = new float[4];
        arr[0] = -0.5f;
        arr[1] = 2.25f;
        arr[2] = -3f;
        arr[3] = arr[0] + arr[1] + arr[2];
        return arr[3];
    }

    // Computed (not literal) index on both the write and read side, plus a
    // loop -- TestArray above only ever indexes with literal 0/1/2.
    [TestCase(ExpectedResult = 10f)]
    public float TestArray_IndexedWriteAndReadInLoop()
    {
        var arr = new float[5];
        for (int i = 0; i < 5; i++)
            arr[i] = i;
        float sum = 0f;
        for (int i = 0; i < 5; i++)
            sum = sum + arr[i];
        return sum;
    }

    private static float SumArray(float[] arr)
    {
        float sum = 0f;
        for (int i = 0; i < arr.Length; i++)
            sum = sum + arr[i];
        return sum;
    }

    [TestCase(ExpectedResult = 6f)]
    public float TestArray_PassedAsParameter()
    {
        var arr = new float[3];
        arr[0] = 1f;
        arr[1] = 2f;
        arr[2] = 3f;
        return SumArray(arr);
    }

    // Non-constant initializer elements (variables, not literals) -- same
    // shape as ArrayTest.cs's Int_Array_Initializer, deliberately avoiding
    // Roslyn's blob-based InitializeArray optimization (which only fires
    // for all-constant elements) since that path was never built for
    // float's 5-byte MFLPT representation.
    [TestCase(ExpectedResult = 7.5f)]
    public float TestArray_Initializer()
    {
        float a = 2.5f;
        float b = 5f;
        var arr = new float[] { a, b };
        return arr[0] + arr[1];
    }

    private class FloatHolder
    {
        public float Value;
    }
}
