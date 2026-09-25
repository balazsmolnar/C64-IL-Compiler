using NUnit.Framework;

namespace Compiler.Test;

// Reproduces a real compiler bug found while building Demo/Program.cs's
// rotating-cube scene: ILTypeStaticFieldInitPass.cs emits every static
// field as a single ".byte 0", regardless of the field's actual storage
// width (TypeExtensions.GetStorageBytes() -- 5 bytes for float's MFLPT
// representation, 2 for long/ulong, 1 for everything else). The read/
// write macros Ldsfld/Stsfld emit (Compiler/Operands/OperandBase.cs's
// OpLdsld/OpStsfld -- #stack_push_var_mflpt/#stack_pull_mflpt for float,
// #stack_push_var16/#stack_pull_int16 for long/ulong) DO use the correct
// width. So a multi-byte static field's address is under-reserved by the
// allocator but over-written by every read/write against it -- writing
// FieldA silently spills into whatever's declared right after it
// (originally reserved for FieldB), and if FieldB is EVER written too,
// FieldB's write spills back over FieldA's own trailing bytes, corrupting
// it. This is exactly what happened to Demo/Program.cs's cosAx_/sinAx_
// (or, in an earlier version, cosAy_/sinAy_ reassigned per rotation
// frame): the second field's write silently corrupted the first's value.
[TestFixture]
public class StaticFieldLayoutTest
{
    // float is 5-byte MFLPT storage. With only 1 byte reserved per field,
    // fieldA_'s write (5 bytes starting at fieldA_'s own address) already
    // spills into fieldB_'s reserved byte and beyond; fieldB_'s own write
    // right after (5 bytes starting 1 byte past fieldA_'s address)
    // overwrites fieldA_'s bytes 1-4 with fieldB_'s bytes 0-3, corrupting
    // fieldA_'s mantissa.
    static float floatFieldA_;
    static float floatFieldB_;

    [TestCase(ExpectedResult = true)]
    public bool StaticFloatField_SecondFieldWrite_DoesNotCorruptFirst()
    {
        floatFieldA_ = 2.5f;
        floatFieldB_ = 4.5f;
        return floatFieldA_ == 2.5f;
    }

    // Same underlying bug, for long/ulong's 2-byte storage: ulongA_'s
    // write (2 bytes) spills 1 byte into ulongB_'s reserved byte;
    // ulongB_'s write then overwrites ulongA_'s high byte.
    static ulong ulongFieldA_;
    static ulong ulongFieldB_;

    [TestCase(ExpectedResult = true)]
    public bool StaticULongField_SecondFieldWrite_DoesNotCorruptFirst()
    {
        ulongFieldA_ = 300;
        ulongFieldB_ = 500;
        return ulongFieldA_ == 300;
    }
}
