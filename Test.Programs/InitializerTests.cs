using System;
using C64Lib;
using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.ProgramTest;

class Node
{
    public string[] Names { get; init; }
    public Node Child { get; init; }
    public uint A { get; init; }
    public uint B { get; init; }
    public ulong C { get; init; }
}

// Object initializers inside collection expressions: Roslyn spills them
// into a temporary local (Compiler/ILInitializerTempPass.cs turns that back
// into the dup form), and constant property values after a computed one
// are folded into the object's initial bytes too
// (ILObjectInitializerOptimizer.TrySkip).
[TestFixture]
public class InitializerTests
{
    static uint s_calls;
    static uint Count() => ++s_calls;

    static uint Collecting()
    {
        GC.Collect();
        return 5;
    }

    [Test]
    public void Spilled_Initializers_With_Computed_And_Constant_Values()
    {
        s_calls = 0;
        Node[] nodes =
        [
            new Node { Names = ["x", "yy"], A = 3, Child = new Node { A = Count(), B = 9 }, B = 4, C = 1000 },
            new Node { A = 1 },
            new Node { B = Count(), Names = ["z"], C = 70 },
        ];
        GC.Collect();
        Assert.AreEqual(nodes.Length, 3);
        Assert.IsTrue(nodes[0].Names[1] == "yy", "names");
        Assert.IsTrue(nodes[0].A == 3, "a");
        Assert.IsTrue(nodes[0].B == 4, "b");
        Assert.IsTrue(nodes[0].C == 1000, "c");
        Assert.IsTrue(nodes[0].Child.A == 1, "child a");
        Assert.IsTrue(nodes[0].Child.B == 9, "child b");
        Assert.IsTrue(nodes[1].A == 1 && nodes[1].B == 0 && nodes[1].Names == null, "second");
        Assert.IsTrue(nodes[2].B == 2 && nodes[2].C == 70 && nodes[2].Names[0] == "z", "third");
        Assert.IsTrue(C64.Debug.IsAlive(C64.Debug.GetObjectId(nodes[0].Child)), "child alive");
    }

    // A value that may collect keeps the temporary (a root) around.
    [Test]
    public void Initializer_That_Collects_Keeps_Its_Object()
    {
        Node[] nodes = [new Node { Names = ["k"], A = Collecting(), B = 8 }];
        GC.Collect();
        Assert.IsTrue(nodes[0].A == 5 && nodes[0].B == 8 && nodes[0].Names[0] == "k");
    }
}
