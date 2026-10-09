using System;
using System.Collections.Generic;
using Compiler.Ops;

namespace Compiler;

// A single "match a fixed-length window of IL operations, replace it with one
// synthesized operation" rule. See PeepholeOptimizerPass for how these run.
class PeepholeRule
{
    private readonly Func<ILOperation, bool>[] _matchers;
    private readonly Func<CompilerMethodContext, ILOperation[], OpBase> _build;
    private Func<CompilerMethodContext, ILOperation[], object> _rawParameter;
    private Func<ILOperation[], List<Type>> _stackContent;
    private Func<CompilerMethodContext, ILOperation[], bool> _guard;
    private bool _allowInlinedWindows;

    public PeepholeRule(
        Func<CompilerMethodContext, ILOperation[], OpBase> build,
        params Func<ILOperation, bool>[] matchers)
    {
        _build = build;
        _matchers = matchers;
    }

    // Overrides the default RawParameter (Operation.ConvertParameter(context, null))
    // for rules that build it from the matched window instead (e.g. forwarding a
    // constant's already-converted value, or hand-formatting a multi-part parameter).
    public PeepholeRule WithRawParameter(Func<CompilerMethodContext, ILOperation[], object> f)
    {
        _rawParameter = f;
        return this;
    }

    // Overrides the default StackContent (the last matched line's StackContent)
    // for rules where that's not correct.
    public PeepholeRule WithStackContent(Func<ILOperation[], List<Type>> f)
    {
        _stackContent = f;
        return this;
    }

    // Extra check run after the per-line matchers all pass, with the full
    // matched window and context available -- for conditions a single-line
    // predicate can't express (e.g. "only if this field is 8-bit", where the
    // replacement macro has no 16-bit variant). Declining here means the
    // window is left untouched for a later, more general pass to handle.
    public PeepholeRule WithGuard(Func<CompilerMethodContext, ILOperation[], bool> f)
    {
        _guard = f;
        return this;
    }

    // Opts this rule out of the inlined-window guard below. Safe ONLY for a
    // rule whose built operation (and whose RawParameter, however it's
    // computed) never re-derives anything from a matched line's raw
    // VarIndex/argument-index against context.Method -- i.e. every matched
    // line's own already-resolved RawParameter/Pos/RelPos (or, like
    // OpFusedWiden8To16, the original ILOperation itself) is reused as-is.
    // That value was correctly computed against the CALLEE's own context
    // when ILMethodInliningPass's callee-isolated pre-pass first produced
    // it, and localsStack addressing is relative to the runtime stackPointer
    // either way, so reusing it verbatim remains correct however far the
    // matched window reaches into (or across) an inlined splice. A rule
    // that instead captures a bare VarIndex/argument-index (OpLdloc.VarIndex
    // etc.) for its OWN synthesized operand to resolve later -- e.g.
    // ILMethodIncOptimizer, ILMethodBranchIfLessOptimizer -- must NOT call
    // this: that resolution happens at emit time against context.Method,
    // which for an inlined window is the CALLER, not the callee the raw
    // index actually belongs to (ILMethodInliningPass's own "VarIndex
    // hazard" comment). Audited individually, not guessed, for every rule
    // this is currently applied to -- see each one's own call site.
    public PeepholeRule AllowInlinedWindows()
    {
        _allowInlinedWindows = true;
        return this;
    }

    // Tries to match this rule starting at lines[i]. On success, inserts the
    // synthesized replacement operation right after the matched window and
    // marks every matched line Optimized. Returns whether it matched.
    public bool TryApply(CompilerMethodContext context, List<ILOperation> lines, int i)
    {
        if (i + _matchers.Length > lines.Count)
            return false;

        for (int k = 0; k < _matchers.Length; k++)
            if (!_matchers[k](lines[i + k]))
                return false;

        var window = new ILOperation[_matchers.Length];
        for (int k = 0; k < _matchers.Length; k++)
            window[k] = lines[i + k];

        // Never synthesize a new operation out of a window that touches an
        // inlined callee's own instructions (ILMethodInliningPass marks
        // those via SourceMethod): whatever gets built here would resolve
        // ConvertParameter/VarIndex against context.Method (the CALLER), not
        // the callee the window's instructions actually belong to -- see
        // ILMethodInliningPass's "VarIndex hazard" comment. Centralizing the
        // check here, once, protects every PeepholeOptimizerPass subclass
        // without each one needing its own guard -- except a rule that opts
        // out via AllowInlinedWindows() above, having been individually
        // audited as not needing it.
        if (!_allowInlinedWindows)
            for (int k = 0; k < window.Length; k++)
                if (window[k].SourceMethod != null)
                    return false;

        if (_guard != null && !_guard(context, window))
            return false;

        var newOperation = new ILOperation
        {
            Operation = _build(context, window),
        };
        newOperation.RawParameter = _rawParameter != null
            ? _rawParameter(context, window)
            : newOperation.Operation.ConvertParameter(context, null);
        newOperation.StackContent = _stackContent != null
            ? _stackContent(window)
            : window[^1].StackContent;

        lines.Insert(i + _matchers.Length, newOperation);
        foreach (var l in window)
            l.Optimized = true;

        return true;
    }
}
