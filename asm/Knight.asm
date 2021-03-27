

;----------------------------------------
; TYPE: Hunchback.Knight
; METHOD: get_X
;----------------------------------------
Knight_get_X 
    #init_locals_pull_parameters 1, [0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
  ; OPT   jmp Knight_get_X_10 ; Br_s
Knight_get_X_10:  ; OPT   #locals_push_value_8 2 ; Ldloc_0
    #pushfld 0 ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Knight
; METHOD: set_X
;----------------------------------------
Knight_set_X 
    #init_locals_pull_parameters 0, [0,0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stfld 0 ; Stfld
    #setfld 1, 2, 0 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #locals_push_value_8 2 ; Ldarg_1
    jsr Sprite_set_X ; Callvirt
    nop ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Knight
; METHOD: get_Y
;----------------------------------------
Knight_get_Y 
    #init_locals_pull_parameters 1, [0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
  ; OPT   jmp Knight_get_Y_10 ; Br_s
Knight_get_Y_10:  ; OPT   #locals_push_value_8 2 ; Ldloc_0
    #pushfld 1 ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Knight
; METHOD: set_Y
;----------------------------------------
Knight_set_Y 
    #init_locals_pull_parameters 0, [0,0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stfld 1 ; Stfld
    #setfld 1, 2, 1 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #locals_push_value_8 2 ; Ldarg_1
    jsr Sprite_set_Y ; Callvirt
    nop ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Knight
; METHOD: set_Sprite
;----------------------------------------
Knight_set_Sprite 
    #init_locals_pull_parameters 0, [0,0]
    nop ; Nop
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
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
  ; OPT   #stack_push_pointer string_1879048329 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_knight_climb_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_MultiColor ; Callvirt
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_Visible ; Callvirt
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #stack_push_int 11 ; Ldc_i4_s
    jsr Sprite_set_Color ; Callvirt
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 4 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 197 ; Ldc_i4
    jsr Knight_set_Y ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 40 ; Ldc_i4_s
    jsr Knight_set_X ; Call
    nop ; Nop
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Knight
; METHOD: Move
;----------------------------------------
Knight_Move 
    #init_locals_pull_parameters 3, [0]
    nop ; Nop
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
    #stack_push_int 32 ; Ldc_i4_s
    #compareLess ; Clt
    #locals_pull_value_8 2, 0 ; Stloc_0
    #locals_push_value_8 2 ; Ldloc_0
    #branch_false Knight_Move_31 ; Brfalse_s
    jmp Knight_Move_130 ; Br_s
Knight_Move_31:    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 3 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #branch_false Knight_Move_50 ; Brfalse_s
    #stack_push_int 0 ; Ldc_i4_0
    jmp Knight_Move_51 ; Br_s
Knight_Move_50:    #stack_push_int 1 ; Ldc_i4_1
Knight_Move_51:    #stfld 2 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld 4 ; Nop
    #locals_pull_value_8 3, 0 ; Stloc_1
    #locals_push_value_8 3 ; Ldloc_1
    #branch_false Knight_Move_106 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr Knight_get_Y ; Call
    #stack_push_int 8 ; Ldc_i4_8
    #sub ; Sub
    jsr Knight_set_Y ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    jsr Knight_get_Y ; Call
    #stack_push_int 117 ; Ldc_i4_s
    #compareEqual ; Ceq
    #locals_pull_value_8 4, 0 ; Stloc_2
    #locals_push_value_8 4 ; Ldloc_2
    #branch_false Knight_Move_103 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 4 ; Stfld
Knight_Move_103:    nop ; Nop
    jmp Knight_Move_123 ; Br_s
Knight_Move_106:    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr Knight_get_X ; Call
    #stack_push_int 8 ; Ldc_i4_8
    #add ; Add
    jsr Knight_set_X ; Call
    nop ; Nop
    nop ; Nop
Knight_Move_123:    #locals_push_value_8 1 ; Ldarg_0
    jsr Knight_SetFrame ; Call
    nop ; Nop
Knight_Move_130:    #method_exit 6, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Knight
; METHOD: Die
;----------------------------------------
Knight_Die 
    #init_locals_pull_parameters 4, [0]
    nop ; Nop
    #stack_push_int 2 ; Ldc_i4_2
    jsr C64_SetBorderColor ; Call
    nop ; Nop
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #init_var 2, 0 ; Nop
    jmp Knight_Die_34 ; Br_s
Knight_Die_12:  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #init_var 3, 0 ; Nop
    jmp Knight_Die_21 ; Br_s
Knight_Die_16:    nop ; Nop
  ; OPT   #locals_push_value_8 3 ; Ldloc_1
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #inc_var 3 ; Nop
Knight_Die_21:  ; OPT   #locals_push_value_8 3 ; Ldloc_1
  ; OPT   #stack_push_int 100 ; Ldc_i4_s
  ; OPT   #compareLess ; Clt
  ; OPT   #locals_pull_value_8 4, 0 ; Stloc_2
  ; OPT   #locals_push_value_8 4 ; Ldloc_2
  ; OPT   #branch_true Knight_Die_16 ; Brtrue_s
    #branch_if_var_less 3, 100, Knight_Die_16 ; Nop
  ; OPT   #locals_push_value_8 2 ; Ldloc_0
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #inc_var 2 ; Nop
Knight_Die_34:  ; OPT   #locals_push_value_8 2 ; Ldloc_0
  ; OPT   #stack_push_int 100 ; Ldc_i4_s
  ; OPT   #compareLess ; Clt
  ; OPT   #locals_pull_value_8 5, 0 ; Stloc_3
  ; OPT   #locals_push_value_8 5 ; Ldloc_3
  ; OPT   #branch_true Knight_Die_12 ; Brtrue_s
    #branch_if_var_less 2, 100, Knight_Die_12 ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_SetBorderColor ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 200 ; Ldc_i4
    jsr Knight_set_Y ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 20 ; Ldc_i4_s
    jsr Knight_set_X ; Call
    nop ; Nop
    #method_exit 7, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Knight
; METHOD: SetFrame
;----------------------------------------
Knight_SetFrame 
    #init_locals_pull_parameters 3, [0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld 4 ; Nop
    #locals_pull_value_8 2, 0 ; Stloc_0
    #locals_push_value_8 2 ; Ldloc_0
    #branch_false Knight_SetFrame_74 ; Brfalse_s
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #locals_pull_value_8 3, 0 ; Stloc_1
    #locals_push_value_8 3 ; Ldloc_1
    #branch_false Knight_SetFrame_49 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
  ; OPT   #stack_push_pointer string_1879048329 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_knight_climb_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Knight_SetFrame_71 ; Br_s
Knight_SetFrame_49:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
  ; OPT   #stack_push_pointer string_1879048367 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_knight_climb_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
Knight_SetFrame_71:    nop ; Nop
    jmp Knight_SetFrame_135 ; Br_s
Knight_SetFrame_74:    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #locals_pull_value_8 4, 0 ; Stloc_2
    #locals_push_value_8 4 ; Ldloc_2
    #branch_false Knight_SetFrame_112 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
  ; OPT   #stack_push_pointer string_1879048405 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_knight_walk_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Knight_SetFrame_134 ; Br_s
Knight_SetFrame_112:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
  ; OPT   #stack_push_pointer string_1879048441 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_knight_walk_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
Knight_SetFrame_134:    nop ; Nop
Knight_SetFrame_135:    #method_exit 6, [] ; Ret
