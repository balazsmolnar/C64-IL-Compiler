using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

// Recognizes the IL shape Roslyn emits for a plain (no format/alignment)
// C# interpolated string assigned to `string` -- construction of a
// System.Runtime.CompilerServices.DefaultInterpolatedStringHandler
// (ldloca.s handler; ldc.i4 literalLen; ldc.i4 holeCount; call .ctor),
// followed by one AppendLiteral(string)/AppendFormatted<T>(T) call per
// literal chunk/hole (each its own ldloca.s handler; <push value>; call),
// ending with ToStringAndClear() -- and rewrites it into calls this
// compiler already supports: a NumberFormat_*ToString conversion (see
// NumericToStringSupport.cs) for each non-string hole, folded together via
// asm/helper/stringops.asm's String_Concat (the exact routine
// ILStringOpsPass.cs's `a + b` rewrite already uses).
//
// DefaultInterpolatedStringHandler's real implementation (pooled char[]
// buffers, Span<char>, a generic AppendFormatted<T> doing interface
// dispatch at runtime) is far beyond what this compiler could ever support
// directly -- there's no generics, no Span<T>, anywhere else in this
// codebase. This pass sidesteps all of that: the handler struct itself is
// never actually built here, its whole construct/append/finish sequence is
// deleted and replaced with exactly the ToString()+Concat() calls a
// hand-written `a.ToString() + "+" + b.ToString()` expression would already
// compile to. No explicit "re-push an accumulator" step is needed:
// String_Concat pops its second operand first (pushed last) and writes its
// first operand's characters before its second's (confirmed by reading
// asm/helper/stringops.asm directly), and Roslyn already emits each
// hole/literal's value-producing instructions in left-to-right source
// order -- so simply deleting the handler scaffolding and inserting
// conversion/concat jsr's in the gaps naturally folds left-to-right.
//
// Confirmed empirically (live build-and-revert against a temp test) that
// earlier pipeline stages tolerate the unrecognized
// DefaultInterpolatedStringHandler struct type fine on their own --
// TypeExtensions.GetStorageBytes falls through to 1 byte for any
// unrecognized type, and nothing chokes before OperandBase.cs's
// EnsureCallIsResolvable, which is exactly why this pass (running well
// before that, in the setup group below) gets a clean shot at rewriting
// the pattern before that check would otherwise reject it.
//
// Runs in the setup group (Program.cs), after ILMethodBuildEvaluationStackPass
// (needs real StackContent on the instructions it reads) and, like
// ILStringOpsPass/ILNumericToStringPass, deliberately ungated -- do NOT add
// an Optimize-only guard here (that's an optimizer-group concept, e.g.
// ILStaticArrayInitializerPass; this pass belongs with the other two
// setup-group pattern-matches it most resembles).
//
// Known limitation, not defended against: a hole whose own sub-expression
// contains a method call (e.g. $"{SomeMethod()}") fails the segment scan's
// "next Call/Callvirt is on DefaultInterpolatedStringHandler" check against
// SomeMethod's own call instead -- this throws the same clear
// NotSupportedException as any other unrecognized shape (safe, not a
// silent miscompile), just with a less specific message. Supporting calls
// inside holes would need tracking call-depth while scanning for the
// segment terminator; out of scope for "very basic" interpolation.
//
// Also known limitation: format specifiers and alignment (`{x:F2}`,
// `{x,10}`) are rejected with a clear NotSupportedException (see the
// AppendFormatted branch below) rather than supported -- this compiler has
// no format-provider/culture machinery to implement them against. There is
// no automated regression test for this rejection path: the whole Test/*.cs
// project is compiled as a single batch before any NUnit test runs, so a
// test method that deliberately triggers this exception would fail the
// entire batch, not just itself. Verified manually instead (temp test,
// build, read the exception, revert) during this pass's own development.
class ILStringInterpolationPass : ICompilerMethodPass
{
    private const string HandlerTypeName = "System.Runtime.CompilerServices.DefaultInterpolatedStringHandler";

    public void Execute(CompilerMethodContext context)
    {
        var lines = context.Lines;
        if (lines == null)
            return;

        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].OpCode != ILOpCode.Call)
                continue;

            var ctor = ResolveMethod(context, lines[i]);
            if (ctor == null || ctor.DeclaringType?.FullName != HandlerTypeName || ctor.Name != ".ctor")
                continue;

            RewritePattern(context, lines, i);
        }
    }

    private static MethodBase ResolveMethod(CompilerMethodContext context, ILOperation line) =>
        context.CompilerContext.Assembly.ManifestModule.ResolveMethod((int)line.OriginalParameter) as MethodBase;

    private static void RewritePattern(CompilerMethodContext context, List<ILOperation> lines, int ctorIndex)
    {
        // Lead-in: ldloca.s handler; ldc.i4 literalLen; ldc.i4 holeCount; call .ctor
        // -- ldc.i4 matched via OpLdConst (not a specific Ldc_I4* opcode),
        // since Roslyn may emit any of the short forms for a small
        // constant, all normalized to OpLdConst by the decoder (same check
        // ILStaticArrayInitializerPass.cs uses for an array-length push).
        if (ctorIndex < 3
            || lines[ctorIndex - 3].OpCode != ILOpCode.Ldloca_s
            || !(lines[ctorIndex - 2].Operation is OpLdConst)
            || !(lines[ctorIndex - 1].Operation is OpLdConst))
            throw new NotSupportedException(
                "Unsupported string interpolation: unrecognized DefaultInterpolatedStringHandler construction shape.");

        int handlerLocal = (int)lines[ctorIndex - 3].OriginalParameter;
        for (int j = ctorIndex - 3; j <= ctorIndex; j++)
            lines[j].Optimized = true;

        // The only StackContent assignment that actually matters: nothing
        // downstream links back through any of our synthesized nodes (this
        // pass never rewires PreviousInstructions/NextInstructions, same as
        // ILStringOpsPass/ILNumericToStringPass), so whatever follows the
        // whole pattern in the original IL still finds its own
        // PreviousInstructions pointing at the untouched ToStringAndClear
        // call, whose own precomputed StackContent is already correct.
        // This is only needed so the LAST synthesized op (if any) reports a
        // StackContent consistent with what a reader of the operation list
        // itself would expect at that position -- nothing reads it through
        // a graph edge.
        ILOperation lastSynthesized = null;
        bool isFirstPiece = true;
        int segStart = ctorIndex + 1;

        for (; ; )
        {
            if (segStart >= lines.Count || lines[segStart].OpCode != ILOpCode.Ldloca_s || (int)lines[segStart].OriginalParameter != handlerLocal)
                throw new NotSupportedException(
                    "Unsupported string interpolation: unrecognized interpolated string handler shape.");

            int callIndex = segStart + 1;
            while (callIndex < lines.Count && lines[callIndex].OpCode != ILOpCode.Call && lines[callIndex].OpCode != ILOpCode.Callvirt)
                callIndex++;
            if (callIndex >= lines.Count)
                throw new NotSupportedException(
                    "Unsupported string interpolation: unrecognized interpolated string handler shape.");

            var callee = ResolveMethod(context, lines[callIndex]);
            if (callee == null || callee.DeclaringType?.FullName != HandlerTypeName)
                throw new NotSupportedException(
                    "Unsupported string interpolation: unrecognized interpolated string handler shape " +
                    "(a hole containing its own method call is not supported).");

            // Delete the segment's own receiver load -- everything between
            // it and the terminating call is left untouched (it already
            // pushes the right value: a literal string via ldstr, or an
            // arbitrary sub-expression for a computed hole).
            lines[segStart].Optimized = true;

            if (callee.Name == "ToStringAndClear")
            {
                if (callee.GetParameters().Length != 0)
                    throw new NotSupportedException(
                        "Unsupported string interpolation: unrecognized ToStringAndClear overload.");
                lines[callIndex].Optimized = true;
                if (lastSynthesized != null)
                    lastSynthesized.StackContent = lines[callIndex].StackContent;
                return;
            }

            bool pieceIsString;
            Type holeType = null;

            if (callee.Name == "AppendLiteral")
            {
                if (callee.GetParameters().Length != 1)
                    throw new NotSupportedException(
                        "Unsupported string interpolation: unrecognized AppendLiteral overload.");
                pieceIsString = true;
            }
            else if (callee.Name == "AppendFormatted")
            {
                if (callee.GetParameters().Length != 1)
                    throw new NotSupportedException(
                        "Unsupported string interpolation: format specifiers/alignment in interpolation holes " +
                        "(e.g. {x:F2} or {x,10}) are not supported.");

                if (callee.IsGenericMethod)
                {
                    holeType = callee.GetGenericArguments()[0];
                    pieceIsString = false;
                }
                else if (callee.GetParameters()[0].ParameterType == typeof(string))
                {
                    pieceIsString = true;
                }
                else
                {
                    throw new NotSupportedException(
                        "Unsupported string interpolation: unrecognized AppendFormatted overload for " +
                        $"{callee.GetParameters()[0].ParameterType.Name}.");
                }
            }
            else
            {
                throw new NotSupportedException(
                    $"Unsupported string interpolation: unrecognized interpolated string handler member '{callee.Name}'.");
            }

            lines[callIndex].Optimized = true;

            if (!pieceIsString)
            {
                if (!NumericToStringSupport.IsSupportedNumericType(holeType))
                    throw new NotSupportedException(
                        $"Unsupported string interpolation: hole of type {holeType.Name} is not supported -- " +
                        "only uint/int/ulong/long/float (and byte/sbyte) and string holes are.");

                var label = NumericToStringSupport.ConversionLabel(holeType);
                // float's routine is always-included, like every other
                // Float_* routine -- see ILNumericToStringPass's identical
                // comment on the same check.
                if (holeType != typeof(float))
                    context.CompilerContext.UsedLibraryLabels.Add(label);

                var convOp = new ILOperation
                {
                    Operation = new OpBase(0, "jsr"),
                    RawParameter = label,
                    StackContent = lines[callIndex].StackContent,
                };
                lines.Insert(callIndex + 1, convOp);
                callIndex++;
                lastSynthesized = convOp;
            }

            if (!isFirstPiece)
            {
                context.CompilerContext.UsedLibraryLabels.Add("String_Concat");
                var foldOp = new ILOperation
                {
                    Operation = new OpBase(0, "jsr"),
                    RawParameter = "String_Concat",
                    StackContent = lines[callIndex].StackContent,
                };
                lines.Insert(callIndex + 1, foldOp);
                callIndex++;
                lastSynthesized = foldOp;
            }

            isFirstPiece = false;
            segStart = callIndex + 1;
        }
    }
}
