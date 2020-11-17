using System.Net.WebSockets;
using System.Threading.Tasks.Dataflow;
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
    class ILMethodCodePass: ICompilerPass
    {
        public void Execute(CompilerContext context)
        {
            var body = context.Method.GetMethodBody();
            var input = body.GetILAsByteArray();
            var lines = new List<ILLine>();
            var index = 0;
            while (index < input.Length) {

                ILOpCode opCode;
                ILLine line = new ILLine();
                line.Position = index;

                if (input[index] >= 254) {
                    opCode = (ILOpCode)(input[index++]*256 + input[index++]);                    
                } else {
                    opCode = (ILOpCode)(input[index++]);
                }
                line.OpCode = opCode;
                                
                if (!CommandMap.Supported(opCode))
                    throw new NotSupportedException($"Command is not supported: {opCode.ToString()}");

                switch (opCode)
                {
                    case ILOpCode.Ldstr: {
                        var token = input[index++]+256*input[index++]+256*256*input[index++]+256*256*256*input[index++];
                        line.Parameter = $".string_{token}";
                        context.StringValues.Add(token, context.Assembly.ManifestModule.ResolveString(token));
                        break;
                    }

                    case ILOpCode.Call: {
                        var token = input[index++]+256*input[index++]+256*256*input[index++]+256*256*256*input[index++];
                        line.Parameter = context.Assembly.ManifestModule.ResolveMethod(token).GetLabel();
                        break;
                    }

                    case ILOpCode.Ldc_i4_s: {
                        var param = input[index++];
                        line.Parameter = param;
                        break;
                    }

                    case ILOpCode.Br_s: 
                    case ILOpCode.Brtrue_s: {
                        var target = input[index++];
                        line.Parameter = target < 128 ? (int)target : (int)target-256;
                        break;
                    }
                    case ILOpCode.Stloc_0:
                    case ILOpCode.Ldloc_0:
                        line.Parameter = $".{context.Method.GetLabel()}_var0";
                        break;                        
                    case ILOpCode.Stloc_1:
                    case ILOpCode.Ldloc_1:
                        line.Parameter = $".{context.Method.GetLabel()}_var1";
                        break;                        
                    case ILOpCode.Stloc_2:
                    case ILOpCode.Ldloc_2:
                        line.Parameter = $".{context.Method.GetLabel()}_var2";
                        break;                        
                    case ILOpCode.Stloc_3:
                    case ILOpCode.Ldloc_3:
                        line.Parameter = $".{context.Method.GetLabel()}_var3";
                        break;                        
                }
                line.Size = index-line.Position;
                lines.Add(line);
            }
            context.Lines = lines.ToArray();
        }       
    }    
}
