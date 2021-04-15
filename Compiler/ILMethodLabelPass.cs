using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Linq;

namespace Compiler
{
    class ILMethodLabelPass : ICompilerMethodPass
    {
        public void Execute(CompilerMethodContext context)
        {
            Dictionary<int, string> labelPositions = new Dictionary<int, string>();
            var lines = context.Lines;

            foreach (var line in lines)
            {
                if (line.OpCode.IsBranch())
                {
                    var target = (int)line.RawParameter + line.Position;
                    var label = $"{context.Method.GetLabel()}_{target}";
                    if (!labelPositions.ContainsKey(target))
                        labelPositions.Add(target, label);
                    line.RawParameter = label;
                }

                if (line.OpCode == ILOpCode.Switch)
                {
                    List<int> parameters = (List<int>)line.RawParameter;
                    foreach (var parameter in parameters)
                    {
                        var target = parameter + line.Position + parameters.Count * 4 + 5;
                        var label = $"{context.Method.GetLabel()}_{target}";
                        if (!labelPositions.ContainsKey(target))
                            labelPositions.Add(target, label);
                    }
                }
            }

            foreach (var line in lines)
            {
                if (labelPositions.ContainsKey(line.Position))
                {
                    line.Label = labelPositions[line.Position];
                    labelPositions.Remove(line.Position);
                }
            }

            if (labelPositions.Count > 0)
            {
                throw new InvalidOperationException("Missing label." + labelPositions.First().ToString());
            }
        }
    }
}
