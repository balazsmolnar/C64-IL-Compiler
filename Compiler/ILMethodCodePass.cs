using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace Compiler
{
    class ILMethodCodePass : ICompilerMethodPass
    {
        public void Execute(CompilerMethodContext context)
        {
            if (context.Method.IsAbstract)
                return;

            var body = context.Method.GetMethodBody();
            var input = body.GetILAsByteArray();
            var lines = new List<ILOperation>();

            var index = 0;
            while (index < input.Length)
            {

                ILOpCode opCode;
                ILOperation operation = new ILOperation();
                operation.Position = index;

                if (input[index] >= 254)
                {
                    opCode = (ILOpCode)(input[index++] * 256 + input[index++]);
                }
                else
                {
                    opCode = (ILOpCode)(input[index++]);
                }
                operation.OpCode = opCode;
                operation.Operation = CommandMap.Get(operation.OpCode);

                if (!CommandMap.Supported(opCode))
                    throw new NotSupportedException($"Command is not supported: {opCode.ToString()}");

                var op = CommandMap.Get(opCode);
                int parameter = 0;
                if (op.ParameterSize == 1)
                    parameter = input[index++];
                if (op.ParameterSize == 4)
                    parameter = input[index++] + 256 * input[index++] + 256 * 256 * input[index++] + 256 * 256 * 256 * input[index++];
                operation.RawParameter = parameter;
                operation.OriginalParameter = parameter;
                if (op.ParameterSize == -1)
                {
                    var parameterSize = parameter = input[index++] + 256 * input[index++] + 256 * 256 * input[index++] + 256 * 256 * 256 * input[index++];
                    List<int> parameters = new List<int>(parameterSize);
                    for (int i = 0; i < parameterSize; i++)
                    {
                        parameters.Add(input[index++] + 256 * input[index++] + 256 * 256 * input[index++] + 256 * 256 * 256 * input[index++]);
                    }
                    operation.RawParameter = parameters;
                    operation.OriginalParameter = parameter;
                }

                if (op.ParameterSize > -1)
                    operation.RawParameter = op.ConvertParameter(context, operation);
                operation.Size = index - operation.Position;

                lines.Add(operation);
            }
            context.Lines = lines;
        }
    }
}
