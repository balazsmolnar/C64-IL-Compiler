using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Ops;

class OpArithmetic2 : OpBase
{
    public OpArithmetic2(string command) : base(0, command)
    {
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        var last = operation.StackContent.Last();
        var last2 = operation.StackContent.Last(1);
        if (last != typeof(int) && last != typeof(uint) && last != typeof(long) && last != typeof(ulong) && last != typeof(bool) && last != typeof(float))
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
        if (last != typeof(int) && last != typeof(uint) && last != typeof(long) && last != typeof(ulong) && last != typeof(bool) && last != typeof(float))
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
        if (last != typeof(int) && last != typeof(uint) && last != typeof(long) && last != typeof(ulong) && last != typeof(bool) && last != typeof(float))
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