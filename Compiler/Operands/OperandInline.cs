using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Compiler;

namespace Compiler.Ops;

// The three "seam" operations ILMethodInliningPass splices in at a call site
// it decides to inline, in place of the original Call and every Ret inside
// the callee's own decoded body -- see that pass's own comment for the full
// splicing mechanics. None of these ever carry a SourceMethod (they have no
// VarIndex of their own to disambiguate): everything they need is computed
// once, at splice time, from a throwaway CompilerMethodContext built against
// the callee, exactly the same way the normal top-level prologue
// (ILMethodEmitPass) and OpRet.ConvertParameter compute theirs today.

// Replaces the original Call. Reserves the callee's own locals/parameter
// space on localsStack (no hardware-stack return address involved -- there
// was no jsr) and pulls its arguments off the evaluation stack, exactly the
// way a real call's prologue would, just without the return-address step.
class OpInlinePrologue : OpBase
{
    private readonly int _localsSize;
    private readonly List<string> _refList;
    private readonly List<string> _localRefList;
    private readonly MethodBase _callee;

    public OpInlinePrologue(int localsSize, List<string> refList, List<string> localRefList, MethodBase callee)
        : base(0, "#init_locals_pull_parameters_inline")
    {
        _localsSize = localsSize;
        _refList = refList;
        _localRefList = localRefList;
        _callee = callee;
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation) =>
        $"{_localsSize}, [{string.Join(",", _refList)}], [{string.Join(",", _localRefList)}]";

    // Mirrors OpCall.SetStackContent's own argument removal (never its
    // return-type Add -- the callee's own body pushes that, via whatever
    // expression precedes each Ret, exactly as it would if this method
    // weren't being inlined at all): this operation replaces the original
    // Call, so it must leave the compile-time StackContent type list in the
    // same state OpCall.SetStackContent would have, for everything spliced
    // in after it (the callee's own first real instruction) to see the
    // correct starting shape.
    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        var parameterNum = _callee.GetParameters().Length + (_callee.IsStatic ? 0 : 1);
        operation.StackContent.RemoveLast(parameterNum);
    }
}

// Replaces one of the callee's own Ret operations. Decrements GC refcounts
// for the callee's reference-typed locals/params and deallocates its
// locals-stack space, then either jmp's to the shared continuation label for
// this inline site (every Ret except a trailing one) or falls straight
// through into the OpInlineContinue marker that immediately follows
// (continueLabel == null -- only when this Ret is the callee's own last
// operation, so nothing real sits between it and that marker).
class OpInlineEpilogue : OpBase
{
    private readonly int _stackSize;
    private readonly List<string> _refList;
    private readonly string _continueLabel;

    public OpInlineEpilogue(int stackSize, List<string> refList, string continueLabel)
        : base(0, continueLabel == null ? "#method_exit_inline_fallthrough" : "#method_exit_inline")
    {
        _stackSize = stackSize;
        _refList = refList;
        _continueLabel = continueLabel;
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation) =>
        _continueLabel == null
            ? $"{_stackSize}, [{string.Join(",", _refList)}]"
            : $"{_stackSize}, [{string.Join(",", _refList)}], {_continueLabel}";

    // Mirrors OpRet's own override (empty -- control never falls through a
    // real return either) when this epilogue ends in a jmp: the textually
    // next line in the merged Lines list (another of the callee's own
    // instructions, if this wasn't its last Ret, or the continuation
    // marker) is NOT where control goes next -- the jmp target (found by
    // label, same technique OpBranchIfNotEqual/OpBranchIfVarLess already use
    // for their own jump target) is. When there IS no jmp (the fallthrough
    // case), control genuinely does fall through to the textually next
    // line, so the inherited default (OpBase's -- link to nextOperation) is
    // correct and this override doesn't run at all.
    public override void SetNextInstructions(CompilerMethodContext context, ILOperation operation, ILOperation nextOperation)
    {
        if (_continueLabel == null)
        {
            base.SetNextInstructions(context, operation, nextOperation);
            return;
        }

        var target = context.Lines.FirstOrDefault(l => l.Label == _continueLabel);
        operation.NextInstructions.Add(target);
    }
}

// Used only by ILMethodInliningPass's trivial-argument-substitution splice
// (TryBuildTrivialSubstitution): since no locals-stack space was ever
// reserved for that callee -- every parameter reads directly from the
// caller's own existing slot instead of a freshly materialized copy -- an
// early (non-trailing) Ret needs nothing beyond a plain jump to the shared
// continuation label. No refcount decrements (nothing new was retained to
// release) and no stackPointer rewind (nothing was reserved to give back),
// unlike OpInlineEpilogue's jmp variant, which still owns both of those.
class OpInlineJumpOnly : OpBase
{
    private readonly string _continueLabel;

    public OpInlineJumpOnly(string continueLabel) : base(0)
    {
        _continueLabel = continueLabel;
    }

    public override string Emit(CompilerMethodContext context, ILOperation operation) => $"jmp {_continueLabel}";

    // Same reason as OpInlineEpilogue's own override: textually the next
    // line isn't where control goes after a jmp.
    public override void SetNextInstructions(CompilerMethodContext context, ILOperation operation, ILOperation nextOperation)
    {
        var target = context.Lines.FirstOrDefault(l => l.Label == _continueLabel);
        operation.NextInstructions.Add(target);
    }
}

// Pure label marker dropped right after an inlined splice's last
// instruction -- a dedicated position for OpInlineEpilogue's jmp target
// instead of relying on whatever caller instruction happens to follow,
// which could independently also be a real caller branch target (see
// ILMethodInliningPass's own comment for why that collision matters).
// Emits nothing at all; its only purpose is to hold a renumbered Position
// and a preset Label for ILMethodNextInstructionPass/ILAddressFromLabelPass
// to find.
class OpInlineContinue : OpBase
{
    public OpInlineContinue() : base(0)
    {
    }

    public override string Emit(CompilerMethodContext context, ILOperation operation) => "";
}

// Fuses a trivially-substituted Ldarg/Ldloc immediately followed by Ldfld
// into one #pushfld8_at/#pushfld16_at/#pushfldflt_at macro call (asm/
// helper/optimized.asm), the same way ILPropertyGettterOptimizer already
// fuses Ldarg_0+Ldfld into #pushfld8/#pushfld16 for a NON-inlined method's
// own `this.field` access -- just generalized to an arbitrary compile-time
// relPos instead of always assuming 1 (`this`). Built directly by
// ILMethodInliningPass.TryBuildTrivialSubstitution, at splice-construction
// time, deliberately NOT as a PeepholeRule: that pass's own blanket
// SourceMethod guard (see PeepholeRule.TryApply's comment) exists because
// a rule running later, over already-spliced lines, can't always tell
// which method's ConvertParameter/VarIndex a window of operations
// resolves against -- a non-issue here, since this fusion is decided with
// full knowledge of both the caller's substituted position and the
// callee's own field access, before either one is spliced into anything.
class OpPushFldAt : OpBase, IPushFldOperation
{
    private readonly string _relPos;
    private readonly string _pos;
    private readonly Type _fieldType;

    public string RelPos => _relPos;
    public string Pos => _pos;
    bool IPushFldOperation.IsFloat => _fieldType == typeof(float);
    bool IPushFldOperation.Is16BitField => _fieldType.GetStorageBytes() == 2;

    public OpPushFldAt(string relPos, string pos, Type fieldType) : base(0, "#pushfld")
    {
        _relPos = relPos;
        _pos = pos;
        _fieldType = fieldType;
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation) =>
        $"{_relPos}, {_pos}";

    protected override string SizeSuffix(CompilerMethodContext context, ILOperation operation)
    {
        if (_fieldType == typeof(float))
            return "flt_at";
        return _fieldType.GetStorageBytes() == 2 ? "16_at" : "8_at";
    }

    // Represents BOTH the substituted Ldarg/Ldloc (push the object
    // reference) and the Ldfld that immediately consumed it (pop 1, push
    // the field's own type) -- net effect on the caller's abstract stack
    // model is a single net push of the field's type, same as the two
    // unfused operations together would have produced.
    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        operation.StackContent.Add(_fieldType);
    }
}
