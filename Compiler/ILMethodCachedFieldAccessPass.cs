using Compiler.Ops;

namespace Compiler;

// Collapses a chain of consecutive IPushFldOperation reads (OpPushFld --
// ILPropertyGettterOptimizer's fused `this.field`; OpPushFldAt --
// ILMethodInliningPass's trivial-argument-substitution fusion) that all
// resolve the SAME object's pointer back to back: only the first one
// actually needs to call resolveObjPtr. Every subsequent one in the chain
// becomes an OpPushFldCached instead, skipping straight to the field read
// and reusing the already-populated tmpPointer -- see that class's own
// comment and asm/helper/optimized.asm's pushfld*_cached macros for why
// this is safe (this GC only ever moves an object via an explicit
// GC.Collect() call, never implicitly on allocation).
//
// Found while investigating a real question (how often does
// resolveObjPtr actually get called): 156 of Hunchback's own 494 call
// sites were a provably-redundant re-resolve of an object within a few
// lines of the one right before it -- "tmp = player.X + player.Y"-shaped
// code (and the equivalent, much older and more common "this.a + this.b"
// shape ILPropertyGettterOptimizer already fuses two separate ways) was
// paying for the full resolve twice in a row for nothing.
//
// Deliberately NOT a PeepholeRule: OpPushFldAt instances always carry
// SourceMethod != null (see ILMethodInliningPass's own comment), which
// PeepholeRule.TryApply's blanket guard rejects outright -- this pass
// only ever reads each operation's own already-computed RelPos/Pos
// (never re-deriving anything via ConvertParameter against the wrong
// method), so that hazard doesn't apply here the way it would to a rule
// that tried to synthesize something new from scratch.
//
// Scoped narrow on purpose for a first cut: only DIRECTLY adjacent reads
// (nothing real between them) are collapsed. Two DIFFERENT reasons an
// operation can be "not real", needing two different safety checks:
//
// 1. A PeepholeRule match (ILPropertyGettterOptimizer's own #pushfld
//    fusion, among others): TryApply never removes the matched window's
//    original lines, just marks them Optimized and inserts the new
//    operation right after them (see that method's own body) -- so a
//    chain here may run through several Optimized "zombie" lines that
//    don't really execute, AND the freshly-inserted operation that
//    replaces them never gets a PreviousInstructions entry at all
//    (ILMethodNextInstructionPass, which builds that graph, already ran
//    before this pass's optimizer group even starts). Both are handled
//    by skipping Optimized lines outright (continuing past them, as if
//    removed) and trusting that a brand-new operation with no Label can
//    never be a branch target regardless of what its missing
//    PreviousInstructions might suggest.
// 2. ILMethodInliningPass's own splice markers (OpInlineContinue,
//    OpInlineJumpOnly): built EARLY, before ILMethodNextInstructionPass
//    runs, so their PreviousInstructions IS reliable -- but their Label
//    is not a useful signal, since a splice's continuation marker always
//    carries one (for a possible early-return jmp to land on) even when
//    the callee has no early return at all and nothing really targets it
//    (confirmed by shipping an earlier version that checked Label here
//    and found zero real pairs anywhere, including the exact
//    "player.X + player.Y" case this pass exists for). The graph check
//    instead asks the real question directly: is this marker's only
//    actual predecessor the line immediately before it. That's true for
//    an ordinary trailing-Ret-only callee (nothing ever jumps here) and
//    false the moment a real early return also lands here (a genuine
//    multi-path merge this linear scan can't safely reason about).
//
// Any other operation -- not an IPushFldOperation, not Optimized, not an
// inline marker -- resets the chase unconditionally, conservatively, the
// same way: even operations that don't themselves touch tmpPointer or
// the heap are treated as unsafe to see through, rather than auditing
// each opcode individually the way the original #pushfld/#pushfld_at
// fusions did for their own, narrower patterns. A non-null Label on one
// of THESE (an ordinary branch merge elsewhere in the method, not a
// splice marker) also resets it, for the same multi-path reason as #2.
class ILMethodCachedFieldAccessPass : ICompilerMethodPass
{
    public void Execute(CompilerMethodContext context)
    {
        if (context.Lines == null)
            return;

        string cachedRelPos = null;
        ILOperation previousLine = null;

        foreach (var line in context.Lines)
        {
            if (line.Optimized)
            {
                if (line.Label != null)
                    cachedRelPos = null;
                continue;
            }

            bool isInlineMarker = line.Operation is OpInlineContinue || line.Operation is OpInlineJumpOnly;

            if (isInlineMarker)
            {
                bool simpleFallthrough = previousLine != null
                    && line.PreviousInstructions.Count == 1
                    && line.PreviousInstructions[0] == previousLine;
                if (!simpleFallthrough)
                    cachedRelPos = null;
                // Transparent otherwise -- doesn't touch tmpPointer, so
                // whatever is cached (or isn't) passes through unchanged.
            }
            else
            {
                if (line.Label != null)
                    cachedRelPos = null;

                if (line.Operation is IPushFldOperation pf)
                {
                    if (cachedRelPos == pf.RelPos)
                    {
                        var cached = new OpPushFldCached(pf.RelPos, pf.Pos, pf.Is16BitField, pf.IsFloat);
                        line.Operation = cached;
                        line.RawParameter = cached.ConvertParameter(context, line);
                    }

                    cachedRelPos = pf.RelPos;
                }
                else
                {
                    cachedRelPos = null;
                }
            }

            previousLine = line;
        }
    }
}
