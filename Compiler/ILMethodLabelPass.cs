using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Linq;

namespace Compiler
{
    class ILMethodLabelPass: ICompilerPass
    {
        public void Execute(CompilerContext context)
        {
            Dictionary<int, string> labelPositions = new Dictionary<int, string>();
            int labelNum = 0;
            var lines = context.Lines;

            foreach (var line in lines)
            {
                if (line.OpCode.IsBranch())
                {
                    var target = (int)line.Parameter+line.Position ;
                    var label = $"label_{context.Method.Name}_{target}";
                    if (!labelPositions.ContainsKey(target))
                        labelPositions.Add(target, label);
                    line.Parameter = label;
                    labelNum++;
                }
            }

            foreach (var line in lines)
            {
                if (labelPositions.ContainsKey(line.Position)) {
                    line.Label = labelPositions[line.Position];
                    labelPositions.Remove(line.Position);
                }
            }

            if (labelPositions.Count > 0) {
                throw new InvalidOperationException("Missing label." + labelPositions.First().ToString());
            }
        }         
    }    
}
