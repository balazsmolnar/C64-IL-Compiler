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
