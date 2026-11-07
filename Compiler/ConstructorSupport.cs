using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Compiler;

// How an instance constructor of the program is compiled. Roslyn puts every
// field and property initializer (`int x = 5;`, `X { get; set; } = 3;`) and
// a primary constructor's captured parameters (`class Star(uint speed)`:
// `this.<speed>P = speed`) at the start of each constructor that does not
// chain to `this(...)`, before the `: base(...)` call; the constructor body
// comes after it.
//
//   Trivial       nothing but a call to a base constructor that is itself
//                 trivial (or outside the program: System.Object). Never
//                 compiled; newobj is today's plain #newObj (arguments, if
//                 any, are dropped), and a derived constructor's call to it
//                 is dropped too (OpCall, ILTrivialConstructorCallPass).
//   ConstantInit  no parameters, and only stores of constants into fields of
//                 `this` (plus calls to Trivial/ConstantInit base or this(...)
//                 constructors, merged in place). Folded into the object's
//                 initial bytes: newobj becomes #newObjInit (and joins the
//                 object-initializer and object-array-table forms), so a
//                 field initializer costs no code at all. Compiled as a method
//                 only when a Call constructor chains to it.
//   Call          anything else: compiled as an ordinary instance method, and
//                 newobj allocates, puts the handle under the arguments
//                 (#newObjCtor) and calls it.
enum ConstructorKind { Trivial, ConstantInit, Call }

static class ConstructorSupport
{
    private record Store(FieldInfo Field, string[] Bytes);

    private record Analysis(ConstructorKind Kind, List<Store> Stores);

    private static readonly Dictionary<MethodBase, Analysis> Cache = new();

    private static readonly Dictionary<short, OpCode> OpCodesByValue =
        typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(f => (OpCode)f.GetValue(null))
            .ToDictionary(o => o.Value);

    // Only constructors of the program itself are compiled; a class-library
    // base constructor (System.Object's) does nothing that matters here.
    private static bool IsProgramConstructor(MethodBase ctor) =>
        ctor.DeclaringType != null && MethodBaseExtension.ProgramAssembly != null
        && ctor.DeclaringType.Assembly == MethodBaseExtension.ProgramAssembly;

    public static bool IsInstanceConstructor(MethodBase method) =>
        method is ConstructorInfo && !method.IsStatic;

    public static ConstructorKind Kind(MethodBase ctor) => Analyze(ctor).Kind;

    // The object's initial bytes for a ConstantInit constructor, in the
    // layout #newObjInit copies (size bytes, "0" where nothing is stored).
    public static string[] InitialBytes(CompilerContext context, MethodBase ctor, int size)
    {
        var mem = Enumerable.Repeat("0", size).ToArray();
        foreach (var store in Analyze(ctor).Stores)
        {
            var pos = context.GetFieldPosition(store.Field);
            for (int i = 0; i < store.Bytes.Length && pos + i < size; i++)
                mem[pos + i] = store.Bytes[i];
        }
        return mem;
    }

    private static Analysis Analyze(MethodBase ctor)
    {
        lock (Cache)
        {
            if (Cache.TryGetValue(ctor, out var known))
                return known;
            // A cycle (this(...) chains are acyclic in valid C#, but be safe).
            Cache[ctor] = new Analysis(ConstructorKind.Call, new List<Store>());
            var result = AnalyzeBody(ctor);
            Cache[ctor] = result;
            return result;
        }
    }

    private static readonly Analysis CallKind = new(ConstructorKind.Call, new List<Store>());

    private static Analysis AnalyzeBody(MethodBase ctor)
    {
        if (!IsProgramConstructor(ctor))
            return new Analysis(ConstructorKind.Trivial, new List<Store>());
        var body = ctor.GetMethodBody();
        if (body == null)
            return new Analysis(ConstructorKind.Trivial, new List<Store>());

        var ops = Decode(ctor, body.GetILAsByteArray());
        if (ops == null)
            return CallKind;
        ops.RemoveAll(o => o.op == OpCodes.Nop);

        var stores = new List<Store>();
        int i = 0;
        while (i < ops.Count)
        {
            var o = ops[i];
            if (o.op == OpCodes.Ret && i == ops.Count - 1)
            {
                i++;
                continue;
            }
            if (o.op != OpCodes.Ldarg_0 || i + 1 >= ops.Count)
                return CallKind;

            // ldarg.0; call <ctor>()
            if (ops[i + 1].op == OpCodes.Call)
            {
                MethodBase target;
                try { target = ctor.Module.ResolveMethod(ops[i + 1].token); }
                catch { return CallKind; }
                if (!IsInstanceConstructor(target) || target.GetParameters().Length != 0)
                    return CallKind;
                var inner = Analyze(target);
                if (inner.Kind == ConstructorKind.Call)
                    return CallKind;
                stores.AddRange(inner.Stores);
                i += 2;
                continue;
            }

            // ldarg.0; <constant>; [conv]; stfld f
            int j = i + 1;
            var value = ops[j++];
            bool widened = false;
            if (j < ops.Count && (ops[j].op == OpCodes.Conv_I8 || ops[j].op == OpCodes.Conv_U8))
            {
                widened = true;
                j++;
            }
            if (j >= ops.Count || ops[j].op != OpCodes.Stfld)
                return CallKind;
            FieldInfo field;
            try { field = ctor.Module.ResolveField(ops[j].token); }
            catch { return CallKind; }
            var bytes = ConstantBytes(value, widened, field.FieldType);
            if (bytes == null)
                return CallKind;
            stores.Add(new Store(field, bytes));
            i = j + 1;
        }

        if (stores.Count == 0)
            return new Analysis(ConstructorKind.Trivial, stores);
        // A parameter has to be pulled off the stack by real code.
        if (ctor.GetParameters().Length != 0)
            return CallKind;
        return new Analysis(ConstructorKind.ConstantInit, stores);
    }

    // The bytes a constant takes in a field of `type`, or null when it is
    // not a plain constant this can fold (a string, an out-of-range value:
    // the compiled constructor reports those the normal way).
    private static string[] ConstantBytes(OpCodeInfo value, bool widened, Type type)
    {
        if (value.op == OpCodes.Ldnull)
            return type.IsValueType ? null : new[] { "0" };
        if (!type.IsValueType || type == typeof(string))
            return null;

        if (value.op == OpCodes.Ldc_R4)
        {
            if (type != typeof(float) || widened)
                return null;
            try { return Mflpt.ToBytes(BitConverter.Int32BitsToSingle(value.token)).Select(b => b.ToString()).ToArray(); }
            catch { return null; }
        }
        if (type == typeof(float))
            return null;

        long v;
        if (value.op == OpCodes.Ldc_I8)
            v = value.long64;
        else if (!TryInt32Constant(value, out var iv))
            return null;
        else
            v = iv;

        int width = type.GetStorageBytes();
        if (width == 1)
        {
            if (widened || value.op == OpCodes.Ldc_I8 || v < -128 || v > 255)
                return null;
            return new[] { $"<{v}" };
        }
        if (width == 2)
        {
            if (v < -32768 || v > 65535)
                return null;
            return new[] { $"<{v}", $">{v}" };
        }
        return null;
    }

    private static bool TryInt32Constant(OpCodeInfo o, out int v)
    {
        v = 0;
        var op = o.op;
        if (op == OpCodes.Ldc_I4_M1) v = -1;
        else if (op == OpCodes.Ldc_I4_0) v = 0;
        else if (op == OpCodes.Ldc_I4_1) v = 1;
        else if (op == OpCodes.Ldc_I4_2) v = 2;
        else if (op == OpCodes.Ldc_I4_3) v = 3;
        else if (op == OpCodes.Ldc_I4_4) v = 4;
        else if (op == OpCodes.Ldc_I4_5) v = 5;
        else if (op == OpCodes.Ldc_I4_6) v = 6;
        else if (op == OpCodes.Ldc_I4_7) v = 7;
        else if (op == OpCodes.Ldc_I4_8) v = 8;
        else if (op == OpCodes.Ldc_I4_S) v = (sbyte)o.token;
        else if (op == OpCodes.Ldc_I4) v = o.token;
        else return false;
        return true;
    }

    private struct OpCodeInfo
    {
        public OpCode op;
        public int token;   // the 1/4-byte operand (token, constant)
        public long long64; // ldc.i8
    }

    private static List<OpCodeInfo> Decode(MethodBase method, byte[] il)
    {
        var result = new List<OpCodeInfo>();
        int index = 0;
        while (index < il.Length)
        {
            short value = il[index] == 0xFE && index + 1 < il.Length ? (short)(0xFE00 | il[index + 1]) : il[index];
            if (!OpCodesByValue.TryGetValue(value, out var op))
                return null;
            index += op.Size;
            var info = new OpCodeInfo { op = op };
            switch (op.OperandType)
            {
                case OperandType.InlineNone:
                    break;
                case OperandType.ShortInlineI:
                case OperandType.ShortInlineVar:
                case OperandType.ShortInlineBrTarget:
                    info.token = il[index];
                    index += 1;
                    break;
                case OperandType.InlineVar:
                    index += 2;
                    break;
                case OperandType.InlineI8:
                case OperandType.InlineR:
                    info.long64 = BitConverter.ToInt64(il, index);
                    index += 8;
                    break;
                case OperandType.InlineSwitch:
                    int n = BitConverter.ToInt32(il, index);
                    index += 4 + 4 * n;
                    break;
                default:
                    info.token = BitConverter.ToInt32(il, index);
                    index += 4;
                    break;
            }
            result.Add(info);
        }
        return result;
    }

    // --- which constructors are compiled ------------------------------------

    private static readonly Dictionary<Assembly, HashSet<MethodBase>> ChainedTo = new();

    // A Call constructor is always compiled; a ConstantInit one only when a
    // compiled constructor calls it (`: base()` / `: this()` from a
    // constructor with real work). Trivial ones never.
    public static bool IsCompiled(MethodBase ctor, Assembly assembly)
    {
        if (!IsInstanceConstructor(ctor) || ctor.DeclaringType?.Assembly != assembly)
            return false;
        if (typeof(Delegate).IsAssignableFrom(ctor.DeclaringType))
            return false;
        if (ctor.DeclaringType.IsGenericTypeDefinition || ctor.DeclaringType.ContainsGenericParameters)
            return false;
        var kind = Kind(ctor);
        if (kind == ConstructorKind.Call)
            return true;
        if (kind == ConstructorKind.Trivial)
            return false;
        return ChainTargets(assembly).Contains(Normalize(ctor));
    }

    private static MethodBase Normalize(MethodBase m)
    {
        try { return m.Module.ResolveMethod(m.MetadataToken) ?? m; }
        catch { return m; }
    }

    private static HashSet<MethodBase> ChainTargets(Assembly assembly)
    {
        lock (ChainedTo)
        {
            if (ChainedTo.TryGetValue(assembly, out var set))
                return set;
            set = new HashSet<MethodBase>();
            var work = new Stack<MethodBase>();
            foreach (var type in assembly.GetTypes())
                foreach (var c in type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                    if (Kind(c) == ConstructorKind.Call)
                        work.Push(c);
            var seen = new HashSet<MethodBase>();
            while (work.Count > 0)
            {
                var c = work.Pop();
                if (!seen.Add(Normalize(c)))
                    continue;
                var body = c.GetMethodBody();
                var ops = body == null ? null : Decode(c, body.GetILAsByteArray());
                if (ops == null)
                    continue;
                foreach (var o in ops.Where(o => o.op == OpCodes.Call))
                {
                    MethodBase target;
                    try { target = c.Module.ResolveMethod(o.token); }
                    catch { continue; }
                    if (!IsInstanceConstructor(target) || !IsProgramConstructor(target))
                        continue;
                    if (Kind(target) == ConstructorKind.ConstantInit)
                    {
                        // Compiled, it calls its own chained constructors
                        // as well, so follow it.
                        set.Add(Normalize(target));
                        work.Push(target);
                    }
                }
            }
            ChainedTo[assembly] = set;
            return set;
        }
    }

    // Instance constructors of `type` (declared there) that are compiled.
    public static IEnumerable<MethodBase> CompiledConstructors(Type type, Assembly assembly) =>
        type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Where(c => IsCompiled(c, assembly));

    // Argument bytes a newobj/call of this constructor has on the stack
    // (without `this`).
    public static int ArgumentBytes(MethodBase ctor) =>
        ctor.GetParameters().Sum(p => p.ParameterType.GetStorageBytes());

    // Records: Roslyn generates Equals/GetHashCode/ToString/PrintMembers
    // (StringBuilder, EqualityComparer<T>, typeof) and a virtual <Clone>$
    // for every record class. None of that is supported, and most of it is
    // never called, so say so once for the type instead of a list of
    // errors about members the program never wrote.
    public static bool IsRecord(Type type) =>
        type.GetMethod("<Clone>$", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly) != null;
}

// Removes a constructor's `ldarg.0; call Base::.ctor()` when the base
// constructor is not compiled (System.Object's, or a trivial one): the pair
// does nothing. A call with arguments is dropped by OpCall.Emit instead.
class ILTrivialConstructorCallPass : ICompilerMethodPass
{
    public void Execute(CompilerMethodContext context)
    {
        if (!ConstructorSupport.IsInstanceConstructor(context.Method) || context.Lines == null)
            return;
        var lines = context.Lines;
        for (int i = 1; i < lines.Count; i++)
        {
            var call = lines[i];
            var load = lines[i - 1];
            if (call.OpCode != System.Reflection.Metadata.ILOpCode.Call || call.Optimized || call.Label != null
                || load.OpCode != System.Reflection.Metadata.ILOpCode.Ldarg_0 || load.Optimized || load.Label != null)
                continue;
            MethodBase target;
            try { target = context.CompilerContext.Assembly.ManifestModule.ResolveMethod((int)call.OriginalParameter); }
            catch { continue; }
            if (target == null || !ConstructorSupport.IsInstanceConstructor(target) || target.GetParameters().Length != 0
                || ConstructorSupport.IsCompiled(target, context.CompilerContext.Assembly))
                continue;
            load.Optimized = true;
            call.Optimized = true;
        }
    }
}
