

;----------------------------------------
; TYPE: Hunchback.Enemy
; METHOD: get_X
;----------------------------------------
Enemy_get_X 
    #init_locals_pull_parameters 1, [0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
  ; OPT   jmp Enemy_get_X_10 ; Br_s
Enemy_get_X_10:  ; OPT   #locals_push_value_8 2 ; Ldloc_0
    #pushfld 0 ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Enemy
; METHOD: set_X
;----------------------------------------
Enemy_set_X 
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
; TYPE: Hunchback.Enemy
; METHOD: get_Y
;----------------------------------------
Enemy_get_Y 
    #init_locals_pull_parameters 1, [0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
  ; OPT   jmp Enemy_get_Y_10 ; Br_s
Enemy_get_Y_10:  ; OPT   #locals_push_value_8 2 ; Ldloc_0
    #pushfld 1 ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Enemy
; METHOD: set_Y
;----------------------------------------
Enemy_set_Y 
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
; TYPE: Hunchback.Enemy
; METHOD: set_Sprite
;----------------------------------------
Enemy_set_Sprite 
    #init_locals_pull_parameters 0, [0,0]
    nop ; Nop
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
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stfld 3 ; Stfld
    #setfld 1, 2, 3 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_MultiColor ; Callvirt
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    #compareGreater_unsigned ; Cgt_un
    jsr Sprite_set_Visible ; Callvirt
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #stack_push_int 4 ; Ldc_i4_4
    jsr Sprite_set_Color ; Callvirt
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_HighPosition ; Callvirt
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 4 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 117 ; Ldc_i4_s
    jsr Enemy_set_Y ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 60 ; Ldc_i4_s
    jsr Enemy_set_X ; Call
    nop ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Enemy
; METHOD: Move
;----------------------------------------
Enemy_Move 
    #init_locals_pull_parameters 4, [0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #locals_pull_value_8 2, 0 ; Stloc_0
    #locals_push_value_8 2 ; Ldloc_0
    #branch_false Enemy_Move_16 ; Brfalse_s
    jmp Enemy_Move_136 ; Br_s
Enemy_Move_16:    #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #stfld 2 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 4 ; Ldc_i4_4
    #compareEqual ; Ceq
    #locals_pull_value_8 3, 0 ; Stloc_1
    #locals_push_value_8 3 ; Ldloc_1
    #branch_false Enemy_Move_50 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 2 ; Stfld
Enemy_Move_50:    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr Enemy_get_X ; Call
    #stack_push_int 4 ; Ldc_i4_4
    #sub ; Sub
    jsr Enemy_set_X ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    jsr Enemy_get_X ; Call
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #locals_pull_value_8 4, 0 ; Stloc_2
    #locals_push_value_8 4 ; Ldloc_2
    #branch_false Enemy_Move_129 ; Brfalse_s
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld 4 ; Nop
    #locals_pull_value_8 5, 0 ; Stloc_3
    #locals_push_value_8 5 ; Ldloc_3
    #branch_false Enemy_Move_113 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 4 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    jsr Sprite_set_HighPosition ; Callvirt
    nop ; Nop
    nop ; Nop
    jmp Enemy_Move_128 ; Br_s
Enemy_Move_113:    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    jsr Enemy_Init ; Call
    nop ; Nop
    nop ; Nop
Enemy_Move_128:    nop ; Nop
Enemy_Move_129:    #locals_push_value_8 1 ; Ldarg_0
    jsr Enemy_SetFrame ; Call
    nop ; Nop
Enemy_Move_136:    #method_exit 7, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Enemy
; METHOD: SetFrame
;----------------------------------------
Enemy_SetFrame 
    #init_locals_pull_parameters 4, [0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #locals_pull_value_8 2, 0 ; Stloc_0
    #locals_push_value_8 2 ; Ldloc_0
    #branch_false Enemy_SetFrame_38 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
  ; OPT   #stack_push_pointer string_1879048193 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_fireball_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Enemy_SetFrame_147 ; Br_s
Enemy_SetFrame_38:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #compareEqual ; Ceq
    #locals_pull_value_8 3, 0 ; Stloc_1
    #locals_push_value_8 3 ; Ldloc_1
    #branch_false Enemy_SetFrame_75 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
  ; OPT   #stack_push_pointer string_1879048223 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_fireball_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Enemy_SetFrame_147 ; Br_s
Enemy_SetFrame_75:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 2 ; Ldc_i4_2
    #compareEqual ; Ceq
    #locals_pull_value_8 4, 0 ; Stloc_2
    #locals_push_value_8 4 ; Ldloc_2
    #branch_false Enemy_SetFrame_112 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
  ; OPT   #stack_push_pointer string_1879048253 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_fireball_2 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Enemy_SetFrame_147 ; Br_s
Enemy_SetFrame_112:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 3 ; Ldc_i4_3
    #compareEqual ; Ceq
    #locals_pull_value_8 5, 0 ; Stloc_3
    #locals_push_value_8 5 ; Ldloc_3
    #branch_false Enemy_SetFrame_147 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
  ; OPT   #stack_push_pointer string_1879048283 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_fireball_3 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
Enemy_SetFrame_147:    #method_exit 7, [] ; Ret
