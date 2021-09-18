

;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: set_Sprite
;----------------------------------------
Player_set_Sprite 
    #init_locals_pull_parameters 0, [0,0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 2 ; Ldarg_1
  ; OPT   #stfld8 14 ; Stfld
    #setfld8 1, 2, 14 ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: Init
;----------------------------------------
Player_Init 
    #init_locals_pull_parameters 1, [1,0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 14 ; Ldfld
    #pushfld8 14 ; Nop
  ; OPT   #stack_push_pointer string_1879048575 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_right_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 14 ; Ldfld
    #pushfld8 14 ; Nop
    #stack_push_int8 1 ; Ldc_i4_1
    jsr Sprite_set_MultiColor ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 14 ; Ldfld
    #pushfld8 14 ; Nop
    #stack_push_int8 1 ; Ldc_i4_1
    jsr Sprite_set_Visible ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 14 ; Ldfld
    #pushfld8 14 ; Nop
    #stack_push_int8 5 ; Ldc_i4_5
    jsr Sprite_set_Color ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 14 ; Ldfld
    #pushfld8 14 ; Nop
    jsr Sprite_get_IsInCollision ; Callvirt
    #locals_pull_value8 3, 0 ; Stloc_0
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 14 ; Ldfld
    #pushfld8 14 ; Nop
    jsr Sprite_get_IsInBackgroundCollision ; Callvirt
    #locals_pull_value8 3, 0 ; Stloc_0
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 0 ; Ldc_i4_0
    #stfld8 9 ; Stfld
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 0 ; Ldc_i4_0
    #stfld8 11 ; Stfld
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 2 ; Ldarg_1
  ; OPT   #stfld8 1 ; Stfld
    #setfld8 1, 2, 1 ; Nop
    #locals_push_value8 1 ; Ldarg_0
    jsr Player_InitJumpOffsets ; Call
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 117 ; Ldc_i4_s
    jsr Player_set_Y ; Call
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int16 40 ; Ldc_i4_s
    ; conv ; Conv_i8
    jsr Player_set_X ; Call
    #method_exit 5, [2] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: Move
;----------------------------------------
Player_Move 
    #init_locals_pull_parameters 0, [0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 14 ; Ldfld
    #pushfld8 14 ; Nop
    jsr Sprite_get_IsInBackgroundCollision ; Callvirt
    #branch_false Player_Move_41 ; Brfalse_s
    #locals_push_value8 1 ; Ldarg_0
    jsr Player_get_X ; Call
    #stack_push_int16 300 ; Ldc_i4
    ; conv ; Conv_i8
    #branch_less_equal_unsigned16 Player_Move_34 ; Ble_un_s
    #locals_push_value8 1 ; Ldarg_0
    jsr Player_LevelComplete ; Call
    #method_exit 3, [] ; Ret
Player_Move_34:    #locals_push_value8 1 ; Ldarg_0
    jsr Player_Die ; Call
    #method_exit 3, [] ; Ret
Player_Move_41:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 12 ; Ldfld
    #pushfld8 12 ; Nop
    #branch_false Player_Move_82 ; Brfalse_s
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 0 ; Ldc_i4_0
    #stfld8 9 ; Stfld
    #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 0 ; Ldfld
    #pushfld8 0 ; Nop
    #ldfld8 4 ; Ldfld
    #conv_8_16 ; Conv_u8
    jsr Player_set_X ; Call
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 116 ; Ldc_i4_s
    jsr Player_set_Y ; Call
Player_Move_82:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 9 ; Ldfld
    #pushfld8 9 ; Nop
    #branch_true Player_Move_163 ; Brtrue_s
    #stack_push_int8 22 ; Ldc_i4_s
    jsr C64_IsKeyPressed ; Call
    #branch_false Player_Move_163 ; Brfalse_s
    jsr C64_get_Sound ; Call
    #stack_push_int8 64 ; Ldc_i4_s
    #stack_push_int16 1024 ; Ldc_i4
    ; conv ; Conv_i8
    #stack_push_int16 16424 ; Ldc_i4
    ; conv ; Conv_i8
    #stack_push_int8 0 ; Ldc_i4_0
    #stack_push_int8 9 ; Ldc_i4_s
    #stack_push_int8 1 ; Ldc_i4_1
    jsr Sound_PlayEffectReg1 ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 12 ; Ldfld
    #pushfld8 12 ; Nop
    #branch_false Player_Move_149 ; Brfalse_s
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 1 ; Ldc_i4_1
    #stfld8 10 ; Stfld
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 0 ; Ldc_i4_0
    #stfld8 12 ; Stfld
Player_Move_149:    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 1 ; Ldc_i4_1
    #stfld8 9 ; Stfld
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 0 ; Ldc_i4_0
    #stfld8 7 ; Stfld
Player_Move_163:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 9 ; Ldfld
    #pushfld8 9 ; Nop
    #branch_false Player_Move_231 ; Brfalse_s
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 117 ; Ldc_i4_s
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 7 ; Ldfld
    #pushfld8 7 ; Nop
    #ldelem ; Ldelem_i4
    #sub8 ; Sub
    jsr Player_set_Y ; Call
    #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 7 ; Ldfld
    #pushfld8 7 ; Nop
    #stack_push_int8 1 ; Ldc_i4_1
    #add8 ; Add
    #stfld8 7 ; Stfld
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 7 ; Ldfld
    #pushfld8 7 ; Nop
  ; OPT   #stack_push_int8 16 ; Ldc_i4_s
  ; OPT   #branch_not_equal8 Player_Move_231 ; Bne_un_s
    #branch_not_equal_const 16, Player_Move_231 ; Nop
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 0 ; Ldc_i4_0
    #stfld8 9 ; Stfld
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 0 ; Ldc_i4_0
    #stfld8 10 ; Stfld
Player_Move_231:    #stack_push_int8 0 ; Ldc_i4_0
    jsr C64_IsKeyPressed ; Call
    #branch_false Player_Move_283 ; Brfalse_s
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 1 ; Ldc_i4_1
    #stfld8 8 ; Stfld
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 12 ; Ldfld
    #pushfld8 12 ; Nop
    #branch_true Player_Move_283 ; Brtrue_s
    #locals_push_value8 1 ; Ldarg_0
    #locals_push_value8 1 ; Ldarg_0
    jsr Player_get_X ; Call
    #stack_push_int16 2 ; Ldc_i4_2
    ; conv ; Conv_i8
    #sub16 ; Sub
    jsr Player_set_X ; Call
    #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 6 ; Ldfld
    #pushfld8 6 ; Nop
    #stack_push_int8 1 ; Ldc_i4_1
    #add8 ; Add
    #stfld8 6 ; Stfld
Player_Move_283:    #stack_push_int8 3 ; Ldc_i4_3
    jsr C64_IsKeyPressed ; Call
    #branch_false Player_Move_335 ; Brfalse_s
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 0 ; Ldc_i4_0
    #stfld8 8 ; Stfld
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 12 ; Ldfld
    #pushfld8 12 ; Nop
    #branch_true Player_Move_335 ; Brtrue_s
    #locals_push_value8 1 ; Ldarg_0
    #locals_push_value8 1 ; Ldarg_0
    jsr Player_get_X ; Call
    #stack_push_int16 2 ; Ldc_i4_2
    ; conv ; Conv_i8
    #add16 ; Add
    jsr Player_set_X ; Call
    #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 6 ; Ldfld
    #pushfld8 6 ; Nop
    #stack_push_int8 1 ; Ldc_i4_1
    #add8 ; Add
    #stfld8 6 ; Stfld
Player_Move_335:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 6 ; Ldfld
    #pushfld8 6 ; Nop
  ; OPT   #stack_push_int8 4 ; Ldc_i4_4
  ; OPT   #branch_not_equal8 Player_Move_351 ; Bne_un_s
    #branch_not_equal_const 4, Player_Move_351 ; Nop
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 0 ; Ldc_i4_0
    #stfld8 6 ; Stfld
Player_Move_351:    #locals_push_value8 1 ; Ldarg_0
    jsr Player_get_Y ; Call
  ; OPT   #stack_push_int8 117 ; Ldc_i4_s
  ; OPT   #branch_not_equal8 Player_Move_394 ; Bne_un_s
    #branch_not_equal_const 117, Player_Move_394 ; Nop
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 10 ; Ldfld
    #pushfld8 10 ; Nop
    #branch_true Player_Move_394 ; Brtrue_s
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 1 ; Ldfld
    #pushfld8 1 ; Nop
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld16 3 ; Ldfld
    #pushfld16 3 ; Nop
    jsr Wall_IsHole ; Callvirt
    #branch_false Player_Move_394 ; Brfalse_s
    #locals_push_value8 1 ; Ldarg_0
    jsr Player_Die ; Call
Player_Move_394:    #locals_push_value8 1 ; Ldarg_0
    jsr Player_SetFrame ; Call
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: Die
;----------------------------------------
Player_Die 
    #init_locals_pull_parameters 1, [0]
    jmp Player_Die_24 ; Br_s
Player_Die_2:    #locals_push_value8 1 ; Ldarg_0
    jsr Player_get_Y ; Call
    #locals_pull_value8 2, 0 ; Stloc_0
    #locals_push_value8 1 ; Ldarg_0
    #locals_push_value8 2 ; Ldloc_0
    #stack_push_int8 1 ; Ldc_i4_1
    #add8 ; Add
    jsr Player_set_Y ; Call
    #stack_push_int8 2 ; Ldc_i4_2
    jsr Delay_Wait ; Call
Player_Die_24:    #locals_push_value8 1 ; Ldarg_0
    jsr Player_get_Y ; Call
  ; OPT   #stack_push_int8 250 ; Ldc_i4
  ; OPT   #branch_less_unsigned8 Player_Die_2 ; Blt_un_s
    #branch_less_unsigned_const 250, Player_Die_2 ; Nop
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 1 ; Ldc_i4_1
    #stfld8 11 ; Stfld
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 14 ; Ldfld
    #pushfld8 14 ; Nop
    #stack_push_int8 0 ; Ldc_i4_0
    jsr Sprite_set_Visible ; Callvirt
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: SetOnRope
;----------------------------------------
Player_SetOnRope 
    #init_locals_pull_parameters 0, [1,0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 10 ; Ldfld
    #pushfld8 10 ; Nop
    #branch_false Player_SetOnRope_9 ; Brfalse_s
    #method_exit 4, [2] ; Ret
Player_SetOnRope_9:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 2 ; Ldarg_1
  ; OPT   #stfld8 0 ; Stfld
    #setfld8 1, 2, 0 ; Nop
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 1 ; Ldc_i4_1
    #stfld8 12 ; Stfld
    #method_exit 4, [2] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: get_X
;----------------------------------------
Player_get_X 
    #init_locals_pull_parameters 0, [0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld16 3 ; Ldfld
    #pushfld16 3 ; Nop
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: set_X
;----------------------------------------
Player_set_X 
    #init_locals_pull_parameters 0, [0, 0,0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value16 3 ; Ldarg_1
  ; OPT   #stfld16 3 ; Stfld
    #setfld16 1, 3, 3 ; Nop
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 14 ; Ldfld
    #pushfld8 14 ; Nop
    #locals_push_value16 3 ; Ldarg_1
    jsr Sprite_set_X ; Callvirt
    #method_exit 5, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: get_Y
;----------------------------------------
Player_get_Y 
    #init_locals_pull_parameters 0, [0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 5 ; Ldfld
    #pushfld8 5 ; Nop
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: set_Y
;----------------------------------------
Player_set_Y 
    #init_locals_pull_parameters 0, [0,0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 2 ; Ldarg_1
  ; OPT   #stfld8 5 ; Stfld
    #setfld8 1, 2, 5 ; Nop
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 14 ; Ldfld
    #pushfld8 14 ; Nop
    #locals_push_value8 2 ; Ldarg_1
    jsr Sprite_set_Y ; Callvirt
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: SetFrame
;----------------------------------------
Player_SetFrame 
    #init_locals_pull_parameters 2, [0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 12 ; Ldfld
    #pushfld8 12 ; Nop
    #branch_false Player_SetFrame_60 ; Brfalse_s
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 8 ; Ldfld
    #pushfld8 8 ; Nop
    #branch_false Player_SetFrame_38 ; Brfalse_s
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 14 ; Ldfld
    #pushfld8 14 ; Nop
  ; OPT   #stack_push_pointer string_1879048613 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_rope_left ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_38:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 14 ; Ldfld
    #pushfld8 14 ; Nop
  ; OPT   #stack_push_pointer string_1879048655 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_rope_right ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_60:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 9 ; Ldfld
    #pushfld8 9 ; Nop
    #branch_false Player_SetFrame_249 ; Brfalse
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 8 ; Ldfld
    #pushfld8 8 ; Nop
    #branch_false Player_SetFrame_164 ; Brfalse_s
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 7 ; Ldfld
    #pushfld8 7 ; Nop
  ; OPT   #stack_push_int8 6 ; Ldc_i4_6
  ; OPT   #branch_greater_equal8 Player_SetFrame_110 ; Bge_s
    #branch_greater_equal_const 6, Player_SetFrame_110 ; Nop
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 14 ; Ldfld
    #pushfld8 14 ; Nop
  ; OPT   #stack_push_pointer string_1879048699 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_jump_left_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_110:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 7 ; Ldfld
    #pushfld8 7 ; Nop
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #branch_greater_equal8 Player_SetFrame_142 ; Bge_s
    #branch_greater_equal_const 10, Player_SetFrame_142 ; Nop
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 14 ; Ldfld
    #pushfld8 14 ; Nop
  ; OPT   #stack_push_pointer string_1879048745 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_jump_left_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_142:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 14 ; Ldfld
    #pushfld8 14 ; Nop
  ; OPT   #stack_push_pointer string_1879048791 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_jump_left_2 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_164:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 7 ; Ldfld
    #pushfld8 7 ; Nop
  ; OPT   #stack_push_int8 6 ; Ldc_i4_6
  ; OPT   #branch_greater_equal8 Player_SetFrame_195 ; Bge_s
    #branch_greater_equal_const 6, Player_SetFrame_195 ; Nop
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 14 ; Ldfld
    #pushfld8 14 ; Nop
  ; OPT   #stack_push_pointer string_1879048837 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_jump_right_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_195:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 7 ; Ldfld
    #pushfld8 7 ; Nop
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #branch_greater_equal8 Player_SetFrame_227 ; Bge_s
    #branch_greater_equal_const 10, Player_SetFrame_227 ; Nop
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 14 ; Ldfld
    #pushfld8 14 ; Nop
  ; OPT   #stack_push_pointer string_1879048885 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_jump_right_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_227:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 14 ; Ldfld
    #pushfld8 14 ; Nop
  ; OPT   #stack_push_pointer string_1879048933 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_jump_right_2 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_249:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 8 ; Ldfld
    #pushfld8 8 ; Nop
    #branch_false Player_SetFrame_375 ; Brfalse_s
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 6 ; Ldfld
    #pushfld8 6 ; Nop
    #locals_pull_value8 2, 0 ; Stloc_0
    #locals_push_value8 2 ; Ldloc_0
    #switch Player_SetFrame_Jump_265 ; Switch
    #method_exit 5, [] ; Ret
Player_SetFrame_287:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 14 ; Ldfld
    #pushfld8 14 ; Nop
  ; OPT   #stack_push_pointer string_1879048981 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_left_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_309:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 14 ; Ldfld
    #pushfld8 14 ; Nop
  ; OPT   #stack_push_pointer string_1879049017 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_left_3 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_331:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 14 ; Ldfld
    #pushfld8 14 ; Nop
  ; OPT   #stack_push_pointer string_1879049053 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_left_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_353:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 14 ; Ldfld
    #pushfld8 14 ; Nop
  ; OPT   #stack_push_pointer string_1879049089 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_left_2 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_375:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 6 ; Ldfld
    #pushfld8 6 ; Nop
    #locals_pull_value8 3, 0 ; Stloc_1
    #locals_push_value8 3 ; Ldloc_1
    #switch Player_SetFrame_Jump_383 ; Switch
    #method_exit 5, [] ; Ret
Player_SetFrame_405:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 14 ; Ldfld
    #pushfld8 14 ; Nop
  ; OPT   #stack_push_pointer string_1879048575 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_right_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_427:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 14 ; Ldfld
    #pushfld8 14 ; Nop
  ; OPT   #stack_push_pointer string_1879049125 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_right_3 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_449:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 14 ; Ldfld
    #pushfld8 14 ; Nop
  ; OPT   #stack_push_pointer string_1879049163 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_right_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #method_exit 5, [] ; Ret
Player_SetFrame_471:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 14 ; Ldfld
    #pushfld8 14 ; Nop
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
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 1 ; Ldc_i4_1
    #stfld8 13 ; Stfld
    jsr C64_get_Sound ; Call
    #stack_push_int8 16 ; Ldc_i4_s
    #stack_push_int16 18532 ; Ldc_i4
    ; conv ; Conv_i8
    #stack_push_int16 4096 ; Ldc_i4
    ; conv ; Conv_i8
    #stack_push_int8 10 ; Ldc_i4_s
    #stack_push_int8 0 ; Ldc_i4_0
    #stack_push_int8 0 ; Ldc_i4_0
    jsr Sound_PlayEffectReg2 ; Callvirt
  ; OPT   #stack_push_int8 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value8 2, 0 ; Stloc_0
    #init_var 2, 0 ; Nop
    jmp Player_LevelComplete_56 ; Br_s
Player_LevelComplete_39:  ; OPT   #stack_push_int8 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value8 3, 0 ; Stloc_1
    #init_var 3, 0 ; Nop
    jmp Player_LevelComplete_47 ; Br_s
Player_LevelComplete_43:  ; OPT   #locals_push_value8 3 ; Ldloc_1
  ; OPT   #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #add8 ; Add
  ; OPT   #locals_pull_value8 3, 0 ; Stloc_1
    #inc_var 3 ; Nop
Player_LevelComplete_47:    #locals_push_value8 3 ; Ldloc_1
  ; OPT   #stack_push_int8 50 ; Ldc_i4_s
  ; OPT   #branch_less8 Player_LevelComplete_43 ; Blt_s
    #branch_less_const 50, Player_LevelComplete_43 ; Nop
  ; OPT   #locals_push_value8 2 ; Ldloc_0
  ; OPT   #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #add8 ; Add
  ; OPT   #locals_pull_value8 2, 0 ; Stloc_0
    #inc_var 2 ; Nop
Player_LevelComplete_56:    #locals_push_value8 2 ; Ldloc_0
  ; OPT   #stack_push_int8 127 ; Ldc_i4_s
  ; OPT   #branch_less8 Player_LevelComplete_39 ; Blt_s
    #branch_less_const 127, Player_LevelComplete_39 ; Nop
    jsr C64_get_Sound ; Call
    #stack_push_int8 16 ; Ldc_i4_s
    #stack_push_int16 4708 ; Ldc_i4
    ; conv ; Conv_i8
    #stack_push_int16 4096 ; Ldc_i4
    ; conv ; Conv_i8
    #stack_push_int8 10 ; Ldc_i4_s
    #stack_push_int8 0 ; Ldc_i4_0
    #stack_push_int8 0 ; Ldc_i4_0
    jsr Sound_PlayEffectReg2 ; Callvirt
    #method_exit 5, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Player
; METHOD: InitJumpOffsets
;----------------------------------------
Player_InitJumpOffsets 
    #init_locals_pull_parameters 1, [0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #branch_false Player_InitJumpOffsets_9 ; Brfalse_s
    #method_exit 4, [] ; Ret
Player_InitJumpOffsets_9:    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 16 ; Ldc_i4_s
    #newArr  ; Newarr
    #stfld8 2 ; Stfld
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int8 0 ; Ldc_i4_0
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int8 15 ; Ldc_i4_s
    #stack_push_int8 0 ; Ldc_i4_0
    #stack_duplicate8 ; Dup
    #locals_pull_value8 2, 0 ; Stloc_0
    #stelem ; Stelem_i4
    #locals_push_value8 2 ; Ldloc_0
    #stelem ; Stelem_i4
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int8 14 ; Ldc_i4_s
    #stack_push_int8 6 ; Ldc_i4_6
    #stack_duplicate8 ; Dup
    #locals_pull_value8 2, 0 ; Stloc_0
    #stelem ; Stelem_i4
    #locals_push_value8 2 ; Ldloc_0
    #stelem ; Stelem_i4
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int8 2 ; Ldc_i4_2
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int8 13 ; Ldc_i4_s
    #stack_push_int8 11 ; Ldc_i4_s
    #stack_duplicate8 ; Dup
    #locals_pull_value8 2, 0 ; Stloc_0
    #stelem ; Stelem_i4
    #locals_push_value8 2 ; Ldloc_0
    #stelem ; Stelem_i4
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int8 3 ; Ldc_i4_3
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int8 12 ; Ldc_i4_s
    #stack_push_int8 14 ; Ldc_i4_s
    #stack_duplicate8 ; Dup
    #locals_pull_value8 2, 0 ; Stloc_0
    #stelem ; Stelem_i4
    #locals_push_value8 2 ; Ldloc_0
    #stelem ; Stelem_i4
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int8 4 ; Ldc_i4_4
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int8 11 ; Ldc_i4_s
    #stack_push_int8 16 ; Ldc_i4_s
    #stack_duplicate8 ; Dup
    #locals_pull_value8 2, 0 ; Stloc_0
    #stelem ; Stelem_i4
    #locals_push_value8 2 ; Ldloc_0
    #stelem ; Stelem_i4
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int8 5 ; Ldc_i4_5
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int8 10 ; Ldc_i4_s
    #stack_push_int8 18 ; Ldc_i4_s
    #stack_duplicate8 ; Dup
    #locals_pull_value8 2, 0 ; Stloc_0
    #stelem ; Stelem_i4
    #locals_push_value8 2 ; Ldloc_0
    #stelem ; Stelem_i4
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int8 6 ; Ldc_i4_6
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int8 9 ; Ldc_i4_s
    #stack_push_int8 19 ; Ldc_i4_s
    #stack_duplicate8 ; Dup
    #locals_pull_value8 2, 0 ; Stloc_0
    #stelem ; Stelem_i4
    #locals_push_value8 2 ; Ldloc_0
    #stelem ; Stelem_i4
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int8 7 ; Ldc_i4_7
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int8 8 ; Ldc_i4_8
    #stack_push_int8 20 ; Ldc_i4_s
    #stack_duplicate8 ; Dup
    #locals_pull_value8 2, 0 ; Stloc_0
    #stelem ; Stelem_i4
    #locals_push_value8 2 ; Ldloc_0
    #stelem ; Stelem_i4
    #method_exit 4, [] ; Ret
