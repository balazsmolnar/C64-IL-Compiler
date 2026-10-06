using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

// Method inlining (see the approved plan -- Phase 1: "single-call-site
// leaf methods"; Phase 2: devirtualized Callvirt, multi-site net-size
// inlining, [MethodImpl(AggressiveInlining)]). Splices a callee's own body
// directly into a call site, replacing the jsr/rts and the stack-machine
// argument marshalling with a lighter inline prologue/epilogue, for every
// `Call` or `Callvirt` line whose resolved target is a key in
// ILCallSiteCountPass's InlineCandidates -- all the real eligibility
// deciding (site count, devirtualizability, byte-cost model, leaf, size
// cap) already happened once, up front, in that pass; see its own comment
// for why it can't just read context.Methods.
//
// Runs as a setup pass, right after ILMethodCodePass and BEFORE
// ILMethodPromoteLocalsPass/ILLibraryUsagePass/ILMethodLabelPass/etc. --
// splicing happens on the RAW, pre-label, pre-stack-content Lines list, so
// every one of those later passes just sees one bigger, well-formed method
// and needs no special-casing of its own (see "the VarIndex hazard" in the
// plan for the one real exception: PeepholeRule.TryApply's centralized
// guard, and the handful of SetStackContent/Is16Bit/SizeSuffix/
// PromotedLocalSlot call sites in Operands/OperandBase.cs that now consult
// operation.SourceMethod).
class ILMethodInliningPass : ICompilerMethodPass
{
    private int _inlineSiteCounter;

    // Result of deciding how to splice one call site: how many of the
    // CALLER's own lines immediately before the Call/Callvirt (its
    // argument-push instructions) get removed along with it -- 0 for the
    // normal materializing splice (they stay live; the inline prologue
    // consumes them off the evaluation stack exactly like a real call's
    // prologue would), or the callee's own parameter count for a trivial-
    // argument substitution (nothing is pushed at all, so those pushes
    // have nothing left to feed).
    private readonly struct SpliceResult
    {
        public readonly int ArgumentLinesRemoved;
        public readonly List<ILOperation> Replacement;
        public SpliceResult(int argumentLinesRemoved, List<ILOperation> replacement)
        {
            ArgumentLinesRemoved = argumentLinesRemoved;
            Replacement = replacement;
        }
    }

    public void Execute(CompilerMethodContext context)
    {
        if (context.Lines == null || context.Method.IsAbstract)
            return;
        // No separate EmitDebugInfo check here: ILCallSiteCountPass itself
        // leaves InlineCandidates empty under EmitDebugInfo (inlining
        // would scramble the 1:1 "one PDB sequence point per C# line"
        // mapping across the splice boundary), so TryBuildSplice already
        // never matches anything in that case -- deliberately a SINGLE
        // source of truth rather than two separate checks that have to be
        // kept in sync (see ILCallSiteCountPass's own comment for the bug
        // that exact duplication caused the first time Phase 1 ran for real).

        for (int i = 0; i < context.Lines.Count; i++)
        {
            var line = context.Lines[i];
            if (line.OpCode != ILOpCode.Call && line.OpCode != ILOpCode.Callvirt)
                continue;

            var splice = TryBuildSplice(context, i);
            if (splice == null)
                continue;

            int removeFrom = i - splice.Value.ArgumentLinesRemoved;
            int removeCount = splice.Value.ArgumentLinesRemoved + 1;

            // The FIRST of the lines about to be removed -- whichever
            // argument-push comes first, or the Call/Callvirt itself when
            // there are no arguments at all -- can be the start of a basic
            // block two different paths converge on, so its own Position
            // can be a real branch target. ILMethodLabelPass/
            // ILAddressFromLabelPass -- both running AFTER this pass --
            // resolve branches by matching a target IL offset against some
            // operation's Position, not by list adjacency, so simply
            // removing every line in this range would leave anything
            // targeting that offset with nowhere to land ("Missing label"
            // from ILMethodLabelPass). Found via two independent real
            // builds: Hunchback's LevelPlay.Play (SetOnRope's own first
            // argument push is such a merge point) and Catacombs'
            // Program.EnterRoom (a zero-argument call -- Item.CollectedCount
            // -- whose own Position is one; an EARLIER version of this fix
            // that inserted the replacement before a blanked-in-place call
            // broke this exact case, since a branch landing directly on
            // that position skipped the whole splice). A dedicated,
            // zero-cost entry marker carrying that one Position replaces
            // the entire removed range instead -- ILMethodLabelPass
            // attaches the label to it like any other operation, and
            // control falls straight through into the real splice content
            // right after it either way (by fallthrough from the
            // preceding line, or by a branch landing here directly).
            var entryMarker = new ILOperation
            {
                OpCode = ILOpCode.Nop,
                Position = context.Lines[removeFrom].Position,
                Operation = new OpInlineContinue(),
            };

            context.Lines.RemoveRange(removeFrom, removeCount);
            context.Lines.Insert(removeFrom, entryMarker);
            context.Lines.InsertRange(removeFrom + 1, splice.Value.Replacement);
            i = removeFrom + splice.Value.Replacement.Count;
        }
    }

    private SpliceResult? TryBuildSplice(CompilerMethodContext context, int callLineIndex)
    {
        var callLine = context.Lines[callLineIndex];
        var target = context.CompilerContext.Assembly.ManifestModule.ResolveMethod((int)callLine.OriginalParameter) as MethodBase;
        if (target == null)
            return null;
        if (!context.CompilerContext.InlineCandidates.TryGetValue(target, out var calleeLines))
            return null;

        var substitution = TryBuildTrivialSubstitution(context, callLineIndex, target, calleeLines);
        if (substitution != null)
            return substitution;

        var calleeContext = new CompilerMethodContext
        {
            CompilerContext = context.CompilerContext,
            Method = target,
            Lines = calleeLines,
        };

        return new SpliceResult(0, BuildSplice(context, calleeContext, calleeLines));
    }

    // Trivial-argument substitution (the proven technique behind every
    // mainstream inliner, just at different IR levels -- Scheifler's 1977
    // CACM paper is the foundational correctness analysis; LLVM gets it
    // for free via SSA value mapping in InlineFunction's ValueMap, which
    // maps a formal parameter straight to the actual argument VALUE with
    // no alloca unless the ABI itself forces one; Go's documented
    // source-level inliner, go.dev/blog/inliner, states the same rule
    // explicitly). When EVERY one of the callee's parameters (including
    // `this`) is fed, at THIS call site, by a bare, side-effect-free read
    // of something already resident at a fixed position in the CALLER's
    // own frame -- Ldarg_N/Ldloc_N, never an Ldfld/arithmetic result/call
    // -- the splice can skip materializing a brand-new locals-stack copy
    // entirely: every reference inside the callee's body is rewritten to
    // read the caller's EXISTING slot directly, and the caller's own
    // argument-push instructions (now redundant -- nothing is left to
    // consume them) are deleted along with the Call.
    //
    // Found while investigating a real reported bug (Hunchback's
    // Player.X getter, get => x_, is exactly this shape: every call site
    // is `this.X`, a bare Ldarg_0) and confirmed the existing materializing
    // splice was wasting ~15 bytes per copy just moving `this` from the
    // caller's own slot into a brand-new one, immediately reading it back
    // out again to use it -- see the approved research for the measured
    // comparison.
    //
    // Soundness of the fixed-offset window below (no StackContent/
    // PreviousInstructions available yet -- this runs before
    // ILMethodBuildEvaluationStackPass): Ldarg*/Ldloc* are all 0-pop,
    // 1-push by the IL spec. If an EARLIER argument's own production took
    // more than one instruction, the window position this method checks
    // for THIS argument would misalign onto that earlier expression's
    // OWN last instruction -- which, to balance the stack at all, must
    // itself pop something, so it can never BE a bare Ldarg/Ldloc. Any
    // misalignment therefore always lands on a non-matching opcode and
    // safely rejects; the only way every window position matches is if
    // every argument really is a single trivial instruction.
    //
    // Deliberately narrow for a first cut: the callee must have no local
    // variables of its own (nothing else would need materializing anyway,
    // keeping this an all-or-nothing decision rather than a partial-offset
    // puzzle), and must never itself write back to the substituted
    // parameter (Starg/Starg_s) -- doing so would alias the caller's own
    // variable, so the write would corrupt it instead of just the
    // callee's own copy. Either condition failing falls back to
    // BuildSplice's existing, already-correct materializing path,
    // unchanged.
    private SpliceResult? TryBuildTrivialSubstitution(CompilerMethodContext context, int callLineIndex, MethodBase target, List<ILOperation> calleeLines)
    {
        if (!CalleeQualifiesForTrivialSubstitution(target, calleeLines))
            return null;

        bool isInstance = !target.IsStatic;
        int paramCount = target.GetParameters().Length + (isInstance ? 1 : 0);
        // paramCount == 0 (a static method taking no arguments at all) is
        // not rejected -- there is nothing to substitute, so this is the
        // most trivial case there is: the loop below runs zero times and
        // the splice skips the prologue/epilogue entirely, same as every
        // other fully-trivial callee.
        if (callLineIndex < paramCount)
            return null;

        var callerRelPos = new int[paramCount];
        for (int p = 0; p < paramCount; p++)
        {
            var producer = context.Lines[callLineIndex - paramCount + p];
            int? relPos = ProducerRelPos(context, producer);
            if (relPos == null)
                return null;
            callerRelPos[p] = relPos.Value;
        }

        int siteId = _inlineSiteCounter++;
        int positionBase = 1_000_000 + 10_000 * siteId;
        string continueLabel = $"{context.Method.GetLabel()}_inline{siteId}_end";
        var result = new List<ILOperation>(calleeLines.Count + 1);

        for (int k = 0; k < calleeLines.Count; k++)
        {
            var original = calleeLines[k];
            int? paramIndex = ArgIndex(original);

            // Fuse a substituted Ldarg/Ldloc immediately followed by Ldfld
            // into the same #pushfld-style macro the standalone (non-
            // inlined) path already gets from ILPropertyGettterOptimizer --
            // see OpPushFldAt's own comment for why this is decided HERE,
            // at splice-build time, rather than as a later peephole rule.
            // Ldfld is the only opcode that needs ruling out noticing a
            // multi-instruction object expression: it pops 1 (an object
            // reference) and pushes 1 (the field), so if it directly
            // follows a 0-pop producer like Ldarg/Ldloc, that producer's
            // own push is necessarily what it consumes -- nothing else
            // could be between them without breaking IL's own stack
            // balance.
            if (paramIndex != null && k + 1 < calleeLines.Count && calleeLines[k + 1].OpCode == ILOpCode.Ldfld)
            {
                var fieldLine = calleeLines[k + 1];
                var field = context.CompilerContext.Assembly.ManifestModule.ResolveField((int)fieldLine.OriginalParameter);
                var fusedOperation = new OpPushFldAt(callerRelPos[paramIndex.Value].ToString(), fieldLine.RawParameter.ToString(), field.FieldType);
                var fusedLine = new ILOperation
                {
                    OpCode = ILOpCode.Nop,
                    Position = positionBase + 1 + original.Position,
                    Size = original.Size,
                    Operation = fusedOperation,
                    SourceMethod = target,
                };
                // Unlike a decoded line (whose RawParameter gets set by
                // ILMethodCodePass.Decode calling ConvertParameter once,
                // during normal decode), this operation is built directly
                // here and never goes through that path -- same reason
                // OpInlinePrologue/OpInlineEpilogue's own construction
                // above calls ConvertParameter explicitly instead of
                // leaving RawParameter at its default null.
                fusedLine.RawParameter = fusedOperation.ConvertParameter(context, fusedLine);
                result.Add(fusedLine);
                k++; // the Ldfld is consumed too -- don't emit it again below
                continue;
            }

            var calleeLine = new ILOperation
            {
                OpCode = original.OpCode,
                OriginalParameter = original.OriginalParameter,
                RawParameter = original.RawParameter,
                Position = positionBase + 1 + original.Position,
                Size = original.Size,
                Operation = original.Operation,
            };

            if (calleeLine.OpCode == ILOpCode.Ret)
            {
                bool isTrailing = k == calleeLines.Count - 1;
                if (isTrailing)
                {
                    // Nothing was ever reserved, so there is nothing for a
                    // real epilogue to release either -- just fall straight
                    // through into the continuation marker right after it.
                    calleeLine.OpCode = ILOpCode.Nop;
                    calleeLine.Operation = new OpInlineContinue();
                    calleeLine.RawParameter = null;
                }
                else
                {
                    calleeLine.OpCode = ILOpCode.Nop;
                    calleeLine.Operation = new OpInlineJumpOnly(continueLabel);
                    calleeLine.RawParameter = null;
                }
            }
            else if (paramIndex != null)
            {
                // SourceMethod still needs to point at the callee -- the
                // operand's own SizeSuffix/Is16Bit/StackContent all key off
                // it to get this parameter's real TYPE right (unaffected by
                // which position we point it at); only the POSITION itself
                // (RawParameter) changes, to the caller's existing slot.
                calleeLine.SourceMethod = target;
                calleeLine.RawParameter = callerRelPos[paramIndex.Value].ToString();
            }
            else
            {
                calleeLine.SourceMethod = target;
            }

            result.Add(calleeLine);
        }

        var continueMarker = new ILOperation
        {
            OpCode = ILOpCode.Nop,
            Position = positionBase + 9999,
            Operation = new OpInlineContinue(),
            Label = continueLabel,
        };
        result.Add(continueMarker);

        return new SpliceResult(paramCount, result);
    }

    // Shared with ILCallSiteCountPass, which needs this exact same opcode
    // classification to estimate -- BEFORE any splice is actually built --
    // how many of a method's call sites will qualify for trivial-argument
    // substitution, so its byte-cost model can tell those sites apart from
    // ordinary materializing ones. Single source of truth for "which
    // opcodes read something already resident at a fixed position with no
    // side effect": ProducerRelPos below computes the actual position for
    // this pass's own splice-building use; ILCallSiteCountPass only needs
    // the yes/no.
    internal static bool IsTrivialArgumentProducer(ILOpCode opCode) => opCode switch
    {
        ILOpCode.Ldarg_0 or ILOpCode.Ldarg_1 or ILOpCode.Ldarg_2 or ILOpCode.Ldarg_3
            or ILOpCode.Ldarg or ILOpCode.Ldarg_s
            or ILOpCode.Ldloc_0 or ILOpCode.Ldloc_1 or ILOpCode.Ldloc_2 or ILOpCode.Ldloc_3
            or ILOpCode.Ldloc or ILOpCode.Ldloc_s => true,
        _ => false,
    };

    // Shared with ILCallSiteCountPass for the same reason as
    // IsTrivialArgumentProducer above: a CALLEE-level (not call-site-level)
    // precondition for trivial substitution -- no local variables of its
    // own, and it never reassigns one of its own parameters (see
    // TryBuildTrivialSubstitution's own comment for why the latter
    // matters). ILCallSiteCountPass needs this to decide whether a
    // method's own TrivialSiteCounts tally is actually usable for its cost
    // model, or must be treated as 0 because the callee itself disqualifies
    // every one of those sites regardless of how trivial their arguments
    // are.
    internal static bool CalleeQualifiesForTrivialSubstitution(MethodBase target, List<ILOperation> calleeLines)
    {
        var body = target.GetMethodBody();
        if (body == null || body.LocalVariables.Count > 0)
            return false;
        foreach (var line in calleeLines)
        {
            if (line.OpCode == ILOpCode.Starg || line.OpCode == ILOpCode.Starg_s)
                return false;
        }
        return true;
    }

    // A "trivial property getter": the callee's ENTIRE body is exactly
    // [trivial producer, Ldfld, Ret] -- nothing else. At every call site
    // where this shape's own argument is also trivial, TryBuildTrivialSubstitution
    // always fuses it into a single OpPushFldAt (see that class's own
    // comment), so the real per-copy cost is known precisely, not
    // estimated: measured directly off a real Hunchback splice
    // (Player.X's own getter, forced via AggressiveInlining to confirm),
    // deleting its 54-byte standalone definition while paying 17 bytes
    // per inlined copy (16-bit field) against a 9-byte real call site --
    // net win up to N=6 sites (54 / (17-9) = 6.75), a loss past that.
    // ILCallSiteCountPass treats this shape as an unconditional win
    // regardless of N, by explicit choice: this codebase's own property
    // getters realistically have a handful of call sites, never dozens,
    // so the byte-cost model's much-less-reliable general formula (see
    // its own history of persistent, non-trivial gaps between predicted
    // and measured savings) isn't trusted to gate this one well-understood,
    // fully-measured case. Revisit if a getter this shape is ever found
    // with a large, genuinely unbounded call-site count.
    internal static bool IsTrivialPropertyGetterShape(List<ILOperation> lines) =>
        lines.Count == 3
        && IsTrivialArgumentProducer(lines[0].OpCode)
        && lines[1].OpCode == ILOpCode.Ldfld
        && lines[2].OpCode == ILOpCode.Ret;

    // The caller-side rel_pos a trivial argument-producing instruction
    // reads from, or null if it isn't one of the recognized trivial shapes.
    private static int? ProducerRelPos(CompilerMethodContext context, ILOperation producer)
    {
        if (!IsTrivialArgumentProducer(producer.OpCode))
            return null;

        return producer.OpCode switch
        {
            ILOpCode.Ldarg_0 => context.GetParameterReferencePosition(0),
            ILOpCode.Ldarg_1 => context.GetParameterReferencePosition(1),
            ILOpCode.Ldarg_2 => context.GetParameterReferencePosition(2),
            ILOpCode.Ldarg_3 => context.GetParameterReferencePosition(3),
            ILOpCode.Ldarg => context.GetParameterReferencePosition((int)producer.OriginalParameter),
            ILOpCode.Ldarg_s => context.GetParameterReferencePosition((int)producer.OriginalParameter),
            ILOpCode.Ldloc_0 => context.GetLocalVariableReferencePosition(0),
            ILOpCode.Ldloc_1 => context.GetLocalVariableReferencePosition(1),
            ILOpCode.Ldloc_2 => context.GetLocalVariableReferencePosition(2),
            ILOpCode.Ldloc_3 => context.GetLocalVariableReferencePosition(3),
            ILOpCode.Ldloc => context.GetLocalVariableReferencePosition((int)producer.OriginalParameter),
            ILOpCode.Ldloc_s => context.GetLocalVariableReferencePosition((int)producer.OriginalParameter),
            _ => null,
        };
    }

    // Which parameter index (within the CALLEE's own body) this operation
    // reads, or null if it isn't an argument read at all.
    private static int? ArgIndex(ILOperation operation) => operation.OpCode switch
    {
        ILOpCode.Ldarg_0 => 0,
        ILOpCode.Ldarg_1 => 1,
        ILOpCode.Ldarg_2 => 2,
        ILOpCode.Ldarg_3 => 3,
        ILOpCode.Ldarg => (int)operation.OriginalParameter,
        ILOpCode.Ldarg_s => (int)operation.OriginalParameter,
        _ => null,
    };

    private List<ILOperation> BuildSplice(CompilerMethodContext context, CompilerMethodContext calleeContext, List<ILOperation> calleeLines)
    {
        var target = calleeContext.Method;
        int siteId = _inlineSiteCounter++;
        // Disjoint from the caller's own Position range (bounded by its own
        // IL byte length, always far smaller than this) and from every
        // other inline site's -- see the plan's own "splicing mechanics"
        // for why this means ILMethodLabelPass needs no changes at all:
        // relative branch offsets inside the callee are preserved exactly
        // by applying the same shift uniformly to every copied operation's
        // own ORIGINAL Position (its byte offset within the callee's own
        // IL stream) -- NOT a compacted index into calleeLines. A branch's
        // RawParameter is still the raw relative BYTE offset decode left it
        // as (ILMethodLabelPass hasn't run yet), and
        // target = RawParameter + Position only comes out right if Position
        // keeps the same byte-offset units/spacing it already had, just
        // shifted by a constant. 10,000-wide window per site: comfortably
        // more than any eligible (MaxCalleeOperationCount-bounded) callee
        // body's IL byte length, so the prologue (window start), every
        // shifted callee position, and the continuation marker (window
        // end) can never collide with each other or with an adjacent
        // site's window.
        int positionBase = 1_000_000 + 10_000 * siteId;
        string continueLabel = $"{context.Method.GetLabel()}_inline{siteId}_end";

        var result = new List<ILOperation>(calleeLines.Count + 2);

        var prologueOperation = new OpInlinePrologue(calleeContext.GetLocalsSize(), target.GetParameterRefList(), target);
        var prologue = new ILOperation
        {
            OpCode = ILOpCode.Nop,
            Position = positionBase,
            Operation = prologueOperation,
        };
        prologue.RawParameter = prologueOperation.ConvertParameter(context, prologue);
        result.Add(prologue);

        // No return-address slot was ever reserved for this splice (there
        // was no jsr), so the epilogue only ever needs to rewind
        // stackPointer by params+this+locals, not GetLocalStackSize()'s own
        // +2.
        int calleeStackSize = calleeContext.GetLocalStackSize() - 2;
        var refListPositions = BuildRefListPositions(calleeContext, target);

        for (int k = 0; k < calleeLines.Count; k++)
        {
            // Clone, never reuse the cached ILOperation instance directly:
            // once a method can have more than one inlinable site (Phase
            // 2), the same cached body gets spliced at every one of them,
            // each needing its OWN Position/SourceMethod/OpCode/Operation
            // (an epilogue's jmp target, in particular, differs per site).
            // The cached line's own NextInstructions/PreviousInstructions/
            // StackContent/Optimized are all still at their decode-time
            // defaults (empty/null/false) at this point -- nothing between
            // ILCallSiteCountPass's own decode and here ever touches them
            // -- so a fresh ILOperation naturally starts the same way.
            var original = calleeLines[k];
            var calleeLine = new ILOperation
            {
                OpCode = original.OpCode,
                OriginalParameter = original.OriginalParameter,
                RawParameter = original.RawParameter,
                Position = positionBase + 1 + original.Position,
                Size = original.Size,
                Operation = original.Operation,
            };

            if (calleeLine.OpCode == ILOpCode.Ret)
            {
                // A trailing Ret (the callee's own last operation) falls
                // straight through into the continuation marker right
                // after it -- no jmp needed. Any other Ret (an early
                // return) must jmp there instead, since real callee
                // instructions still follow it before that marker.
                bool isTrailing = k == calleeLines.Count - 1;
                var epilogueOperation = new OpInlineEpilogue(calleeStackSize, refListPositions, isTrailing ? null : continueLabel);
                calleeLine.Operation = epilogueOperation;
                // Tagged Nop, not Ret -- ILMethodPromoteLocalsPass's
                // whole-method SafeOpCodes allowlist (and anything else
                // that pattern-matches on OpCode) must see this as the
                // benign bookkeeping it actually is, not a real return it
                // would need its own special-case for.
                calleeLine.OpCode = ILOpCode.Nop;
                calleeLine.RawParameter = epilogueOperation.ConvertParameter(context, calleeLine);
            }
            else
            {
                // Marks every other spliced-in line as belonging to the
                // callee, not the caller -- see ILOperation.SourceMethod's
                // own comment for exactly which downstream call sites this
                // protects and why.
                calleeLine.SourceMethod = target;
            }

            result.Add(calleeLine);
        }

        var continueMarker = new ILOperation
        {
            OpCode = ILOpCode.Nop,
            // Comfortably past any shifted callee position (see the window
            // comment above) -- not positionBase + 1 + calleeLines.Count,
            // which is a compacted INDEX and would collide with a shifted
            // Position the same way the original bug did.
            Position = positionBase + 9999,
            Operation = new OpInlineContinue(),
            Label = continueLabel,
        };
        result.Add(continueMarker);

        return result;
    }

    // Same ref_list shape OpRet.ConvertParameter builds for a normal,
    // non-inlined method's own #method_exit -- position-based (rel_pos
    // values method_exit_inline's own ldx localsStack-ref,y loop indexes
    // with), not the boolean-ish shape GetParameterRefList builds for the
    // PROLOGUE's pull loop. Computed once, here, against the callee's own
    // calleeContext -- never deferred to OpRet's own ConvertParameter
    // running later against the shared (caller's) context, which is
    // exactly the VarIndex hazard this pass exists to avoid.
    private static List<string> BuildRefListPositions(CompilerMethodContext calleeContext, MethodBase target)
    {
        var refList = new List<string>();
        bool isInstance = !target.IsStatic;
        for (int i = 0; i < target.GetParameters().Length; i++)
        {
            if (target.GetParameters()[i].ParameterType.IsReferenceCounted())
                refList.Add(calleeContext.GetParameterReferencePosition(i + (isInstance ? 1 : 0)).ToString());
        }

        var body = target.GetMethodBody();
        var variables = body.LocalVariables;
        for (int i = 0; i < variables.Count; i++)
        {
            if (variables[i].LocalType.IsReferenceCounted())
                refList.Add(calleeContext.GetLocalVariableReferencePosition(i).ToString());
        }

        return refList;
    }
}
