using System.Collections.Generic;
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
                { ILOpCode.Ldc_i4_m1, new OpLdc_i4_const(-1) },
                { ILOpCode.Ldc_i4_s, new OpLdc_i4_s() },
                { ILOpCode.Ldc_i4, new OpLdc_i4() },
                { ILOpCode.Ldnull, new OpLdnull() },
                { ILOpCode.Newobj, new OpNewObj() },
                { ILOpCode.Newarr, new OpNewArr() },
                { ILOpCode.Ldftn, new OpLdftn() },
                { ILOpCode.Dup, new OpDup() },
                { ILOpCode.Pop, new OpPop() },
                { ILOpCode.Stloc_0, new OpStloc(0) },
                { ILOpCode.Stloc_1, new OpStloc(1) },
                { ILOpCode.Stloc_2, new OpStloc(2) },
                { ILOpCode.Stloc_3, new OpStloc(3) },
                { ILOpCode.Stloc_s, new OpStloc_s() },
                { ILOpCode.Stsfld, new OpStsfld() },
                { ILOpCode.Stfld, new OpStfld() },
                { ILOpCode.Stelem_ref, new OpStElem(true) },
                { ILOpCode.Ldelem_ref, new OpLdElem(true, typeof(object)) },
                { ILOpCode.Stelem_i4, new OpStElem(false) },
                { ILOpCode.Ldelem_i4, new OpLdElem(false, typeof(int)) },
                { ILOpCode.Ldlen, new OpLdLen() },
                { ILOpCode.Ldfld, new OpLdfld() },
                { ILOpCode.Ldloc_0, new OpLdloc(0) },
                { ILOpCode.Ldloc_1, new OpLdloc(1) },
                { ILOpCode.Ldloc_2, new OpLdloc(2) },
                { ILOpCode.Ldloc_3, new OpLdloc(3) },
                { ILOpCode.Ldloc_s, new OpLdloc_s() },
                { ILOpCode.Ldarg_0, new OpLdarg(0) },
                { ILOpCode.Ldarg_1, new OpLdarg(1) },
                { ILOpCode.Ldarg_2, new OpLdarg(2) },
                { ILOpCode.Ldarg_3, new OpLdarg(3) },
                { ILOpCode.Ldsfld, new OpLdsld() },
                { ILOpCode.Br_s, new OpShortJump("jmp", JumpType.UnConditional) },
                { ILOpCode.Beq, new OpShortJump("#branch_equal", JumpType.Compare) },
                { ILOpCode.Beq_s, new OpShortJump("#branch_equal", JumpType.Compare) },
                { ILOpCode.Bne_un_s, new OpShortJump("#branch_not_equal", JumpType.Compare) },
                { ILOpCode.Bne_un, new OpShortJump("#branch_not_equal", JumpType.Compare) },
                { ILOpCode.Blt, new OpShortJump("#branch_less", JumpType.Compare) },
                { ILOpCode.Blt_s, new OpShortJump("#branch_less", JumpType.Compare) },
                { ILOpCode.Blt_un, new OpShortJump("#branch_less_unsigned", JumpType.Compare) },
                { ILOpCode.Blt_un_s, new OpShortJump("#branch_less_unsigned", JumpType.Compare) },
                { ILOpCode.Ble, new OpShortJump("#branch_less_equal", JumpType.Compare) },
                { ILOpCode.Ble_s, new OpShortJump("#branch_less_equal", JumpType.Compare) },
                { ILOpCode.Ble_un_s, new OpShortJump("#branch_less_equal_unsigned", JumpType.Compare) },
                { ILOpCode.Ble_un, new OpShortJump("#branch_less_equal_unsigned", JumpType.Compare) },
                { ILOpCode.Bge_un_s, new OpShortJump("#branch_greater_equal_unsigned", JumpType.Compare) },
                { ILOpCode.Bge_un, new OpShortJump("#branch_greater_equal_unsigned", JumpType.Compare) },
                { ILOpCode.Bge_s, new OpShortJump("#branch_greater_equal", JumpType.Compare) },
                { ILOpCode.Bge, new OpShortJump("#branch_greater_equal", JumpType.Compare) },
                { ILOpCode.Bgt, new OpShortJump("#branch_greter", JumpType.Compare) },
                { ILOpCode.Bgt_s, new OpShortJump("#branch_greater", JumpType.Compare) },
                { ILOpCode.Bgt_un_s, new OpShortJump("#branch_greater_unsigned", JumpType.Compare) },
                { ILOpCode.Br, new OpLongJump("jmp", JumpType.UnConditional) },
                { ILOpCode.Brtrue_s, new OpShortJump("#branch_true", JumpType.Conditional) },
                { ILOpCode.Brtrue, new OpLongJump("#branch_true", JumpType.Conditional) },
                { ILOpCode.Brfalse_s, new OpShortJump("#branch_false", JumpType.Conditional) },
                { ILOpCode.Brfalse, new OpLongJump("#branch_false", JumpType.Conditional) },
                { ILOpCode.Nop, new OpBase(0, "; nop") },
                { ILOpCode.Add, new OpArithmetic2("#add") },
                { ILOpCode.Shl, new OpArithmetic2("#shift_left") },
                { ILOpCode.Sub, new OpArithmetic2("#sub") },
                { ILOpCode.Neg, new OpArithmetic1("#negate") },
                { ILOpCode.And, new OpArithmetic2("#and8") },
                { ILOpCode.Ret, new OpRet()  },
                { ILOpCode.Clt, new OpCompare("#compareLess8") },
                { ILOpCode.Clt_un, new OpCompare("#compareLess8_unsigned") },
                { ILOpCode.Cgt, new OpCompare("#compareGreater") },
                { ILOpCode.Cgt_un, new OpCompare("#compareGreater_unsigned") },
                { ILOpCode.Switch, new OpSwitch() },

                { ILOpCode.Ceq, new OpCompare("#compareEqual") },
                { ILOpCode.Conv_i8, new OpBase(0, "nop ;Conv_i8") },
                { ILOpCode.Conv_u8, new OpBase(0, "nop ;Conv_u8") },
                { ILOpCode.Conv_u4, new OpBase(0, "nop ;Conv_u4") },
                { ILOpCode.Conv_i4, new OpBase(0, "nop ;Conv_i4") },
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