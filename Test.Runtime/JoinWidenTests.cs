using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.RuntimeTest;

// `big ? 136u : 149u` passed where a ulong is expected compiles to one
// Conv_u8 after the two branches join. The constant right before that Conv
// used to look ahead, see the Conv and push itself 16 bits wide -- while the
// Conv still widened, since its other predecessor pushed 8 bits -- so that
// path left a stray byte on the hardware stack on every call. Hundreds of
// calls, and a frameless caller (static void, no parameters: its return
// address lives on the hardware stack), must both come out intact.
[TestFixture]
public class JoinWidenTests
{
    static ulong total;

    static ulong Pass(ulong v, uint dummy)
    {
        return v + dummy;
    }

    static void Add(bool big)
    {
        total = total + Pass(big ? 136u : 149u, 0);
    }

    // Frameless: returns through the hardware stack.
    static void AddMany()
    {
        for (uint i = 0; i < 100; i++)
            Add(true);
        for (uint i = 0; i < 50; i++)
            Add(false);
    }

    [Test]
    public void Conditional_Constant_Widened_After_A_Join_Leaves_The_Stack_Balanced()
    {
        total = 0;
        AddMany();
        AddMany();
        Assert.IsTrue(total == 2 * (100 * 136 + 50 * 149));
    }
}
