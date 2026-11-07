using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.LanguageTest;

// Constructors that do real work (Compiler/ConstructorSupport.cs): bodies
// with parameters, field and property initializers, primary constructors,
// : base(...) / : this(...) chaining, a constructor that calls methods or
// allocates other objects, and arrays of objects built with constructors.

class CtorPoint
{
    public uint X;
    public uint Y;
    public CtorPoint(uint x, uint y) { X = x; Y = y; }
}

class CtorDefaults
{
    public uint A = 5;
    public ulong Wide = 1000;
    public float F = 1.5f;
    public uint P { get; set; } = 3;
    public bool On = true;
}

class CtorPrimary(uint speed)
{
    public uint X { get; private set; }
    public void Move() => X += speed;
}

class CtorAnimal(uint legs)
{
    public uint Legs => legs;
    public uint Eyes { get; set; } = 2;
}

class CtorDog : CtorAnimal
{
    public uint Tail;
    public CtorDog(uint tail) : base(4) { Tail = tail; }
}

class CtorChained
{
    public uint A;
    public uint B;
    public CtorChained(uint a) : this(a, a + 1) { }
    public CtorChained(uint a, uint b) { A = a; B = b; }
}

class CtorCallsMethods
{
    public uint Total;
    static uint Double(uint v) => v + v;
    void Add(uint v) { Total += v; }
    public CtorCallsMethods(uint v)
    {
        Add(Double(v));
        Add(1);
    }
}

class CtorChild
{
    public uint Value;
    public CtorChild(uint value) { Value = value; }
}

class CtorParent
{
    public CtorChild Child;
    public uint Own = 7;
    public CtorParent(uint v) { Child = new CtorChild(v + 1); }
}

class CtorNode
{
    public CtorNode Next;
    public uint Value;
    public CtorNode(uint n)
    {
        Value = n;
        if (n > 0)
            Next = new CtorNode(n - 1);
    }
}

class CtorWide
{
    public ulong W;
    public float F;
    public uint B;
    public CtorWide(ulong w, float f, uint b) { W = w; F = f; B = b; }
}

class CtorIgnoresArgument
{
    public uint Kept;
    public CtorIgnoresArgument(uint unused) { }
}

class CtorTakesReference
{
    public CtorChild Held;
    public uint Copy;
    public CtorTakesReference(CtorChild c) { Held = c; Copy = c.Value; }
}

class CtorBaseDefaults
{
    public uint Base1 = 11;
}

class CtorDerivedDefaults : CtorBaseDefaults
{
    public uint Derived1 = 22;
}

[TestFixture]
public class ConstructorTests
{
    [Test]
    public void Constructor_With_Parameters_Sets_Fields()
    {
        var p = new CtorPoint(3, 200);
        Assert.AreEqual(p.X, 3u);
        Assert.AreEqual(p.Y, 200u);
    }

    [Test]
    public void Field_And_Property_Initializers_Are_Applied()
    {
        var d = new CtorDefaults();
        Assert.AreEqual(d.A, 5u);
        Assert.IsTrue(d.Wide == 1000);
        Assert.IsTrue(d.F > 1.4f && d.F < 1.6f);
        Assert.AreEqual(d.P, 3u);
        Assert.IsTrue(d.On);
    }

    [Test]
    public void Object_Initializer_Runs_After_Field_Initializers()
    {
        var d = new CtorDefaults { P = 9 };
        Assert.AreEqual(d.A, 5u);
        Assert.AreEqual(d.P, 9u);
    }

    [Test]
    public void Inherited_Field_Initializers_Are_Applied()
    {
        var d = new CtorDerivedDefaults();
        Assert.AreEqual(d.Base1, 11u);
        Assert.AreEqual(d.Derived1, 22u);
    }

    [Test]
    public void Primary_Constructor_Captures_Its_Parameter()
    {
        CtorPrimary[] sky = [new(1), new(2)];
        foreach (var s in sky)
            s.Move();
        sky[1].Move();
        Assert.AreEqual(sky[0].X, 1u);
        Assert.AreEqual(sky[1].X, 4u);
    }

    [Test]
    public void Base_Constructor_Gets_Its_Arguments()
    {
        var dog = new CtorDog(9);
        Assert.AreEqual(dog.Legs, 4u);
        Assert.AreEqual(dog.Eyes, 2u);
        Assert.AreEqual(dog.Tail, 9u);
    }

    [Test]
    public void This_Constructor_Chaining_Works()
    {
        var c = new CtorChained(5);
        Assert.AreEqual(c.A, 5u);
        Assert.AreEqual(c.B, 6u);
    }

    [Test]
    public void Constructor_Can_Call_Methods()
    {
        var c = new CtorCallsMethods(4);
        Assert.AreEqual(c.Total, 9u);
    }

    [Test]
    public void Constructor_Can_Allocate_Another_Object()
    {
        var p = new CtorParent(40);
        System.GC.Collect();
        var q = new CtorParent(50);
        System.GC.Collect();
        Assert.AreEqual(p.Child.Value, 41u);
        Assert.AreEqual(q.Child.Value, 51u);
        Assert.AreEqual(p.Own, 7u);
    }

    [Test]
    public void Recursive_Constructor_Builds_A_List()
    {
        var head = new CtorNode(5);
        System.GC.Collect();
        uint count = 0;
        uint sum = 0;
        for (var n = head; n != null; n = n.Next)
        {
            count++;
            sum += n.Value;
        }
        Assert.AreEqual(count, 6u);
        Assert.AreEqual(sum, 15u);
    }

    [Test]
    public void Wide_Parameters_Are_Passed_In_Order()
    {
        var w = new CtorWide(40000, 2.5f, 7);
        Assert.IsTrue(w.W == 40000);
        Assert.IsTrue(w.F > 2.4f && w.F < 2.6f);
        Assert.AreEqual(w.B, 7u);
    }

    [Test]
    public void Arguments_Of_An_Empty_Constructor_Are_Dropped()
    {
        uint before = 3;
        var c = new CtorIgnoresArgument(before + 1);
        Assert.AreEqual(c.Kept, 0u);
        Assert.AreEqual(before, 3u);
    }

    [Test]
    public void Constructor_Takes_A_Reference()
    {
        var child = new CtorChild(8);
        var t = new CtorTakesReference(child);
        System.GC.Collect();
        Assert.AreEqual(t.Copy, 8u);
        Assert.AreEqual(t.Held.Value, 8u);
    }

    [Test]
    public void Array_Of_Objects_Built_With_Constructors()
    {
        var points = new CtorPoint[3];
        for (uint i = 0; i < 3; i++)
            points[i] = new CtorPoint(i, i + 10);
        uint sum = 0;
        foreach (var p in points)
            sum += p.X + p.Y;
        Assert.AreEqual(sum, 36u);
    }
}
