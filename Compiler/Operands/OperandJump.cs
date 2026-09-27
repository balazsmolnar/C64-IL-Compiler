using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Ops;

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

    // Conditional (Brtrue/Brfalse -- a bare `if (x)`/`if (!x)` truthiness
    // test) needs this too, not just Compare: string is the one reference
    // type wider than 1 byte (TypeExtensions.GetStorageBytes), and Roslyn
    // compiles a reference null-check used as a plain `if` condition
    // straight to `ldloc s; brtrue.s`/`brfalse.s` -- no Ceq, so no OTHER
    // instruction ever gets a chance to notice the width -- confirmed by
    // dumping the actual IL for `if (s == null)`. Pulling only 1 byte for a
    // 2-byte value (the untouched default before this) both desyncs the
    // evaluation stack by 1 byte AND tests only the low byte's zero-ness,
    // silently misjudging a non-null pointer whose low byte happens to be
    // 0 (e.g. any page-aligned address) as null. A bool or an ordinary
    // 1-byte object-table handle -- everything else Brtrue/Brfalse was
    // ever used on -- is unaffected (GetStorageBytes()==1 either way).
    // UnConditional (Br/Br_s) is excluded: it consumes no stack value at
    // all, so there's nothing to check the width of, and its predecessor's
    // StackContent could legitimately be empty (e.g. a Br bypassing an
    // else-branch, jumped to right after the stack was already emptied by
    // a Ret/Stloc) -- Is16Bit would either throw or read meaningless data.
    public override bool Is16BitSupported => _jumpType != JumpType.UnConditional;

    // Guarded (not a bare PreviousInstructions[0] access): an operation an
    // optimizer SYNTHESIZED and inserted -- fusing a separate compare+branch
    // into one branch-on-comparison op, e.g. ILMethodBranchIfLessOptimizer,
    // is exactly this shape -- never went through ILMethodNextInstructionPass
    // (which only runs once, before the optimizers), so it can have an
    // empty PreviousInstructions list; confirmed live (ArgumentOutOfRangeException,
    // an otherwise-passing ternary-with-a-comparison test) once Conditional
    // started asking this question too (Compare already did, apparently
    // without ever hitting a synthesized/predecessor-less case in practice).
    // Falls back to the 1-byte width, exactly this method's behavior before
    // Is16BitSupported covered Conditional at all -- correct for everything
    // except a synthesized branch testing a 2-byte value's truthiness
    // directly, which nothing in this compiler currently synthesizes (the
    // optimizers that build these fuse a COMPARE, i.e. Compare-type, not a
    // bare Brtrue/Brfalse on an already-2-byte value).
    public override bool Is16Bit(CompilerMethodContext context, ILOperation operation)
    {
        if (operation.PreviousInstructions.Count != 1)
            return false;
        return operation.PreviousInstructions[0].StackContent.Last().GetStorageBytes() == 2;
    }

    // Roslyn can fuse a simple "if (a < b)" straight into Blt/Beq/etc.
    // (skipping a separate Clt/Ceq + Brtrue) for float exactly like it
    // already does for int -- these macros need their own float-aware
    // compare-and-branch (asm/helper/float.asm), not the 8/16-bit family.
    protected override string SizeSuffix(CompilerMethodContext context, ILOperation operation)
    {
        if (_jumpType == JumpType.Compare && operation.PreviousInstructions[0].StackContent.Last() == typeof(float))
            return "flt";
        return base.SizeSuffix(context, operation);
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

    // Uses JumpType.Conditional only to inherit OpJump's RemoveLast(1)
    // SetStackContent -- this isn't a bare truthiness test on a raw pushed
    // value (Brtrue/Brfalse) the way that base class's Is16Bit override now
    // assumes, and its Command is always a full, already-specific,
    // unsuffixed macro name built by ILMethodBranchConstOptimizer (e.g.
    // "#branch_greater_unsigned_const") with no "8"/"16"-suffixed variant
    // defined at all -- letting SizeSuffix append one produced "not defined
    // symbol 'branch_greater_unsigned_const8'" (confirmed live: broke an
    // otherwise-passing comparison-in-a-ternary test the moment Conditional
    // jumps generally became width-aware).
    public override bool Is16BitSupported => false;
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