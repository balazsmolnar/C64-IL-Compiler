using C64Lib;
using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.Test;

// Exercises C64.Random() (C64Lib/C64.cs) against asm/C64.asm's C64_Random,
// an 8-bit maximal-length Galois LFSR seeded from a fixed, real (non-
// zero-cost-.virtual) c64_rng_state byte. Each [Test] here gets a freshly
// loaded emulator (RunInEmulatorAspect.OnInvoke creates a new Emulator and
// LoadPrg's the .prg from scratch per test), so c64_rng_state's initial
// value is deterministically $A5 at the start of every test below -- these
// rely on that determinism, not on actual randomness.
[TestFixture]
public class RandomTests
{
    // The core correctness property a maximal-length LFSR with the right
    // tap value guarantees: starting from any nonzero seed, all 255
    // nonzero byte values are visited exactly once before the sequence
    // repeats, and 0 never appears mid-cycle. Verified empirically here
    // rather than just trusted from picking a textbook tap byte ($B8) --
    // see C64.asm's C64_Random comment.
    [Test]
    public void FullPeriod_NoRepeatsNoZero()
    {
        // uint[], not bool[] -- Ldelem_u1/Stelem_i1 (byte/bool array
        // element access) aren't supported by this compiler at all
        // (a separate, confirmed gap; out of scope here), so 0/nonzero
        // stands in for false/true.
        uint[] seen = new uint[256];
        uint count = 0;
        for (uint i = 0; i < 255; i++)
        {
            uint r = C64.Random();
            if (r == 0)
                Assert.Fail();
            if (seen[r] != 0)
                Assert.Fail();
            seen[r] = 1;
            count++;
        }
        Assert.AreEqual(count, 255u);
    }

    [Test]
    public void ConsecutiveCalls_Differ()
    {
        uint a = C64.Random();
        uint b = C64.Random();
        if (a == b)
            Assert.Fail();
    }

    // The intended real-world usage this feature exists for (per the
    // compiler-feature survey's LevelDescription.cs finding): combining
    // with the now-existing % operator for a bounded range.
    [Test]
    public void Modulo_StaysInBoundedRange()
    {
        for (uint i = 0; i < 50; i++)
        {
            uint r = C64.Random() % 5;
            if (r >= 5)
                Assert.Fail();
        }
    }

    // Same deterministic seed every test run (see class comment) -- checks
    // the first call's exact value by hand-computing one LFSR step from
    // $A5: ASL sets carry from bit 7 (1), so the EOR $1D branch is taken:
    // ($A5 << 1 = $4A) EOR $1D = $57. Confirms the actual asm executes the
    // documented algorithm, not just "returns something."
    [Test]
    public void Deterministic_FirstCallFromFreshSeed()
    {
        uint first = C64.Random();
        Assert.AreEqual(first, 0x57u);
    }
}
