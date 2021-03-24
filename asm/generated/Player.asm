

;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: set_Sprite
;----------------------------------------
Player_set_Sprite 
    #init_locals_pull_parameters 0, [0,0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stfld 9 ; Stfld
    #setfld 1, 2, 9 ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: Init
;----------------------------------------
Player_Init 
    #init_locals_pull_parameters 0, [0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 9 ; Ldfld
    #pushfld 9 ; Nop
  ; OPT   #stack_push_pointer string_1879048517 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_right_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 9 ; Ldfld
    #pushfld 9 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_MultiColor ; Callvirt
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 9 ; Ldfld
    #pushfld 9 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_Visible ; Callvirt
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 9 ; Ldfld
    #pushfld 9 ; Nop
    #stack_push_int 5 ; Ldc_i4_5
    jsr Sprite_set_Color ; Callvirt
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 6 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_InitJumpOffsets ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 117 ; Ldc_i4_s
    jsr Player_set_Y ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 40 ; Ldc_i4_s
    jsr Player_set_X ; Call
    nop ; Nop
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: Move
;----------------------------------------
Player_Move 
    #init_locals_pull_parameters 14, [0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 9 ; Ldfld
    #pushfld 9 ; Nop
    jsr Sprite_get_IsInCollision ; Callvirt
    #locals_pull_value_8 2, 0 ; Stloc_0
    #locals_push_value_8 2 ; Ldloc_0
    #branch_false Player_Move_18 ; Brfalse_s
    nop ; Nop
    nop ; Nop
Player_Move_18:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 8 ; Ldfld
    #pushfld 8 ; Nop
    #locals_pull_value_8 3, 0 ; Stloc_1
    #locals_push_value_8 3 ; Ldloc_1
    #branch_false Player_Move_33 ; Brfalse_s
    jmp Player_Move_508 ; Br
Player_Move_33:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 6 ; Ldfld
    #pushfld 6 ; Nop
    #branch_true Player_Move_50 ; Brtrue_s
    #stack_push_int 22 ; Ldc_i4_s
    jsr C64_IsKeyPressed ; Call
    jmp Player_Move_51 ; Br_s
Player_Move_50:    #stack_push_int 0 ; Ldc_i4_0
Player_Move_51:    #locals_pull_value_8 4, 0 ; Stloc_2
    #locals_push_value_8 4 ; Ldloc_2
    #branch_false Player_Move_71 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 6 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 4 ; Stfld
    nop ; Nop
Player_Move_71:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 6 ; Ldfld
    #pushfld 6 ; Nop
    #locals_pull_value_8 5, 0 ; Stloc_3
    #locals_push_value_8 5 ; Ldloc_3
    #branch_false Player_Move_145 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 117 ; Ldc_i4_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld 4 ; Nop
    #ldelem ; Ldelem_i4
    #sub ; Sub
    jsr Player_set_Y ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld 4 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #stfld 4 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld 4 ; Nop
    #stack_push_int 16 ; Ldc_i4_s
    #compareEqual ; Ceq
    #locals_pull_value_8 6, 0 ; Stloc_s
    #locals_push_value_8 6 ; Ldloc_s
    #branch_false Player_Move_144 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 6 ; Stfld
    nop ; Nop
Player_Move_144:    nop ; Nop
Player_Move_145:    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_IsKeyPressed ; Call
    #locals_pull_value_8 7, 0 ; Stloc_s
    #locals_push_value_8 7 ; Ldloc_s
    #branch_false Player_Move_243 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 5 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_X ; Call
    #stack_push_int 2 ; Ldc_i4_2
    #sub ; Sub
    jsr Player_set_X ; Call
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 7 ; Ldfld
    #pushfld 7 ; Nop
    #branch_false Player_Move_199 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_X ; Call
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    jmp Player_Move_200 ; Br_s
Player_Move_199:    #stack_push_int 0 ; Ldc_i4_0
Player_Move_200:    #locals_pull_value_8 8, 0 ; Stloc_s
    #locals_push_value_8 8 ; Ldloc_s
    #branch_false Player_Move_228 ; Brfalse_s
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 9 ; Ldfld
    #pushfld 9 ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    jsr Sprite_set_HighPosition ; Callvirt
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 7 ; Stfld
    nop ; Nop
Player_Move_228:    #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #stfld 3 ; Stfld
    nop ; Nop
Player_Move_243:    #stack_push_int 3 ; Ldc_i4_3
    jsr C64_IsKeyPressed ; Call
    #locals_pull_value_8 9, 0 ; Stloc_s
    #locals_push_value_8 9 ; Ldloc_s
    #branch_false Player_Move_341 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 5 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_X ; Call
    #stack_push_int 2 ; Ldc_i4_2
    #add ; Add
    jsr Player_set_X ; Call
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 7 ; Ldfld
    #pushfld 7 ; Nop
    #branch_true Player_Move_297 ; Brtrue_s
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_X ; Call
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    jmp Player_Move_298 ; Br_s
Player_Move_297:    #stack_push_int 0 ; Ldc_i4_0
Player_Move_298:    #locals_pull_value_8 10, 0 ; Stloc_s
    #locals_push_value_8 10 ; Ldloc_s
    #branch_false Player_Move_326 ; Brfalse_s
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 9 ; Ldfld
    #pushfld 9 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_HighPosition ; Callvirt
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 7 ; Stfld
    nop ; Nop
Player_Move_326:    #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #stfld 3 ; Stfld
    nop ; Nop
Player_Move_341:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 4 ; Ldc_i4_4
    #compareEqual ; Ceq
    #locals_pull_value_8 11, 0 ; Stloc_s
    #locals_push_value_8 11 ; Ldloc_s
    #branch_false Player_Move_363 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 3 ; Stfld
Player_Move_363:    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_Y ; Call
    #stack_push_int 117 ; Ldc_i4_s
    #compareEqual ; Ceq
    #locals_pull_value_8 12, 0 ; Stloc_s
    #locals_push_value_8 12 ; Ldloc_s
    #branch_false Player_Move_501 ; Brfalse_s
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
    #stack_push_int 92 ; Ldc_i4_s
    #branch_less_equal_unsigned Player_Move_402 ; Ble_un_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
    #stack_push_int 114 ; Ldc_i4_s
    #compareLess ; Clt_un
    jmp Player_Move_403 ; Br_s
Player_Move_402:    #stack_push_int 0 ; Ldc_i4_0
Player_Move_403:    #locals_pull_value_8 13, 0 ; Stloc_s
    #locals_push_value_8 13 ; Ldloc_s
    #branch_false Player_Move_416 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_Die ; Call
    nop ; Nop
Player_Move_416:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
    #stack_push_int 164 ; Ldc_i4
    #branch_less_equal_unsigned Player_Move_444 ; Ble_un_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
    #stack_push_int 186 ; Ldc_i4
    #compareLess ; Clt_un
    jmp Player_Move_445 ; Br_s
Player_Move_444:    #stack_push_int 0 ; Ldc_i4_0
Player_Move_445:    #locals_pull_value_8 14, 0 ; Stloc_s
    #locals_push_value_8 14 ; Ldloc_s
    #branch_false Player_Move_458 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_Die ; Call
    nop ; Nop
Player_Move_458:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
    #stack_push_int 236 ; Ldc_i4
    #branch_less_equal_unsigned Player_Move_486 ; Ble_un_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
    #stack_push_int 255 ; Ldc_i4
    #compareLess ; Clt_un
    jmp Player_Move_487 ; Br_s
Player_Move_486:    #stack_push_int 0 ; Ldc_i4_0
Player_Move_487:    #locals_pull_value_8 15, 0 ; Stloc_s
    #locals_push_value_8 15 ; Ldloc_s
    #branch_false Player_Move_500 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_Die ; Call
    nop ; Nop
Player_Move_500:    nop ; Nop
Player_Move_501:    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_SetFrame ; Call
    nop ; Nop
Player_Move_508:    #method_exit 17, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: get_X
;----------------------------------------
Player_get_X 
    #init_locals_pull_parameters 1, [0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
  ; OPT   jmp Player_get_X_10 ; Br_s
Player_get_X_10:  ; OPT   #locals_push_value_8 2 ; Ldloc_0
    #pushfld 1 ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: set_X
;----------------------------------------
Player_set_X 
    #init_locals_pull_parameters 0, [0,0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stfld 1 ; Stfld
    #setfld 1, 2, 1 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 9 ; Ldfld
    #pushfld 9 ; Nop
    #locals_push_value_8 2 ; Ldarg_1
    jsr Sprite_set_X ; Callvirt
    nop ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: get_Y
;----------------------------------------
Player_get_Y 
    #init_locals_pull_parameters 1, [0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
  ; OPT   jmp Player_get_Y_10 ; Br_s
Player_get_Y_10:  ; OPT   #locals_push_value_8 2 ; Ldloc_0
    #pushfld 2 ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: set_Y
;----------------------------------------
Player_set_Y 
    #init_locals_pull_parameters 0, [0,0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stfld 2 ; Stfld
    #setfld 1, 2, 2 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 9 ; Ldfld
    #pushfld 9 ; Nop
    #locals_push_value_8 2 ; Ldarg_1
    jsr Sprite_set_Y ; Callvirt
    nop ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: Die
;----------------------------------------
Player_Die 
    #init_locals_pull_parameters 2, [0]
    nop ; Nop
    jmp Player_Die_29 ; Br_s
Player_Die_3:    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_Y ; Call
    #locals_pull_value_8 2, 0 ; Stloc_0
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 2 ; Ldloc_0
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    jsr Player_set_Y ; Call
    nop ; Nop
    #stack_push_int 2 ; Ldc_i4_2
    jsr Delay_Wait ; Call
    nop ; Nop
    nop ; Nop
Player_Die_29:    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_Y ; Call
    #stack_push_int 250 ; Ldc_i4
    #compareLess ; Clt_un
    #locals_pull_value_8 3, 0 ; Stloc_1
    #locals_push_value_8 3 ; Ldloc_1
    #branch_true Player_Die_3 ; Brtrue_s
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_Init ; Call
    nop ; Nop
    #method_exit 5, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: SetFrame
;----------------------------------------
Player_SetFrame 
    #init_locals_pull_parameters 15, [0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 6 ; Ldfld
    #pushfld 6 ; Nop
    #locals_pull_value_8 2, 0 ; Stloc_0
    #locals_push_value_8 2 ; Ldloc_0
    #branch_false Player_SetFrame_234 ; Brfalse
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #locals_pull_value_8 3, 0 ; Stloc_1
    #locals_push_value_8 3 ; Ldloc_1
    #branch_false Player_SetFrame_126 ; Brfalse_s
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld 4 ; Nop
    #stack_push_int 6 ; Ldc_i4_6
    #compareLess ; Clt
    #locals_pull_value_8 4, 0 ; Stloc_2
    #locals_push_value_8 4 ; Ldloc_2
    #branch_false Player_SetFrame_63 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 9 ; Ldfld
    #pushfld 9 ; Nop
  ; OPT   #stack_push_pointer string_1879048555 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_jump_left_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Player_SetFrame_123 ; Br_s
Player_SetFrame_63:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld 4 ; Nop
    #stack_push_int 10 ; Ldc_i4_s
    #compareLess ; Clt
    #locals_pull_value_8 5, 0 ; Stloc_3
    #locals_push_value_8 5 ; Ldloc_3
    #branch_false Player_SetFrame_101 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 9 ; Ldfld
    #pushfld 9 ; Nop
  ; OPT   #stack_push_pointer string_1879048601 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_jump_left_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Player_SetFrame_123 ; Br_s
Player_SetFrame_101:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 9 ; Ldfld
    #pushfld 9 ; Nop
  ; OPT   #stack_push_pointer string_1879048647 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_jump_left_2 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
Player_SetFrame_123:    nop ; Nop
    jmp Player_SetFrame_229 ; Br_s
Player_SetFrame_126:    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld 4 ; Nop
    #stack_push_int 6 ; Ldc_i4_6
    #compareLess ; Clt
    #locals_pull_value_8 6, 0 ; Stloc_s
    #locals_push_value_8 6 ; Ldloc_s
    #branch_false Player_SetFrame_166 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 9 ; Ldfld
    #pushfld 9 ; Nop
  ; OPT   #stack_push_pointer string_1879048693 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_jump_right_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Player_SetFrame_228 ; Br_s
Player_SetFrame_166:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld 4 ; Nop
    #stack_push_int 10 ; Ldc_i4_s
    #compareLess ; Clt
    #locals_pull_value_8 7, 0 ; Stloc_s
    #locals_push_value_8 7 ; Ldloc_s
    #branch_false Player_SetFrame_206 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 9 ; Ldfld
    #pushfld 9 ; Nop
  ; OPT   #stack_push_pointer string_1879048741 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_jump_right_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Player_SetFrame_228 ; Br_s
Player_SetFrame_206:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 9 ; Ldfld
    #pushfld 9 ; Nop
  ; OPT   #stack_push_pointer string_1879048789 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_jump_right_2 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
Player_SetFrame_228:    nop ; Nop
Player_SetFrame_229:    jmp Player_SetFrame_566 ; Br
Player_SetFrame_234:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #locals_pull_value_8 8, 0 ; Stloc_s
    #locals_push_value_8 8 ; Ldloc_s
    #branch_false Player_SetFrame_410 ; Brfalse
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #locals_pull_value_8 9, 0 ; Stloc_s
    #locals_push_value_8 9 ; Ldloc_s
    #branch_false Player_SetFrame_289 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 9 ; Ldfld
    #pushfld 9 ; Nop
  ; OPT   #stack_push_pointer string_1879048837 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_left_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Player_SetFrame_404 ; Br_s
Player_SetFrame_289:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #compareEqual ; Ceq
    #locals_pull_value_8 10, 0 ; Stloc_s
    #locals_push_value_8 10 ; Ldloc_s
    #branch_false Player_SetFrame_328 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 9 ; Ldfld
    #pushfld 9 ; Nop
  ; OPT   #stack_push_pointer string_1879048873 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_left_3 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Player_SetFrame_404 ; Br_s
Player_SetFrame_328:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 2 ; Ldc_i4_2
    #compareEqual ; Ceq
    #locals_pull_value_8 11, 0 ; Stloc_s
    #locals_push_value_8 11 ; Ldloc_s
    #branch_false Player_SetFrame_367 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 9 ; Ldfld
    #pushfld 9 ; Nop
  ; OPT   #stack_push_pointer string_1879048909 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_left_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Player_SetFrame_404 ; Br_s
Player_SetFrame_367:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 3 ; Ldc_i4_3
    #compareEqual ; Ceq
    #locals_pull_value_8 12, 0 ; Stloc_s
    #locals_push_value_8 12 ; Ldloc_s
    #branch_false Player_SetFrame_404 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 9 ; Ldfld
    #pushfld 9 ; Nop
  ; OPT   #stack_push_pointer string_1879048945 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_left_2 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
Player_SetFrame_404:    nop ; Nop
    jmp Player_SetFrame_566 ; Br
Player_SetFrame_410:    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #locals_pull_value_8 13, 0 ; Stloc_s
    #locals_push_value_8 13 ; Ldloc_s
    #branch_false Player_SetFrame_450 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 9 ; Ldfld
    #pushfld 9 ; Nop
  ; OPT   #stack_push_pointer string_1879048517 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_right_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Player_SetFrame_565 ; Br_s
Player_SetFrame_450:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #compareEqual ; Ceq
    #locals_pull_value_8 14, 0 ; Stloc_s
    #locals_push_value_8 14 ; Ldloc_s
    #branch_false Player_SetFrame_489 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 9 ; Ldfld
    #pushfld 9 ; Nop
  ; OPT   #stack_push_pointer string_1879048981 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_right_3 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Player_SetFrame_565 ; Br_s
Player_SetFrame_489:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 2 ; Ldc_i4_2
    #compareEqual ; Ceq
    #locals_pull_value_8 15, 0 ; Stloc_s
    #locals_push_value_8 15 ; Ldloc_s
    #branch_false Player_SetFrame_528 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 9 ; Ldfld
    #pushfld 9 ; Nop
  ; OPT   #stack_push_pointer string_1879049019 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_right_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Player_SetFrame_565 ; Br_s
Player_SetFrame_528:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 3 ; Ldc_i4_3
    #compareEqual ; Ceq
    #locals_pull_value_8 16, 0 ; Stloc_s
    #locals_push_value_8 16 ; Ldloc_s
    #branch_false Player_SetFrame_565 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 9 ; Ldfld
    #pushfld 9 ; Nop
  ; OPT   #stack_push_pointer string_1879049057 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_right_2 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
Player_SetFrame_565:    nop ; Nop
Player_SetFrame_566:    #method_exit 18, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: InitJumpOffsets
;----------------------------------------
Player_InitJumpOffsets 
    #init_locals_pull_parameters 2, [0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #stack_push_int 0 ; Ldnull
    #compareGreater_unsigned ; Cgt_un
    #locals_pull_value_8 2, 0 ; Stloc_0
    #locals_push_value_8 2 ; Ldloc_0
    #branch_false Player_InitJumpOffsets_19 ; Brfalse_s
    jmp Player_InitJumpOffsets_205 ; Br
Player_InitJumpOffsets_19:    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 16 ; Ldc_i4_s
    #newArr  ; Newarr
    #stfld 0 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #stack_push_int 15 ; Ldc_i4_s
    #stack_push_int 0 ; Ldc_i4_0
    #stack_duplicate ; Dup
    #locals_pull_value_8 3, 0 ; Stloc_1
    #stelem ; Stelem_i4
    #locals_push_value_8 3 ; Ldloc_1
    #stelem ; Stelem_i4
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #stack_push_int 14 ; Ldc_i4_s
    #stack_push_int 6 ; Ldc_i4_6
    #stack_duplicate ; Dup
    #locals_pull_value_8 3, 0 ; Stloc_1
    #stelem ; Stelem_i4
    #locals_push_value_8 3 ; Ldloc_1
    #stelem ; Stelem_i4
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #stack_push_int 2 ; Ldc_i4_2
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #stack_push_int 13 ; Ldc_i4_s
    #stack_push_int 11 ; Ldc_i4_s
    #stack_duplicate ; Dup
    #locals_pull_value_8 3, 0 ; Stloc_1
    #stelem ; Stelem_i4
    #locals_push_value_8 3 ; Ldloc_1
    #stelem ; Stelem_i4
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #stack_push_int 3 ; Ldc_i4_3
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #stack_push_int 12 ; Ldc_i4_s
    #stack_push_int 14 ; Ldc_i4_s
    #stack_duplicate ; Dup
    #locals_pull_value_8 3, 0 ; Stloc_1
    #stelem ; Stelem_i4
    #locals_push_value_8 3 ; Ldloc_1
    #stelem ; Stelem_i4
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #stack_push_int 4 ; Ldc_i4_4
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #stack_push_int 11 ; Ldc_i4_s
    #stack_push_int 16 ; Ldc_i4_s
    #stack_duplicate ; Dup
    #locals_pull_value_8 3, 0 ; Stloc_1
    #stelem ; Stelem_i4
    #locals_push_value_8 3 ; Ldloc_1
    #stelem ; Stelem_i4
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #stack_push_int 5 ; Ldc_i4_5
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #stack_push_int 10 ; Ldc_i4_s
    #stack_push_int 18 ; Ldc_i4_s
    #stack_duplicate ; Dup
    #locals_pull_value_8 3, 0 ; Stloc_1
    #stelem ; Stelem_i4
    #locals_push_value_8 3 ; Ldloc_1
    #stelem ; Stelem_i4
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #stack_push_int 6 ; Ldc_i4_6
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #stack_push_int 9 ; Ldc_i4_s
    #stack_push_int 19 ; Ldc_i4_s
    #stack_duplicate ; Dup
    #locals_pull_value_8 3, 0 ; Stloc_1
    #stelem ; Stelem_i4
    #locals_push_value_8 3 ; Ldloc_1
    #stelem ; Stelem_i4
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #stack_push_int 7 ; Ldc_i4_7
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #stack_push_int 8 ; Ldc_i4_8
    #stack_push_int 20 ; Ldc_i4_s
    #stack_duplicate ; Dup
    #locals_pull_value_8 3, 0 ; Stloc_1
    #stelem ; Stelem_i4
    #locals_push_value_8 3 ; Ldloc_1
    #stelem ; Stelem_i4
Player_InitJumpOffsets_205:    #method_exit 5, [] ; Ret
