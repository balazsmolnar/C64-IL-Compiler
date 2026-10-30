using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using Compiler.Ops;

namespace Compiler;

// Call sites of static-frame methods (ILStaticFramePass), after every
// optimizer: OpCall.Emit's general form pushes every argument onto the
// hardware stack and pulls it straight back into the callee's parameter
// slot (`lda src; pha ... pla; sta slot`), then pushes the returned A/X
// (sf_result) for whatever consumes it. When every argument is a single
// simple load -- a local/parameter, a constant or a static field, the
// lines immediately before the call -- this copies each one straight into
// its slot instead (`lda src; sta slot`); and when the line right after
// the call stores the result into a value-type local or static field, or
// pops it, the A/X value is stored directly. Either half applies on its
// own.
class ILStaticCallPass : ICompilerMethodPass
{
    public void Execute(CompilerMethodContext context)
    {
        if (context.CompilerContext.StaticFrames.Count == 0)
            return;
        var lines = context.Lines;
        for (int i = 0; i < lines.Count; i++)
        {
            var call = lines[i];
            if (call.Optimized || call.Operation is not OpCall || (call.OpCode != ILOpCode.Call && call.OpCode != ILOpCode.Callvirt))
                continue;
            var callee = context.CompilerContext.Assembly.ManifestModule.ResolveMethod((int)call.OriginalParameter);
            var frame = context.CompilerContext.GetStaticFrame(callee);
            if (frame == null || frame.CalleePulls)
                continue;
            if (call.OpCode == ILOpCode.Callvirt && !callee.IsDevirtualizable(context.CompilerContext.Assembly))
                continue;

            var args = TryDirectArguments(context, lines, i, callee, frame, out var argLines);
            var result = TryDirectResult(context, lines, i, frame, out var resultLine);
            if (args == null && result == null)
                continue;

            var sb = new StringBuilder();
            sb.Append(args ?? StaticPulls(frame));
            sb.Append($"jsr {callee.GetLabel()}");
            if (result != null)
                sb.Append(result);
            else if (frame.ReturnBytes > 0)
                sb.Append($"\n    #sf_result {frame.ReturnBytes}");

            if (args != null)
                foreach (var l in argLines)
                    l.Optimized = true;
            if (result != null)
                resultLine.Optimized = true;
            call.Operation = new OpStaticFrameCall(sb.ToString());
        }
    }

    private static string StaticPulls(StaticFrameInfo frame) => OpCall.StaticFramePulls(frame);

    // Byte k of an argument's source (k = 0 low): `lda ...`, or null when
    // the producer is not a simple load this pass can copy from.
    private sealed class Source
    {
        public Func<int, string> Load;
        public bool NeedsY;
        public int Bytes;
    }

    private static string TryDirectArguments(CompilerMethodContext context, List<ILOperation> lines, int callIndex,
        MethodBase callee, StaticFrameInfo frame, out List<ILOperation> argLines)
    {
        argLines = new List<ILOperation>();
        var call = lines[callIndex];
        if (call.Label != null)
            return null;

        var parameters = callee.GetParameters();
        bool isInstance = !callee.IsStatic;
        int count = parameters.Length + (isInstance ? 1 : 0);
        if (count == 0)
            return null;

        // Producers, last argument first, walking back from the call.
        var sources = new Source[count];
        int at = callIndex - 1;
        for (int a = count - 1; a >= 0; a--)
        {
            if (at < 0)
                return null;
            var line = lines[at];
            var group = new List<ILOperation>();
            // ldc + an empty widening conv (OpLdConst pushes the 16-bit form itself)
            if ((line.OpCode == ILOpCode.Conv_i8 || line.OpCode == ILOpCode.Conv_u8) && at > 0
                && lines[at - 1].Operation is OpLdConst && line.Label == null)
            {
                group.Add(line);
                at--;
                line = lines[at];
            }
            group.Insert(0, line);
            at--;

            if (group.Any(g => g.Optimized || g.SourceMethod != call.SourceMethod))
                return null;
            // Only the first producer may be a branch target.
            if (a > 0 && group[0].Label != null)
                return null;

            var source = SourceOf(context, line);
            if (source == null)
                return null;
            int expected = (isInstance && a == 0) ? 1 : parameters[a - (isInstance ? 1 : 0)].ParameterType.GetStorageBytes();
            if (source.Bytes != expected)
                return null;
            sources[a] = source;
            argLines.InsertRange(0, group);
        }

        // Destination: a static callee's parameter a sits at dynamic rel_pos
        // GetParameterDynamicPosition(a) + RelShift; byte k at +k.
        var calleeContext = new CompilerMethodContext { CompilerContext = context.CompilerContext, Method = callee, Lines = new List<ILOperation>() };
        var refBytes = new HashSet<int>();
        var sb = new StringBuilder();
        if (sources.Any(s => s.NeedsY))
            sb.Append("ldy stackPointer\n    ");
        for (int a = 0; a < count; a++)
        {
            int rel = calleeContext.GetParameterDynamicPosition(a) + frame.RelShift;
            var type = calleeContext.GetParameterType(a);
            bool isRef = !(isInstance && a == 0) && type.IsReferenceCounted();
            for (int k = 0; k < sources[a].Bytes; k++)
            {
                sb.Append($"{sources[a].Load(k)}\n    sta SF_TOP-{rel}+{k}\n    ");
                if (isRef)
                    sb.Append("tax\n    inc objTableRootCount,x\n    ");
            }
        }
        return sb.ToString();
    }

    private static Source SourceOf(CompilerMethodContext context, ILOperation line)
    {
        var type = line.StackContent?.LastOrDefault();
        if (type == null || type == typeof(float))
            return null;
        int bytes = type.GetStorageBytes();
        if (bytes > 2)
            return null;

        switch (line.Operation)
        {
            case OpLdloc:
            case OpLdloc_s:
            case OpLdarg:
            case OpLdarg_s:
            {
                // A zero-page-promoted local/parameter only exists in a leaf
                // method (ILMethodPromoteLocalsPass), which has no calls; be
                // safe anyway.
                if (context.PromotedLocals.Count > 0 || context.PromotedParameters.Count > 0)
                    return null;
                if (!int.TryParse(line.RawParameter?.ToString(), out var rel))
                    return null;
                bool isStatic = rel >= 256;
                return new Source
                {
                    Bytes = bytes,
                    NeedsY = !isStatic,
                    Load = k => isStatic ? $"lda SF_TOP-{rel}+{k}" : $"lda localsStack-{rel}+{k},y",
                };
            }
            case OpLdConst:
            {
                var value = line.RawParameter?.ToString();
                if (string.IsNullOrEmpty(value))
                    return null;
                return new Source { Bytes = bytes, Load = k => k == 0 ? $"lda #<({value})" : $"lda #>({value})" };
            }
            case OpLdsld:
            {
                var label = line.RawParameter?.ToString();
                if (string.IsNullOrEmpty(label))
                    return null;
                return new Source { Bytes = bytes, Load = k => $"lda {label}+{k}" };
            }
            default:
                return null;
        }
    }

    // The value sf_ret hands back (A = low byte for 1 byte; X = low, A =
    // high for 2), stored by the line after the call.
    private static string TryDirectResult(CompilerMethodContext context, List<ILOperation> lines, int callIndex,
        StaticFrameInfo frame, out ILOperation resultLine)
    {
        resultLine = null;
        if (frame.ReturnBytes == 0 || callIndex + 1 >= lines.Count)
            return null;
        var next = lines[callIndex + 1];
        var call = lines[callIndex];
        if (next.Optimized || next.Label != null || next.SourceMethod != null || call.SourceMethod != null)
            return null;
        int bytes = frame.ReturnBytes;

        string text = null;
        switch (next.Operation)
        {
            case OpPop:
                text = "";
                break;
            case OpStloc stloc:
                text = StoreLocal(context, stloc.VarIndex, bytes);
                break;
            case OpStloc_s:
                text = StoreLocal(context, (int)next.OriginalParameter, bytes);
                break;
            case OpStsfld:
            {
                var field = context.Method.ReflectedType.Module.ResolveField((int)next.OriginalParameter);
                if (field.FieldType.IsReferenceCounted() || field.FieldType == typeof(float) || field.FieldType.GetStorageBytes() != bytes)
                    return null;
                var label = next.RawParameter.ToString().Split(',')[0].Trim();
                text = bytes == 1 ? $"\n    sta {label}" : $"\n    sta {label}+1\n    stx {label}";
                break;
            }
        }
        if (text == null)
            return null;
        resultLine = next;
        return text;
    }

    private static string StoreLocal(CompilerMethodContext context, int varIndex, int bytes)
    {
        var type = context.GetLocalVariableType(varIndex);
        if (type.IsReferenceCounted() || type == typeof(float) || type.GetStorageBytes() != bytes)
            return null;
        if (context.PromotedLocalSlot(varIndex) != null)
            return bytes == 1 ? $"\n    sta {context.PromotedLocalSlot(varIndex)}" : null;
        int rel = context.GetLocalVariableReferencePosition(varIndex);
        if (rel >= 256)
            return bytes == 1 ? $"\n    sta SF_TOP-{rel}" : $"\n    stx SF_TOP-{rel}\n    sta SF_TOP-{rel}+1";
        return bytes == 1
            ? $"\n    ldy stackPointer\n    sta localsStack-{rel},y"
            : $"\n    ldy stackPointer\n    sta localsStack-{rel}+1,y\n    txa\n    sta localsStack-{rel},y";
    }
}

// A call into a static-frame method whose whole text ILStaticCallPass has
// already built (direct argument copies and/or a direct result store).
class OpStaticFrameCall : OpBase
{
    private readonly string _text;

    public OpStaticFrameCall(string text) : base(0)
    {
        _text = text;
    }

    public override string Emit(CompilerMethodContext context, ILOperation operation) => _text;
}
