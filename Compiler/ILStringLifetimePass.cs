using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;

namespace Compiler;

// Run-time strings live in one shared scratch buffer each: String_Concat and
// String_PadLeft write into stringops_buffer, every NumberFormat_*ToString
// (and so every interpolation hole) into tostring_buffer (asm/helper/
// objectTables.asm). A string here is a raw 2-byte pointer to null-terminated
// text, not a heap object, so the result of `"Slide " + n` is simply the
// address of stringops_buffer: the next concatenation anywhere in the
// program rewrites the text behind it. Used right away (drawn, compared,
// concatenated further) that is fine; kept in a field, a static, an array
// element or a closure, it silently turns into whatever was built last.
//
// Copying such a string to the heap when it escapes is not possible without
// changing what a string is: heap objects are 1-byte handles whose data the
// garbage collector moves, while a string field holds a fixed address (a
// literal's, usually). A ring of buffers only makes the failure rarer. So
// this pass rejects the store at compile time (C64007), at no run-time cost.
//
// A whole-program data-flow check over the IL (before ILCodePass, which
// throws the collected diagnostics together with its own):
//   - a value is "built at run time" when it comes from string.Concat,
//     string.PadLeft, a class-library ToString() (numbers), an interpolated
//     string, or a program method that can return such a value (directly or
//     by returning one of its parameters that was given one);
//   - locals and parameters are tracked flow-insensitively (once a local
//     holds such a string anywhere, every load of it may);
//   - a sink is stfld/stsfld (auto-property setters are decoded as stfld),
//     stelem*, stind.ref/stobj, and an argument to a parameter that the
//     callee itself stores into a sink ("escapes"), found by iterating the
//     per-method summaries to a fixpoint (virtual calls take every override
//     in the program into account).
// Not covered: a local holding a run-time string across a later
// concatenation or ToString() (`var a = x + "1"; var b = y + "2"; Write(a)`)
// (documented in Catacombs/TECH.md, "Text"); a value that only reaches a
// sink through a Func<T> invocation.
class ILStringLifetimePass : ICompilerPass
{
    private static readonly Dictionary<short, OpCode> OpCodesByValue = typeof(OpCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Select(f => (OpCode)f.GetValue(null))
        .GroupBy(o => o.Value)
        .ToDictionary(g => g.Key, g => g.First());

    private struct Val
    {
        public bool Dyn;
        public ulong Params;
        public static Val operator |(Val a, Val b) => new Val { Dyn = a.Dyn || b.Dyn, Params = a.Params | b.Params };
        public bool Same(Val o) => Dyn == o.Dyn && Params == o.Params;
    }

    private class Summary
    {
        public bool ReturnsDyn;
        public ulong ReturnParams;
        public ulong EscapingParams;
        public bool Same(Summary o) => ReturnsDyn == o.ReturnsDyn && ReturnParams == o.ReturnParams && EscapingParams == o.EscapingParams;
    }

    private readonly Dictionary<MethodBase, Summary> _summaries = new();
    private readonly Dictionary<MethodBase, List<ILOperation>> _bodies = new();
    private CompilerContext _context;
    private Module _module;

    public void Execute(CompilerContext context)
    {
        _context = context;
        _module = context.Assembly.ManifestModule;

        // Decoding registers string literals (and so on) in the context it
        // is given: a throwaway one, so dead methods add nothing.
        var decodeContext = new CompilerContext
        {
            Assembly = context.Assembly,
            GlobalOutputFile = new System.IO.StreamWriter(System.IO.Stream.Null),
            OutputDirectory = context.OutputDirectory,
            IsUnitTest = context.IsUnitTest,
            Methods = new List<CompilerMethodContext>(),
        };
        foreach (var method in ProgramMethods(context).Select(ILStaticFramePass.Normalize).Distinct())
        {
            try
            {
                var scratch = new CompilerMethodContext
                {
                    CompilerContext = decodeContext,
                    Method = method,
                    Lines = new List<ILOperation>(),
                };
                ILMethodCodePass.Decode(scratch);
                if (scratch.Lines.Any(l => l.OpCode == ILOpCode.Calli))
                    continue;
                _bodies[method] = scratch.Lines;
                _summaries[method] = new Summary();
            }
            catch
            {
                // ILCodePass reports whatever is wrong with it.
            }
        }

        for (int round = 0; round < 50; round++)
        {
            bool changed = false;
            foreach (var (method, lines) in _bodies)
            {
                var summary = Analyze(method, lines, null);
                if (summary != null && !summary.Same(_summaries[method]))
                {
                    _summaries[method] = summary;
                    changed = true;
                }
            }
            if (!changed)
                break;
        }

        foreach (var (method, lines) in _bodies)
            Analyze(method, lines, (offset, message) => Report(method, offset, message));
    }

    private void Report(MethodBase method, int offset, string message)
    {
        var location = _context.SourceLocator.Locate(method, offset);
        _context.Diagnostics.Add(new CompilerDiagnostic
        {
            Code = DiagnosticCodes.RunTimeStringEscapes,
            Message = message + " A string built at run time (by +, an interpolated string, ToString() or PadLeft) lives in a scratch buffer shared by the whole program, so the next one built overwrites it, and the stored copy would later show different text.",
            Hint = "Store only string literals (pick one from a constant string array, or store the number and build the text where it is drawn, e.g. Write(x, y, \"Level \" + level.ToString())).",
            File = location?.file,
            Line = location?.line ?? 0,
            Method = $"{method.DeclaringType?.Name}.{method.Name}",
        });
    }

    private static IEnumerable<MethodBase> ProgramMethods(CompilerContext context)
    {
        const BindingFlags all = BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly;
        Type[] types;
        try { types = context.Assembly.GetTypes(); }
        catch { yield break; }
        foreach (var type in types)
        {
            if (type.IsGenericTypeDefinition)
                continue;
            foreach (var m in type.GetMethods(all).OfType<MethodBase>().Concat(type.GetConstructors(all)))
            {
                if (m.IsAbstract || m.ContainsGenericParameters || m.GetMethodBody() == null)
                    continue;
                yield return m;
            }
        }
    }

    // Is this a call whose result is a run-time string in a shared buffer?
    private static bool ProducesBufferString(MethodBase callee, CompilerContext context)
    {
        if (callee is not MethodInfo info || info.ReturnType != typeof(string))
            return false;
        var declaring = callee.DeclaringType;
        if (declaring == null || declaring.Assembly == context.Assembly)
            return false;
        if (declaring == typeof(string))
            return callee.Name is nameof(string.Concat) or nameof(string.PadLeft) or nameof(string.PadRight)
                or nameof(string.Format) or nameof(string.Join) or nameof(string.Substring);
        if (callee.Name == nameof(ToString))
            return true;
        return declaring.FullName == "System.Runtime.CompilerServices.DefaultInterpolatedStringHandler"
            && callee.Name == "ToStringAndClear";
    }

    // The program methods a call can reach: the target, and for a virtual
    // call every override of it in the program.
    private IEnumerable<Summary> Callees(MethodBase target, bool virtualCall)
    {
        target = ILStaticFramePass.Normalize(target);
        if (_summaries.TryGetValue(target, out var s))
            yield return s;
        if (!virtualCall || !target.IsVirtual || target.DeclaringType == null)
            yield break;
        foreach (var (m, summary) in _summaries)
        {
            if (m != target && m.IsVirtual && m.Name == target.Name && m.DeclaringType != null
                && target.DeclaringType.IsAssignableFrom(m.DeclaringType)
                && m.GetParameters().Length == target.GetParameters().Length)
                yield return summary;
        }
    }

    private static string Describe(MethodBase m) =>
        m.IsConstructor ? $"the constructor of '{m.DeclaringType?.Name}'" : $"'{m.DeclaringType?.Name}.{m.Name}'";

    // One method under the current summaries: returns its new summary;
    // with `report`, also reports every store of a run-time string.
    private Summary Analyze(MethodBase method, List<ILOperation> lines, Action<int, string> report)
    {
        var body = method.GetMethodBody();
        int argCount = method.GetParameters().Length + (method.IsStatic ? 0 : 1);
        var locals = new Val[body.LocalVariables.Count];
        var args = new Val[argCount];
        var result = new Summary();
        var byOffset = new Dictionary<int, int>();
        for (int i = 0; i < lines.Count; i++)
            byOffset[lines[i].Position] = i;
        var reported = new HashSet<int>();

        void Sink(Val v, int offset, string what)
        {
            result.EscapingParams |= v.Params;
            if (v.Dyn && report != null && reported.Add(offset))
                report(offset, what);
        }

        bool localsChanged = true;
        for (int pass = 0; pass < 20 && localsChanged; pass++)
        {
            localsChanged = false;
            var entry = new List<Val>[lines.Count];
            var work = new Stack<int>();
            void Flow(int index, List<Val> stack)
            {
                if (index < 0 || index >= lines.Count)
                    return;
                if (entry[index] == null)
                {
                    entry[index] = new List<Val>(stack);
                    work.Push(index);
                    return;
                }
                var e = entry[index];
                bool changed = false;
                for (int k = 0; k < Math.Min(e.Count, stack.Count); k++)
                {
                    var merged = e[e.Count - 1 - k] | stack[stack.Count - 1 - k];
                    if (!merged.Same(e[e.Count - 1 - k]))
                    {
                        e[e.Count - 1 - k] = merged;
                        changed = true;
                    }
                }
                if (changed)
                    work.Push(index);
            }

            Flow(0, new List<Val>());
            // Exception handlers are not supported; their entries just start empty.
            foreach (var clause in body.ExceptionHandlingClauses)
            {
                try
                {
                    if (byOffset.TryGetValue(clause.HandlerOffset, out var h))
                        Flow(h, clause.Flags == ExceptionHandlingClauseOptions.Clause ? new List<Val> { default } : new List<Val>());
                }
                catch { }
            }

            while (work.Count > 0)
            {
                int i = work.Pop();
                var line = lines[i];
                var stack = new List<Val>(entry[i]);
                if (!OpCodesByValue.TryGetValue((short)line.OpCode, out var op))
                    return null;

                Val Pop()
                {
                    if (stack.Count == 0)
                        return default;
                    var v = stack[^1];
                    stack.RemoveAt(stack.Count - 1);
                    return v;
                }

                void SetLocal(int index, Val v)
                {
                    if (index < 0 || index >= locals.Length)
                        return;
                    var merged = locals[index] | v;
                    if (!merged.Same(locals[index]))
                    {
                        locals[index] = merged;
                        localsChanged = true;
                    }
                }

                void SetArg(int index, Val v)
                {
                    if (index < 0 || index >= args.Length)
                        return;
                    var merged = args[index] | v;
                    if (!merged.Same(args[index]))
                    {
                        args[index] = merged;
                        localsChanged = true;
                    }
                }

                Val LoadArg(int index) =>
                    index < 0 || index >= args.Length ? default : args[index] | new Val { Params = index < 64 ? 1UL << index : 0 };

                int token = line.OriginalParameter is int t ? t : 0;
                bool handled = true;
                switch (line.OpCode)
                {
                    case ILOpCode.Ldarg_0: stack.Add(LoadArg(0)); break;
                    case ILOpCode.Ldarg_1: stack.Add(LoadArg(1)); break;
                    case ILOpCode.Ldarg_2: stack.Add(LoadArg(2)); break;
                    case ILOpCode.Ldarg_3: stack.Add(LoadArg(3)); break;
                    case ILOpCode.Ldarg_s:
                    case ILOpCode.Ldarg: stack.Add(LoadArg(token)); break;
                    case ILOpCode.Starg_s:
                    case ILOpCode.Starg: SetArg(token, Pop()); break;
                    case ILOpCode.Ldloc_0: stack.Add(locals.Length > 0 ? locals[0] : default); break;
                    case ILOpCode.Ldloc_1: stack.Add(locals.Length > 1 ? locals[1] : default); break;
                    case ILOpCode.Ldloc_2: stack.Add(locals.Length > 2 ? locals[2] : default); break;
                    case ILOpCode.Ldloc_3: stack.Add(locals.Length > 3 ? locals[3] : default); break;
                    case ILOpCode.Ldloc_s:
                    case ILOpCode.Ldloc: stack.Add(token >= 0 && token < locals.Length ? locals[token] : default); break;
                    case ILOpCode.Stloc_0: SetLocal(0, Pop()); break;
                    case ILOpCode.Stloc_1: SetLocal(1, Pop()); break;
                    case ILOpCode.Stloc_2: SetLocal(2, Pop()); break;
                    case ILOpCode.Stloc_3: SetLocal(3, Pop()); break;
                    case ILOpCode.Stloc_s:
                    case ILOpCode.Stloc: SetLocal(token, Pop()); break;
                    case ILOpCode.Dup:
                        {
                            var v = Pop();
                            stack.Add(v);
                            stack.Add(v);
                            break;
                        }
                    case ILOpCode.Castclass:
                        break; // passes the value through
                    case ILOpCode.Stfld:
                    case ILOpCode.Stsfld:
                        {
                            var v = Pop();
                            if (line.OpCode == ILOpCode.Stfld)
                                Pop();
                            string name = "a field";
                            try { var f = _module.ResolveField(token); name = $"the field '{f.DeclaringType?.Name}.{f.Name}'"; } catch { }
                            Sink(v, line.Position, $"A string built at run time is stored in {name}.");
                            break;
                        }
                    case ILOpCode.Stelem:
                    case ILOpCode.Stelem_ref:
                    case ILOpCode.Stelem_i:
                    case ILOpCode.Stelem_i1:
                    case ILOpCode.Stelem_i2:
                    case ILOpCode.Stelem_i4:
                    case ILOpCode.Stelem_i8:
                    case ILOpCode.Stelem_r4:
                    case ILOpCode.Stelem_r8:
                        {
                            var v = Pop();
                            Pop();
                            Pop();
                            Sink(v, line.Position, "A string built at run time is stored in an array element.");
                            break;
                        }
                    case ILOpCode.Stind_ref:
                    case ILOpCode.Stobj:
                        {
                            var v = Pop();
                            Pop();
                            Sink(v, line.Position, "A string built at run time is stored through a reference.");
                            break;
                        }
                    case ILOpCode.Ret:
                        if (method is MethodInfo mi && mi.ReturnType != typeof(void))
                        {
                            var v = Pop();
                            result.ReturnsDyn |= v.Dyn;
                            result.ReturnParams |= v.Params;
                        }
                        break;
                    case ILOpCode.Call:
                    case ILOpCode.Callvirt:
                    case ILOpCode.Newobj:
                        {
                            MethodBase callee;
                            try { callee = _module.ResolveMethod(token); }
                            catch { return null; }
                            int n = callee.GetParameters().Length;
                            bool hasThis = !callee.IsStatic && line.OpCode != ILOpCode.Newobj;
                            int count = n + (hasThis ? 1 : 0);
                            var callArgs = new Val[count];
                            for (int k = count - 1; k >= 0; k--)
                                callArgs[k] = Pop();
                            // Argument k binds to the callee's arg index k (+1 for
                            // a constructor, whose arg 0 is the new object).
                            int shift = line.OpCode == ILOpCode.Newobj ? 1 : 0;
                            var ret = new Val { Dyn = ProducesBufferString(callee, _context) };
                            foreach (var s in Callees(callee, line.OpCode == ILOpCode.Callvirt))
                            {
                                for (int k = 0; k < count; k++)
                                {
                                    int bit = k + shift;
                                    if (bit >= 64)
                                        continue;
                                    if ((s.EscapingParams & (1UL << bit)) != 0)
                                    {
                                        var name = callee.GetParameters().ElementAtOrDefault(bit - (callee.IsStatic ? 0 : 1))?.Name;
                                        Sink(callArgs[k], line.Position,
                                            $"A string built at run time is passed to {Describe(callee)}, which keeps it (stores its parameter '{name}' in a field, a static or an array).");
                                    }
                                    if ((s.ReturnParams & (1UL << bit)) != 0)
                                        ret |= callArgs[k];
                                }
                                ret.Dyn |= s.ReturnsDyn;
                            }
                            if (line.OpCode == ILOpCode.Newobj)
                                stack.Add(default);
                            else if (callee is MethodInfo cmi && cmi.ReturnType != typeof(void))
                                stack.Add(ret);
                            break;
                        }
                    default:
                        handled = false;
                        break;
                }

                if (!handled)
                {
                    int pops = PopCount(op.StackBehaviourPop, method);
                    if (pops < 0)
                        return null;
                    for (int k = 0; k < pops; k++)
                        Pop();
                    int pushes = PushCount(op.StackBehaviourPush);
                    for (int k = 0; k < pushes; k++)
                        stack.Add(default);
                }

                // Successors.
                int next = i + 1;
                switch (op.FlowControl)
                {
                    case FlowControl.Return:
                    case FlowControl.Throw:
                        break;
                    case FlowControl.Branch:
                    case FlowControl.Cond_Branch:
                        if (line.OpCode == ILOpCode.Switch)
                        {
                            if (line.RawParameter is List<int> targets)
                                foreach (var target in targets)
                                    if (byOffset.TryGetValue(line.Position + line.Size + target, out var ti))
                                        Flow(ti, stack);
                        }
                        else
                        {
                            int delta = op.OperandType == OperandType.ShortInlineBrTarget ? (sbyte)(byte)token : token;
                            if (byOffset.TryGetValue(line.Position + line.Size + delta, out var ti))
                                Flow(ti, stack);
                        }
                        if (op.FlowControl == FlowControl.Cond_Branch)
                            Flow(next, stack);
                        break;
                    default:
                        Flow(next, stack);
                        break;
                }
            }
        }
        return result;
    }

    private static int PopCount(StackBehaviour b, MethodBase method)
    {
        if (b == StackBehaviour.Varpop)
            return -1;
        if (b == StackBehaviour.Pop0)
            return 0;
        return b.ToString().Split('_').Length;
    }

    private static int PushCount(StackBehaviour b) => b switch
    {
        StackBehaviour.Push0 => 0,
        StackBehaviour.Push1_push1 => 2,
        StackBehaviour.Varpush => 1,
        _ => 1,
    };
}
