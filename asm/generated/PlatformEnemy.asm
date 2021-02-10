

;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: set_Platform
;----------------------------------------
PlatformEnemy_set_Platform 
    #locals_init_locals 0
    #locals_pull_param_8 1
    #locals_pull_param_8 0
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 100 ; Ldc_i4_s
    jsr PlatformEnemy_set_MinX ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 200 ; Ldc_i4
    jsr PlatformEnemy_set_MaxX ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 2 ; Ldarg_1
    #ldfld 2 ; Ldfld
    #stack_push_int 2 ; Ldc_i4_2
    #sub ; Sub
    #stfld 1 ; Stfld
    #locals_method_exit 4 ; Ret


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: set_Sprite
;----------------------------------------
PlatformEnemy_set_Sprite 
    #locals_init_locals 0
    #locals_pull_param_8 0
    #locals_pull_param_8 0
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 2 ; Ldarg_1
    #stfld 8 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 8 ; Ldfld
    #pushfld 8 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 7 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr PlatformEnemy_get_MinX ; Call
    jsr PlatformEnemy_set_X ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr PlatformEnemy_get_Y ; Call
    jsr PlatformEnemy_set_Y ; Call
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 8 ; Ldfld
    #pushfld 8 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_Visible ; Callvirt
    nop ; Nop
    #locals_method_exit 4 ; Ret


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: set_Color
;----------------------------------------
PlatformEnemy_set_Color 
    #locals_init_locals 0
    #locals_pull_param_8 0
    #locals_pull_param_8 0
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 8 ; Ldfld
    #pushfld 8 ; Nop
    #locals_push_value_8 2 ; Ldarg_1
    jsr Sprite_set_Color ; Callvirt
    nop ; Nop
    #locals_method_exit 4 ; Ret


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: Move
;----------------------------------------
PlatformEnemy_Move 
    #locals_init_locals 3
    #locals_pull_param_8 0
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 7 ; Ldfld
    #pushfld 7 ; Nop
    #locals_pull_value_8 2, 0 ; Stloc_0
    #locals_push_value_8 2 ; Ldloc_0
    #branch_false label_PlatformEnemy_Move_60 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr PlatformEnemy_get_X ; Call
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #add ; Add
    jsr PlatformEnemy_set_X ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    jsr PlatformEnemy_get_X ; Call
    #locals_push_value_8 1 ; Ldarg_0
    jsr PlatformEnemy_get_MaxX ; Call
    #compareGreater ; Cgt_un
    #locals_pull_value_8 3, 0 ; Stloc_1
    #locals_push_value_8 3 ; Ldloc_1
    #branch_false label_PlatformEnemy_Move_57 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 7 ; Stfld
label_PlatformEnemy_Move_57:    nop ; Nop
    jmp label_PlatformEnemy_Move_107 ; Br_s
label_PlatformEnemy_Move_60:    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr PlatformEnemy_get_X ; Call
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld 4 ; Nop
    #sub ; Sub
    jsr PlatformEnemy_set_X ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    jsr PlatformEnemy_get_X ; Call
    #locals_push_value_8 1 ; Ldarg_0
    jsr PlatformEnemy_get_MinX ; Call
    #compareLess ; Clt_un
    #locals_pull_value_8 4, 0 ; Stloc_2
    #locals_push_value_8 4 ; Ldloc_2
    #branch_false label_PlatformEnemy_Move_106 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 7 ; Stfld
label_PlatformEnemy_Move_106:    nop ; Nop
label_PlatformEnemy_Move_107:    #locals_method_exit 6 ; Ret


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: get_X
;----------------------------------------
PlatformEnemy_get_X 
    #locals_init_locals 1
    #locals_pull_param_8 0
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
  ; OPT   jmp label_PlatformEnemy_get_X_10 ; Br_s
label_PlatformEnemy_get_X_10:  ; OPT   #locals_push_value_8 2 ; Ldloc_0
    #pushfld 0 ; Nop
    #locals_method_exit 4 ; Ret


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: set_X
;----------------------------------------
PlatformEnemy_set_X 
    #locals_init_locals 0
    #locals_pull_param_8 0
    #locals_pull_param_8 0
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 2 ; Ldarg_1
    #stfld 0 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 8 ; Ldfld
    #pushfld 8 ; Nop
    #locals_push_value_8 2 ; Ldarg_1
    jsr Sprite_set_X ; Callvirt
    nop ; Nop
    #locals_method_exit 4 ; Ret


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: get_Y
;----------------------------------------
PlatformEnemy_get_Y 
    #locals_init_locals 1
    #locals_pull_param_8 0
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
  ; OPT   jmp label_PlatformEnemy_get_Y_10 ; Br_s
label_PlatformEnemy_get_Y_10:  ; OPT   #locals_push_value_8 2 ; Ldloc_0
    #pushfld 1 ; Nop
    #locals_method_exit 4 ; Ret


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: set_Y
;----------------------------------------
PlatformEnemy_set_Y 
    #locals_init_locals 0
    #locals_pull_param_8 0
    #locals_pull_param_8 0
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 2 ; Ldarg_1
    #stfld 1 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 8 ; Ldfld
    #pushfld 8 ; Nop
    #locals_push_value_8 2 ; Ldarg_1
    jsr Sprite_set_Y ; Callvirt
    nop ; Nop
    #locals_method_exit 4 ; Ret


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: get_MinX
;----------------------------------------
PlatformEnemy_get_MinX 
    #locals_init_locals 0
    #locals_pull_param_8 0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #locals_method_exit 3 ; Ret


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: set_MinX
;----------------------------------------
PlatformEnemy_set_MinX 
    #locals_init_locals 0
    #locals_pull_param_8 0
    #locals_pull_param_8 0
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 2 ; Ldarg_1
    #stfld 5 ; Stfld
    #locals_method_exit 4 ; Ret


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: get_MaxX
;----------------------------------------
PlatformEnemy_get_MaxX 
    #locals_init_locals 0
    #locals_pull_param_8 0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 6 ; Ldfld
    #pushfld 6 ; Nop
    #locals_method_exit 3 ; Ret


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: set_MaxX
;----------------------------------------
PlatformEnemy_set_MaxX 
    #locals_init_locals 0
    #locals_pull_param_8 0
    #locals_pull_param_8 0
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 2 ; Ldarg_1
    #stfld 6 ; Stfld
    #locals_method_exit 4 ; Ret
