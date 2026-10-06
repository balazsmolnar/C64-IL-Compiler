using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;

namespace Compiler;

// Whole-program pre-pass, run before ILCodePass (see Program.cs). Two jobs,
// both needing every method decoded up front, before ILCodePass's own
// method-enumeration order has a chance to visit a caller before (or after)
// a method it calls:
//
// 1. Tallies InlinableSiteCounts (Call sites plus devirtualizable Callvirt
//    sites -- see MethodBaseExtension.IsDevirtualizable) and
//    MandatoryStandalone (Ldftn targets and non-devirtualizable Callvirt
//    targets -- see that set's own comment in ICompilerPass.cs).
// 2. From those two, decides InlineCandidates/DeleteStandaloneDefinition:
//    a method qualifies only if it's a leaf (no Call/Callvirt/Switch
//    anywhere in its own body -- recursion and nested inlining are both
//    out of scope, Switch excluded defensively pending a check of
//    ILMethodJumpTablePass's assumptions) and decodes to no more than
//    MaxCalleeOperationCount operations -- hard preconditions,
//    [MethodImpl(MethodImplOptions.AggressiveInlining)] included.
//    Past that: a single inlinable site is a win whenever the standalone
//    definition also gets deleted (the body isn't duplicated, just
//    relocated); every other case -- more than one site, or exactly one
//    site but MandatoryStandalone (the original has to keep existing for
//    some OTHER reference anyway, so inlining this one just duplicates it
//    for a 3-byte jsr saving) -- goes through the same byte-cost model,
//    unless AggressiveInlining forces it regardless. Measured against
//    Hunchback: multi-site inlining of an ordinary small property getter
//    is already a net loss at N=2 (the per-copy prologue/epilogue
//    overhead this stack machine pays dwarfs the few bytes a single jsr
//    saves), so in practice the model approves N>1 only when
//    AggressiveInlining forces it -- see ShouldInline's own comment for
//    the full derivation. ILMethodInliningPass splices the cached decoded body
//    into EVERY inlinable site (cloning fresh per site -- see
//    ILMethodInliningPass's own comment for why cloning, not reusing, is
//    now required); ILMethodEmitPass skips emitting a standalone
//    definition for any method in DeleteStandaloneDefinition, since
//    nothing jsr's into it anymore -- WITHOUT that skip, inlining would
//    silently DUPLICATE the callee's code instead of relocating it (found
//    via Test's own unittest.prg tipping over its memory ceiling the first
//    time Phase 1 ran for real).
//
// This can't be split into a per-method setup pass the way ILLibraryUsagePass's
// similar whole-program scan is: that one only needs its own aggregate read
// back once every method is done (ILLibraryFlagsPass runs strictly after
// ILCodePass finishes), but ILMethodInliningPass needs a COMPLETE decision
// while still processing the very first caller.
class ILCallSiteCountPass : ICompilerPass
{
    private const int MaxCalleeOperationCount = 40;

    // Byte-cost constants for the N>1 model, all MEASURED directly off a
    // real inlined splice in Hunchback's own assembled output (an
    // Enemy.get_X copy spliced at one of its 4 call sites: a 1-param,
    // 0-local, empty-ref_list leaf), not guessed -- see the approved plan's
    // "Cost model for N > 1" for the derivation and
    // asm/helper/localsStack.asm for the macro bodies these come from.
    //
    // InlineOverhead = one inlined copy's own prologue (9 bytes: `ldy
    // stackPointer; pla; sta localsStack,y; iny; sty stackPointer`) plus
    // its epilogue (7 bytes fallthrough: `lda stackPointer; sec; sbc #N;
    // sta stackPointer`, or +3 more for the jmp variant's `jmp Label`).
    // Paid EVERY one of the N inlined copies, in full -- NOT a delta,
    // unlike DeletionBonus below, because unlike the real (shared, paid
    // once) prologue/epilogue, each inlined copy needs its own.
    private const int InlineOverheadTrailingRet = 9 + 7;
    private const int InlineOverheadWithJmp = 9 + 7 + 3;

    // DeletionBonus = the ONE-TIME saving from deleting the standalone
    // definition entirely (only applies when !mandatoryStandalone): the
    // real prologue is exactly 8 bytes bigger than the inline one (`pla;
    // sta localsStack,y; pla; sta localsStack+1,y`, the return-address
    // handling inline skips), and the real epilogue is exactly 10 (trailing)
    // or 7 (jmp) bytes bigger (`tax; lda localsStack+1,x; pha; lda
    // localsStack,x; pha; rts` vs. nothing/`jmp Label`) -- this is a DELTA,
    // not InlineOverhead's absolute cost, and the ref_list loop (identical
    // in both variants of each macro) cancels out of it either way.
    private const int DeletionBonusTrailingRet = 8 + 10;
    private const int DeletionBonusWithJmp = 8 + 7;

    private const int JsrRemoved = 3;

    // "Average compiled bytes per non-Ret ILOperation" -- the same real
    // splice measured Ldarg_0 at 6 bytes and Ldfld16 at 14 (20 bytes over 2
    // operations). Ret is excluded from both this average and from the
    // body-size count below on purpose: its cost is already fully captured
    // by InlineOverhead/DeletionBonus above, so counting it again here
    // would double-charge it.
    private const int EstimatedBytesPerOperation = 10;

    public void Execute(CompilerContext context)
    {
        // EmitDebugInfo must gate every decision this pass makes -- the two
        // halves (suppress a method's standalone definition / actually
        // splice its sites) have to be made by the exact same condition, or
        // one half fires without the other: suppressing the definition
        // while EmitDebugInfo skips the splice leaves a `jsr` to a label
        // that no longer exists (found via the debug build's own assembler
        // failing with "not defined symbol" on exactly the methods that
        // would otherwise have been inlined, the first time Phase 1 ran for
        // real).
        if (context.EmitDebugInfo)
            return;

        var decodedBodies = new Dictionary<MethodBase, List<ILOperation>>();

        foreach (var method in EnumerateCompiledMethods(context))
        {
            // ILTypeVTablePass emits `.word <every virtual method's own
            // label>` for its declaring type's vtable, UNCONDITIONALLY --
            // completely independent of whether any Call/Callvirt/Ldftn
            // instruction anywhere in the IL ever references it. That's an
            // implicit address-taken reference this scan can't see by
            // walking opcodes at all (found via Test's own TestA/
            // StandaloneVirtual producing "not defined symbol" vtable
            // errors the first time devirtualized-Callvirt inlining ran for
            // real) -- so every virtual method is mandatory-standalone by
            // construction, regardless of how its own Callvirt call sites
            // happen to devirtualize.
            if (method.IsVirtual)
                context.MandatoryStandalone.Add(method);

            List<ILOperation> lines;
            try
            {
                var scratchContext = new CompilerMethodContext
                {
                    CompilerContext = context,
                    Method = method,
                    Lines = new List<ILOperation>(),
                };
                ILMethodCodePass.Decode(scratchContext);
                lines = scratchContext.Lines;
            }
            catch
            {
                // Some construct ILMethodCodePass itself would also reject --
                // never reported here, the real ILCodePass run right after
                // this one reports it properly; this method just can't be
                // counted or considered for inlining.
                continue;
            }

            decodedBodies[method] = lines;

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (line.OpCode == ILOpCode.Call)
                {
                    var target = context.Assembly.ManifestModule.ResolveMethod((int)line.OriginalParameter) as MethodBase;
                    if (target == null)
                        continue;
                    context.InlinableSiteCounts.TryGetValue(target, out var count);
                    context.InlinableSiteCounts[target] = count + 1;
                    CountIfTrivialSite(context, target, lines, i);
                }
                else if (line.OpCode == ILOpCode.Callvirt)
                {
                    var target = context.Assembly.ManifestModule.ResolveMethod((int)line.OriginalParameter) as MethodBase;
                    if (target == null)
                        continue;
                    if (target.IsDevirtualizable(context.Assembly))
                    {
                        context.InlinableSiteCounts.TryGetValue(target, out var count);
                        context.InlinableSiteCounts[target] = count + 1;
                        CountIfTrivialSite(context, target, lines, i);
                    }
                    else
                    {
                        context.MandatoryStandalone.Add(target);
                    }
                }
                else if (line.OpCode == ILOpCode.Ldftn)
                {
                    var target = context.Assembly.ManifestModule.ResolveMethod((int)line.OriginalParameter) as MethodBase;
                    if (target != null)
                        context.MandatoryStandalone.Add(target);
                }
            }
        }

        foreach (var (method, lines) in decodedBodies)
        {
            if (!context.InlinableSiteCounts.TryGetValue(method, out var n) || n == 0)
                continue;
            if (lines.Count == 0 || lines.Count > MaxCalleeOperationCount)
                continue;
            if (lines.Any(l => l.OpCode == ILOpCode.Call || l.OpCode == ILOpCode.Callvirt || l.OpCode == ILOpCode.Switch))
                continue;

            // NOT GetCustomAttribute<MethodImplAttribute>() -- [MethodImpl]
            // compiles to a method header FLAG in metadata (checked via
            // MethodImplementationFlags/MethodImplAttributes), not a
            // retrievable custom attribute blob; GetCustomAttribute<T>()
            // silently returns null for it regardless of the source-level
            // attribute being present (confirmed via a real test: the
            // force-inline splice never fired, the call site stayed a
            // plain jsr, until this was fixed).
            bool forceInline = (method.MethodImplementationFlags & MethodImplAttributes.AggressiveInlining) != 0;
            bool mandatoryStandalone = context.MandatoryStandalone.Contains(method);

            // TrivialSiteCounts was tallied per CALL SITE above (a
            // property of each caller's own code), but whether that tally
            // is even usable is a CALLEE-level question: if this method
            // has its own locals, or reassigns one of its own parameters,
            // ILMethodInliningPass.CalleeQualifiesForTrivialSubstitution
            // rejects every one of its sites regardless of how trivial
            // their arguments are, so none of this method's sites will
            // actually get the cheaper splice -- treat the count as 0, not
            // whatever the (irrelevant) per-site tally says.
            int trivialSites = ILMethodInliningPass.CalleeQualifiesForTrivialSubstitution(method, lines)
                ? Math.Min(n, context.TrivialSiteCounts.TryGetValue(method, out var tcount) ? tcount : 0)
                : 0;

            // A trivial property getter (ILMethodInliningPass.
            // IsTrivialPropertyGetterShape -- see its own comment for the
            // measured break-even this deliberately ignores) with every
            // one of its call sites trivially substitutable, and not
            // mandatoryStandalone (the definition really does go away, so
            // the one-time deletion saving is real, not forfeited): treat
            // as an unconditional win, the same way forceInline bypasses
            // the general byte-cost model.
            bool isTrivialGetter = !mandatoryStandalone && trivialSites == n
                && ILMethodInliningPass.IsTrivialPropertyGetterShape(lines);

            if (!forceInline && !isTrivialGetter && !ShouldInline(n, trivialSites, lines, mandatoryStandalone))
                continue;

            context.InlineCandidates[method] = lines;
            if (!mandatoryStandalone)
                context.DeleteStandaloneDefinition.Add(method);
        }
    }

    // Per-site byte savings a TRIVIAL-ARGUMENT-SUBSTITUTED splice gets over
    // an ordinary materializing one (ILMethodInliningPass.
    // TryBuildTrivialSubstitution): no inline prologue at all (saves the
    // materializing prologue's own 9 bytes, InlineOverheadTrailingRet/
    // WithJmp's shared "9 +" term), and the epilogue either disappears
    // entirely (trailing Ret: was 7, now 0) or shrinks to a bare jmp
    // (early Ret: was 7+3=10, now just the 3-byte jmp). Both shapes save
    // exactly 9+7=16 -- the "+3 for jmp" on the materializing side and the
    // "3-byte jmp" on the trivial side are the same 3 bytes, so it cancels
    // out of the difference either way. Derived algebraically from the
    // SAME measured constants below, re-deriving the whole savings formula
    // with two overhead constants instead of one: this (MOH-TOH) term
    // falls out identically in both the mandatoryStandalone and the
    // deletable case. Deliberately conservative -- it does NOT also credit
    // the caller's own now-dead argument-push instructions (blanked to
    // no-ops at a trivial site, per ArgumentLinesToBlank), which are a
    // real but separate, unmeasured saving; leaving them out only makes
    // this UNDER-estimate the true benefit, never over-promise it.
    private const int TrivialOverheadSavingsPerSite = 16;

    // No N == 1 shortcut: Phase 1's "a single inlinable site is always a
    // win" claim is only true when the standalone definition also gets
    // deleted (the general formula below reduces to exactly that for
    // n == 1 when !mandatoryStandalone -- (1-1)*(...) vanishes, leaving
    // DeletionBonus + JsrRemoved, which is always positive). It's NOT true
    // when mandatoryStandalone: inlining the one site while the original
    // still has to exist anyway for some OTHER reference pays a full
    // InlineOverhead+body to save only JsrRemoved (3 bytes) -- almost
    // always a net loss, so this has to run through the real formula too,
    // not bypass it.
    //
    // trivialSites (new): how many of the n sites will actually get the
    // cheaper trivial-substitution splice instead of the materializing
    // one -- see TrivialOverheadSavingsPerSite above. The rest of the
    // formula is unchanged from the materializing-only model; this is a
    // pure additive correction (derived in ILCallSiteCountPass's own
    // commit, by re-deriving the savings formula with two overhead
    // constants instead of one and simplifying -- the (MOH-TOH) term that
    // falls out is this same constant regardless of trailingRet/jmp
    // shape, in BOTH the mandatoryStandalone and deletable cases).
    private static bool ShouldInline(int n, int trivialSites, List<ILOperation> calleeLines, bool mandatoryStandalone)
    {
        bool trailingRet = HasOnlyATrailingRet(calleeLines);
        int inlineOverhead = trailingRet ? InlineOverheadTrailingRet : InlineOverheadWithJmp;
        // Ret lines excluded from the body-size estimate -- see
        // EstimatedBytesPerOperation's own comment for why double-charging
        // them (once here, once via InlineOverhead/DeletionBonus) was the
        // actual bug the first version of this model shipped with.
        int nonRetOperationCount = calleeLines.Count(l => l.OpCode != ILOpCode.Ret);
        int estimatedBodySize = nonRetOperationCount * EstimatedBytesPerOperation;

        int savings;
        if (mandatoryStandalone)
        {
            // The standalone definition stays regardless (something else
            // still needs it), so there's no one-time deletion bonus --
            // each inlined copy pays its own full prologue+body+epilogue
            // while only ever saving the jsr it replaces.
            savings = -n * (estimatedBodySize + inlineOverhead - JsrRemoved);
        }
        else
        {
            int deletionBonus = trailingRet ? DeletionBonusTrailingRet : DeletionBonusWithJmp;
            savings = (1 - n) * (estimatedBodySize + inlineOverhead) + deletionBonus + n * JsrRemoved;
        }

        savings += trivialSites * TrivialOverheadSavingsPerSite;

        return savings > 0;
    }

    // Tallies context.TrivialSiteCounts[target] when the call at lines[i]
    // feeds EVERY one of target's parameters (including `this`) from a
    // bare Ldarg_N/Ldloc_N immediately before it -- the same shape
    // ILMethodInliningPass.TryBuildTrivialSubstitution itself requires,
    // checked here using only target's PARAMETER SIGNATURE (reflection,
    // always available regardless of decode order) and the CALLER's own
    // already-decoded lines, never target's own body -- see
    // ILMethodInliningPass.IsTrivialArgumentProducer's own comment for why
    // a fixed-offset window here is sound without StackContent.
    private static void CountIfTrivialSite(CompilerContext context, MethodBase target, List<ILOperation> lines, int callIndex)
    {
        bool isInstance = !target.IsStatic;
        int paramCount = target.GetParameters().Length + (isInstance ? 1 : 0);
        if (callIndex < paramCount)
            return;

        for (int p = 0; p < paramCount; p++)
        {
            if (!ILMethodInliningPass.IsTrivialArgumentProducer(lines[callIndex - paramCount + p].OpCode))
                return;
        }

        context.TrivialSiteCounts.TryGetValue(target, out var count);
        context.TrivialSiteCounts[target] = count + 1;
    }

    private static bool HasOnlyATrailingRet(List<ILOperation> calleeLines)
    {
        for (int i = 0; i < calleeLines.Count; i++)
        {
            if (calleeLines[i].OpCode == ILOpCode.Ret && i != calleeLines.Count - 1)
                return false;
        }
        return true;
    }

    // Mirrors ILCodePass.Execute's own type/method enumeration exactly, so
    // this pass counts/decodes precisely the methods the real pass will
    // later process -- see that pass's own loop.
    private static IEnumerable<MethodBase> EnumerateCompiledMethods(CompilerContext context)
    {
        foreach (var type in context.Assembly.GetTypes())
        {
            if (type.IsValueType)
                continue;

            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).OfType<MethodBase>();
            var staticConstructors = type.GetConstructors(BindingFlags.Static | BindingFlags.NonPublic).OfType<MethodBase>();

            foreach (var method in methods.Concat(staticConstructors).Where(m => m.DeclaringType != typeof(object)))
            {
                if (!method.IsAbstract && method.GetMethodBody() == null)
                    continue;
                if (method.DeclaringType != null && method.DeclaringType.Assembly != context.Assembly)
                    continue;
                if (method.IsAbstract)
                    continue;

                yield return method;
            }
        }
    }
}
