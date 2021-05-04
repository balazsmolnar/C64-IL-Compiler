using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compiler.Ops;

namespace Compiler.Ops
{
    class OpConv_8_16 : OpBase
    {
        public OpConv_8_16() : base(0, "#conv_8_16")
        {

        }

        public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
        {
            var last = operation.StackContent.Last();
            // last.CheckCompatible(typeof(int));
            operation.StackContent.RemoveLast(1);
            operation.StackContent.Add(typeof(long));
        }

        public override string Emit(CompilerMethodContext context, ILOperation operation)
        {
            if (operation.PreviousInstructions.First().StackContent.Last().GetStorageBytes() == 1)
                return base.Emit(context, operation);
            else
                return "; conv";
        }

    }

    class OpConv_16_8 : OpBase
    {
        public OpConv_16_8() : base(0, "#conv_16_8")
        {

        }

        public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
        {
            var last = operation.StackContent.Last();
            // last.CheckCompatible(typeof(long));
            operation.StackContent.RemoveLast(1);
            operation.StackContent.Add(typeof(int));
        }

        public override string Emit(CompilerMethodContext context, ILOperation operation)
        {
            if (operation.PreviousInstructions.First().StackContent.Last().GetStorageBytes() == 2)
                return base.Emit(context, operation);
            else
                return "; conv";
        }
    }

}
