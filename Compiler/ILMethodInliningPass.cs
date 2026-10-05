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

            var replacement = TryBuildSplice(context, line);
            if (replacement == null)
                continue;

            context.Lines.RemoveAt(i);
            context.Lines.InsertRange(i, replacement);
            i += replacement.Count - 1;
        }
    }

    private List<ILOperation> TryBuildSplice(CompilerMethodContext context, ILOperation callLine)
    {
        var target = context.CompilerContext.Assembly.ManifestModule.ResolveMethod((int)callLine.OriginalParameter) as MethodBase;
        if (target == null)
            return null;
        if (!context.CompilerContext.InlineCandidates.TryGetValue(target, out var calleeLines))
            return null;

        var calleeContext = new CompilerMethodContext
        {
            CompilerContext = context.CompilerContext,
            Method = target,
            Lines = calleeLines,
        };

        return BuildSplice(context, calleeContext, calleeLines);
    }

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
