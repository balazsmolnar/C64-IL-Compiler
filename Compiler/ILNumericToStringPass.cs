using System;
using System.Reflection;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

// Rewrites x.ToString() on a numeric value into a single jsr to a
// hand-written conversion routine. The value itself is already handled by
// the time this runs regardless of which IL shape below applies:
// Ldloca_s/Ldarga_s reuse OpLdloc_s/OpLdarg_s verbatim (CommandMap.cs), so
// they already push the real value, not a real address -- this compiler
// has no ref/out/pointer support, so nothing could ever legitimately rely
// on that distinction.
//
// Two IL shapes both need handling here, confirmed by reading the actual
// generated asm rather than assumed from general IL knowledge:
//
// 1. The common case, what Roslyn actually emits for e.g. `uint x; ...
//    x.ToString();`: a single `call` (not `callvirt`) straight to the
//    concrete type's own override (e.g. `call instance string
//    [mscorlib]System.UInt32::ToString()`) -- no `constrained.` prefix at
//    all. Makes sense in hindsight: value types can't be further
//    subclassed, so there's no dispatch ambiguity for `constrained.` to
//    resolve; Roslyn only needs that prefix when a generic type parameter
//    stands in for the receiver. This is the ONLY shape actually produced
//    by any call in this compiler's own Test/ToStringTests.cs -- confirmed
//    by reading the generated asm (`Ldloca_s` immediately followed by
//    `jsr UInt32_ToString ; Call`).
// 2. `constrained. T` + `callvirt instance string Object::ToString()` --
//    kept as a defensive fallback (e.g. a generic-constrained call could
//    still produce this) even though nothing this compiler can currently
//    build exercises it.
//
// Same pattern-match-and-replace shape as ILAddressFromLabelPass (Ldstr +
// Call C64Address.FromLabel): mark the matched line(s) Optimized (their
// Emit() output becomes a `;`-prefixed comment in the generated asm, per
// ILMethodEmitPass, rather than being assembled) and insert one new
// operation, reusing the consumed Call/Callvirt line's already-computed
// StackContent (pop the pushed value, push string) instead of recomputing
// it -- OpCall.SetStackContent already computed exactly that, since it
// runs during the earlier ILMethodBuildEvaluationStackPass setup step.
//
// Runs in the setup group, right after ILAddressFromLabelPass -- both are
// simple forward pattern-matches over the as-decoded instruction stream,
// before any optimizer pass gets a chance to touch these opcodes.
class ILNumericToStringPass : ICompilerMethodPass
{
    public void Execute(CompilerMethodContext context)
    {
        var lines = context.Lines;
        if (lines == null)
            return;

        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].OpCode == ILOpCode.Call)
            {
                var callee = context.CompilerContext.Assembly.ManifestModule.ResolveMethod((int)lines[i].OriginalParameter) as MethodBase;
                if (callee == null || callee.Name != nameof(ToString) || callee.GetParameters().Length != 0 || !IsSupportedNumericType(callee.DeclaringType))
                    continue;

                Replace(context, lines, i, i, callee.DeclaringType);
            }
            else if (lines[i].OpCode == ILOpCode.Constrained && i + 1 < lines.Count && lines[i + 1].OpCode == ILOpCode.Callvirt)
            {
                var callee = context.CompilerContext.Assembly.ManifestModule.ResolveMethod((int)lines[i + 1].OriginalParameter) as MethodBase;
                if (callee?.DeclaringType != typeof(object) || callee.Name != nameof(ToString))
                    continue;

                var constrainedType = (Type)lines[i].RawParameter;
                if (!IsSupportedNumericType(constrainedType))
                    throw new NotSupportedException($"ToString() is not supported for type {constrainedType.Name} -- only uint/int/ulong/long/float (and byte/sbyte) are.");

                Replace(context, lines, i, i + 1, constrainedType);
            }
        }
    }

    private static bool IsSupportedNumericType(Type type) =>
        type == typeof(uint) || type == typeof(byte) ||
        type == typeof(int) || type == typeof(sbyte) ||
        type == typeof(ulong) || type == typeof(long) ||
        type == typeof(float);

    // Marks lines[firstConsumed..lastConsumed] Optimized and inserts the
    // conversion-routine call right after lastConsumed, reusing its
    // StackContent.
    private static void Replace(CompilerMethodContext context, System.Collections.Generic.List<ILOperation> lines, int firstConsumed, int lastConsumed, Type numericType)
    {
        var label = ConversionLabel(numericType);

        // The float routine (asm/helper/float.asm) is always included,
        // like every other Float_* routine -- asm/helper/*.asm was out of
        // scope for the per-method dead-code-elimination work, so there's
        // no flag for it to register. The four integer routines
        // (asm/helper/tostring.asm) ARE gated the same way C64Lib
        // subroutines are.
        if (numericType != typeof(float))
            context.CompilerContext.UsedLibraryLabels.Add(label);

        var newOperation = new ILOperation
        {
            Operation = new OpBase(0, "jsr"),
            RawParameter = label,
            StackContent = lines[lastConsumed].StackContent,
        };
        lines.Insert(lastConsumed + 1, newOperation);
        for (int j = firstConsumed; j <= lastConsumed; j++)
            lines[j].Optimized = true;
    }

    private static string ConversionLabel(Type numericType)
    {
        if (numericType == typeof(uint) || numericType == typeof(byte))
            return "NumberFormat_UInt8ToString";
        if (numericType == typeof(int) || numericType == typeof(sbyte))
            return "NumberFormat_Int8ToString";
        if (numericType == typeof(ulong))
            return "NumberFormat_UInt16ToString";
        if (numericType == typeof(long))
            return "NumberFormat_Int16ToString";
        return "NumberFormat_FloatToString";
    }
}
