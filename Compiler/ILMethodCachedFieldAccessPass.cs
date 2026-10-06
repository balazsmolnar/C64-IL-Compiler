using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

// Caches a resolved object pointer across a WHOLE method body, not just
// between two physically-adjacent reads: a real forward dataflow over the
// method's own control-flow graph, tracking which object (by its
// compile-time-constant relpos) is currently resolved into the shared
// tmpPointer, correctly across branches and loop back-edges.
//
// Supersedes an earlier, narrower version of this pass that only ever
// collapsed STRICTLY adjacent same-object reads -- found, while measuring
// how often resolveObjPtr is actually called in Hunchback, that the
// biggest remaining opportunity (e.g. Player.SetFrame/Player.Move/
// Rope.SetFrame reading several of `this`'s own fields across arithmetic,
// branches, and short loops) was invisible to that narrower pass: nothing
// about an Add, a Br, or a loop back-edge actually touches tmpPointer, so
// treating them as "unsafe to see through" the way any OTHER operation
// was previously treated was needlessly conservative.
//
// Safety rests on two facts, not one:
// 1. This GC only ever moves an object via an explicit GC.Collect() call,
//    never implicitly on allocation (Runtime_CheckHeapRoom hard-faults on
//    insufficient room instead of compacting) -- so the HANDLE a resolved
//    pointer corresponds to never goes stale on its own.
// 2. tmpPointer is a single, shared zero-page slot: ANY other call to
//    resolveObjPtr -- for a different object, or for an object reached
//    generically (an unfused Ldfld/Stfld/array element access, where the
//    actual object isn't known at compile time) -- overwrites it,
//    regardless of whether anything moved. A real Call/Callvirt is the
//    same risk one level removed: the callee's OWN code might resolve
//    something else into tmpPointer before returning -- and so is float
//    arithmetic (Float_Add/Sub/Mul/Div/Compare route through a real
//    `jsr` into the BASIC ROM, see IsKnownSafeTransparent's own comment
//    for the real regression this caused and how it was found/fixed).
//    Measured directly
//    against Rope.SetFrame: its 68 `this`-field reads looked like a huge
//    opportunity by count alone, but nearly every one is immediately
//    followed by a real call (feeding a Sprite's hardware registers from
//    `this`'s own fields) that resolves the SPRITE object internally --
//    so crossing a call was deliberately never pursued here; a call (or a
//    generic field/array op) unconditionally invalidates the cache, same
//    as before, just now decided per-point via the real CFG instead of a
//    linear scan. The REAL, re-measured opportunity (counting only
//    call-free runs) is smaller than the raw count but still real --
//    e.g. Player.SetFrame/Player.Move each had several runs of 2-8
//    consecutive `this`-reads with nothing but arithmetic/branches
//    between them.
//
// Deliberately NOT a PeepholeRule, for the same reason as before:
// OpPushFldAt instances carry SourceMethod != null, which
// PeepholeRule.TryApply's blanket guard rejects outright.
//
// Tracks ANY relpos uniformly (via IObjectRootOperation), not just
// `this` -- there's no need to pick "the most-used object" ahead of
// time or special-case `this`: whichever object was most recently
// resolved for real is exactly what's available in tmpPointer, and the
// dataflow discovers every safe reuse of it on its own. A read
// (IPushFldOperation: OpPushFld, OpPushFldAt, and this pass's own
// OpPushFldCached) is the only one rewritten -- OpSetfld/OpIncfld
// (writes) are tracked as GEN too (they resolve and refresh tmpPointer
// exactly the same way) since that's free, but aren't themselves
// rewritten to skip their own resolve: writes are a small fraction of
// the real field traffic here (measured: Player.Move's own 33 `this`
// touches were 30 reads, 3 increments), not worth the extra macro
// variants yet.
//
// Mechanics: two different operation-insertion times need two different
// predecessor-finding rules, same underlying issue as the narrower pass
// this replaces had to work around for its own two marker types:
// 1. Lines built EARLY (ILMethodCodePass.Decode's own output,
//    ILMethodInliningPass's splices) have real PreviousInstructions,
//    populated once by ILMethodNextInstructionPass before this
//    (fixpoint) pass group even starts -- trusted directly.
// 2. Lines built LATE, by a PeepholeRule in this SAME fixpoint group
//    (OpPushFld/OpSetfld/OpIncfld's own optimizers) -- PreviousInstructions
//    is always empty for these (the graph was already built before they
//    existed), AND PeepholeRule.TryApply never removes the matched
//    window it fused, just marks it Optimized and inserts the new
//    operation right after -- so an empty-graph line's one true
//    predecessor is simply whatever effective (non-Optimized) line
//    comes right before it in list order, found by walking through any
//    Optimized "zombie" lines in between.
class ILMethodCachedFieldAccessPass : ICompilerMethodPass
{
    public void Execute(CompilerMethodContext context)
    {
        if (context.Lines == null)
            return;

        var effectiveLines = context.Lines.Where(l => !l.Optimized).ToList();
        if (effectiveLines.Count == 0)
            return;

        var effectiveOf = BuildEffectiveLineMap(context.Lines);
        var predecessors = BuildPredecessors(effectiveLines, effectiveOf);

        var stateOut = new Dictionary<ILOperation, string>();
        foreach (var line in effectiveLines)
            stateOut[line] = null;

        bool changed = true;
        for (int iteration = 0; iteration < effectiveLines.Count + 1 && changed; iteration++)
        {
            changed = false;
            foreach (var line in effectiveLines)
            {
                string stateIn = Meet(predecessors[line].Select(p => stateOut[p]));
                string newStateOut = Transfer(stateIn, line);
                if (newStateOut != stateOut[line])
                {
                    stateOut[line] = newStateOut;
                    changed = true;
                }
            }
        }

        foreach (var line in effectiveLines)
        {
            string stateIn = Meet(predecessors[line].Select(p => stateOut[p]));
            if (line.Operation is IPushFldOperation pf && stateIn == pf.RelPos && !(line.Operation is OpPushFldCached))
            {
                var cached = new OpPushFldCached(pf.RelPos, pf.Pos, pf.Is16BitField, pf.IsFloat);
                line.Operation = cached;
                line.RawParameter = cached.ConvertParameter(context, line);
            }
        }
    }

    // GEN: an IObjectRootOperation (read or write) sets the state to
    // whichever relpos it just resolved -- unconditionally, regardless of
    // stateIn, since it performed a real resolveObjPtr call either way
    // (or, for OpPushFldCached specifically, is already known-correct for
    // that same relpos from an earlier point in this same chain).
    // TRANSPARENT only for a small, explicitly-verified-jsr-free allowlist
    // (see IsKnownSafeTransparent) -- KILL for everything else, by
    // default. This default was originally "transparent unless it's a
    // field/array op or a real Call/Callvirt", which seemed sound (those
    // are the only things that call resolveObjPtr) but turned out NOT to
    // be: float arithmetic/comparisons route through the BASIC ROM
    // (Float_Add/Sub/Mul/Div/Compare, asm/helper/float.asm and branch.asm)
    // via a real `jsr`, and Newobj's constructor call
    // (asm/helper/heap.asm's `jsr \ctor`) can touch arbitrary heap state
    // -- either one corrupts tmpPointer exactly like a real Call does,
    // just without ever being an IL-level Call/Callvirt. Found via a REAL
    // test regression (InstanceFloatFieldBugTest) the first time this
    // pass shipped with the broader default: `this.a_ + this.b_` cached
    // correctly, but `x * this.a_` right after it did not, because the
    // Mul in between was wrongly treated as transparent. Confirmed via
    // a targeted experiment (temporarily killing on Add/Sub/Mul/Div)
    // that this was the exact cause, then verified every macro in the
    // allowlist below is genuinely jsr-free by grepping
    // asm/helper/*.asm directly rather than assuming -- integer
    // arithmetic (add8/16, sub8/16) and integer division (div8/16, which
    // DOES jsr, but only into Divide8Core/NegateParam*, confirmed pure
    // 6502 with no further jsr and no zero-page overlap with tmpPointer)
    // were deliberately left OFF this allowlist anyway, in favor of a
    // short, easy-to-re-verify list over a longer one that needs re-
    // auditing every time asm/helper's internals change.
    private static string Transfer(string stateIn, ILOperation line)
    {
        if (line.Operation is IObjectRootOperation root)
            return root.RelPos;
        if (IsKnownSafeTransparent(line))
            return stateIn;
        return null;
    }

    // Explicitly verified jsr-free (or, for the splice markers, verified
    // to emit nothing at all) by reading the actual macro bodies in
    // asm/helper/*.asm -- see Transfer's own comment. Deliberately an
    // allowlist, not a denylist: missing a safe case here only costs a
    // missed optimization, which is the direction to err in.
    //
    // Genuine IL Nop is deliberately NOT in this list, even though it
    // sounds trivially safe: every PeepholeRule-synthesized operation in
    // this compiler (ILMethodIncOptimizer, ILMethodCompareConstOptimizer,
    // ILMethodMulConstOptimizer, ILMethodWiden8To16Optimizer, and others)
    // tags its own replacement line's OpCode as Nop too (see the "; Nop"
    // comment on every one of them in generated asm) -- so "OpCode ==
    // Nop" is actually "this is SOME fused operation, of any kind",
    // not "this is a real no-op". Treating that as a blanket allow would
    // have hidden this exact class of bug just as easily as treating all
    // arithmetic as safe did: each fused kind needs its own macro
    // checked, not a shortcut through the marker they all happen to
    // share. Only the specific fused kinds verified below (the var
    // inc/dec/init/copy family -- confirmed pure zero-page reads/writes,
    // no jsr at all) are allowed through; anything else fused defaults
    // to the same conservative kill as an unrecognized real opcode would.
    private static bool IsKnownSafeTransparent(ILOperation line)
    {
        if (line.Operation is OpInlineContinue || line.Operation is OpInlineJumpOnly)
            return true;

        // ILMethodIncOptimizer/DecOptimizer/SetVariableOptimizer's own
        // fused forms (asm/helper/optimized.asm's inc_var/dec_var/
        // init_var/copy_var and their _zp twins) -- every one of them is
        // a direct zero-page or locals-stack read/increment/write, no
        // jsr anywhere in the macro body.
        if (line.Operation is OpIncVar || line.Operation is OpIncVarZp
            || line.Operation is OpDecVar || line.Operation is OpDecVarZp
            || line.Operation is OpIncOrDecVarExpr || line.Operation is OpPostIncOrDecVarLeaveOnStack
            || line.Operation is OpInitVar || line.Operation is OpInitVarZp
            || line.Operation is OpCopyVar)
            return true;

        switch (line.OpCode)
        {
            case ILOpCode.Br:
            case ILOpCode.Br_s:
            case ILOpCode.Dup:
            case ILOpCode.Ret:
            case ILOpCode.Ldc_i4:
            case ILOpCode.Ldc_i4_s:
            case ILOpCode.Ldc_i4_0:
            case ILOpCode.Ldc_i4_1:
            case ILOpCode.Ldc_i4_2:
            case ILOpCode.Ldc_i4_3:
            case ILOpCode.Ldc_i4_4:
            case ILOpCode.Ldc_i4_5:
            case ILOpCode.Ldc_i4_6:
            case ILOpCode.Ldc_i4_7:
            case ILOpCode.Ldc_i4_8:
            case ILOpCode.Ldc_i4_m1:
            case ILOpCode.Ldc_i8:
            case ILOpCode.Ldc_r4:
            case ILOpCode.Ldc_r8:
            case ILOpCode.Ldloc:
            case ILOpCode.Ldloc_s:
            case ILOpCode.Ldloc_0:
            case ILOpCode.Ldloc_1:
            case ILOpCode.Ldloc_2:
            case ILOpCode.Ldloc_3:
            case ILOpCode.Stloc:
            case ILOpCode.Stloc_s:
            case ILOpCode.Stloc_0:
            case ILOpCode.Stloc_1:
            case ILOpCode.Stloc_2:
            case ILOpCode.Stloc_3:
            case ILOpCode.Ldarg:
            case ILOpCode.Ldarg_s:
            case ILOpCode.Ldarg_0:
            case ILOpCode.Ldarg_1:
            case ILOpCode.Ldarg_2:
            case ILOpCode.Ldarg_3:
            case ILOpCode.Starg:
            case ILOpCode.Starg_s:
            // Static field access (asm/helper/stack.asm's
            // stack_pull_int_ref/stack_push_int_ref and friends) never
            // calls resolveObjPtr -- a static has a fixed address, not a
            // heap handle. Its ref-counted path (#deref) only decrements
            // a count in objTableRootCount; this GC's own rule (only an
            // explicit GC.Collect() call ever sweeps/compacts) means that
            // alone can't touch tmpPointer either.
            case ILOpCode.Ldsfld:
            case ILOpCode.Stsfld:
                return true;
            // Add/Sub/Mul/Div: safe for integers (add8/16, sub8/16, mul8/16
            // have no jsr at all; div8/16's jsr targets -- Divide8Core,
            // NegateParam*If*Negative -- are pure 6502 with no further jsr
            // and no zero-page overlap with tmpPointer, verified directly).
            // NOT safe for float: Float_Add/Sub/Mul/Div
            // (asm/helper/float.asm) run through the BASIC ROM banking
            // window -- this is the real bug this allowlist exists to
            // never repeat, see Transfer's own comment.
            case ILOpCode.Add:
            case ILOpCode.Sub:
            case ILOpCode.Mul:
            case ILOpCode.Div:
                return !IsFloatResult(line);
            default:
                return false;
        }
    }

    private static bool IsFloatResult(ILOperation line) =>
        line.StackContent != null && line.StackContent.Count > 0 && line.StackContent.Last() == typeof(float);

    // "Must" meet: known and agreeing on every incoming path -> that
    // value; anything else (no predecessors, a disagreement, or any
    // predecessor itself unknown) -> null. A method's own entry point
    // (no predecessors at all) correctly lands on null via the same rule.
    private static string Meet(IEnumerable<string> states)
    {
        string result = null;
        bool first = true;
        foreach (var state in states)
        {
            if (first)
            {
                result = state;
                first = false;
            }
            else if (result != state)
            {
                return null;
            }
        }
        return first ? null : result;
    }

    // Maps every line (including an Optimized "zombie") to the effective,
    // still-live line that represents it going forward: itself, if it's
    // not Optimized; otherwise whatever comes right after it once every
    // Optimized line in that same run is skipped -- see this class's own
    // comment for why that's always exactly the fused replacement
    // PeepholeRule.TryApply inserted for that window.
    private static Dictionary<ILOperation, ILOperation> BuildEffectiveLineMap(List<ILOperation> lines)
    {
        var map = new Dictionary<ILOperation, ILOperation>();
        ILOperation nextEffective = null;
        for (int i = lines.Count - 1; i >= 0; i--)
        {
            var line = lines[i];
            if (!line.Optimized)
                nextEffective = line;
            map[line] = nextEffective;
        }
        return map;
    }

    private static Dictionary<ILOperation, List<ILOperation>> BuildPredecessors(
        List<ILOperation> effectiveLines, Dictionary<ILOperation, ILOperation> effectiveOf)
    {
        var predecessors = new Dictionary<ILOperation, List<ILOperation>>();
        for (int i = 0; i < effectiveLines.Count; i++)
        {
            var line = effectiveLines[i];
            List<ILOperation> preds;
            if (line.PreviousInstructions.Count > 0)
            {
                preds = line.PreviousInstructions
                    .Select(p => effectiveOf.TryGetValue(p, out var eff) ? eff : null)
                    .Where(p => p != null)
                    .Distinct()
                    .ToList();
            }
            else if (i > 0)
            {
                preds = new List<ILOperation> { effectiveLines[i - 1] };
            }
            else
            {
                preds = new List<ILOperation>();
            }
            predecessors[line] = preds;
        }
        return predecessors;
    }
}
