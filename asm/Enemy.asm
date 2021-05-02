

;----------------------------------------
; TYPE: Hunchback.Enemy
; METHOD: get_X
;----------------------------------------
Enemy_get_X 
    #init_locals_pull_parameters 0, [0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Enemy
; METHOD: set_X
;----------------------------------------
Enemy_set_X 
    #init_locals_pull_parameters 0, [0,0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stfld 0 ; Stfld
    #setfld 1, 2, 0 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #locals_push_value_8 2 ; Ldarg_1
    jsr Sprite_set_X ; Callvirt
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Enemy
; METHOD: get_Y
;----------------------------------------
Enemy_get_Y 
    #init_locals_pull_parameters 0, [0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Enemy
; METHOD: set_Y
;----------------------------------------
Enemy_set_Y 
    #init_locals_pull_parameters 0, [0,0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stfld 1 ; Stfld
    #setfld 1, 2, 1 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #locals_push_value_8 2 ; Ldarg_1
    jsr Sprite_set_Y ; Callvirt
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Enemy
; METHOD: set_Sprite
;----------------------------------------
Enemy_set_Sprite 
    #init_locals_pull_parameters 0, [0,0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stfld 5 ; Stfld
    #setfld 1, 2, 5 ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Enemy
; METHOD: Init
;----------------------------------------
Enemy_Init 
    #init_locals_pull_parameters 0, [0,0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stfld 3 ; Stfld
    #setfld 1, 2, 3 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_MultiColor ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #compareGreater_unsigned ; Cgt_un
    compareGreater_unsigned8_const 0 ; Nop
    jsr Sprite_set_Visible ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #stack_push_int 4 ; Ldc_i4_4
    jsr Sprite_set_Color ; Callvirt
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 117 ; Ldc_i4_s
    jsr Enemy_set_Y ; Call
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 16 ; Ldc_i4_s
    #and8 ; And
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #branch_less_equal Enemy_Init_79 ; Ble_s
    #branch_less_equal_const 0, Enemy_Init_79 ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 87 ; Ldc_i4_s
    jsr Enemy_set_Y ; Call
Enemy_Init_79:    #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 8 ; Ldc_i4_8
    #and8 ; And
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #compareGreater ; Cgt
    compareGreater8_const 0 ; Nop
    #stfld 6 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 2 ; Ldc_i4_2
    #and8 ; And
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #compareGreater ; Cgt
    compareGreater8_const 0 ; Nop
    #stfld 7 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 6 ; Ldfld
    #pushfld 6 ; Nop
    #branch_false Enemy_Init_149 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    jsr Enemy_set_X ; Call
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 4 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    jsr Sprite_set_HighPosition ; Callvirt
    jmp Enemy_Init_176 ; Br_s
Enemy_Init_149:    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 4 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_HighPosition ; Callvirt
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 60 ; Ldc_i4_s
    jsr Enemy_set_X ; Call
Enemy_Init_176:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 7 ; Ldfld
    #pushfld 7 ; Nop
    #branch_false Enemy_Init_213 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 6 ; Ldfld
    #pushfld 6 ; Nop
    #branch_false Enemy_Init_213 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
  ; OPT   #stack_push_pointer string_1879048193 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_arrow_right ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
Enemy_Init_213:    jsr C64_get_Sound ; Call
    #stack_push_int 128 ; Ldc_i4
    #stack_push_int 100 ; Ldc_i4_s
    #stack_push_int 44 ; Ldc_i4_s
    #stack_push_int 0 ; Ldc_i4_0
    #stack_push_int 0 ; Ldc_i4_0
    #stack_push_int 128 ; Ldc_i4
    #stack_push_int 0 ; Ldc_i4_0
    #stack_push_int 0 ; Ldc_i4_0
    jsr Sound_PlayEffectReg2 ; Callvirt
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Enemy
; METHOD: Move
;----------------------------------------
Enemy_Move 
    #init_locals_pull_parameters 0, [0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #branch_true Enemy_Move_9 ; Brtrue_s
    #method_exit 3, [] ; Ret
Enemy_Move_9:    #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #stfld 2 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #stack_push_int 4 ; Ldc_i4_4
  ; OPT   #branch_not_equal Enemy_Move_39 ; Bne_un_s
    #branch_not_equal_const 4, Enemy_Move_39 ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 2 ; Stfld
Enemy_Move_39:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 6 ; Ldfld
    #pushfld 6 ; Nop
    #branch_false Enemy_Move_120 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr Enemy_get_X ; Call
    #stack_push_int 4 ; Ldc_i4_4
    #add ; Add
    jsr Enemy_set_X ; Call
    #locals_push_value_8 1 ; Ldarg_0
    jsr Enemy_get_X ; Call
    #branch_true Enemy_Move_88 ; Brtrue_s
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 4 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_HighPosition ; Callvirt
Enemy_Move_88:    #locals_push_value_8 1 ; Ldarg_0
    jsr Enemy_get_X ; Call
  ; OPT   #stack_push_int 60 ; Ldc_i4_s
  ; OPT   #branch_not_equal Enemy_Move_183 ; Bne_un_s
    #branch_not_equal_const 60, Enemy_Move_183 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld 4 ; Nop
    #branch_false Enemy_Move_183 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    jsr Enemy_Init ; Call
    jmp Enemy_Move_183 ; Br_s
Enemy_Move_120:    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr Enemy_get_X ; Call
    #stack_push_int 4 ; Ldc_i4_4
    #sub ; Sub
    jsr Enemy_set_X ; Call
    #locals_push_value_8 1 ; Ldarg_0
    jsr Enemy_get_X ; Call
    #branch_true Enemy_Move_183 ; Brtrue_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld 4 ; Nop
    #branch_false Enemy_Move_171 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 4 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    jsr Sprite_set_HighPosition ; Callvirt
    jmp Enemy_Move_183 ; Br_s
Enemy_Move_171:    #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    jsr Enemy_Init ; Call
Enemy_Move_183:    #locals_push_value_8 1 ; Ldarg_0
    jsr Enemy_SetFrame ; Call
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Enemy
; METHOD: SetFrame
;----------------------------------------
Enemy_SetFrame 
    #init_locals_pull_parameters 1, [0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 7 ; Ldfld
    #pushfld 7 ; Nop
    #branch_false Enemy_SetFrame_9 ; Brfalse_s
    #method_exit 4, [] ; Ret
Enemy_SetFrame_9:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #locals_pull_value_8 2, 0 ; Stloc_0
    #locals_push_value_8 2 ; Ldloc_0
    #switch Enemy_SetFrame_Jump_17 ; Switch
    #method_exit 4, [] ; Ret
Enemy_SetFrame_39:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
  ; OPT   #stack_push_pointer string_1879048225 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_fireball_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 4, [] ; Ret
Enemy_SetFrame_61:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
  ; OPT   #stack_push_pointer string_1879048255 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_fireball_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 4, [] ; Ret
Enemy_SetFrame_83:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
  ; OPT   #stack_push_pointer string_1879048285 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_fireball_2 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 4, [] ; Ret
Enemy_SetFrame_105:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
  ; OPT   #stack_push_pointer string_1879048315 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_fireball_3 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 4, [] ; Ret
Enemy_SetFrame_Jump_17 .word  Enemy_SetFrame_39-1,  Enemy_SetFrame_61-1,  Enemy_SetFrame_83-1,  Enemy_SetFrame_105-1, 0
