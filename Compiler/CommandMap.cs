using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;

namespace Compiler
{
    internal class OpCode
    {
        public OpCode(ILOpCode opCode, string command, int parameterSize, int constParameter, Func<OpCode, CompilerContext, int?, object> converter = null)
        {
            ILOpCode = opCode;
            Command = command;
            ParameterSize = parameterSize;
            Converter = converter ?? NullConverter;;
            ConstParameter = constParameter;
        }

        public OpCode(ILOpCode opCode, string command):this(opCode, command, 0, -1, NullConverter)
        {
        }

        public ILOpCode ILOpCode { get; }
        public string Command { get; }
        public int ParameterSize { get; }
        public int ConstParameter { get; }
        public Func<OpCode, CompilerContext, int?, object> Converter { get; }

        public static object NullConverter(OpCode opCode, CompilerContext context, int? parameter) => null;

        public static object ParameterConverter(OpCode opCode, CompilerContext context, int? parameter) => parameter ?? opCode.ConstParameter;

        public static object VariableConverter(OpCode opCode, CompilerContext context, int? parameter) => $".{context.Method.GetLabel()}_var{parameter ?? opCode.ConstParameter}";

        public static object ArgConverter(OpCode opCode, CompilerContext context, int? parameter) => $".{context.Method.GetLabel()}_{context.Method.GetParameters()[parameter ?? opCode.ConstParameter].Name}";

        public static object StringConverter(OpCode opCode, CompilerContext context, int? parameter) {

            context.StringValues.Add(parameter.Value, context.Assembly.ManifestModule.ResolveString(parameter.Value));
            return $".string_{parameter}";
        }

        public static object ShortJumpConverter(OpCode opCode, CompilerContext context, int? parameter) => parameter.Value < 128 ? (int)parameter : (int)parameter-256;

        public static object CallConverter(OpCode opCode, CompilerContext context, int? parameter) => context.Assembly.ManifestModule.ResolveMethod(parameter.Value).GetLabel();
    }

    internal static class CommandMap
    {
        private static Dictionary<ILOpCode, OpCode> map
            = new Dictionary<ILOpCode, OpCode>
            {
                { ILOpCode.Ldstr, new OpCode(ILOpCode.Ldstr, "+stack_push_pointer", 4, -1, OpCode.StringConverter) },
                { ILOpCode.Call, new OpCode(ILOpCode.Call, "jsr", 4, -1, OpCode.CallConverter) },
                { ILOpCode.Ldc_i4_0, new OpCode(ILOpCode.Ldc_i4_0, "+stack_push_int", 0, 0, OpCode.ParameterConverter) },
                { ILOpCode.Ldc_i4_1, new OpCode(ILOpCode.Ldc_i4_1, "+stack_push_int", 0, 1, OpCode.ParameterConverter) },
                { ILOpCode.Ldc_i4_2, new OpCode(ILOpCode.Ldc_i4_2, "+stack_push_int", 0, 2, OpCode.ParameterConverter) },
                { ILOpCode.Ldc_i4_3, new OpCode(ILOpCode.Ldc_i4_3, "+stack_push_int", 0, 3, OpCode.ParameterConverter) },
                { ILOpCode.Ldc_i4_4, new OpCode(ILOpCode.Ldc_i4_4, "+stack_push_int", 0, 4, OpCode.ParameterConverter) },
                { ILOpCode.Ldc_i4_5, new OpCode(ILOpCode.Ldc_i4_5, "+stack_push_int", 0, 5, OpCode.ParameterConverter) },
                { ILOpCode.Ldc_i4_6, new OpCode(ILOpCode.Ldc_i4_6, "+stack_push_int", 0, 6, OpCode.ParameterConverter) },
                { ILOpCode.Ldc_i4_7, new OpCode(ILOpCode.Ldc_i4_7, "+stack_push_int", 0, 7, OpCode.ParameterConverter) },
                { ILOpCode.Ldc_i4_8, new OpCode(ILOpCode.Ldc_i4_8, "+stack_push_int", 0, 8, OpCode.ParameterConverter) },
                { ILOpCode.Ldc_i4_m1, new OpCode(ILOpCode.Ldc_i4_m1, "+stack_push_int", 0, 0xFFFF, OpCode.ParameterConverter) },
                { ILOpCode.Ldc_i4_s, new OpCode(ILOpCode.Ldc_i4_s, "+stack_push_int", 1, -1, OpCode.ParameterConverter) },
                { ILOpCode.Ldc_i4, new OpCode(ILOpCode.Ldc_i4, "+stack_push_int", 4, -1, OpCode.ParameterConverter) },
                { ILOpCode.Stloc_0, new OpCode(ILOpCode.Stloc_0, "+stack_pull_int", 0, 0, OpCode.VariableConverter) },
                { ILOpCode.Stloc_1, new OpCode(ILOpCode.Stloc_1, "+stack_pull_int", 0, 1, OpCode.VariableConverter) },
                { ILOpCode.Stloc_2, new OpCode(ILOpCode.Stloc_2, "+stack_pull_int", 0, 2, OpCode.VariableConverter) },
                { ILOpCode.Stloc_3, new OpCode(ILOpCode.Stloc_3, "+stack_pull_int", 0, 3, OpCode.VariableConverter) },
                { ILOpCode.Stloc_s, new OpCode(ILOpCode.Stloc_s, "+stack_pull_int", 1, 0, OpCode.VariableConverter) },
                { ILOpCode.Ldloc_0, new OpCode(ILOpCode.Ldloc_0, "+stack_push_var", 0, 0, OpCode.VariableConverter) },
                { ILOpCode.Ldloc_1, new OpCode(ILOpCode.Ldloc_1, "+stack_push_var", 0, 1, OpCode.VariableConverter) },
                { ILOpCode.Ldloc_2, new OpCode(ILOpCode.Ldloc_2, "+stack_push_var", 0, 2, OpCode.VariableConverter) },
                { ILOpCode.Ldloc_3, new OpCode(ILOpCode.Ldloc_3, "+stack_push_var", 0, 3, OpCode.VariableConverter) },
                { ILOpCode.Ldloc_s, new OpCode(ILOpCode.Ldloc_s, "+stack_push_var", 1, 0, OpCode.VariableConverter) },
                { ILOpCode.Ldarg_0, new OpCode(ILOpCode.Ldarg_0, "+stack_push_var", 0, 0, OpCode.ArgConverter) },
                { ILOpCode.Ldarg_1, new OpCode(ILOpCode.Ldarg_1, "+stack_push_var", 0, 1, OpCode.ArgConverter) },
                { ILOpCode.Ldarg_2, new OpCode(ILOpCode.Ldarg_2, "+stack_push_var", 0, 2, OpCode.ArgConverter) },
                { ILOpCode.Br_s, new OpCode(ILOpCode.Br_s, "jmp", 1, 0, OpCode.ShortJumpConverter) },
                { ILOpCode.Brtrue_s, new OpCode(ILOpCode.Brtrue_s, "+branch_true", 1, 0, OpCode.ShortJumpConverter) },
                { ILOpCode.Brfalse_s, new OpCode(ILOpCode.Brfalse_s, "+branch_false", 1, 0, OpCode.ShortJumpConverter) },
                { ILOpCode.Nop, new OpCode(ILOpCode.Nop, "nop") },
                { ILOpCode.Add, new OpCode(ILOpCode.Add, "+add16") },
                { ILOpCode.Ret, new OpCode(ILOpCode.Ret, "rts") },
                { ILOpCode.Clt, new OpCode(ILOpCode.Ret, "+compareLess16") },
                { ILOpCode.Ceq, new OpCode(ILOpCode.Ret, "+compareEqual16") },
            };
    
        public static bool Supported(ILOpCode code) {
            return map.ContainsKey(code);
        }

        public static OpCode Get(ILOpCode opCode) {
            return map[opCode];
        }
    }
}