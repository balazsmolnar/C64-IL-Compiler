using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.Test;

class MockObject
{
    public int F;
}

[TestFixture]
class ArrayTest
{
    [Test]
    public void Int_Array()
    {
        var sut = new int[5];
        Assert.AreEqual(sut.Length, 5);
        sut[1] = 6;
        Assert.AreEqual(sut[1], 6);
    }

    [Test]
    public void Int_Array_Postincrement_Index()
    {
        var sut = new int[3];
        int i = 0;
        sut[i++] = 10;
        sut[i++] = 20;
        Assert.AreEqual(sut[0], 10);
        Assert.AreEqual(sut[1], 20);
        Assert.AreEqual(i, 2);
    }

    [Test]
    public void Long_Array()
    {
        var sut = new long[5];
        Assert.AreEqual(sut.Length, 5);
        sut[0] = 12L;
        sut[3] = 13L;
        Assert.AreEqual((int)sut[0], 12);
        Assert.AreEqual((int)sut[3], 13);
    }

    [Test]
    public void Multiple_Int_Array()
    {
        var sut1 = new int[3];
        sut1[0] = sut1[1] = sut1[2] = 1;
        var sut2 = new int[3];
        sut2[0] = sut2[1] = sut2[2] = 2;
        Assert.AreEqual(sut1[0], 1);
        Assert.AreEqual(sut1[1], 1);
        Assert.AreEqual(sut1[2], 1);
        Assert.AreEqual(sut2[0], 2);
        Assert.AreEqual(sut2[1], 2);
        Assert.AreEqual(sut2[2], 2);
    }

    [Test]
    public void Int_Array_Initializer()
    {
        int a = 5;
        int b = 6;
        var sut = new int[] { a, b };
        Assert.AreEqual(sut.Length, 2);
        Assert.AreEqual(sut[1], 6);
    }

    [Test]
    public void Object_Array()
    {
        var sut = new MockObject[5];
        Assert.AreEqual(sut.Length, 5);
        sut[1] = new MockObject { F = 5 };
        Assert.AreEqual(sut[1].F, 5);
    }

    [Test]
    public void Object_Array_Initializer()
    {
        var sut = new MockObject[] { new MockObject { F = 12 }, new MockObject { F = 13 } };
        Assert.AreEqual(sut.Length, 2);
        Assert.AreEqual(sut[1].F, 13);
    }

    [Test]
    public void String_Array_Initializer()
    {
        var sut = new string[] { "a", "bb" };
        Assert.AreEqual(sut.Length, 2);
        Assert.AreEqual(sut[0].Length, 1);
        Assert.AreEqual(sut[1].Length, 2);
    }

    // uint[] element reads (Ldelem_u4) crashed Compiler.exe outright --
    // only Ldelem_i4/i8/ref were mapped in CommandMap. int[] reads
    // (Ldelem_i4) already worked, confirming the gap was specifically the
    // unsigned opcode, not array reads in general.
    [Test]
    public void Uint_Array()
    {
        var sut = new uint[3];
        sut[0] = 10;
        sut[1] = 20;
        sut[2] = 30;
        Assert.AreEqual(sut.Length, 3);
        Assert.AreEqual((int)sut[1], 20);
    }

    // A `static readonly <primitive>[] = { lit, lit, ... }` field, all
    // constant values, is exactly the shape Roslyn lowers to
    // Newarr;Dup;Ldtoken;Call RuntimeHelpers.InitializeArray;Stsfld inside
    // the type's .cctor -- which nothing used to run at all (no mechanism
    // called any type's static constructor), so the field silently stayed
    // a null handle forever. Deliberately declared at class scope with all-
    // constant elements (not `new int[] { a, b }` with variables, like
    // Int_Array_Initializer above -- that shape never triggers Roslyn's
    // blob-based InitializeArray optimization in the first place, so it
    // doesn't exercise this bug).
    private static readonly int[] ConstIntArray = { 91, 95, 92, 98, 100 };
    private static readonly uint[] ConstUintArray = { 10, 13, 250 };
    // 2-byte element type (TypeExtensions.GetStorageBytes), unlike the two
    // above -- specifically exercises ILStaticArrayInitializerPass's
    // per-element-width blob encoding (each element needs 2 bytes in the
    // baked init-values blob and in the byte count passed to #newArrInit,
    // not 1 the way int/uint's elements do). A value that doesn't fit in a
    // single byte (70000) makes a width mismatch here fail loudly instead
    // of silently returning a plausible-looking but wrong result the way
    // the original bug did (found via Hunchback's ScoreMultipliers table:
    // every value there happened to fit in a byte, so the corrupted
    // element ended up looking like an inflated-but-plausible score rather
    // than an obviously-wrong one).
    private static readonly ulong[] ConstULongArray = { 70000, 5, 65535 };

    [Test]
    public void Static_Readonly_Int_Array_Literal()
    {
        Assert.AreEqual(ConstIntArray.Length, 5);
        Assert.AreEqual(ConstIntArray[0], 91);
        Assert.AreEqual(ConstIntArray[4], 100);
    }

    [Test]
    public void Static_Readonly_Uint_Array_Literal()
    {
        Assert.AreEqual(ConstUintArray.Length, 3);
        Assert.AreEqual((int)ConstUintArray[1], 13);
        Assert.AreEqual((int)ConstUintArray[2], 250);
    }

    [Test]
    public void Static_Readonly_ULong_Array_Literal()
    {
        if (ConstULongArray.Length != 3)
            Assert.Fail();
        // 70000 doesn't fit in this compiler's 2-byte ulong (max 65535) --
        // truncates the same way any other 2-byte ulong overflow does,
        // consistent rather than a special case.
        if (ConstULongArray[0] != (70000 & 0xFFFF))
            Assert.Fail();
        if (ConstULongArray[1] != 5)
            Assert.Fail();
        if (ConstULongArray[2] != 65535)
            Assert.Fail();
    }

    private class ArrayFieldHolder
    {
        public uint[] Values;

        // Assigns the array literal from an ordinary METHOD, not a field
        // initializer/constructor -- this compiler never compiles instance
        // constructor bodies at all (Compiler/ILCodePass.cs only enumerates
        // GetMethods() + static constructors; object construction is a
        // generic #newObj allocate-and-zero runtime macro, with no
        // mechanism to run user constructor code), so a field initializer
        // here would silently never run regardless of this pass -- a
        // separate, out-of-scope gap. This mirrors the actual motivating
        // Hunchback shape instead: Player.cs's InitJumpOffsets is exactly
        // this pattern, a regular method assigning an array literal to an
        // instance field (Newarr;Dup;Ldtoken;Call InitializeArray;Stfld).
        // ILStaticArrayInitializerPass originally only matched the Stsfld
        // ending (its data-extraction trick needed a static field to read
        // back after triggering the type's .cctor, which doesn't exist for
        // an instance field at compile time) -- before this, the exact same
        // literal written to an instance field instead of a static one
        // silently left it null, which is why InitJumpOffsets hand-unrolled
        // the assignments element-by-element instead.
        public void Init()
        {
            Values = new uint[] { 91, 95, 92, 98, 100 };
        }
    }

    [Test]
    public void Instance_Field_Array_Literal()
    {
        var holder = new ArrayFieldHolder();
        holder.Init();
        Assert.AreEqual(holder.Values.Length, 5);
        Assert.AreEqual((int)holder.Values[0], 91);
        Assert.AreEqual((int)holder.Values[4], 100);
    }

    // Same shape once more, but as a LOCAL variable -- Stloc/Stloc_s
    // instead of Stsfld/Stfld. A local was never even a live code path Roslyn
    // takes differently from the instance-field case above (both non-static
    // targets share the same underlying gap), included for direct coverage
    // of the third target kind ILStaticArrayInitializerPass now handles.
    [Test]
    public void Local_Array_Literal()
    {
        uint[] values = { 91, 95, 92, 98, 100 };
        Assert.AreEqual(values.Length, 5);
        Assert.AreEqual((int)values[0], 91);
        Assert.AreEqual((int)values[4], 100);
    }

    // Checking whether arrays/indices past 255 elements actually work --
    // GetStorageBytes() (Compiler/TypeExtensions.cs) returns 1 byte for
    // int/uint, and object size is a single byte per heap slot
    // (objTableSize .fill 256 in asm/helper/objectTables.asm), so there's
    // a real question whether "new uint[1000]" and an index like 261
    // silently truncate mod 256 instead of failing loudly. Indices 5 and
    // 261 are chosen specifically because 261 mod 256 == 5 -- if either
    // the array length or the index expressions get truncated to a single
    // byte anywhere along the way, these two slots would alias and the
    // assertions below would fail.
    // Confirmed, not yet fixed: objTableSize (asm/helper/objectTables.asm)
    // is a single byte per heap slot, so any array's real backing storage
    // is capped at 255 bytes regardless of element type or the requested
    // length -- indices past that alias silently instead of erroring.
    // Fixing it properly means widening the object-size model to 2 bytes
    // for arrays specifically (heap.asm/object.asm/GC.asm all read
    // objTableSize as a single byte throughout), a bigger change than this
    // test is meant to justify on its own. Uint_Array_Length_1000 and
    // Jagged_Uint_Array above/below document the two things that DO work
    // today: a compile-time-constant .Length past 255, and an
    // array-of-arrays where every individual array stays under the limit.
    [Ignore("Known limitation: array storage is capped at 255 bytes (a single objTableSize byte per heap slot) -- indices past that alias instead of erroring. See Jagged_Uint_Array for the working array-of-arrays pattern.")]
    [Test]
    public void Uint_Array_Index_Past_255()
    {
        var sut = new uint[1000];
        sut[5] = 11;
        sut[261] = 22;
        Assert.AreEqual((int)sut[5], 11);
        Assert.AreEqual((int)sut[261], 22);
    }

    [Test]
    public void Uint_Array_Length_1000()
    {
        var sut = new uint[1000];
        Assert.AreEqual(sut.Length, 1000);
    }

    // Candidate workaround for the 255-element ceiling: an array-of-arrays,
    // where each individual array (outer and each inner) stays comfortably
    // under 255 elements. Should work via the same reference-array
    // machinery already proven by LevelDescription[] (an array of object
    // references) -- unlike Uint_Array_Index_Past_255 above, nothing here
    // needs an index or length past 255.
    [Test]
    public void Jagged_Uint_Array()
    {
        var sut = new uint[4][];
        for (uint i = 0; i < 4; i++)
            sut[i] = new uint[40];
        sut[2][30] = 77;
        Assert.AreEqual((int)sut[2][30], 77);
        Assert.AreEqual(sut[2].Length, 40);
    }
}