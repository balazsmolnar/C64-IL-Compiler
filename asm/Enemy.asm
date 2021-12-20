
Enemy_VTable:  .word Enemy_Init,Enemy_Move


;----------------------------------------
; TYPE: Hunchback.Enemy
; METHOD: Init
;----------------------------------------
Enemy_Init 
    #init_locals_pull_parameters 0, [0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 4 ; Ldfld
    #pushfld8 4 ; Nop
    #stack_push_int8 1 ; Ldc_i4_1
    jsr Sprite_set_MultiColor ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 4 ; Ldfld
    #pushfld8 4 ; Nop
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 7 ; Ldfld
    #pushfld8 7 ; Nop
  ; OPT   #stack_push_int8 0 ; Ldc_i4_0
  ; OPT   #compareGreater_unsigned8 ; Cgt_un
    compareGreater_unsigned_const8 0 ; Nop
    jsr Sprite_set_Visible ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 4 ; Ldfld
    #pushfld8 4 ; Nop
    #stack_push_int8 4 ; Ldc_i4_4
    jsr Sprite_set_Color ; Callvirt
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 117 ; Ldc_i4_s
    jsr Enemy_set_Y ; Call
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 7 ; Ldfld
    #pushfld8 7 ; Nop
    #stack_push_int8 16 ; Ldc_i4_s
    #and8 ; And
  ; OPT   #stack_push_int8 0 ; Ldc_i4_0
  ; OPT   #branch_less_equal8 Enemy_Init_72 ; Ble_s
    #branch_less_equal_const 0, Enemy_Init_72 ; Nop
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 87 ; Ldc_i4_s
    jsr Enemy_set_Y ; Call
Enemy_Init_72:    #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 7 ; Ldfld
    #pushfld8 7 ; Nop
    #stack_push_int8 8 ; Ldc_i4_8
    #and8 ; And
  ; OPT   #stack_push_int8 0 ; Ldc_i4_0
  ; OPT   #compareGreater8 ; Cgt
    compareGreater_const8 0 ; Nop
    #stfld8 5 ; Stfld
    #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 7 ; Ldfld
    #pushfld8 7 ; Nop
    #stack_push_int8 2 ; Ldc_i4_2
    #and8 ; And
  ; OPT   #stack_push_int8 0 ; Ldc_i4_0
  ; OPT   #compareGreater8 ; Cgt
    compareGreater_const8 0 ; Nop
    #stfld8 6 ; Stfld
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 5 ; Ldfld
    #pushfld8 5 ; Nop
    #branch_false Enemy_Init_124 ; Brfalse_s
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int16 0 ; Ldc_i4_0
    ; conv ; Conv_i8
    jsr Enemy_set_X ; Call
    jmp Enemy_Init_136 ; Br_s
Enemy_Init_124:    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int16 316 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Enemy_set_X ; Call
Enemy_Init_136:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 6 ; Ldfld
    #pushfld8 6 ; Nop
    #branch_false Enemy_Init_173 ; Brfalse_s
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 5 ; Ldfld
    #pushfld8 5 ; Nop
    #branch_false Enemy_Init_173 ; Brfalse_s
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 4 ; Ldfld
    #pushfld8 4 ; Nop
  ; OPT   #stack_push_pointer string_1879048193 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_arrow_right ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
Enemy_Init_173:    jsr C64_get_Sound ; Call
    #stack_push_int8 128 ; Ldc_i4
    #stack_push_int16 11364 ; Ldc_i4
    ; conv ; Conv_i8
    #stack_push_int16 0 ; Ldc_i4_0
    ; conv ; Conv_i8
    #stack_push_int8 128 ; Ldc_i4
    #stack_push_int8 0 ; Ldc_i4_0
    #stack_push_int8 0 ; Ldc_i4_0
    jsr Sound_PlayEffectReg2 ; Callvirt
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Enemy
; METHOD: Move
;----------------------------------------
Enemy_Move 
    #init_locals_pull_parameters 0, [0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 7 ; Ldfld
    #pushfld8 7 ; Nop
    #branch_true Enemy_Move_9 ; Brtrue_s
    #method_exit 3, [] ; Ret
Enemy_Move_9:    #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
    #stack_push_int8 1 ; Ldc_i4_1
    #add8 ; Add
    #stfld8 3 ; Stfld
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
  ; OPT   #stack_push_int8 4 ; Ldc_i4_4
  ; OPT   #branch_not_equal8 Enemy_Move_39 ; Bne_un_s
    #branch_not_equal_const 4, Enemy_Move_39 ; Nop
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 0 ; Ldc_i4_0
    #stfld8 3 ; Stfld
Enemy_Move_39:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 5 ; Ldfld
    #pushfld8 5 ; Nop
    #branch_false Enemy_Move_84 ; Brfalse_s
    #locals_push_value8 1 ; Ldarg_0
    #locals_push_value8 1 ; Ldarg_0
    jsr Enemy_get_X ; Call
    #stack_push_int16 4 ; Ldc_i4_4
    ; conv ; Conv_i8
    #add16 ; Add
    jsr Enemy_set_X ; Call
    #locals_push_value8 1 ; Ldarg_0
    jsr Enemy_get_X ; Call
    #stack_push_int16 316 ; Ldc_i4
    ; conv ; Conv_i8
    #branch_not_equal16 Enemy_Move_116 ; Bne_un_s
    #locals_push_value8 1 ; Ldarg_0
    #callVirt 0, 1 ; Callvirt
    jmp Enemy_Move_116 ; Br_s
Enemy_Move_84:    #locals_push_value8 1 ; Ldarg_0
    #locals_push_value8 1 ; Ldarg_0
    jsr Enemy_get_X ; Call
    #stack_push_int16 4 ; Ldc_i4_4
    ; conv ; Conv_i8
    #sub16 ; Sub
    jsr Enemy_set_X ; Call
    #locals_push_value8 1 ; Ldarg_0
    jsr Enemy_get_X ; Call
    #stack_push_int16 20 ; Ldc_i4_s
    ; conv ; Conv_i8
    #branch_not_equal16 Enemy_Move_116 ; Bne_un_s
    #locals_push_value8 1 ; Ldarg_0
    #callVirt 0, 1 ; Callvirt
Enemy_Move_116:    #locals_push_value8 1 ; Ldarg_0
    jsr Enemy_SetFrame ; Call
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Enemy
; METHOD: get_X
;----------------------------------------
Enemy_get_X 
    #init_locals_pull_parameters 0, [0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld16 0 ; Ldfld
    #pushfld16 0 ; Nop
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Enemy
; METHOD: set_X
;----------------------------------------
Enemy_set_X 
    #init_locals_pull_parameters 0, [0, 0,0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value16 3 ; Ldarg_1
  ; OPT   #stfld16 0 ; Stfld
    #setfld16 1, 3, 0 ; Nop
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 4 ; Ldfld
    #pushfld8 4 ; Nop
    #locals_push_value16 3 ; Ldarg_1
    jsr Sprite_set_X ; Callvirt
    #method_exit 5, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Enemy
; METHOD: get_Y
;----------------------------------------
Enemy_get_Y 
    #init_locals_pull_parameters 0, [0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Enemy
; METHOD: set_Y
;----------------------------------------
Enemy_set_Y 
    #init_locals_pull_parameters 0, [0,0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 2 ; Ldarg_1
  ; OPT   #stfld8 2 ; Stfld
    #setfld8 1, 2, 2 ; Nop
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 4 ; Ldfld
    #pushfld8 4 ; Nop
    #locals_push_value8 2 ; Ldarg_1
    jsr Sprite_set_Y ; Callvirt
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Enemy
; METHOD: set_Sprite
;----------------------------------------
Enemy_set_Sprite 
    #init_locals_pull_parameters 0, [0,0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 2 ; Ldarg_1
  ; OPT   #stfld8 4 ; Stfld
    #setfld8 1, 2, 4 ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Enemy
; METHOD: SetFrame
;----------------------------------------
Enemy_SetFrame 
    #init_locals_pull_parameters 1, [0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 6 ; Ldfld
    #pushfld8 6 ; Nop
    #branch_false Enemy_SetFrame_9 ; Brfalse_s
    #method_exit 4, [] ; Ret
Enemy_SetFrame_9:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
    #locals_pull_value8 2, 0 ; Stloc_0
    #locals_push_value8 2 ; Ldloc_0
    #switch Enemy_SetFrame_Jump_17 ; Switch
    #method_exit 4, [] ; Ret
Enemy_SetFrame_39:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 4 ; Ldfld
    #pushfld8 4 ; Nop
  ; OPT   #stack_push_pointer string_1879048225 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_fireball_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 4, [] ; Ret
Enemy_SetFrame_61:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 4 ; Ldfld
    #pushfld8 4 ; Nop
  ; OPT   #stack_push_pointer string_1879048255 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_fireball_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 4, [] ; Ret
Enemy_SetFrame_83:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 4 ; Ldfld
    #pushfld8 4 ; Nop
  ; OPT   #stack_push_pointer string_1879048285 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_fireball_2 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 4, [] ; Ret
Enemy_SetFrame_105:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 4 ; Ldfld
    #pushfld8 4 ; Nop
  ; OPT   #stack_push_pointer string_1879048315 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_fireball_3 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 4, [] ; Ret
Enemy_SetFrame_Jump_17 .word  Enemy_SetFrame_39-1,  Enemy_SetFrame_61-1,  Enemy_SetFrame_83-1,  Enemy_SetFrame_105-1, 0
