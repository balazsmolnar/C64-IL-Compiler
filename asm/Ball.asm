

;----------------------------------------
; TYPE: Ball
; METHOD: get_X
;----------------------------------------
Ball_get_X 
    #stack_save_return_adress Ball_get_X_ReturnAddress
    #stack_pull_int Ball_get_X_this
    nop ; Nop
  ; OPT   #stack_push_var Ball_get_X_this ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
  ; OPT   #stack_pull_int_ref Ball_get_X_var0, 0 ; Stloc_0
  ; OPT   jmp label_Ball_get_X_10 ; Br_s
label_Ball_get_X_10:  ; OPT   #stack_push_var Ball_get_X_var0 ; Ldloc_0
    #pushfld Ball_get_X_this, 0 ; Nop
    #stack_return_to_saved_address Ball_get_X_ReturnAddress ; Ret
Ball_get_X_this .byte 0, 0
Ball_get_X_ReturnAddress .byte 0,0
Ball_get_X_var0 .byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: set_X
;----------------------------------------
Ball_set_X 
    #stack_save_return_adress Ball_set_X_ReturnAddress
    #stack_pull_int_ref Ball_set_X_value, 0
    #stack_pull_int Ball_set_X_this
    nop ; Nop
    #stack_push_var Ball_set_X_this ; Ldarg_0
    #stack_push_var Ball_set_X_value ; Ldarg_1
    #stfld 0 ; Stfld
  ; OPT   #stack_push_var Ball_set_X_this ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld Ball_set_X_this, 4 ; Nop
    #stack_push_var Ball_set_X_value ; Ldarg_1
    jsr Sprite_set_X ; Callvirt
    nop ; Nop
    #stack_return_to_saved_address Ball_set_X_ReturnAddress ; Ret
Ball_set_X_this .byte 0, 0
Ball_set_X_value .byte 0, 0
Ball_set_X_ReturnAddress .byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: get_Y
;----------------------------------------
Ball_get_Y 
    #stack_save_return_adress Ball_get_Y_ReturnAddress
    #stack_pull_int Ball_get_Y_this
    nop ; Nop
  ; OPT   #stack_push_var Ball_get_Y_this ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
  ; OPT   #stack_pull_int_ref Ball_get_Y_var0, 0 ; Stloc_0
  ; OPT   jmp label_Ball_get_Y_10 ; Br_s
label_Ball_get_Y_10:  ; OPT   #stack_push_var Ball_get_Y_var0 ; Ldloc_0
    #pushfld Ball_get_Y_this, 1 ; Nop
    #stack_return_to_saved_address Ball_get_Y_ReturnAddress ; Ret
Ball_get_Y_this .byte 0, 0
Ball_get_Y_ReturnAddress .byte 0,0
Ball_get_Y_var0 .byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: set_Y
;----------------------------------------
Ball_set_Y 
    #stack_save_return_adress Ball_set_Y_ReturnAddress
    #stack_pull_int_ref Ball_set_Y_value, 0
    #stack_pull_int Ball_set_Y_this
    nop ; Nop
    #stack_push_var Ball_set_Y_this ; Ldarg_0
    #stack_push_var Ball_set_Y_value ; Ldarg_1
    #stfld 1 ; Stfld
  ; OPT   #stack_push_var Ball_set_Y_this ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld Ball_set_Y_this, 4 ; Nop
    #stack_push_var Ball_set_Y_value ; Ldarg_1
    jsr Sprite_set_Y ; Callvirt
    nop ; Nop
    #stack_return_to_saved_address Ball_set_Y_ReturnAddress ; Ret
Ball_set_Y_this .byte 0, 0
Ball_set_Y_value .byte 0, 0
Ball_set_Y_ReturnAddress .byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: get_VX
;----------------------------------------
Ball_get_VX 
    #stack_save_return_adress Ball_get_VX_ReturnAddress
    #stack_pull_int Ball_get_VX_this
  ; OPT   #stack_push_var Ball_get_VX_this ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld Ball_get_VX_this, 2 ; Nop
    #stack_return_to_saved_address Ball_get_VX_ReturnAddress ; Ret
Ball_get_VX_this .byte 0, 0
Ball_get_VX_ReturnAddress .byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: set_VX
;----------------------------------------
Ball_set_VX 
    #stack_save_return_adress Ball_set_VX_ReturnAddress
    #stack_pull_int_ref Ball_set_VX_value, 0
    #stack_pull_int Ball_set_VX_this
    #stack_push_var Ball_set_VX_this ; Ldarg_0
    #stack_push_var Ball_set_VX_value ; Ldarg_1
    #stfld 2 ; Stfld
    #stack_return_to_saved_address Ball_set_VX_ReturnAddress ; Ret
Ball_set_VX_this .byte 0, 0
Ball_set_VX_value .byte 0, 0
Ball_set_VX_ReturnAddress .byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: get_VY
;----------------------------------------
Ball_get_VY 
    #stack_save_return_adress Ball_get_VY_ReturnAddress
    #stack_pull_int Ball_get_VY_this
  ; OPT   #stack_push_var Ball_get_VY_this ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld Ball_get_VY_this, 3 ; Nop
    #stack_return_to_saved_address Ball_get_VY_ReturnAddress ; Ret
Ball_get_VY_this .byte 0, 0
Ball_get_VY_ReturnAddress .byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: set_VY
;----------------------------------------
Ball_set_VY 
    #stack_save_return_adress Ball_set_VY_ReturnAddress
    #stack_pull_int_ref Ball_set_VY_value, 0
    #stack_pull_int Ball_set_VY_this
    #stack_push_var Ball_set_VY_this ; Ldarg_0
    #stack_push_var Ball_set_VY_value ; Ldarg_1
    #stfld 3 ; Stfld
    #stack_return_to_saved_address Ball_set_VY_ReturnAddress ; Ret
Ball_set_VY_this .byte 0, 0
Ball_set_VY_value .byte 0, 0
Ball_set_VY_ReturnAddress .byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: set_Sprite
;----------------------------------------
Ball_set_Sprite 
    #stack_save_return_adress Ball_set_Sprite_ReturnAddress
    #stack_pull_int_ref Ball_set_Sprite_value, 0
    #stack_pull_int Ball_set_Sprite_this
    nop ; Nop
    #stack_push_var Ball_set_Sprite_this ; Ldarg_0
    #stack_push_var Ball_set_Sprite_value ; Ldarg_1
    #stfld 4 ; Stfld
  ; OPT   #stack_push_var Ball_set_Sprite_this ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld Ball_set_Sprite_this, 4 ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    #stack_push_var Ball_set_Sprite_this ; Ldarg_0
    #stack_push_int 20 ; Ldc_i4_s
    jsr Ball_set_X ; Call
    nop ; Nop
    #stack_push_var Ball_set_Sprite_this ; Ldarg_0
    #stack_push_int 45 ; Ldc_i4_s
    jsr Ball_set_Y ; Call
    nop ; Nop
  ; OPT   #stack_push_var Ball_set_Sprite_this ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld Ball_set_Sprite_this, 4 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_Visible ; Callvirt
    nop ; Nop
    #stack_return_to_saved_address Ball_set_Sprite_ReturnAddress ; Ret
Ball_set_Sprite_this .byte 0, 0
Ball_set_Sprite_value .byte 0, 0
Ball_set_Sprite_ReturnAddress .byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: set_BallColor
;----------------------------------------
Ball_set_BallColor 
    #stack_save_return_adress Ball_set_BallColor_ReturnAddress
    #stack_pull_int_ref Ball_set_BallColor_value, 0
    #stack_pull_int Ball_set_BallColor_this
    nop ; Nop
  ; OPT   #stack_push_var Ball_set_BallColor_this ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld Ball_set_BallColor_this, 4 ; Nop
    #stack_push_var Ball_set_BallColor_value ; Ldarg_1
    jsr Sprite_set_Color ; Callvirt
    nop ; Nop
    #stack_return_to_saved_address Ball_set_BallColor_ReturnAddress ; Ret
Ball_set_BallColor_this .byte 0, 0
Ball_set_BallColor_value .byte 0, 0
Ball_set_BallColor_ReturnAddress .byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: Move
;----------------------------------------
Ball_Move 
    #stack_save_return_adress Ball_Move_ReturnAddress
    #stack_pull_int Ball_Move_this
    nop ; Nop
    #stack_push_var Ball_Move_this ; Ldarg_0
    #stack_push_var Ball_Move_this ; Ldarg_0
    jsr Ball_get_VX ; Call
    nop ;Conv_i8 ; Conv_i8
    #stack_push_var Ball_Move_this ; Ldarg_0
    jsr Ball_get_X ; Call
    nop ;Conv_u8 ; Conv_u8
    #add ; Add
    nop ;Conv_u4 ; Conv_u4
    jsr Ball_set_X ; Call
    nop ; Nop
    #stack_push_var Ball_Move_this ; Ldarg_0
    #stack_push_var Ball_Move_this ; Ldarg_0
    jsr Ball_get_VY ; Call
    nop ;Conv_i8 ; Conv_i8
    #stack_push_var Ball_Move_this ; Ldarg_0
    jsr Ball_get_Y ; Call
    nop ;Conv_u8 ; Conv_u8
    #add ; Add
    nop ;Conv_u4 ; Conv_u4
    jsr Ball_set_Y ; Call
    nop ; Nop
    #stack_push_var Ball_Move_this ; Ldarg_0
    jsr Ball_get_X ; Call
    #stack_push_int 250 ; Ldc_i4
    #compareGreater ; Cgt_un
    #stack_pull_int_ref Ball_Move_var0, 0 ; Stloc_0
    #stack_push_var Ball_Move_var0 ; Ldloc_0
    #branch_false label_Ball_Move_78 ; Brfalse_s
    #stack_push_var Ball_Move_this ; Ldarg_0
    #stack_push_var Ball_Move_this ; Ldarg_0
    jsr Ball_get_VX ; Call
    #negate ; Neg
    jsr Ball_set_VX ; Call
    nop ; Nop
label_Ball_Move_78:    #stack_push_var Ball_Move_this ; Ldarg_0
    jsr Ball_get_X ; Call
    #stack_push_int 20 ; Ldc_i4_s
    #compareLess ; Clt_un
    #stack_pull_int_ref Ball_Move_var1, 0 ; Stloc_1
    #stack_push_var Ball_Move_var1 ; Ldloc_1
    #branch_false label_Ball_Move_106 ; Brfalse_s
    #stack_push_var Ball_Move_this ; Ldarg_0
    #stack_push_var Ball_Move_this ; Ldarg_0
    jsr Ball_get_VX ; Call
    #negate ; Neg
    jsr Ball_set_VX ; Call
    nop ; Nop
label_Ball_Move_106:    #stack_push_var Ball_Move_this ; Ldarg_0
    jsr Ball_get_Y ; Call
    #stack_push_int 228 ; Ldc_i4
    #compareGreater ; Cgt_un
    #stack_pull_int_ref Ball_Move_var2, 0 ; Stloc_2
    #stack_push_var Ball_Move_var2 ; Ldloc_2
    #branch_false label_Ball_Move_137 ; Brfalse_s
    #stack_push_var Ball_Move_this ; Ldarg_0
    #stack_push_var Ball_Move_this ; Ldarg_0
    jsr Ball_get_VY ; Call
    #negate ; Neg
    jsr Ball_set_VY ; Call
    nop ; Nop
label_Ball_Move_137:    #stack_push_var Ball_Move_this ; Ldarg_0
    jsr Ball_get_Y ; Call
    #stack_push_int 45 ; Ldc_i4_s
    #compareLess ; Clt_un
    #stack_pull_int_ref Ball_Move_var3, 0 ; Stloc_3
    #stack_push_var Ball_Move_var3 ; Ldloc_3
    #branch_false label_Ball_Move_165 ; Brfalse_s
    #stack_push_var Ball_Move_this ; Ldarg_0
    #stack_push_var Ball_Move_this ; Ldarg_0
    jsr Ball_get_VY ; Call
    #negate ; Neg
    jsr Ball_set_VY ; Call
    nop ; Nop
label_Ball_Move_165:    #stack_return_to_saved_address Ball_Move_ReturnAddress ; Ret
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
    #stack_save_return_adress Ball_RunBalls_ReturnAddress
    nop ; Nop
    #newObj 5, 0 ; Newobj
    #stack_pull_int_ref Ball_RunBalls_var0, 1 ; Stloc_0
    #stack_push_var Ball_RunBalls_var0 ; Ldloc_0
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite0 ; Callvirt
    jsr Ball_set_Sprite ; Callvirt
    nop ; Nop
    #stack_push_var Ball_RunBalls_var0 ; Ldloc_0
    #stack_push_int 1 ; Ldc_i4_1
    jsr Ball_set_VX ; Callvirt
    nop ; Nop
    #stack_push_var Ball_RunBalls_var0 ; Ldloc_0
    #stack_push_int 1 ; Ldc_i4_1
    jsr Ball_set_VY ; Callvirt
    nop ; Nop
    #stack_push_var Ball_RunBalls_var0 ; Ldloc_0
    #stack_push_int 70 ; Ldc_i4_s
    jsr Ball_set_Y ; Callvirt
    nop ; Nop
    #stack_push_var Ball_RunBalls_var0 ; Ldloc_0
    #stack_push_int 2 ; Ldc_i4_2
    jsr Ball_set_BallColor ; Callvirt
    nop ; Nop
    #newObj 5, 0 ; Newobj
    #stack_pull_int_ref Ball_RunBalls_var1, 1 ; Stloc_1
    #stack_push_var Ball_RunBalls_var1 ; Ldloc_1
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite1 ; Callvirt
    jsr Ball_set_Sprite ; Callvirt
    nop ; Nop
    #stack_push_var Ball_RunBalls_var1 ; Ldloc_1
    #stack_push_int 1 ; Ldc_i4_1
    jsr Ball_set_VX ; Callvirt
    nop ; Nop
    #stack_push_var Ball_RunBalls_var1 ; Ldloc_1
    #stack_push_int 2 ; Ldc_i4_2
    jsr Ball_set_VY ; Callvirt
    nop ; Nop
    #stack_push_var Ball_RunBalls_var1 ; Ldloc_1
    #stack_push_int 100 ; Ldc_i4_s
    jsr Ball_set_X ; Callvirt
    nop ; Nop
    #stack_push_var Ball_RunBalls_var1 ; Ldloc_1
    #stack_push_int 7 ; Ldc_i4_7
    jsr Ball_set_BallColor ; Callvirt
    nop ; Nop
    #newObj 5, 0 ; Newobj
    #stack_pull_int_ref Ball_RunBalls_var2, 1 ; Stloc_2
    #stack_push_var Ball_RunBalls_var2 ; Ldloc_2
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite2 ; Callvirt
    jsr Ball_set_Sprite ; Callvirt
    nop ; Nop
    #stack_push_var Ball_RunBalls_var2 ; Ldloc_2
    #stack_push_int 2 ; Ldc_i4_2
    jsr Ball_set_VX ; Callvirt
    nop ; Nop
    #stack_push_var Ball_RunBalls_var2 ; Ldloc_2
    #stack_push_int 1 ; Ldc_i4_1
    jsr Ball_set_VY ; Callvirt
    nop ; Nop
    #stack_push_var Ball_RunBalls_var2 ; Ldloc_2
    #stack_push_int 120 ; Ldc_i4_s
    jsr Ball_set_X ; Callvirt
    nop ; Nop
    #stack_push_var Ball_RunBalls_var2 ; Ldloc_2
    #stack_push_int 4 ; Ldc_i4_4
    jsr Ball_set_BallColor ; Callvirt
    nop ; Nop
    jmp label_Ball_RunBalls_217 ; Br_s
label_Ball_RunBalls_171:    nop ; Nop
    #stack_push_var Ball_RunBalls_var0 ; Ldloc_0
    jsr Ball_Move ; Callvirt
    nop ; Nop
    #stack_push_var Ball_RunBalls_var1 ; Ldloc_1
    jsr Ball_Move ; Callvirt
    nop ; Nop
    #stack_push_var Ball_RunBalls_var2 ; Ldloc_2
    jsr Ball_Move ; Callvirt
    nop ; Nop
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #stack_pull_int_ref Ball_RunBalls_var3, 0 ; Stloc_3
    #init_var Ball_RunBalls_var3, 0 ; Nop
    jmp label_Ball_RunBalls_202 ; Br_s
label_Ball_RunBalls_197:    nop ; Nop
  ; OPT   #stack_push_var Ball_RunBalls_var3 ; Ldloc_3
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #stack_pull_int_ref Ball_RunBalls_var3, 0 ; Stloc_3
    #inc_var Ball_RunBalls_var3 ; Nop
label_Ball_RunBalls_202:    #stack_push_var Ball_RunBalls_var3 ; Ldloc_3
    #stack_push_int 255 ; Ldc_i4
    #compareLess ; Clt_un
    #stack_pull_int_ref Ball_RunBalls_var4, 0 ; Stloc_s
    #stack_push_var Ball_RunBalls_var4 ; Ldloc_s
    #branch_true label_Ball_RunBalls_197 ; Brtrue_s
    nop ; Nop
label_Ball_RunBalls_217:    jmp label_Ball_RunBalls_171 ; Br_s
Ball_RunBalls_ReturnAddress .byte 0,0
Ball_RunBalls_var0 .byte 0,0
Ball_RunBalls_var1 .byte 0,0
Ball_RunBalls_var2 .byte 0,0
Ball_RunBalls_var3 .byte 0,0
Ball_RunBalls_var4 .byte 0,0
