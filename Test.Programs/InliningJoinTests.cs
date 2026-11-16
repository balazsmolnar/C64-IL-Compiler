using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.ProgramTest;

// A static auto-property's setter is inlined by trivial-argument
// substitution. When its argument is the join of a ternary, the first arm's
// `br` targets the call itself and the second arm's push sits right before
// it: they are not one basic block, so the substitution must not replace
// them (it used to drop the branch target: "Missing label",
// ILMethodInliningPass.IsBranchTarget).
static class Pick
{
    public static uint Chosen { get; private set; }

    public static void Choose(bool first) => Chosen = first ? 7u : 9u;
}

[TestFixture]
public class InliningJoinTests
{
    [Test]
    public void Ternary_Into_Static_Setter_Takes_Each_Arm()
    {
        Pick.Choose(true);
        Assert.AreEqual(Pick.Chosen, 7u);
        Pick.Choose(false);
        Assert.AreEqual(Pick.Chosen, 9u);
    }
}
