using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.RuntimeTest;

class TosThing
{
    public uint Value;
}

// Top of stack kept in A across a macro boundary (Compiler/TopOfStackInA.cs):
// the pha/pla pair between a producing and a consuming line is dropped.
// These are the cases where that can go wrong: a branch reading the N/Z
// flags the pla would have set, consumers that overwrite A before their
// pull (reference stores), and chains where one line both takes the value
// in A and leaves its own result there. Each case runs a zero and a
// non-zero value, since the flags are what differs.
[TestFixture]
public class TopOfStackInATests
{
    static bool flag_;
    static uint byteField_;
    static TosThing thing_;

    // and8_const leaves N/Z from its `and`: the branch may use them.
    [TestCase(0x14u, ExpectedResult = 1u)]
    [TestCase(0x10u, ExpectedResult = 2u)]
    public uint Masked_Value_Branches_On_Flags(uint x)
    {
        if ((x & 0x04) != 0)
            return 1;
        return 2;
    }

    // mul8 returns from jsr Multiply8 with unrelated flags: the pair has to
    // stay (or the flags be set again) before `pla; bne`.
    [TestCase(3u, 0u, ExpectedResult = 2u)]
    [TestCase(3u, 5u, ExpectedResult = 1u)]
    [TestCase(0u, 7u, ExpectedResult = 2u)]
    public uint Product_Branches_On_Flags(uint a, uint b)
    {
        if (a * b != 0)
            return 1;
        return 2;
    }

    // An identity mask emits no instruction, so its flags are whatever the
    // multiply left.
    [TestCase(3u, 0u, ExpectedResult = 2u)]
    [TestCase(3u, 5u, ExpectedResult = 1u)]
    public uint Identity_Mask_Of_Product_Branches_On_Flags(uint a, uint b)
    {
        if (((a * b) & 0xFF) != 0)
            return 1;
        return 2;
    }

    // A static bool read (lda) straight into branch_false8/branch_true8.
    [TestCase(0u, ExpectedResult = 2u)]
    [TestCase(1u, ExpectedResult = 1u)]
    public uint Static_Flag_Branches(uint x)
    {
        flag_ = x != 0;
        if (!flag_)
            return 2;
        return 1;
    }

    // A chain where each line consumes the previous result in A and leaves
    // its own there, ending in a store and in a compare.
    [TestCase(0x1Au, 0x03u, ExpectedResult = 0x05u)]
    [TestCase(0x0Bu, 0x05u, ExpectedResult = 0xFFu)]
    public uint Chain_Through_A(uint x, uint y)
    {
        byteField_ = (((x + y) & 0x0F) >> 1) - 1;
        return byteField_;
    }

    // Reference stores drop the old reference before pulling the new one
    // (it overwrites A): a new object stored to a static and a local field.
    [TestCase(9u, ExpectedResult = 9u + 10u)]
    public uint Reference_Stores_Keep_The_Value(uint v)
    {
        thing_ = new TosThing { Value = v };
        TosThing local = new TosThing { Value = v + 1 };
        thing_ = new TosThing { Value = thing_.Value };
        return thing_.Value + local.Value;
    }

    // A static array element (OpStaticArrayLoad's raw `lda (tmpPointer),y /
    // pha`) straight into a truthiness branch.
    static uint[] table_ = new uint[] { 0, 7, 0, 200 };

    [TestCase(0u, ExpectedResult = 2u)]
    [TestCase(1u, ExpectedResult = 1u)]
    [TestCase(3u, ExpectedResult = 1u)]
    public uint Static_Array_Element_Branches(uint i)
    {
        if (table_[i] != 0)
            return 1;
        return 2;
    }

    // A conditional join on the consumer: the pair must stay.
    [TestCase(1u, 0u, 5u, ExpectedResult = 2u)]
    [TestCase(0u, 0u, 5u, ExpectedResult = 1u)]
    public uint Join_Before_Branch(uint pick, uint x, uint y)
    {
        if ((pick != 0 ? x : y) != 0)
            return 1;
        return 2;
    }
}
