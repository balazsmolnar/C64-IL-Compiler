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

namespace Compiler
{
    class ILCodePass : ICompilerPass
    {
        private readonly IEnumerable<ICompilerTypePass> _typePasses;
        private IEnumerable<ICompilerMethodPass> _methodPasses;
        public ILCodePass(IEnumerable<ICompilerTypePass> typePasses, IEnumerable<ICompilerMethodPass> methodPasses)
        {
            _typePasses = typePasses;
            _methodPasses = methodPasses;

        }
        public void Execute(CompilerContext context)
        {
            context.Methods = new List<CompilerMethodContext>();
            foreach (var @type in context.Assembly.GetTypes())
            {
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

                    var methods = @type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    foreach (var method in methods.Where(m => m.DeclaringType == @type))
                    {
                        var methodContext = new CompilerMethodContext()
                        {
                            CompilerContext = context,
                            TypeContext = typeContext,
                            Lines = new List<ILLine>(),
                            Method = method,
                        };
                        context.Methods.Add(methodContext);

                        foreach (var pass in _methodPasses)
                        {
                            pass.Execute(methodContext);
                        }
                    }
                }
            }
        }
    }
}
