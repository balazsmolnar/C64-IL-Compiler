using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

// Object initializers that Roslyn spills into a temporary local, back on the
// stack. An initializer whose evaluation might need the stack (a collection
// expression, `Items = [..]`, inside an element of another collection
// expression -- every slide's `Elements = [ new BulletList { .. } ]`) is
// compiled as
//
//   newobj T; stloc t; ldloc t; X1; stfld A; ldloc t; X2; stfld B; ldloc t
//
// instead of the `newobj T; dup; X1; stfld A; dup; X2; stfld B` it uses
// everywhere else. The temporary costs a counted store (#locals_pull_value8
// with its root count), a load per field, the final load, and a slot in the
// method's prologue zeroing and exit release list; it also hides the
// constant stores from ILObjectInitializerOptimizer, which only folds the
// dup form. This pass rewrites a window like that into the dup form (stloc
// and the final ldloc go, each field's ldloc becomes a dup), when
//   - every use of the local in the method is inside such windows, so the
//     local is never live anywhere else (it is then dropped from the frame's
//     reference list, CompilerMethodContext.DeadLocals);
//   - each window is straight-line code (no branch target, no branch) and
//     its field values X do not use the local;
//   - nothing in the X's can reach GC.Collect (MayCollect: no dynamic call,
//     no call that leads there): the object is on the evaluation stack only,
//     not counted as a root, like every dup-form initializer.
// Runs after ILMethodBuildEvaluationStackPass (the windows are found by
// stack depth) and rebuilds the stack contents when it changed anything.
class ILInitializerTempPass : ICompilerMethodPass
{
    public void Execute(CompilerMethodContext context)
    {
        if (!context.CompilerContext.Optimize || context.CompilerContext.EmitDebugInfo || context.Method.IsAbstract)
            return;
        var lines = context.Lines;
        // Inlined callee lines index the callee's locals: leave such methods alone.
        if (lines.Count == 0 || lines.Any(l => l.SourceMethod != null || l.StackContent == null))
            return;

        var body = context.Method.GetMethodBody();
        if (body == null)
            return;

        bool changed = false;
        var removed = new List<ILOperation>();
        for (int local = 0; local < body.LocalVariables.Count; local++)
        {
            var uses = new List<int>();
            for (int i = 0; i < lines.Count; i++)
                if (LocalOf(lines[i]) == local)
                    uses.Add(i);
            if (uses.Count == 0)
                continue;

            var windows = new List<(int store, List<int> dups, int last)>();
            var covered = new HashSet<int>();
            bool ok = true;
            foreach (var u in uses)
            {
                if (covered.Contains(u))
                    continue;
                if (!IsStore(lines[u]) || !TryWindow(context, lines, u, local, out var window))
                {
                    ok = false;
                    break;
                }
                windows.Add(window);
                covered.Add(window.store);
                covered.UnionWith(window.dups);
                covered.Add(window.last);
            }
            if (!ok || uses.Any(u => !covered.Contains(u)))
                continue;

            foreach (var (store, dups, last) in windows)
            {
                removed.Add(lines[store]);
                foreach (var d in dups)
                    MakeDup(lines[d]);
                removed.Add(lines[last]);
            }
            context.DeadLocals.Add(local);
            changed = true;
        }

        if (changed)
        {
            // The stloc and the final ldloc go (straight-line code: one
            // predecessor, one successor each), so the dup form is exactly
            // what ILObjectInitializerOptimizer looks for.
            foreach (var line in removed)
            {
                var prev = line.PreviousInstructions.Single();
                var next = line.NextInstructions.Single();
                prev.NextInstructions[prev.NextInstructions.IndexOf(line)] = next;
                next.PreviousInstructions[next.PreviousInstructions.IndexOf(line)] = prev;
                lines.Remove(line);
            }
            // The exit's release list was taken at decode time: drop the
            // dead locals from it, as from the prologue's zeroing list.
            foreach (var line in lines.Where(l => l.Operation is OpRet && l.RawParameter is string))
            {
                var old = (string)line.RawParameter;
                var fresh = line.Operation.ConvertParameter(context, line).ToString();
                line.RawParameter = old.Substring(0, old.IndexOf('[')) + fresh.Substring(fresh.IndexOf('['));
            }
            foreach (var line in lines)
                line.StackContent = null;
            new ILMethodBuildEvaluationStackPass().Execute(context);
        }
    }

    // stloc t at `store`, right after a newobj; then (ldloc t; X; stfld)*;
    // then the final ldloc t.
    private static bool TryWindow(CompilerMethodContext context, List<ILOperation> lines, int store, int local,
        out (int store, List<int> dups, int last) window)
    {
        window = default;
        if (store == 0 || lines[store].Label != null)
            return false;
        var newobj = lines[store - 1];
        if (newobj.OpCode != ILOpCode.Newobj || newobj.Operation is not OpNewObj || newobj.RawParameter == null)
            return false;
        if (!IsProgramObject(context, newobj))
            return false;
        int baseDepth = lines[store].StackContent.Count;

        var dups = new List<int>();
        int p = store + 1;
        while (p < lines.Count)
        {
            if (lines[p].Label != null || LocalOf(lines[p]) != local || !IsLoad(lines[p]))
                return false;
            // The field store this load feeds: the first stfld back at the
            // depth below the object.
            int q = p + 1;
            while (q < lines.Count && lines[q].StackContent.Count > baseDepth && lines[q].OpCode != ILOpCode.Ret)
            {
                var l = lines[q];
                if (l.Label != null || LocalOf(l) == local || l.OpCode.IsBranch()
                    || l.OpCode is ILOpCode.Switch or ILOpCode.Calli
                    || (l.OpCode is ILOpCode.Call or ILOpCode.Callvirt && MayCollect(context, l))
                    || (l.OpCode == ILOpCode.Newobj && !IsLibraryObject(context, l) && MayCollect(context, l)))
                    return false;
                q++;
            }
            if (q < lines.Count && q > p + 1 && lines[q].Label == null && lines[q].Operation is OpStfld
                && lines[q].StackContent.Count == baseDepth)
            {
                dups.Add(p);
                p = q + 1;
                continue;
            }
            // Not a field store: the object escapes here (the last load).
            if (lines[p].StackContent.Count != baseDepth + 1)
                return false;
            window = (store, dups, p);
            return true;
        }
        return false;
    }

    // Can this call end up in GC.Collect (the only thing that collects)? A
    // class-library method other than GC.Collect cannot; a program method
    // can if anything it calls can. Dynamic calls (a virtual call that is
    // not devirtualized, a delegate) count as yes, and so does a cycle.
    private static bool MayCollect(CompilerMethodContext context, ILOperation call)
    {
        MethodBase target;
        try
        {
            target = context.CompilerContext.Assembly.ManifestModule.ResolveMethod((int)call.OriginalParameter);
        }
        catch
        {
            return true;
        }
        if (call.OpCode == ILOpCode.Callvirt && target.IsVirtual && !target.IsDevirtualizable(context.CompilerContext.Assembly))
            return true;
        return MayCollect(context.CompilerContext, target);
    }

    private static bool MayCollect(CompilerContext context, MethodBase method)
    {
        if (method == null)
            return true;
        if (method.DeclaringType == typeof(System.GC))
            return true;
        if (method.DeclaringType?.Assembly != context.Assembly)
            return typeof(System.Delegate).IsAssignableFrom(method.DeclaringType);
        method = ILStaticFramePass.Normalize(method);
        if (context.MayCollect.TryGetValue(method, out var known))
            return known;
        context.MayCollect[method] = true; // a cycle: assume the worst
        bool result = false;
        try
        {
            var scratch = new CompilerMethodContext { CompilerContext = context, Method = method, Lines = new List<ILOperation>() };
            if (method.GetMethodBody() != null)
                ILMethodCodePass.Decode(scratch);
            foreach (var line in scratch.Lines)
            {
                if (line.OpCode is ILOpCode.Calli or ILOpCode.Ldftn or ILOpCode.Ldvirtftn)
                {
                    result = true;
                    break;
                }
                if (line.OpCode is not (ILOpCode.Call or ILOpCode.Callvirt or ILOpCode.Newobj))
                    continue;
                var target = context.Assembly.ManifestModule.ResolveMethod((int)line.OriginalParameter);
                if ((line.OpCode == ILOpCode.Callvirt && target.IsVirtual && !target.IsDevirtualizable(context.Assembly))
                    || MayCollect(context, target))
                {
                    result = true;
                    break;
                }
            }
        }
        catch
        {
            result = true;
        }
        context.MayCollect[method] = result;
        return result;
    }

    // A newobj of a program type whose constructor body is never run (or is
    // folded into initial bytes): no code of ours runs during it.
    private static bool IsProgramObject(CompilerMethodContext context, ILOperation newobj)
    {
        try
        {
            var ctor = context.CompilerContext.Assembly.ManifestModule.ResolveMethod((int)newobj.OriginalParameter);
            return ctor.DeclaringType?.Assembly == context.CompilerContext.Assembly
                && ConstructorSupport.Kind(ctor) != ConstructorKind.Call;
        }
        catch
        {
            return false;
        }
    }

    // A delegate or other library object: no program code runs.
    private static bool IsLibraryObject(CompilerMethodContext context, ILOperation newobj)
    {
        try
        {
            var ctor = context.CompilerContext.Assembly.ManifestModule.ResolveMethod((int)newobj.OriginalParameter);
            return ctor.DeclaringType?.Assembly != context.CompilerContext.Assembly;
        }
        catch
        {
            return false;
        }
    }

    private static readonly OpBase Dup = CommandMap.Get(ILOpCode.Dup);


    private static void MakeDup(ILOperation line)
    {
        line.OpCode = ILOpCode.Dup;
        line.Operation = Dup;
        line.RawParameter = null;
        line.OriginalParameter = null;
    }

    private static bool IsStore(ILOperation line) => line.Operation is OpStloc or OpStloc_s;

    private static bool IsLoad(ILOperation line) =>
        line.OpCode is ILOpCode.Ldloc_0 or ILOpCode.Ldloc_1 or ILOpCode.Ldloc_2 or ILOpCode.Ldloc_3 or ILOpCode.Ldloc_s or ILOpCode.Ldloc;

    // The local a line reads, writes or takes the address of; -1 for none.
    private static int LocalOf(ILOperation line)
    {
        switch (line.OpCode)
        {
            case ILOpCode.Ldloc_0: case ILOpCode.Stloc_0: return 0;
            case ILOpCode.Ldloc_1: case ILOpCode.Stloc_1: return 1;
            case ILOpCode.Ldloc_2: case ILOpCode.Stloc_2: return 2;
            case ILOpCode.Ldloc_3: case ILOpCode.Stloc_3: return 3;
            case ILOpCode.Ldloc_s: case ILOpCode.Stloc_s: case ILOpCode.Ldloca_s:
            case ILOpCode.Ldloc: case ILOpCode.Stloc: case ILOpCode.Ldloca:
                return line.OriginalParameter is int index ? index : -2;
            default:
                return -1;
        }
    }
}
