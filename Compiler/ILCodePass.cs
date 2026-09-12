using System.Net.WebSockets;
using System.Threading.Tasks.Dataflow;
using System.Threading;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Xml;
using System.Net.Mail;
using System.Reflection.Metadata.Ecma335;
using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Linq;
using System.Reflection;
using System.IO;

namespace Compiler;

class ILCodePass : ICompilerPass
{
    // Once-only setup passes (decode IL, assign labels, compute stack content, etc.)
    // -- re-running these would rebuild Lines from scratch, not optimize it.
    private readonly IEnumerable<ICompilerTypePass> _typePasses;
    private readonly IEnumerable<ICompilerMethodPass> _setupPasses;

    // Peephole optimizer passes, run repeatedly to a fixpoint: one rule's
    // output can expose a new match for another rule (or itself) that
    // already had its turn in an earlier sweep -- e.g. a newly-inserted
    // operation sitting adjacent to something another rule can now also
    // collapse. Every rule only ever *inserts* one new operation per match
    // and never removes a line (matched lines are just marked Optimized),
    // so Lines.Count strictly increases on any real progress -- an
    // unchanged count after a full sweep reliably means nothing matched
    // anywhere, with no need for passes to separately report whether they
    // changed anything.
    private readonly IEnumerable<ICompilerMethodPass> _optimizerPasses;
    private const int MaxOptimizerIterations = 10;

    // Once-only final passes (write asm text, build jump tables) -- must
    // run after the optimizer set has settled, not be looped themselves.
    private readonly IEnumerable<ICompilerMethodPass> _finalPasses;

    public ILCodePass(
        IEnumerable<ICompilerTypePass> typePasses,
        IEnumerable<ICompilerMethodPass> setupPasses,
        IEnumerable<ICompilerMethodPass> optimizerPasses,
        IEnumerable<ICompilerMethodPass> finalPasses)
    {
        _typePasses = typePasses;
        _setupPasses = setupPasses;
        _optimizerPasses = optimizerPasses;
        _finalPasses = finalPasses;
    }

    public void Execute(CompilerContext context)
    {
        context.Methods = new List<CompilerMethodContext>();
        foreach (var @type in context.Assembly.GetTypes())
        {
            if (@type.IsValueType)
                continue;
            // context.GlobalOutputFile.WriteLine($".include \".\\\\{type.Name}.asm\"");

            var normalizedName = type.Name.ToValidName();
            var import = $"\"./{normalizedName}.asm\"";
            context.GlobalOutputFile.WriteLine($".include {import}");
            using (var outputFile = File.CreateText(Path.Combine(context.OutputDirectory, $"{normalizedName}.asm")))
            {

                var typeContext = new CompilerTypeContext()
                {
                    CompilerContext = context,
                    Type = @type,
                    OutputFile = outputFile
                };
                foreach (var pass in _typePasses)
                {
                    pass.Execute(typeContext);
                }

                var methods = @type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).OfType<MethodBase>();
                // var staticconstructors = @type.GetConstructors(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).OfType<MethodBase>();
                // var constructors = @type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).OfType<MethodBase>();
                // if (constructors.Count() > 1)
                //     throw new Exception($"Only 1 constructor supported (at the moment). Type : {@type} ");

                foreach (var method in methods.Where(m => m.DeclaringType != typeof(object)))
                {
                    var methodContext = new CompilerMethodContext()
                    {
                        CompilerContext = context,
                        TypeContext = typeContext,
                        Lines = new List<ILOperation>(),
                        Method = method,
                    };
                    context.Methods.Add(methodContext);

                    foreach (var pass in _setupPasses)
                    {
                        pass.Execute(methodContext);
                    }

                    for (int iteration = 0; iteration < MaxOptimizerIterations; iteration++)
                    {
                        var before = methodContext.Lines.Count;
                        foreach (var pass in _optimizerPasses)
                        {
                            pass.Execute(methodContext);
                        }
                        if (methodContext.Lines.Count == before)
                            break;
                    }

                    foreach (var pass in _finalPasses)
                    {
                        pass.Execute(methodContext);
                    }
                }
            }
        }
    }
}
