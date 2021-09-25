using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Linq;

namespace Compiler
{
    class ILMethodBuildEvaluationStackPass : ICompilerMethodPass
    {
        public void Execute(CompilerMethodContext context)
        {
            if (context.Method.IsAbstract)
                return;
            var stack = new Stack<ILOperation>();
            stack.Push(context.Lines[0]);

            while (stack.Count > 0)
            {
                var op = stack.Pop();
                if (op.StackContent != null)
                    continue;
                if (op.PreviousInstructions.Count == 0)
                    op.StackContent = new List<Type>();
                else
                    op.StackContent = new List<Type>(op.PreviousInstructions.First(x => x.StackContent != null).StackContent);
                op.Operation.SetStackContent(context, op);
                foreach (var instruction in op.NextInstructions)
                {
                    if (instruction.StackContent == null)
                        stack.Push(instruction);
                }
            }
        }
    }
}
