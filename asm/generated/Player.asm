

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
