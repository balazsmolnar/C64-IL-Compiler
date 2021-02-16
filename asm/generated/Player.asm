

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
    #stack_push_int 3 ; Ldc_i4_3
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 4 ; Ldc_i4_4
    #stfld 2 ; Stfld
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
    #method_exit 4 ; Ret


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
    #method_exit 4 ; Ret


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
    #method_exit 4 ; Ret


;----------------------------------------
; TYPE: Demo.Player
; METHOD: Move
;----------------------------------------
Player_Move 
    #init_locals_pull_parameters 17, [0,0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    jsr Sprite_get_IsInCollision ; Callvirt
    #branch_true Player_Move_29 ; Brtrue_s
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_Y ; Call
    #stack_push_int 240 ; Ldc_i4
    #compareGreater ; Cgt_un
    jmp Player_Move_30 ; Br_s
Player_Move_29:    #stack_push_int 1 ; Ldc_i4_1
Player_Move_30:    #locals_pull_value_8 4, 0 ; Stloc_1
    #locals_push_value_8 4 ; Ldloc_1
    #branch_false Player_Move_47 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_Die ; Call
    nop ; Nop
    jmp Player_Move_572 ; Br
Player_Move_47:  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_0
    #init_var 3, 0 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #branch_true Player_Move_63 ; Brtrue_s
    #locals_push_value_8 2 ; Ldarg_1
    #stack_push_int 4 ; Ldc_i4_4
    #compareGreater ; Cgt_un
    jmp Player_Move_64 ; Br_s
Player_Move_63:    #stack_push_int 0 ; Ldc_i4_0
Player_Move_64:    #locals_pull_value_8 5, 0 ; Stloc_2
    #locals_push_value_8 5 ; Ldloc_2
    #branch_false Player_Move_70 ; Brfalse_s
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_0
    #init_var 3, 1 ; Nop
Player_Move_70:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #branch_true Player_Move_90 ; Brtrue_s
    #locals_push_value_8 3 ; Ldloc_0
    #branch_true Player_Move_90 ; Brtrue_s
    #stack_push_int 22 ; Ldc_i4_s
    jsr C64_IsKeyPressed ; Call
    jmp Player_Move_91 ; Br_s
Player_Move_90:    #stack_push_int 0 ; Ldc_i4_0
Player_Move_91:    #locals_pull_value_8 6, 0 ; Stloc_3
    #locals_push_value_8 6 ; Ldloc_3
    #branch_false Player_Move_111 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 3 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 4 ; Stfld
    nop ; Nop
Player_Move_111:    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_IsKeyPressed ; Call
    #locals_pull_value_8 7, 0 ; Stloc_s
    #locals_push_value_8 7 ; Ldloc_s
    #branch_false Player_Move_299 ; Brfalse
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_X ; Call
    #locals_pull_value_8 8, 0 ; Stloc_s
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 8 ; Ldloc_s
    #stack_push_int 1 ; Ldc_i4_1
    #sub ; Sub
    jsr Player_set_X ; Call
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 5 ; Ldc_i4_5
    #compareLess ; Clt
    #locals_pull_value_8 9, 0 ; Stloc_s
    #locals_push_value_8 9 ; Ldloc_s
    #branch_false Player_Move_168 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 5 ; Ldc_i4_5
    #stfld 2 ; Stfld
Player_Move_168:    #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #stfld 2 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 9 ; Ldc_i4_s
    #compareEqual ; Ceq
    #locals_pull_value_8 10, 0 ; Stloc_s
    #locals_push_value_8 10 ; Ldloc_s
    #branch_false Player_Move_205 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 6 ; Ldc_i4_6
    #stfld 2 ; Stfld
Player_Move_205:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #locals_pull_value_8 11, 0 ; Stloc_s
    #locals_push_value_8 11 ; Ldloc_s
    #branch_false Player_Move_245 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_X ; Call
    #locals_pull_value_8 8, 0 ; Stloc_s
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 8 ; Ldloc_s
    #stack_push_int 1 ; Ldc_i4_1
    #sub ; Sub
    jsr Player_set_X ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 7 ; Ldc_i4_7
    #stfld 2 ; Stfld
    nop ; Nop
Player_Move_245:    #locals_push_value_8 3 ; Ldloc_0
    #locals_pull_value_8 12, 0 ; Stloc_s
    #locals_push_value_8 12 ; Ldloc_s
    #branch_false Player_Move_280 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_X ; Call
    #locals_pull_value_8 8, 0 ; Stloc_s
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 8 ; Ldloc_s
    #stack_push_int 1 ; Ldc_i4_1
    #sub ; Sub
    jsr Player_set_X ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 7 ; Ldc_i4_7
    #stfld 2 ; Stfld
    nop ; Nop
Player_Move_280:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    nop ; Nop
Player_Move_299:    #stack_push_int 3 ; Ldc_i4_3
    jsr C64_IsKeyPressed ; Call
    #locals_pull_value_8 13, 0 ; Stloc_s
    #locals_push_value_8 13 ; Ldloc_s
    #branch_false Player_Move_464 ; Brfalse
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_X ; Call
    #locals_pull_value_8 8, 0 ; Stloc_s
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 8 ; Ldloc_s
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    jsr Player_set_X ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
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
    #compareGreater ; Cgt
    #locals_pull_value_8 14, 0 ; Stloc_s
    #locals_push_value_8 14 ; Ldloc_s
    #branch_false Player_Move_370 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 2 ; Stfld
Player_Move_370:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #locals_pull_value_8 15, 0 ; Stloc_s
    #locals_push_value_8 15 ; Ldloc_s
    #branch_false Player_Move_410 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_X ; Call
    #locals_pull_value_8 8, 0 ; Stloc_s
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 8 ; Ldloc_s
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    jsr Player_set_X ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 2 ; Ldc_i4_2
    #stfld 2 ; Stfld
    nop ; Nop
Player_Move_410:    #locals_push_value_8 3 ; Ldloc_0
    #locals_pull_value_8 16, 0 ; Stloc_s
    #locals_push_value_8 16 ; Ldloc_s
    #branch_false Player_Move_445 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_X ; Call
    #locals_pull_value_8 8, 0 ; Stloc_s
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 8 ; Ldloc_s
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    jsr Player_set_X ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 2 ; Ldc_i4_2
    #stfld 2 ; Stfld
    nop ; Nop
Player_Move_445:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    nop ; Nop
Player_Move_464:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #locals_pull_value_8 17, 0 ; Stloc_s
    #locals_push_value_8 17 ; Ldloc_s
    #branch_false Player_Move_548 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld 4 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #stfld 4 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_Y ; Call
    nop ;Conv_u8 ; Conv_u8
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld 4 ; Nop
    #stack_push_int 10 ; Ldc_i4_s
    #branch_less Player_Move_512 ; Blt_s
    #stack_push_int 1 ; Ldc_i4_1
    jmp Player_Move_513 ; Br_s
Player_Move_512:    #stack_push_int 3 ; Ldc_i4_3
Player_Move_513:    nop ;Conv_i8 ; Conv_i8
    #sub ; Sub
    nop ;Conv_u4 ; Conv_u4
    jsr Player_set_Y ; Call
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld 4 ; Nop
    #stack_push_int 16 ; Ldc_i4_s
    #compareEqual ; Ceq
    #locals_pull_value_8 18, 0 ; Stloc_s
    #locals_push_value_8 18 ; Ldloc_s
    #branch_false Player_Move_547 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 3 ; Stfld
    nop ; Nop
Player_Move_547:    nop ; Nop
Player_Move_548:    #locals_push_value_8 3 ; Ldloc_0
    #locals_pull_value_8 19, 0 ; Stloc_s
    #locals_push_value_8 19 ; Ldloc_s
    #branch_false Player_Move_572 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_Y ; Call
    #stack_push_int 3 ; Ldc_i4_3
    #add ; Add
    jsr Player_set_Y ; Call
    nop ; Nop
    nop ; Nop
Player_Move_572:    #method_exit 21 ; Ret


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
    #method_exit 4 ; Ret


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
    #method_exit 4 ; Ret


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
    #method_exit 7 ; Ret
