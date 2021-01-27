using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Compiler
{
    class CompilerContext
    {
        public Assembly Assembly { get; set; }
        public StreamWriter GlobalOutputFile { get; set; }
        public IList<CompilerMethodContext> Methods { get; set; }
        public Dictionary<int, string> StringValues { get; set; } = new Dictionary<int, string>();

        public string OutputDirectory { get; set; }

        public int GetFieldPosition(FieldInfo field)
        {
            var t = field.DeclaringType;
            var pos = 0;
            var fields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var f in fields.Where(f => !f.IsLiteral).OrderBy(f => f.FieldType.IsReferenceCounted() ? 0 : 1))
            {
                if (f == field)
                    break;
                if (f.FieldType == typeof(string))
                {
                    pos += 2;
                }
                else
                {
                    pos += 1;
                }
            }
            return pos;
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
        public List<ILLine> Lines { get; set; }
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