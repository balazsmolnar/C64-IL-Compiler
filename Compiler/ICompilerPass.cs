using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Compiler;

class CompilerContext
{
    public Assembly Assembly { get; set; }
    public StreamWriter GlobalOutputFile { get; set; }
    public IList<CompilerMethodContext> Methods { get; set; }
    public Dictionary<int, string> StringValues { get; } = new Dictionary<int, string>();
    public HashSet<string> OptimizedStringValues { get; } = new HashSet<string>();

    public string OutputDirectory { get; set; }

    // Populated once, by ILCallSiteCountPass, before ILCodePass even starts
    // (a real whole-program pre-pass, not a per-method setup pass -- see its
    // own comment for why: ILMethodInliningPass needs a COMPLETE count while
    // still processing the very first caller, and ILCodePass's own
    // method-enumeration order isn't guaranteed to visit every caller of a
    // given method before that method itself gets processed).
    //
    // Call sites PLUS Callvirt sites where MethodBaseExtension.
    // IsDevirtualizable is true -- both are mechanically identical once
    // devirtualized (OpCallVirt.Emit already falls through to a plain `jsr`
    // for exactly these, see that method), so they're counted together as
    // "could this call site be inlined". A Callvirt site that is NOT
    // devirtualizable (real dynamic dispatch) never enters this count at
    // all -- see MandatoryStandalone below, which is where that distinction
    // actually matters.
    public Dictionary<MethodBase, int> InlinableSiteCounts { get; } = new Dictionary<MethodBase, int>();

    // Subset of InlinableSiteCounts' own count, per method: how many of
    // those sites ALSO pass ILMethodInliningPass's trivial-argument-
    // substitution shape (every argument, including `this`, is a bare
    // Ldarg_N/Ldloc_N at that specific call site -- see
    // ILMethodInliningPass.IsTrivialArgumentProducer). This is a property
    // of the CALL SITE (the caller's own code), computed here because only
    // this pass's first loop walks every caller's lines; whether the
    // callee itself actually qualifies (no locals of its own, never
    // writes back to the substituted parameter -- see
    // ILMethodInliningPass.CalleeQualifiesForTrivialSubstitution) is a
    // separate, callee-level question ShouldInline checks before trusting
    // this count for anything.
    public Dictionary<MethodBase, int> TrivialSiteCounts { get; } = new Dictionary<MethodBase, int>();

    // Methods whose standalone definition can NEVER be deleted, regardless
    // of InlinableSiteCounts -- something else needs the real, jsr-able
    // subroutine to keep existing:
    //   - any Ldftn reference anywhere (a delegate target -- construction
    //     needs a real function pointer);
    //   - any Callvirt reference that is NOT devirtualizable (genuine
    //     dynamic dispatch through a vtable slot).
    // A method here can still have its OTHER (inlinable) call sites
    // spliced via InlineCandidates below -- this only blocks deleting the
    // original, never blocks inlining elsewhere.
    public HashSet<MethodBase> MandatoryStandalone { get; } = new HashSet<MethodBase>();

    // Decided by ILCallSiteCountPass: a method is a key here iff its call
    // sites should be spliced in -- either because the size/count math in
    // that pass says the aggregate is a net win, or because
    // [MethodImpl(MethodImplOptions.AggressiveInlining)] forces it
    // regardless. ILMethodInliningPass splices the already-decoded
    // List<ILOperation> value in here into EVERY one of the method's
    // inlinable (Call or devirtualizable-Callvirt) sites -- cloned fresh
    // per site, never the same ILOperation instances reused twice, since
    // (unlike single-call-site Phase 1) the same cached body can now be
    // spliced more than once.
    public Dictionary<MethodBase, List<ILOperation>> InlineCandidates { get; } = new Dictionary<MethodBase, List<ILOperation>>();

    // Subset of InlineCandidates.Keys: methods whose standalone definition
    // ILMethodEmitPass should actually skip emitting (every one of its
    // references got inlined, and MandatoryStandalone doesn't apply) --
    // see InlineCandidates above for why those two questions are no longer
    // the same question now that a method can have some call sites
    // inlined while others still need the real subroutine.
    public HashSet<MethodBase> DeleteStandaloneDefinition { get; } = new HashSet<MethodBase>();

    // Every "this C# construct isn't supported" problem found while compiling,
    // collected per method (ILCodePass) so one build reports all of them, with
    // source lines from the PDB when there is one.
    public CompilerDiagnostics Diagnostics { get; } = new CompilerDiagnostics();
    private SourceLocator _sourceLocator;
    public SourceLocator SourceLocator => _sourceLocator ??= new SourceLocator(Assembly);

    // Read by ILEntryPointPass: where to write the top-level entry .asm
    // file (e.g. asm/hunchback.asm), and whether it should be the
    // unittest.asm-style test harness instead of a normal Program_Main
    // entry point.
    public string EntryFilePath { get; set; }
    public bool IsUnitTest { get; set; }

    public bool Optimize { get; set; }

    // Set from the compiler's 5th CLI arg ("debug"); gates
    // ILMethodDebugLabelPass/ILDebugMapPass. Off by default -- labeling
    // every source-line-start operation (needed for breakpoint resolution)
    // suppresses several optimizer passes that skip already-labeled
    // operations, so this must never be on for a normal build.
    public bool EmitDebugInfo { get; set; }
    public List<DebugSequencePoint> DebugSequencePoints { get; } = new();
    public List<DebugLocal> DebugLocals { get; } = new();

    // Set by ILLibraryUsagePass as it scans every compiled method's Call/
    // Callvirt targets for C64Lib methods; read by ILLibraryFlagsPass once
    // all methods are done, to decide which individual hand-written asm
    // subroutines (each wrapped in its own `.weak Flag_X = 0 .endweak .if
    // Flag_X ... .endif` in asm/C64*.asm) this program actually needs.
    // Labels, not method names -- matches MethodBaseExtensions.GetLabel()
    // exactly, since that's what the asm side's Flag_<label> names are
    // keyed on.
    public HashSet<string> UsedLibraryLabels { get; } = new HashSet<string>();

    // Set by ILCodePass as it discovers each type's static constructor (if
    // any); read by ILEntryPointPass, which calls every one of them once at
    // program startup, before Program_Main/the test method runs -- there is
    // no other mechanism that runs a .cctor at all otherwise.
    public List<string> StaticConstructorLabels { get; } = new List<string>();
    public int GetFieldPosition(FieldInfo field)
    {
        var t = field.ReflectedType;
        var pos = 0;
        var fields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var f in fields.Where(f => !f.IsLiteral).OrderBy(f => f.FieldType.IsReferenceCounted() ? 0 : 1))
        {
            if (f == field)
                break;
            pos += f.FieldType.GetStorageBytes();
        }
        return pos;
    }

    public List<string> InitValues { get; } = new();

    public string GetInitValueLabel(string initValue)
    {

        var index = InitValues.IndexOf(initValue);
        if (index < 0)
        {
            InitValues.Add(initValue);
            index = InitValues.Count - 1;
        }

        return $"Init_Values_{index}";
    }
}

class CompilerTypeContext
{
    public CompilerContext CompilerContext { get; set; }
    public Type Type { get; set; }
    public StreamWriter OutputFile { get; set; }
}
class CompilerMethodContext
{
    public CompilerContext CompilerContext { get; set; }
    public CompilerTypeContext TypeContext { get; set; }
    public MethodBase Method { get; set; }
    public List<ILOperation> Lines { get; set; }

    // IL offset of the instruction currently being decoded/translated, so a
    // failure can be reported against the C# line that produced it.
    public int? CurrentIlOffset { get; set; }

    public int GetLocalVariableReferencePosition(int index)
    {
        bool isInstance = !Method.IsStatic;
        int relPos = isInstance ? 1 : 0;
        for (int i = 0; i < Method.GetParameters().Length; i++)
            relPos += Method.GetParameters()[i].ParameterType.GetStorageBytes();

        var body = Method.GetMethodBody();
        var variables = body.LocalVariables;
        for (int i = 0; i <= index; i++)
            relPos += variables[i].LocalType.GetStorageBytes();
        return relPos;
    }

    public Type GetLocalVariableType(int index) => GetLocalVariableType(Method, index);

    // sourceMethod: the method that actually declares local #index --
    // defaults to Method (this context's own method, the normal case for
    // every line that isn't part of an ILMethodInliningPass splice). Needed
    // because this is called again, after decode, by SetStackContent/
    // Is16Bit/SizeSuffix (OpLdloc/OpLdloc_s) against the one shared
    // (caller's) context -- an inlined callee's local #index must resolve
    // against the CALLEE's own locals, never the caller's (see
    // ILMethodInliningPass's "VarIndex hazard" comment).
    public Type GetLocalVariableType(MethodBase sourceMethod, int index)
    {
        var body = sourceMethod.GetMethodBody();
        var variables = body.LocalVariables;
        return variables[index].LocalType;
    }


    public int GetParameterReferencePosition(int index)
    {
        int relPos = 0;
        var parameters = Method.GetParameters();
        bool isInstance = !Method.IsStatic;

        if (isInstance && index == 0)
            return 1;

        if (isInstance)
            relPos = 1;

        for (int i = 0; i <= index - (isInstance ? 1 : 0); i++)
        {
            relPos += parameters[i].ParameterType.GetStorageBytes();
        }
        return relPos;
    }

    public Type GetParameterType(int index) => GetParameterType(Method, index);

    // sourceMethod: see GetLocalVariableType's own comment -- same reason,
    // consulted again after decode (SetStackContent/Is16Bit/SizeSuffix on
    // OpLdarg/OpLdarg_s/OpStarg_s) against the shared caller context.
    public Type GetParameterType(MethodBase sourceMethod, int index)
    {
        var parameters = sourceMethod.GetParameters();
        bool isInstance = !sourceMethod.IsStatic;

        if (isInstance && index == 0)
            return typeof(object);

        return parameters[index - (isInstance ? 1 : 0)].ParameterType;
    }

    public int GetLocalStackSize()
    {
        bool isInstance = !Method.IsStatic;
        int relPos = isInstance ? 1 : 0;
        for (int i = 0; i < Method.GetParameters().Length; i++)
            relPos += Method.GetParameters()[i].ParameterType.GetStorageBytes();

        relPos += GetLocalsSize();
        return relPos + 2;
    }

    public int GetParameterSize(int index) => GetParameterSize(Method, index);

    // sourceMethod: see GetLocalVariableType's own comment.
    public int GetParameterSize(MethodBase sourceMethod, int index)
    {
        var parameters = sourceMethod.GetParameters();
        bool isInstance = !sourceMethod.IsStatic;

        if (isInstance && index == 0)
            return 1;

        return parameters[index - (isInstance ? 1 : 0)].ParameterType.GetStorageBytes();
    }

    public int GetLocalsSize()
    {
        int size = 0;
        var body = Method.GetMethodBody();
        var variables = body.LocalVariables;
        for (int i = 0; i < variables.Count; i++)
            size += variables[i].LocalType.GetStorageBytes();
        return size;
    }

    // PROTOTYPE: set by ILMethodPromoteLocalsPass once per method, before
    // anything reads it (a setup pass, see Compiler/Program.cs's pass
    // ordering) -- see zp_local0's own comment in asm/helper/zeropage.asm
    // for what "qualifies" means and why. Maps a LOCAL VARIABLE's index
    // (never a parameter, never `this` -- see that pass) to the zero-page
    // symbol (e.g. "zp_local2") its ldloc/stloc should use instead of the
    // localsStack-relative #locals_push_value8/#locals_pull_value8 macros.
    // A local not in this map (the common case, including in every
    // non-leaf method) is untouched -- normal locals-stack addressing.
    public Dictionary<int, string> PromotedLocals { get; } = new Dictionary<int, string>();

    public string PromotedLocalSlot(int varIndex) =>
        PromotedLocals.TryGetValue(varIndex, out var slot) ? slot : null;

    // sourceMethod: null means "this line belongs to the method actually
    // being compiled" (the normal case) -- non-null means it was spliced in
    // by ILMethodInliningPass, in which case varIndex is the CALLEE's own
    // local index, never this (the caller's) PromotedLocals map's. An
    // inlined local is never promoted in this phase, full stop -- see
    // ILMethodInliningPass's "VarIndex hazard" comment.
    public string PromotedLocalSlot(MethodBase sourceMethod, int varIndex) =>
        sourceMethod == null ? PromotedLocalSlot(varIndex) : null;
}

interface ICompilerPass
{
    void Execute(CompilerContext context);
}

interface ICompilerTypePass
{
    void Execute(CompilerTypeContext context);
}

interface ICompilerMethodPass
{
    void Execute(CompilerMethodContext context);
}