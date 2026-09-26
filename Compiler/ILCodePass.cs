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

            // FullName, not Name -- two different types can share a short
            // name (most commonly Roslyn's own compiler-generated "<>O"
            // cached-static-delegate holder class, synthesized once per
            // declaring type for any `SomeEvent += StaticMethod;`-shaped
            // conversion: Test/FuncTest.cs and Test/InterruptTests.cs each
            // get their own "<>O", and Name-only labeling silently wrote
            // both to the same asm/unittest/x__O.asm, truncating whichever
            // was compiled first and then double-.include-ing what
            // remained). FullName's own separators ('.' for namespaces,
            // '+' for nesting) are already handled by ToValidName.
            var normalizedName = type.FullName.ToValidName();
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
                CheckFields(context, @type);

                var methods = @type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).OfType<MethodBase>();
                var staticConstructors = @type.GetConstructors(BindingFlags.Static | BindingFlags.NonPublic).OfType<MethodBase>();
                // var constructors = @type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).OfType<MethodBase>();
                // if (constructors.Count() > 1)
                //     throw new Exception($"Only 1 constructor supported (at the moment). Type : {@type} ");

                foreach (var method in methods.Concat(staticConstructors).Where(m => m.DeclaringType != typeof(object)))
                {
                    // Implemented by the runtime, not in IL (a delegate type's
                    // Invoke/BeginInvoke/EndInvoke): nothing to compile. A call
                    // to one is rejected where it is made (OpCallVirt).
                    if (!method.IsAbstract && method.GetMethodBody() == null)
                        continue;
                    // Inherited from the class library (a delegate type's
                    // MulticastDelegate members, say): not ours to compile.
                    if (method.DeclaringType != null && method.DeclaringType.Assembly != context.Assembly)
                        continue;

                    // MethodBase.IsConstructor is deliberately false for a
                    // static constructor (.NET's own docs: it "excludes
                    // type initializers") -- check the name instead.
                    if (method.IsStatic && method.Name == ".cctor")
                        context.StaticConstructorLabels.Add(method.GetLabel());

                    var methodContext = new CompilerMethodContext()
                    {
                        CompilerContext = context,
                        TypeContext = typeContext,
                        Lines = new List<ILOperation>(),
                        Method = method,
                    };
                    context.Methods.Add(methodContext);

                    // One method failing must not stop the others from being
                    // checked: every unsupported construct in the program is
                    // reported in a single build.
                    try
                    {
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
                    catch (UnsupportedFeatureException e)
                    {
                        Report(context, methodContext, e.Code, e.Message, e.Hint);
                    }
                    catch (NotSupportedException e)
                    {
                        // Thrown by passes that already say what is unsupported
                        // (string interpolation shapes, ...): not a compiler bug.
                        Report(context, methodContext, DiagnosticCodes.UnsupportedInstruction, e.Message, null);
                    }
                    catch (Exception e) when (e is not CompilationFailedException)
                    {
                        // A crash inside the compiler itself. Whatever the C#
                        // was doing is most likely a construct nothing here
                        // handles, so say that, with the location.
                        Report(context, methodContext, DiagnosticCodes.InternalError,
                            $"The compiler failed on this method ({e.GetType().Name}: {FirstLine(e.Message)}).",
                            "This C# construct is probably not supported. Simplify the code at the reported line, and report it if it should work.");
                    }
                }
            }
        }

        // Nothing is worth assembling if anything above was rejected.
        context.Diagnostics.ThrowIfErrors();
    }

    private static string FirstLine(string text) => (text ?? "").Split((char)10)[0].Trim();

    private static void Report(CompilerContext context, CompilerMethodContext methodContext, string code, string message, string hint)
    {
        var method = methodContext.Method;
        var location = context.SourceLocator.Locate(method, methodContext.CurrentIlOffset);
        context.Diagnostics.Add(new CompilerDiagnostic
        {
            Code = code,
            Message = message,
            Hint = hint,
            File = location?.file,
            Line = location?.line ?? 0,
            Method = $"{method.DeclaringType?.Name}.{method.Name}",
        });
    }

    // Field types can't be reached through a method body, so check them
    // separately. Compiler-generated holders (static array data) hold
    // value-type fields by design and are never real program state.
    private static void CheckFields(CompilerContext context, Type type)
    {
        if (type.Name.StartsWith("<PrivateImplementationDetails>"))
            return;

        var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        foreach (var field in type.GetFields(flags))
        {
            if (field.IsLiteral)
                continue;
            var problem = UnsupportedTypes.Check(field.FieldType, context.Assembly, $"The field '{field.Name}'");
            if (problem == null)
                continue;
            context.Diagnostics.Add(new CompilerDiagnostic
            {
                Code = problem.Code,
                Message = problem.Message,
                Hint = problem.Hint,
                Method = type.Name,
            });
        }
    }
}
