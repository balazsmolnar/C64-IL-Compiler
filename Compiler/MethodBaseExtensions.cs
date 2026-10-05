using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Compiler;

static class MethodBaseExtension
{
    static public string GetLabel(this MethodBase method)
    {
        return $"{method.ReflectedType.Name.ToValidName()}_{method.Name.ToValidName()}";
    }

    // Single source of truth for "is this Callvirt target really just one
    // concrete method" -- moved out of OpCallVirt (Operands/OperandBase.cs)
    // unchanged in logic, so ILCallSiteCountPass can ask the exact same
    // question OpCallVirt.Emit already answers for devirtualizing a call at
    // emit time. Deliberately returns false (not true) for the interface/
    // unsupported-delegate-Invoke cases: inlining must never bypass
    // OpCallVirt.Emit's own diagnostic throws for those by "handling" the
    // call site before Emit ever sees it.
    static public bool IsDevirtualizable(this MethodBase methodInfo, Assembly assembly)
    {
        if (methodInfo.DeclaringType != null && methodInfo.DeclaringType.IsInterface)
            return false;
        if (methodInfo.Name == "Invoke" && methodInfo.DeclaringType != null
            && typeof(Delegate).IsAssignableFrom(methodInfo.DeclaringType)
            && !methodInfo.DeclaringType.Name.StartsWith("Func"))
            return false;
        // for Func<>
        if (!methodInfo.IsVirtual || methodInfo.ReflectedType.Name.StartsWith("Func"))
            return true;
        // Closed-world devirtualization: a general JIT has to stay conservative
        // here, since code can still load dynamically after it decides. This
        // compiler never has that problem -- the entire program is one already-
        // fully-loaded assembly by the time any of it is compiled, so "nothing
        // anywhere overrides this" is a permanent fact, not just true so far.
        // (methodInfo.IsAbstract is excluded because it never has a body of its
        // own to jump to -- true regardless of overrides, and if it somehow had
        // none, the vtable slot it'd otherwise use would be equally meaningless.)
        return !methodInfo.IsAbstract && !IsOverriddenAnywhere(assembly, methodInfo.ReflectedType, methodInfo.Name);
    }

    // Keyed by (call site's static receiver type, method name) -- this compiler's
    // own vtable (TypeExtensions.GetVirtualMethodIndex/GetVirtualMethods) already
    // resolves overrides by name only, not full signature, so this check matches
    // that same (name-only) notion of "override" for consistency, rather than a
    // stricter one that could disagree with it. Static (not per-OpCallVirt-
    // instance): IsDevirtualizable needs to call this from ILCallSiteCountPass
    // too, which has no OpCallVirt instance to hand -- OpCallVirt itself was
    // already a single whole-program-shared instance via CommandMap, so this
    // changes no behavior, just who can reach the cache.
    private static readonly Dictionary<(Type, string), bool> _overriddenAnywhereCache = new();

    private static bool IsOverriddenAnywhere(Assembly assembly, Type declaringType, string methodName)
    {
        var key = (declaringType, methodName);
        if (_overriddenAnywhereCache.TryGetValue(key, out var cached))
            return cached;

        bool overridden = false;
        foreach (var candidateType in assembly.GetTypes())
        {
            if (candidateType == declaringType || !declaringType.IsAssignableFrom(candidateType))
                continue;

            var methods = candidateType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
            if (methods.Any(m => m.IsVirtual && m.Name == methodName))
            {
                overridden = true;
                break;
            }
        }

        _overriddenAnywhereCache[key] = overridden;
        return overridden;
    }

    // The boolean-ish ref_list a prologue (#init_locals_pull_parameters /
    // #init_locals_pull_parameters_inline) needs to pull this method's own
    // parameters/`this` off the evaluation stack: one "1" per reference-typed
    // parameter (a reference is always a single 1-byte object-table handle),
    // one "0" per storage byte of every other parameter (generalizes the
    // 1-byte/2-byte split to any width, e.g. float's 5 bytes), reversed
    // (pulled in the opposite order they were pushed), with one more "0" for
    // `this` appended last if the method is an instance method. Factored out
    // of ILMethodEmitPass so ILMethodInliningPass can build the identical
    // shape for a callee being spliced in instead of jsr'd into.
    static public List<string> GetParameterRefList(this MethodBase method)
    {
        var refParams = new List<string>();
        foreach (var param in method.GetParameters().Reverse())
        {
            refParams.Add(param.ParameterType.IsReferenceCounted()
                ? "1"
                : string.Join(", ", Enumerable.Repeat("0", param.ParameterType.GetStorageBytes())));
        }
        if (!method.IsStatic)
            refParams.Add("0");
        return refParams;
    }
}