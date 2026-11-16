using System;
using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.RuntimeTest;

// Every class with a lambda gets its own compiler-generated nested class
// "<>c" (and one "<>c__DisplayClassN_M" per closure scope). Their asm labels
// must include the enclosing class, or two such classes in one program
// collide in 64tass ("duplicate definition").
class LambdaHolderA
{
    public static Func<uint> Make() => () => 11;

    public static Func<uint> Capture(uint x) => () => x + 1;

    public static uint Local(uint x)
    {
        return Twice(x) + 1;
        static uint Twice(uint v) => (uint)(v * 2);
    }
}

class LambdaHolderB
{
    public static Func<uint> Make() => () => 22;

    public static Func<uint> Capture(uint x) => () => x + 2;

    public static uint Local(uint x)
    {
        return Twice(x) + 2;
        static uint Twice(uint v) => (uint)(v * 2);
    }
}

[TestFixture]
public class LambdaLabelTests
{
    [Test]
    public void LambdasInTwoClasses()
    {
        Func<uint> a = LambdaHolderA.Make();
        Func<uint> b = LambdaHolderB.Make();
        Assert.AreEqual(a() + b(), 33u);
    }

    [Test]
    public void ClosuresInTwoClasses()
    {
        Func<uint> a = LambdaHolderA.Capture(5);
        Func<uint> b = LambdaHolderB.Capture(7);
        Assert.AreEqual(a() * 10 + b(), 69u);
    }

    [Test]
    public void LocalFunctionsInTwoClasses()
    {
        Assert.AreEqual(LambdaHolderA.Local(3) * 10 + LambdaHolderB.Local(4), 80u);
    }
}
