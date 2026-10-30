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

        // Drops `ldy stackPointer` reloads Y already satisfies -- see
        // YRegisterTracker.
        var yTracker = new YRegisterTracker();
        if (context.StaticFrame != null)
        {
            // ILStaticFramePass: the caller already stored the arguments in
            // this method's fixed parameter slots, and the return address
            // stays on the hardware stack -- no prologue at all.
            var frame = context.StaticFrame;
            output.WriteLine($"    ; static frame: StaticFrames+{frame.Offset}, {frame.Size} bytes{(frame.CalleePulls ? ", callee pulls" : "")}");
            if (frame.CalleePulls)
            {
                // Save the return address in the frame's bottom two bytes,
                // then pull the arguments into their slots.
                output.WriteLine($"    pla\n    sta SF_TOP-{frame.ReturnAddressRel}\n    pla\n    sta SF_TOP-{frame.ReturnAddressRel}+1");
                var pulls = Compiler.Ops.OpCall.StaticFramePulls(frame);
                if (pulls.Length > 0)
                    output.WriteLine("    " + pulls.TrimEnd());
            }
            if (local_refs.Count > 0)
                output.WriteLine($"    #sf_enter [{string.Join(',', local_refs)}]");
        }
        else
        {
            outputLine = context.IsFrameless
                ? $"    #method_enter_frameless {context.GetLocalsSize()}, [{string.Join(',', local_refs)}]"
                : $"    #init_locals_pull_parameters {context.GetLocalsSize()}, [{string.Join(',', ref_params)}], [{string.Join(',', local_refs)}]";
            output.WriteLine(yTracker.Process(outputLine, false));
        }

        // PROTOTYPE (ILMethodPromoteParametersPass): the prologue just
        // above is otherwise completely unmodified -- unlike a promoted
        // LOCAL (which is only ever born via Stloc, so nothing extra is
        // needed), a promoted PARAMETER's real initial value just landed
        // in its ordinary localsStack slot via that same prologue, so one
        // one-time copy into its zp byte is needed here before anything
        // else runs. After this, every Ldarg/Starg of that parameter for
        // the rest of the method reads/writes the zp byte directly (see
        // OpLdarg/OpLdarg_s/OpStarg_s's own Emit overrides), and its
        // localsStack slot is never touched again.
        foreach (var (argIndex, slot) in context.PromotedParameters)
        {
            int relPos = context.GetParameterReferencePosition(argIndex);
            var copyIn = new TopOfStackInA.Line
            {
                Code = new List<string>
                {
                    yTracker.Process($"    #locals_push_value8 {relPos} ; (promoted parameter copy-in)", false),
                    yTracker.Process($"    #zp_pull_value8 {slot}", false),
                },
            };
            if (context.TypeContext.CompilerContext.Optimize)
                TopOfStackInA.Apply(new List<TopOfStackInA.Line> { copyIn });
            foreach (var text in copyIn.Code)
                output.WriteLine(text);
        }

        var emitted = new List<TopOfStackInA.Line>();
        foreach (var line in context.Lines)
        {
            context.CurrentIlOffset = line.Position;
            var code = line.Operation.Emit(context, line);
            if (line.Optimized)
            {
                if (line.Label != null)
                    yTracker.Reset();
            }
            else
                code = yTracker.Process(code, line.Label != null);
            emitted.Add(new TopOfStackInA.Line { Label = line.Label, Optimized = line.Optimized, Code = (code ?? "").Split('\n').ToList() });
        }

        // Drops pha/pla pairs between consecutive lines -- see TopOfStackInA.
        if (context.TypeContext.CompilerContext.Optimize)
            TopOfStackInA.Apply(emitted);

        for (int i = 0; i < emitted.Count; i++)
        {
            var line = context.Lines[i];
            var code = string.Join("\n", emitted[i].Code);
            outputLine = $"{(line.Label == null ? "" : line.Label + ":")}  {(line.Optimized ? "; OPT " : "")}  {code} ; {line.OpCode}";
            output.WriteLine(outputLine);
        }

        // A callee-pulls static frame's shared exit (OpRet jumps here,
        // unless the method's last line is itself the ret that emits it).
        if (context.StaticFrame is { CalleePulls: true } && context.Lines.Count > 0
            && context.Lines[^1].Operation is not Compiler.Ops.OpRet)
            output.WriteLine(Compiler.Ops.OpRet.StaticExitBlock(context));
    }
}