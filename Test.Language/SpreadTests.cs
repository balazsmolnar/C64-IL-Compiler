using System;
using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.LanguageTest;

// `[.. a, x, .. b]` into an array: Roslyn allocates the summed length and
// copies each spread with ReadOnlySpan<T>.CopyTo(Span<T>.Slice(...));
// Compiler/SpreadSupport.cs maps that onto a byte copy (#arrayCopyInto).
class SpreadItem
{
    public uint Id;
}

static class SpreadPartA
{
    public static readonly uint[] Values = [1, 2, 3];
    public static readonly ulong[] Wide = [1000, 2000];
    public static readonly SpreadItem[] Items = [new SpreadItem { Id = 1 }, new SpreadItem { Id = 2 }];
    public static readonly Func<uint>[] Makers = [() => 10, () => 20];
}

static class SpreadPartB
{
    public static readonly uint[] Values = [7, 8];
    public static readonly ulong[] Wide = [3000];
    public static readonly SpreadItem[] Items = [new SpreadItem { Id = 5 }];
    public static readonly Func<uint>[] Makers = [() => 30];
}

[TestFixture]
public class SpreadTests
{
    static readonly Func<uint>[] AllMakers = [.. SpreadPartA.Makers, .. SpreadPartB.Makers];

    [Test]
    public void SpreadValueArrays()
    {
        uint[] all = [.. SpreadPartA.Values, .. SpreadPartB.Values];
        Assert.AreEqual((uint)all.Length, 5u);
        Assert.AreEqual(all[0], 1u);
        Assert.AreEqual(all[2], 3u);
        Assert.AreEqual(all[3], 7u);
        Assert.AreEqual(all[4], 8u);
    }

    [Test]
    public void SpreadWideValuesWithSingleElements()
    {
        ulong extra = 500;
        ulong[] all = [.. SpreadPartA.Wide, extra, .. SpreadPartB.Wide];
        Assert.AreEqual((uint)all.Length, 4u);
        Assert.AreEqual((uint)(all[0] / 1000), 1u);
        Assert.AreEqual((uint)(all[1] / 1000), 2u);
        Assert.AreEqual((uint)(all[2] / 100), 5u);
        Assert.AreEqual((uint)(all[3] / 1000), 3u);
    }

    [Test]
    public void SpreadReferenceArrays()
    {
        var single = new SpreadItem { Id = 4 };
        SpreadItem[] all = [.. SpreadPartA.Items, single, .. SpreadPartB.Items];
        Assert.AreEqual((uint)all.Length, 4u);
        Assert.AreEqual(all[0].Id, 1u);
        Assert.AreEqual(all[1].Id, 2u);
        Assert.AreEqual(all[2].Id, 4u);
        Assert.AreEqual(all[3].Id, 5u);
        // Shared, not copied: the parts see the same objects.
        all[1].Id = 6;
        Assert.AreEqual(SpreadPartA.Items[1].Id, 6u);
        SpreadPartA.Items[1].Id = 2;
    }

    [Test]
    public void SpreadDelegateArraysInAStatic()
    {
        Assert.AreEqual((uint)AllMakers.Length, 3u);
        Assert.AreEqual(AllMakers[0]() + AllMakers[1]() + AllMakers[2](), 60u);
    }

    [Test]
    public void SpreadOfAnEmptyArrayAndALocal()
    {
        uint[] none = new uint[0];
        uint[] local = [4, 5];
        uint[] all = [.. none, .. local, .. none];
        Assert.AreEqual((uint)all.Length, 2u);
        Assert.AreEqual(all[0] * 10 + all[1], 45u);
    }
}
