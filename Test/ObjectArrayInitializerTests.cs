using System;
using C64Lib;
using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.Test;

// Array literals of objects: runs of elements whose fields are all constants
// are built from one table by NewObjArrayInitL (Compiler/
// ILObjectArrayInitializerOptimizer.cs); everything else keeps its own code.
// Every field of every element is checked, so a wrong index, a shifted
// table row or a wrong object size shows up here. (Checks are table driven:
// this program is close to its memory ceiling.)
class InitTone
{
    public ulong Frequency;
    public uint Sustain;
    public string Name;
}

class InitNode
{
    public InitNode Child;
    public uint Value;
    public ulong Wide;
}

[TestFixture]
public class ObjectArrayInitializerTests
{
    // Not a constant: an element using it falls back to ordinary code.
    static uint s_seed = 33;

    static bool ToneIs(InitTone t, ulong frequency, uint sustain) =>
        t.Frequency == frequency && t.Sustain == sustain;

    static bool NodeIs(InitNode n, uint value, ulong wide, bool hasChild) =>
        n.Value == value && n.Wide == wide && (n.Child != null) == hasChild;

    [Test]
    public void Constant_Object_Array_Has_Every_Field()
    {
        var tones = new[]
        {
            new InitTone { Frequency = 0x0CD8, Sustain = 10, Name = "AB" },
            new InitTone { Frequency = 0x1125, Sustain = 20, Name = "CD" },
            new InitTone { Frequency = 0x224B, Sustain = 255, Name = "EF" },
            new InitTone { Sustain = 7 },
            new InitTone(),
        };

        ulong[] frequencies = { 0x0CD8, 0x1125, 0x224B, 0, 0 };
        uint[] sustains = { 10, 20, 255, 7, 0 };
        for (int i = 0; i < 5; i++)
            Assert.IsTrue(ToneIs(tones[i], frequencies[i], sustains[i]), "tone");
        Assert.AreEqualString(tones[0].Name, "AB");
        Assert.AreEqualString(tones[1].Name, "CD");
        Assert.AreEqualString(tones[2].Name, "EF");
    }

    [Test]
    public void Mixed_Object_Array_With_A_Reference_Field_Has_Every_Field()
    {
        var nodes = new[]
        {
            new InitNode { Value = 1, Wide = 1000 },                            // table (run of 2)
            new InitNode { Value = 2, Wide = 2000 },
            new InitNode { Value = s_seed, Wide = 3000 },                       // not constant
            new InitNode { Child = new InitNode { Value = 99 }, Value = 4 },    // sets the reference field
            new InitNode { Value = 5 },                                         // table again, from index 4
            new InitNode { Value = 6, Wide = 6000 },
            new InitNode(),
        };

        uint[] values = { 1, 2, 33, 4, 5, 6, 0 };
        ulong[] wides = { 1000, 2000, 3000, 0, 0, 6000, 0 };
        for (int i = 0; i < 7; i++)
            Assert.IsTrue(NodeIs(nodes[i], values[i], wides[i], i == 3), "node");
        Assert.IsTrue(NodeIs(nodes[3].Child, 99, 0, false), "child");

        // A table-built object's reference field is traced by the GC like
        // any other, and the objects themselves stay reachable through the
        // array.
        nodes[4].Child = new InitNode { Value = 77 };
        var childId = C64.Debug.GetObjectId(nodes[4].Child);
        GC.Collect();
        Assert.IsTrue(C64.Debug.IsAlive(childId), "gc");
        Assert.IsTrue(NodeIs(nodes[4].Child, 77, 0, false), "gc child");
    }

    // `x.Field = new T { A = constant }`: the object initializer's last
    // constant store is followed directly by the store into x. That used to
    // be folded into the new object as well (constant written at the
    // field's offset, x.Field never set).
    [Test]
    public void Object_Initializer_Stored_Into_A_Field()
    {
        var holder = new InitNode();
        holder.Child = new InitNode { Value = 5 };
        Assert.IsTrue(NodeIs(holder, 0, 0, true), "holder");
        Assert.IsTrue(NodeIs(holder.Child, 5, 0, false), "child");
    }
}
