

;----------------------------------------
; TYPE: Hunchback.Knight
; METHOD: get_X
;----------------------------------------
Knight_get_X 
    #init_locals_pull_parameters 0, [0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 0 ; Ldfld
    #pushfld8 0 ; Nop
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Knight
; METHOD: set_X
;----------------------------------------
Knight_set_X 
    #init_locals_pull_parameters 0, [0,0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 2 ; Ldarg_1
  ; OPT   #stfld8 0 ; Stfld
    #setfld8 1, 2, 0 ; Nop
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 5 ; Ldfld
    #pushfld8 5 ; Nop
    #locals_push_value8 2 ; Ldarg_1
    #conv_8_16 ; Conv_u8
    jsr Sprite_set_X ; Callvirt
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Knight
; METHOD: get_Y
;----------------------------------------
Knight_get_Y 
    #init_locals_pull_parameters 0, [0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 1 ; Ldfld
    #pushfld8 1 ; Nop
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Knight
; METHOD: set_Y
;----------------------------------------
Knight_set_Y 
    #init_locals_pull_parameters 0, [0,0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 2 ; Ldarg_1
  ; OPT   #stfld8 1 ; Stfld
    #setfld8 1, 2, 1 ; Nop
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 5 ; Ldfld
    #pushfld8 5 ; Nop
    #locals_push_value8 2 ; Ldarg_1
    jsr Sprite_set_Y ; Callvirt
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Knight
; METHOD: set_Sprite
;----------------------------------------
Knight_set_Sprite 
    #init_locals_pull_parameters 0, [0,0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 2 ; Ldarg_1
  ; OPT   #stfld8 5 ; Stfld
    #setfld8 1, 2, 5 ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Knight
; METHOD: Init
;----------------------------------------
Knight_Init 
    #init_locals_pull_parameters 0, [0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 5 ; Ldfld
    #pushfld8 5 ; Nop
  ; OPT   #stack_push_pointer string_1879048387 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_knight_climb_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 5 ; Ldfld
    #pushfld8 5 ; Nop
    #stack_push_int8 1 ; Ldc_i4_1
    jsr Sprite_set_MultiColor ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 5 ; Ldfld
    #pushfld8 5 ; Nop
    #stack_push_int8 1 ; Ldc_i4_1
    jsr Sprite_set_Visible ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 5 ; Ldfld
    #pushfld8 5 ; Nop
    #stack_push_int8 11 ; Ldc_i4_s
    jsr Sprite_set_Color ; Callvirt
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 1 ; Ldc_i4_1
    #stfld8 4 ; Stfld
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 197 ; Ldc_i4
    jsr Knight_set_Y ; Call
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 40 ; Ldc_i4_s
    jsr Knight_set_X ; Call
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Knight
; METHOD: Move
;----------------------------------------
Knight_Move 
    #init_locals_pull_parameters 0, [0]
    #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
    #stack_push_int8 1 ; Ldc_i4_1
    #add8 ; Add
    #stfld8 3 ; Stfld
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
  ; OPT   #stack_push_int8 32 ; Ldc_i4_s
  ; OPT   #branch_greater_equal8 Knight_Move_25 ; Bge_s
    #branch_greater_equal_const 32, Knight_Move_25 ; Nop
    #method_exit 3, [] ; Ret
Knight_Move_25:    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 0 ; Ldc_i4_0
    #stfld8 3 ; Stfld
    #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #branch_false Knight_Move_44 ; Brfalse_s
    #stack_push_int8 0 ; Ldc_i4_0
    jmp Knight_Move_45 ; Br_s
Knight_Move_44:    #stack_push_int8 1 ; Ldc_i4_1
Knight_Move_45:    #stfld8 2 ; Stfld
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 4 ; Ldfld
    #pushfld8 4 ; Nop
    #branch_false Knight_Move_91 ; Brfalse_s
    #locals_push_value8 1 ; Ldarg_0
    #locals_push_value8 1 ; Ldarg_0
    jsr Knight_get_Y ; Call
    #stack_push_int8 8 ; Ldc_i4_8
    #sub8 ; Sub
    jsr Knight_set_Y ; Call
    #locals_push_value8 1 ; Ldarg_0
    jsr Knight_get_Y ; Call
  ; OPT   #stack_push_int8 117 ; Ldc_i4_s
  ; OPT   #branch_not_equal8 Knight_Move_105 ; Bne_un_s
    #branch_not_equal_const 117, Knight_Move_105 ; Nop
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 0 ; Ldc_i4_0
    #stfld8 4 ; Stfld
    jmp Knight_Move_105 ; Br_s
Knight_Move_91:    #locals_push_value8 1 ; Ldarg_0
    #locals_push_value8 1 ; Ldarg_0
    jsr Knight_get_X ; Call
    #stack_push_int8 8 ; Ldc_i4_8
    #add8 ; Add
    jsr Knight_set_X ; Call
Knight_Move_105:    #locals_push_value8 1 ; Ldarg_0
    jsr Knight_SetFrame ; Call
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Knight
; METHOD: SetFrame
;----------------------------------------
Knight_SetFrame 
    #init_locals_pull_parameters 0, [0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 4 ; Ldfld
    #pushfld8 4 ; Nop
    #branch_false Knight_SetFrame_60 ; Brfalse_s
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #branch_true Knight_SetFrame_38 ; Brtrue_s
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 5 ; Ldfld
    #pushfld8 5 ; Nop
  ; OPT   #stack_push_pointer string_1879048387 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_knight_climb_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 3, [] ; Ret
Knight_SetFrame_38:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 5 ; Ldfld
    #pushfld8 5 ; Nop
  ; OPT   #stack_push_pointer string_1879048425 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_knight_climb_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 3, [] ; Ret
Knight_SetFrame_60:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #branch_true Knight_SetFrame_90 ; Brtrue_s
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 5 ; Ldfld
    #pushfld8 5 ; Nop
  ; OPT   #stack_push_pointer string_1879048463 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_knight_walk_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 3, [] ; Ret
Knight_SetFrame_90:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 5 ; Ldfld
    #pushfld8 5 ; Nop
  ; OPT   #stack_push_pointer string_1879048499 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_knight_walk_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 3, [] ; Ret
