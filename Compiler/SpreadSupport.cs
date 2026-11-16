using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

// `..` spreads in a collection expression whose target is an array
// (T[] all = [.. a, x, .. b]). Roslyn allocates the result with the summed
// length and copies each spread through spans, one group per spread:
//
//   ldloca RS; <load a>; call ReadOnlySpan<T>::.ctor(T[])
//   ldloca RS; <load dest>; newobj Span<T>::.ctor(T[]); stloc S
//   ldloca S; ldloc idx; ldloca RS; call ReadOnlySpan<T>::get_Length
//   call Span<T>::Slice(int, int); call ReadOnlySpan<T>::CopyTo(Span<T>)
//   ldloc idx; ldloca RS; call ReadOnlySpan<T>::get_Length; add; stloc idx
//
// (single elements in between are plain stelems). Spans don't exist here,
// so Decode (ILMethodCodePass) rewrites them away before any parameter is
// converted: a span is just the array it was made from, so the
// constructors go, `ldloca span` loads that array, get_Length is ldlen,
// Slice(start, length) keeps only start (the length is the whole source),
// and CopyTo becomes #arrayCopyInto (asm/helper/heap.asm), a byte copy
// linked only into programs that use it. Anything else on spans is left
// alone and reported as usual (C64002/C64003).
//
// Roslyn sometimes builds the same expression through List<T> instead
// (Add/AddRange/ToArray): that is reported with its own message.
static class SpreadSupport
{
    public static bool IsSpanType(Type t) =>
        t != null && t.IsGenericType && !t.IsByRef
        && (t.GetGenericTypeDefinition() == typeof(Span<>) || t.GetGenericTypeDefinition() == typeof(ReadOnlySpan<>));

    public static void Rewrite(CompilerMethodContext context, List<ILOperation> lines)
    {
        var module = context.CompilerContext.Assembly.ManifestModule;
        var callees = new MethodBase[lines.Count];
        bool any = false;
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].OpCode is not (ILOpCode.Call or ILOpCode.Callvirt or ILOpCode.Newobj) || lines[i].OriginalParameter is not int token)
                continue;
            try { callees[i] = module.ResolveMethod(token); } catch { }
            if (IsSpanType(callees[i]?.DeclaringType))
                any = true;
        }
        CheckListSpread(context, lines, callees);
        if (!any)
            return;

        // Span locals made from an array: local index -> that array's load.
        var source = new Dictionary<int, ILOperation>();
        var removed = new HashSet<int>();
        var groupStarts = new HashSet<int>();
        for (int i = 0; i + 2 < lines.Count; i++)
        {
            // ldloca X; <load>; call Span::.ctor(T[])
            if (LocalAddress(lines[i]) is int x && IsSimpleLoad(lines[i + 1]) && IsArrayCtor(callees[i + 2], ILOpCode.Call, lines[i + 2]))
            {
                source[x] = lines[i + 1];
                groupStarts.Add(i);
                removed.UnionWith(new[] { i, i + 1, i + 2 });
                i += 2;
            }
            // <load>; newobj Span::.ctor(T[]); stloc X
            else if (IsSimpleLoad(lines[i]) && IsArrayCtor(callees[i + 1], ILOpCode.Newobj, lines[i + 1]) && StoredLocal(lines[i + 2]) is int y)
            {
                source[y] = lines[i];
                groupStarts.Add(i);
                removed.UnionWith(new[] { i, i + 1, i + 2 });
                i += 2;
            }
        }
        if (source.Count == 0)
            return;

        var result = new List<ILOperation>(lines.Count);
        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (removed.Contains(i))
            {
                // A branch could target the first line of a removed group:
                // keep its position on a no-op.
                if (groupStarts.Contains(i))
                    result.Add(Make(ILOpCode.Nop, line.Position, null, new OpBase(0, "; spread")));
                continue;
            }
            var callee = callees[i];
            if (LocalAddress(line) is int x && source.TryGetValue(x, out var load))
                result.Add(Make(load.OpCode, line.Position, load.OriginalParameter));
            else if (IsSpanType(callee?.DeclaringType) && callee.Name == "get_Length")
            {
                result.Add(Make(ILOpCode.Ldlen, line.Position, 0));
                result.Add(Make(ILOpCode.Conv_i4, line.Position, 0));
            }
            else if (IsSpanType(callee?.DeclaringType) && callee.Name == "Slice" && callee.GetParameters().Length == 2)
                result.Add(Make(ILOpCode.Pop, line.Position, 0));
            else if (IsSpanType(callee?.DeclaringType) && callee.Name == "CopyTo" && callee.GetParameters().Length == 1
                && IsSpanType(callee.GetParameters()[0].ParameterType))
            {
                var element = callee.DeclaringType.GetGenericArguments()[0];
                int bytes = element.IsValueType || element == typeof(string) ? element.GetStorageBytes() : 1;
                result.Add(Make(ILOpCode.Nop, line.Position, null, new OpArrayCopyInto(bytes)));
            }
            else
                result.Add(line);
        }
        lines.Clear();
        lines.AddRange(result);
    }

    // After the rewrite, a span local still in use is a real span: rejected.
    public static void CheckSpanLocals(CompilerMethodContext context)
    {
        var body = context.Method.GetMethodBody();
        if (body == null)
            return;
        var locals = body.LocalVariables;
        foreach (var line in context.Lines)
        {
            int? index = LocalAddress(line) ?? StoredLocal(line) ?? LoadedLocal(line);
            if (index is int i && i < locals.Count && IsSpanType(locals[i].LocalType))
            {
                context.CurrentIlOffset = line.Position;
                var problem = UnsupportedTypes.Check(locals[i].LocalType, context.CompilerContext.Assembly, "A local variable");
                throw problem ?? new UnsupportedFeatureException(DiagnosticCodes.UnsupportedType,
                    $"A local variable of type '{locals[i].LocalType.Name}' is not supported.", "Use an array.");
            }
        }
    }

    private static void CheckListSpread(CompilerMethodContext context, List<ILOperation> lines, MethodBase[] callees)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            var c = callees[i];
            if (c?.DeclaringType == null || !c.DeclaringType.IsGenericType
                || c.DeclaringType.GetGenericTypeDefinition() != typeof(List<>) || c.Name != "AddRange")
                continue;
            if (!callees.Skip(i).Any(m => m?.DeclaringType == c.DeclaringType && m.Name == "ToArray"))
                continue;
            context.CurrentIlOffset = lines[i].Position;
            throw new UnsupportedFeatureException(DiagnosticCodes.UnsupportedCall,
                "This collection expression with '..' is built by the C# compiler through a List<T>, which is not supported.",
                "Spreads into an array are supported when C# copies them directly, e.g. T[] all = [.. a, .. b] or [.. a, x, .. b] with a and b arrays. " +
                "Here it chose a List<T> (it does for some mixes of single elements and spreads): move the single elements into a small array and spread that, e.g. [.. first, .. a, .. b].");
        }
    }

    private static ILOperation Make(ILOpCode opCode, int position, object parameter, OpBase operation = null) => new()
    {
        OpCode = opCode,
        Position = position,
        OriginalParameter = parameter,
        RawParameter = parameter,
        Operation = operation ?? CommandMap.Get(opCode),
    };

    private static bool IsArrayCtor(MethodBase m, ILOpCode opCode, ILOperation line) =>
        line.OpCode == opCode && m is ConstructorInfo && IsSpanType(m.DeclaringType)
        && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType.IsArray;

    private static bool IsSimpleLoad(ILOperation line) => line.OpCode is
        ILOpCode.Ldloc_0 or ILOpCode.Ldloc_1 or ILOpCode.Ldloc_2 or ILOpCode.Ldloc_3 or ILOpCode.Ldloc_s or ILOpCode.Ldloc or
        ILOpCode.Ldarg_0 or ILOpCode.Ldarg_1 or ILOpCode.Ldarg_2 or ILOpCode.Ldarg_3 or ILOpCode.Ldarg_s or ILOpCode.Ldarg or
        ILOpCode.Ldsfld;

    private static int? LocalAddress(ILOperation line) =>
        line.OpCode is ILOpCode.Ldloca_s or ILOpCode.Ldloca && line.OriginalParameter is int i ? i : null;

    private static int? StoredLocal(ILOperation line) => line.OpCode switch
    {
        ILOpCode.Stloc_0 => 0, ILOpCode.Stloc_1 => 1, ILOpCode.Stloc_2 => 2, ILOpCode.Stloc_3 => 3,
        ILOpCode.Stloc_s or ILOpCode.Stloc when line.OriginalParameter is int i => i,
        _ => null,
    };

    private static int? LoadedLocal(ILOperation line) => line.OpCode switch
    {
        ILOpCode.Ldloc_0 => 0, ILOpCode.Ldloc_1 => 1, ILOpCode.Ldloc_2 => 2, ILOpCode.Ldloc_3 => 3,
        ILOpCode.Ldloc_s or ILOpCode.Ldloc when line.OriginalParameter is int i => i,
        _ => null,
    };
}

// Synthesized by SpreadSupport for ReadOnlySpan<T>.CopyTo: pops src, dest
// and start, copies src into dest from element start on.
class OpArrayCopyInto : OpBase
{
    private readonly int _elementBytes;

    public OpArrayCopyInto(int elementBytes) : base(0, "#arrayCopyInto")
    {
        _elementBytes = elementBytes;
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        operation.StackContent.RemoveLast(3);
    }

    public override string Emit(CompilerMethodContext context, ILOperation operation)
    {
        context.CompilerContext.UsedLibraryLabels.Add("Array_CopyInto");
        return $"{Command} {_elementBytes}";
    }
}
