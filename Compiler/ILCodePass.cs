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

namespace Compiler
{
    class ILCodePass: ICompilerPass
    {
        private IEnumerable<ICompilerPass> _methodPasses;
        public ILCodePass (IEnumerable<ICompilerPass> methodPasses)
        {
            _methodPasses = methodPasses;

        }
        public void Execute(CompilerContext context)
        {
            foreach (var method in context.Assembly.GetTypes().SelectMany(t=> t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))) 
            {
                context.Method = method;
                foreach (var pass in _methodPasses)
                {
                    pass.Execute(context);
                }
            }
        }       
    }    
}
