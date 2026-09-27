using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Ops;

class OpArithmetic2 : OpBase
{
    // The types an arithmetic/compare operand may have on the evaluation stack.
    // byte, sbyte and char are 1-byte values like int/uint (this compiler's
    // narrow integer widths; see TypeExtensions.GetStorageBytes).
    internal static bool IsNumericOperand(System.Type type) =>
        type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong) ||
        type == typeof(bool) || type == typeof(float) ||
        type == typeof(byte) || type == typeof(sbyte) || type == typeof(char);

    public OpArithmetic2(string command) : base(0, command)
    {
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        var last = operation.StackContent.Last();
        var last2 = operation.StackContent.Last(1);
        if (!IsNumericOperand(last))
            throw new InvalidOperationException("Unsupported type in arithmetic operation.");
        if (last2.GetStorageBytes() != last.GetStorageBytes())
            throw new InvalidOperationException("2 types in stack must be equal.");

        operation.StackContent.RemoveLast(2);
        operation.StackContent.Add(last);
    }
    public override bool Is16BitSupported => true;

    // "flt" instead of "8"/"16" -- float isn't a width variant of the same
    // macro family, it's an entirely different implementation (the BASIC
    // ROM's routines, see asm/helper/float.asm), so it gets its own command
    // name rather than a size suffix. Checked after SetStackContent has
    // already run, same timing the base Is16Bit relies on: for a 2-operand
    // arithmetic op the result type (re-added via Add(last) above) is the
    // same type as both operands, so StackContent.Last() here is exactly
    // "does this op operate on float".
    protected override string SizeSuffix(CompilerMethodContext context, ILOperation operation)
    {
        if (operation.StackContent.Last() == typeof(float))
            return "flt";
        return base.SizeSuffix(context, operation);
    }
}

class OpArithmetic1 : OpBase
{
    public OpArithmetic1(string command) : base(0, command)
    {
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        var last = operation.StackContent.Last();
        if (!OpArithmetic2.IsNumericOperand(last))
            throw new InvalidOperationException("Unsupported type in arithmetic operation.");

        operation.StackContent.RemoveLast(1);
        operation.StackContent.Add(last);
    }

    public override bool Is16BitSupported => true;

    protected override string SizeSuffix(CompilerMethodContext context, ILOperation operation)
    {
        if (operation.StackContent.Last() == typeof(float))
            return "flt";
        return base.SizeSuffix(context, operation);
    }
}

class OpCompare : OpArithmetic2
{
    public OpCompare(string command) : base(command)
    {
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        var last = operation.StackContent.Last();
        var last2 = operation.StackContent.Last(1);

        // Object references (class instances, arrays, null) are 1-byte object
        // table handles, so `a == b`, `a != b` and `a != null` used as a VALUE
        // (bool x = a != null; return a == b;) compare two bytes with the same
        // 8-bit/16-bit macros as numbers: equal handles/pointers = the same
        // object, 0 = null. (In an `if` condition the compiler emits
        // brtrue/beq, which already worked.)
        //
        // string is included here too, but ONLY reaches a raw Ceq/Cgt/Clt (as
        // opposed to a call to the overloaded op_Equality/op_Inequality,
        // handled separately via ILStringOpsPass -> String_Equals) when one
        // side is the null LITERAL: Roslyn compiles `s == null`/`s != null`
        // straight to `ceq`/`cgt.un` on the raw pointer, bypassing the
        // content-comparison operator entirely (confirmed by dumping the
        // actual IL) -- a real two-string content comparison (`a == b`,
        // neither side a literal null) always goes through op_Equality
        // instead, so a raw pointer-identity compare between two REAL
        // (non-null) strings never reaches here to be silently wrong.
        // OpLdnull infers string's width (2 bytes, not the usual 1-byte
        // handle) from context specifically for this case.
        bool isReference = !last.IsValueType;
        if (!isReference && !IsNumericOperand(last))
            throw new InvalidOperationException("Unsupported type in arithmetic operation.");
        if (last2.GetStorageBytes() != last.GetStorageBytes())
            throw new InvalidOperationException("2 types in stack must be equal.");

        operation.StackContent.RemoveLast(2);
        operation.StackContent.Add(typeof(bool));
    }

    public override bool Is16Bit(CompilerMethodContext context, ILOperation operation)
    {
        return operation.PreviousInstructions[0].StackContent.Last().GetStorageBytes() == 2;
    }

    // A compare's own result is always bool (1 byte) -- StackContent.Last()
    // (what OpArithmetic2's override checks) can never be float here, so
    // this has to look at the operand's type the same way Is16Bit above
    // does: the stack content of whatever fed this compare, one line back.
    protected override string SizeSuffix(CompilerMethodContext context, ILOperation operation)
    {
        if (operation.PreviousInstructions[0].StackContent.Last() == typeof(float))
            return "flt";
        return base.SizeSuffix(context, operation);
    }
}