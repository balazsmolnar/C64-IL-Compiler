using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.ProgramTest;

// The shape of C64Presentation's ButterflyChart "grow" animation (one series
// at a time, all of its bars growing together), which drew garbage.
sealed class GrowRow
{
    public string Label { get; init; }
    public ulong LeftValue { get; init; }
    public ulong RightValue { get; init; }
}

sealed class GrowChart
{
    public ulong Max { get; init; }
    public GrowRow[] Rows { get; init; }

    public readonly ulong[] X0 = new ulong[8];
    public readonly ulong[] Len = new ulong[8];
    public uint Frames;

    public void DrawSeries(bool left, ulong axis, uint color)
    {
        const ulong step = 8;
        ulong area = 64;
        for (ulong grown = step; ; grown += step)
        {
            bool growing = false;
            uint row = 4;
            foreach (var item in Rows)
            {
                ulong length = Length(left ? item.LeftValue : item.RightValue, area);
                if (length > grown)
                {
                    length = grown;
                    growing = true;
                }
                Bar(left ? axis - length : axis, length, row, color);
                row += 2;
            }
            Frames++;
            if (!growing)
                break;
        }
    }

    ulong Length(ulong value, ulong area)
    {
        ulong max = Max;
        while (max > 255)
        {
            max /= 2;
            value /= 2;
        }
        ulong length = value * area / max;
        return length < 2 ? 2 : length;
    }

    void Bar(ulong x, ulong length, uint row, uint color)
    {
        uint i = (row - 4) / 2;
        X0[i] = x;
        Len[i] = length;
    }
}

[TestFixture]
public class GrowingBarsTests
{
    static GrowChart Make() => new GrowChart
    {
        Max = 3420,
        Rows =
        [
            new() { Label = "a", LeftValue = 3357, RightValue = 0 },
            new() { Label = "b", LeftValue = 2304, RightValue = 1818 },
            new() { Label = "c", LeftValue = 1983, RightValue = 2579 },
            new() { Label = "d", LeftValue = 824, RightValue = 1168 },
            new() { Label = "e", LeftValue = 0, RightValue = 616 },
            new() { Label = "f", LeftValue = 311, RightValue = 495 },
            new() { Label = "g", LeftValue = 28, RightValue = 340 },
            new() { Label = "h", LeftValue = 85, RightValue = 326 },
        ],
    };

    // max 3420 -> 213 after 4 halvings; value/16 * 64 / 213.
    [Test]
    public void LeftSeriesGrowsToItsLengths()
    {
        var chart = Make();
        chart.DrawSeries(true, 111, 3);
        Assert.AreEqual(chart.Frames, 8u, "A1");
        Assert.AreEqual((uint)chart.Len[0], 62u, "A2");
        Assert.AreEqual((uint)chart.X0[0], 49u, "A3");
        Assert.AreEqual((uint)chart.Len[1], 43u, "A4");
        Assert.AreEqual((uint)chart.X0[1], 68u, "A5");
        Assert.AreEqual((uint)chart.Len[3], 15u, "A6");
        Assert.AreEqual((uint)chart.Len[4], 2u, "A7");
        Assert.AreEqual((uint)chart.X0[4], 109u, "A8");
        Assert.AreEqual((uint)chart.Len[7], 2u, "A9");
    }

    [Test]
    public void RightSeriesGrowsToItsLengths()
    {
        var chart = Make();
        chart.DrawSeries(false, 208, 3);
        Assert.AreEqual(chart.Frames, 6u, "A10");
        Assert.AreEqual((uint)chart.X0[0], 208u, "A11");
        Assert.AreEqual((uint)chart.Len[0], 2u, "A12");
        Assert.AreEqual((uint)chart.Len[2], 48u, "A13");
        Assert.AreEqual((uint)chart.X0[5], 208u, "A14");
        Assert.AreEqual((uint)chart.Len[5], 9u, "A15");
    }


    sealed class Box
    {
        public uint V;
        public ulong W;
    }

    static uint ReadAfterReassign(Box a, Box b)
    {
        Box current = a;
        uint first = current.V;
        current = b;            // the cached pointer must not survive this
        uint second = current.V;
        return first * 10 + second;
    }

    static ulong PickField(bool left, Box a, Box b)
    {
        Box item = left ? a : b;
        return left ? item.W : (ulong)item.V;  // a field read on each arm
    }

    [Test]
    public void CachedFieldPointerIsDroppedWhenTheLocalChanges()
    {
        Assert.AreEqual(ReadAfterReassign(new Box { V = 3 }, new Box { V = 4 }), 34u, "B1");
    }

    [Test]
    public void CachedFieldPointerIsNotTakenAcrossATernaryArm()
    {
        var a = new Box { V = 1, W = 500 };
        var b = new Box { V = 2, W = 600 };
        Assert.AreEqual((uint)(PickField(true, a, b) / 100), 5u, "C1");
        Assert.AreEqual((uint)PickField(false, a, b), 2u, "C2");
    }
}
