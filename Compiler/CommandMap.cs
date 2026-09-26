using System.Collections.Generic;
using System.Reflection.Metadata;
using Compiler.Ops;
namespace Compiler;

internal static class CommandMap
{
    private static Dictionary<ILOpCode, OpBase> map
        = new Dictionary<ILOpCode, OpBase>
        {
            { ILOpCode.Ldstr, new OpLdstr() },
            { ILOpCode.Ldtoken, new OpLdtoken() },
            { ILOpCode.Call, new OpCall() },
            { ILOpCode.Callvirt, new OpCallVirt() },
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
            { ILOpCode.Stelem_ref, new OpStElem() },
            { ILOpCode.Ldelem_ref, new OpLdElem() },
            // byte[]/sbyte[]/bool[] elements are 1 byte, exactly like this
            // compiler's uint[]/int[]: the array ops pick their width from the
            // element TYPE (GetStorageBytes), never from the opcode, so the
            // 1-byte-element opcodes just reuse the same operations.
            { ILOpCode.Stelem_i1, new OpStElem() },
            { ILOpCode.Ldelem_i1, new OpLdElem() },
            { ILOpCode.Ldelem_u1, new OpLdElem() },
            { ILOpCode.Stelem_i4, new OpStElem() },
            { ILOpCode.Ldelem_i4, new OpLdElem() },
            // Ldelem_u4 is IL's uint[]-read opcode -- stelem has no
            // unsigned variant (a raw store doesn't care about sign), and
            // OpLdElem itself picks the actual macro purely from the
            // array's element Type (see its Emit), never from which
            // Ldelem_* variant dispatched here, so reusing it is exactly
            // as correct as the existing Ldelem_i4 mapping.
            { ILOpCode.Ldelem_u4, new OpLdElem() },
            { ILOpCode.Stelem_i8, new OpStElem() },
            { ILOpCode.Ldelem_i8, new OpLdElem() },
            // Same reasoning as Ldelem_u4 above -- OpStElem/OpLdElem pick
            // their macro purely from the array's actual element Type, so
            // reusing them for the float-array-specific opcodes is exact.
            { ILOpCode.Stelem_r4, new OpStElem() },
            { ILOpCode.Ldelem_r4, new OpLdElem() },
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
            { ILOpCode.Ldarg_s, new OpLdarg_s() },
            // "x.ToString()" on a value type compiles to ldloca.s/ldarga.s
            // (address of the value) + constrained. + callvirt
            // Object::ToString() -- see ILNumericToStringPass, which
            // collapses the constrained./callvirt pair into a single
            // conversion-routine call. Ldloca_s/Ldarga_s themselves just
            // reuse OpLdloc_s/OpLdarg_s verbatim (no separate "address of"
            // Op class): this compiler has no ref/out/pointer support at
            // all, so "address of a local" and "value of a local" are
            // interchangeable for every case that can otherwise compile
            // here -- OpLdloc_s/OpLdarg_s already push exactly the value
            // (including the float SizeSuffix handling), purely from the
            // local/arg index in operation.OriginalParameter, never
            // inspecting operation.OpCode.
            { ILOpCode.Ldloca_s, new OpLdloc_s() },
            { ILOpCode.Ldarga_s, new OpLdarg_s() },
            // "instanceField.ToString()"/"staticField.ToString()" use
            // Ldflda/Ldsflda (address of the field) instead of Ldloca_s/
            // Ldarga_s -- same reasoning, same reuse trick: OpLdfld/OpLdsld
            // already push exactly the field's value with the right
            // pop-this/push-value stack behavior.
            { ILOpCode.Ldflda, new OpLdfld() },
            { ILOpCode.Ldsflda, new OpLdsld() },
            { ILOpCode.Constrained, new OpConstrained() },
            { ILOpCode.Ldsfld, new OpLdsld() },
            { ILOpCode.Br_s, new OpShortJump("jmp", JumpType.UnConditional) },
            { ILOpCode.Beq, new OpLongJump("#branch_equal", JumpType.Compare) },
            { ILOpCode.Beq_s, new OpShortJump("#branch_equal", JumpType.Compare) },
            { ILOpCode.Bne_un_s, new OpShortJump("#branch_not_equal", JumpType.Compare) },
            { ILOpCode.Bne_un, new OpLongJump("#branch_not_equal", JumpType.Compare) },
            { ILOpCode.Blt, new OpLongJump("#branch_less", JumpType.Compare) },
            { ILOpCode.Blt_s, new OpShortJump("#branch_less", JumpType.Compare) },
            { ILOpCode.Blt_un, new OpLongJump("#branch_less_unsigned", JumpType.Compare) },
            { ILOpCode.Blt_un_s, new OpShortJump("#branch_less_unsigned", JumpType.Compare) },
            { ILOpCode.Ble, new OpLongJump("#branch_less_equal", JumpType.Compare) },
            { ILOpCode.Ble_s, new OpShortJump("#branch_less_equal", JumpType.Compare) },
            { ILOpCode.Ble_un_s, new OpShortJump("#branch_less_equal_unsigned", JumpType.Compare) },
            { ILOpCode.Ble_un, new OpLongJump("#branch_less_equal_unsigned", JumpType.Compare) },
            { ILOpCode.Bge_un_s, new OpShortJump("#branch_greater_equal_unsigned", JumpType.Compare) },
            { ILOpCode.Bge_un, new OpLongJump("#branch_greater_equal_unsigned", JumpType.Compare) },
            { ILOpCode.Bge_s, new OpShortJump("#branch_greater_equal", JumpType.Compare) },
            { ILOpCode.Bge, new OpLongJump("#branch_greater_equal", JumpType.Compare) },
            // Bgt previously read "#branch_greter" here (a typo -- no asm/
            // helper/branch.asm macro of that name has ever existed, unlike
            // Bgt_s's correctly-spelled "#branch_greater" right below it),
            // so the long-jump (far-branch) form of `>` silently crashed
            // with an undefined-symbol assembler error if ever actually
            // hit. Fixed to match Bgt_s's spelling -- found and fixed
            // incidentally while adding 16-bit signed branch support below,
            // which needed branch_greater16 to exist under one consistent
            // name rather than duplicating the typo.
            { ILOpCode.Bgt, new OpLongJump("#branch_greater", JumpType.Compare) },
            { ILOpCode.Bgt_s, new OpShortJump("#branch_greater", JumpType.Compare) },
            // Bgt_un (long-jump unsigned `>`) was missing entirely -- only
            // its short-jump sibling Bgt_un_s was registered, so a far
            // unsigned-greater-than branch would have hit
            // ILMethodCodePass's NotSupportedException. Found alongside the
            // Bgt fix above.
            { ILOpCode.Bgt_un, new OpLongJump("#branch_greater_unsigned", JumpType.Compare) },
            { ILOpCode.Bgt_un_s, new OpShortJump("#branch_greater_unsigned", JumpType.Compare) },
            { ILOpCode.Br, new OpLongJump("jmp", JumpType.UnConditional) },
            { ILOpCode.Brtrue_s, new OpShortJump("#branch_true", JumpType.Conditional) },
            { ILOpCode.Brtrue, new OpLongJump("#branch_true", JumpType.Conditional) },
            { ILOpCode.Brfalse_s, new OpShortJump("#branch_false", JumpType.Conditional) },
            { ILOpCode.Brfalse, new OpLongJump("#branch_false", JumpType.Conditional) },
            { ILOpCode.Nop, new OpBase(0, "; nop") },
            { ILOpCode.Add, new OpArithmetic2("#add") },
            { ILOpCode.Mul, new OpArithmetic2("#mul") },
            { ILOpCode.Shl, new OpArithmetic2("#shift_left") },
            { ILOpCode.Shr, new OpArithmetic2("#shift_right") },
            { ILOpCode.Shr_un, new OpArithmetic2("#shift_right") },
            { ILOpCode.Sub, new OpArithmetic2("#sub") },
            // #div8/#div16 (signed) and #div_unsigned8/#div_unsigned16 are
            // asm/helper/division.asm's shift-subtract routines; #divflt
            // (float) is asm/helper/float.asm's BASIC-ROM-backed one --
            // OpArithmetic2's SizeSuffix already picks "8"/"16"/"flt" purely
            // from operand width/type, so Div/Div_un only need the right
            // Command prefix here, no special-casing.
            { ILOpCode.Div, new OpArithmetic2("#div") },
            { ILOpCode.Div_un, new OpArithmetic2("#div_unsigned") },
            { ILOpCode.Rem, new OpArithmetic2("#rem") },
            { ILOpCode.Rem_un, new OpArithmetic2("#rem_unsigned") },
            { ILOpCode.Neg, new OpArithmetic1("#negate") },
            { ILOpCode.Not, new OpArithmetic1("#not") },
            { ILOpCode.Starg_s, new OpStarg_s() },
            { ILOpCode.And, new OpArithmetic2("#and") },
            // Same reasoning as Div/Div_un above -- Or/Xor reuse OpArithmetic2
            // verbatim, just like And; C# has no `|`/`^` operator on float, so
            // Roslyn can never emit these with a float operand and #orflt/
            // #xorflt are never needed (unlike #divflt above).
            { ILOpCode.Or, new OpArithmetic2("#or") },
            { ILOpCode.Xor, new OpArithmetic2("#xor") },
            { ILOpCode.Ret, new OpRet()  },
            { ILOpCode.Clt, new OpCompare("#compareLess") },
            { ILOpCode.Clt_un, new OpCompare("#compareLess_unsigned") },
            { ILOpCode.Cgt, new OpCompare("#compareGreater") },
            { ILOpCode.Cgt_un, new OpCompare("#compareGreater_unsigned") },
            { ILOpCode.Ceq, new OpCompare("#compareEqual") },
            { ILOpCode.Switch, new OpSwitch() },
            { ILOpCode.Conv_i8, new OpConv_8_16() },
            { ILOpCode.Conv_u8, new OpConv_8_16() },
            { ILOpCode.Conv_u4, new OpConv_16_8() },
            { ILOpCode.Conv_i4, new OpConv_16_8()  },
            // (byte)x / (sbyte)x: this compiler's int/uint are already 8 bits,
            // so narrowing to a byte is the same "keep the low byte" step as
            // narrowing a 16-bit value to int (and a no-op for an 8-bit source).
            { ILOpCode.Conv_u1, new OpConv_16_8() },
            { ILOpCode.Conv_i1, new OpConv_16_8() },
            { ILOpCode.Conv_r4, new OpConvIntToFloat() },
            // Roslyn emits Conv_r_un (not Conv_r4) for an implicit uint ->
            // float conversion (e.g. "float f = someUint;" or a uint
            // return coerced to a float-returning method) -- same handler,
            // it already branches on the source being uint vs int.
            { ILOpCode.Conv_r_un, new OpConvIntToFloat() },
            { ILOpCode.Ldc_r4, new OpLdc_r4() },
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