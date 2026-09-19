using System;
using System.Reflection;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

// Rewrites string.Concat(string,string)/.Length/.PadLeft(int,char) into a
// single jsr to a hand-written asm/helper/stringops.asm routine -- same
// pattern-match-and-replace shape as ILNumericToStringPass, which this
// closely mirrors. Call/Callvirt are already opcode-supported in general
// (CommandMap.cs), so without this pass they'd resolve (via
// MethodBaseExtensions.GetLabel) to a label with no corresponding compiled
// method -- System.String has no C# source in this project to reflect
// against -- and fail only much later as a confusing 64tass "undefined
// label" assembler error, not a clean compile-time diagnostic. This pass
// turns exactly the three supported String members into a real, resolvable
// jsr before ILMethodEmitPass (or, for Callvirt, OpCallVirt's own
// devirtualization logic) ever gets to them.
//
// Deliberately narrow: only the 2-string Concat overload (not the
// object-boxing overloads Roslyn emits for e.g. `"literal" + someInt`
// without an explicit .ToString() first -- this compiler has no boxing
// support at all, so those could never work regardless of this pass), and
// only PadLeft(int,char) (not the 1-arg space-padding overload). Anything
// else -- string.Equals, LINQ, Console.WriteLine, an unsupported
// PadLeft/Concat overload -- still falls through unresolved exactly as
// before; making that fail cleanly instead of via a confusing assembler
// error is a separate, later change (see the "unresolvable Call" item in
// this session's own compiler-feature-support survey).
//
// Runs in the setup group, alongside ILNumericToStringPass -- both are
// simple forward pattern-matches over the as-decoded instruction stream,
// before any optimizer pass gets a chance to touch these opcodes.
class ILStringOpsPass : ICompilerMethodPass
{
    public void Execute(CompilerMethodContext context)
    {
        var lines = context.Lines;
        if (lines == null)
            return;

        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].OpCode != ILOpCode.Call && lines[i].OpCode != ILOpCode.Callvirt)
                continue;

            var callee = context.CompilerContext.Assembly.ManifestModule.ResolveMethod((int)lines[i].OriginalParameter) as MethodBase;
            if (callee == null || callee.DeclaringType != typeof(string))
                continue;

            var label = ResolveLabel(callee);
            if (label == null)
                continue;

            context.CompilerContext.UsedLibraryLabels.Add(label);
            var newOperation = new ILOperation
            {
                Operation = new OpBase(0, "jsr"),
                RawParameter = label,
                StackContent = lines[i].StackContent,
            };
            lines.Insert(i + 1, newOperation);
            lines[i].Optimized = true;
        }
    }

    private static string ResolveLabel(MethodBase callee)
    {
        var parameters = callee.GetParameters();

        if (callee.Name == nameof(string.Concat) && parameters.Length == 2 &&
            parameters[0].ParameterType == typeof(string) && parameters[1].ParameterType == typeof(string))
            return "String_Concat";

        if (callee.Name == "get_" + nameof(string.Length) && parameters.Length == 0)
            return "String_Length";

        if (callee.Name == nameof(string.PadLeft) && parameters.Length == 2 &&
            parameters[0].ParameterType == typeof(int) && parameters[1].ParameterType == typeof(char))
            return "String_PadLeft";

        return null;
    }
}
