using System;
using C64Lib;
using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.ProgramTest;

enum TableColor
{
    Red = 2,
    Green = 5,
    Blue = 6,
    White = 1,
}

class StringHolder
{
    public string[] Items { get; init; }
    public uint Count { get; init; }
}

// Array literals with constant elements compile to one #newArrInit table
// (Compiler/ILConstantArrayInitializerOptimizer.cs): string[] as a table of
// string addresses, enum/uint/ulong arrays as their bytes. The array must
// still be an ordinary heap array: same contents, rooted and collected like
// one built element by element.
[TestFixture]
public class ConstantArrayTableTests
{
    static string[] s_strings;
    static string[] s_built;
    static string Dynamic() => "dyn";

    static uint CountChars(string[] items)
    {
        uint n = 0;
        foreach (var item in items)
            n += (uint)item.Length;
        return n;
    }

    [Test]
    public void String_Array_Literal_Contents()
    {
        var items = new[] { "alpha", "be", "gamma", "d", "epsilon" };
        Assert.AreEqual(items.Length, 5);
        Assert.IsTrue(items[0] == "alpha");
        Assert.IsTrue(items[1] == "be");
        Assert.IsTrue(items[2] == "gamma");
        Assert.IsTrue(items[3] == "d");
        Assert.IsTrue(items[4] == "epsilon");
    }

    // A null element is a skipped store in IL: its slot must stay 0.
    [Test]
    public void String_Array_Literal_With_Null_And_Dynamic_Elements()
    {
        string[] items = ["one", null, Dynamic(), "four"];
        Assert.AreEqual(items.Length, 4);
        Assert.IsTrue(items[0] == "one");
        Assert.IsTrue(items[1] == null);
        Assert.IsTrue(items[2] == "dyn");
        Assert.IsTrue(items[3] == "four");
    }

    [Test]
    public void String_Array_Survives_GC_And_Is_Collected_When_Dropped()
    {
        s_strings = ["red", "green", "blue"];
        var id = C64.Debug.GetObjectId(s_strings);
        // Rooted exactly like an array built element by element.
        s_built = new string[3];
        s_built[0] = Dynamic();
        var builtId = C64.Debug.GetObjectId(s_built);
        var roots = C64.Debug.GetObjectRoots(builtId);
        Assert.IsTrue(roots == 1, "one root");
        Assert.IsTrue(C64.Debug.GetObjectRoots(id) == roots, "same roots");

        // Garbage around it, then a collection that compacts the heap.
        for (uint i = 0; i < 4; i++)
        {
            var junk = new[] { "x", "y" };
            junk[0] = "z";
        }
        GC.Collect();

        // Bit 7 of the root count is the collector's mark bit.
        Assert.IsTrue(C64.Debug.IsAlive(id), "alive");
        Assert.IsTrue((C64.Debug.GetObjectRoots(id) & 127) == 1, "roots after GC");
        Assert.IsTrue((C64.Debug.GetObjectRoots(builtId) & 127) == 1, "built roots after GC");
        Assert.IsTrue(s_strings[0] == "red");
        Assert.IsTrue(s_strings[1] == "green");
        Assert.IsTrue(s_strings[2] == "blue");

        s_strings = null;
        s_built = null;
        GC.Collect();
        Assert.IsFalse(C64.Debug.IsAlive(id), "table array collected");
        Assert.IsFalse(C64.Debug.IsAlive(builtId), "built array collected");
    }

    [Test]
    public void String_Array_In_An_Initializer_And_As_An_Argument()
    {
        var holder = new StringHolder { Count = 3, Items = ["ab", "cde", "f"] };
        GC.Collect();
        Assert.AreEqual(holder.Count, 3u);
        Assert.AreEqual(CountChars(holder.Items), 6u);
        Assert.AreEqual(CountChars(["1234", "56"]), 6u);
        Assert.IsTrue(holder.Items[1] == "cde");
    }

    [Test]
    public void Enum_And_Small_Number_Arrays()
    {
        TableColor[] colors = [TableColor.Green, TableColor.Red, TableColor.White];
        Assert.AreEqual(colors.Length, 3);
        Assert.IsTrue(colors[0] == TableColor.Green);
        Assert.IsTrue(colors[1] == TableColor.Red);
        Assert.IsTrue(colors[2] == TableColor.White);

        uint[] pair = [7, 9];
        Assert.AreEqual(pair[0] + pair[1], 16u);
    }

    [Test]
    public void Ulong_Array_With_A_Dynamic_Element()
    {
        ulong[] values = [300, (ulong)Dynamic().Length, 65535, 2];
        GC.Collect();
        Assert.IsTrue(values[0] == 300);
        Assert.IsTrue(values[1] == 3);
        Assert.IsTrue(values[2] == 65535);
        Assert.IsTrue(values[3] == 2);
    }
}
