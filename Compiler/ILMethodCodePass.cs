using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace Compiler
{
    class ILMethodCodePass: ICompilerMethodPass
    {
        public void Execute(CompilerMethodContext context)
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

                var op = CommandMap.Get(opCode);
                int parameter = 0;
                if (op.ParameterSize == 1)
                    parameter = input[index++];
                if (op.ParameterSize == 4)
                    parameter = input[index++]+256*input[index++]+256*256*input[index++]+256*256*256*input[index++];
                line.Parameter = op.ConvertParameter(context, parameter);
                line.Size = index-line.Position;
                Console.WriteLine(line);

                lines.Add(line);
            }
            context.Lines = lines.ToArray();
        }       
    }    
}
