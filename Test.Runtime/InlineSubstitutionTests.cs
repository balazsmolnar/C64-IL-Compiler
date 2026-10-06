using System.Runtime.CompilerServices;
using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.RuntimeTest;

// Runtime correctness for the trivial-argument substitution optimization
// (ILMethodInliningPass) -- see Compiler.UnitTests/
// InlineParameterSubstitutionTests.cs for the structural ("did the
// optimization actually fire") side of this, and that file's own comment
// for the research behind it (Scheifler 1977, LLVM's ValueMap-based
// parameter substitution, Go's source-level inliner). These only check
// that observable behavior stays correct, forced via AggressiveInlining
// the same way Hunchback's own Player.X getter would be if force-inlined
// (see the real investigation that prompted this: Player.X's getter,
// get => x_;, is exactly this shape -- a trivial `this`-sourced argument
// at every one of its call sites).
class TrivialGetterHolder
{
    public uint value_;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint GetValue() => value_;
}

class StaticIdentity
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Identity(uint v) => v;
}

class SelfMutatingParam
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Bump(uint v)
    {
        v = v + 100;
        return v;
    }
}

[TestFixture]
public class InlineSubstitutionTests
{
    [Test]
    public void This_Sourced_Trivial_Argument_Correct_At_Multiple_Sites()
    {
        var a = new TrivialGetterHolder { value_ = 10 };
        var b = new TrivialGetterHolder { value_ = 20 };

        // Same shape as Hunchback's Player.X getter being read from several
        // places in Player.Move() -- each read goes through its own
        // Callvirt-on-`this` call site, every one eligible for trivial
        // substitution independently.
        uint sum = a.GetValue() + b.GetValue() + a.GetValue();
        if (sum != 40)
            Assert.Fail();
    }

    [Test]
    public void Local_Variable_Sourced_Trivial_Argument_Correct()
    {
        uint x = 7;
        uint local = x + 5;
        if (StaticIdentity.Identity(local) != 12)
            Assert.Fail();
    }

    [Test]
    public void Written_Parameter_Does_Not_Alias_Callers_Variable()
    {
        uint x = 5;
        uint result = SelfMutatingParam.Bump(x);

        // If the "callee writes to its own parameter" guard were missing,
        // substitution would alias x directly onto the callee's `v`, and
        // the write inside Bump would corrupt x itself instead of just the
        // callee's own independent copy.
        if (x != 5)
            Assert.Fail();
        if (result != 105)
            Assert.Fail();
    }
}
