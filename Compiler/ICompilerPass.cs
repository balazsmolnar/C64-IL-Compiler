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

        public bool Optimize { get; set; }
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

        public int GetLocalVariableReferencePosition(int index)
        {
            bool isInstance = !Method.IsStatic;
            int relPos = isInstance ? 1 : 0;
            for (int i = 0; i < Method.GetParameters().Length; i++)
                relPos += Method.GetParameters()[i].ParameterType.GetStorageBytes();

            var body = Method.GetMethodBody();
            var variables = body.LocalVariables;
            for (int i = 0; i <= index; i++)
                relPos += variables[i].LocalType.GetStorageBytes();
            return relPos;
        }

        public int GetParameterReferencePosition(int index)
        {
            int relPos = 0;
            var parameters = Method.GetParameters();
            bool isInstance = !Method.IsStatic;

            if (isInstance && index == 0)
                return 1;

            if (isInstance)
                relPos = 1;

            for (int i = 0; i <= index - (isInstance ? 1 : 0); i++)
            {
                relPos += parameters[i].ParameterType.GetStorageBytes();
            }
            return relPos;
        }

        public int GetLocalStackSize()
        {
            bool isInstance = !Method.IsStatic;
            int relPos = isInstance ? 1 : 0;
            for (int i = 0; i < Method.GetParameters().Length; i++)
                relPos += Method.GetParameters()[i].ParameterType.GetStorageBytes();

            relPos += GetLocalsSize();
            return relPos + 2;
        }

        public int GetParameterSize(int index)
        {
            var parameters = Method.GetParameters();
            bool isInstance = !Method.IsStatic;

            if (isInstance && index == 0)
                return 1;

            return parameters[index - (isInstance ? 1 : 0)].ParameterType.GetStorageBytes();
        }

        public int GetLocalsSize()
        {
            int size = 0;
            var body = Method.GetMethodBody();
            var variables = body.LocalVariables;
            for (int i = 0; i < variables.Count; i++)
                size += variables[i].LocalType.GetStorageBytes();
            return size;
        }
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