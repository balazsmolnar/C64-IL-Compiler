using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Compiler
{
    class CompilerContext
    {
        public Assembly Assembly { get; set; }
        public StreamWriter OutputFile { get; set; }
        public IList<CompilerMethodContext> Methods { get; set; }
        public Dictionary<int, string> StringValues { get; set; } = new Dictionary<int, string>();
    }

    class CompilerTypeContext
    {
        public CompilerContext CompilerContext { get; set; }
        public Type Type { get; set; }
    }
    class CompilerMethodContext
    {
        public CompilerContext CompilerContext { get; set; }
        public MethodInfo Method { get; set; }
        public ParameterInfo[] MethodParameters { get; set; }
        public IList<ILLine> Lines { get; set; }
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

}