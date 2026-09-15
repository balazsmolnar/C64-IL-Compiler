using NUnit.Framework;
using Compiler;

namespace Compiler.UnitTests;

[TestFixture]
public class MflptTests
{
    [TestCase(0f, 0x00, 0x00, 0x00, 0x00, 0x00)]
    [TestCase(1f, 0x81, 0x00, 0x00, 0x00, 0x00)]
    [TestCase(-1f, 0x81, 0x80, 0x00, 0x00, 0x00)]
    [TestCase(0.5f, 0x80, 0x00, 0x00, 0x00, 0x00)]
    [TestCase(2f, 0x82, 0x00, 0x00, 0x00, 0x00)]
    [TestCase(-0.5f, 0x80, 0x80, 0x00, 0x00, 0x00)]
    public void ToBytes_MatchesKnownReferenceEncoding(float value, byte b0, byte b1, byte b2, byte b3, byte b4)
    {
        var bytes = Mflpt.ToBytes(value);
        Assert.That(bytes, Is.EqualTo(new[] { b0, b1, b2, b3, b4 }));
    }

    [TestCase(0f)]
    [TestCase(1f)]
    [TestCase(-1f)]
    [TestCase(0.5f)]
    [TestCase(-0.5f)]
    [TestCase(3.14159f)]
    [TestCase(-123.456f)]
    [TestCase(100000f)]
    [TestCase(0.0001f)]
    [TestCase(-0.0001f)]
    public void RoundTrips_ExactlyThroughToBytesAndFromBytes(float value)
    {
        var bytes = Mflpt.ToBytes(value);
        var back = Mflpt.FromBytes(bytes);
        Assert.That(back, Is.EqualTo(value));
    }

    [Test]
    public void ToBytes_Throws_ForInfinity()
    {
        Assert.Throws<System.InvalidOperationException>(() => Mflpt.ToBytes(float.PositiveInfinity));
    }

    [Test]
    public void ToBytes_Throws_ForNaN()
    {
        Assert.Throws<System.InvalidOperationException>(() => Mflpt.ToBytes(float.NaN));
    }

    [Test]
    public void ToBytes_FlushesSubnormalToZero()
    {
        var bytes = Mflpt.ToBytes(float.Epsilon);
        Assert.That(bytes, Is.EqualTo(new byte[5]));
    }
}
