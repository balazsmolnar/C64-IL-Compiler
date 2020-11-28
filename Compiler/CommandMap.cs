using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using Compiler.Ops;

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

        public static object ShortJumpConverter(OpCode opCode, CompilerContext context, int? parameter) => (parameter.Value < 128 ? (int)parameter : (int)parameter-256) + 2;

        public static object LongJumpConverter(OpCode opCode, CompilerContext context, int? parameter) => parameter.Value + 5;

        public static object CallConverter(OpCode opCode, CompilerContext context, int? parameter) => context.Assembly.ManifestModule.ResolveMethod(parameter.Value).GetLabel();

        public static object RtsConverter(OpCode opCode, CompilerContext context, int? parameter) => $".{context.Method.GetLabel()}_ReturnAddress";

    }

    internal static class CommandMap
    {
        private static Dictionary<ILOpCode, OpBase> map
            = new Dictionary<ILOpCode, OpBase>
            {
                { ILOpCode.Ldstr, new OpLdstr() },
                { ILOpCode.Call, new OpCall() },
                { ILOpCode.Ldc_i4_0, new OpLdc_i4_const(0) },
                { ILOpCode.Ldc_i4_1, new OpLdc_i4_const(1) },
                { ILOpCode.Ldc_i4_2, new OpLdc_i4_const(2) },
                { ILOpCode.Ldc_i4_3, new OpLdc_i4_const(3) },
                { ILOpCode.Ldc_i4_4, new OpLdc_i4_const(4) },
                { ILOpCode.Ldc_i4_5, new OpLdc_i4_const(5) },
                { ILOpCode.Ldc_i4_6, new OpLdc_i4_const(6) },
                { ILOpCode.Ldc_i4_7, new OpLdc_i4_const(7) },
                { ILOpCode.Ldc_i4_8, new OpLdc_i4_const(8) },
                { ILOpCode.Ldc_i4_m1, new OpLdc_i4_const(0xFFFF) },
                { ILOpCode.Ldc_i4_s, new OpLdc_i4_s() },
                { ILOpCode.Ldc_i4, new OpLdc_i4() },
                { ILOpCode.Stloc_0, new OpStloc(0) },
                { ILOpCode.Stloc_1, new OpStloc(1) },
                { ILOpCode.Stloc_2, new OpStloc(2) },
                { ILOpCode.Stloc_3, new OpStloc(3) },
                { ILOpCode.Stloc_s, new OpStloc_s() },
                { ILOpCode.Ldloc_0, new OpLdloc(0) },
                { ILOpCode.Ldloc_1, new OpLdloc(1) },
                { ILOpCode.Ldloc_2, new OpLdloc(2) },
                { ILOpCode.Ldloc_3, new OpLdloc(3) },
                { ILOpCode.Ldloc_s, new OpLdloc_s() },
                { ILOpCode.Ldarg_0, new OpLdarg(0) },
                { ILOpCode.Ldarg_1, new OpLdarg(1) },
                { ILOpCode.Ldarg_2, new OpLdarg(2) },
                { ILOpCode.Br_s, new OpShortJump("jmp") },
                { ILOpCode.Br, new OpLongJump("jmp") },
                { ILOpCode.Brtrue_s, new OpShortJump("+branch_true") },
                { ILOpCode.Brfalse_s, new OpShortJump("+branch_false") },
                { ILOpCode.Nop, new OpBase(0, "nop") },
                { ILOpCode.Add, new OpArithmetic2("+add16") },
                { ILOpCode.Neg, new OpArithmetic1("+negate16") },
                { ILOpCode.Ret, new OpRet()  },
                { ILOpCode.Clt, new OpArithmetic2("+compareLess16") },
                { ILOpCode.Ceq, new OpArithmetic2("+compareEqual16") },
            };
    
        public static bool Supported(ILOpCode code) {
            return map.ContainsKey(code);
        }

        public static OpBase Get(ILOpCode opCode) {
            return map[opCode];
        }
    }
}