using System.Threading;
using System.Globalization;
using System.Reflection;
using System.Security.AccessControl;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Linq;

namespace Compiler;

class ILMethodEmitPass : ICompilerMethodPass
{
    public void Execute(CompilerMethodContext context)
    {
        if (context.Method.DeclaringType != context.TypeContext.Type)
            return;

        // ILCallSiteCountPass already decided every reference to this
        // method got spliced in directly (ILMethodInliningPass did that
        // splicing back in the setup group) and that nothing else needs the
        // real subroutine (MandatoryStandalone doesn't apply) -- nothing
        // jsr's into a standalone copy of it anymore, so emitting one here
        // would just silently DUPLICATE its code instead of relocating it.
        // See DeleteStandaloneDefinition's own comment.
        if (context.CompilerContext.DeleteStandaloneDefinition.Contains(context.Method))
            return;

        var output = context.TypeContext.OutputFile;
        output.WriteLine("");
        output.WriteLine("");
        output.WriteLine(";----------------------------------------");
        output.WriteLine($"; TYPE: {context.Method.ReflectedType.FullName}");
        output.WriteLine($"; METHOD: {context.Method.Name}");
        output.WriteLine(";----------------------------------------");
        output.WriteLine($"{context.Method.GetLabel()} ");

        if (context.Method.IsAbstract)
        {
            output.WriteLine("    brk");
            return;
        }

        string outputLine;
        var ref_params = context.Method.GetParameterRefList();
        // Zeroes every reference-typed LOCAL's own slot at method entry --
        // see CompilerMethodContext.GetLocalRefPositions's own comment for
        // why this is load-bearing, not cosmetic: without it, a local's
        // first write decrements whatever handle happens to still be
        // sitting in that shared, never-re-zeroed localsStack position
        // from some earlier, unrelated call.
        var local_refs = context.GetLocalRefPositions();

        outputLine = $"    #init_locals_pull_parameters {context.GetLocalsSize()}, [{string.Join(',', ref_params)}], [{string.Join(',', local_refs)}]";
        output.WriteLine(outputLine);

        foreach (var line in context.Lines)
        {
            context.CurrentIlOffset = line.Position;
            outputLine = $"{(line.Label == null ? "" : line.Label + ":")}  {(line.Optimized ? "; OPT " : "")}  {line.Operation.Emit(context, line)} ; {line.OpCode}";
            output.WriteLine(outputLine);

        }
    }
}