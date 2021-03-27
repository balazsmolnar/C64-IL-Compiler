

;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: Draw
;----------------------------------------
Wall_Draw 
    #init_locals_pull_parameters 3, [0,0,0]
    nop ; Nop
    #locals_push_value_8 2 ; Ldarg_1
    jsr Wall_BuildBasicWall ; Call
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 3 ; Ldarg_2
  ; OPT   #stfld 0 ; Stfld
    #setfld 1, 3, 0 ; Nop
    #locals_push_value_8 3 ; Ldarg_2
    #stack_push_int 2 ; Ldc_i4_2
    #compareEqual ; Ceq
    #locals_pull_value_8 4, 0 ; Stloc_0
    #locals_push_value_8 4 ; Ldloc_0
    #branch_false Wall_Draw_49 ; Brfalse_s
    nop ; Nop
    #stack_push_int 9 ; Ldc_i4_s
    jsr Wall_BuildKnightPit ; Call
    nop ; Nop
    #stack_push_int 18 ; Ldc_i4_s
    jsr Wall_BuildKnightPit ; Call
    nop ; Nop
    #stack_push_int 27 ; Ldc_i4_s
    jsr Wall_BuildKnightPit ; Call
    nop ; Nop
    nop ; Nop
Wall_Draw_49:    #locals_push_value_8 3 ; Ldarg_2
    #stack_push_int 1 ; Ldc_i4_1
    #compareEqual ; Ceq
    #locals_pull_value_8 5, 0 ; Stloc_1
    #locals_push_value_8 5 ; Ldloc_1
    #branch_false Wall_Draw_83 ; Brfalse_s
    nop ; Nop
    #stack_push_int 9 ; Ldc_i4_s
    jsr Wall_BuildEmptyPit ; Call
    nop ; Nop
    #stack_push_int 18 ; Ldc_i4_s
    jsr Wall_BuildEmptyPit ; Call
    nop ; Nop
    #stack_push_int 27 ; Ldc_i4_s
    jsr Wall_BuildEmptyPit ; Call
    nop ; Nop
    nop ; Nop
Wall_Draw_83:    #locals_push_value_8 3 ; Ldarg_2
    #stack_push_int 3 ; Ldc_i4_3
    #compareEqual ; Ceq
    #locals_pull_value_8 6, 0 ; Stloc_2
    #locals_push_value_8 6 ; Ldloc_2
    #branch_false Wall_Draw_99 ; Brfalse_s
    nop ; Nop
    jsr Wall_BuildRopePit ; Call
    nop ; Nop
    nop ; Nop
Wall_Draw_99:    #method_exit 8, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: Move
;----------------------------------------
Wall_Move 
    #init_locals_pull_parameters 10, [0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #stack_push_int 2 ; Ldc_i4_2
    #compareEqual ; Ceq
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #locals_pull_value_8 3, 0 ; Stloc_1
    #locals_push_value_8 3 ; Ldloc_1
    #branch_false Wall_Move_22 ; Brfalse_s
    jmp Wall_Move_241 ; Br
Wall_Move_22:    #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #stfld 1 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
    #stack_push_int 6 ; Ldc_i4_6
    #compareEqual ; Ceq
    #locals_pull_value_8 4, 0 ; Stloc_2
    #locals_push_value_8 4 ; Ldloc_2
    #branch_false Wall_Move_92 ; Brfalse_s
    nop ; Nop
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
    #stack_push_int 6 ; Ldc_i4_6
    #compareGreater_unsigned ; Cgt_un
    #locals_pull_value_8 5, 0 ; Stloc_3
    #locals_push_value_8 5 ; Ldloc_3
    #branch_false Wall_Move_91 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 2 ; Stfld
Wall_Move_91:    nop ; Nop
Wall_Move_92:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 4 ; Ldc_i4_4
    #compareLess ; Clt_un
    #locals_pull_value_8 6, 0 ; Stloc_s
    #locals_push_value_8 6 ; Ldloc_s
    #branch_false Wall_Move_112 ; Brfalse_s
    jmp Wall_Move_241 ; Br
Wall_Move_112:  ; OPT   #stack_push_int 10 ; Ldc_i4_s
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #init_var 2, 10 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 4 ; Ldc_i4_4
    #compareEqual ; Ceq
    #locals_pull_value_8 7, 0 ; Stloc_s
    #locals_push_value_8 7 ; Ldloc_s
    #branch_false Wall_Move_133 ; Brfalse_s
  ; OPT   #stack_push_int 28 ; Ldc_i4_s
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #init_var 2, 28 ; Nop
Wall_Move_133:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 5 ; Ldc_i4_5
    #compareEqual ; Ceq
    #locals_pull_value_8 8, 0 ; Stloc_s
    #locals_push_value_8 8 ; Ldloc_s
    #branch_false Wall_Move_151 ; Brfalse_s
  ; OPT   #stack_push_int 19 ; Ldc_i4_s
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #init_var 2, 19 ; Nop
Wall_Move_151:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #branch_equal Wall_Move_171 ; Beq_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
    #stack_push_int 5 ; Ldc_i4_5
    #compareEqual ; Ceq
    jmp Wall_Move_172 ; Br_s
Wall_Move_171:    #stack_push_int 1 ; Ldc_i4_1
Wall_Move_172:    #locals_pull_value_8 9, 0 ; Stloc_s
    #locals_push_value_8 9 ; Ldloc_s
    #branch_false Wall_Move_185 ; Brfalse_s
    #locals_push_value_8 2 ; Ldloc_0
    jsr Wall_KnightPitFrame1 ; Call
    nop ; Nop
Wall_Move_185:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
    #stack_push_int 2 ; Ldc_i4_2
    #branch_equal Wall_Move_205 ; Beq_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
    #stack_push_int 4 ; Ldc_i4_4
    #compareEqual ; Ceq
    jmp Wall_Move_206 ; Br_s
Wall_Move_205:    #stack_push_int 1 ; Ldc_i4_1
Wall_Move_206:    #locals_pull_value_8 10, 0 ; Stloc_s
    #locals_push_value_8 10 ; Ldloc_s
    #branch_false Wall_Move_219 ; Brfalse_s
    #locals_push_value_8 2 ; Ldloc_0
    jsr Wall_KnightPitFrame2 ; Call
    nop ; Nop
Wall_Move_219:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
    #stack_push_int 3 ; Ldc_i4_3
    #compareEqual ; Ceq
    #locals_pull_value_8 11, 0 ; Stloc_s
    #locals_push_value_8 11 ; Ldloc_s
    #branch_false Wall_Move_241 ; Brfalse_s
    #locals_push_value_8 2 ; Ldloc_0
    jsr Wall_KnightPitFrame3 ; Call
    nop ; Nop
Wall_Move_241:    #method_exit 13, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: IsHole
;----------------------------------------
Wall_IsHole 
    #init_locals_pull_parameters 7, [0,0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #branch_equal Wall_IsHole_21 ; Beq_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #stack_push_int 2 ; Ldc_i4_2
    #compareEqual ; Ceq
    jmp Wall_IsHole_22 ; Br_s
Wall_IsHole_21:    #stack_push_int 1 ; Ldc_i4_1
Wall_IsHole_22:    #locals_pull_value_8 3, 0 ; Stloc_0
    #locals_push_value_8 3 ; Ldloc_0
    #branch_false Wall_IsHole_105 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 2 ; Ldarg_1
    #stack_push_int 92 ; Ldc_i4_s
    #branch_less_equal_unsigned Wall_IsHole_39 ; Ble_un_s
    #locals_push_value_8 2 ; Ldarg_1
    #stack_push_int 114 ; Ldc_i4_s
    #compareLess ; Clt_un
    jmp Wall_IsHole_40 ; Br_s
Wall_IsHole_39:    #stack_push_int 0 ; Ldc_i4_0
Wall_IsHole_40:    #locals_pull_value_8 4, 0 ; Stloc_1
    #locals_push_value_8 4 ; Ldloc_1
    #branch_false Wall_IsHole_48 ; Brfalse_s
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #locals_pull_value_8 5, 0 ; Stloc_2
    #init_var 5, 1 ; Nop
    jmp Wall_IsHole_152 ; Br_s
Wall_IsHole_48:    #locals_push_value_8 2 ; Ldarg_1
    #stack_push_int 164 ; Ldc_i4
    #branch_less_equal_unsigned Wall_IsHole_66 ; Ble_un_s
    #locals_push_value_8 2 ; Ldarg_1
    #stack_push_int 186 ; Ldc_i4
    #compareLess ; Clt_un
    jmp Wall_IsHole_67 ; Br_s
Wall_IsHole_66:    #stack_push_int 0 ; Ldc_i4_0
Wall_IsHole_67:    #locals_pull_value_8 6, 0 ; Stloc_3
    #locals_push_value_8 6 ; Ldloc_3
    #branch_false Wall_IsHole_75 ; Brfalse_s
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #locals_pull_value_8 5, 0 ; Stloc_2
    #init_var 5, 1 ; Nop
    jmp Wall_IsHole_152 ; Br_s
Wall_IsHole_75:    #locals_push_value_8 2 ; Ldarg_1
    #stack_push_int 236 ; Ldc_i4
    #branch_less_equal_unsigned Wall_IsHole_93 ; Ble_un_s
    #locals_push_value_8 2 ; Ldarg_1
    #stack_push_int 255 ; Ldc_i4
    #compareLess ; Clt_un
    jmp Wall_IsHole_94 ; Br_s
Wall_IsHole_93:    #stack_push_int 0 ; Ldc_i4_0
Wall_IsHole_94:    #locals_pull_value_8 7, 0 ; Stloc_s
    #locals_push_value_8 7 ; Ldloc_s
    #branch_false Wall_IsHole_104 ; Brfalse_s
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #locals_pull_value_8 5, 0 ; Stloc_2
    #init_var 5, 1 ; Nop
    jmp Wall_IsHole_152 ; Br_s
Wall_IsHole_104:    nop ; Nop
Wall_IsHole_105:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #stack_push_int 3 ; Ldc_i4_3
    #compareEqual ; Ceq
    #locals_pull_value_8 8, 0 ; Stloc_s
    #locals_push_value_8 8 ; Ldloc_s
    #branch_false Wall_IsHole_148 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 2 ; Ldarg_1
    #stack_push_int 104 ; Ldc_i4_s
    #branch_less_equal_unsigned Wall_IsHole_136 ; Ble_un_s
    #locals_push_value_8 2 ; Ldarg_1
    #stack_push_int 238 ; Ldc_i4
    #compareLess ; Clt_un
    jmp Wall_IsHole_137 ; Br_s
Wall_IsHole_136:    #stack_push_int 0 ; Ldc_i4_0
Wall_IsHole_137:    #locals_pull_value_8 9, 0 ; Stloc_s
    #locals_push_value_8 9 ; Ldloc_s
    #branch_false Wall_IsHole_147 ; Brfalse_s
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #locals_pull_value_8 5, 0 ; Stloc_2
    #init_var 5, 1 ; Nop
    jmp Wall_IsHole_152 ; Br_s
Wall_IsHole_147:    nop ; Nop
Wall_IsHole_148:  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 5, 0 ; Stloc_2
    #init_var 5, 0 ; Nop
    jmp Wall_IsHole_152 ; Br_s
Wall_IsHole_152:    #locals_push_value_8 5 ; Ldloc_2
    #method_exit 11, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: BuildBasicWall
;----------------------------------------
Wall_BuildBasicWall 
    #init_locals_pull_parameters 6, [0]
    nop ; Nop
  ; OPT   #stack_push_pointer string_1879050029 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer screenMemory+$01B8 ; Nop
    #stack_push_int 62 ; Ldc_i4_s
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
    nop ; Nop
  ; OPT   #stack_push_pointer string_1879050067 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer screenMemory+$02B8 ; Nop
    #stack_push_int 62 ; Ldc_i4_s
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
    nop ; Nop
  ; OPT   #stack_push_pointer string_1879050105 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer screenMemory+$0300 ; Nop
    #stack_push_int 62 ; Ldc_i4_s
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
    nop ; Nop
  ; OPT   #stack_push_pointer string_1879050143 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer colorMemory+$01B8 ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
    nop ; Nop
  ; OPT   #stack_push_pointer string_1879050179 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer colorMemory+$02B8 ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
    nop ; Nop
  ; OPT   #stack_push_pointer string_1879050215 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer colorMemory+$0300 ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
    nop ; Nop
  ; OPT   #stack_push_pointer string_1879050251 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer screenMemory+$0320 ; Nop
  ; OPT   #stack_push_pointer string_1879050289 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer tbl_WallBaseChars ; Nop
    #stack_push_int 199 ; Ldc_i4
    jsr C64_CopyMemory ; Call
    nop ; Nop
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #init_var 2, 0 ; Nop
  ; OPT   #stack_push_int 3 ; Ldc_i4_3
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #init_var 3, 3 ; Nop
    jmp Wall_BuildBasicWall_209 ; Br_s
Wall_BuildBasicWall_149:    nop ; Nop
  ; OPT   #stack_push_int 37 ; Ldc_i4_s
  ; OPT   #locals_pull_value_8 4, 0 ; Stloc_2
    #init_var 4, 37 ; Nop
    jmp Wall_BuildBasicWall_193 ; Br_s
Wall_BuildBasicWall_155:    nop ; Nop
  ; OPT   #stack_push_pointer string_1879050325 ; Ldstr
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
    nop ; Nop
  ; OPT   #locals_push_value_8 2 ; Ldloc_0
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #inc_var 2 ; Nop
    nop ; Nop
  ; OPT   #locals_push_value_8 4 ; Ldloc_2
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 4, 0 ; Stloc_2
    #inc_var 4 ; Nop
Wall_BuildBasicWall_193:    #locals_push_value_8 4 ; Ldloc_2
    #stack_push_int 40 ; Ldc_i4_s
    #compareLess ; Clt_un
    #locals_pull_value_8 6, 0 ; Stloc_s
    #locals_push_value_8 6 ; Ldloc_s
    #branch_true Wall_BuildBasicWall_155 ; Brtrue_s
    nop ; Nop
  ; OPT   #locals_push_value_8 3 ; Ldloc_1
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #inc_var 3 ; Nop
Wall_BuildBasicWall_209:    #locals_push_value_8 3 ; Ldloc_1
    #stack_push_int 11 ; Ldc_i4_s
    #compareLess ; Clt_un
    #locals_pull_value_8 7, 0 ; Stloc_s
    #locals_push_value_8 7 ; Ldloc_s
    #branch_true Wall_BuildBasicWall_149 ; Brtrue_s
    #method_exit 9, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: BuildKnightPit
;----------------------------------------
Wall_BuildKnightPit 
    #init_locals_pull_parameters 7, [0]
    nop ; Nop
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #init_var 2, 0 ; Nop
  ; OPT   #stack_push_int 10 ; Ldc_i4_s
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #init_var 3, 10 ; Nop
    jmp Wall_BuildKnightPit_86 ; Br_s
Wall_BuildKnightPit_8:    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #locals_pull_value_8 4, 0 ; Stloc_2
    jmp Wall_BuildKnightPit_69 ; Br_s
Wall_BuildKnightPit_13:    nop ; Nop
  ; OPT   #stack_push_pointer string_1879050369 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer tbl_KnightChars ; Nop
    #locals_push_value_8 2 ; Ldloc_0
    jsr C64_GetMemory ; Call
    #locals_pull_value_8 5, 0 ; Stloc_3
  ; OPT   #stack_push_pointer string_1879050401 ; Ldstr
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
    nop ; Nop
  ; OPT   #locals_push_value_8 2 ; Ldloc_0
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #inc_var 2 ; Nop
    nop ; Nop
  ; OPT   #locals_push_value_8 4 ; Ldloc_2
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 4, 0 ; Stloc_2
    #inc_var 4 ; Nop
Wall_BuildKnightPit_69:    #locals_push_value_8 4 ; Ldloc_2
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 4 ; Ldc_i4_4
    #add ; Add
    #compareLess ; Clt_un
    #locals_pull_value_8 7, 0 ; Stloc_s
    #locals_push_value_8 7 ; Ldloc_s
    #branch_true Wall_BuildKnightPit_13 ; Brtrue_s
    nop ; Nop
  ; OPT   #locals_push_value_8 3 ; Ldloc_1
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #inc_var 3 ; Nop
Wall_BuildKnightPit_86:    #locals_push_value_8 3 ; Ldloc_1
    #stack_push_int 15 ; Ldc_i4_s
    #compareLess ; Clt_un
    #locals_pull_value_8 8, 0 ; Stloc_s
    #locals_push_value_8 8 ; Ldloc_s
    #branch_true Wall_BuildKnightPit_8 ; Brtrue_s
    #method_exit 10, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: BuildEmptyPit
;----------------------------------------
Wall_BuildEmptyPit 
    #init_locals_pull_parameters 7, [0]
    nop ; Nop
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #init_var 2, 0 ; Nop
  ; OPT   #stack_push_int 10 ; Ldc_i4_s
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #init_var 3, 10 ; Nop
    jmp Wall_BuildEmptyPit_86 ; Br_s
Wall_BuildEmptyPit_8:    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #locals_pull_value_8 4, 0 ; Stloc_2
    jmp Wall_BuildEmptyPit_69 ; Br_s
Wall_BuildEmptyPit_13:    nop ; Nop
  ; OPT   #stack_push_pointer string_1879050447 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer tbl_EmptyPitChars ; Nop
    #locals_push_value_8 2 ; Ldloc_0
    jsr C64_GetMemory ; Call
    #locals_pull_value_8 5, 0 ; Stloc_3
  ; OPT   #stack_push_pointer string_1879050483 ; Ldstr
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
    nop ; Nop
  ; OPT   #locals_push_value_8 2 ; Ldloc_0
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #inc_var 2 ; Nop
    nop ; Nop
  ; OPT   #locals_push_value_8 4 ; Ldloc_2
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 4, 0 ; Stloc_2
    #inc_var 4 ; Nop
Wall_BuildEmptyPit_69:    #locals_push_value_8 4 ; Ldloc_2
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 4 ; Ldc_i4_4
    #add ; Add
    #compareLess ; Clt_un
    #locals_pull_value_8 7, 0 ; Stloc_s
    #locals_push_value_8 7 ; Ldloc_s
    #branch_true Wall_BuildEmptyPit_13 ; Brtrue_s
    nop ; Nop
  ; OPT   #locals_push_value_8 3 ; Ldloc_1
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #inc_var 3 ; Nop
Wall_BuildEmptyPit_86:    #locals_push_value_8 3 ; Ldloc_1
    #stack_push_int 14 ; Ldc_i4_s
    #compareLess ; Clt_un
    #locals_pull_value_8 8, 0 ; Stloc_s
    #locals_push_value_8 8 ; Ldloc_s
    #branch_true Wall_BuildEmptyPit_8 ; Brtrue_s
    #method_exit 10, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: BuildRopePit
;----------------------------------------
Wall_BuildRopePit 
    #init_locals_pull_parameters 6, []
    nop ; Nop
  ; OPT   #stack_push_int 12 ; Ldc_i4_s
  ; OPT   #locals_pull_value_8 1, 0 ; Stloc_0
    #init_var 1, 12 ; Nop
    jmp Wall_BuildRopePit_55 ; Br_s
Wall_BuildRopePit_6:    nop ; Nop
  ; OPT   #stack_push_int 11 ; Ldc_i4_s
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_1
    #init_var 2, 11 ; Nop
    jmp Wall_BuildRopePit_29 ; Br_s
Wall_BuildRopePit_12:    nop ; Nop
    #locals_push_value_8 1 ; Ldloc_0
    #locals_push_value_8 2 ; Ldloc_1
    #stack_push_int 32 ; Ldc_i4_s
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_SetChar ; Call
    nop ; Nop
    nop ; Nop
  ; OPT   #locals_push_value_8 2 ; Ldloc_1
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_1
    #inc_var 2 ; Nop
Wall_BuildRopePit_29:  ; OPT   #locals_push_value_8 2 ; Ldloc_1
  ; OPT   #stack_push_int 21 ; Ldc_i4_s
  ; OPT   #compareLess ; Clt_un
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_2
  ; OPT   #locals_push_value_8 3 ; Ldloc_2
  ; OPT   #branch_true Wall_BuildRopePit_12 ; Brtrue_s
    #branch_if_var_less 2, 21, Wall_BuildRopePit_12 ; Nop
    #locals_push_value_8 1 ; Ldloc_0
    #stack_push_int 21 ; Ldc_i4_s
    #stack_push_int 114 ; Ldc_i4_s
    #stack_push_int 2 ; Ldc_i4_2
    jsr C64_SetChar ; Call
    nop ; Nop
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldloc_0
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 1, 0 ; Stloc_0
    #inc_var 1 ; Nop
Wall_BuildRopePit_55:  ; OPT   #locals_push_value_8 1 ; Ldloc_0
  ; OPT   #stack_push_int 28 ; Ldc_i4_s
  ; OPT   #compareLess ; Clt_un
  ; OPT   #locals_pull_value_8 4, 0 ; Stloc_3
  ; OPT   #locals_push_value_8 4 ; Ldloc_3
  ; OPT   #branch_true Wall_BuildRopePit_6 ; Brtrue_s
    #branch_if_var_less 1, 28, Wall_BuildRopePit_6 ; Nop
    #stack_push_int 12 ; Ldc_i4_s
    #locals_pull_value_8 5, 0 ; Stloc_s
    jmp Wall_BuildRopePit_91 ; Br_s
Wall_BuildRopePit_70:    nop ; Nop
    #stack_push_int 11 ; Ldc_i4_s
    #locals_push_value_8 5 ; Ldloc_s
    #stack_push_int 60 ; Ldc_i4_s
    #stack_push_int 2 ; Ldc_i4_2
    jsr C64_SetChar ; Call
    nop ; Nop
    nop ; Nop
    #locals_push_value_8 5 ; Ldloc_s
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #locals_pull_value_8 5, 0 ; Stloc_s
Wall_BuildRopePit_91:    #locals_push_value_8 5 ; Ldloc_s
    #stack_push_int 22 ; Ldc_i4_s
    #compareLess ; Clt_un
    #locals_pull_value_8 6, 0 ; Stloc_s
    #locals_push_value_8 6 ; Ldloc_s
    #branch_true Wall_BuildRopePit_70 ; Brtrue_s
    #stack_push_int 11 ; Ldc_i4_s
    #stack_push_int 11 ; Ldc_i4_s
    #stack_push_int 59 ; Ldc_i4_s
    #stack_push_int 2 ; Ldc_i4_2
    jsr C64_SetChar ; Call
    nop ; Nop
    #method_exit 8, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: KnightPitFrame1
;----------------------------------------
Wall_KnightPitFrame1 
    #init_locals_pull_parameters 0, [0]
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 8 ; Ldc_i4_8
    #stack_push_int 32 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 9 ; Ldc_i4_s
    #stack_push_int 32 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 10 ; Ldc_i4_s
    #stack_push_int 69 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 11 ; Ldc_i4_s
    #stack_push_int 71 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 12 ; Ldc_i4_s
    #stack_push_int 79 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 13 ; Ldc_i4_s
    #stack_push_int 80 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 14 ; Ldc_i4_s
    #stack_push_int 81 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: KnightPitFrame2
;----------------------------------------
Wall_KnightPitFrame2 
    #init_locals_pull_parameters 0, [0]
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 8 ; Ldc_i4_8
    #stack_push_int 32 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 9 ; Ldc_i4_s
    #stack_push_int 69 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 10 ; Ldc_i4_s
    #stack_push_int 70 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 11 ; Ldc_i4_s
    #stack_push_int 71 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 12 ; Ldc_i4_s
    #stack_push_int 82 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 13 ; Ldc_i4_s
    #stack_push_int 83 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 14 ; Ldc_i4_s
    #stack_push_int 84 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Wall
; METHOD: KnightPitFrame3
;----------------------------------------
Wall_KnightPitFrame3 
    #init_locals_pull_parameters 0, [0]
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 8 ; Ldc_i4_8
    #stack_push_int 69 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 9 ; Ldc_i4_s
    #stack_push_int 70 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 10 ; Ldc_i4_s
    #stack_push_int 70 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 11 ; Ldc_i4_s
    #stack_push_int 86 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 12 ; Ldc_i4_s
    #stack_push_int 85 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 13 ; Ldc_i4_s
    #stack_push_int 87 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 14 ; Ldc_i4_s
    #stack_push_int 88 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    #method_exit 3, [] ; Ret
