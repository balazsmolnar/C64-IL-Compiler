using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.Collections;
using System.Net.WebSockets;
using System.Net;
using System.Diagnostics.Tracing;
using System.Diagnostics;
using System.Threading;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Xml;
using System.Net.Mail;
using System.Reflection.Metadata.Ecma335;
using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Linq;

namespace Compiler
{
    class ILMethodLabelPass: ICompilerPass
    {
        public void Execute(CompilerContext context)
        {
            Dictionary<int, string> labelPositions = new Dictionary<int, string>();
            ILOpCode[] shortBranches = new [] { ILOpCode.Br_s, ILOpCode.Brtrue_s };
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
