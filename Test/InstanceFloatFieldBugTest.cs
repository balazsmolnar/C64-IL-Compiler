using NUnit.Framework;

namespace Compiler.Test;

// Regression test for a real, now-FIXED compiler bug: ILPropertyGettterOptimizer
// (Compiler/ILPropertyGetterOptimizer.cs) fuses "Ldarg_0; Ldfld" into a single
// OpPushFld operation, which only ever emits #pushfld8 or #pushfld16 (picked
// by a single is16Bit bool -- asm/helper/optimized.asm has no #pushfldflt).
// A float field (GetStorageBytes()==5, TypeExtensions.cs) is neither 8 nor
// 16 bit, so it silently fell through to #pushfld8, which pushes exactly 1
// byte onto the hardware evaluation stack instead of the 5 a float needs.
// Each misfire desynced that stack by 4 bytes with no immediate symptom --
// confirmed via a standalone SimpleEmulator-based repro (outside this
// project's tight unittest.asm memory budget) that the hardware stack
// pointer drifted by exactly 4 bytes per affected field read (32 bytes per
// method call reading 8 such fields), until enough of it accumulated that
// a later float operation pulled stale/foreign bytes as its operand,
// producing a bogus value a C64 ROM float routine then legitimately
// flagged as overflow -- which crashed only because this project's
// SimpleEmulator-based test harness (like the real one used here) never
// runs the KERNAL cold-start sequence that would normally initialize the
// BASIC ROM's IERROR vector ($0300/$0301) the overflow handler jumps
// through.
//
// Fixed by guarding both ILPropertyGettterOptimizer rules to skip float
// fields (falling back to the always-correct unfused Ldarg_0+Ldfld path,
// which already handles float correctly via OpLdfld's own SizeSuffix).
//
// IMPORTANT -- uses public fields set via C#'s object-initializer syntax
// (`new T { a_ = ... }`), NOT a constructor body. A real, SEPARATE,
// pre-existing compiler gap was found while writing this test:
// Compiler/Operands/OperandBase.cs's OpNewObj has the line that would call
// a type's actual constructor (`ctor = $"{t.Name}_x_ctor"`) commented out
// and hardcoded to "0" (no call at all) for every ordinary class -- only
// the special-cased Func<T> delegate type gets its ctor invoked. A
// constructor body (parameterless or not) that sets fields never actually
// runs; the object's fields silently stay at their zero-initialized heap
// default. Confirmed directly via SimpleEmulator (reading the object's
// float fields right after construction: all-zero, for both a
// (float a, float b) constructor and a parameterless one) -- and this
// wasn't introduced by today's fix: nothing in this 560+ test suite
// exercises a real constructor body (every existing type either has no
// constructor and uses object-initializer syntax like this test now does,
// or a default no-op constructor, per a survey of Test/*.cs's object
// types). Left unfixed here -- a real, previously-undiscovered gap, but a
// separate, larger piece of work, not this session's task.
class InstanceFloatFieldMath
{
    public float a_, b_;
    float last_;

    // Mirrors the fused Ldarg_0+Ldfld shape the bug lived in: multiple
    // reads of this instance's own float fields inside one method.
    public void Compute(float x)
    {
        float sum = a_ + b_;
        float scaled = x * a_ - b_;
        last_ = scaled / sum;
    }

    public float Last => last_;
}

[TestFixture]
public class InstanceFloatFieldBugTest
{
    // sum=1+3=4. x=8 -> scaled=8*1-3=5 -> last=5/4=1.25 (exact in binary).
    [TestCase(ExpectedResult = true)]
    public bool InstanceFloatFieldMethod_ComputesCorrectValue_FirstCall()
    {
        var m = new InstanceFloatFieldMath { a_ = 1f, b_ = 3f };
        m.Compute(8f);
        return m.Last == 1.25f;
    }

    // Same instance, second call with different x, same fields re-read.
    // x=2 -> scaled=2*1-3=-1 -> last=-1/4=-0.25 (exact).
    [TestCase(ExpectedResult = true)]
    public bool InstanceFloatFieldMethod_ComputesCorrectValue_SecondCallSameInstance()
    {
        var m = new InstanceFloatFieldMath { a_ = 1f, b_ = 3f };
        m.Compute(8f);
        m.Compute(2f);
        return m.Last == -0.25f;
    }
}
