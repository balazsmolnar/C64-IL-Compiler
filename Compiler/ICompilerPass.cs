using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Compiler
{
    class CompilerContext 
    {
        public Assembly Assembly { get; set; }
        public MethodInfo Method { get; set; }
        public StreamWriter OutputFile { get; set; }
        public IList<ILLine> Lines { get; set; }
        public Dictionary<int, string> StringValues { get; set; } = new Dictionary<int, string>();
    }

    interface ICompilerPass
    {
        void Execute(CompilerContext context);
    }

}