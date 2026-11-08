using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;

namespace Compiler;

// Where a static-frame method's parameters and locals live. The frame is the
// bytes [StaticFrames+Offset, StaticFrames+Offset+Size): the same layout a
// localsStack frame has (parameters on top, locals below them, no
// return-address bytes), shifted by RelShift so that rel_pos = dynamic
// rel_pos + RelShift and the byte is SF_TOP-rel_pos (asm/helper/
// localsStack.asm's ls_op; SF_TOP = StaticFrames + 512).
class StaticFrameInfo
{
    public int Offset { get; init; }
    public int Size { get; init; }
    public int ParameterBytes { get; init; }
    public int ReturnBytes { get; init; }
    // Per parameter byte, in the order the caller pulls them (the last
    // argument's low byte first, `this` last): is it a reference to count?
    public bool[] ParameterRefBytes { get; init; }
    // The other calling convention (see ILStaticFramePass.ChooseCalleePulls):
    // the callee itself pulls its return address and arguments off the
    // hardware stack into its frame, and returns through the saved address
    // with the result left on the stack -- no per-call-site code at all.
    // The two return-address bytes are the bottom of the frame.
    public bool CalleePulls { get; init; }
    public int RelShift => 512 - Offset - Size;
    // rel_pos of the saved return address's low byte (CalleePulls only).
    public int ReturnAddressRel => RelShift + Size;
}

// Static (compiled) frames, as in cc65 --static-locals, KickC, SDCC overlays
// and Oscar64: a method that can never be active twice at once does not
// need a stack frame. Its parameters and locals get fixed addresses, so it
// needs no prologue (init_locals_pull_parameters: ~23 bytes, 40-70 cycles),
// it returns with a plain rts, and every local access is an absolute
// lda/sta with no `ldy stackPointer`.
//
// Whole-program pass, run before ILCodePass. It builds the call graph from
// the IL of every compiled method and gives a method a static frame when:
//   - it is not part of a cycle (recursion), so one activation at a time;
//   - it is not a delegate target (Ldftn: C64.Interrupt handlers, Func<T>)
//     and not reachable from one: those run when the library decides (an
//     interrupt can arrive in the middle of any method), which the call
//     graph cannot see;
//   - it is not a virtual method (dispatch through the vtable jsr's it with
//     the normal convention) and not a unit-test entry point (the test
//     harness jumps to it by label with the arguments on the stack);
//   - it returns nothing or a 1-2 byte value (passed back in registers);
//     at level 1 (--static-frames=1) also only a static method with no
//     reference-counted parameter, local or return value. Instance methods
//     keep `this` in their frame; a reference parameter is counted by the
//     caller as it stores it (like the prologue would), reference locals
//     are zeroed on entry (sf_enter) and everything is released at the
//     exit (sf_exit), exactly as a localsStack frame does.
// Frames are overlaid along the call graph: a method's frame starts after
// the frame end of every method that can be active below it (any caller,
// static or not, transitively), so two frames that can be live at once
// never share a byte while unrelated methods reuse the same bytes.
//
// Calling convention (OpCall.Emit / OpRet): the caller evaluates the
// arguments onto the hardware stack as usual, then right before the jsr
// pulls them into the callee's parameter slots (so a call nested in an
// argument expression has already finished with its own frame). The callee
// keeps the return address on the hardware stack; a return value comes back
// in A/X (sf_ret) and the caller pushes it (sf_result).
class ILStaticFramePass : ICompilerPass
{
    // Rel_pos values must stay in 256..511 (see ls_op), so the overlaid
    // frames can span at most 256 bytes. Real programs need a few dozen.
    private const int MaxTotalSize = 256;

    public void Execute(CompilerContext context)
    {
        // The TestDebugger's local-variable inspector reads every local from
        // localsStack; debug builds keep the dynamic frames.
        if (context.StaticFramesLevel <= 0 || context.EmitDebugInfo)
            return;

        var methods = EnumerateCompiledMethods(context).Select(Normalize).Distinct().ToList();
        var methodSet = new HashSet<MethodBase>(methods);
        var callees = new Dictionary<MethodBase, HashSet<MethodBase>>();
        var callbackRoots = new HashSet<MethodBase>();
        var undecodable = new HashSet<MethodBase>();
        var siteCounts = new Dictionary<MethodBase, int>();

        foreach (var method in methods)
        {
            var targets = new HashSet<MethodBase>();
            callees[method] = targets;
            List<ILOperation> lines;
            try
            {
                var scratch = new CompilerMethodContext
                {
                    CompilerContext = context,
                    Method = method,
                    Lines = new List<ILOperation>(),
                };
                ILMethodCodePass.Decode(scratch);
                lines = scratch.Lines;
            }
            catch
            {
                // ILCodePass reports it; just keep this one dynamic.
                undecodable.Add(method);
                continue;
            }

            foreach (var line in lines)
            {
                // Newobj calls a constructor that does real work
                // (ConstructorSupport); for the others the target is not
                // a compiled method and drops out below.
                if (line.OpCode != ILOpCode.Call && line.OpCode != ILOpCode.Callvirt
                    && line.OpCode != ILOpCode.Ldftn && line.OpCode != ILOpCode.Ldvirtftn
                    && line.OpCode != ILOpCode.Newobj)
                    continue;
                MethodBase target;
                try
                {
                    target = context.Assembly.ManifestModule.ResolveMethod((int)line.OriginalParameter);
                }
                catch
                {
                    continue;
                }
                if (target == null)
                    continue;
                target = Normalize(target);

                var resolved = new List<MethodBase>();
                if (methodSet.Contains(target))
                    resolved.Add(target);
                if (line.OpCode == ILOpCode.Callvirt || line.OpCode == ILOpCode.Ldvirtftn)
                    resolved.AddRange(Overrides(methods, target));

                if (line.OpCode == ILOpCode.Ldftn || line.OpCode == ILOpCode.Ldvirtftn)
                    callbackRoots.UnionWith(resolved);
                else
                {
                    targets.UnionWith(resolved);
                    siteCounts[target] = siteCounts.GetValueOrDefault(target) + 1;
                }
            }
        }

        // Everything a delegate target can reach.
        var callbackReachable = Reachable(callees, callbackRoots);

        // Strongly connected components (Tarjan), produced in reverse
        // topological order of the condensation.
        var sccs = StronglyConnected(methods, callees);
        var sccOf = new Dictionary<MethodBase, int>();
        for (int i = 0; i < sccs.Count; i++)
            foreach (var m in sccs[i])
                sccOf[m] = i;

        var candidates = new Dictionary<MethodBase, (int size, int paramBytes, int returnBytes)>();
        foreach (var method in methods)
        {
            var scc = sccs[sccOf[method]];
            bool recursive = scc.Count > 1 || callees[method].Contains(method);
            var reason = Ineligible(context, method, recursive, callbackReachable.Contains(method), undecodable.Contains(method),
                out var size, out var paramBytes, out var returnBytes);
            if (reason != null)
                context.StaticFrameRejections[method] = reason;
            else
                candidates[method] = (size, paramBytes, returnBytes);
        }

        // Overlay: walk the condensation in topological order (callers
        // first). start = the highest frame end of anything that can be
        // active below; end = start + own size for a static frame.
        var callers = new Dictionary<int, HashSet<int>>();
        foreach (var (caller, set) in callees)
            foreach (var callee in set)
            {
                int a = sccOf[caller], b = sccOf[callee];
                if (a == b)
                    continue;
                if (!callers.TryGetValue(b, out var preds))
                    callers[b] = preds = new HashSet<int>();
                preds.Add(a);
            }

        var end = new int[sccs.Count];
        int total = 0;
        for (int i = sccs.Count - 1; i >= 0; i--)
        {
            int start = 0;
            if (callers.TryGetValue(i, out var preds))
                foreach (var p in preds)
                    start = Math.Max(start, end[p]);
            end[i] = start;

            if (sccs[i].Count != 1 || !candidates.TryGetValue(sccs[i][0], out var c))
                continue;
            var method = sccs[i][0];
            bool calleePulls = ChooseCalleePulls(c.paramBytes, c.returnBytes, siteCounts.GetValueOrDefault(method));
            int size = c.size + (calleePulls ? 2 : 0);
            if (start + size > MaxTotalSize)
            {
                context.StaticFrameRejections[method] = "static frame area full";
                continue;
            }
            context.StaticFrames[method] = new StaticFrameInfo
            {
                Offset = start,
                Size = size,
                ParameterBytes = c.paramBytes,
                ReturnBytes = c.returnBytes,
                ParameterRefBytes = ParameterRefBytes(method),
                CalleePulls = calleePulls,
            };
            end[i] = start + size;
            total = Math.Max(total, end[i]);
        }
        context.StaticFramesSize = total;

        WriteSummary(context, methods.Count);
    }

    private static string Ineligible(CompilerContext context, MethodBase method, bool recursive, bool callbackReachable, bool undecodable,
        out int size, out int paramBytes, out int returnBytes)
    {
        size = paramBytes = returnBytes = 0;
        if (method.IsVirtual)
            return "virtual";
        if (undecodable)
            return "not compilable";
        // An instance constructor (ConstructorSupport) returns nothing; a
        // static one runs once at startup, nothing to gain.
        if (method is ConstructorInfo && method.IsStatic)
            return "constructor";
        if (method.IsAbstract || method.GetMethodBody() == null)
            return "no body";
        if (method.IsGenericMethod || method.ContainsGenericParameters)
            return "generic";
        if (recursive)
            return "recursive";
        if (callbackReachable)
            return "delegate/interrupt reachable";
        if (method.IsVirtual)
            return "virtual";
        if (context.IsUnitTest && IsTestEntryPoint(method))
            return "test entry point";
        bool phase1 = context.StaticFramesLevel < 2;
        if (phase1 && !method.IsStatic)
            return "instance method";

        foreach (var p in method.GetParameters())
        {
            if (phase1 && p.ParameterType.IsReferenceCounted())
                return "reference parameter";
            paramBytes += p.ParameterType.GetStorageBytes();
        }
        if (!method.IsStatic)
            paramBytes += 1; // this
        int localBytes = 0;
        foreach (var l in method.GetMethodBody().LocalVariables)
        {
            if (phase1 && l.LocalType.IsReferenceCounted())
                return "reference local";
            localBytes += l.LocalType.GetStorageBytes();
        }

        var ret = (method as MethodInfo)?.ReturnType ?? typeof(void);
        if (ret != typeof(void))
        {
            if (phase1 && ret.IsReferenceCounted())
                return "reference return";
            returnBytes = ret.GetStorageBytes();
        }
        size = paramBytes + localBytes;
        return null;
    }

    // Which convention a static frame uses. Caller-stores (the default):
    // every call site pulls the arguments into the callee's slots (or
    // copies them there directly, ILStaticCallPass) and pushes the result
    // that comes back in A/X -- the fastest, and free for a method without
    // parameters or result. Callee-pulls: the callee saves its return
    // address and pulls its own arguments (like the localsStack prologue,
    // ~15 bytes + 4 per parameter byte, once) and the result stays on the
    // stack -- smaller when many call sites pass many bytes, and the only
    // way to return a float. Estimated per-site cost of caller-stores over
    // callee-pulls: ~3 bytes per parameter byte (2 for a direct copy, 4 for
    // a pull) plus the result push.
    internal static int CalleePullsBias =
        int.TryParse(Environment.GetEnvironmentVariable("C64_SF_BIAS"), out var b) ? b : 0;

    private static bool ChooseCalleePulls(int paramBytes, int returnBytes, int sites)
    {
        if (returnBytes > 2)
            return true;
        if (paramBytes == 0)
            return false;
        int resultPush = returnBytes == 1 ? 1 : returnBytes == 2 ? 3 : 0;
        return sites * (3 * paramBytes + resultPush) > 15 + 4 * paramBytes + CalleePullsBias;
    }

    // ParameterRefBytes: the bytes in the order the caller pulls them,
    // flattened from GetParameterRefList (the prologue's own pull list).
    private static bool[] ParameterRefBytes(MethodBase method) =>
        method.GetParameterRefList()
            .SelectMany(entry => entry.Split(',').Select(x => x.Trim() == "1"))
            .ToArray();

    // The test harness (Compiler/Templates/UnitTestEntry.asm) jumps to a
    // test method by its label with the arguments pushed on the stack.
    private static bool IsTestEntryPoint(MethodBase method) =>
        method.GetCustomAttributesData().Any(a => a.AttributeType.Namespace != null && a.AttributeType.Namespace.StartsWith("NUnit"));

    private static IEnumerable<MethodBase> Overrides(List<MethodBase> methods, MethodBase target)
    {
        var type = target.DeclaringType;
        if (type == null)
            yield break;
        foreach (var m in methods)
            if (m.IsVirtual && m.Name == target.Name && m.DeclaringType != null
                && (type.IsAssignableFrom(m.DeclaringType) || m.DeclaringType.IsAssignableFrom(type)))
                yield return m;
    }

    private static HashSet<MethodBase> Reachable(Dictionary<MethodBase, HashSet<MethodBase>> callees, IEnumerable<MethodBase> roots)
    {
        var seen = new HashSet<MethodBase>();
        var work = new Stack<MethodBase>(roots);
        while (work.Count > 0)
        {
            var m = work.Pop();
            if (!seen.Add(m))
                continue;
            if (callees.TryGetValue(m, out var next))
                foreach (var n in next)
                    work.Push(n);
        }
        return seen;
    }

    private static List<List<MethodBase>> StronglyConnected(List<MethodBase> nodes, Dictionary<MethodBase, HashSet<MethodBase>> edges)
    {
        var index = new Dictionary<MethodBase, int>();
        var low = new Dictionary<MethodBase, int>();
        var onStack = new HashSet<MethodBase>();
        var stack = new Stack<MethodBase>();
        var result = new List<List<MethodBase>>();
        int counter = 0;

        void Visit(MethodBase v)
        {
            index[v] = low[v] = counter++;
            stack.Push(v);
            onStack.Add(v);
            foreach (var w in edges[v])
            {
                if (!index.ContainsKey(w))
                {
                    Visit(w);
                    low[v] = Math.Min(low[v], low[w]);
                }
                else if (onStack.Contains(w))
                    low[v] = Math.Min(low[v], index[w]);
            }
            if (low[v] == index[v])
            {
                var scc = new List<MethodBase>();
                MethodBase w;
                do
                {
                    w = stack.Pop();
                    onStack.Remove(w);
                    scc.Add(w);
                } while (w != v);
                result.Add(scc);
            }
        }

        foreach (var n in nodes)
            if (!index.ContainsKey(n))
                Visit(n);
        return result;
    }

    // One identity per method: a MethodInfo obtained through a derived
    // type's GetMethods() differs from the one ResolveMethod returns.
    public static MethodBase Normalize(MethodBase method)
    {
        try
        {
            return method.Module.ResolveMethod(method.MetadataToken) ?? method;
        }
        catch
        {
            return method;
        }
    }

    private static IEnumerable<MethodBase> EnumerateCompiledMethods(CompilerContext context)
    {
        foreach (var type in context.Assembly.GetTypes())
        {
            if (type.IsValueType)
                continue;
            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly).OfType<MethodBase>();
            var staticConstructors = type.GetConstructors(BindingFlags.Static | BindingFlags.NonPublic).OfType<MethodBase>()
                .Concat(ConstructorSupport.CompiledConstructors(type, context.Assembly));
            foreach (var method in methods.Concat(staticConstructors))
            {
                if (method.DeclaringType == null || method.DeclaringType.Assembly != context.Assembly)
                    continue;
                if (!method.IsAbstract && method.GetMethodBody() == null)
                    continue;
                yield return method;
            }
        }
    }

    private static void WriteSummary(CompilerContext context, int methodCount)
    {
        var output = context.GlobalOutputFile;
        output.WriteLine($"; Static frames (Compiler/ILStaticFramePass.cs): {context.StaticFrames.Count} of {methodCount} methods, {context.StaticFramesSize} bytes");
        output.WriteLine($"StaticFramesSize = {context.StaticFramesSize}");
        foreach (var (method, info) in context.StaticFrames.OrderBy(x => x.Value.Offset).ThenBy(x => x.Key.GetLabel()))
            output.WriteLine($";   {method.GetLabel()}: offset {info.Offset}, {info.Size} bytes{(info.CalleePulls ? ", callee pulls" : "")}");
        foreach (var group in context.StaticFrameRejections.GroupBy(x => x.Value).OrderByDescending(g => g.Count()))
            output.WriteLine($"; dynamic ({group.Key}): {group.Count()}: {string.Join(", ", group.Select(x => x.Key.GetLabel()).OrderBy(x => x))}");
        Console.WriteLine($"Static frames: {context.StaticFrames.Count} of {methodCount} methods ({context.StaticFrames.Values.Count(f => f.CalleePulls)} callee-pulls), {context.StaticFramesSize} bytes; dynamic: "
            + string.Join(", ", context.StaticFrameRejections.GroupBy(x => x.Value).OrderByDescending(g => g.Count()).Select(g => $"{g.Key} {g.Count()}")));
    }
}
