

;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: Draw
;----------------------------------------
Wall_Draw 
    #init_locals_pull_parameters 0, [0,0,0]
    #locals_push_value8 2 ; Ldarg_1
    jsr Wall_BuildBasicWall ; Call
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 3 ; Ldarg_2
  ; OPT   #stfld8 0 ; Stfld
    #setfld8 1, 3, 0 ; Nop
    #locals_push_value8 3 ; Ldarg_2
  ; OPT   #stack_push_int8 2 ; Ldc_i4_2
  ; OPT   #branch_not_equal8 Wall_Draw_38 ; Bne_un_s
    #branch_not_equal_const 2, Wall_Draw_38 ; Nop
    #stack_push_int8 9 ; Ldc_i4_s
    jsr Wall_BuildKnightPit ; Call
    #stack_push_int8 18 ; Ldc_i4_s
    jsr Wall_BuildKnightPit ; Call
    #stack_push_int8 27 ; Ldc_i4_s
    jsr Wall_BuildKnightPit ; Call
Wall_Draw_38:    #locals_push_value8 3 ; Ldarg_2
  ; OPT   #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #branch_not_equal8 Wall_Draw_63 ; Bne_un_s
    #branch_not_equal_const 1, Wall_Draw_63 ; Nop
    #stack_push_int8 9 ; Ldc_i4_s
    jsr Wall_BuildEmptyPit ; Call
    #stack_push_int8 18 ; Ldc_i4_s
    jsr Wall_BuildEmptyPit ; Call
    #stack_push_int8 27 ; Ldc_i4_s
    jsr Wall_BuildEmptyPit ; Call
Wall_Draw_63:    #locals_push_value8 3 ; Ldarg_2
  ; OPT   #stack_push_int8 3 ; Ldc_i4_3
  ; OPT   #branch_not_equal8 Wall_Draw_72 ; Bne_un_s
    #branch_not_equal_const 3, Wall_Draw_72 ; Nop
    jsr Wall_BuildRopePit ; Call
Wall_Draw_72:    #method_exit 5, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: Move
;----------------------------------------
Wall_Move 
    #init_locals_pull_parameters 2, [0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 0 ; Ldfld
    #pushfld8 0 ; Nop
  ; OPT   #stack_push_int8 2 ; Ldc_i4_2
  ; OPT   #branch_equal8 Wall_Move_10 ; Beq_s
    #branch_equal_const 2, Wall_Move_10 ; Nop
    #method_exit 5, [] ; Ret
Wall_Move_10:    #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
    #stack_push_int8 1 ; Ldc_i4_1
    #add8 ; Add
    #stfld8 3 ; Stfld
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
  ; OPT   #stack_push_int8 2 ; Ldc_i4_2
  ; OPT   #branch_greater_equal_unsigned8 Wall_Move_34 ; Bge_un_s
    #branch_greater_equal_unsigned_const 2, Wall_Move_34 ; Nop
    #method_exit 5, [] ; Ret
Wall_Move_34:    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 0 ; Ldc_i4_0
    #stfld8 3 ; Stfld
    #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 1 ; Ldfld
    #pushfld8 1 ; Nop
    #stack_push_int8 1 ; Ldc_i4_1
    #add8 ; Add
    #stfld8 1 ; Stfld
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 1 ; Ldfld
    #pushfld8 1 ; Nop
  ; OPT   #stack_push_int8 5 ; Ldc_i4_5
  ; OPT   #branch_not_equal8 Wall_Move_101 ; Bne_un_s
    #branch_not_equal_const 5, Wall_Move_101 ; Nop
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 0 ; Ldc_i4_0
    #stfld8 1 ; Stfld
    #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int8 1 ; Ldc_i4_1
    #add8 ; Add
    #stfld8 2 ; Stfld
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
  ; OPT   #stack_push_int8 6 ; Ldc_i4_6
  ; OPT   #branch_less_equal_unsigned8 Wall_Move_101 ; Ble_un_s
    #branch_less_equal_unsigned_const 6, Wall_Move_101 ; Nop
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 0 ; Ldc_i4_0
    #stfld8 2 ; Stfld
Wall_Move_101:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
  ; OPT   #stack_push_int8 4 ; Ldc_i4_4
  ; OPT   #branch_greater_equal_unsigned8 Wall_Move_111 ; Bge_un_s
    #branch_greater_equal_unsigned_const 4, Wall_Move_111 ; Nop
    #method_exit 5, [] ; Ret
Wall_Move_111:  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #locals_pull_value8 2, 0 ; Stloc_0
    #init_var 2, 10 ; Nop
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
  ; OPT   #stack_push_int8 4 ; Ldc_i4_4
  ; OPT   #branch_not_equal8 Wall_Move_126 ; Bne_un_s
    #branch_not_equal_const 4, Wall_Move_126 ; Nop
  ; OPT   #stack_push_int8 28 ; Ldc_i4_s
  ; OPT   #locals_pull_value8 2, 0 ; Stloc_0
    #init_var 2, 28 ; Nop
Wall_Move_126:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
  ; OPT   #stack_push_int8 5 ; Ldc_i4_5
  ; OPT   #branch_not_equal8 Wall_Move_138 ; Bne_un_s
    #branch_not_equal_const 5, Wall_Move_138 ; Nop
  ; OPT   #stack_push_int8 19 ; Ldc_i4_s
  ; OPT   #locals_pull_value8 2, 0 ; Stloc_0
    #init_var 2, 19 ; Nop
Wall_Move_138:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 1 ; Ldfld
    #pushfld8 1 ; Nop
    #locals_pull_value8 3, 0 ; Stloc_1
    #locals_push_value8 3 ; Ldloc_1
    #switch Wall_Move_Jump_146 ; Switch
    #method_exit 5, [] ; Ret
Wall_Move_172:    #locals_push_value8 2 ; Ldloc_0
    jsr Wall_KnightPitFrame1 ; Call
    #method_exit 5, [] ; Ret
Wall_Move_179:    #locals_push_value8 2 ; Ldloc_0
    jsr Wall_KnightPitFrame2 ; Call
    #method_exit 5, [] ; Ret
Wall_Move_186:    #locals_push_value8 2 ; Ldloc_0
    jsr Wall_KnightPitFrame3 ; Call
    #method_exit 5, [] ; Ret
Wall_Move_Jump_146 .word  Wall_Move_172-1,  Wall_Move_179-1,  Wall_Move_186-1,  Wall_Move_179-1,  Wall_Move_172-1, 0


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: IsHole
;----------------------------------------
Wall_IsHole 
    #init_locals_pull_parameters 0, [0, 0,0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 0 ; Ldfld
    #pushfld8 0 ; Nop
  ; OPT   #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #branch_equal8 Wall_IsHole_18 ; Beq_s
    #branch_equal_const 1, Wall_IsHole_18 ; Nop
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 0 ; Ldfld
    #pushfld8 0 ; Nop
  ; OPT   #stack_push_int8 2 ; Ldc_i4_2
  ; OPT   #branch_not_equal8 Wall_IsHole_72 ; Bne_un_s
    #branch_not_equal_const 2, Wall_IsHole_72 ; Nop
Wall_IsHole_18:    #locals_push_value16 3 ; Ldarg_1
    #stack_push_int16 92 ; Ldc_i4_s
    ; conv ; Conv_i8
    #branch_less_equal_unsigned16 Wall_IsHole_32 ; Ble_un_s
    #locals_push_value16 3 ; Ldarg_1
    #stack_push_int16 114 ; Ldc_i4_s
    ; conv ; Conv_i8
    #branch_greater_equal_unsigned16 Wall_IsHole_32 ; Bge_un_s
    #stack_push_int8 1 ; Ldc_i4_1
    #method_exit 5, [] ; Ret
Wall_IsHole_32:    #locals_push_value16 3 ; Ldarg_1
    #stack_push_int16 164 ; Ldc_i4
    ; conv ; Conv_i8
    #branch_less_equal_unsigned16 Wall_IsHole_52 ; Ble_un_s
    #locals_push_value16 3 ; Ldarg_1
    #stack_push_int16 186 ; Ldc_i4
    ; conv ; Conv_i8
    #branch_greater_equal_unsigned16 Wall_IsHole_52 ; Bge_un_s
    #stack_push_int8 1 ; Ldc_i4_1
    #method_exit 5, [] ; Ret
Wall_IsHole_52:    #locals_push_value16 3 ; Ldarg_1
    #stack_push_int16 236 ; Ldc_i4
    ; conv ; Conv_i8
    #branch_less_equal_unsigned16 Wall_IsHole_72 ; Ble_un_s
    #locals_push_value16 3 ; Ldarg_1
    #stack_push_int16 255 ; Ldc_i4
    ; conv ; Conv_i8
    #branch_greater_equal_unsigned16 Wall_IsHole_72 ; Bge_un_s
    #stack_push_int8 1 ; Ldc_i4_1
    #method_exit 5, [] ; Ret
Wall_IsHole_72:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 0 ; Ldfld
    #pushfld8 0 ; Nop
  ; OPT   #stack_push_int8 3 ; Ldc_i4_3
  ; OPT   #branch_not_equal8 Wall_IsHole_98 ; Bne_un_s
    #branch_not_equal_const 3, Wall_IsHole_98 ; Nop
    #locals_push_value16 3 ; Ldarg_1
    #stack_push_int16 106 ; Ldc_i4_s
    ; conv ; Conv_i8
    #branch_less_equal_unsigned16 Wall_IsHole_98 ; Ble_un_s
    #locals_push_value16 3 ; Ldarg_1
    #stack_push_int16 238 ; Ldc_i4
    ; conv ; Conv_i8
    #branch_greater_equal_unsigned16 Wall_IsHole_98 ; Bge_un_s
    #stack_push_int8 1 ; Ldc_i4_1
    #method_exit 5, [] ; Ret
Wall_IsHole_98:    #stack_push_int8 0 ; Ldc_i4_0
    #method_exit 5, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: BuildBasicWall
;----------------------------------------
Wall_BuildBasicWall 
    #init_locals_pull_parameters 4, [0]
  ; OPT   #stack_push_pointer string_1879050873 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer screenMemory+$01B8 ; Nop
    #stack_push_int8 62 ; Ldc_i4_s
    #stack_push_int8 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
  ; OPT   #stack_push_pointer string_1879050911 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer screenMemory+$02B8 ; Nop
    #stack_push_int8 62 ; Ldc_i4_s
    #stack_push_int8 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
  ; OPT   #stack_push_pointer string_1879050949 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer screenMemory+$0300 ; Nop
    #stack_push_int8 62 ; Ldc_i4_s
    #stack_push_int8 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
  ; OPT   #stack_push_pointer string_1879050987 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer colorMemory+$01B8 ; Nop
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
  ; OPT   #stack_push_pointer string_1879051023 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer colorMemory+$02B8 ; Nop
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
  ; OPT   #stack_push_pointer string_1879051059 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer colorMemory+$0300 ; Nop
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
  ; OPT   #stack_push_pointer string_1879051095 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer screenMemory+$0320 ; Nop
  ; OPT   #stack_push_pointer string_1879051133 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer tbl_WallBaseChars ; Nop
    #stack_push_int8 199 ; Ldc_i4
    jsr C64_CopyMemory ; Call
  ; OPT   #stack_push_int8 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value8 2, 0 ; Stloc_0
    #init_var 2, 0 ; Nop
  ; OPT   #stack_push_int8 3 ; Ldc_i4_3
  ; OPT   #locals_pull_value8 3, 0 ; Stloc_1
    #init_var 3, 3 ; Nop
    jmp Wall_BuildBasicWall_190 ; Br_s
Wall_BuildBasicWall_141:  ; OPT   #stack_push_int8 37 ; Ldc_i4_s
  ; OPT   #locals_pull_value8 4, 0 ; Stloc_2
    #init_var 4, 37 ; Nop
    jmp Wall_BuildBasicWall_181 ; Br_s
Wall_BuildBasicWall_146:  ; OPT   #stack_push_pointer string_1879051169 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer tbl_LevelEndBellChars ; Nop
    #locals_push_value8 2 ; Ldloc_0
    jsr C64_GetMemory ; Call
    #locals_pull_value8 5, 0 ; Stloc_3
    #locals_push_value8 4 ; Ldloc_2
    #locals_push_value8 3 ; Ldloc_1
    #locals_push_value8 5 ; Ldloc_3
    #stack_push_int8 15 ; Ldc_i4_s
    jsr C64_SetChar ; Call
  ; OPT   #locals_push_value8 2 ; Ldloc_0
  ; OPT   #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #add8 ; Add
  ; OPT   #locals_pull_value8 2, 0 ; Stloc_0
    #inc_var 2 ; Nop
  ; OPT   #locals_push_value8 4 ; Ldloc_2
  ; OPT   #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #add8 ; Add
  ; OPT   #locals_pull_value8 4, 0 ; Stloc_2
    #inc_var 4 ; Nop
Wall_BuildBasicWall_181:    #locals_push_value8 4 ; Ldloc_2
  ; OPT   #stack_push_int8 40 ; Ldc_i4_s
  ; OPT   #branch_less_unsigned8 Wall_BuildBasicWall_146 ; Blt_un_s
    #branch_less_unsigned_const 40, Wall_BuildBasicWall_146 ; Nop
  ; OPT   #locals_push_value8 3 ; Ldloc_1
  ; OPT   #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #add8 ; Add
  ; OPT   #locals_pull_value8 3, 0 ; Stloc_1
    #inc_var 3 ; Nop
Wall_BuildBasicWall_190:    #locals_push_value8 3 ; Ldloc_1
  ; OPT   #stack_push_int8 11 ; Ldc_i4_s
  ; OPT   #branch_less_unsigned8 Wall_BuildBasicWall_141 ; Blt_un_s
    #branch_less_unsigned_const 11, Wall_BuildBasicWall_141 ; Nop
    #method_exit 7, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: BuildKnightPit
;----------------------------------------
Wall_BuildKnightPit 
    #init_locals_pull_parameters 5, [0]
  ; OPT   #stack_push_int8 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value8 2, 0 ; Stloc_0
    #init_var 2, 0 ; Nop
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #locals_pull_value8 3, 0 ; Stloc_1
    #init_var 3, 10 ; Nop
    jmp Wall_BuildKnightPit_74 ; Br_s
Wall_BuildKnightPit_7:    #locals_push_value8 1 ; Ldarg_0
    #locals_pull_value8 4, 0 ; Stloc_2
    jmp Wall_BuildKnightPit_64 ; Br_s
Wall_BuildKnightPit_11:  ; OPT   #stack_push_pointer string_1879051213 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer tbl_KnightChars ; Nop
    #locals_push_value8 2 ; Ldloc_0
    jsr C64_GetMemory ; Call
    #locals_pull_value8 5, 0 ; Stloc_3
  ; OPT   #stack_push_pointer string_1879051245 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer tbl_KnightCharsColours ; Nop
    #locals_push_value8 2 ; Ldloc_0
    jsr C64_GetMemory ; Call
    #locals_pull_value8 6, 0 ; Stloc_s
    #locals_push_value8 4 ; Ldloc_2
    #locals_push_value8 3 ; Ldloc_1
    #locals_push_value8 5 ; Ldloc_3
    #locals_push_value8 6 ; Ldloc_s
    jsr C64_SetChar ; Call
  ; OPT   #locals_push_value8 2 ; Ldloc_0
  ; OPT   #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #add8 ; Add
  ; OPT   #locals_pull_value8 2, 0 ; Stloc_0
    #inc_var 2 ; Nop
  ; OPT   #locals_push_value8 4 ; Ldloc_2
  ; OPT   #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #add8 ; Add
  ; OPT   #locals_pull_value8 4, 0 ; Stloc_2
    #inc_var 4 ; Nop
Wall_BuildKnightPit_64:    #locals_push_value8 4 ; Ldloc_2
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 4 ; Ldc_i4_4
    #add8 ; Add
    #branch_less_unsigned8 Wall_BuildKnightPit_11 ; Blt_un_s
  ; OPT   #locals_push_value8 3 ; Ldloc_1
  ; OPT   #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #add8 ; Add
  ; OPT   #locals_pull_value8 3, 0 ; Stloc_1
    #inc_var 3 ; Nop
Wall_BuildKnightPit_74:    #locals_push_value8 3 ; Ldloc_1
  ; OPT   #stack_push_int8 15 ; Ldc_i4_s
  ; OPT   #branch_less_unsigned8 Wall_BuildKnightPit_7 ; Blt_un_s
    #branch_less_unsigned_const 15, Wall_BuildKnightPit_7 ; Nop
    #method_exit 8, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: BuildEmptyPit
;----------------------------------------
Wall_BuildEmptyPit 
    #init_locals_pull_parameters 5, [0]
  ; OPT   #stack_push_int8 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value8 2, 0 ; Stloc_0
    #init_var 2, 0 ; Nop
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #locals_pull_value8 3, 0 ; Stloc_1
    #init_var 3, 10 ; Nop
    jmp Wall_BuildEmptyPit_74 ; Br_s
Wall_BuildEmptyPit_7:    #locals_push_value8 1 ; Ldarg_0
    #locals_pull_value8 4, 0 ; Stloc_2
    jmp Wall_BuildEmptyPit_64 ; Br_s
Wall_BuildEmptyPit_11:  ; OPT   #stack_push_pointer string_1879051291 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer tbl_EmptyPitChars ; Nop
    #locals_push_value8 2 ; Ldloc_0
    jsr C64_GetMemory ; Call
    #locals_pull_value8 5, 0 ; Stloc_3
  ; OPT   #stack_push_pointer string_1879051327 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer tbl_EmptyPitCharColours ; Nop
    #locals_push_value8 2 ; Ldloc_0
    jsr C64_GetMemory ; Call
    #locals_pull_value8 6, 0 ; Stloc_s
    #locals_push_value8 4 ; Ldloc_2
    #locals_push_value8 3 ; Ldloc_1
    #locals_push_value8 5 ; Ldloc_3
    #locals_push_value8 6 ; Ldloc_s
    jsr C64_SetChar ; Call
  ; OPT   #locals_push_value8 2 ; Ldloc_0
  ; OPT   #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #add8 ; Add
  ; OPT   #locals_pull_value8 2, 0 ; Stloc_0
    #inc_var 2 ; Nop
  ; OPT   #locals_push_value8 4 ; Ldloc_2
  ; OPT   #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #add8 ; Add
  ; OPT   #locals_pull_value8 4, 0 ; Stloc_2
    #inc_var 4 ; Nop
Wall_BuildEmptyPit_64:    #locals_push_value8 4 ; Ldloc_2
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 4 ; Ldc_i4_4
    #add8 ; Add
    #branch_less_unsigned8 Wall_BuildEmptyPit_11 ; Blt_un_s
  ; OPT   #locals_push_value8 3 ; Ldloc_1
  ; OPT   #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #add8 ; Add
  ; OPT   #locals_pull_value8 3, 0 ; Stloc_1
    #inc_var 3 ; Nop
Wall_BuildEmptyPit_74:    #locals_push_value8 3 ; Ldloc_1
  ; OPT   #stack_push_int8 14 ; Ldc_i4_s
  ; OPT   #branch_less_unsigned8 Wall_BuildEmptyPit_7 ; Blt_un_s
    #branch_less_unsigned_const 14, Wall_BuildEmptyPit_7 ; Nop
    #method_exit 8, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: BuildRopePit
;----------------------------------------
Wall_BuildRopePit 
    #init_locals_pull_parameters 3, []
  ; OPT   #stack_push_int8 12 ; Ldc_i4_s
  ; OPT   #locals_pull_value8 1, 0 ; Stloc_0
    #init_var 1, 12 ; Nop
    jmp Wall_BuildRopePit_44 ; Br_s
Wall_BuildRopePit_5:  ; OPT   #stack_push_int8 11 ; Ldc_i4_s
  ; OPT   #locals_pull_value8 2, 0 ; Stloc_1
    #init_var 2, 11 ; Nop
    jmp Wall_BuildRopePit_24 ; Br_s
Wall_BuildRopePit_10:    #locals_push_value8 1 ; Ldloc_0
    #locals_push_value8 2 ; Ldloc_1
    #stack_push_int8 32 ; Ldc_i4_s
    #stack_push_int8 0 ; Ldc_i4_0
    jsr C64_SetChar ; Call
  ; OPT   #locals_push_value8 2 ; Ldloc_1
  ; OPT   #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #add8 ; Add
  ; OPT   #locals_pull_value8 2, 0 ; Stloc_1
    #inc_var 2 ; Nop
Wall_BuildRopePit_24:    #locals_push_value8 2 ; Ldloc_1
  ; OPT   #stack_push_int8 21 ; Ldc_i4_s
  ; OPT   #branch_less_unsigned8 Wall_BuildRopePit_10 ; Blt_un_s
    #branch_less_unsigned_const 21, Wall_BuildRopePit_10 ; Nop
    #locals_push_value8 1 ; Ldloc_0
    #stack_push_int8 21 ; Ldc_i4_s
    #stack_push_int8 114 ; Ldc_i4_s
    #stack_push_int8 2 ; Ldc_i4_2
    jsr C64_SetChar ; Call
  ; OPT   #locals_push_value8 1 ; Ldloc_0
  ; OPT   #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #add8 ; Add
  ; OPT   #locals_pull_value8 1, 0 ; Stloc_0
    #inc_var 1 ; Nop
Wall_BuildRopePit_44:    #locals_push_value8 1 ; Ldloc_0
  ; OPT   #stack_push_int8 28 ; Ldc_i4_s
  ; OPT   #branch_less_unsigned8 Wall_BuildRopePit_5 ; Blt_un_s
    #branch_less_unsigned_const 28, Wall_BuildRopePit_5 ; Nop
  ; OPT   #stack_push_int8 12 ; Ldc_i4_s
  ; OPT   #locals_pull_value8 3, 0 ; Stloc_2
    #init_var 3, 12 ; Nop
    jmp Wall_BuildRopePit_69 ; Br_s
Wall_BuildRopePit_54:    #stack_push_int8 11 ; Ldc_i4_s
    #locals_push_value8 3 ; Ldloc_2
    #stack_push_int8 60 ; Ldc_i4_s
    #stack_push_int8 2 ; Ldc_i4_2
    jsr C64_SetChar ; Call
  ; OPT   #locals_push_value8 3 ; Ldloc_2
  ; OPT   #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #add8 ; Add
  ; OPT   #locals_pull_value8 3, 0 ; Stloc_2
    #inc_var 3 ; Nop
Wall_BuildRopePit_69:    #locals_push_value8 3 ; Ldloc_2
  ; OPT   #stack_push_int8 22 ; Ldc_i4_s
  ; OPT   #branch_less_unsigned8 Wall_BuildRopePit_54 ; Blt_un_s
    #branch_less_unsigned_const 22, Wall_BuildRopePit_54 ; Nop
    #stack_push_int8 11 ; Ldc_i4_s
    #stack_push_int8 11 ; Ldc_i4_s
    #stack_push_int8 59 ; Ldc_i4_s
    #stack_push_int8 2 ; Ldc_i4_2
    jsr C64_SetChar ; Call
    #method_exit 5, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: KnightPitFrame1
;----------------------------------------
Wall_KnightPitFrame1 
    #init_locals_pull_parameters 0, [0]
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 8 ; Ldc_i4_8
    #stack_push_int8 32 ; Ldc_i4_s
    #stack_push_int8 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 9 ; Ldc_i4_s
    #stack_push_int8 32 ; Ldc_i4_s
    #stack_push_int8 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 10 ; Ldc_i4_s
    #stack_push_int8 69 ; Ldc_i4_s
    #stack_push_int8 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 11 ; Ldc_i4_s
    #stack_push_int8 71 ; Ldc_i4_s
    #stack_push_int8 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 12 ; Ldc_i4_s
    #stack_push_int8 79 ; Ldc_i4_s
    #stack_push_int8 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 13 ; Ldc_i4_s
    #stack_push_int8 80 ; Ldc_i4_s
    #stack_push_int8 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 14 ; Ldc_i4_s
    #stack_push_int8 81 ; Ldc_i4_s
    #stack_push_int8 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: KnightPitFrame2
;----------------------------------------
Wall_KnightPitFrame2 
    #init_locals_pull_parameters 0, [0]
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 8 ; Ldc_i4_8
    #stack_push_int8 32 ; Ldc_i4_s
    #stack_push_int8 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 9 ; Ldc_i4_s
    #stack_push_int8 69 ; Ldc_i4_s
    #stack_push_int8 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 10 ; Ldc_i4_s
    #stack_push_int8 70 ; Ldc_i4_s
    #stack_push_int8 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 11 ; Ldc_i4_s
    #stack_push_int8 71 ; Ldc_i4_s
    #stack_push_int8 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 12 ; Ldc_i4_s
    #stack_push_int8 82 ; Ldc_i4_s
    #stack_push_int8 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 13 ; Ldc_i4_s
    #stack_push_int8 83 ; Ldc_i4_s
    #stack_push_int8 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 14 ; Ldc_i4_s
    #stack_push_int8 84 ; Ldc_i4_s
    #stack_push_int8 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: KnightPitFrame3
;----------------------------------------
Wall_KnightPitFrame3 
    #init_locals_pull_parameters 0, [0]
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 8 ; Ldc_i4_8
    #stack_push_int8 69 ; Ldc_i4_s
    #stack_push_int8 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 9 ; Ldc_i4_s
    #stack_push_int8 70 ; Ldc_i4_s
    #stack_push_int8 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 10 ; Ldc_i4_s
    #stack_push_int8 70 ; Ldc_i4_s
    #stack_push_int8 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 11 ; Ldc_i4_s
    #stack_push_int8 86 ; Ldc_i4_s
    #stack_push_int8 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 12 ; Ldc_i4_s
    #stack_push_int8 85 ; Ldc_i4_s
    #stack_push_int8 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 13 ; Ldc_i4_s
    #stack_push_int8 87 ; Ldc_i4_s
    #stack_push_int8 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 14 ; Ldc_i4_s
    #stack_push_int8 88 ; Ldc_i4_s
    #stack_push_int8 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #method_exit 3, [] ; Ret
