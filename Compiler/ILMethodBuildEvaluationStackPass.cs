using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Linq;

namespace Compiler;

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
            // The method's first instruction starts with an empty stack even
            // when it has predecessors: a method whose body begins with a
            // loop ("for (;;) { ... }" as its first statement) has only the
            // loop's back edge there, which has not been visited yet.
            if (op.PreviousInstructions.Count == 0 || op == context.Lines[0])
                op.StackContent = new List<Type>();
            else
                op.StackContent = new List<Type>(op.PreviousInstructions.First(x => x.StackContent != null).StackContent);
            context.CurrentIlOffset = op.Position;
            op.Operation.SetStackContent(context, op);
            foreach (var instruction in op.NextInstructions)
            {
                if (instruction.StackContent == null)
                    stack.Push(instruction);
            }
        }
    }
}