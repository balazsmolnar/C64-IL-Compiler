

;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: set_Sprite
;----------------------------------------
Player_set_Sprite 
    #init_locals_pull_parameters 0, [0,0]
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 2 ; Ldarg_1
    #stfld 14 ; Stfld
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: Init
;----------------------------------------
Player_Init 
    #init_locals_pull_parameters 1, [1,0]
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
  ; OPT   #stack_push_pointer string_1879048549 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_right_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_MultiColor ; Callvirt
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_Visible ; Callvirt
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
    #stack_push_int 5 ; Ldc_i4_5
    jsr Sprite_set_Color ; Callvirt
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
    #stack_push_int 0 ; Ldc_i4_0
    jsr Sprite_set_HighPosition ; Callvirt
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
    jsr Sprite_get_IsInCollision ; Callvirt
    #locals_pull_value_8 3, 0 ; Stloc_0
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
    jsr Sprite_get_IsInBackgroundCollision ; Callvirt
    #locals_pull_value_8 3, 0 ; Stloc_0
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 13 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 8 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 10 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 2 ; Ldarg_1
    #stfld 1 ; Stfld
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
    #method_exit 5, [2] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: Move
;----------------------------------------
Player_Move 
    #init_locals_pull_parameters 15, [0]
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
    jsr Sprite_get_IsInBackgroundCollision ; Callvirt
    #locals_pull_value_8 2, 0 ; Stloc_0
    #locals_push_value_8 2 ; Ldloc_0
    #branch_false Player_Move_50 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 13 ; Ldfld
    #locals_pull_value_8 3, 0 ; Stloc_1
    #locals_push_value_8 3 ; Ldloc_1
    #branch_false Player_Move_38 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_LevelComplete ; Call
    nop ; Nop
    nop ; Nop
    jmp Player_Move_45 ; Br_s
Player_Move_38:    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_Die ; Call
    nop ; Nop
Player_Move_45:    jmp Player_Move_580 ; Br
Player_Move_50:    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 11 ; Ldfld
    #locals_pull_value_8 4, 0 ; Stloc_2
    #locals_push_value_8 4 ; Ldloc_2
    #branch_false Player_Move_96 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 8 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 0 ; Ldfld
    #ldfld 4 ; Ldfld
    jsr Player_set_X ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 116 ; Ldc_i4_s
    jsr Player_set_Y ; Call
    nop ; Nop
    nop ; Nop
Player_Move_96:    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 8 ; Ldfld
    #branch_true Player_Move_113 ; Brtrue_s
    #stack_push_int 22 ; Ldc_i4_s
    jsr C64_IsKeyPressed ; Call
    jmp Player_Move_114 ; Br_s
Player_Move_113:    #stack_push_int 0 ; Ldc_i4_0
Player_Move_114:    #locals_pull_value_8 5, 0 ; Stloc_3
    #locals_push_value_8 5 ; Ldloc_3
    #branch_false Player_Move_185 ; Brfalse_s
    nop ; Nop
    jsr C64_get_Sound ; Call
    #stack_push_int 64 ; Ldc_i4_s
    #stack_push_int 0 ; Ldc_i4_0
    #stack_push_int 4 ; Ldc_i4_4
    #stack_push_int 40 ; Ldc_i4_s
    #stack_push_int 64 ; Ldc_i4_s
    #stack_push_int 0 ; Ldc_i4_0
    #stack_push_int 9 ; Ldc_i4_s
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sound_PlayEffectReg1 ; Callvirt
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 11 ; Ldfld
    #locals_pull_value_8 6, 0 ; Stloc_s
    #locals_push_value_8 6 ; Ldloc_s
    #branch_false Player_Move_170 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 9 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 11 ; Stfld
    nop ; Nop
Player_Move_170:    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 8 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 6 ; Stfld
    nop ; Nop
Player_Move_185:    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 8 ; Ldfld
    #locals_pull_value_8 7, 0 ; Stloc_s
    #locals_push_value_8 7 ; Ldloc_s
    #branch_false Player_Move_268 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 117 ; Ldc_i4_s
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 2 ; Ldfld
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 6 ; Ldfld
    #ldelem ; Ldelem_i4
    #sub ; Sub
    jsr Player_set_Y ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 6 ; Ldfld
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #stfld 6 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 6 ; Ldfld
    #stack_push_int 16 ; Ldc_i4_s
    #compareEqual ; Ceq
    #locals_pull_value_8 8, 0 ; Stloc_s
    #locals_push_value_8 8 ; Ldloc_s
    #branch_false Player_Move_267 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 8 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 9 ; Stfld
    nop ; Nop
Player_Move_267:    nop ; Nop
Player_Move_268:    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_IsKeyPressed ; Call
    #locals_pull_value_8 9, 0 ; Stloc_s
    #locals_push_value_8 9 ; Ldloc_s
    #branch_false Player_Move_383 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 7 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 11 ; Ldfld
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #locals_pull_value_8 10, 0 ; Stloc_s
    #locals_push_value_8 10 ; Ldloc_s
    #branch_false Player_Move_382 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_X ; Call
    #stack_push_int 2 ; Ldc_i4_2
    #sub ; Sub
    jsr Player_set_X ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 13 ; Ldfld
    #branch_false Player_Move_338 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_X ; Call
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    jmp Player_Move_339 ; Br_s
Player_Move_338:    #stack_push_int 0 ; Ldc_i4_0
Player_Move_339:    #locals_pull_value_8 11, 0 ; Stloc_s
    #locals_push_value_8 11 ; Ldloc_s
    #branch_false Player_Move_367 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
    #stack_push_int 0 ; Ldc_i4_0
    jsr Sprite_set_HighPosition ; Callvirt
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 13 ; Stfld
    nop ; Nop
Player_Move_367:    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 5 ; Ldfld
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #stfld 5 ; Stfld
    nop ; Nop
Player_Move_382:    nop ; Nop
Player_Move_383:    #stack_push_int 3 ; Ldc_i4_3
    jsr C64_IsKeyPressed ; Call
    #locals_pull_value_8 12, 0 ; Stloc_s
    #locals_push_value_8 12 ; Ldloc_s
    #branch_false Player_Move_498 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 7 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 11 ; Ldfld
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #locals_pull_value_8 13, 0 ; Stloc_s
    #locals_push_value_8 13 ; Ldloc_s
    #branch_false Player_Move_497 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_X ; Call
    #stack_push_int 2 ; Ldc_i4_2
    #add ; Add
    jsr Player_set_X ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 13 ; Ldfld
    #branch_true Player_Move_453 ; Brtrue_s
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_X ; Call
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    jmp Player_Move_454 ; Br_s
Player_Move_453:    #stack_push_int 0 ; Ldc_i4_0
Player_Move_454:    #locals_pull_value_8 14, 0 ; Stloc_s
    #locals_push_value_8 14 ; Ldloc_s
    #branch_false Player_Move_482 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_HighPosition ; Callvirt
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 13 ; Stfld
    nop ; Nop
Player_Move_482:    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 5 ; Ldfld
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #stfld 5 ; Stfld
    nop ; Nop
Player_Move_497:    nop ; Nop
Player_Move_498:    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 5 ; Ldfld
    #stack_push_int 4 ; Ldc_i4_4
    #compareEqual ; Ceq
    #locals_pull_value_8 15, 0 ; Stloc_s
    #locals_push_value_8 15 ; Ldloc_s
    #branch_false Player_Move_520 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 5 ; Stfld
Player_Move_520:    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_Y ; Call
    #stack_push_int 117 ; Ldc_i4_s
    #branch_not_equal Player_Move_557 ; Bne_un_s
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 9 ; Ldfld
    #branch_true Player_Move_557 ; Brtrue_s
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 1 ; Ldfld
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 3 ; Ldfld
    jsr Wall_IsHole ; Callvirt
    jmp Player_Move_558 ; Br_s
Player_Move_557:    #stack_push_int 0 ; Ldc_i4_0
Player_Move_558:    #locals_pull_value_8 16, 0 ; Stloc_s
    #locals_push_value_8 16 ; Ldloc_s
    #branch_false Player_Move_573 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_Die ; Call
    nop ; Nop
    nop ; Nop
Player_Move_573:    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_SetFrame ; Call
    nop ; Nop
Player_Move_580:    #method_exit 18, [] ; Ret


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
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 10 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
    #stack_push_int 0 ; Ldc_i4_0
    jsr Sprite_set_Visible ; Callvirt
    nop ; Nop
    #method_exit 5, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: SetOnRope
;----------------------------------------
Player_SetOnRope 
    #init_locals_pull_parameters 1, [1,0]
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 9 ; Ldfld
    #locals_pull_value_8 3, 0 ; Stloc_0
    #locals_push_value_8 3 ; Ldloc_0
    #branch_false Player_SetOnRope_13 ; Brfalse_s
    jmp Player_SetOnRope_27 ; Br_s
Player_SetOnRope_13:    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 2 ; Ldarg_1
    #stfld 0 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 11 ; Stfld
Player_SetOnRope_27:    #method_exit 5, [2] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: get_X
;----------------------------------------
Player_get_X 
    #init_locals_pull_parameters 1, [0]
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 3 ; Ldfld
    #locals_pull_value_8 2, 0 ; Stloc_0
    jmp Player_get_X_10 ; Br_s
Player_get_X_10:    #locals_push_value_8 2 ; Ldloc_0
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: set_X
;----------------------------------------
Player_set_X 
    #init_locals_pull_parameters 0, [0,0]
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 2 ; Ldarg_1
    #stfld 3 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
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
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 4 ; Ldfld
    #locals_pull_value_8 2, 0 ; Stloc_0
    jmp Player_get_Y_10 ; Br_s
Player_get_Y_10:    #locals_push_value_8 2 ; Ldloc_0
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: set_Y
;----------------------------------------
Player_set_Y 
    #init_locals_pull_parameters 0, [0,0]
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 2 ; Ldarg_1
    #stfld 4 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
    #locals_push_value_8 2 ; Ldarg_1
    jsr Sprite_set_Y ; Callvirt
    nop ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: SetFrame
;----------------------------------------
Player_SetFrame 
    #init_locals_pull_parameters 13, [0]
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 11 ; Ldfld
    #locals_pull_value_8 2, 0 ; Stloc_0
    #locals_push_value_8 2 ; Ldloc_0
    #branch_false Player_SetFrame_77 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 7 ; Ldfld
    #locals_pull_value_8 3, 0 ; Stloc_1
    #locals_push_value_8 3 ; Ldloc_1
    #branch_false Player_SetFrame_48 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
  ; OPT   #stack_push_pointer string_1879048587 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_rope_left ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    nop ; Nop
    jmp Player_SetFrame_72 ; Br_s
Player_SetFrame_48:    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
  ; OPT   #stack_push_pointer string_1879048629 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_rope_right ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    nop ; Nop
Player_SetFrame_72:    jmp Player_SetFrame_604 ; Br
Player_SetFrame_77:    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 8 ; Ldfld
    #locals_pull_value_8 4, 0 ; Stloc_2
    #locals_push_value_8 4 ; Ldloc_2
    #branch_false Player_SetFrame_314 ; Brfalse
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 7 ; Ldfld
    #locals_pull_value_8 5, 0 ; Stloc_3
    #locals_push_value_8 5 ; Ldloc_3
    #branch_false Player_SetFrame_206 ; Brfalse_s
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 6 ; Ldfld
    #stack_push_int 6 ; Ldc_i4_6
    #compareLess ; Clt
    #locals_pull_value_8 6, 0 ; Stloc_s
    #locals_push_value_8 6 ; Ldloc_s
    #branch_false Player_SetFrame_141 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
  ; OPT   #stack_push_pointer string_1879048673 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_jump_left_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Player_SetFrame_203 ; Br_s
Player_SetFrame_141:    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 6 ; Ldfld
    #stack_push_int 10 ; Ldc_i4_s
    #compareLess ; Clt
    #locals_pull_value_8 7, 0 ; Stloc_s
    #locals_push_value_8 7 ; Ldloc_s
    #branch_false Player_SetFrame_181 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
  ; OPT   #stack_push_pointer string_1879048719 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_jump_left_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Player_SetFrame_203 ; Br_s
Player_SetFrame_181:    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
  ; OPT   #stack_push_pointer string_1879048765 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_jump_left_2 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
Player_SetFrame_203:    nop ; Nop
    jmp Player_SetFrame_309 ; Br_s
Player_SetFrame_206:    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 6 ; Ldfld
    #stack_push_int 6 ; Ldc_i4_6
    #compareLess ; Clt
    #locals_pull_value_8 8, 0 ; Stloc_s
    #locals_push_value_8 8 ; Ldloc_s
    #branch_false Player_SetFrame_246 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
  ; OPT   #stack_push_pointer string_1879048811 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_jump_right_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Player_SetFrame_308 ; Br_s
Player_SetFrame_246:    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 6 ; Ldfld
    #stack_push_int 10 ; Ldc_i4_s
    #compareLess ; Clt
    #locals_pull_value_8 9, 0 ; Stloc_s
    #locals_push_value_8 9 ; Ldloc_s
    #branch_false Player_SetFrame_286 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
  ; OPT   #stack_push_pointer string_1879048859 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_jump_right_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Player_SetFrame_308 ; Br_s
Player_SetFrame_286:    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
  ; OPT   #stack_push_pointer string_1879048907 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_jump_right_2 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
Player_SetFrame_308:    nop ; Nop
Player_SetFrame_309:    jmp Player_SetFrame_604 ; Br
Player_SetFrame_314:    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 7 ; Ldfld
    #locals_pull_value_8 10, 0 ; Stloc_s
    #locals_push_value_8 10 ; Ldloc_s
    #branch_false Player_SetFrame_469 ; Brfalse
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 5 ; Ldfld
    #locals_pull_value_8 12, 0 ; Stloc_s
    #locals_push_value_8 12 ; Ldloc_s
    #locals_pull_value_8 11, 0 ; Stloc_s
    #locals_push_value_8 11 ; Ldloc_s
    #switch Player_SetFrame_Jump_344 ; Switch
    jmp Player_SetFrame_463 ; Br_s
Player_SetFrame_367:    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
  ; OPT   #stack_push_pointer string_1879048955 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_left_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Player_SetFrame_463 ; Br_s
Player_SetFrame_391:    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
  ; OPT   #stack_push_pointer string_1879048991 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_left_3 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Player_SetFrame_463 ; Br_s
Player_SetFrame_415:    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
  ; OPT   #stack_push_pointer string_1879049027 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_left_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Player_SetFrame_463 ; Br_s
Player_SetFrame_439:    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
  ; OPT   #stack_push_pointer string_1879049063 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_left_2 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Player_SetFrame_463 ; Br_s
Player_SetFrame_463:    nop ; Nop
    jmp Player_SetFrame_604 ; Br
Player_SetFrame_469:    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 5 ; Ldfld
    #locals_pull_value_8 14, 0 ; Stloc_s
    #locals_push_value_8 14 ; Ldloc_s
    #locals_pull_value_8 13, 0 ; Stloc_s
    #locals_push_value_8 13 ; Ldloc_s
    #switch Player_SetFrame_Jump_484 ; Switch
    jmp Player_SetFrame_603 ; Br_s
Player_SetFrame_507:    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
  ; OPT   #stack_push_pointer string_1879048549 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_right_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Player_SetFrame_603 ; Br_s
Player_SetFrame_531:    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
  ; OPT   #stack_push_pointer string_1879049099 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_right_3 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Player_SetFrame_603 ; Br_s
Player_SetFrame_555:    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
  ; OPT   #stack_push_pointer string_1879049137 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_right_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Player_SetFrame_603 ; Br_s
Player_SetFrame_579:    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 14 ; Ldfld
  ; OPT   #stack_push_pointer string_1879049175 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_right_2 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Player_SetFrame_603 ; Br_s
Player_SetFrame_603:    nop ; Nop
Player_SetFrame_604:    #method_exit 16, [] ; Ret
Player_SetFrame_Jump_344 .word  Player_SetFrame_367-1,  Player_SetFrame_391-1,  Player_SetFrame_415-1,  Player_SetFrame_439-1, 0
Player_SetFrame_Jump_484 .word  Player_SetFrame_507-1,  Player_SetFrame_531-1,  Player_SetFrame_555-1,  Player_SetFrame_579-1, 0


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: LevelComplete
;----------------------------------------
Player_LevelComplete 
    #init_locals_pull_parameters 4, [0]
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 12 ; Stfld
    jsr C64_get_Sound ; Call
    #stack_push_int 16 ; Ldc_i4_s
    #stack_push_int 100 ; Ldc_i4_s
    #stack_push_int 72 ; Ldc_i4_s
    #stack_push_int 0 ; Ldc_i4_0
    #stack_push_int 16 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    #stack_push_int 0 ; Ldc_i4_0
    #stack_push_int 0 ; Ldc_i4_0
    jsr Sound_PlayEffectReg2 ; Callvirt
    nop ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    #locals_pull_value_8 2, 0 ; Stloc_0
    jmp Player_LevelComplete_61 ; Br_s
Player_LevelComplete_36:    nop ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    #locals_pull_value_8 3, 0 ; Stloc_1
    jmp Player_LevelComplete_47 ; Br_s
Player_LevelComplete_41:    nop ; Nop
    nop ; Nop
    #locals_push_value_8 3 ; Ldloc_1
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #locals_pull_value_8 3, 0 ; Stloc_1
Player_LevelComplete_47:    #locals_push_value_8 3 ; Ldloc_1
    #stack_push_int 50 ; Ldc_i4_s
    #compareLess ; Clt
    #locals_pull_value_8 4, 0 ; Stloc_2
    #locals_push_value_8 4 ; Ldloc_2
    #branch_true Player_LevelComplete_41 ; Brtrue_s
    nop ; Nop
    #locals_push_value_8 2 ; Ldloc_0
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #locals_pull_value_8 2, 0 ; Stloc_0
Player_LevelComplete_61:    #locals_push_value_8 2 ; Ldloc_0
    #stack_push_int 127 ; Ldc_i4_s
    #compareLess ; Clt
    #locals_pull_value_8 5, 0 ; Stloc_3
    #locals_push_value_8 5 ; Ldloc_3
    #branch_true Player_LevelComplete_36 ; Brtrue_s
    jsr C64_get_Sound ; Call
    #stack_push_int 16 ; Ldc_i4_s
    #stack_push_int 100 ; Ldc_i4_s
    #stack_push_int 18 ; Ldc_i4_s
    #stack_push_int 0 ; Ldc_i4_0
    #stack_push_int 16 ; Ldc_i4_s
    #stack_push_int 10 ; Ldc_i4_s
    #stack_push_int 0 ; Ldc_i4_0
    #stack_push_int 0 ; Ldc_i4_0
    jsr Sound_PlayEffectReg2 ; Callvirt
    nop ; Nop
    #method_exit 7, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: InitJumpOffsets
;----------------------------------------
Player_InitJumpOffsets 
    #init_locals_pull_parameters 2, [0]
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 2 ; Ldfld
    #stack_push_int 0 ; Ldnull
    #compareGreater_unsigned ; Cgt_un
    #locals_pull_value_8 2, 0 ; Stloc_0
    #locals_push_value_8 2 ; Ldloc_0
    #branch_false Player_InitJumpOffsets_19 ; Brfalse_s
    jmp Player_InitJumpOffsets_205 ; Br
Player_InitJumpOffsets_19:    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 16 ; Ldc_i4_s
    #newArr  ; Newarr
    #stfld 2 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 2 ; Ldfld
    #stack_push_int 0 ; Ldc_i4_0
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 2 ; Ldfld
    #stack_push_int 15 ; Ldc_i4_s
    #stack_push_int 0 ; Ldc_i4_0
    #stack_duplicate ; Dup
    #locals_pull_value_8 3, 0 ; Stloc_1
    #stelem ; Stelem_i4
    #locals_push_value_8 3 ; Ldloc_1
    #stelem ; Stelem_i4
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 2 ; Ldfld
    #stack_push_int 1 ; Ldc_i4_1
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 2 ; Ldfld
    #stack_push_int 14 ; Ldc_i4_s
    #stack_push_int 6 ; Ldc_i4_6
    #stack_duplicate ; Dup
    #locals_pull_value_8 3, 0 ; Stloc_1
    #stelem ; Stelem_i4
    #locals_push_value_8 3 ; Ldloc_1
    #stelem ; Stelem_i4
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 2 ; Ldfld
    #stack_push_int 2 ; Ldc_i4_2
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 2 ; Ldfld
    #stack_push_int 13 ; Ldc_i4_s
    #stack_push_int 11 ; Ldc_i4_s
    #stack_duplicate ; Dup
    #locals_pull_value_8 3, 0 ; Stloc_1
    #stelem ; Stelem_i4
    #locals_push_value_8 3 ; Ldloc_1
    #stelem ; Stelem_i4
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 2 ; Ldfld
    #stack_push_int 3 ; Ldc_i4_3
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 2 ; Ldfld
    #stack_push_int 12 ; Ldc_i4_s
    #stack_push_int 14 ; Ldc_i4_s
    #stack_duplicate ; Dup
    #locals_pull_value_8 3, 0 ; Stloc_1
    #stelem ; Stelem_i4
    #locals_push_value_8 3 ; Ldloc_1
    #stelem ; Stelem_i4
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 2 ; Ldfld
    #stack_push_int 4 ; Ldc_i4_4
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 2 ; Ldfld
    #stack_push_int 11 ; Ldc_i4_s
    #stack_push_int 16 ; Ldc_i4_s
    #stack_duplicate ; Dup
    #locals_pull_value_8 3, 0 ; Stloc_1
    #stelem ; Stelem_i4
    #locals_push_value_8 3 ; Ldloc_1
    #stelem ; Stelem_i4
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 2 ; Ldfld
    #stack_push_int 5 ; Ldc_i4_5
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 2 ; Ldfld
    #stack_push_int 10 ; Ldc_i4_s
    #stack_push_int 18 ; Ldc_i4_s
    #stack_duplicate ; Dup
    #locals_pull_value_8 3, 0 ; Stloc_1
    #stelem ; Stelem_i4
    #locals_push_value_8 3 ; Ldloc_1
    #stelem ; Stelem_i4
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 2 ; Ldfld
    #stack_push_int 6 ; Ldc_i4_6
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 2 ; Ldfld
    #stack_push_int 9 ; Ldc_i4_s
    #stack_push_int 19 ; Ldc_i4_s
    #stack_duplicate ; Dup
    #locals_pull_value_8 3, 0 ; Stloc_1
    #stelem ; Stelem_i4
    #locals_push_value_8 3 ; Ldloc_1
    #stelem ; Stelem_i4
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 2 ; Ldfld
    #stack_push_int 7 ; Ldc_i4_7
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 2 ; Ldfld
    #stack_push_int 8 ; Ldc_i4_8
    #stack_push_int 20 ; Ldc_i4_s
    #stack_duplicate ; Dup
    #locals_pull_value_8 3, 0 ; Stloc_1
    #stelem ; Stelem_i4
    #locals_push_value_8 3 ; Ldloc_1
    #stelem ; Stelem_i4
Player_InitJumpOffsets_205:    #method_exit 5, [] ; Ret
