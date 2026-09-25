using C64TestFramework;
using NUnit.Framework;

namespace Compiler.RuntimeTest;

// Conditions the compiled program can't continue from end in a runtime fault
// (asm/helper/fault.asm) instead of silently corrupting memory or crashing
// to $0000. In the unit test harness a fault ends the run with a code that
// [ExpectFault] checks (see RunInEmulatorAspect).
[TestFixture]
public class RuntimeFaultTests
{
    static uint[] sink_;
    static float zero_, big_, negative_, result_;

    // This program's heap is under 56KB (it ends at $e000, KERNAL ROM), so
    // 250 arrays of 250 bytes run past it -- while still using fewer than the
    // object table's 255 slots, so it's the heap that gives out first.
    [Test]
    [ExpectFault(RuntimeFault.OutOfMemory)]
    public void Allocating_Past_The_End_Of_The_Heap_Faults()
    {
        for (ulong i = 0; i < 250; i++)
            sink_ = new uint[250];
    }

    // The object table has 255 usable slots and nothing frees one without a
    // GC.Collect().
    [Test]
    [ExpectFault(RuntimeFault.TooManyObjects)]
    public void Allocating_More_Than_255_Objects_Faults()
    {
        for (ulong i = 0; i < 300; i++)
            sink_ = new uint[1];
    }

    // Read through a static so the compiler can't fold it into a constant.
    [Test]
    [ExpectFault(RuntimeFault.DivisionByZero)]
    public void Float_Division_By_Zero_Faults()
    {
        result_ = 1f / zero_;
    }

    [Test]
    [ExpectFault(RuntimeFault.Overflow)]
    public void Float_Overflow_Faults()
    {
        big_ = 1e30f;
        result_ = big_ * big_;
    }

    [Test]
    [ExpectFault(RuntimeFault.IllegalQuantity)]
    public void Float_Square_Root_Of_A_Negative_Number_Faults()
    {
        negative_ = -4f;
        result_ = System.MathF.Sqrt(negative_);
    }

    // The checks must not fault a program that stays inside its limits.
    [TestCase(ExpectedResult = true)]
    public bool Allocating_Within_The_Limits_Does_Not_Fault()
    {
        for (uint i = 0; i < 5; i++)
            sink_ = new uint[100];
        return true;
    }
}
