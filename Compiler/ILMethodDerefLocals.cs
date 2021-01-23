using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Linq;
using Compiler.Ops;

namespace Compiler
{
    class ILMethodDerefLocals : ICompilerMethodPass
    {
        public void Execute(CompilerMethodContext context)
        {
            var lines = context.Lines;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Operand is OpRet)
                {
                    var body = context.Method.GetMethodBody();

                    foreach (var v in body.LocalVariables)
                    {
                        if (!v.LocalType.IsReferenceCounted())
                            continue;


                        ILLine newLine = new ILLine
                        {
                            Operand = new OpDeref(v.LocalIndex),
                        };
                        newLine.Parameter = newLine.Operand.ConvertParameter(context, v.LocalIndex);
                        lines.Insert(i, newLine);
                        i++;
                    }

                    foreach (var param in context.Method.GetParameters())
                    {
                        if (!param.ParameterType.IsReferenceCounted())
                            continue;

                        ILLine newLine = new ILLine
                        {
                            Operand = new OpDerefParam(param.Name),
                        };
                        newLine.Parameter = newLine.Operand.ConvertParameter(context, 0);
                        lines.Insert(i, newLine);
                        i++;
                    }
                }
            }
        }
    }
}
