using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;

namespace Compiler;

class ILMethodCodePass : ICompilerMethodPass
{
    public void Execute(CompilerMethodContext context)
    {
        if (context.Method.IsAbstract)
            return;

        CheckDeclaration(context);
        Decode(context);
    }

    // Decodes context.Method's raw IL into context.Lines. Factored out of
    // Execute so ILMethodInliningPass can decode a CANDIDATE CALLEE in
    // isolation (via its own throwaway CompilerMethodContext, Method = that
    // callee) independent of ILCodePass's own method-enumeration order --
    // see that pass's own comment for why it can't just read another
    // method's context.Lines off CompilerContext.Methods. Every opcode's
    // ConvertParameter (called below) either only touches
    // context.CompilerContext.Assembly or stores a raw index/offset -- never
    // anything that depends on where the resulting ILOperation ends up
    // later -- so this produces a correct, self-contained List<ILOperation>
    // for whatever Method the passed-in context names, regardless of
    // whether that method is the one actually being compiled right now.
    public static void Decode(CompilerMethodContext context)
    {
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
            context.CurrentIlOffset = operation.Position;

            // Checked before the lookup: CommandMap.Get throws a bare
            // KeyNotFoundException for anything it doesn't know.
            if (!CommandMap.Supported(opCode))
                throw UnsupportedInstructions.For(opCode);

            operation.Operation = CommandMap.Get(operation.OpCode);
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
        context.CurrentIlOffset = null;
    }

    // Generic definitions can't be compiled (each instantiation would need its
    // own copy), and the types of everything the method declares have to be
    // ones the compiler can represent.
    private static void CheckDeclaration(CompilerMethodContext context)
    {
        var method = context.Method;
        var assembly = context.CompilerContext.Assembly;

        if (method.IsGenericMethodDefinition)
            throw new UnsupportedFeatureException(DiagnosticCodes.UnsupportedDeclaration,
                $"The generic method '{method.Name}' is not supported.",
                "Write a separate method for each type you need. Generic methods and classes with methods are not supported yet.");
        if (method.DeclaringType != null && method.DeclaringType.IsGenericTypeDefinition)
            throw new UnsupportedFeatureException(DiagnosticCodes.UnsupportedDeclaration,
                $"The method '{method.Name}' of the generic class '{method.DeclaringType.Name.Split('`')[0]}<...>' is not supported.",
                "Write a separate class for each type you need. Generic classes with methods are not supported yet (a generic class with only fields is).");

        if (method is MethodInfo info)
        {
            var returned = UnsupportedTypes.Check(info.ReturnType, assembly, "A return value");
            if (returned != null)
                throw returned;
        }
        foreach (var parameter in method.GetParameters())
        {
            var problem = UnsupportedTypes.Check(parameter.ParameterType, assembly, $"The parameter '{parameter.Name}'");
            if (problem != null)
                throw problem;
        }
        var body = method.GetMethodBody();
        if (body != null)
        {
            foreach (var local in body.LocalVariables)
            {
                var problem = UnsupportedTypes.Check(local.LocalType, assembly, "A local variable");
                if (problem != null)
                    throw problem;
            }
        }
    }
}