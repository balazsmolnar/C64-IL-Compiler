


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: Draw
;----------------------------------------
Wall_Draw 
    #init_locals_pull_parameters 0, [0,0,0]
    #locals_push_value_8 2 ; Ldarg_1
    jsr Wall_BuildBasicWall ; Call
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 3 ; Ldarg_2
  ; OPT   #stfld 0 ; Stfld
    #setfld 1, 3, 0 ; Nop
    #locals_push_value_8 3 ; Ldarg_2
  ; OPT   #stack_push_int 2 ; Ldc_i4_2
  ; OPT   #branch_not_equal Wall_Draw_38 ; Bne_un_s
    #branch_not_equal_const 2, Wall_Draw_38 ; Nop
    #stack_push_int 9 ; Ldc_i4_s
    jsr Wall_BuildKnightPit ; Call
    #stack_push_int 18 ; Ldc_i4_s
    jsr Wall_BuildKnightPit ; Call
    #stack_push_int 27 ; Ldc_i4_s
    jsr Wall_BuildKnightPit ; Call
Wall_Draw_38:    #locals_push_value_8 3 ; Ldarg_2
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #branch_not_equal Wall_Draw_63 ; Bne_un_s
    #branch_not_equal_const 1, Wall_Draw_63 ; Nop
    #stack_push_int 9 ; Ldc_i4_s
    jsr Wall_BuildEmptyPit ; Call
    #stack_push_int 18 ; Ldc_i4_s
    jsr Wall_BuildEmptyPit ; Call
    #stack_push_int 27 ; Ldc_i4_s
    jsr Wall_BuildEmptyPit ; Call
Wall_Draw_63:    #locals_push_value_8 3 ; Ldarg_2
  ; OPT   #stack_push_int 3 ; Ldc_i4_3
  ; OPT   #branch_not_equal Wall_Draw_72 ; Bne_un_s
    #branch_not_equal_const 3, Wall_Draw_72 ; Nop
    jsr Wall_BuildRopePit ; Call
Wall_Draw_72:    #method_exit 5, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: Move
;----------------------------------------
Wall_Move 
    #init_locals_pull_parameters 2, [0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
  ; OPT   #stack_push_int 2 ; Ldc_i4_2
  ; OPT   #branch_equal Wall_Move_10 ; Beq_s
    #branch_equal_const 2, Wall_Move_10 ; Nop
    #method_exit 5, [] ; Ret
Wall_Move_10:    #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #stfld 3 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
  ; OPT   #stack_push_int 2 ; Ldc_i4_2
  ; OPT   #branch_greater_equal_unsigned Wall_Move_34 ; Bge_un_s
    #branch_greater_equal_unsigned_const 2, Wall_Move_34 ; Nop
    #method_exit 5, [] ; Ret
Wall_Move_34:    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 3 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #stfld 1 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
  ; OPT   #stack_push_int 5 ; Ldc_i4_5
  ; OPT   #branch_not_equal Wall_Move_101 ; Bne_un_s
    #branch_not_equal_const 5, Wall_Move_101 ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 1 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #stfld 2 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #stack_push_int 6 ; Ldc_i4_6
  ; OPT   #branch_less_equal_unsigned Wall_Move_101 ; Ble_un_s
    #branch_less_equal_unsigned_const 6, Wall_Move_101 ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 2 ; Stfld
Wall_Move_101:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #stack_push_int 4 ; Ldc_i4_4
  ; OPT   #branch_greater_equal_unsigned Wall_Move_111 ; Bge_un_s
    #branch_greater_equal_unsigned_const 4, Wall_Move_111 ; Nop
    #method_exit 5, [] ; Ret
Wall_Move_111:  ; OPT   #stack_push_int 10 ; Ldc_i4_s
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #init_var 2, 10 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #stack_push_int 4 ; Ldc_i4_4
  ; OPT   #branch_not_equal Wall_Move_126 ; Bne_un_s
    #branch_not_equal_const 4, Wall_Move_126 ; Nop
  ; OPT   #stack_push_int 28 ; Ldc_i4_s
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #init_var 2, 28 ; Nop
Wall_Move_126:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #stack_push_int 5 ; Ldc_i4_5
  ; OPT   #branch_not_equal Wall_Move_138 ; Bne_un_s
    #branch_not_equal_const 5, Wall_Move_138 ; Nop
  ; OPT   #stack_push_int 19 ; Ldc_i4_s
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #init_var 2, 19 ; Nop
Wall_Move_138:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
    #locals_pull_value_8 3, 0 ; Stloc_1
    #locals_push_value_8 3 ; Ldloc_1
    #switch Wall_Move_Jump_146 ; Switch
    #method_exit 5, [] ; Ret
Wall_Move_172:    #locals_push_value_8 2 ; Ldloc_0
    jsr Wall_KnightPitFrame1 ; Call
    #method_exit 5, [] ; Ret
Wall_Move_179:    #locals_push_value_8 2 ; Ldloc_0
    jsr Wall_KnightPitFrame2 ; Call
    #method_exit 5, [] ; Ret
Wall_Move_186:    #locals_push_value_8 2 ; Ldloc_0
    jsr Wall_KnightPitFrame3 ; Call
    #method_exit 5, [] ; Ret
Wall_Move_Jump_146 .word  Wall_Move_172-1,  Wall_Move_179-1,  Wall_Move_186-1,  Wall_Move_179-1,  Wall_Move_172-1, 0


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: IsHole
;----------------------------------------
Wall_IsHole 
    #init_locals_pull_parameters 0, [0,0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #branch_equal Wall_IsHole_18 ; Beq_s
    #branch_equal_const 1, Wall_IsHole_18 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
  ; OPT   #stack_push_int 2 ; Ldc_i4_2
  ; OPT   #branch_not_equal Wall_IsHole_66 ; Bne_un_s
    #branch_not_equal_const 2, Wall_IsHole_66 ; Nop
Wall_IsHole_18:    #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stack_push_int 92 ; Ldc_i4_s
  ; OPT   #branch_less_equal_unsigned Wall_IsHole_30 ; Ble_un_s
    #branch_less_equal_unsigned_const 92, Wall_IsHole_30 ; Nop
    #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stack_push_int 114 ; Ldc_i4_s
  ; OPT   #branch_greater_equal_unsigned Wall_IsHole_30 ; Bge_un_s
    #branch_greater_equal_unsigned_const 114, Wall_IsHole_30 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #method_exit 4, [] ; Ret
Wall_IsHole_30:    #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stack_push_int 164 ; Ldc_i4
  ; OPT   #branch_less_equal_unsigned Wall_IsHole_48 ; Ble_un_s
    #branch_less_equal_unsigned_const 164, Wall_IsHole_48 ; Nop
    #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stack_push_int 186 ; Ldc_i4
  ; OPT   #branch_greater_equal_unsigned Wall_IsHole_48 ; Bge_un_s
    #branch_greater_equal_unsigned_const 186, Wall_IsHole_48 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #method_exit 4, [] ; Ret
Wall_IsHole_48:    #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stack_push_int 236 ; Ldc_i4
  ; OPT   #branch_less_equal_unsigned Wall_IsHole_66 ; Ble_un_s
    #branch_less_equal_unsigned_const 236, Wall_IsHole_66 ; Nop
    #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stack_push_int 255 ; Ldc_i4
  ; OPT   #branch_greater_equal_unsigned Wall_IsHole_66 ; Bge_un_s
    #branch_greater_equal_unsigned_const 255, Wall_IsHole_66 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #method_exit 4, [] ; Ret
Wall_IsHole_66:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
  ; OPT   #stack_push_int 3 ; Ldc_i4_3
  ; OPT   #branch_not_equal Wall_IsHole_90 ; Bne_un_s
    #branch_not_equal_const 3, Wall_IsHole_90 ; Nop
    #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stack_push_int 106 ; Ldc_i4_s
  ; OPT   #branch_less_equal_unsigned Wall_IsHole_90 ; Ble_un_s
    #branch_less_equal_unsigned_const 106, Wall_IsHole_90 ; Nop
    #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stack_push_int 238 ; Ldc_i4
  ; OPT   #branch_greater_equal_unsigned Wall_IsHole_90 ; Bge_un_s
    #branch_greater_equal_unsigned_const 238, Wall_IsHole_90 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #method_exit 4, [] ; Ret
Wall_IsHole_90:    #stack_push_int 0 ; Ldc_i4_0
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: BuildBasicWall
;----------------------------------------
Wall_BuildBasicWall 
    #init_locals_pull_parameters 4, [0]
  ; OPT   #stack_push_pointer string_1879050847 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer screenMemory+$01B8 ; Nop
    #stack_push_int 62 ; Ldc_i4_s
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
  ; OPT   #stack_push_pointer string_1879050885 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer screenMemory+$02B8 ; Nop
    #stack_push_int 62 ; Ldc_i4_s
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
  ; OPT   #stack_push_pointer string_1879050923 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer screenMemory+$0300 ; Nop
    #stack_push_int 62 ; Ldc_i4_s
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
  ; OPT   #stack_push_pointer string_1879050961 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer colorMemory+$01B8 ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
  ; OPT   #stack_push_pointer string_1879050997 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer colorMemory+$02B8 ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
  ; OPT   #stack_push_pointer string_1879051033 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer colorMemory+$0300 ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
  ; OPT   #stack_push_pointer string_1879051069 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer screenMemory+$0320 ; Nop
  ; OPT   #stack_push_pointer string_1879051107 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer tbl_WallBaseChars ; Nop
    #stack_push_int 199 ; Ldc_i4
    jsr C64_CopyMemory ; Call
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #init_var 2, 0 ; Nop
  ; OPT   #stack_push_int 3 ; Ldc_i4_3
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #init_var 3, 3 ; Nop
    jmp Wall_BuildBasicWall_190 ; Br_s
Wall_BuildBasicWall_141:  ; OPT   #stack_push_int 37 ; Ldc_i4_s
  ; OPT   #locals_pull_value_8 4, 0 ; Stloc_2
    #init_var 4, 37 ; Nop
    jmp Wall_BuildBasicWall_181 ; Br_s
Wall_BuildBasicWall_146:  ; OPT   #stack_push_pointer string_1879051143 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer tbl_LevelEndBellChars ; Nop
    #locals_push_value_8 2 ; Ldloc_0
    jsr C64_GetMemory ; Call
    #locals_pull_value_8 5, 0 ; Stloc_3
    #locals_push_value_8 4 ; Ldloc_2
    #locals_push_value_8 3 ; Ldloc_1
    #locals_push_value_8 5 ; Ldloc_3
    #stack_push_int 15 ; Ldc_i4_s
    jsr C64_SetChar ; Call
  ; OPT   #locals_push_value_8 2 ; Ldloc_0
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #inc_var 2 ; Nop
  ; OPT   #locals_push_value_8 4 ; Ldloc_2
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 4, 0 ; Stloc_2
    #inc_var 4 ; Nop
Wall_BuildBasicWall_181:    #locals_push_value_8 4 ; Ldloc_2
  ; OPT   #stack_push_int 40 ; Ldc_i4_s
  ; OPT   #branch_less_unsigned Wall_BuildBasicWall_146 ; Blt_un_s
    #branch_less_unsigned_const 40, Wall_BuildBasicWall_146 ; Nop
  ; OPT   #locals_push_value_8 3 ; Ldloc_1
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #inc_var 3 ; Nop
Wall_BuildBasicWall_190:    #locals_push_value_8 3 ; Ldloc_1
  ; OPT   #stack_push_int 11 ; Ldc_i4_s
  ; OPT   #branch_less_unsigned Wall_BuildBasicWall_141 ; Blt_un_s
    #branch_less_unsigned_const 11, Wall_BuildBasicWall_141 ; Nop
    #method_exit 7, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: BuildKnightPit
;----------------------------------------
Wall_BuildKnightPit 
    #init_locals_pull_parameters 5, [0]
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #init_var 2, 0 ; Nop
  ; OPT   #stack_push_int 10 ; Ldc_i4_s
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #init_var 3, 10 ; Nop
    jmp Wall_BuildKnightPit_74 ; Br_s
Wall_BuildKnightPit_7:    #locals_push_value_8 1 ; Ldarg_0
    #locals_pull_value_8 4, 0 ; Stloc_2
    jmp Wall_BuildKnightPit_64 ; Br_s
Wall_BuildKnightPit_11:  ; OPT   #stack_push_pointer string_1879051187 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer tbl_KnightChars ; Nop
    #locals_push_value_8 2 ; Ldloc_0
    jsr C64_GetMemory ; Call
    #locals_pull_value_8 5, 0 ; Stloc_3
  ; OPT   #stack_push_pointer string_1879051219 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer tbl_KnightCharsColours ; Nop
    #locals_push_value_8 2 ; Ldloc_0
    jsr C64_GetMemory ; Call
    #locals_pull_value_8 6, 0 ; Stloc_s
    #locals_push_value_8 4 ; Ldloc_2
    #locals_push_value_8 3 ; Ldloc_1
    #locals_push_value_8 5 ; Ldloc_3
    #locals_push_value_8 6 ; Ldloc_s
    jsr C64_SetChar ; Call
  ; OPT   #locals_push_value_8 2 ; Ldloc_0
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #inc_var 2 ; Nop
  ; OPT   #locals_push_value_8 4 ; Ldloc_2
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 4, 0 ; Stloc_2
    #inc_var 4 ; Nop
Wall_BuildKnightPit_64:    #locals_push_value_8 4 ; Ldloc_2
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 4 ; Ldc_i4_4
    #add ; Add
    #branch_less_unsigned Wall_BuildKnightPit_11 ; Blt_un_s
  ; OPT   #locals_push_value_8 3 ; Ldloc_1
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #inc_var 3 ; Nop
Wall_BuildKnightPit_74:    #locals_push_value_8 3 ; Ldloc_1
  ; OPT   #stack_push_int 15 ; Ldc_i4_s
  ; OPT   #branch_less_unsigned Wall_BuildKnightPit_7 ; Blt_un_s
    #branch_less_unsigned_const 15, Wall_BuildKnightPit_7 ; Nop
    #method_exit 8, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: BuildEmptyPit
;----------------------------------------
Wall_BuildEmptyPit 
    #init_locals_pull_parameters 5, [0]
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #init_var 2, 0 ; Nop
  ; OPT   #stack_push_int 10 ; Ldc_i4_s
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #init_var 3, 10 ; Nop
    jmp Wall_BuildEmptyPit_74 ; Br_s
Wall_BuildEmptyPit_7:    #locals_push_value_8 1 ; Ldarg_0
    #locals_pull_value_8 4, 0 ; Stloc_2
    jmp Wall_BuildEmptyPit_64 ; Br_s
Wall_BuildEmptyPit_11:  ; OPT   #stack_push_pointer string_1879051265 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer tbl_EmptyPitChars ; Nop
    #locals_push_value_8 2 ; Ldloc_0
    jsr C64_GetMemory ; Call
    #locals_pull_value_8 5, 0 ; Stloc_3
  ; OPT   #stack_push_pointer string_1879051301 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer tbl_EmptyPitCharColours ; Nop
    #locals_push_value_8 2 ; Ldloc_0
    jsr C64_GetMemory ; Call
    #locals_pull_value_8 6, 0 ; Stloc_s
    #locals_push_value_8 4 ; Ldloc_2
    #locals_push_value_8 3 ; Ldloc_1
    #locals_push_value_8 5 ; Ldloc_3
    #locals_push_value_8 6 ; Ldloc_s
    jsr C64_SetChar ; Call
  ; OPT   #locals_push_value_8 2 ; Ldloc_0
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #inc_var 2 ; Nop
  ; OPT   #locals_push_value_8 4 ; Ldloc_2
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 4, 0 ; Stloc_2
    #inc_var 4 ; Nop
Wall_BuildEmptyPit_64:    #locals_push_value_8 4 ; Ldloc_2
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 4 ; Ldc_i4_4
    #add ; Add
    #branch_less_unsigned Wall_BuildEmptyPit_11 ; Blt_un_s
  ; OPT   #locals_push_value_8 3 ; Ldloc_1
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #inc_var 3 ; Nop
Wall_BuildEmptyPit_74:    #locals_push_value_8 3 ; Ldloc_1
  ; OPT   #stack_push_int 14 ; Ldc_i4_s
  ; OPT   #branch_less_unsigned Wall_BuildEmptyPit_7 ; Blt_un_s
    #branch_less_unsigned_const 14, Wall_BuildEmptyPit_7 ; Nop
    #method_exit 8, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: BuildRopePit
;----------------------------------------
Wall_BuildRopePit 
    #init_locals_pull_parameters 3, []
  ; OPT   #stack_push_int 12 ; Ldc_i4_s
  ; OPT   #locals_pull_value_8 1, 0 ; Stloc_0
    #init_var 1, 12 ; Nop
    jmp Wall_BuildRopePit_44 ; Br_s
Wall_BuildRopePit_5:  ; OPT   #stack_push_int 11 ; Ldc_i4_s
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_1
    #init_var 2, 11 ; Nop
    jmp Wall_BuildRopePit_24 ; Br_s
Wall_BuildRopePit_10:    #locals_push_value_8 1 ; Ldloc_0
    #locals_push_value_8 2 ; Ldloc_1
    #stack_push_int 32 ; Ldc_i4_s
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_SetChar ; Call
  ; OPT   #locals_push_value_8 2 ; Ldloc_1
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_1
    #inc_var 2 ; Nop
Wall_BuildRopePit_24:    #locals_push_value_8 2 ; Ldloc_1
  ; OPT   #stack_push_int 21 ; Ldc_i4_s
  ; OPT   #branch_less_unsigned Wall_BuildRopePit_10 ; Blt_un_s
    #branch_less_unsigned_const 21, Wall_BuildRopePit_10 ; Nop
    #locals_push_value_8 1 ; Ldloc_0
    #stack_push_int 21 ; Ldc_i4_s
    #stack_push_int 114 ; Ldc_i4_s
    #stack_push_int 2 ; Ldc_i4_2
    jsr C64_SetChar ; Call
  ; OPT   #locals_push_value_8 1 ; Ldloc_0
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 1, 0 ; Stloc_0
    #inc_var 1 ; Nop
Wall_BuildRopePit_44:    #locals_push_value_8 1 ; Ldloc_0
  ; OPT   #stack_push_int 28 ; Ldc_i4_s
  ; OPT   #branch_less_unsigned Wall_BuildRopePit_5 ; Blt_un_s
    #branch_less_unsigned_const 28, Wall_BuildRopePit_5 ; Nop
  ; OPT   #stack_push_int 12 ; Ldc_i4_s
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_2
    #init_var 3, 12 ; Nop
    jmp Wall_BuildRopePit_69 ; Br_s
Wall_BuildRopePit_54:    #stack_push_int 11 ; Ldc_i4_s
    #locals_push_value_8 3 ; Ldloc_2
    #stack_push_int 60 ; Ldc_i4_s
    #stack_push_int 2 ; Ldc_i4_2
    jsr C64_SetChar ; Call
  ; OPT   #locals_push_value_8 3 ; Ldloc_2
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_2
    #inc_var 3 ; Nop
Wall_BuildRopePit_69:    #locals_push_value_8 3 ; Ldloc_2
  ; OPT   #stack_push_int 22 ; Ldc_i4_s
  ; OPT   #branch_less_unsigned Wall_BuildRopePit_54 ; Blt_un_s
    #branch_less_unsigned_const 22, Wall_BuildRopePit_54 ; Nop
    #stack_push_int 11 ; Ldc_i4_s
    #stack_push_int 11 ; Ldc_i4_s
    #stack_push_int 59 ; Ldc_i4_s
    #stack_push_int 2 ; Ldc_i4_2
    jsr C64_SetChar ; Call
    #method_exit 5, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: KnightPitFrame1
;----------------------------------------
Wall_KnightPitFrame1 
    #init_locals_pull_parameters 0, [0]
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 8 ; Ldc_i4_8
    #stack_push_int 32 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 9 ; Ldc_i4_s
    #stack_push_int 32 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 10 ; Ldc_i4_s
    #stack_push_int 69 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 11 ; Ldc_i4_s
    #stack_push_int 71 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 12 ; Ldc_i4_s
    #stack_push_int 79 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 13 ; Ldc_i4_s
    #stack_push_int 80 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 14 ; Ldc_i4_s
    #stack_push_int 81 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: KnightPitFrame2
;----------------------------------------
Wall_KnightPitFrame2 
    #init_locals_pull_parameters 0, [0]
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 8 ; Ldc_i4_8
    #stack_push_int 32 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 9 ; Ldc_i4_s
    #stack_push_int 69 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 10 ; Ldc_i4_s
    #stack_push_int 70 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 11 ; Ldc_i4_s
    #stack_push_int 71 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 12 ; Ldc_i4_s
    #stack_push_int 82 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 13 ; Ldc_i4_s
    #stack_push_int 83 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 14 ; Ldc_i4_s
    #stack_push_int 84 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: KnightPitFrame3
;----------------------------------------
Wall_KnightPitFrame3 
    #init_locals_pull_parameters 0, [0]
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 8 ; Ldc_i4_8
    #stack_push_int 69 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 9 ; Ldc_i4_s
    #stack_push_int 70 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 10 ; Ldc_i4_s
    #stack_push_int 70 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 11 ; Ldc_i4_s
    #stack_push_int 86 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 12 ; Ldc_i4_s
    #stack_push_int 85 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 13 ; Ldc_i4_s
    #stack_push_int 87 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 14 ; Ldc_i4_s
    #stack_push_int 88 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    #method_exit 3, [] ; Ret
