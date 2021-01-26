

;----------------------------------------
; TYPE: Ball
; METHOD: get_X
;----------------------------------------
Ball_get_X 
    +stack_save_return_adress .Ball_get_X_ReturnAddress
    +stack_pull_int .Ball_get_X_this
    nop ; Nop
    +stack_push_var .Ball_get_X_this ; Ldarg_0
    +ldfld 0 ; Ldfld
    +stack_pull_int_ref .Ball_get_X_var0, 0 ; Stloc_0
    jmp label_Ball_get_X_10 ; Br_s
label_Ball_get_X_10:    +stack_push_var .Ball_get_X_var0 ; Ldloc_0
    +stack_return_to_saved_address .Ball_get_X_ReturnAddress ; Ret
.Ball_get_X_this !byte 0, 0
.Ball_get_X_ReturnAddress !byte 0,0
.Ball_get_X_var0 !byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: set_X
;----------------------------------------
Ball_set_X 
    +stack_save_return_adress .Ball_set_X_ReturnAddress
    +stack_pull_int_ref .Ball_set_X_value, 0
    +stack_pull_int .Ball_set_X_this
    nop ; Nop
    +stack_push_var .Ball_set_X_this ; Ldarg_0
    +stack_push_var .Ball_set_X_value ; Ldarg_1
    +stfld 0 ; Stfld
    +stack_push_var .Ball_set_X_this ; Ldarg_0
    +ldfld 4 ; Ldfld
    +stack_push_var .Ball_set_X_value ; Ldarg_1
    jsr Sprite_set_X ; Callvirt
    nop ; Nop
    +stack_return_to_saved_address .Ball_set_X_ReturnAddress ; Ret
.Ball_set_X_this !byte 0, 0
.Ball_set_X_value !byte 0, 0
.Ball_set_X_ReturnAddress !byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: get_Y
;----------------------------------------
Ball_get_Y 
    +stack_save_return_adress .Ball_get_Y_ReturnAddress
    +stack_pull_int .Ball_get_Y_this
    nop ; Nop
    +stack_push_var .Ball_get_Y_this ; Ldarg_0
    +ldfld 1 ; Ldfld
    +stack_pull_int_ref .Ball_get_Y_var0, 0 ; Stloc_0
    jmp label_Ball_get_Y_10 ; Br_s
label_Ball_get_Y_10:    +stack_push_var .Ball_get_Y_var0 ; Ldloc_0
    +stack_return_to_saved_address .Ball_get_Y_ReturnAddress ; Ret
.Ball_get_Y_this !byte 0, 0
.Ball_get_Y_ReturnAddress !byte 0,0
.Ball_get_Y_var0 !byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: set_Y
;----------------------------------------
Ball_set_Y 
    +stack_save_return_adress .Ball_set_Y_ReturnAddress
    +stack_pull_int_ref .Ball_set_Y_value, 0
    +stack_pull_int .Ball_set_Y_this
    nop ; Nop
    +stack_push_var .Ball_set_Y_this ; Ldarg_0
    +stack_push_var .Ball_set_Y_value ; Ldarg_1
    +stfld 1 ; Stfld
    +stack_push_var .Ball_set_Y_this ; Ldarg_0
    +ldfld 4 ; Ldfld
    +stack_push_var .Ball_set_Y_value ; Ldarg_1
    jsr Sprite_set_Y ; Callvirt
    nop ; Nop
    +stack_return_to_saved_address .Ball_set_Y_ReturnAddress ; Ret
.Ball_set_Y_this !byte 0, 0
.Ball_set_Y_value !byte 0, 0
.Ball_set_Y_ReturnAddress !byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: get_VX
;----------------------------------------
Ball_get_VX 
    +stack_save_return_adress .Ball_get_VX_ReturnAddress
    +stack_pull_int .Ball_get_VX_this
    +stack_push_var .Ball_get_VX_this ; Ldarg_0
    +ldfld 2 ; Ldfld
    +stack_return_to_saved_address .Ball_get_VX_ReturnAddress ; Ret
.Ball_get_VX_this !byte 0, 0
.Ball_get_VX_ReturnAddress !byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: set_VX
;----------------------------------------
Ball_set_VX 
    +stack_save_return_adress .Ball_set_VX_ReturnAddress
    +stack_pull_int_ref .Ball_set_VX_value, 0
    +stack_pull_int .Ball_set_VX_this
    +stack_push_var .Ball_set_VX_this ; Ldarg_0
    +stack_push_var .Ball_set_VX_value ; Ldarg_1
    +stfld 2 ; Stfld
    +stack_return_to_saved_address .Ball_set_VX_ReturnAddress ; Ret
.Ball_set_VX_this !byte 0, 0
.Ball_set_VX_value !byte 0, 0
.Ball_set_VX_ReturnAddress !byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: get_VY
;----------------------------------------
Ball_get_VY 
    +stack_save_return_adress .Ball_get_VY_ReturnAddress
    +stack_pull_int .Ball_get_VY_this
    +stack_push_var .Ball_get_VY_this ; Ldarg_0
    +ldfld 3 ; Ldfld
    +stack_return_to_saved_address .Ball_get_VY_ReturnAddress ; Ret
.Ball_get_VY_this !byte 0, 0
.Ball_get_VY_ReturnAddress !byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: set_VY
;----------------------------------------
Ball_set_VY 
    +stack_save_return_adress .Ball_set_VY_ReturnAddress
    +stack_pull_int_ref .Ball_set_VY_value, 0
    +stack_pull_int .Ball_set_VY_this
    +stack_push_var .Ball_set_VY_this ; Ldarg_0
    +stack_push_var .Ball_set_VY_value ; Ldarg_1
    +stfld 3 ; Stfld
    +stack_return_to_saved_address .Ball_set_VY_ReturnAddress ; Ret
.Ball_set_VY_this !byte 0, 0
.Ball_set_VY_value !byte 0, 0
.Ball_set_VY_ReturnAddress !byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: set_Sprite
;----------------------------------------
Ball_set_Sprite 
    +stack_save_return_adress .Ball_set_Sprite_ReturnAddress
    +stack_pull_int_ref .Ball_set_Sprite_value, 0
    +stack_pull_int .Ball_set_Sprite_this
    nop ; Nop
    +stack_push_var .Ball_set_Sprite_this ; Ldarg_0
    +stack_push_var .Ball_set_Sprite_value ; Ldarg_1
    +stfld 4 ; Stfld
    +stack_push_var .Ball_set_Sprite_this ; Ldarg_0
    +ldfld 4 ; Ldfld
    +stack_push_int 0 ; Ldc_i4_0
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    +stack_push_var .Ball_set_Sprite_this ; Ldarg_0
    +stack_push_int 20 ; Ldc_i4_s
    jsr Ball_set_X ; Call
    nop ; Nop
    +stack_push_var .Ball_set_Sprite_this ; Ldarg_0
    +stack_push_int 45 ; Ldc_i4_s
    jsr Ball_set_Y ; Call
    nop ; Nop
    +stack_push_var .Ball_set_Sprite_this ; Ldarg_0
    +ldfld 4 ; Ldfld
    +stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_Visible ; Callvirt
    nop ; Nop
    +stack_return_to_saved_address .Ball_set_Sprite_ReturnAddress ; Ret
.Ball_set_Sprite_this !byte 0, 0
.Ball_set_Sprite_value !byte 0, 0
.Ball_set_Sprite_ReturnAddress !byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: set_BallColor
;----------------------------------------
Ball_set_BallColor 
    +stack_save_return_adress .Ball_set_BallColor_ReturnAddress
    +stack_pull_int_ref .Ball_set_BallColor_value, 0
    +stack_pull_int .Ball_set_BallColor_this
    nop ; Nop
    +stack_push_var .Ball_set_BallColor_this ; Ldarg_0
    +ldfld 4 ; Ldfld
    +stack_push_var .Ball_set_BallColor_value ; Ldarg_1
    jsr Sprite_set_Color ; Callvirt
    nop ; Nop
    +stack_return_to_saved_address .Ball_set_BallColor_ReturnAddress ; Ret
.Ball_set_BallColor_this !byte 0, 0
.Ball_set_BallColor_value !byte 0, 0
.Ball_set_BallColor_ReturnAddress !byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: Move
;----------------------------------------
Ball_Move 
    +stack_save_return_adress .Ball_Move_ReturnAddress
    +stack_pull_int .Ball_Move_this
    nop ; Nop
    +stack_push_var .Ball_Move_this ; Ldarg_0
    +stack_push_var .Ball_Move_this ; Ldarg_0
    jsr Ball_get_VX ; Call
    nop ;Conv_i8 ; Conv_i8
    +stack_push_var .Ball_Move_this ; Ldarg_0
    jsr Ball_get_X ; Call
    nop ;Conv_u8 ; Conv_u8
    +add ; Add
    nop ;Conv_u4 ; Conv_u4
    jsr Ball_set_X ; Call
    nop ; Nop
    +stack_push_var .Ball_Move_this ; Ldarg_0
    +stack_push_var .Ball_Move_this ; Ldarg_0
    jsr Ball_get_VY ; Call
    nop ;Conv_i8 ; Conv_i8
    +stack_push_var .Ball_Move_this ; Ldarg_0
    jsr Ball_get_Y ; Call
    nop ;Conv_u8 ; Conv_u8
    +add ; Add
    nop ;Conv_u4 ; Conv_u4
    jsr Ball_set_Y ; Call
    nop ; Nop
    +stack_push_var .Ball_Move_this ; Ldarg_0
    jsr Ball_get_X ; Call
    +stack_push_int 250 ; Ldc_i4
    +compareGreater ; Cgt_un
    +stack_pull_int_ref .Ball_Move_var0, 0 ; Stloc_0
    +stack_push_var .Ball_Move_var0 ; Ldloc_0
    +branch_false label_Ball_Move_78 ; Brfalse_s
    +stack_push_var .Ball_Move_this ; Ldarg_0
    +stack_push_var .Ball_Move_this ; Ldarg_0
    jsr Ball_get_VX ; Call
    +negate ; Neg
    jsr Ball_set_VX ; Call
    nop ; Nop
label_Ball_Move_78:    +stack_push_var .Ball_Move_this ; Ldarg_0
    jsr Ball_get_X ; Call
    +stack_push_int 20 ; Ldc_i4_s
    +compareLess ; Clt_un
    +stack_pull_int_ref .Ball_Move_var1, 0 ; Stloc_1
    +stack_push_var .Ball_Move_var1 ; Ldloc_1
    +branch_false label_Ball_Move_106 ; Brfalse_s
    +stack_push_var .Ball_Move_this ; Ldarg_0
    +stack_push_var .Ball_Move_this ; Ldarg_0
    jsr Ball_get_VX ; Call
    +negate ; Neg
    jsr Ball_set_VX ; Call
    nop ; Nop
label_Ball_Move_106:    +stack_push_var .Ball_Move_this ; Ldarg_0
    jsr Ball_get_Y ; Call
    +stack_push_int 228 ; Ldc_i4
    +compareGreater ; Cgt_un
    +stack_pull_int_ref .Ball_Move_var2, 0 ; Stloc_2
    +stack_push_var .Ball_Move_var2 ; Ldloc_2
    +branch_false label_Ball_Move_137 ; Brfalse_s
    +stack_push_var .Ball_Move_this ; Ldarg_0
    +stack_push_var .Ball_Move_this ; Ldarg_0
    jsr Ball_get_VY ; Call
    +negate ; Neg
    jsr Ball_set_VY ; Call
    nop ; Nop
label_Ball_Move_137:    +stack_push_var .Ball_Move_this ; Ldarg_0
    jsr Ball_get_Y ; Call
    +stack_push_int 45 ; Ldc_i4_s
    +compareLess ; Clt_un
    +stack_pull_int_ref .Ball_Move_var3, 0 ; Stloc_3
    +stack_push_var .Ball_Move_var3 ; Ldloc_3
    +branch_false label_Ball_Move_165 ; Brfalse_s
    +stack_push_var .Ball_Move_this ; Ldarg_0
    +stack_push_var .Ball_Move_this ; Ldarg_0
    jsr Ball_get_VY ; Call
    +negate ; Neg
    jsr Ball_set_VY ; Call
    nop ; Nop
label_Ball_Move_165:    +stack_return_to_saved_address .Ball_Move_ReturnAddress ; Ret
.Ball_Move_this !byte 0, 0
.Ball_Move_ReturnAddress !byte 0,0
.Ball_Move_var0 !byte 0,0
.Ball_Move_var1 !byte 0,0
.Ball_Move_var2 !byte 0,0
.Ball_Move_var3 !byte 0,0


;----------------------------------------
; TYPE: Ball
; METHOD: RunBalls
;----------------------------------------
Ball_RunBalls 
    +stack_save_return_adress .Ball_RunBalls_ReturnAddress
    nop ; Nop
    +newObj 5, 0 ; Newobj
    +stack_pull_int_ref .Ball_RunBalls_var0, 1 ; Stloc_0
    +stack_push_var .Ball_RunBalls_var0 ; Ldloc_0
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite0 ; Callvirt
    jsr Ball_set_Sprite ; Callvirt
    nop ; Nop
    +stack_push_var .Ball_RunBalls_var0 ; Ldloc_0
    +stack_push_int 1 ; Ldc_i4_1
    jsr Ball_set_VX ; Callvirt
    nop ; Nop
    +stack_push_var .Ball_RunBalls_var0 ; Ldloc_0
    +stack_push_int 1 ; Ldc_i4_1
    jsr Ball_set_VY ; Callvirt
    nop ; Nop
    +stack_push_var .Ball_RunBalls_var0 ; Ldloc_0
    +stack_push_int 70 ; Ldc_i4_s
    jsr Ball_set_Y ; Callvirt
    nop ; Nop
    +stack_push_var .Ball_RunBalls_var0 ; Ldloc_0
    +stack_push_int 2 ; Ldc_i4_2
    jsr Ball_set_BallColor ; Callvirt
    nop ; Nop
    +newObj 5, 0 ; Newobj
    +stack_pull_int_ref .Ball_RunBalls_var1, 1 ; Stloc_1
    +stack_push_var .Ball_RunBalls_var1 ; Ldloc_1
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite1 ; Callvirt
    jsr Ball_set_Sprite ; Callvirt
    nop ; Nop
    +stack_push_var .Ball_RunBalls_var1 ; Ldloc_1
    +stack_push_int 1 ; Ldc_i4_1
    jsr Ball_set_VX ; Callvirt
    nop ; Nop
    +stack_push_var .Ball_RunBalls_var1 ; Ldloc_1
    +stack_push_int 2 ; Ldc_i4_2
    jsr Ball_set_VY ; Callvirt
    nop ; Nop
    +stack_push_var .Ball_RunBalls_var1 ; Ldloc_1
    +stack_push_int 100 ; Ldc_i4_s
    jsr Ball_set_X ; Callvirt
    nop ; Nop
    +stack_push_var .Ball_RunBalls_var1 ; Ldloc_1
    +stack_push_int 7 ; Ldc_i4_7
    jsr Ball_set_BallColor ; Callvirt
    nop ; Nop
    +newObj 5, 0 ; Newobj
    +stack_pull_int_ref .Ball_RunBalls_var2, 1 ; Stloc_2
    +stack_push_var .Ball_RunBalls_var2 ; Ldloc_2
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite2 ; Callvirt
    jsr Ball_set_Sprite ; Callvirt
    nop ; Nop
    +stack_push_var .Ball_RunBalls_var2 ; Ldloc_2
    +stack_push_int 2 ; Ldc_i4_2
    jsr Ball_set_VX ; Callvirt
    nop ; Nop
    +stack_push_var .Ball_RunBalls_var2 ; Ldloc_2
    +stack_push_int 1 ; Ldc_i4_1
    jsr Ball_set_VY ; Callvirt
    nop ; Nop
    +stack_push_var .Ball_RunBalls_var2 ; Ldloc_2
    +stack_push_int 120 ; Ldc_i4_s
    jsr Ball_set_X ; Callvirt
    nop ; Nop
    +stack_push_var .Ball_RunBalls_var2 ; Ldloc_2
    +stack_push_int 4 ; Ldc_i4_4
    jsr Ball_set_BallColor ; Callvirt
    nop ; Nop
    jmp label_Ball_RunBalls_217 ; Br_s
label_Ball_RunBalls_171:    nop ; Nop
    +stack_push_var .Ball_RunBalls_var0 ; Ldloc_0
    jsr Ball_Move ; Callvirt
    nop ; Nop
    +stack_push_var .Ball_RunBalls_var1 ; Ldloc_1
    jsr Ball_Move ; Callvirt
    nop ; Nop
    +stack_push_var .Ball_RunBalls_var2 ; Ldloc_2
    jsr Ball_Move ; Callvirt
    nop ; Nop
  ; OPT   +stack_push_int 0 ; Ldc_i4_0
  ; OPT   +stack_pull_int_ref .Ball_RunBalls_var3, 0 ; Stloc_3
    +init_var .Ball_RunBalls_var3, 0 ; Nop
    jmp label_Ball_RunBalls_202 ; Br_s
label_Ball_RunBalls_197:    nop ; Nop
  ; OPT   +stack_push_var .Ball_RunBalls_var3 ; Ldloc_3
  ; OPT   +stack_push_int 1 ; Ldc_i4_1
  ; OPT   +add ; Add
  ; OPT   +stack_pull_int_ref .Ball_RunBalls_var3, 0 ; Stloc_3
    +inc_var .Ball_RunBalls_var3 ; Nop
label_Ball_RunBalls_202:    +stack_push_var .Ball_RunBalls_var3 ; Ldloc_3
    +stack_push_int 255 ; Ldc_i4
    +compareLess ; Clt_un
    +stack_pull_int_ref .Ball_RunBalls_var4, 0 ; Stloc_s
    +stack_push_var .Ball_RunBalls_var4 ; Ldloc_s
    +branch_true label_Ball_RunBalls_197 ; Brtrue_s
    nop ; Nop
label_Ball_RunBalls_217:    jmp label_Ball_RunBalls_171 ; Br_s
.Ball_RunBalls_ReturnAddress !byte 0,0
.Ball_RunBalls_var0 !byte 0,0
.Ball_RunBalls_var1 !byte 0,0
.Ball_RunBalls_var2 !byte 0,0
.Ball_RunBalls_var3 !byte 0,0
.Ball_RunBalls_var4 !byte 0,0


;----------------------------------------
; TYPE: Game
; METHOD: Init
;----------------------------------------
Game_Init 
    +stack_save_return_adress .Game_Init_ReturnAddress
    +stack_pull_int .Game_Init_this
    nop ; Nop
    jsr Game_ClearScreen ; Call
    nop ; Nop
    +stack_push_int 0 ; Ldc_i4_0
    jsr C64_SetBorderColor ; Call
    nop ; Nop
    +stack_push_int 0 ; Ldc_i4_0
    jsr C64_SetBackgroundColor ; Call
    nop ; Nop
    +stack_push_var .Game_Init_this ; Ldarg_0
    jsr Game_InitPlatforms ; Call
    nop ; Nop
    +stack_push_var .Game_Init_this ; Ldarg_0
    +stack_push_int 2 ; Ldc_i4_2
    +newArr  ; Newarr
    +stack_duplicate ; Dup
    +stack_push_int 0 ; Ldc_i4_0
    +newObj 9, 0 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_var .Game_Init_this ; Ldarg_0
    +ldfld 2 ; Ldfld
    +stack_push_int 0 ; Ldc_i4_0
    +ldelemRef ; Ldelem_ref
    jsr PlatformEnemy_set_Platform ; Callvirt
    nop ; Nop
    +stack_duplicate ; Dup
    +stack_push_int 2 ; Ldc_i4_2
    +stfld 4 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 2 ; Ldc_i4_2
    +stfld 3 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 0 ; Ldc_i4_0
    +stfld 2 ; Stfld
    +stack_duplicate ; Dup
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite2 ; Callvirt
    jsr PlatformEnemy_set_Sprite ; Callvirt
    nop ; Nop
    +stack_duplicate ; Dup
    +stack_push_int 2 ; Ldc_i4_2
    jsr PlatformEnemy_set_Color ; Callvirt
    nop ; Nop
    +stelemRef ; Stelem_ref
    +stack_duplicate ; Dup
    +stack_push_int 1 ; Ldc_i4_1
    +newObj 9, 0 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_var .Game_Init_this ; Ldarg_0
    +ldfld 2 ; Ldfld
    +stack_push_int 2 ; Ldc_i4_2
    +ldelemRef ; Ldelem_ref
    jsr PlatformEnemy_set_Platform ; Callvirt
    nop ; Nop
    +stack_duplicate ; Dup
    +stack_push_int 8 ; Ldc_i4_8
    +stfld 4 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 1 ; Ldc_i4_1
    +stfld 3 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 0 ; Ldc_i4_0
    +stfld 2 ; Stfld
    +stack_duplicate ; Dup
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite5 ; Callvirt
    jsr PlatformEnemy_set_Sprite ; Callvirt
    nop ; Nop
    +stack_duplicate ; Dup
    +stack_push_int 2 ; Ldc_i4_2
    jsr PlatformEnemy_set_Color ; Callvirt
    nop ; Nop
    +stelemRef ; Stelem_ref
    +stfld 0 ; Stfld
    +stack_push_var .Game_Init_this ; Ldarg_0
    +newObj 6, 0 ; Newobj
    +stack_duplicate ; Dup
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite4 ; Callvirt
    jsr Player_set_Sprite ; Callvirt
    nop ; Nop
    +stfld 1 ; Stfld
    +stack_return_to_saved_address .Game_Init_ReturnAddress ; Ret
.Game_Init_this !byte 0, 0
.Game_Init_ReturnAddress !byte 0,0


;----------------------------------------
; TYPE: Game
; METHOD: InitPlatforms
;----------------------------------------
Game_InitPlatforms 
    +stack_save_return_adress .Game_InitPlatforms_ReturnAddress
    +stack_pull_int .Game_InitPlatforms_this
    nop ; Nop
    +stack_push_var .Game_InitPlatforms_this ; Ldarg_0
    +stack_push_int 11 ; Ldc_i4_s
    +newArr  ; Newarr
    +stack_duplicate ; Dup
    +stack_push_int 0 ; Ldc_i4_0
    +newObj 7, 0 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 10 ; Ldc_i4_s
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 10 ; Ldc_i4_s
    +stfld 1 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 7 ; Ldc_i4_7
    +stfld 6 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 10 ; Ldc_i4_s
    +stfld 5 ; Stfld
    +stelemRef ; Stelem_ref
    +stack_duplicate ; Dup
    +stack_push_int 1 ; Ldc_i4_1
    +newObj 7, 0 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 1 ; Ldc_i4_1
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 23 ; Ldc_i4_s
    +stfld 1 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 5 ; Ldc_i4_5
    +stfld 6 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 10 ; Ldc_i4_s
    +stfld 5 ; Stfld
    +stelemRef ; Stelem_ref
    +stack_duplicate ; Dup
    +stack_push_int 2 ; Ldc_i4_2
    +newObj 7, 0 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 12 ; Ldc_i4_s
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 20 ; Ldc_i4_s
    +stfld 1 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 5 ; Ldc_i4_5
    +stfld 6 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 13 ; Ldc_i4_s
    +stfld 5 ; Stfld
    +stelemRef ; Stelem_ref
    +stack_duplicate ; Dup
    +stack_push_int 3 ; Ldc_i4_3
    +newObj 7, 0 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 23 ; Ldc_i4_s
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 16 ; Ldc_i4_s
    +stfld 1 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 14 ; Ldc_i4_s
    +stfld 6 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 6 ; Ldc_i4_6
    +stfld 5 ; Stfld
    +stelemRef ; Stelem_ref
    +stack_duplicate ; Dup
    +stack_push_int 4 ; Ldc_i4_4
    +newObj 7, 0 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 26 ; Ldc_i4_s
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 12 ; Ldc_i4_s
    +stfld 1 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 14 ; Ldc_i4_s
    +stfld 6 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 1 ; Ldc_i4_1
    +stfld 5 ; Stfld
    +stelemRef ; Stelem_ref
    +stack_duplicate ; Dup
    +stack_push_int 5 ; Ldc_i4_5
    +newObj 7, 0 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 0 ; Ldc_i4_0
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 9 ; Ldc_i4_s
    +stfld 1 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 7 ; Ldc_i4_7
    +stfld 6 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 9 ; Ldc_i4_s
    +stfld 5 ; Stfld
    +stelemRef ; Stelem_ref
    +stack_duplicate ; Dup
    +stack_push_int 6 ; Ldc_i4_6
    +newObj 7, 0 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 9 ; Ldc_i4_s
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 5 ; Ldc_i4_5
    +stfld 1 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 2 ; Ldc_i4_2
    +stfld 6 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 1 ; Ldc_i4_1
    +stfld 5 ; Stfld
    +stelemRef ; Stelem_ref
    +stack_duplicate ; Dup
    +stack_push_int 7 ; Ldc_i4_7
    +newObj 7, 0 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 15 ; Ldc_i4_s
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 5 ; Ldc_i4_5
    +stfld 1 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 2 ; Ldc_i4_2
    +stfld 6 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 1 ; Ldc_i4_1
    +stfld 5 ; Stfld
    +stelemRef ; Stelem_ref
    +stack_duplicate ; Dup
    +stack_push_int 8 ; Ldc_i4_8
    +newObj 7, 0 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 21 ; Ldc_i4_s
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 5 ; Ldc_i4_5
    +stfld 1 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 2 ; Ldc_i4_2
    +stfld 6 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 1 ; Ldc_i4_1
    +stfld 5 ; Stfld
    +stelemRef ; Stelem_ref
    +stack_duplicate ; Dup
    +stack_push_int 9 ; Ldc_i4_s
    +newObj 7, 0 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 27 ; Ldc_i4_s
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 5 ; Ldc_i4_5
    +stfld 1 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 2 ; Ldc_i4_2
    +stfld 6 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 1 ; Ldc_i4_1
    +stfld 5 ; Stfld
    +stelemRef ; Stelem_ref
    +stack_duplicate ; Dup
    +stack_push_int 10 ; Ldc_i4_s
    +newObj 7, 0 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 33 ; Ldc_i4_s
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 5 ; Ldc_i4_5
    +stfld 1 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 2 ; Ldc_i4_2
    +stfld 6 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 1 ; Ldc_i4_1
    +stfld 5 ; Stfld
    +stelemRef ; Stelem_ref
    +stfld 2 ; Stfld
  ; OPT   +stack_push_int 0 ; Ldc_i4_0
  ; OPT   +stack_pull_int_ref .Game_InitPlatforms_var0, 0 ; Stloc_0
    +init_var .Game_InitPlatforms_var0, 0 ; Nop
    jmp label_Game_InitPlatforms_455 ; Br_s
label_Game_InitPlatforms_437:    +stack_push_var .Game_InitPlatforms_this ; Ldarg_0
    +ldfld 2 ; Ldfld
    +stack_push_var .Game_InitPlatforms_var0 ; Ldloc_0
    +ldelemRef ; Ldelem_ref
    jsr Platform_Draw ; Callvirt
    nop ; Nop
  ; OPT   +stack_push_var .Game_InitPlatforms_var0 ; Ldloc_0
  ; OPT   +stack_push_int 1 ; Ldc_i4_1
  ; OPT   +add ; Add
  ; OPT   +stack_pull_int_ref .Game_InitPlatforms_var0, 0 ; Stloc_0
    +inc_var .Game_InitPlatforms_var0 ; Nop
label_Game_InitPlatforms_455:    +stack_push_var .Game_InitPlatforms_var0 ; Ldloc_0
    nop ;Conv_u8 ; Conv_u8
    +stack_push_var .Game_InitPlatforms_this ; Ldarg_0
    +ldfld 2 ; Ldfld
    +ldlen ; Ldlen
    nop ;Conv_i4 ; Conv_i4
    nop ;Conv_i8 ; Conv_i8
    +compareLess ; Clt
    +stack_pull_int_ref .Game_InitPlatforms_var1, 0 ; Stloc_1
    +stack_push_var .Game_InitPlatforms_var1 ; Ldloc_1
    +branch_true label_Game_InitPlatforms_437 ; Brtrue_s
    +stack_return_to_saved_address .Game_InitPlatforms_ReturnAddress ; Ret
.Game_InitPlatforms_this !byte 0, 0
.Game_InitPlatforms_ReturnAddress !byte 0,0
.Game_InitPlatforms_var0 !byte 0,0
.Game_InitPlatforms_var1 !byte 0,0


;----------------------------------------
; TYPE: Game
; METHOD: Run
;----------------------------------------
Game_Run 
    +stack_save_return_adress .Game_Run_ReturnAddress
    +stack_pull_int .Game_Run_this
    nop ; Nop
    jmp label_Game_Run_47 ; Br_s
label_Game_Run_3:    nop ; Nop
    +stack_push_var .Game_Run_this ; Ldarg_0
    jsr Game_Step ; Call
    nop ; Nop
  ; OPT   +stack_push_int 0 ; Ldc_i4_0
  ; OPT   +stack_pull_int_ref .Game_Run_var0, 0 ; Stloc_0
    +init_var .Game_Run_var0, 0 ; Nop
    jmp label_Game_Run_37 ; Br_s
label_Game_Run_15:  ; OPT   +stack_push_int 0 ; Ldc_i4_0
  ; OPT   +stack_pull_int_ref .Game_Run_var1, 0 ; Stloc_1
    +init_var .Game_Run_var1, 0 ; Nop
    jmp label_Game_Run_24 ; Br_s
label_Game_Run_19:    nop ; Nop
  ; OPT   +stack_push_var .Game_Run_var1 ; Ldloc_1
  ; OPT   +stack_push_int 1 ; Ldc_i4_1
  ; OPT   +add ; Add
  ; OPT   +stack_pull_int_ref .Game_Run_var1, 0 ; Stloc_1
    +inc_var .Game_Run_var1 ; Nop
label_Game_Run_24:  ; OPT   +stack_push_var .Game_Run_var1 ; Ldloc_1
  ; OPT   +stack_push_int 15 ; Ldc_i4_s
  ; OPT   +compareLess ; Clt_un
  ; OPT   +stack_pull_int_ref .Game_Run_var2, 0 ; Stloc_2
  ; OPT   +stack_push_var .Game_Run_var2 ; Ldloc_2
  ; OPT   +branch_true label_Game_Run_19 ; Brtrue_s
    +branch_if_var_less .Game_Run_var1, 15, label_Game_Run_19 ; Nop
  ; OPT   +stack_push_var .Game_Run_var0 ; Ldloc_0
  ; OPT   +stack_push_int 1 ; Ldc_i4_1
  ; OPT   +add ; Add
  ; OPT   +stack_pull_int_ref .Game_Run_var0, 0 ; Stloc_0
    +inc_var .Game_Run_var0 ; Nop
label_Game_Run_37:  ; OPT   +stack_push_var .Game_Run_var0 ; Ldloc_0
  ; OPT   +stack_push_int 100 ; Ldc_i4_s
  ; OPT   +compareLess ; Clt_un
  ; OPT   +stack_pull_int_ref .Game_Run_var3, 0 ; Stloc_3
  ; OPT   +stack_push_var .Game_Run_var3 ; Ldloc_3
  ; OPT   +branch_true label_Game_Run_15 ; Brtrue_s
    +branch_if_var_less .Game_Run_var0, 100, label_Game_Run_15 ; Nop
    nop ; Nop
label_Game_Run_47:    jmp label_Game_Run_3 ; Br_s
.Game_Run_this !byte 0, 0
.Game_Run_ReturnAddress !byte 0,0
.Game_Run_var0 !byte 0,0
.Game_Run_var1 !byte 0,0
.Game_Run_var2 !byte 0,0
.Game_Run_var3 !byte 0,0


;----------------------------------------
; TYPE: Game
; METHOD: Step
;----------------------------------------
Game_Step 
    +stack_save_return_adress .Game_Step_ReturnAddress
    +stack_pull_int .Game_Step_this
    nop ; Nop
  ; OPT   +stack_push_int 0 ; Ldc_i4_0
  ; OPT   +stack_pull_int_ref .Game_Step_var1, 0 ; Stloc_1
    +init_var .Game_Step_var1, 0 ; Nop
    jmp label_Game_Step_23 ; Br_s
label_Game_Step_5:    +stack_push_var .Game_Step_this ; Ldarg_0
    +ldfld 0 ; Ldfld
    +stack_push_var .Game_Step_var1 ; Ldloc_1
    +ldelemRef ; Ldelem_ref
    jsr PlatformEnemy_Move ; Callvirt
    nop ; Nop
  ; OPT   +stack_push_var .Game_Step_var1 ; Ldloc_1
  ; OPT   +stack_push_int 1 ; Ldc_i4_1
  ; OPT   +add ; Add
  ; OPT   +stack_pull_int_ref .Game_Step_var1, 0 ; Stloc_1
    +inc_var .Game_Step_var1 ; Nop
label_Game_Step_23:    +stack_push_var .Game_Step_var1 ; Ldloc_1
    +stack_push_var .Game_Step_this ; Ldarg_0
    +ldfld 0 ; Ldfld
    +ldlen ; Ldlen
    nop ;Conv_i4 ; Conv_i4
    +compareLess ; Clt
    +stack_pull_int_ref .Game_Step_var2, 0 ; Stloc_2
    +stack_push_var .Game_Step_var2 ; Ldloc_2
    +branch_true label_Game_Step_5 ; Brtrue_s
  ; OPT   +stack_push_int 255 ; Ldc_i4
  ; OPT   +stack_pull_int_ref .Game_Step_var0, 0 ; Stloc_0
    +init_var .Game_Step_var0, 255 ; Nop
  ; OPT   +stack_push_int 0 ; Ldc_i4_0
  ; OPT   +stack_pull_int_ref .Game_Step_var3, 0 ; Stloc_3
    +init_var .Game_Step_var3, 0 ; Nop
    jmp label_Game_Step_105 ; Br_s
label_Game_Step_48:    nop ; Nop
    +stack_push_var .Game_Step_this ; Ldarg_0
    +ldfld 2 ; Ldfld
    +stack_push_var .Game_Step_var3 ; Ldloc_3
    +ldelemRef ; Ldelem_ref
    +stack_push_var .Game_Step_this ; Ldarg_0
    +ldfld 1 ; Ldfld
    jsr Player_get_X ; Callvirt
    +stack_push_var .Game_Step_this ; Ldarg_0
    +ldfld 1 ; Ldfld
    jsr Player_get_Y ; Callvirt
    jsr Platform_DistanceToPlatform ; Callvirt
    +stack_pull_int_ref .Game_Step_var4, 0 ; Stloc_s
    +stack_push_var .Game_Step_var4 ; Ldloc_s
    +stack_push_var .Game_Step_var0 ; Ldloc_0
    +compareLess ; Clt_un
    +stack_pull_int_ref .Game_Step_var5, 0 ; Stloc_s
    +stack_push_var .Game_Step_var5 ; Ldloc_s
    +branch_false label_Game_Step_100 ; Brfalse_s
    +stack_push_var .Game_Step_var4 ; Ldloc_s
    +stack_pull_int_ref .Game_Step_var0, 0 ; Stloc_0
label_Game_Step_100:    nop ; Nop
  ; OPT   +stack_push_var .Game_Step_var3 ; Ldloc_3
  ; OPT   +stack_push_int 1 ; Ldc_i4_1
  ; OPT   +add ; Add
  ; OPT   +stack_pull_int_ref .Game_Step_var3, 0 ; Stloc_3
    +inc_var .Game_Step_var3 ; Nop
label_Game_Step_105:    +stack_push_var .Game_Step_var3 ; Ldloc_3
    +stack_push_var .Game_Step_this ; Ldarg_0
    +ldfld 2 ; Ldfld
    +ldlen ; Ldlen
    nop ;Conv_i4 ; Conv_i4
    +compareLess ; Clt
    +stack_pull_int_ref .Game_Step_var6, 0 ; Stloc_s
    +stack_push_var .Game_Step_var6 ; Ldloc_s
    +branch_true label_Game_Step_48 ; Brtrue_s
    +stack_push_var .Game_Step_this ; Ldarg_0
    +ldfld 1 ; Ldfld
    +stack_push_var .Game_Step_var0 ; Ldloc_0
    jsr Player_Move ; Callvirt
    nop ; Nop
    +stack_return_to_saved_address .Game_Step_ReturnAddress ; Ret
.Game_Step_this !byte 0, 0
.Game_Step_ReturnAddress !byte 0,0
.Game_Step_var0 !byte 0,0
.Game_Step_var1 !byte 0,0
.Game_Step_var2 !byte 0,0
.Game_Step_var3 !byte 0,0
.Game_Step_var4 !byte 0,0
.Game_Step_var5 !byte 0,0
.Game_Step_var6 !byte 0,0


;----------------------------------------
; TYPE: Game
; METHOD: ClearScreen
;----------------------------------------
Game_ClearScreen 
    +stack_save_return_adress .Game_ClearScreen_ReturnAddress
    nop ; Nop
  ; OPT   +stack_push_int 0 ; Ldc_i4_0
  ; OPT   +stack_pull_int_ref .Game_ClearScreen_var0, 0 ; Stloc_0
    +init_var .Game_ClearScreen_var0, 0 ; Nop
    jmp label_Game_ClearScreen_42 ; Br_s
label_Game_ClearScreen_5:    nop ; Nop
  ; OPT   +stack_push_int 0 ; Ldc_i4_0
  ; OPT   +stack_pull_int_ref .Game_ClearScreen_var1, 0 ; Stloc_1
    +init_var .Game_ClearScreen_var1, 0 ; Nop
    jmp label_Game_ClearScreen_28 ; Br_s
label_Game_ClearScreen_10:    nop ; Nop
    +stack_push_var .Game_ClearScreen_var0 ; Ldloc_0
    +stack_push_var .Game_ClearScreen_var1 ; Ldloc_1
    +stack_push_int 32 ; Ldc_i4_s
    +stack_push_int 14 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    nop ; Nop
  ; OPT   +stack_push_var .Game_ClearScreen_var1 ; Ldloc_1
  ; OPT   +stack_push_int 1 ; Ldc_i4_1
  ; OPT   +add ; Add
  ; OPT   +stack_pull_int_ref .Game_ClearScreen_var1, 0 ; Stloc_1
    +inc_var .Game_ClearScreen_var1 ; Nop
label_Game_ClearScreen_28:  ; OPT   +stack_push_var .Game_ClearScreen_var1 ; Ldloc_1
  ; OPT   +stack_push_int 25 ; Ldc_i4_s
  ; OPT   +compareLess ; Clt_un
  ; OPT   +stack_pull_int_ref .Game_ClearScreen_var2, 0 ; Stloc_2
  ; OPT   +stack_push_var .Game_ClearScreen_var2 ; Ldloc_2
  ; OPT   +branch_true label_Game_ClearScreen_10 ; Brtrue_s
    +branch_if_var_less .Game_ClearScreen_var1, 25, label_Game_ClearScreen_10 ; Nop
    nop ; Nop
  ; OPT   +stack_push_var .Game_ClearScreen_var0 ; Ldloc_0
  ; OPT   +stack_push_int 1 ; Ldc_i4_1
  ; OPT   +add ; Add
  ; OPT   +stack_pull_int_ref .Game_ClearScreen_var0, 0 ; Stloc_0
    +inc_var .Game_ClearScreen_var0 ; Nop
label_Game_ClearScreen_42:  ; OPT   +stack_push_var .Game_ClearScreen_var0 ; Ldloc_0
  ; OPT   +stack_push_int 40 ; Ldc_i4_s
  ; OPT   +compareLess ; Clt_un
  ; OPT   +stack_pull_int_ref .Game_ClearScreen_var3, 0 ; Stloc_3
  ; OPT   +stack_push_var .Game_ClearScreen_var3 ; Ldloc_3
  ; OPT   +branch_true label_Game_ClearScreen_5 ; Brtrue_s
    +branch_if_var_less .Game_ClearScreen_var0, 40, label_Game_ClearScreen_5 ; Nop
    +stack_return_to_saved_address .Game_ClearScreen_ReturnAddress ; Ret
.Game_ClearScreen_ReturnAddress !byte 0,0
.Game_ClearScreen_var0 !byte 0,0
.Game_ClearScreen_var1 !byte 0,0
.Game_ClearScreen_var2 !byte 0,0
.Game_ClearScreen_var3 !byte 0,0
.GCTest_field_a1 !byte 0
.GCTest_field_a2 !byte 0


;----------------------------------------
; TYPE: GCTest
; METHOD: Two_Instances_First_GCd_Static_Field
;----------------------------------------
GCTest_Two_Instances_First_GCd_Static_Field 
    +stack_save_return_adress .GCTest_Two_Instances_First_GCd_Static_Field_ReturnAddress
    nop ; Nop
    +newObj 2, 1 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 4 ; Ldc_i4_4
    +stfld 0 ; Stfld
    +stack_pull_int_ref .GCTest_field_a1, 1 ; Stsfld
    +newObj 2, 1 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 5 ; Ldc_i4_5
    +stfld 0 ; Stfld
    +stack_pull_int_ref .GCTest_field_a2, 1 ; Stsfld
    jsr C64_get_Debug ; Call
    +stack_push_var .GCTest_field_a1 ; Ldsfld
    jsr Debug_GetObjectId ; Callvirt
    +stack_pull_int_ref .GCTest_Two_Instances_First_GCd_Static_Field_var0, 0 ; Stloc_0
    jsr C64_get_Debug ; Call
    +stack_push_var .GCTest_field_a2 ; Ldsfld
    jsr Debug_GetObjectId ; Callvirt
    +stack_pull_int_ref .GCTest_Two_Instances_First_GCd_Static_Field_var1, 0 ; Stloc_1
    jsr C64_get_Debug ; Call
    +stack_push_var .GCTest_Two_Instances_First_GCd_Static_Field_var0 ; Ldloc_0
    jsr Debug_IsAlive ; Callvirt
    +stack_push_int 0 ; Ldc_i4_0
    +compareEqual ; Ceq
    +stack_pull_int_ref .GCTest_Two_Instances_First_GCd_Static_Field_var2, 0 ; Stloc_2
    +stack_push_var .GCTest_Two_Instances_First_GCd_Static_Field_var2 ; Ldloc_2
    +branch_false label_GCTest_Two_Instances_First_GCd_Static_Field_102 ; Brfalse_s
    nop ; Nop
    +stack_push_pointer .string_1879048193 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
    jmp label_GCTest_Two_Instances_First_GCd_Static_Field_236 ; Br
label_GCTest_Two_Instances_First_GCd_Static_Field_102:    +stack_push_int 0 ; Ldnull
    +stack_pull_int_ref .GCTest_field_a1, 1 ; Stsfld
    jsr GC_Collect ; Call
    nop ; Nop
    jsr C64_get_Debug ; Call
    +stack_push_var .GCTest_Two_Instances_First_GCd_Static_Field_var0 ; Ldloc_0
    jsr Debug_IsAlive ; Callvirt
    +stack_pull_int_ref .GCTest_Two_Instances_First_GCd_Static_Field_var3, 0 ; Stloc_3
    +stack_push_var .GCTest_Two_Instances_First_GCd_Static_Field_var3 ; Ldloc_3
    +branch_false label_GCTest_Two_Instances_First_GCd_Static_Field_140 ; Brfalse_s
    +stack_push_pointer .string_1879048213 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
label_GCTest_Two_Instances_First_GCd_Static_Field_140:    jsr C64_get_Debug ; Call
    +stack_push_var .GCTest_Two_Instances_First_GCd_Static_Field_var1 ; Ldloc_1
    jsr Debug_IsAlive ; Callvirt
    +stack_push_int 0 ; Ldc_i4_0
    +compareEqual ; Ceq
    +stack_pull_int_ref .GCTest_Two_Instances_First_GCd_Static_Field_var4, 0 ; Stloc_s
    +stack_push_var .GCTest_Two_Instances_First_GCd_Static_Field_var4 ; Ldloc_s
    +branch_false label_GCTest_Two_Instances_First_GCd_Static_Field_171 ; Brfalse_s
    +stack_push_pointer .string_1879048245 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
label_GCTest_Two_Instances_First_GCd_Static_Field_171:    +stack_push_var .GCTest_field_a2 ; Ldsfld
    +ldfld 0 ; Ldfld
    +stack_push_int 5 ; Ldc_i4_5
    +compareEqual ; Ceq
    +stack_push_int 0 ; Ldc_i4_0
    +compareEqual ; Ceq
    +stack_pull_int_ref .GCTest_Two_Instances_First_GCd_Static_Field_var5, 0 ; Stloc_s
    +stack_push_var .GCTest_Two_Instances_First_GCd_Static_Field_var5 ; Ldloc_s
    +branch_false label_GCTest_Two_Instances_First_GCd_Static_Field_204 ; Brfalse_s
    +stack_push_pointer .string_1879048273 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
label_GCTest_Two_Instances_First_GCd_Static_Field_204:    +stack_push_int 0 ; Ldc_i4_0
    +stack_push_int 1 ; Ldc_i4_1
    +stack_push_var .GCTest_field_a2 ; Ldsfld
    +ldfld 0 ; Ldfld
    +stack_push_int 14 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    +stack_push_int 0 ; Ldnull
    +stack_duplicate ; Dup
    +stack_pull_int_ref .GCTest_field_a2, 1 ; Stsfld
    +stack_pull_int_ref .GCTest_field_a1, 1 ; Stsfld
label_GCTest_Two_Instances_First_GCd_Static_Field_236:    +stack_return_to_saved_address .GCTest_Two_Instances_First_GCd_Static_Field_ReturnAddress ; Ret
.GCTest_Two_Instances_First_GCd_Static_Field_ReturnAddress !byte 0,0
.GCTest_Two_Instances_First_GCd_Static_Field_var0 !byte 0,0
.GCTest_Two_Instances_First_GCd_Static_Field_var1 !byte 0,0
.GCTest_Two_Instances_First_GCd_Static_Field_var2 !byte 0,0
.GCTest_Two_Instances_First_GCd_Static_Field_var3 !byte 0,0
.GCTest_Two_Instances_First_GCd_Static_Field_var4 !byte 0,0
.GCTest_Two_Instances_First_GCd_Static_Field_var5 !byte 0,0


;----------------------------------------
; TYPE: GCTest
; METHOD: Start
;----------------------------------------
GCTest_Start 
    +stack_save_return_adress .GCTest_Start_ReturnAddress
    nop ; Nop
    jsr GCTest_Two_Instances_First_GCd_Static_Field ; Call
    nop ; Nop
    +newObj 2, 1 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 1 ; Ldc_i4_1
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +newObj 2, 1 ; Newobj
    +stfld 1 ; Stfld
    +stack_pull_int_ref .GCTest_Start_var0, 1 ; Stloc_0
    +newObj 2, 1 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 4 ; Ldc_i4_4
    +stfld 0 ; Stfld
    +stack_pull_int_ref .GCTest_Start_var1, 1 ; Stloc_1
    +newObj 2, 1 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 2 ; Ldc_i4_2
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_var .GCTest_Start_var0 ; Ldloc_0
    +stfld 1 ; Stfld
    +stack_pull_int_ref .GCTest_Start_var2, 1 ; Stloc_2
    +newObj 2, 1 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 5 ; Ldc_i4_5
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_var .GCTest_Start_var1 ; Ldloc_1
    +stfld 1 ; Stfld
    +stack_pull_int_ref .GCTest_Start_var3, 1 ; Stloc_3
    +newObj 2, 1 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 3 ; Ldc_i4_3
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_var .GCTest_Start_var2 ; Ldloc_2
    +stfld 1 ; Stfld
    +stack_pull_int_ref .GCTest_Start_var4, 1 ; Stloc_s
    +newObj 2, 1 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 6 ; Ldc_i4_6
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_var .GCTest_Start_var3 ; Ldloc_3
    +stfld 1 ; Stfld
    +stack_pull_int_ref .GCTest_Start_var5, 1 ; Stloc_s
    +stack_push_int 0 ; Ldnull
    +stack_duplicate ; Dup
    +stack_pull_int_ref .GCTest_Start_var4, 1 ; Stloc_s
    +stack_duplicate ; Dup
    +stack_pull_int_ref .GCTest_Start_var2, 1 ; Stloc_2
    +stack_pull_int_ref .GCTest_Start_var0, 1 ; Stloc_0
    +stack_push_int 0 ; Ldnull
    +stack_duplicate ; Dup
    +stack_pull_int_ref .GCTest_Start_var3, 1 ; Stloc_3
    +stack_pull_int_ref .GCTest_Start_var1, 1 ; Stloc_1
    +stack_push_var .GCTest_Start_var5 ; Ldloc_s
    +ldfld 1 ; Ldfld
    +ldfld 1 ; Ldfld
    +ldfld 0 ; Ldfld
    +stack_push_int 4 ; Ldc_i4_4
    +compareEqual ; Ceq
    +stack_push_int 0 ; Ldc_i4_0
    +compareEqual ; Ceq
    +stack_pull_int_ref .GCTest_Start_var6, 0 ; Stloc_s
    +stack_push_var .GCTest_Start_var6 ; Ldloc_s
    +branch_false label_GCTest_Start_177 ; Brfalse_s
    +stack_push_pointer .string_1879048299 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
label_GCTest_Start_177:    jsr GC_Collect ; Call
    nop ; Nop
    +stack_push_var .GCTest_Start_var5 ; Ldloc_s
    +ldfld 0 ; Ldfld
    +stack_push_int 6 ; Ldc_i4_6
    +compareEqual ; Ceq
    +stack_push_int 0 ; Ldc_i4_0
    +compareEqual ; Ceq
    +stack_pull_int_ref .GCTest_Start_var7, 0 ; Stloc_s
    +stack_push_var .GCTest_Start_var7 ; Ldloc_s
    +branch_false label_GCTest_Start_213 ; Brfalse_s
    +stack_push_pointer .string_1879048345 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
label_GCTest_Start_213:    +stack_push_var .GCTest_Start_var5 ; Ldloc_s
    +ldfld 1 ; Ldfld
    +ldfld 0 ; Ldfld
    +stack_push_int 5 ; Ldc_i4_5
    +compareEqual ; Ceq
    +stack_push_int 0 ; Ldc_i4_0
    +compareEqual ; Ceq
    +stack_pull_int_ref .GCTest_Start_var8, 0 ; Stloc_s
    +stack_push_var .GCTest_Start_var8 ; Ldloc_s
    +branch_false label_GCTest_Start_248 ; Brfalse_s
    +stack_push_pointer .string_1879048367 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
label_GCTest_Start_248:    +stack_push_var .GCTest_Start_var5 ; Ldloc_s
    +ldfld 1 ; Ldfld
    +ldfld 1 ; Ldfld
    +ldfld 0 ; Ldfld
    +stack_push_int 4 ; Ldc_i4_4
    +compareEqual ; Ceq
    +stack_push_int 0 ; Ldc_i4_0
    +compareEqual ; Ceq
    +stack_pull_int_ref .GCTest_Start_var9, 0 ; Stloc_s
    +stack_push_var .GCTest_Start_var9 ; Ldloc_s
    +branch_false label_GCTest_Start_288 ; Brfalse_s
    +stack_push_pointer .string_1879048299 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
label_GCTest_Start_288:    +stack_push_pointer .string_1879048401 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
    +deref .GCTest_Start_var0 ; Nop
    +deref .GCTest_Start_var1 ; Nop
    +deref .GCTest_Start_var2 ; Nop
    +deref .GCTest_Start_var3 ; Nop
    +deref .GCTest_Start_var4 ; Nop
    +deref .GCTest_Start_var5 ; Nop
    +stack_return_to_saved_address .GCTest_Start_ReturnAddress ; Ret
.GCTest_Start_ReturnAddress !byte 0,0
.GCTest_Start_var0 !byte 0,0
.GCTest_Start_var1 !byte 0,0
.GCTest_Start_var2 !byte 0,0
.GCTest_Start_var3 !byte 0,0
.GCTest_Start_var4 !byte 0,0
.GCTest_Start_var5 !byte 0,0
.GCTest_Start_var6 !byte 0,0
.GCTest_Start_var7 !byte 0,0
.GCTest_Start_var8 !byte 0,0
.GCTest_Start_var9 !byte 0,0


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: set_Platform
;----------------------------------------
PlatformEnemy_set_Platform 
    +stack_save_return_adress .PlatformEnemy_set_Platform_ReturnAddress
    +stack_pull_int_ref .PlatformEnemy_set_Platform_value, 1
    +stack_pull_int .PlatformEnemy_set_Platform_this
    nop ; Nop
    +stack_push_var .PlatformEnemy_set_Platform_this ; Ldarg_0
    +stack_push_var .PlatformEnemy_set_Platform_value ; Ldarg_1
    +ldfld 3 ; Ldfld
    +stack_push_int 20 ; Ldc_i4_s
    +add ; Add
    jsr PlatformEnemy_set_MinX ; Call
    nop ; Nop
    +stack_push_var .PlatformEnemy_set_Platform_this ; Ldarg_0
    +stack_push_var .PlatformEnemy_set_Platform_value ; Ldarg_1
    +ldfld 4 ; Ldfld
    +stack_push_int 20 ; Ldc_i4_s
    +sub ; Sub
    jsr PlatformEnemy_set_MaxX ; Call
    nop ; Nop
    +stack_push_var .PlatformEnemy_set_Platform_this ; Ldarg_0
    +stack_push_var .PlatformEnemy_set_Platform_value ; Ldarg_1
    +ldfld 2 ; Ldfld
    +stack_push_int 2 ; Ldc_i4_2
    +sub ; Sub
    +stfld 1 ; Stfld
    +deref .PlatformEnemy_set_Platform_value ; Nop
    +stack_return_to_saved_address .PlatformEnemy_set_Platform_ReturnAddress ; Ret
.PlatformEnemy_set_Platform_this !byte 0, 0
.PlatformEnemy_set_Platform_value !byte 0, 0
.PlatformEnemy_set_Platform_ReturnAddress !byte 0,0


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: set_Sprite
;----------------------------------------
PlatformEnemy_set_Sprite 
    +stack_save_return_adress .PlatformEnemy_set_Sprite_ReturnAddress
    +stack_pull_int_ref .PlatformEnemy_set_Sprite_value, 0
    +stack_pull_int .PlatformEnemy_set_Sprite_this
    nop ; Nop
    +stack_push_var .PlatformEnemy_set_Sprite_this ; Ldarg_0
    +stack_push_var .PlatformEnemy_set_Sprite_value ; Ldarg_1
    +stfld 8 ; Stfld
    +stack_push_var .PlatformEnemy_set_Sprite_this ; Ldarg_0
    +ldfld 8 ; Ldfld
    +stack_push_var .PlatformEnemy_set_Sprite_this ; Ldarg_0
    +ldfld 2 ; Ldfld
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    +stack_push_var .PlatformEnemy_set_Sprite_this ; Ldarg_0
    +stack_push_int 1 ; Ldc_i4_1
    +stfld 7 ; Stfld
    +stack_push_var .PlatformEnemy_set_Sprite_this ; Ldarg_0
    +stack_push_var .PlatformEnemy_set_Sprite_this ; Ldarg_0
    jsr PlatformEnemy_get_MinX ; Call
    jsr PlatformEnemy_set_X ; Call
    nop ; Nop
    +stack_push_var .PlatformEnemy_set_Sprite_this ; Ldarg_0
    +stack_push_var .PlatformEnemy_set_Sprite_this ; Ldarg_0
    jsr PlatformEnemy_get_Y ; Call
    jsr PlatformEnemy_set_Y ; Call
    nop ; Nop
    +stack_push_var .PlatformEnemy_set_Sprite_this ; Ldarg_0
    +ldfld 8 ; Ldfld
    +stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_Visible ; Callvirt
    nop ; Nop
    +stack_return_to_saved_address .PlatformEnemy_set_Sprite_ReturnAddress ; Ret
.PlatformEnemy_set_Sprite_this !byte 0, 0
.PlatformEnemy_set_Sprite_value !byte 0, 0
.PlatformEnemy_set_Sprite_ReturnAddress !byte 0,0


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: set_Color
;----------------------------------------
PlatformEnemy_set_Color 
    +stack_save_return_adress .PlatformEnemy_set_Color_ReturnAddress
    +stack_pull_int_ref .PlatformEnemy_set_Color_value, 0
    +stack_pull_int .PlatformEnemy_set_Color_this
    nop ; Nop
    +stack_push_var .PlatformEnemy_set_Color_this ; Ldarg_0
    +ldfld 8 ; Ldfld
    +stack_push_var .PlatformEnemy_set_Color_value ; Ldarg_1
    jsr Sprite_set_Color ; Callvirt
    nop ; Nop
    +stack_return_to_saved_address .PlatformEnemy_set_Color_ReturnAddress ; Ret
.PlatformEnemy_set_Color_this !byte 0, 0
.PlatformEnemy_set_Color_value !byte 0, 0
.PlatformEnemy_set_Color_ReturnAddress !byte 0,0


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: Move
;----------------------------------------
PlatformEnemy_Move 
    +stack_save_return_adress .PlatformEnemy_Move_ReturnAddress
    +stack_pull_int .PlatformEnemy_Move_this
    nop ; Nop
    +stack_push_var .PlatformEnemy_Move_this ; Ldarg_0
    +ldfld 7 ; Ldfld
    +stack_pull_int_ref .PlatformEnemy_Move_var0, 0 ; Stloc_0
    +stack_push_var .PlatformEnemy_Move_var0 ; Ldloc_0
    +branch_false label_PlatformEnemy_Move_60 ; Brfalse_s
    nop ; Nop
    +stack_push_var .PlatformEnemy_Move_this ; Ldarg_0
    +stack_push_var .PlatformEnemy_Move_this ; Ldarg_0
    jsr PlatformEnemy_get_X ; Call
    +stack_push_var .PlatformEnemy_Move_this ; Ldarg_0
    +ldfld 3 ; Ldfld
    +add ; Add
    jsr PlatformEnemy_set_X ; Call
    nop ; Nop
    +stack_push_var .PlatformEnemy_Move_this ; Ldarg_0
    jsr PlatformEnemy_get_X ; Call
    +stack_push_var .PlatformEnemy_Move_this ; Ldarg_0
    jsr PlatformEnemy_get_MaxX ; Call
    +compareGreater ; Cgt_un
    +stack_pull_int_ref .PlatformEnemy_Move_var1, 0 ; Stloc_1
    +stack_push_var .PlatformEnemy_Move_var1 ; Ldloc_1
    +branch_false label_PlatformEnemy_Move_57 ; Brfalse_s
    +stack_push_var .PlatformEnemy_Move_this ; Ldarg_0
    +stack_push_int 0 ; Ldc_i4_0
    +stfld 7 ; Stfld
label_PlatformEnemy_Move_57:    nop ; Nop
    jmp label_PlatformEnemy_Move_107 ; Br_s
label_PlatformEnemy_Move_60:    nop ; Nop
    +stack_push_var .PlatformEnemy_Move_this ; Ldarg_0
    +stack_push_var .PlatformEnemy_Move_this ; Ldarg_0
    jsr PlatformEnemy_get_X ; Call
    +stack_push_var .PlatformEnemy_Move_this ; Ldarg_0
    +ldfld 4 ; Ldfld
    +sub ; Sub
    jsr PlatformEnemy_set_X ; Call
    nop ; Nop
    +stack_push_var .PlatformEnemy_Move_this ; Ldarg_0
    jsr PlatformEnemy_get_X ; Call
    +stack_push_var .PlatformEnemy_Move_this ; Ldarg_0
    jsr PlatformEnemy_get_MinX ; Call
    +compareLess ; Clt_un
    +stack_pull_int_ref .PlatformEnemy_Move_var2, 0 ; Stloc_2
    +stack_push_var .PlatformEnemy_Move_var2 ; Ldloc_2
    +branch_false label_PlatformEnemy_Move_106 ; Brfalse_s
    +stack_push_var .PlatformEnemy_Move_this ; Ldarg_0
    +stack_push_int 1 ; Ldc_i4_1
    +stfld 7 ; Stfld
label_PlatformEnemy_Move_106:    nop ; Nop
label_PlatformEnemy_Move_107:    +stack_return_to_saved_address .PlatformEnemy_Move_ReturnAddress ; Ret
.PlatformEnemy_Move_this !byte 0, 0
.PlatformEnemy_Move_ReturnAddress !byte 0,0
.PlatformEnemy_Move_var0 !byte 0,0
.PlatformEnemy_Move_var1 !byte 0,0
.PlatformEnemy_Move_var2 !byte 0,0


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: get_X
;----------------------------------------
PlatformEnemy_get_X 
    +stack_save_return_adress .PlatformEnemy_get_X_ReturnAddress
    +stack_pull_int .PlatformEnemy_get_X_this
    nop ; Nop
    +stack_push_var .PlatformEnemy_get_X_this ; Ldarg_0
    +ldfld 0 ; Ldfld
    +stack_pull_int_ref .PlatformEnemy_get_X_var0, 0 ; Stloc_0
    jmp label_PlatformEnemy_get_X_10 ; Br_s
label_PlatformEnemy_get_X_10:    +stack_push_var .PlatformEnemy_get_X_var0 ; Ldloc_0
    +stack_return_to_saved_address .PlatformEnemy_get_X_ReturnAddress ; Ret
.PlatformEnemy_get_X_this !byte 0, 0
.PlatformEnemy_get_X_ReturnAddress !byte 0,0
.PlatformEnemy_get_X_var0 !byte 0,0


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: set_X
;----------------------------------------
PlatformEnemy_set_X 
    +stack_save_return_adress .PlatformEnemy_set_X_ReturnAddress
    +stack_pull_int_ref .PlatformEnemy_set_X_value, 0
    +stack_pull_int .PlatformEnemy_set_X_this
    nop ; Nop
    +stack_push_var .PlatformEnemy_set_X_this ; Ldarg_0
    +stack_push_var .PlatformEnemy_set_X_value ; Ldarg_1
    +stfld 0 ; Stfld
    +stack_push_var .PlatformEnemy_set_X_this ; Ldarg_0
    +ldfld 8 ; Ldfld
    +stack_push_var .PlatformEnemy_set_X_value ; Ldarg_1
    jsr Sprite_set_X ; Callvirt
    nop ; Nop
    +stack_return_to_saved_address .PlatformEnemy_set_X_ReturnAddress ; Ret
.PlatformEnemy_set_X_this !byte 0, 0
.PlatformEnemy_set_X_value !byte 0, 0
.PlatformEnemy_set_X_ReturnAddress !byte 0,0


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: get_Y
;----------------------------------------
PlatformEnemy_get_Y 
    +stack_save_return_adress .PlatformEnemy_get_Y_ReturnAddress
    +stack_pull_int .PlatformEnemy_get_Y_this
    nop ; Nop
    +stack_push_var .PlatformEnemy_get_Y_this ; Ldarg_0
    +ldfld 1 ; Ldfld
    +stack_pull_int_ref .PlatformEnemy_get_Y_var0, 0 ; Stloc_0
    jmp label_PlatformEnemy_get_Y_10 ; Br_s
label_PlatformEnemy_get_Y_10:    +stack_push_var .PlatformEnemy_get_Y_var0 ; Ldloc_0
    +stack_return_to_saved_address .PlatformEnemy_get_Y_ReturnAddress ; Ret
.PlatformEnemy_get_Y_this !byte 0, 0
.PlatformEnemy_get_Y_ReturnAddress !byte 0,0
.PlatformEnemy_get_Y_var0 !byte 0,0


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: set_Y
;----------------------------------------
PlatformEnemy_set_Y 
    +stack_save_return_adress .PlatformEnemy_set_Y_ReturnAddress
    +stack_pull_int_ref .PlatformEnemy_set_Y_value, 0
    +stack_pull_int .PlatformEnemy_set_Y_this
    nop ; Nop
    +stack_push_var .PlatformEnemy_set_Y_this ; Ldarg_0
    +stack_push_var .PlatformEnemy_set_Y_value ; Ldarg_1
    +stfld 1 ; Stfld
    +stack_push_var .PlatformEnemy_set_Y_this ; Ldarg_0
    +ldfld 8 ; Ldfld
    +stack_push_var .PlatformEnemy_set_Y_value ; Ldarg_1
    jsr Sprite_set_Y ; Callvirt
    nop ; Nop
    +stack_return_to_saved_address .PlatformEnemy_set_Y_ReturnAddress ; Ret
.PlatformEnemy_set_Y_this !byte 0, 0
.PlatformEnemy_set_Y_value !byte 0, 0
.PlatformEnemy_set_Y_ReturnAddress !byte 0,0


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: get_MinX
;----------------------------------------
PlatformEnemy_get_MinX 
    +stack_save_return_adress .PlatformEnemy_get_MinX_ReturnAddress
    +stack_pull_int .PlatformEnemy_get_MinX_this
    +stack_push_var .PlatformEnemy_get_MinX_this ; Ldarg_0
    +ldfld 5 ; Ldfld
    +stack_return_to_saved_address .PlatformEnemy_get_MinX_ReturnAddress ; Ret
.PlatformEnemy_get_MinX_this !byte 0, 0
.PlatformEnemy_get_MinX_ReturnAddress !byte 0,0


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: set_MinX
;----------------------------------------
PlatformEnemy_set_MinX 
    +stack_save_return_adress .PlatformEnemy_set_MinX_ReturnAddress
    +stack_pull_int_ref .PlatformEnemy_set_MinX_value, 0
    +stack_pull_int .PlatformEnemy_set_MinX_this
    +stack_push_var .PlatformEnemy_set_MinX_this ; Ldarg_0
    +stack_push_var .PlatformEnemy_set_MinX_value ; Ldarg_1
    +stfld 5 ; Stfld
    +stack_return_to_saved_address .PlatformEnemy_set_MinX_ReturnAddress ; Ret
.PlatformEnemy_set_MinX_this !byte 0, 0
.PlatformEnemy_set_MinX_value !byte 0, 0
.PlatformEnemy_set_MinX_ReturnAddress !byte 0,0


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: get_MaxX
;----------------------------------------
PlatformEnemy_get_MaxX 
    +stack_save_return_adress .PlatformEnemy_get_MaxX_ReturnAddress
    +stack_pull_int .PlatformEnemy_get_MaxX_this
    +stack_push_var .PlatformEnemy_get_MaxX_this ; Ldarg_0
    +ldfld 6 ; Ldfld
    +stack_return_to_saved_address .PlatformEnemy_get_MaxX_ReturnAddress ; Ret
.PlatformEnemy_get_MaxX_this !byte 0, 0
.PlatformEnemy_get_MaxX_ReturnAddress !byte 0,0


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: set_MaxX
;----------------------------------------
PlatformEnemy_set_MaxX 
    +stack_save_return_adress .PlatformEnemy_set_MaxX_ReturnAddress
    +stack_pull_int_ref .PlatformEnemy_set_MaxX_value, 0
    +stack_pull_int .PlatformEnemy_set_MaxX_this
    +stack_push_var .PlatformEnemy_set_MaxX_this ; Ldarg_0
    +stack_push_var .PlatformEnemy_set_MaxX_value ; Ldarg_1
    +stfld 6 ; Stfld
    +stack_return_to_saved_address .PlatformEnemy_set_MaxX_ReturnAddress ; Ret
.PlatformEnemy_set_MaxX_this !byte 0, 0
.PlatformEnemy_set_MaxX_value !byte 0, 0
.PlatformEnemy_set_MaxX_ReturnAddress !byte 0,0


;----------------------------------------
; TYPE: Platform
; METHOD: Draw
;----------------------------------------
Platform_Draw 
    +stack_save_return_adress .Platform_Draw_ReturnAddress
    +stack_pull_int .Platform_Draw_this
    nop ; Nop
    +stack_push_var .Platform_Draw_this ; Ldarg_0
    +ldfld 0 ; Ldfld
    +stack_push_int 1 ; Ldc_i4_1
    +add ; Add
    +stack_pull_int_ref .Platform_Draw_var0, 0 ; Stloc_0
    +stack_push_var .Platform_Draw_this ; Ldarg_0
    +ldfld 0 ; Ldfld
    +stack_push_var .Platform_Draw_this ; Ldarg_0
    +ldfld 5 ; Ldfld
    +add ; Add
    +stack_pull_int_ref .Platform_Draw_var1, 0 ; Stloc_1
    +stack_push_var .Platform_Draw_this ; Ldarg_0
    +ldfld 1 ; Ldfld
    +stack_pull_int_ref .Platform_Draw_var2, 0 ; Stloc_2
    +stack_push_var .Platform_Draw_this ; Ldarg_0
    +ldfld 1 ; Ldfld
    +stack_push_int 1 ; Ldc_i4_1
    +add ; Add
    +stack_pull_int_ref .Platform_Draw_var3, 0 ; Stloc_3
    +stack_push_var .Platform_Draw_this ; Ldarg_0
    +stack_push_var .Platform_Draw_var2 ; Ldloc_2
    +stack_push_int 3 ; Ldc_i4_3
    +shl ; Shl
    +stack_push_int 33 ; Ldc_i4_s
    +add ; Add
    +stfld 2 ; Stfld
    +stack_push_var .Platform_Draw_this ; Ldarg_0
    +stack_push_var .Platform_Draw_var0 ; Ldloc_0
    +stack_push_int 3 ; Ldc_i4_3
    +shl ; Shl
    +stack_push_int 3 ; Ldc_i4_3
    +add ; Add
    +stfld 3 ; Stfld
    +stack_push_var .Platform_Draw_this ; Ldarg_0
    +stack_push_var .Platform_Draw_var1 ; Ldloc_1
    +stack_push_int 3 ; Ldc_i4_3
    +shl ; Shl
    +stack_push_int 20 ; Ldc_i4_s
    +add ; Add
    +stfld 4 ; Stfld
    +stack_push_var .Platform_Draw_var0 ; Ldloc_0
    +stack_pull_int_ref .Platform_Draw_var4, 0 ; Stloc_s
    jmp label_Platform_Draw_122 ; Br_s
label_Platform_Draw_80:    nop ; Nop
    +stack_push_var .Platform_Draw_var4 ; Ldloc_s
    +stack_push_var .Platform_Draw_var2 ; Ldloc_2
    +stack_push_int 67 ; Ldc_i4_s
    +stack_push_var .Platform_Draw_this ; Ldarg_0
    +ldfld 6 ; Ldfld
    jsr C64_SetChar ; Call
    nop ; Nop
    +stack_push_var .Platform_Draw_var4 ; Ldloc_s
    +stack_push_var .Platform_Draw_var3 ; Ldloc_3
    +stack_push_int 67 ; Ldc_i4_s
    +stack_push_var .Platform_Draw_this ; Ldarg_0
    +ldfld 6 ; Ldfld
    jsr C64_SetChar ; Call
    nop ; Nop
    nop ; Nop
    +stack_push_var .Platform_Draw_var4 ; Ldloc_s
    +stack_push_int 1 ; Ldc_i4_1
    +add ; Add
    +stack_pull_int_ref .Platform_Draw_var4, 0 ; Stloc_s
label_Platform_Draw_122:    +stack_push_var .Platform_Draw_var4 ; Ldloc_s
    +stack_push_var .Platform_Draw_var1 ; Ldloc_1
    +compareLess ; Clt_un
    +stack_pull_int_ref .Platform_Draw_var5, 0 ; Stloc_s
    +stack_push_var .Platform_Draw_var5 ; Ldloc_s
    +branch_true label_Platform_Draw_80 ; Brtrue_s
    +stack_push_var .Platform_Draw_this ; Ldarg_0
    +ldfld 0 ; Ldfld
    +stack_push_var .Platform_Draw_var2 ; Ldloc_2
    +stack_push_int 85 ; Ldc_i4_s
    +stack_push_var .Platform_Draw_this ; Ldarg_0
    +ldfld 6 ; Ldfld
    jsr C64_SetChar ; Call
    nop ; Nop
    +stack_push_var .Platform_Draw_this ; Ldarg_0
    +ldfld 0 ; Ldfld
    +stack_push_var .Platform_Draw_var3 ; Ldloc_3
    +stack_push_int 74 ; Ldc_i4_s
    +stack_push_var .Platform_Draw_this ; Ldarg_0
    +ldfld 6 ; Ldfld
    jsr C64_SetChar ; Call
    nop ; Nop
    +stack_push_var .Platform_Draw_var1 ; Ldloc_1
    +stack_push_var .Platform_Draw_var2 ; Ldloc_2
    +stack_push_int 73 ; Ldc_i4_s
    +stack_push_var .Platform_Draw_this ; Ldarg_0
    +ldfld 6 ; Ldfld
    jsr C64_SetChar ; Call
    nop ; Nop
    +stack_push_var .Platform_Draw_var1 ; Ldloc_1
    +stack_push_var .Platform_Draw_var3 ; Ldloc_3
    +stack_push_int 75 ; Ldc_i4_s
    +stack_push_var .Platform_Draw_this ; Ldarg_0
    +ldfld 6 ; Ldfld
    jsr C64_SetChar ; Call
    nop ; Nop
    +stack_return_to_saved_address .Platform_Draw_ReturnAddress ; Ret
.Platform_Draw_this !byte 0, 0
.Platform_Draw_ReturnAddress !byte 0,0
.Platform_Draw_var0 !byte 0,0
.Platform_Draw_var1 !byte 0,0
.Platform_Draw_var2 !byte 0,0
.Platform_Draw_var3 !byte 0,0
.Platform_Draw_var4 !byte 0,0
.Platform_Draw_var5 !byte 0,0


;----------------------------------------
; TYPE: Platform
; METHOD: DistanceToPlatform
;----------------------------------------
Platform_DistanceToPlatform 
    +stack_save_return_adress .Platform_DistanceToPlatform_ReturnAddress
    +stack_pull_int_ref .Platform_DistanceToPlatform_y, 0
    +stack_pull_int_ref .Platform_DistanceToPlatform_x, 0
    +stack_pull_int .Platform_DistanceToPlatform_this
    nop ; Nop
    +stack_push_var .Platform_DistanceToPlatform_x ; Ldarg_1
    +stack_push_var .Platform_DistanceToPlatform_this ; Ldarg_0
    +ldfld 3 ; Ldfld
    +compareLess ; Clt_un
    +stack_pull_int_ref .Platform_DistanceToPlatform_var1, 0 ; Stloc_1
    +stack_push_var .Platform_DistanceToPlatform_var1 ; Ldloc_1
    +branch_false label_Platform_DistanceToPlatform_22 ; Brfalse_s
  ; OPT   +stack_push_int 255 ; Ldc_i4
  ; OPT   +stack_pull_int_ref .Platform_DistanceToPlatform_var2, 0 ; Stloc_2
    +init_var .Platform_DistanceToPlatform_var2, 255 ; Nop
    jmp label_Platform_DistanceToPlatform_79 ; Br_s
label_Platform_DistanceToPlatform_22:    +stack_push_var .Platform_DistanceToPlatform_x ; Ldarg_1
    +stack_push_var .Platform_DistanceToPlatform_this ; Ldarg_0
    +ldfld 4 ; Ldfld
    +compareGreater ; Cgt_un
    +stack_pull_int_ref .Platform_DistanceToPlatform_var3, 0 ; Stloc_3
    +stack_push_var .Platform_DistanceToPlatform_var3 ; Ldloc_3
    +branch_false label_Platform_DistanceToPlatform_43 ; Brfalse_s
  ; OPT   +stack_push_int 255 ; Ldc_i4
  ; OPT   +stack_pull_int_ref .Platform_DistanceToPlatform_var2, 0 ; Stloc_2
    +init_var .Platform_DistanceToPlatform_var2, 255 ; Nop
    jmp label_Platform_DistanceToPlatform_79 ; Br_s
label_Platform_DistanceToPlatform_43:    +stack_push_var .Platform_DistanceToPlatform_y ; Ldarg_2
    +stack_push_var .Platform_DistanceToPlatform_this ; Ldarg_0
    +ldfld 2 ; Ldfld
    +compareGreater ; Cgt_un
    +stack_pull_int_ref .Platform_DistanceToPlatform_var4, 0 ; Stloc_s
    +stack_push_var .Platform_DistanceToPlatform_var4 ; Ldloc_s
    +branch_false label_Platform_DistanceToPlatform_66 ; Brfalse_s
  ; OPT   +stack_push_int 255 ; Ldc_i4
  ; OPT   +stack_pull_int_ref .Platform_DistanceToPlatform_var2, 0 ; Stloc_2
    +init_var .Platform_DistanceToPlatform_var2, 255 ; Nop
    jmp label_Platform_DistanceToPlatform_79 ; Br_s
label_Platform_DistanceToPlatform_66:    +stack_push_var .Platform_DistanceToPlatform_this ; Ldarg_0
    +ldfld 2 ; Ldfld
    +stack_push_var .Platform_DistanceToPlatform_y ; Ldarg_2
    +sub ; Sub
    +stack_pull_int_ref .Platform_DistanceToPlatform_var0, 0 ; Stloc_0
    +stack_push_var .Platform_DistanceToPlatform_var0 ; Ldloc_0
    +stack_pull_int_ref .Platform_DistanceToPlatform_var2, 0 ; Stloc_2
    jmp label_Platform_DistanceToPlatform_79 ; Br_s
label_Platform_DistanceToPlatform_79:    +stack_push_var .Platform_DistanceToPlatform_var2 ; Ldloc_2
    +stack_return_to_saved_address .Platform_DistanceToPlatform_ReturnAddress ; Ret
.Platform_DistanceToPlatform_this !byte 0, 0
.Platform_DistanceToPlatform_x !byte 0, 0
.Platform_DistanceToPlatform_y !byte 0, 0
.Platform_DistanceToPlatform_ReturnAddress !byte 0,0
.Platform_DistanceToPlatform_var0 !byte 0,0
.Platform_DistanceToPlatform_var1 !byte 0,0
.Platform_DistanceToPlatform_var2 !byte 0,0
.Platform_DistanceToPlatform_var3 !byte 0,0
.Platform_DistanceToPlatform_var4 !byte 0,0


;----------------------------------------
; TYPE: Player
; METHOD: set_Sprite
;----------------------------------------
Player_set_Sprite 
    +stack_save_return_adress .Player_set_Sprite_ReturnAddress
    +stack_pull_int_ref .Player_set_Sprite_value, 0
    +stack_pull_int .Player_set_Sprite_this
    nop ; Nop
    jsr C64_get_Sprites ; Call
    +stack_push_int 10 ; Ldc_i4_s
    jsr SpriteCollection_set_CommonColor1 ; Callvirt
    nop ; Nop
    jsr C64_get_Sprites ; Call
    +stack_push_int 6 ; Ldc_i4_6
    jsr SpriteCollection_set_CommonColor2 ; Callvirt
    nop ; Nop
    +stack_push_var .Player_set_Sprite_this ; Ldarg_0
    +stack_push_var .Player_set_Sprite_value ; Ldarg_1
    +stfld 5 ; Stfld
    +stack_push_var .Player_set_Sprite_this ; Ldarg_0
    +ldfld 5 ; Ldfld
    +stack_push_int 3 ; Ldc_i4_3
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    +stack_push_var .Player_set_Sprite_this ; Ldarg_0
    +stack_push_int 4 ; Ldc_i4_4
    +stfld 2 ; Stfld
    +stack_push_var .Player_set_Sprite_this ; Ldarg_0
    +ldfld 5 ; Ldfld
    +stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_MultiColor ; Callvirt
    nop ; Nop
    +stack_push_var .Player_set_Sprite_this ; Ldarg_0
    +ldfld 5 ; Ldfld
    +stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_Visible ; Callvirt
    nop ; Nop
    +stack_push_var .Player_set_Sprite_this ; Ldarg_0
    +ldfld 5 ; Ldfld
    +stack_push_int 9 ; Ldc_i4_s
    jsr Sprite_set_Color ; Callvirt
    nop ; Nop
    +stack_push_var .Player_set_Sprite_this ; Ldarg_0
    +stack_push_int 0 ; Ldc_i4_0
    +stfld 3 ; Stfld
    +stack_push_var .Player_set_Sprite_this ; Ldarg_0
    +stack_push_int 200 ; Ldc_i4
    jsr Player_set_Y ; Call
    nop ; Nop
    +stack_push_var .Player_set_Sprite_this ; Ldarg_0
    +stack_push_int 20 ; Ldc_i4_s
    jsr Player_set_X ; Call
    nop ; Nop
    +stack_return_to_saved_address .Player_set_Sprite_ReturnAddress ; Ret
.Player_set_Sprite_this !byte 0, 0
.Player_set_Sprite_value !byte 0, 0
.Player_set_Sprite_ReturnAddress !byte 0,0


;----------------------------------------
; TYPE: Player
; METHOD: get_X
;----------------------------------------
Player_get_X 
    +stack_save_return_adress .Player_get_X_ReturnAddress
    +stack_pull_int .Player_get_X_this
    nop ; Nop
    +stack_push_var .Player_get_X_this ; Ldarg_0
    +ldfld 0 ; Ldfld
    +stack_pull_int_ref .Player_get_X_var0, 0 ; Stloc_0
    jmp label_Player_get_X_10 ; Br_s
label_Player_get_X_10:    +stack_push_var .Player_get_X_var0 ; Ldloc_0
    +stack_return_to_saved_address .Player_get_X_ReturnAddress ; Ret
.Player_get_X_this !byte 0, 0
.Player_get_X_ReturnAddress !byte 0,0
.Player_get_X_var0 !byte 0,0


;----------------------------------------
; TYPE: Player
; METHOD: get_Y
;----------------------------------------
Player_get_Y 
    +stack_save_return_adress .Player_get_Y_ReturnAddress
    +stack_pull_int .Player_get_Y_this
    nop ; Nop
    +stack_push_var .Player_get_Y_this ; Ldarg_0
    +ldfld 1 ; Ldfld
    +stack_pull_int_ref .Player_get_Y_var0, 0 ; Stloc_0
    jmp label_Player_get_Y_10 ; Br_s
label_Player_get_Y_10:    +stack_push_var .Player_get_Y_var0 ; Ldloc_0
    +stack_return_to_saved_address .Player_get_Y_ReturnAddress ; Ret
.Player_get_Y_this !byte 0, 0
.Player_get_Y_ReturnAddress !byte 0,0
.Player_get_Y_var0 !byte 0,0


;----------------------------------------
; TYPE: Player
; METHOD: Move
;----------------------------------------
Player_Move 
    +stack_save_return_adress .Player_Move_ReturnAddress
    +stack_pull_int_ref .Player_Move_distanceToPlatform, 0
    +stack_pull_int .Player_Move_this
    nop ; Nop
    +stack_push_var .Player_Move_this ; Ldarg_0
    +ldfld 5 ; Ldfld
    jsr Sprite_get_IsInCollision ; Callvirt
    +branch_true label_Player_Move_29 ; Brtrue_s
    +stack_push_var .Player_Move_this ; Ldarg_0
    jsr Player_get_Y ; Call
    +stack_push_int 240 ; Ldc_i4
    +compareGreater ; Cgt_un
    jmp label_Player_Move_30 ; Br_s
label_Player_Move_29:    +stack_push_int 1 ; Ldc_i4_1
label_Player_Move_30:    +stack_pull_int_ref .Player_Move_var1, 0 ; Stloc_1
    +stack_push_var .Player_Move_var1 ; Ldloc_1
    +branch_false label_Player_Move_47 ; Brfalse_s
    nop ; Nop
    +stack_push_var .Player_Move_this ; Ldarg_0
    jsr Player_Die ; Call
    nop ; Nop
    jmp label_Player_Move_572 ; Br
label_Player_Move_47:  ; OPT   +stack_push_int 0 ; Ldc_i4_0
  ; OPT   +stack_pull_int_ref .Player_Move_var0, 0 ; Stloc_0
    +init_var .Player_Move_var0, 0 ; Nop
    +stack_push_var .Player_Move_this ; Ldarg_0
    +ldfld 3 ; Ldfld
    +branch_true label_Player_Move_63 ; Brtrue_s
    +stack_push_var .Player_Move_distanceToPlatform ; Ldarg_1
    +stack_push_int 4 ; Ldc_i4_4
    +compareGreater ; Cgt_un
    jmp label_Player_Move_64 ; Br_s
label_Player_Move_63:    +stack_push_int 0 ; Ldc_i4_0
label_Player_Move_64:    +stack_pull_int_ref .Player_Move_var2, 0 ; Stloc_2
    +stack_push_var .Player_Move_var2 ; Ldloc_2
    +branch_false label_Player_Move_70 ; Brfalse_s
  ; OPT   +stack_push_int 1 ; Ldc_i4_1
  ; OPT   +stack_pull_int_ref .Player_Move_var0, 0 ; Stloc_0
    +init_var .Player_Move_var0, 1 ; Nop
label_Player_Move_70:    +stack_push_var .Player_Move_this ; Ldarg_0
    +ldfld 3 ; Ldfld
    +branch_true label_Player_Move_90 ; Brtrue_s
    +stack_push_var .Player_Move_var0 ; Ldloc_0
    +branch_true label_Player_Move_90 ; Brtrue_s
    +stack_push_int 22 ; Ldc_i4_s
    jsr C64_IsKeyPressed ; Call
    jmp label_Player_Move_91 ; Br_s
label_Player_Move_90:    +stack_push_int 0 ; Ldc_i4_0
label_Player_Move_91:    +stack_pull_int_ref .Player_Move_var3, 0 ; Stloc_3
    +stack_push_var .Player_Move_var3 ; Ldloc_3
    +branch_false label_Player_Move_111 ; Brfalse_s
    nop ; Nop
    +stack_push_var .Player_Move_this ; Ldarg_0
    +stack_push_int 1 ; Ldc_i4_1
    +stfld 3 ; Stfld
    +stack_push_var .Player_Move_this ; Ldarg_0
    +stack_push_int 0 ; Ldc_i4_0
    +stfld 4 ; Stfld
    nop ; Nop
label_Player_Move_111:    +stack_push_int 0 ; Ldc_i4_0
    jsr C64_IsKeyPressed ; Call
    +stack_pull_int_ref .Player_Move_var4, 0 ; Stloc_s
    +stack_push_var .Player_Move_var4 ; Ldloc_s
    +branch_false label_Player_Move_299 ; Brfalse
    nop ; Nop
    +stack_push_var .Player_Move_this ; Ldarg_0
    jsr Player_get_X ; Call
    +stack_pull_int_ref .Player_Move_var5, 0 ; Stloc_s
    +stack_push_var .Player_Move_this ; Ldarg_0
    +stack_push_var .Player_Move_var5 ; Ldloc_s
    +stack_push_int 1 ; Ldc_i4_1
    +sub ; Sub
    jsr Player_set_X ; Call
    nop ; Nop
    +stack_push_var .Player_Move_this ; Ldarg_0
    +ldfld 2 ; Ldfld
    +stack_push_int 5 ; Ldc_i4_5
    +compareLess ; Clt
    +stack_pull_int_ref .Player_Move_var6, 0 ; Stloc_s
    +stack_push_var .Player_Move_var6 ; Ldloc_s
    +branch_false label_Player_Move_168 ; Brfalse_s
    +stack_push_var .Player_Move_this ; Ldarg_0
    +stack_push_int 5 ; Ldc_i4_5
    +stfld 2 ; Stfld
label_Player_Move_168:    +stack_push_var .Player_Move_this ; Ldarg_0
    +stack_push_var .Player_Move_this ; Ldarg_0
    +ldfld 2 ; Ldfld
    +stack_push_int 1 ; Ldc_i4_1
    +add ; Add
    +stfld 2 ; Stfld
    +stack_push_var .Player_Move_this ; Ldarg_0
    +ldfld 2 ; Ldfld
    +stack_push_int 9 ; Ldc_i4_s
    +compareEqual ; Ceq
    +stack_pull_int_ref .Player_Move_var7, 0 ; Stloc_s
    +stack_push_var .Player_Move_var7 ; Ldloc_s
    +branch_false label_Player_Move_205 ; Brfalse_s
    +stack_push_var .Player_Move_this ; Ldarg_0
    +stack_push_int 6 ; Ldc_i4_6
    +stfld 2 ; Stfld
label_Player_Move_205:    +stack_push_var .Player_Move_this ; Ldarg_0
    +ldfld 3 ; Ldfld
    +stack_pull_int_ref .Player_Move_var8, 0 ; Stloc_s
    +stack_push_var .Player_Move_var8 ; Ldloc_s
    +branch_false label_Player_Move_245 ; Brfalse_s
    nop ; Nop
    +stack_push_var .Player_Move_this ; Ldarg_0
    jsr Player_get_X ; Call
    +stack_pull_int_ref .Player_Move_var5, 0 ; Stloc_s
    +stack_push_var .Player_Move_this ; Ldarg_0
    +stack_push_var .Player_Move_var5 ; Ldloc_s
    +stack_push_int 1 ; Ldc_i4_1
    +sub ; Sub
    jsr Player_set_X ; Call
    nop ; Nop
    +stack_push_var .Player_Move_this ; Ldarg_0
    +stack_push_int 7 ; Ldc_i4_7
    +stfld 2 ; Stfld
    nop ; Nop
label_Player_Move_245:    +stack_push_var .Player_Move_var0 ; Ldloc_0
    +stack_pull_int_ref .Player_Move_var9, 0 ; Stloc_s
    +stack_push_var .Player_Move_var9 ; Ldloc_s
    +branch_false label_Player_Move_280 ; Brfalse_s
    nop ; Nop
    +stack_push_var .Player_Move_this ; Ldarg_0
    jsr Player_get_X ; Call
    +stack_pull_int_ref .Player_Move_var5, 0 ; Stloc_s
    +stack_push_var .Player_Move_this ; Ldarg_0
    +stack_push_var .Player_Move_var5 ; Ldloc_s
    +stack_push_int 1 ; Ldc_i4_1
    +sub ; Sub
    jsr Player_set_X ; Call
    nop ; Nop
    +stack_push_var .Player_Move_this ; Ldarg_0
    +stack_push_int 7 ; Ldc_i4_7
    +stfld 2 ; Stfld
    nop ; Nop
label_Player_Move_280:    +stack_push_var .Player_Move_this ; Ldarg_0
    +ldfld 5 ; Ldfld
    +stack_push_var .Player_Move_this ; Ldarg_0
    +ldfld 2 ; Ldfld
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    nop ; Nop
label_Player_Move_299:    +stack_push_int 3 ; Ldc_i4_3
    jsr C64_IsKeyPressed ; Call
    +stack_pull_int_ref .Player_Move_var10, 0 ; Stloc_s
    +stack_push_var .Player_Move_var10 ; Ldloc_s
    +branch_false label_Player_Move_464 ; Brfalse
    nop ; Nop
    +stack_push_var .Player_Move_this ; Ldarg_0
    jsr Player_get_X ; Call
    +stack_pull_int_ref .Player_Move_var5, 0 ; Stloc_s
    +stack_push_var .Player_Move_this ; Ldarg_0
    +stack_push_var .Player_Move_var5 ; Ldloc_s
    +stack_push_int 1 ; Ldc_i4_1
    +add ; Add
    jsr Player_set_X ; Call
    nop ; Nop
    +stack_push_var .Player_Move_this ; Ldarg_0
    +stack_push_var .Player_Move_this ; Ldarg_0
    +ldfld 2 ; Ldfld
    +stack_push_int 1 ; Ldc_i4_1
    +add ; Add
    +stfld 2 ; Stfld
    +stack_push_var .Player_Move_this ; Ldarg_0
    +ldfld 2 ; Ldfld
    +stack_push_int 4 ; Ldc_i4_4
    +compareGreater ; Cgt
    +stack_pull_int_ref .Player_Move_var11, 0 ; Stloc_s
    +stack_push_var .Player_Move_var11 ; Ldloc_s
    +branch_false label_Player_Move_370 ; Brfalse_s
    +stack_push_var .Player_Move_this ; Ldarg_0
    +stack_push_int 1 ; Ldc_i4_1
    +stfld 2 ; Stfld
label_Player_Move_370:    +stack_push_var .Player_Move_this ; Ldarg_0
    +ldfld 3 ; Ldfld
    +stack_pull_int_ref .Player_Move_var12, 0 ; Stloc_s
    +stack_push_var .Player_Move_var12 ; Ldloc_s
    +branch_false label_Player_Move_410 ; Brfalse_s
    nop ; Nop
    +stack_push_var .Player_Move_this ; Ldarg_0
    jsr Player_get_X ; Call
    +stack_pull_int_ref .Player_Move_var5, 0 ; Stloc_s
    +stack_push_var .Player_Move_this ; Ldarg_0
    +stack_push_var .Player_Move_var5 ; Ldloc_s
    +stack_push_int 1 ; Ldc_i4_1
    +add ; Add
    jsr Player_set_X ; Call
    nop ; Nop
    +stack_push_var .Player_Move_this ; Ldarg_0
    +stack_push_int 2 ; Ldc_i4_2
    +stfld 2 ; Stfld
    nop ; Nop
label_Player_Move_410:    +stack_push_var .Player_Move_var0 ; Ldloc_0
    +stack_pull_int_ref .Player_Move_var13, 0 ; Stloc_s
    +stack_push_var .Player_Move_var13 ; Ldloc_s
    +branch_false label_Player_Move_445 ; Brfalse_s
    nop ; Nop
    +stack_push_var .Player_Move_this ; Ldarg_0
    jsr Player_get_X ; Call
    +stack_pull_int_ref .Player_Move_var5, 0 ; Stloc_s
    +stack_push_var .Player_Move_this ; Ldarg_0
    +stack_push_var .Player_Move_var5 ; Ldloc_s
    +stack_push_int 1 ; Ldc_i4_1
    +add ; Add
    jsr Player_set_X ; Call
    nop ; Nop
    +stack_push_var .Player_Move_this ; Ldarg_0
    +stack_push_int 2 ; Ldc_i4_2
    +stfld 2 ; Stfld
    nop ; Nop
label_Player_Move_445:    +stack_push_var .Player_Move_this ; Ldarg_0
    +ldfld 5 ; Ldfld
    +stack_push_var .Player_Move_this ; Ldarg_0
    +ldfld 2 ; Ldfld
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    nop ; Nop
label_Player_Move_464:    +stack_push_var .Player_Move_this ; Ldarg_0
    +ldfld 3 ; Ldfld
    +stack_pull_int_ref .Player_Move_var14, 0 ; Stloc_s
    +stack_push_var .Player_Move_var14 ; Ldloc_s
    +branch_false label_Player_Move_548 ; Brfalse_s
    nop ; Nop
    +stack_push_var .Player_Move_this ; Ldarg_0
    +stack_push_var .Player_Move_this ; Ldarg_0
    +ldfld 4 ; Ldfld
    +stack_push_int 1 ; Ldc_i4_1
    +add ; Add
    +stfld 4 ; Stfld
    +stack_push_var .Player_Move_this ; Ldarg_0
    +stack_push_var .Player_Move_this ; Ldarg_0
    jsr Player_get_Y ; Call
    nop ;Conv_u8 ; Conv_u8
    +stack_push_var .Player_Move_this ; Ldarg_0
    +ldfld 4 ; Ldfld
    +stack_push_int 10 ; Ldc_i4_s
    +branch_less label_Player_Move_512 ; Blt_s
    +stack_push_int 1 ; Ldc_i4_1
    jmp label_Player_Move_513 ; Br_s
label_Player_Move_512:    +stack_push_int 3 ; Ldc_i4_3
label_Player_Move_513:    nop ;Conv_i8 ; Conv_i8
    +sub ; Sub
    nop ;Conv_u4 ; Conv_u4
    jsr Player_set_Y ; Call
    nop ; Nop
    +stack_push_var .Player_Move_this ; Ldarg_0
    +ldfld 4 ; Ldfld
    +stack_push_int 16 ; Ldc_i4_s
    +compareEqual ; Ceq
    +stack_pull_int_ref .Player_Move_var15, 0 ; Stloc_s
    +stack_push_var .Player_Move_var15 ; Ldloc_s
    +branch_false label_Player_Move_547 ; Brfalse_s
    nop ; Nop
    +stack_push_var .Player_Move_this ; Ldarg_0
    +stack_push_int 0 ; Ldc_i4_0
    +stfld 3 ; Stfld
    nop ; Nop
label_Player_Move_547:    nop ; Nop
label_Player_Move_548:    +stack_push_var .Player_Move_var0 ; Ldloc_0
    +stack_pull_int_ref .Player_Move_var16, 0 ; Stloc_s
    +stack_push_var .Player_Move_var16 ; Ldloc_s
    +branch_false label_Player_Move_572 ; Brfalse_s
    nop ; Nop
    +stack_push_var .Player_Move_this ; Ldarg_0
    +stack_push_var .Player_Move_this ; Ldarg_0
    jsr Player_get_Y ; Call
    +stack_push_int 3 ; Ldc_i4_3
    +add ; Add
    jsr Player_set_Y ; Call
    nop ; Nop
    nop ; Nop
label_Player_Move_572:    +stack_return_to_saved_address .Player_Move_ReturnAddress ; Ret
.Player_Move_this !byte 0, 0
.Player_Move_distanceToPlatform !byte 0, 0
.Player_Move_ReturnAddress !byte 0,0
.Player_Move_var0 !byte 0,0
.Player_Move_var1 !byte 0,0
.Player_Move_var2 !byte 0,0
.Player_Move_var3 !byte 0,0
.Player_Move_var4 !byte 0,0
.Player_Move_var5 !byte 0,0
.Player_Move_var6 !byte 0,0
.Player_Move_var7 !byte 0,0
.Player_Move_var8 !byte 0,0
.Player_Move_var9 !byte 0,0
.Player_Move_var10 !byte 0,0
.Player_Move_var11 !byte 0,0
.Player_Move_var12 !byte 0,0
.Player_Move_var13 !byte 0,0
.Player_Move_var14 !byte 0,0
.Player_Move_var15 !byte 0,0
.Player_Move_var16 !byte 0,0


;----------------------------------------
; TYPE: Player
; METHOD: set_X
;----------------------------------------
Player_set_X 
    +stack_save_return_adress .Player_set_X_ReturnAddress
    +stack_pull_int_ref .Player_set_X_value, 0
    +stack_pull_int .Player_set_X_this
    nop ; Nop
    +stack_push_var .Player_set_X_this ; Ldarg_0
    +stack_push_var .Player_set_X_value ; Ldarg_1
    +stfld 0 ; Stfld
    +stack_push_var .Player_set_X_this ; Ldarg_0
    +ldfld 5 ; Ldfld
    +stack_push_var .Player_set_X_value ; Ldarg_1
    jsr Sprite_set_X ; Callvirt
    nop ; Nop
    +stack_return_to_saved_address .Player_set_X_ReturnAddress ; Ret
.Player_set_X_this !byte 0, 0
.Player_set_X_value !byte 0, 0
.Player_set_X_ReturnAddress !byte 0,0


;----------------------------------------
; TYPE: Player
; METHOD: set_Y
;----------------------------------------
Player_set_Y 
    +stack_save_return_adress .Player_set_Y_ReturnAddress
    +stack_pull_int_ref .Player_set_Y_value, 0
    +stack_pull_int .Player_set_Y_this
    nop ; Nop
    +stack_push_var .Player_set_Y_this ; Ldarg_0
    +stack_push_var .Player_set_Y_value ; Ldarg_1
    +stfld 1 ; Stfld
    +stack_push_var .Player_set_Y_this ; Ldarg_0
    +ldfld 5 ; Ldfld
    +stack_push_var .Player_set_Y_value ; Ldarg_1
    jsr Sprite_set_Y ; Callvirt
    nop ; Nop
    +stack_return_to_saved_address .Player_set_Y_ReturnAddress ; Ret
.Player_set_Y_this !byte 0, 0
.Player_set_Y_value !byte 0, 0
.Player_set_Y_ReturnAddress !byte 0,0


;----------------------------------------
; TYPE: Player
; METHOD: Die
;----------------------------------------
Player_Die 
    +stack_save_return_adress .Player_Die_ReturnAddress
    +stack_pull_int .Player_Die_this
    nop ; Nop
    +stack_push_int 2 ; Ldc_i4_2
    jsr C64_SetBorderColor ; Call
    nop ; Nop
  ; OPT   +stack_push_int 0 ; Ldc_i4_0
  ; OPT   +stack_pull_int_ref .Player_Die_var0, 0 ; Stloc_0
    +init_var .Player_Die_var0, 0 ; Nop
    jmp label_Player_Die_34 ; Br_s
label_Player_Die_12:  ; OPT   +stack_push_int 0 ; Ldc_i4_0
  ; OPT   +stack_pull_int_ref .Player_Die_var1, 0 ; Stloc_1
    +init_var .Player_Die_var1, 0 ; Nop
    jmp label_Player_Die_21 ; Br_s
label_Player_Die_16:    nop ; Nop
  ; OPT   +stack_push_var .Player_Die_var1 ; Ldloc_1
  ; OPT   +stack_push_int 1 ; Ldc_i4_1
  ; OPT   +add ; Add
  ; OPT   +stack_pull_int_ref .Player_Die_var1, 0 ; Stloc_1
    +inc_var .Player_Die_var1 ; Nop
label_Player_Die_21:  ; OPT   +stack_push_var .Player_Die_var1 ; Ldloc_1
  ; OPT   +stack_push_int 100 ; Ldc_i4_s
  ; OPT   +compareLess ; Clt
  ; OPT   +stack_pull_int_ref .Player_Die_var2, 0 ; Stloc_2
  ; OPT   +stack_push_var .Player_Die_var2 ; Ldloc_2
  ; OPT   +branch_true label_Player_Die_16 ; Brtrue_s
    +branch_if_var_less .Player_Die_var1, 100, label_Player_Die_16 ; Nop
  ; OPT   +stack_push_var .Player_Die_var0 ; Ldloc_0
  ; OPT   +stack_push_int 1 ; Ldc_i4_1
  ; OPT   +add ; Add
  ; OPT   +stack_pull_int_ref .Player_Die_var0, 0 ; Stloc_0
    +inc_var .Player_Die_var0 ; Nop
label_Player_Die_34:  ; OPT   +stack_push_var .Player_Die_var0 ; Ldloc_0
  ; OPT   +stack_push_int 100 ; Ldc_i4_s
  ; OPT   +compareLess ; Clt
  ; OPT   +stack_pull_int_ref .Player_Die_var3, 0 ; Stloc_3
  ; OPT   +stack_push_var .Player_Die_var3 ; Ldloc_3
  ; OPT   +branch_true label_Player_Die_12 ; Brtrue_s
    +branch_if_var_less .Player_Die_var0, 100, label_Player_Die_12 ; Nop
    +stack_push_int 0 ; Ldc_i4_0
    jsr C64_SetBorderColor ; Call
    nop ; Nop
    +stack_push_var .Player_Die_this ; Ldarg_0
    +stack_push_int 200 ; Ldc_i4
    jsr Player_set_Y ; Call
    nop ; Nop
    +stack_push_var .Player_Die_this ; Ldarg_0
    +stack_push_int 20 ; Ldc_i4_s
    jsr Player_set_X ; Call
    nop ; Nop
    +stack_return_to_saved_address .Player_Die_ReturnAddress ; Ret
.Player_Die_this !byte 0, 0
.Player_Die_ReturnAddress !byte 0,0
.Player_Die_var0 !byte 0,0
.Player_Die_var1 !byte 0,0
.Player_Die_var2 !byte 0,0
.Player_Die_var3 !byte 0,0
.Program_field_x !byte 0


;----------------------------------------
; TYPE: Demo.Program
; METHOD: OnInterrupt
;----------------------------------------
Program_OnInterrupt 
    +stack_save_return_adress .Program_OnInterrupt_ReturnAddress
    +stack_pull_int_ref .Program_OnInterrupt_args, 1
    +stack_pull_int_ref .Program_OnInterrupt_sender, 1
    nop ; Nop
    +stack_push_var .Program_field_x ; Ldsfld
    +stack_push_int 1 ; Ldc_i4_1
    +add ; Add
    +stack_pull_int_ref .Program_field_x, 0 ; Stsfld
    +stack_push_var .Program_field_x ; Ldsfld
    jsr C64_SetBorderColor ; Call
    nop ; Nop
    +deref .Program_OnInterrupt_sender ; Nop
    +deref .Program_OnInterrupt_args ; Nop
    +stack_return_to_saved_address .Program_OnInterrupt_ReturnAddress ; Ret
.Program_OnInterrupt_sender !byte 0, 0
.Program_OnInterrupt_args !byte 0, 0
.Program_OnInterrupt_ReturnAddress !byte 0,0


;----------------------------------------
; TYPE: Demo.Program
; METHOD: Main
;----------------------------------------
Program_Main 
    +stack_save_return_adress .Program_Main_ReturnAddress
    nop ; Nop
    jsr GCTest_Start ; Call
    nop ; Nop
    +stack_return_to_saved_address .Program_Main_ReturnAddress ; Ret
.Program_Main_ReturnAddress !byte 0,0
.string_1879048193 !pet "not alive",0
.string_1879048213 !pet "a1 still alive!",0
.string_1879048245 !pet "a2 not alive!",0
.string_1879048273 !pet "a2 corrupted",0
.string_1879048299 !pet "b3.child.child.id != 4",0
.string_1879048345 !pet "b3.id != 4",0
.string_1879048367 !pet "b3.child.id != 5",0
.string_1879048401 !pet "finished",0
