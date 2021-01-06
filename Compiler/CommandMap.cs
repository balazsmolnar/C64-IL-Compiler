using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using Compiler.Ops;
namespace Compiler
{
    internal static class CommandMap
    {
        private static Dictionary<ILOpCode, OpBase> map
            = new Dictionary<ILOpCode, OpBase>
            {
                { ILOpCode.Ldstr, new OpLdstr() },
                { ILOpCode.Call, new OpCall() },
                { ILOpCode.Callvirt, new OpCall() },
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
                { ILOpCode.Ldnull, new OpLdnull() },
                { ILOpCode.Newobj, new OpNewObj() },
                { ILOpCode.Ldftn, new OpLdftn() },
                { ILOpCode.Stloc_0, new OpStloc(0) },
                { ILOpCode.Stloc_1, new OpStloc(1) },
                { ILOpCode.Stloc_2, new OpStloc(2) },
                { ILOpCode.Stloc_3, new OpStloc(3) },
                { ILOpCode.Stloc_s, new OpStloc_s() },
                { ILOpCode.Stsfld, new OpStsfld() },
                { ILOpCode.Stfld, new OpStfld() },
                { ILOpCode.Ldfld, new OpLdfld() },
                { ILOpCode.Ldloc_0, new OpLdloc(0) },
                { ILOpCode.Ldloc_1, new OpLdloc(1) },
                { ILOpCode.Ldloc_2, new OpLdloc(2) },
                { ILOpCode.Ldloc_3, new OpLdloc(3) },
                { ILOpCode.Ldloc_s, new OpLdloc_s() },
                { ILOpCode.Ldarg_0, new OpLdarg(0) },
                { ILOpCode.Ldarg_1, new OpLdarg(1) },
                { ILOpCode.Ldarg_2, new OpLdarg(2) },
                { ILOpCode.Ldsfld, new OpLdsld() },
                { ILOpCode.Br_s, new OpShortJump("jmp") },
                { ILOpCode.Br, new OpLongJump("jmp") },
                { ILOpCode.Brtrue_s, new OpShortJump("+branch_true") },
                { ILOpCode.Brfalse_s, new OpShortJump("+branch_false") },
                { ILOpCode.Nop, new OpBase(0, "nop") },
                { ILOpCode.Add, new OpArithmetic2("+add") },
                { ILOpCode.Neg, new OpArithmetic1("+negate") },
                { ILOpCode.Ret, new OpRet()  },
                { ILOpCode.Clt, new OpArithmetic2("+compareLess") },
                { ILOpCode.Clt_un, new OpArithmetic2("+compareLess") },
                { ILOpCode.Ceq, new OpArithmetic2("+compareEqual") },
            };

        public static bool Supported(ILOpCode code)
        {
            return map.ContainsKey(code);
        }

        public static OpBase Get(ILOpCode opCode)
        {
            return map[opCode];
        }
    }
}