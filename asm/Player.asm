

;----------------------------------------
; TYPE: Demo.Player
; METHOD: set_Sprite
;----------------------------------------
Player_set_Sprite 
    #init_locals_pull_parameters 0, [0,0]
    nop ; Nop
    jsr C64_get_Sprites ; Call
    #stack_push_int 10 ; Ldc_i4_s
    jsr SpriteCollection_set_CommonColor1 ; Callvirt
    nop ; Nop
    jsr C64_get_Sprites ; Call
    #stack_push_int 6 ; Ldc_i4_6
    jsr SpriteCollection_set_CommonColor2 ; Callvirt
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stfld 5 ; Stfld
    #setfld 1, 2, 5 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
  ; OPT   #stack_push_pointer string_1879048193 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer sprite0 ; Nop
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
    #stack_push_int 9 ; Ldc_i4_s
    jsr Sprite_set_Color ; Callvirt
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 3 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 200 ; Ldc_i4
    jsr Player_set_Y ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 20 ; Ldc_i4_s
    jsr Player_set_X ; Call
    nop ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Demo.Player
; METHOD: get_X
;----------------------------------------
Player_get_X 
    #init_locals_pull_parameters 1, [0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
  ; OPT   jmp Player_get_X_10 ; Br_s
Player_get_X_10:  ; OPT   #locals_push_value_8 2 ; Ldloc_0
    #pushfld 0 ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Demo.Player
; METHOD: get_Y
;----------------------------------------
Player_get_Y 
    #init_locals_pull_parameters 1, [0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
  ; OPT   jmp Player_get_Y_10 ; Br_s
Player_get_Y_10:  ; OPT   #locals_push_value_8 2 ; Ldloc_0
    #pushfld 1 ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Demo.Player
; METHOD: Move
;----------------------------------------
Player_Move 
    #init_locals_pull_parameters 0, [0,0]
    nop ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Demo.Player
; METHOD: set_X
;----------------------------------------
Player_set_X 
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
; TYPE: Demo.Player
; METHOD: set_Y
;----------------------------------------
Player_set_Y 
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
; TYPE: Demo.Player
; METHOD: Die
;----------------------------------------
Player_Die 
    #init_locals_pull_parameters 4, [0]
    nop ; Nop
    #stack_push_int 2 ; Ldc_i4_2
    jsr C64_SetBorderColor ; Call
    nop ; Nop
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #init_var 2, 0 ; Nop
    jmp Player_Die_34 ; Br_s
Player_Die_12:  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #init_var 3, 0 ; Nop
    jmp Player_Die_21 ; Br_s
Player_Die_16:    nop ; Nop
  ; OPT   #locals_push_value_8 3 ; Ldloc_1
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #inc_var 3 ; Nop
Player_Die_21:  ; OPT   #locals_push_value_8 3 ; Ldloc_1
  ; OPT   #stack_push_int 100 ; Ldc_i4_s
  ; OPT   #compareLess ; Clt
  ; OPT   #locals_pull_value_8 4, 0 ; Stloc_2
  ; OPT   #locals_push_value_8 4 ; Ldloc_2
  ; OPT   #branch_true Player_Die_16 ; Brtrue_s
    #branch_if_var_less 3, 100, Player_Die_16 ; Nop
  ; OPT   #locals_push_value_8 2 ; Ldloc_0
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #inc_var 2 ; Nop
Player_Die_34:  ; OPT   #locals_push_value_8 2 ; Ldloc_0
  ; OPT   #stack_push_int 100 ; Ldc_i4_s
  ; OPT   #compareLess ; Clt
  ; OPT   #locals_pull_value_8 5, 0 ; Stloc_3
  ; OPT   #locals_push_value_8 5 ; Ldloc_3
  ; OPT   #branch_true Player_Die_12 ; Brtrue_s
    #branch_if_var_less 2, 100, Player_Die_12 ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_SetBorderColor ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 200 ; Ldc_i4
    jsr Player_set_Y ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 20 ; Ldc_i4_s
    jsr Player_set_X ; Call
    nop ; Nop
    #method_exit 7, [] ; Ret
