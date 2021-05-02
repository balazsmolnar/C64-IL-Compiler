

;----------------------------------------
; TYPE: Hunchback.Knight
; METHOD: get_X
;----------------------------------------
Knight_get_X 
    #init_locals_pull_parameters 0, [0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Knight
; METHOD: set_X
;----------------------------------------
Knight_set_X 
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
; TYPE: Hunchback.Knight
; METHOD: get_Y
;----------------------------------------
Knight_get_Y 
    #init_locals_pull_parameters 0, [0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Knight
; METHOD: set_Y
;----------------------------------------
Knight_set_Y 
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
; TYPE: Hunchback.Knight
; METHOD: set_Sprite
;----------------------------------------
Knight_set_Sprite 
    #init_locals_pull_parameters 0, [0,0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stfld 5 ; Stfld
    #setfld 1, 2, 5 ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Knight
; METHOD: Init
;----------------------------------------
Knight_Init 
    #init_locals_pull_parameters 0, [0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
  ; OPT   #stack_push_pointer string_1879048361 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_knight_climb_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_MultiColor ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_Visible ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #stack_push_int 11 ; Ldc_i4_s
    jsr Sprite_set_Color ; Callvirt
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 4 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 197 ; Ldc_i4
    jsr Knight_set_Y ; Call
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 40 ; Ldc_i4_s
    jsr Knight_set_X ; Call
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Knight
; METHOD: Move
;----------------------------------------
Knight_Move 
    #init_locals_pull_parameters 0, [0]
    #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #stfld 3 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
  ; OPT   #stack_push_int 32 ; Ldc_i4_s
  ; OPT   #branch_greater_equal Knight_Move_25 ; Bge_s
    #branch_greater_equal_const 32, Knight_Move_25 ; Nop
    #method_exit 3, [] ; Ret
Knight_Move_25:    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 3 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #branch_false Knight_Move_44 ; Brfalse_s
    #stack_push_int 0 ; Ldc_i4_0
    jmp Knight_Move_45 ; Br_s
Knight_Move_44:    #stack_push_int 1 ; Ldc_i4_1
Knight_Move_45:    #stfld 2 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld 4 ; Nop
    #branch_false Knight_Move_91 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr Knight_get_Y ; Call
    #stack_push_int 8 ; Ldc_i4_8
    #sub ; Sub
    jsr Knight_set_Y ; Call
    #locals_push_value_8 1 ; Ldarg_0
    jsr Knight_get_Y ; Call
  ; OPT   #stack_push_int 117 ; Ldc_i4_s
  ; OPT   #branch_not_equal Knight_Move_105 ; Bne_un_s
    #branch_not_equal_const 117, Knight_Move_105 ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 4 ; Stfld
    jmp Knight_Move_105 ; Br_s
Knight_Move_91:    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr Knight_get_X ; Call
    #stack_push_int 8 ; Ldc_i4_8
    #add ; Add
    jsr Knight_set_X ; Call
Knight_Move_105:    #locals_push_value_8 1 ; Ldarg_0
    jsr Knight_SetFrame ; Call
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Knight
; METHOD: SetFrame
;----------------------------------------
Knight_SetFrame 
    #init_locals_pull_parameters 0, [0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld 4 ; Nop
    #branch_false Knight_SetFrame_60 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #branch_true Knight_SetFrame_38 ; Brtrue_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
  ; OPT   #stack_push_pointer string_1879048361 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_knight_climb_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 3, [] ; Ret
Knight_SetFrame_38:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
  ; OPT   #stack_push_pointer string_1879048399 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_knight_climb_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 3, [] ; Ret
Knight_SetFrame_60:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #branch_true Knight_SetFrame_90 ; Brtrue_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
  ; OPT   #stack_push_pointer string_1879048437 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_knight_walk_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 3, [] ; Ret
Knight_SetFrame_90:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
  ; OPT   #stack_push_pointer string_1879048473 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_knight_walk_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 3, [] ; Ret
