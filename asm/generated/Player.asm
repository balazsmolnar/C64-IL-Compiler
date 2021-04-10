

;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: set_Sprite
;----------------------------------------
Player_set_Sprite 
    #init_locals_pull_parameters 0, [0,0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stfld 14 ; Stfld
    #setfld 1, 2, 14 ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: Init
;----------------------------------------
Player_Init 
    #init_locals_pull_parameters 1, [1,0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
  ; OPT   #stack_push_pointer string_1879048575 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_right_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_MultiColor ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_Visible ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
    #stack_push_int 5 ; Ldc_i4_5
    jsr Sprite_set_Color ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    jsr Sprite_set_HighPosition ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
    jsr Sprite_get_IsInCollision ; Callvirt
    #locals_pull_value_8 3, 0 ; Stloc_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
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
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stfld 1 ; Stfld
    #setfld 1, 2, 1 ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_InitJumpOffsets ; Call
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 117 ; Ldc_i4_s
    jsr Player_set_Y ; Call
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 40 ; Ldc_i4_s
    jsr Player_set_X ; Call
    #method_exit 5, [2] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: Move
;----------------------------------------
Player_Move 
    #init_locals_pull_parameters 0, [0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
    jsr Sprite_get_IsInBackgroundCollision ; Callvirt
    #branch_false Player_Move_35 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 13 ; Ldfld
    #pushfld 13 ; Nop
    #branch_false Player_Move_28 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_LevelComplete ; Call
    #method_exit 3, [] ; Ret
Player_Move_28:    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_Die ; Call
    #method_exit 3, [] ; Ret
Player_Move_35:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 11 ; Ldfld
    #pushfld 11 ; Nop
    #branch_false Player_Move_75 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 8 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #ldfld 4 ; Ldfld
    jsr Player_set_X ; Call
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 116 ; Ldc_i4_s
    jsr Player_set_Y ; Call
Player_Move_75:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 8 ; Ldfld
    #pushfld 8 ; Nop
    #branch_true Player_Move_150 ; Brtrue_s
    #stack_push_int 22 ; Ldc_i4_s
    jsr C64_IsKeyPressed ; Call
    #branch_false Player_Move_150 ; Brfalse_s
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
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 11 ; Ldfld
    #pushfld 11 ; Nop
    #branch_false Player_Move_136 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 9 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 11 ; Stfld
Player_Move_136:    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 8 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 6 ; Stfld
Player_Move_150:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 8 ; Ldfld
    #pushfld 8 ; Nop
    #branch_false Player_Move_218 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 117 ; Ldc_i4_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 6 ; Ldfld
    #pushfld 6 ; Nop
    #ldelem ; Ldelem_i4
    #sub ; Sub
    jsr Player_set_Y ; Call
    #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 6 ; Ldfld
    #pushfld 6 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #stfld 6 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 6 ; Ldfld
    #pushfld 6 ; Nop
  ; OPT   #stack_push_int 16 ; Ldc_i4_s
  ; OPT   #branch_not_equal Player_Move_218 ; Bne_un_s
    #branch_not_equal_const 16, Player_Move_218 ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 8 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 9 ; Stfld
Player_Move_218:    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_IsKeyPressed ; Call
    #branch_false Player_Move_304 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 7 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 11 ; Ldfld
    #pushfld 11 ; Nop
    #branch_true Player_Move_304 ; Brtrue_s
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_X ; Call
    #stack_push_int 2 ; Ldc_i4_2
    #sub ; Sub
    jsr Player_set_X ; Call
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 13 ; Ldfld
    #pushfld 13 ; Nop
    #branch_false Player_Move_290 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_X ; Call
    #branch_true Player_Move_290 ; Brtrue_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    jsr Sprite_set_HighPosition ; Callvirt
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 13 ; Stfld
Player_Move_290:    #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #stfld 5 ; Stfld
Player_Move_304:    #stack_push_int 3 ; Ldc_i4_3
    jsr C64_IsKeyPressed ; Call
    #branch_false Player_Move_390 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 7 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 11 ; Ldfld
    #pushfld 11 ; Nop
    #branch_true Player_Move_390 ; Brtrue_s
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_X ; Call
    #stack_push_int 2 ; Ldc_i4_2
    #add ; Add
    jsr Player_set_X ; Call
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 13 ; Ldfld
    #pushfld 13 ; Nop
    #branch_true Player_Move_376 ; Brtrue_s
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_X ; Call
    #branch_true Player_Move_376 ; Brtrue_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_HighPosition ; Callvirt
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 13 ; Stfld
Player_Move_376:    #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #stfld 5 ; Stfld
Player_Move_390:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
  ; OPT   #stack_push_int 4 ; Ldc_i4_4
  ; OPT   #branch_not_equal Player_Move_406 ; Bne_un_s
    #branch_not_equal_const 4, Player_Move_406 ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 5 ; Stfld
Player_Move_406:    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_Y ; Call
  ; OPT   #stack_push_int 117 ; Ldc_i4_s
  ; OPT   #branch_not_equal Player_Move_449 ; Bne_un_s
    #branch_not_equal_const 117, Player_Move_449 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 9 ; Ldfld
    #pushfld 9 ; Nop
    #branch_true Player_Move_449 ; Brtrue_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    jsr Wall_IsHole ; Callvirt
    #branch_false Player_Move_449 ; Brfalse_s
    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_Die ; Call
Player_Move_449:    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_SetFrame ; Call
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: Die
;----------------------------------------
Player_Die 
    #init_locals_pull_parameters 1, [0]
    jmp Player_Die_24 ; Br_s
Player_Die_2:    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_Y ; Call
    #locals_pull_value_8 2, 0 ; Stloc_0
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 2 ; Ldloc_0
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    jsr Player_set_Y ; Call
    #stack_push_int 2 ; Ldc_i4_2
    jsr Delay_Wait ; Call
Player_Die_24:    #locals_push_value_8 1 ; Ldarg_0
    jsr Player_get_Y ; Call
  ; OPT   #stack_push_int 250 ; Ldc_i4
  ; OPT   #branch_less_unsigned Player_Die_2 ; Blt_un_s
    #branch_less_unsigned_const 250, Player_Die_2 ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 10 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    jsr Sprite_set_Visible ; Callvirt
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: SetOnRope
;----------------------------------------
Player_SetOnRope 
    #init_locals_pull_parameters 0, [1,0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 9 ; Ldfld
    #pushfld 9 ; Nop
    #branch_false Player_SetOnRope_9 ; Brfalse_s
    #method_exit 4, [2] ; Ret
Player_SetOnRope_9:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stfld 0 ; Stfld
    #setfld 1, 2, 0 ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 11 ; Stfld
    #method_exit 4, [2] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: get_X
;----------------------------------------
Player_get_X 
    #init_locals_pull_parameters 0, [0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: set_X
;----------------------------------------
Player_set_X 
    #init_locals_pull_parameters 0, [0,0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stfld 3 ; Stfld
    #setfld 1, 2, 3 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
    #locals_push_value_8 2 ; Ldarg_1
    jsr Sprite_set_X ; Callvirt
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: get_Y
;----------------------------------------
Player_get_Y 
    #init_locals_pull_parameters 0, [0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld 4 ; Nop
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: set_Y
;----------------------------------------
Player_set_Y 
    #init_locals_pull_parameters 0, [0,0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stfld 4 ; Stfld
    #setfld 1, 2, 4 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
    #locals_push_value_8 2 ; Ldarg_1
    jsr Sprite_set_Y ; Callvirt
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: SetFrame
;----------------------------------------
Player_SetFrame 
    #init_locals_pull_parameters 2, [0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 11 ; Ldfld
    #pushfld 11 ; Nop
    #branch_false Player_SetFrame_60 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 7 ; Ldfld
    #pushfld 7 ; Nop
    #branch_false Player_SetFrame_38 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
  ; OPT   #stack_push_pointer string_1879048613 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_rope_left ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_38:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
  ; OPT   #stack_push_pointer string_1879048655 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_rope_right ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_60:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 8 ; Ldfld
    #pushfld 8 ; Nop
    #branch_false Player_SetFrame_249 ; Brfalse
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 7 ; Ldfld
    #pushfld 7 ; Nop
    #branch_false Player_SetFrame_164 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 6 ; Ldfld
    #pushfld 6 ; Nop
  ; OPT   #stack_push_int 6 ; Ldc_i4_6
  ; OPT   #branch_greater_equal Player_SetFrame_110 ; Bge_s
    #branch_greater_equal_const 6, Player_SetFrame_110 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
  ; OPT   #stack_push_pointer string_1879048699 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_jump_left_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_110:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 6 ; Ldfld
    #pushfld 6 ; Nop
  ; OPT   #stack_push_int 10 ; Ldc_i4_s
  ; OPT   #branch_greater_equal Player_SetFrame_142 ; Bge_s
    #branch_greater_equal_const 10, Player_SetFrame_142 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
  ; OPT   #stack_push_pointer string_1879048745 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_jump_left_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_142:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
  ; OPT   #stack_push_pointer string_1879048791 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_jump_left_2 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_164:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 6 ; Ldfld
    #pushfld 6 ; Nop
  ; OPT   #stack_push_int 6 ; Ldc_i4_6
  ; OPT   #branch_greater_equal Player_SetFrame_195 ; Bge_s
    #branch_greater_equal_const 6, Player_SetFrame_195 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
  ; OPT   #stack_push_pointer string_1879048837 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_jump_right_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_195:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 6 ; Ldfld
    #pushfld 6 ; Nop
  ; OPT   #stack_push_int 10 ; Ldc_i4_s
  ; OPT   #branch_greater_equal Player_SetFrame_227 ; Bge_s
    #branch_greater_equal_const 10, Player_SetFrame_227 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
  ; OPT   #stack_push_pointer string_1879048885 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_jump_right_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_227:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
  ; OPT   #stack_push_pointer string_1879048933 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_jump_right_2 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_249:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 7 ; Ldfld
    #pushfld 7 ; Nop
    #branch_false Player_SetFrame_375 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #locals_pull_value_8 2, 0 ; Stloc_0
    #locals_push_value_8 2 ; Ldloc_0
    #switch Player_SetFrame_Jump_265 ; Switch
    #method_exit 5, [] ; Ret
Player_SetFrame_287:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
  ; OPT   #stack_push_pointer string_1879048981 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_left_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_309:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
  ; OPT   #stack_push_pointer string_1879049017 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_left_3 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_331:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
  ; OPT   #stack_push_pointer string_1879049053 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_left_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_353:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
  ; OPT   #stack_push_pointer string_1879049089 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_left_2 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_375:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld 5 ; Nop
    #locals_pull_value_8 3, 0 ; Stloc_1
    #locals_push_value_8 3 ; Ldloc_1
    #switch Player_SetFrame_Jump_383 ; Switch
    #method_exit 5, [] ; Ret
Player_SetFrame_405:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
  ; OPT   #stack_push_pointer string_1879048575 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_right_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_427:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
  ; OPT   #stack_push_pointer string_1879049125 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_right_3 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_449:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
  ; OPT   #stack_push_pointer string_1879049163 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_right_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_471:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 14 ; Ldfld
    #pushfld 14 ; Nop
  ; OPT   #stack_push_pointer string_1879049201 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_right_2 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_Jump_265 .word  Player_SetFrame_287-1,  Player_SetFrame_309-1,  Player_SetFrame_331-1,  Player_SetFrame_353-1, 0
Player_SetFrame_Jump_383 .word  Player_SetFrame_405-1,  Player_SetFrame_427-1,  Player_SetFrame_449-1,  Player_SetFrame_471-1, 0


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: LevelComplete
;----------------------------------------
Player_LevelComplete 
    #init_locals_pull_parameters 2, [0]
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
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #init_var 2, 0 ; Nop
    jmp Player_LevelComplete_51 ; Br_s
Player_LevelComplete_34:  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #init_var 3, 0 ; Nop
    jmp Player_LevelComplete_42 ; Br_s
Player_LevelComplete_38:  ; OPT   #locals_push_value_8 3 ; Ldloc_1
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #inc_var 3 ; Nop
Player_LevelComplete_42:    #locals_push_value_8 3 ; Ldloc_1
  ; OPT   #stack_push_int 50 ; Ldc_i4_s
  ; OPT   #branch_less Player_LevelComplete_38 ; Blt_s
    #branch_less_const 50, Player_LevelComplete_38 ; Nop
  ; OPT   #locals_push_value_8 2 ; Ldloc_0
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #inc_var 2 ; Nop
Player_LevelComplete_51:    #locals_push_value_8 2 ; Ldloc_0
  ; OPT   #stack_push_int 127 ; Ldc_i4_s
  ; OPT   #branch_less Player_LevelComplete_34 ; Blt_s
    #branch_less_const 127, Player_LevelComplete_34 ; Nop
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
    #method_exit 5, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: InitJumpOffsets
;----------------------------------------
Player_InitJumpOffsets 
    #init_locals_pull_parameters 1, [0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #branch_false Player_InitJumpOffsets_9 ; Brfalse_s
    #method_exit 4, [] ; Ret
Player_InitJumpOffsets_9:    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 16 ; Ldc_i4_s
    #newArr  ; Newarr
    #stfld 2 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 15 ; Ldc_i4_s
    #stack_push_int 0 ; Ldc_i4_0
    #stack_duplicate ; Dup
    #locals_pull_value_8 2, 0 ; Stloc_0
    #stelem ; Stelem_i4
    #locals_push_value_8 2 ; Ldloc_0
    #stelem ; Stelem_i4
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 14 ; Ldc_i4_s
    #stack_push_int 6 ; Ldc_i4_6
    #stack_duplicate ; Dup
    #locals_pull_value_8 2, 0 ; Stloc_0
    #stelem ; Stelem_i4
    #locals_push_value_8 2 ; Ldloc_0
    #stelem ; Stelem_i4
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 2 ; Ldc_i4_2
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 13 ; Ldc_i4_s
    #stack_push_int 11 ; Ldc_i4_s
    #stack_duplicate ; Dup
    #locals_pull_value_8 2, 0 ; Stloc_0
    #stelem ; Stelem_i4
    #locals_push_value_8 2 ; Ldloc_0
    #stelem ; Stelem_i4
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 3 ; Ldc_i4_3
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 12 ; Ldc_i4_s
    #stack_push_int 14 ; Ldc_i4_s
    #stack_duplicate ; Dup
    #locals_pull_value_8 2, 0 ; Stloc_0
    #stelem ; Stelem_i4
    #locals_push_value_8 2 ; Ldloc_0
    #stelem ; Stelem_i4
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 4 ; Ldc_i4_4
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 11 ; Ldc_i4_s
    #stack_push_int 16 ; Ldc_i4_s
    #stack_duplicate ; Dup
    #locals_pull_value_8 2, 0 ; Stloc_0
    #stelem ; Stelem_i4
    #locals_push_value_8 2 ; Ldloc_0
    #stelem ; Stelem_i4
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 5 ; Ldc_i4_5
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 10 ; Ldc_i4_s
    #stack_push_int 18 ; Ldc_i4_s
    #stack_duplicate ; Dup
    #locals_pull_value_8 2, 0 ; Stloc_0
    #stelem ; Stelem_i4
    #locals_push_value_8 2 ; Ldloc_0
    #stelem ; Stelem_i4
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 6 ; Ldc_i4_6
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 9 ; Ldc_i4_s
    #stack_push_int 19 ; Ldc_i4_s
    #stack_duplicate ; Dup
    #locals_pull_value_8 2, 0 ; Stloc_0
    #stelem ; Stelem_i4
    #locals_push_value_8 2 ; Ldloc_0
    #stelem ; Stelem_i4
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 7 ; Ldc_i4_7
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 8 ; Ldc_i4_8
    #stack_push_int 20 ; Ldc_i4_s
    #stack_duplicate ; Dup
    #locals_pull_value_8 2, 0 ; Stloc_0
    #stelem ; Stelem_i4
    #locals_push_value_8 2 ; Ldloc_0
    #stelem ; Stelem_i4
    #method_exit 4, [] ; Ret
