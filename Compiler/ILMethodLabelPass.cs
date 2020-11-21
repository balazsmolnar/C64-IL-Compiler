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
            ILOpCode[] shortBranches = new [] { ILOpCode.Br_s, ILOpCode.Brtrue_s, ILOpCode.Brfalse_s };
            int labelNum = 0;
            var lines = context.Lines;

            foreach (var line in lines)
            {
                if (shortBranches.Contains(line.OpCode))
                {
                    var target = (int)line.Parameter+line.Position + 2;
                    var label = $"label_{context.Method.Name}_{labelNum}";
                    labelPositions.Add(target, label);
                    line.Parameter = label;
                    labelNum++;
                }
            }

            foreach (var line in lines)
            {
                if (labelPositions.ContainsKey(line.Position))
                    line.Label = labelPositions[line.Position];
            }
        }         
    }    
}
