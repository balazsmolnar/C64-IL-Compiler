

;----------------------------------------
; TYPE: Ball
; METHOD: get_X
;----------------------------------------
Ball_get_X 
    #locals_init_locals 1
    #locals_pull_param_8 0
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 0 ; Ldfld
    #locals_pull_value_8 2, 0 ; Stloc_0
    jmp label_Ball_get_X_10 ; Br_s
label_Ball_get_X_10:    #locals_push_value_8 2 ; Ldloc_0
    #locals_method_exit 4 ; Ret
Ball_get_X_this .byte 0, 0
Ball_get_X_ReturnAddress .byte 0,0
Ball_get_X_var0 .byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: set_X
;----------------------------------------
Ball_set_X 
    #locals_init_locals 0
    #locals_pull_param_8 0
    #locals_pull_param_8 0
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 2 ; Ldarg_1
    #stfld 0 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 4 ; Ldfld
    #locals_push_value_8 2 ; Ldarg_1
    jsr Sprite_set_X ; Callvirt
    nop ; Nop
    #locals_method_exit 4 ; Ret
Ball_set_X_this .byte 0, 0
Ball_set_X_value .byte 0, 0
Ball_set_X_ReturnAddress .byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: get_Y
;----------------------------------------
Ball_get_Y 
    #locals_init_locals 1
    #locals_pull_param_8 0
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 1 ; Ldfld
    #locals_pull_value_8 2, 0 ; Stloc_0
    jmp label_Ball_get_Y_10 ; Br_s
label_Ball_get_Y_10:    #locals_push_value_8 2 ; Ldloc_0
    #locals_method_exit 4 ; Ret
Ball_get_Y_this .byte 0, 0
Ball_get_Y_ReturnAddress .byte 0,0
Ball_get_Y_var0 .byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: set_Y
;----------------------------------------
Ball_set_Y 
    #locals_init_locals 0
    #locals_pull_param_8 0
    #locals_pull_param_8 0
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 2 ; Ldarg_1
    #stfld 1 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 4 ; Ldfld
    #locals_push_value_8 2 ; Ldarg_1
    jsr Sprite_set_Y ; Callvirt
    nop ; Nop
    #locals_method_exit 4 ; Ret
Ball_set_Y_this .byte 0, 0
Ball_set_Y_value .byte 0, 0
Ball_set_Y_ReturnAddress .byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: get_VX
;----------------------------------------
Ball_get_VX 
    #locals_init_locals 0
    #locals_pull_param_8 0
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 2 ; Ldfld
    #locals_method_exit 3 ; Ret
Ball_get_VX_this .byte 0, 0
Ball_get_VX_ReturnAddress .byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: set_VX
;----------------------------------------
Ball_set_VX 
    #locals_init_locals 0
    #locals_pull_param_8 0
    #locals_pull_param_8 0
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 2 ; Ldarg_1
    #stfld 2 ; Stfld
    #locals_method_exit 4 ; Ret
Ball_set_VX_this .byte 0, 0
Ball_set_VX_value .byte 0, 0
Ball_set_VX_ReturnAddress .byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: get_VY
;----------------------------------------
Ball_get_VY 
    #locals_init_locals 0
    #locals_pull_param_8 0
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 3 ; Ldfld
    #locals_method_exit 3 ; Ret
Ball_get_VY_this .byte 0, 0
Ball_get_VY_ReturnAddress .byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: set_VY
;----------------------------------------
Ball_set_VY 
    #locals_init_locals 0
    #locals_pull_param_8 0
    #locals_pull_param_8 0
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 2 ; Ldarg_1
    #stfld 3 ; Stfld
    #locals_method_exit 4 ; Ret
Ball_set_VY_this .byte 0, 0
Ball_set_VY_value .byte 0, 0
Ball_set_VY_ReturnAddress .byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: set_Sprite
;----------------------------------------
Ball_set_Sprite 
    #locals_init_locals 0
    #locals_pull_param_8 0
    #locals_pull_param_8 0
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 2 ; Ldarg_1
    #stfld 4 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 4 ; Ldfld
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
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 4 ; Ldfld
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_Visible ; Callvirt
    nop ; Nop
    #locals_method_exit 4 ; Ret
Ball_set_Sprite_this .byte 0, 0
Ball_set_Sprite_value .byte 0, 0
Ball_set_Sprite_ReturnAddress .byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: set_BallColor
;----------------------------------------
Ball_set_BallColor 
    #locals_init_locals 0
    #locals_pull_param_8 0
    #locals_pull_param_8 0
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 4 ; Ldfld
    #locals_push_value_8 2 ; Ldarg_1
    jsr Sprite_set_Color ; Callvirt
    nop ; Nop
    #locals_method_exit 4 ; Ret
Ball_set_BallColor_this .byte 0, 0
Ball_set_BallColor_value .byte 0, 0
Ball_set_BallColor_ReturnAddress .byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: Move
;----------------------------------------
Ball_Move 
    #locals_init_locals 4
    #locals_pull_param_8 0
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
    #branch_false label_Ball_Move_78 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr Ball_get_VX ; Call
    #negate ; Neg
    jsr Ball_set_VX ; Call
    nop ; Nop
label_Ball_Move_78:    #locals_push_value_8 1 ; Ldarg_0
    jsr Ball_get_X ; Call
    #stack_push_int 20 ; Ldc_i4_s
    #compareLess ; Clt_un
    #locals_pull_value_8 3, 0 ; Stloc_1
    #locals_push_value_8 3 ; Ldloc_1
    #branch_false label_Ball_Move_106 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr Ball_get_VX ; Call
    #negate ; Neg
    jsr Ball_set_VX ; Call
    nop ; Nop
label_Ball_Move_106:    #locals_push_value_8 1 ; Ldarg_0
    jsr Ball_get_Y ; Call
    #stack_push_int 228 ; Ldc_i4
    #compareGreater ; Cgt_un
    #locals_pull_value_8 4, 0 ; Stloc_2
    #locals_push_value_8 4 ; Ldloc_2
    #branch_false label_Ball_Move_137 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr Ball_get_VY ; Call
    #negate ; Neg
    jsr Ball_set_VY ; Call
    nop ; Nop
label_Ball_Move_137:    #locals_push_value_8 1 ; Ldarg_0
    jsr Ball_get_Y ; Call
    #stack_push_int 45 ; Ldc_i4_s
    #compareLess ; Clt_un
    #locals_pull_value_8 5, 0 ; Stloc_3
    #locals_push_value_8 5 ; Ldloc_3
    #branch_false label_Ball_Move_165 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr Ball_get_VY ; Call
    #negate ; Neg
    jsr Ball_set_VY ; Call
    nop ; Nop
label_Ball_Move_165:    #locals_method_exit 7 ; Ret
Ball_Move_this .byte 0, 0
Ball_Move_ReturnAddress .byte 0,0
Ball_Move_var0 .byte 0,0
Ball_Move_var1 .byte 0,0
Ball_Move_var2 .byte 0,0
Ball_Move_var3 .byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: RunBalls
;----------------------------------------
Ball_RunBalls 
    #locals_init_locals 5
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
    jmp label_Ball_RunBalls_217 ; Br_s
label_Ball_RunBalls_171:    nop ; Nop
    #locals_push_value_8 1 ; Ldloc_0
    jsr Ball_Move ; Callvirt
    nop ; Nop
    #locals_push_value_8 2 ; Ldloc_1
    jsr Ball_Move ; Callvirt
    nop ; Nop
    #locals_push_value_8 3 ; Ldloc_2
    jsr Ball_Move ; Callvirt
    nop ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    #locals_pull_value_8 4, 0 ; Stloc_3
    jmp label_Ball_RunBalls_202 ; Br_s
label_Ball_RunBalls_197:    nop ; Nop
    #locals_push_value_8 4 ; Ldloc_3
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #locals_pull_value_8 4, 0 ; Stloc_3
label_Ball_RunBalls_202:    #locals_push_value_8 4 ; Ldloc_3
    #stack_push_int 255 ; Ldc_i4
    #compareLess ; Clt_un
    #locals_pull_value_8 5, 0 ; Stloc_s
    #locals_push_value_8 5 ; Ldloc_s
    #branch_true label_Ball_RunBalls_197 ; Brtrue_s
    nop ; Nop
label_Ball_RunBalls_217:    jmp label_Ball_RunBalls_171 ; Br_s
Ball_RunBalls_ReturnAddress .byte 0,0
Ball_RunBalls_var0 .byte 0,0
Ball_RunBalls_var1 .byte 0,0
Ball_RunBalls_var2 .byte 0,0
Ball_RunBalls_var3 .byte 0,0
Ball_RunBalls_var4 .byte 0,0
