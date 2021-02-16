

;----------------------------------------
; TYPE: Demo.Ball
; METHOD: get_X
;----------------------------------------
Ball_get_X 
    #init_locals_pull_parameters 1, [0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
  ; OPT   jmp Ball_get_X_10 ; Br_s
Ball_get_X_10:  ; OPT   #locals_push_value_8 2 ; Ldloc_0
    #pushfld 0 ; Nop
    #method_exit 4 ; Ret


;----------------------------------------
; TYPE: Demo.Ball
; METHOD: set_X
;----------------------------------------
Ball_set_X 
    #init_locals_pull_parameters 0, [0,0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stfld 0 ; Stfld
    #setfld 1, 2, 0 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld 4 ; Nop
    #locals_push_value_8 2 ; Ldarg_1
    jsr Sprite_set_X ; Callvirt
    nop ; Nop
    #method_exit 4 ; Ret


;----------------------------------------
; TYPE: Demo.Ball
; METHOD: get_Y
;----------------------------------------
Ball_get_Y 
    #init_locals_pull_parameters 1, [0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
  ; OPT   jmp Ball_get_Y_10 ; Br_s
Ball_get_Y_10:  ; OPT   #locals_push_value_8 2 ; Ldloc_0
    #pushfld 1 ; Nop
    #method_exit 4 ; Ret


;----------------------------------------
; TYPE: Demo.Ball
; METHOD: set_Y
;----------------------------------------
Ball_set_Y 
    #init_locals_pull_parameters 0, [0,0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stfld 1 ; Stfld
    #setfld 1, 2, 1 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld 4 ; Nop
    #locals_push_value_8 2 ; Ldarg_1
    jsr Sprite_set_Y ; Callvirt
    nop ; Nop
    #method_exit 4 ; Ret


;----------------------------------------
; TYPE: Demo.Ball
; METHOD: get_VX
;----------------------------------------
Ball_get_VX 
    #init_locals_pull_parameters 0, [0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #method_exit 3 ; Ret


;----------------------------------------
; TYPE: Demo.Ball
; METHOD: set_VX
;----------------------------------------
Ball_set_VX 
    #init_locals_pull_parameters 0, [0,0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stfld 2 ; Stfld
    #setfld 1, 2, 2 ; Nop
    #method_exit 4 ; Ret


;----------------------------------------
; TYPE: Demo.Ball
; METHOD: get_VY
;----------------------------------------
Ball_get_VY 
    #init_locals_pull_parameters 0, [0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #method_exit 3 ; Ret


;----------------------------------------
; TYPE: Demo.Ball
; METHOD: set_VY
;----------------------------------------
Ball_set_VY 
    #init_locals_pull_parameters 0, [0,0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stfld 3 ; Stfld
    #setfld 1, 2, 3 ; Nop
    #method_exit 4 ; Ret


;----------------------------------------
; TYPE: Demo.Ball
; METHOD: set_Sprite
;----------------------------------------
Ball_set_Sprite 
    #init_locals_pull_parameters 0, [0,0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stfld 4 ; Stfld
    #setfld 1, 2, 4 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld 4 ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 20 ; Ldc_i4_s
    jsr Ball_set_X ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 45 ; Ldc_i4_s
    jsr Ball_set_Y ; Call
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld 4 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_Visible ; Callvirt
    nop ; Nop
    #method_exit 4 ; Ret


;----------------------------------------
; TYPE: Demo.Ball
; METHOD: set_BallColor
;----------------------------------------
Ball_set_BallColor 
    #init_locals_pull_parameters 0, [0,0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld 4 ; Nop
    #locals_push_value_8 2 ; Ldarg_1
    jsr Sprite_set_Color ; Callvirt
    nop ; Nop
    #method_exit 4 ; Ret


;----------------------------------------
; TYPE: Demo.Ball
; METHOD: Move
;----------------------------------------
Ball_Move 
    #init_locals_pull_parameters 4, [0]
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr Ball_get_VX ; Call
    nop ;Conv_i8 ; Conv_i8
    #locals_push_value_8 1 ; Ldarg_0
    jsr Ball_get_X ; Call
    nop ;Conv_u8 ; Conv_u8
    #add ; Add
    nop ;Conv_u4 ; Conv_u4
    jsr Ball_set_X ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr Ball_get_VY ; Call
    nop ;Conv_i8 ; Conv_i8
    #locals_push_value_8 1 ; Ldarg_0
    jsr Ball_get_Y ; Call
    nop ;Conv_u8 ; Conv_u8
    #add ; Add
    nop ;Conv_u4 ; Conv_u4
    jsr Ball_set_Y ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    jsr Ball_get_X ; Call
    #stack_push_int 250 ; Ldc_i4
    #compareGreater ; Cgt_un
    #locals_pull_value_8 2, 0 ; Stloc_0
    #locals_push_value_8 2 ; Ldloc_0
    #branch_false Ball_Move_78 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr Ball_get_VX ; Call
    #negate ; Neg
    jsr Ball_set_VX ; Call
    nop ; Nop
Ball_Move_78:    #locals_push_value_8 1 ; Ldarg_0
    jsr Ball_get_X ; Call
    #stack_push_int 20 ; Ldc_i4_s
    #compareLess ; Clt_un
    #locals_pull_value_8 3, 0 ; Stloc_1
    #locals_push_value_8 3 ; Ldloc_1
    #branch_false Ball_Move_106 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr Ball_get_VX ; Call
    #negate ; Neg
    jsr Ball_set_VX ; Call
    nop ; Nop
Ball_Move_106:    #locals_push_value_8 1 ; Ldarg_0
    jsr Ball_get_Y ; Call
    #stack_push_int 228 ; Ldc_i4
    #compareGreater ; Cgt_un
    #locals_pull_value_8 4, 0 ; Stloc_2
    #locals_push_value_8 4 ; Ldloc_2
    #branch_false Ball_Move_137 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr Ball_get_VY ; Call
    #negate ; Neg
    jsr Ball_set_VY ; Call
    nop ; Nop
Ball_Move_137:    #locals_push_value_8 1 ; Ldarg_0
    jsr Ball_get_Y ; Call
    #stack_push_int 45 ; Ldc_i4_s
    #compareLess ; Clt_un
    #locals_pull_value_8 5, 0 ; Stloc_3
    #locals_push_value_8 5 ; Ldloc_3
    #branch_false Ball_Move_165 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr Ball_get_VY ; Call
    #negate ; Neg
    jsr Ball_set_VY ; Call
    nop ; Nop
Ball_Move_165:    #method_exit 7 ; Ret


;----------------------------------------
; TYPE: Demo.Ball
; METHOD: RunBalls
;----------------------------------------
Ball_RunBalls 
    #init_locals_pull_parameters 5, []
    nop ; Nop
    #newObj 5, 0 ; Newobj
    #locals_pull_value_8 1, 1 ; Stloc_0
    #locals_push_value_8 1 ; Ldloc_0
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite0 ; Callvirt
    jsr Ball_set_Sprite ; Callvirt
    nop ; Nop
    #locals_push_value_8 1 ; Ldloc_0
    #stack_push_int 1 ; Ldc_i4_1
    jsr Ball_set_VX ; Callvirt
    nop ; Nop
    #locals_push_value_8 1 ; Ldloc_0
    #stack_push_int 1 ; Ldc_i4_1
    jsr Ball_set_VY ; Callvirt
    nop ; Nop
    #locals_push_value_8 1 ; Ldloc_0
    #stack_push_int 70 ; Ldc_i4_s
    jsr Ball_set_Y ; Callvirt
    nop ; Nop
    #locals_push_value_8 1 ; Ldloc_0
    #stack_push_int 2 ; Ldc_i4_2
    jsr Ball_set_BallColor ; Callvirt
    nop ; Nop
    #newObj 5, 0 ; Newobj
    #locals_pull_value_8 2, 1 ; Stloc_1
    #locals_push_value_8 2 ; Ldloc_1
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite1 ; Callvirt
    jsr Ball_set_Sprite ; Callvirt
    nop ; Nop
    #locals_push_value_8 2 ; Ldloc_1
    #stack_push_int 1 ; Ldc_i4_1
    jsr Ball_set_VX ; Callvirt
    nop ; Nop
    #locals_push_value_8 2 ; Ldloc_1
    #stack_push_int 2 ; Ldc_i4_2
    jsr Ball_set_VY ; Callvirt
    nop ; Nop
    #locals_push_value_8 2 ; Ldloc_1
    #stack_push_int 100 ; Ldc_i4_s
    jsr Ball_set_X ; Callvirt
    nop ; Nop
    #locals_push_value_8 2 ; Ldloc_1
    #stack_push_int 7 ; Ldc_i4_7
    jsr Ball_set_BallColor ; Callvirt
    nop ; Nop
    #newObj 5, 0 ; Newobj
    #locals_pull_value_8 3, 1 ; Stloc_2
    #locals_push_value_8 3 ; Ldloc_2
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite2 ; Callvirt
    jsr Ball_set_Sprite ; Callvirt
    nop ; Nop
    #locals_push_value_8 3 ; Ldloc_2
    #stack_push_int 2 ; Ldc_i4_2
    jsr Ball_set_VX ; Callvirt
    nop ; Nop
    #locals_push_value_8 3 ; Ldloc_2
    #stack_push_int 1 ; Ldc_i4_1
    jsr Ball_set_VY ; Callvirt
    nop ; Nop
    #locals_push_value_8 3 ; Ldloc_2
    #stack_push_int 120 ; Ldc_i4_s
    jsr Ball_set_X ; Callvirt
    nop ; Nop
    #locals_push_value_8 3 ; Ldloc_2
    #stack_push_int 4 ; Ldc_i4_4
    jsr Ball_set_BallColor ; Callvirt
    nop ; Nop
    jmp Ball_RunBalls_217 ; Br_s
Ball_RunBalls_171:    nop ; Nop
    #locals_push_value_8 1 ; Ldloc_0
    jsr Ball_Move ; Callvirt
    nop ; Nop
    #locals_push_value_8 2 ; Ldloc_1
    jsr Ball_Move ; Callvirt
    nop ; Nop
    #locals_push_value_8 3 ; Ldloc_2
    jsr Ball_Move ; Callvirt
    nop ; Nop
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 4, 0 ; Stloc_3
    #init_var 4, 0 ; Nop
    jmp Ball_RunBalls_202 ; Br_s
Ball_RunBalls_197:    nop ; Nop
  ; OPT   #locals_push_value_8 4 ; Ldloc_3
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 4, 0 ; Stloc_3
    #inc_var 4 ; Nop
Ball_RunBalls_202:    #locals_push_value_8 4 ; Ldloc_3
    #stack_push_int 255 ; Ldc_i4
    #compareLess ; Clt_un
    #locals_pull_value_8 5, 0 ; Stloc_s
    #locals_push_value_8 5 ; Ldloc_s
    #branch_true Ball_RunBalls_197 ; Brtrue_s
    nop ; Nop
Ball_RunBalls_217:    jmp Ball_RunBalls_171 ; Br_s
