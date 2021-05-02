using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Ops
{
    enum JumpType
    {
        UnConditional,
        Conditional,
        Compare
    }

    class OpJump : OpBase
    {
        private readonly JumpType _jumpType;

        public OpJump(int parameterSize, string command, JumpType jumpType) : base(parameterSize, command)
        {
            _jumpType = jumpType;
        }

        public override void SetNextInstructions(CompilerMethodContext context, ILOperation operation, ILOperation nextOperation)
        {
            if (_jumpType != JumpType.UnConditional)
                operation.NextInstructions.Add(nextOperation);
            var label = operation.RawParameter.ToString();
            var jmpInstruction = context.Lines.FirstOrDefault(l => l.Label == label);
            operation.NextInstructions.Add(jmpInstruction);
        }


        public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
        {
            switch (_jumpType)
            {
                case JumpType.Conditional:
                    operation.StackContent.RemoveLast(1);
                    break;
                case JumpType.Compare:
                    operation.StackContent.RemoveLast(2);
                    break;
            }
        }
    }
    class OpShortJump : OpJump
    {
        public OpShortJump(string command, JumpType jumpType) : base(1, command, jumpType)
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, ILOperation operation) => ((int)operation.RawParameter < 128 ? (int)operation.RawParameter : (int)operation.RawParameter - 256) + 2;

    }

    class OpLongJump : OpJump
    {
        public OpLongJump(string command, JumpType jumpType) : base(4, command, jumpType)
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, ILOperation operation) => (int)operation.RawParameter + 5;
    }

    class OpBranchConst : OpJump
    {
        public OpBranchConst(string command) : base(0, command, JumpType.Conditional)
        {
        }

        public override object ConvertParameter(CompilerMethodContext context, ILOperation operation) => 0;
    }

    class OpBranchIfNotEqual : OpBase
    {
        private int _varIndex;
        private int _value;
        private string _label;
        public OpBranchIfNotEqual(int varIndex, int value, string label) : base(0, "#branch_if_not_equal")
        {
            _varIndex = varIndex;
            _value = value;
            _label = label;
        }

        public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
        {
            var refPos = context.GetLocalVariableReferencePosition(_varIndex);
            return $"{refPos}, {_value}, {_label}";
        }

        public override void SetNextInstructions(CompilerMethodContext context, ILOperation operation, ILOperation nextOperation)
        {
            operation.NextInstructions.Add(nextOperation);
            var jmpInstruction = context.Lines.FirstOrDefault(l => l.Label == _label);
            operation.NextInstructions.Add(jmpInstruction);
        }
    }

    class OpBranchIfVarLess : OpBase
    {
        private int _varIndex;
        private int _value;
        private string _label;

        public OpBranchIfVarLess(int varIndex, int value, string label) : base(0, "#branch_if_var_less")
        {
            _varIndex = varIndex;
            _value = value;
            _label = label;
        }

        public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
        {
            var refPos = context.GetLocalVariableReferencePosition(_varIndex);
            return $"{refPos}, {_value}, {_label}";
        }

        public override void SetNextInstructions(CompilerMethodContext context, ILOperation operation,
            ILOperation nextOperation)
        {
            operation.NextInstructions.Add(nextOperation);
            var jmpInstruction = context.Lines.FirstOrDefault(l => l.Label == _label);
            operation.NextInstructions.Add(jmpInstruction);
        }
    }
}
