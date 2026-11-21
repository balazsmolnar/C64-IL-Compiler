using System;
using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.LanguageTest;

// An array of static method groups as Func<T> (a slide list, say) compiles
// to one #newObjArrayInit table instead of a delegate construction per
// element (ILObjectArrayInitializerOptimizer.TryParseStaticFunc). Each
// element must still be a working delegate to its own method, in order.
// Only a static initializer has that plain shape: in a method body Roslyn
// caches each method group in a <>O field behind a branch, which the pass
// must leave alone (the local and mixed arrays below check that path).
static class MethodGroups
{
    public static uint One() => 1;
    public static uint Two() => 2;
    public static uint Three() => 3;
    public static uint Four() => 4;

    // A static initializer, as in C64Presentation/Program.cs.
    public static readonly Func<uint>[] Table = [One, Two, Three, Four];

    // A lambda in the middle ends the run; the method groups around it are
    // tables of their own (or, alone, ordinary code).
    public static Func<uint>[] Mixed() => [One, Two, () => 7, Three, Four, One];
}

[TestFixture]
public class MethodGroupArrayTests
{
    [Test]
    public void StaticTableCallsEachMethodInOrder()
    {
        // uint is 8 bits: weights 1, 2, 4, 8 tell the order apart below 256.
        var table = MethodGroups.Table;
        uint sum = 0, weight = 1;
        for (int i = 0; i < table.Length; i++)
        {
            sum += table[i]() * weight;
            weight *= 2;
        }
        Assert.AreEqual(sum, 49u);      // 1 + 2*2 + 3*4 + 4*8
    }

    [Test]
    public void LocalArrayCallsEachMethodInOrder()
    {
        Func<uint>[] local = [MethodGroups.Four, MethodGroups.Three, MethodGroups.Two];
        uint sum = 0, weight = 1;
        foreach (var f in local)
        {
            sum += f() * weight;
            weight *= 2;
        }
        Assert.AreEqual(sum, 18u);      // 4 + 3*2 + 2*4
    }

    [Test]
    public void MixedArrayKeepsTheLambda()
    {
        var mixed = MethodGroups.Mixed();
        uint sum = 0;
        foreach (var f in mixed)
            sum += f();
        Assert.AreEqual(sum, 18u);
    }

    [Test]
    public void TableSurvivesACollection()
    {
        Func<uint>[] local = [MethodGroups.Two, MethodGroups.Four];
        GC.Collect();
        Assert.AreEqual(local[0]() * 10 + local[1]() + MethodGroups.Table[2](), 27u);
    }
}
