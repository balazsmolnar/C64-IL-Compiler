using System;
using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.ProgramTest;

// Compiler/ILReachabilityPass.cs drops what can never run. These classes
// mix live and dead code: Shape.Area is dispatched virtually, Shape.Name is
// a virtual slot nobody calls (a 0 in every vtable), Square is never
// created (its Area override is dead), and Unused is never touched (no
// file at all). Compiler.UnitTests/DeadCodeTests.cs checks what is
// removed; this checks that what is left still runs.
// (No field in the base class: a base-class field and a derived-class field
// in one object get two different layouts -- an older, separate problem.)
abstract class Shape
{
    public abstract uint Area();
    public virtual uint Name() => 1;
    public uint Twice() => Area() + Area();
}

class Rect : Shape
{
    public uint Size;
    public uint Height;
    public override uint Area() => Size * Height;
    public override uint Name() => 2;
}

class Triangle : Shape
{
    public uint Size;
    public override uint Area() => Size * Size / 2;
}

class Square : Shape
{
    public uint Size;
    public override uint Area() => Size * Size;
}

static class Unused
{
    public static uint Never(uint x) => x * 3 + 1;
    public static string Text() => "never emitted";
}

static class Callbacks
{
    public static uint Seven() => 7;
}

[TestFixture]
public class DeadCodeTests
{
    static uint Sum(Shape[] shapes)
    {
        uint total = 0;
        foreach (var shape in shapes)
            total += shape.Twice();
        return total;
    }

    [Test]
    public void Virtual_Dispatch_With_Dead_Slots_And_Types()
    {
        Shape[] shapes = [new Rect { Size = 3, Height = 4 }, new Triangle { Size = 4 }];
        Assert.IsTrue(shapes[0].Area() == 12, "area");
        Assert.IsTrue(shapes[1].Area() == 8, "tri");
        Assert.IsTrue(shapes[0].Twice() == 24, "twice");
        Assert.IsTrue(Sum(shapes) == 40, "sum");
    }

    // A method only referenced as a delegate target stays.
    [Test]
    public void Delegate_Target_Is_Kept()
    {
        Func<uint> f = Callbacks.Seven;
        Assert.AreEqual(f(), 7u);
    }
}
