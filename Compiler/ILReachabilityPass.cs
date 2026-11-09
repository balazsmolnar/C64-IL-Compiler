using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using C64Lib;

namespace Compiler;

// Whole-program dead-code elimination: which methods and types can ever be
// used, found by walking the program from its roots (rapid type analysis).
// Run before every other whole-program pass, so the call-site counts,
// static frames and ILCodePass only see live code. A dead method is still
// compiled -- into a throwaway context whose output goes nowhere (see
// ILCodePass), so an unsupported construct in it is still reported -- but
// it emits nothing: no code, no strings, no init tables, no library
// routines. A dead type gets no .asm file at all (no vtable, no static
// fields). A dead virtual method of a live type leaves a 0 in its vtable
// slot (ILTypeVTablePass), which nothing can dispatch to: dispatch to it
// would need an instance of a type that inherits it, and every such
// instantiation makes it live.
//
// Roots (conservative wherever the call graph cannot see a use):
//   - Main (the assembly's entry point, and any Program.Main: the entry
//     template jumps to Program_Main);
//   - every static constructor (the entry template calls them all at
//     startup, used or not);
//   - in a unit-test build, every method of a type that has a method with
//     an NUnit attribute (the harness jumps to tests by label);
//   - every method whose label appears in RawAssembly text or in a string
//     literal of live code (C64Address.FromLabel), and every type whose
//     label prefix does;
//   - overrides of virtual methods declared outside the program (object's
//     ToString, ...) in every instantiated type.
// Constructors that are compiled (ConstructorSupport) are called by the
// newobj or the base/this(...) call that names them.
// Edges: call, ldftn (delegate and interrupt targets), a devirtualizable
// callvirt (it compiles to a plain jsr); a real virtual call or ldvirtftn
// makes the slot live, and a live slot is resolved in every type
// instantiated by live code (newobj), however the two are discovered.
// Types: a type is live when it declares a live method, live code names
// it (any type, field or method token), or a live type derives from it.
//
// C64_DCE=0 (or --no-dce) turns it off; debug builds (the TestDebugger)
// keep everything.
class ILReachabilityPass : ICompilerPass
{
    private static readonly Dictionary<short, OperandType> OperandTypes = typeof(OpCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Select(f => (OpCode)f.GetValue(null))
        .GroupBy(o => o.Value)
        .ToDictionary(g => g.Key, g => g.First().OperandType);

    private static readonly Regex Identifier = new(@"[A-Za-z_][A-Za-z0-9_]*", RegexOptions.Compiled);

    public static bool Disabled(CompilerContext context) =>
        context.EmitDebugInfo || !context.DeadCodeElimination
        || Environment.GetEnvironmentVariable("C64_DCE") == "0";

    public void Execute(CompilerContext context)
    {
        if (Disabled(context))
            return;

        var module = context.Assembly.ManifestModule;
        var methods = EnumerateMethods(context).Select(ILStaticFramePass.Normalize).Distinct().ToList();
        var byLabel = new Dictionary<string, List<MethodBase>>();
        foreach (var m in methods)
        {
            var label = m.GetLabel();
            if (!byLabel.TryGetValue(label, out var list))
                byLabel[label] = list = new List<MethodBase>();
            list.Add(m);
        }
        var programTypes = context.Assembly.GetTypes().Where(t => !t.IsValueType).ToList();

        var live = new HashSet<MethodBase>();
        var liveTypes = new HashSet<Type>();
        var instantiated = new HashSet<Type>();
        var slots = new List<MethodBase>();
        var slotSet = new HashSet<MethodBase>();
        var work = new Stack<MethodBase>();
        var seenWords = new HashSet<string>();

        void Mark(MethodBase m)
        {
            if (m == null)
                return;
            m = ILStaticFramePass.Normalize(m);
            if (m.DeclaringType == null || m.DeclaringType.Assembly != context.Assembly)
                return;
            if (live.Add(m))
                work.Push(m);
        }

        void MarkType(Type t)
        {
            while (t != null && t.Assembly == context.Assembly)
            {
                if (t.IsArray || t.IsByRef || t.IsPointer)
                {
                    t = t.GetElementType();
                    continue;
                }
                if (!liveTypes.Add(t))
                    return;
                if (t.DeclaringType != null)
                    MarkType(t.DeclaringType);
                t = t.BaseType;
            }
        }

        // Every implementation `type` would dispatch `slot` to: the virtual
        // methods of that name along its base chain (all of them, which
        // also covers `new virtual` re-declarations).
        void Resolve(Type type, MethodBase slot)
        {
            if (slot.DeclaringType != null && !slot.DeclaringType.IsAssignableFrom(type))
                return;
            for (var t = type; t != null && t.Assembly == context.Assembly; t = t.BaseType)
                foreach (var m in t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                    if (m.IsVirtual && m.Name == slot.Name)
                        Mark(m);
        }

        void Instantiate(Type type)
        {
            if (type == null || type.Assembly != context.Assembly || !instantiated.Add(type))
                return;
            MarkType(type);
            foreach (var slot in slots)
                Resolve(type, slot);
            // Overrides of methods the program does not declare.
            for (var t = type; t != null && t.Assembly == context.Assembly; t = t.BaseType)
                foreach (var m in t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                    if (m.IsVirtual && m.GetBaseDefinition().DeclaringType?.Assembly != context.Assembly)
                        Mark(m);
        }

        void AddSlot(MethodBase slot)
        {
            slot = ILStaticFramePass.Normalize(slot);
            if (!slotSet.Add(slot))
                return;
            slots.Add(slot);
            foreach (var type in instantiated.ToList())
                Resolve(type, slot);
        }

        void ScanText(string text)
        {
            foreach (Match word in Identifier.Matches(text))
            {
                if (!seenWords.Add(word.Value))
                    continue;
                if (byLabel.TryGetValue(word.Value, out var named))
                    named.ForEach(Mark);
            }
        }

        // Roots.
        if (context.Assembly.EntryPoint != null)
            Mark(context.Assembly.EntryPoint);
        foreach (var m in methods)
        {
            if (m.Name == "Main" && m.IsStatic)
                Mark(m);
            if (m.IsStatic && m.Name == ".cctor")
                Mark(m);
        }
        if (context.IsUnitTest)
        {
            foreach (var type in programTypes)
            {
                var declared = type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (HasNUnit(type) || declared.Any(HasNUnit))
                {
                    MarkType(type);
                    foreach (var m in declared)
                        Mark(m);
                }
            }
        }
        var rawText = RawAssemblyText(context);
        ScanText(rawText);
        var rawWords = new HashSet<string>(Identifier.Matches(rawText).Select(w => w.Value));
        foreach (var type in programTypes)
        {
            var prefix = type.FullName.ToValidName() + "_";
            if (rawWords.Any(w => w.StartsWith(prefix, StringComparison.Ordinal)))
                MarkType(type);
        }

        // Walk.
        while (work.Count > 0)
        {
            var method = work.Pop();
            MarkType(method.DeclaringType);
            if (method.IsAbstract || method.GetMethodBody() == null)
                continue;

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
                // ILCodePass reports it, and the build fails anyway.
                continue;
            }

            foreach (var line in lines)
            {
                if (line.OpCode == ILOpCode.Ldstr)
                {
                    try { ScanText(module.ResolveString(Token(line))); } catch { }
                    continue;
                }
                if (!OperandTypes.TryGetValue((short)line.OpCode, out var operand))
                    continue;
                try
                {
                    switch (operand)
                    {
                        case OperandType.InlineMethod:
                            Method(line, module.ResolveMethod(Token(line)));
                            break;
                        case OperandType.InlineField:
                            MarkType(module.ResolveField(Token(line)).DeclaringType);
                            break;
                        case OperandType.InlineType:
                            MarkType(module.ResolveType(Token(line)));
                            break;
                        case OperandType.InlineTok:
                            var member = module.ResolveMember(Token(line));
                            if (member is Type t)
                                MarkType(t);
                            else if (member != null)
                                MarkType(member.DeclaringType);
                            break;
                    }
                }
                catch
                {
                    // A token this program cannot resolve (generic
                    // instantiations, ...): ILCodePass decides about it.
                }
            }
        }

        void Method(ILOperation line, MethodBase target)
        {
            if (target == null)
                return;
            if (target.DeclaringType != null)
                MarkType(target.DeclaringType);
            switch (line.OpCode)
            {
                case ILOpCode.Newobj:
                    // A compiled constructor (ConstructorSupport) is called.
                    Mark(target);
                    Instantiate(target.DeclaringType);
                    break;
                case ILOpCode.Callvirt:
                case ILOpCode.Ldvirtftn:
                    if (!target.IsVirtual)
                        Mark(target);
                    else
                    {
                        // A devirtualized call jsr's to the target's label.
                        if (target.IsDevirtualizable(context.Assembly))
                            Mark(target);
                        if (target.DeclaringType?.Assembly == context.Assembly)
                            AddSlot(target);
                    }
                    break;
                default:
                    Mark(target);
                    break;
            }
        }

        context.AnalyzedMethods = new HashSet<MethodBase>(methods);
        context.LiveMethods = live;
        context.LiveTypes = liveTypes;

        var deadMethods = methods.Where(m => !live.Contains(m) && !m.IsAbstract).ToList();
        var deadTypes = programTypes.Where(t => !liveTypes.Contains(t)).ToList();
        context.GlobalOutputFile.WriteLine($"; Dead code (Compiler/ILReachabilityPass.cs): {deadMethods.Count} of {methods.Count} methods and {deadTypes.Count} of {programTypes.Count} types removed");
        foreach (var m in deadMethods.Select(m => m.GetLabel()).OrderBy(x => x))
            context.GlobalOutputFile.WriteLine($";   method {m}");
        foreach (var t in deadTypes.Select(t => t.FullName).OrderBy(x => x))
            context.GlobalOutputFile.WriteLine($";   type {t}");
        Console.WriteLine($"Dead code: {deadMethods.Count} of {methods.Count} methods, {deadTypes.Count} of {programTypes.Count} types removed"
            + (deadMethods.Count > 0 ? ": " + string.Join(", ", deadMethods.Select(m => m.GetLabel()).OrderBy(x => x)) : ""));
    }

    private static int Token(ILOperation line) => (int)line.OriginalParameter;

    // An attribute whose assembly cannot be loaded (the test SDK's own
    // generated types) counts: keep that type rather than guess.
    private static bool HasNUnit(MemberInfo member)
    {
        try
        {
            return member.GetCustomAttributesData().Any(IsNUnit);
        }
        catch
        {
            return true;
        }
    }

    private static bool IsNUnit(CustomAttributeData a) =>
        a.AttributeType.Namespace != null && a.AttributeType.Namespace.StartsWith("NUnit");

    private static string RawAssemblyText(CompilerContext context)
    {
        var text = new System.Text.StringBuilder();
        foreach (var attr in context.Assembly.GetCustomAttributes(typeof(RawAssemblyAttribute), false).OfType<RawAssemblyAttribute>())
        {
            if (!string.IsNullOrEmpty(attr.Asm))
                text.AppendLine(attr.Asm);
            else if (!string.IsNullOrEmpty(attr.Resource))
            {
                using var stream = context.Assembly.GetManifestResourceStream(attr.Resource);
                if (stream == null)
                    continue;
                using var reader = new StreamReader(stream);
                text.AppendLine(reader.ReadToEnd());
            }
        }
        return text.ToString();
    }

    // Every method ILCodePass compiles under its own declaring type.
    private static IEnumerable<MethodBase> EnumerateMethods(CompilerContext context)
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
                if (ILMethodCodePass.IsAutoPropertyAccessor(method, context.Assembly.ManifestModule, out _, out _))
                    continue;
                yield return method;
            }
        }
    }
}
