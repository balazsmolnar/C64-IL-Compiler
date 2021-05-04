using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Ops
{
    class OpArithmetic2 : OpBase
    {
        public OpArithmetic2(string command) : base(0, command)
        {
        }

        public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
        {
            var last = operation.StackContent.Last();
            var last2 = operation.StackContent.Last(1);
            if (last != typeof(int) && last != typeof(uint) && last != typeof(long) && last != typeof(ulong))
                throw new InvalidOperationException("Unsupported type in arithmetic operation.");
            if (last2.GetStorageBytes() != last.GetStorageBytes())
                throw new InvalidOperationException("2 types in stack must be equal.");

            operation.StackContent.RemoveLast(2);
            operation.StackContent.Add(last);
        }
        public override bool Is16BitSupported => true;

    }

    class OpArithmetic1 : OpBase
    {
        public OpArithmetic1(string command) : base(0, command)
        {
        }

        public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
        {
            var last = operation.StackContent.Last();
            if (last != typeof(int) && last != typeof(uint) && last != typeof(long) && last != typeof(ulong))
                throw new InvalidOperationException("Unsupported type in arithmetic operation.");

            operation.StackContent.RemoveLast(1);
            operation.StackContent.Add(last);
        }

        public override bool Is16BitSupported => true;
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
            if (last != typeof(int) && last != typeof(uint) && last != typeof(long) && last != typeof(ulong))
                throw new InvalidOperationException("Unsupported type in arithmetic operation.");
            if (last2.GetStorageBytes() != last.GetStorageBytes())
                throw new InvalidOperationException("2 types in stack must be equal.");

            operation.StackContent.RemoveLast(2);
            operation.StackContent.Add(typeof(bool));
        }

    }

}
