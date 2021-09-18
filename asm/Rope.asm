

;----------------------------------------
; TYPE: Hunchback.Rope
; METHOD: set_Sprite1
;----------------------------------------
Rope_set_Sprite1 
    #init_locals_pull_parameters 0, [0,0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 2 ; Ldarg_1
  ; OPT   #stfld8 2 ; Stfld
    #setfld8 1, 2, 2 ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Rope
; METHOD: set_Sprite2
;----------------------------------------
Rope_set_Sprite2 
    #init_locals_pull_parameters 0, [0,0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 2 ; Ldarg_1
  ; OPT   #stfld8 3 ; Stfld
    #setfld8 1, 2, 3 ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Rope
; METHOD: Init
;----------------------------------------
Rope_Init 
    #init_locals_pull_parameters 1, [0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int8 1 ; Ldc_i4_1
    jsr Sprite_set_Visible ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
    #stack_push_int8 1 ; Ldc_i4_1
    jsr Sprite_set_Visible ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int8 1 ; Ldc_i4_1
    jsr Sprite_set_Color ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
    #stack_push_int8 1 ; Ldc_i4_1
    jsr Sprite_set_Color ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int8 1 ; Ldc_i4_1
    #stack_duplicate8 ; Dup
    #locals_pull_value8 2, 0 ; Stloc_0
    jsr Sprite_set_ExpandY ; Callvirt
    #locals_push_value8 2 ; Ldloc_0
    jsr Sprite_set_ExpandX ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
    #stack_push_int8 1 ; Ldc_i4_1
    #stack_duplicate8 ; Dup
    #locals_pull_value8 2, 0 ; Stloc_0
    jsr Sprite_set_ExpandY ; Callvirt
    #locals_push_value8 2 ; Ldloc_0
    jsr Sprite_set_ExpandX ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int16 184 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
    #stack_push_int16 192 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int8 66 ; Ldc_i4_s
    jsr Sprite_set_Y ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
    #stack_push_int8 108 ; Ldc_i4_s
    jsr Sprite_set_Y ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049317 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_14 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049341 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_15 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 0 ; Ldc_i4_0
    #stfld8 0 ; Stfld
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Rope
; METHOD: Move
;----------------------------------------
Rope_Move 
    #init_locals_pull_parameters 0, [0]
    #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 1 ; Ldfld
    #pushfld8 1 ; Nop
    #stack_push_int8 1 ; Ldc_i4_1
    #add8 ; Add
    #stfld8 1 ; Stfld
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 1 ; Ldfld
    #pushfld8 1 ; Nop
  ; OPT   #stack_push_int8 2 ; Ldc_i4_2
  ; OPT   #branch_greater_equal_unsigned8 Rope_Move_24 ; Bge_un_s
    #branch_greater_equal_unsigned_const 2, Rope_Move_24 ; Nop
    #method_exit 3, [] ; Ret
Rope_Move_24:    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 0 ; Ldc_i4_0
    #stfld8 1 ; Stfld
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 0 ; Ldfld
    #pushfld8 0 ; Nop
    #branch_false Rope_Move_49 ; Brfalse_s
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 0 ; Ldfld
    #pushfld8 0 ; Nop
  ; OPT   #stack_push_int8 16 ; Ldc_i4_s
  ; OPT   #branch_not_equal8 Rope_Move_79 ; Bne_un_s
    #branch_not_equal_const 16, Rope_Move_79 ; Nop
Rope_Move_49:    jsr C64_get_Sound ; Call
    #stack_push_int8 128 ; Ldc_i4
    #stack_push_int16 3940 ; Ldc_i4
    ; conv ; Conv_i8
    #stack_push_int16 0 ; Ldc_i4_0
    ; conv ; Conv_i8
    #stack_push_int8 176 ; Ldc_i4
    #stack_push_int8 0 ; Ldc_i4_0
    #stack_push_int8 0 ; Ldc_i4_0
    jsr Sound_PlayEffectReg2 ; Callvirt
Rope_Move_79:    #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 0 ; Ldfld
    #pushfld8 0 ; Nop
    #stack_push_int8 1 ; Ldc_i4_1
    #add8 ; Add
    #stfld8 0 ; Stfld
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 0 ; Ldfld
    #pushfld8 0 ; Nop
  ; OPT   #stack_push_int8 32 ; Ldc_i4_s
  ; OPT   #branch_not_equal8 Rope_Move_110 ; Bne_un_s
    #branch_not_equal_const 32, Rope_Move_110 ; Nop
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 0 ; Ldc_i4_0
    #stfld8 0 ; Stfld
Rope_Move_110:    #locals_push_value8 1 ; Ldarg_0
    jsr Rope_SetFrame ; Call
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Rope
; METHOD: SetFrame
;----------------------------------------
Rope_SetFrame 
    #init_locals_pull_parameters 1, [0]
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 0 ; Ldfld
    #pushfld8 0 ; Nop
  ; OPT   #stack_push_int8 16 ; Ldc_i4_s
  ; OPT   #branch_greater_equal_unsigned8 Rope_SetFrame_18 ; Bge_un_s
    #branch_greater_equal_unsigned_const 16, Rope_SetFrame_18 ; Nop
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 0 ; Ldfld
    #pushfld8 0 ; Nop
    jmp Rope_SetFrame_27 ; Br_s
Rope_SetFrame_18:    #stack_push_int8 31 ; Ldc_i4_s
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 0 ; Ldfld
    #pushfld8 0 ; Nop
    #sub8 ; Sub
Rope_SetFrame_27:    #locals_pull_value8 2, 0 ; Stloc_0
    #locals_push_value8 2 ; Ldloc_0
    #switch Rope_SetFrame_Jump_29 ; Switch
    jmp Rope_SetFrame_1552 ; Br
Rope_SetFrame_103:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049317 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_14 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049341 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_15 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int16 184 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
    #stack_push_int16 192 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 198 ; Ldc_i4
    #stfld8 4 ; Stfld
    jmp Rope_SetFrame_1552 ; Br
Rope_SetFrame_195:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049365 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_12 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049389 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_13 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int16 184 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
    #stack_push_int16 186 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 194 ; Ldc_i4
    #stfld8 4 ; Stfld
    jmp Rope_SetFrame_1552 ; Br
Rope_SetFrame_287:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049413 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_10 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049437 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_11 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int16 184 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
    #stack_push_int16 184 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 186 ; Ldc_i4
    #stfld8 4 ; Stfld
    jmp Rope_SetFrame_1552 ; Br
Rope_SetFrame_379:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049461 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_8 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049483 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_9 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int16 184 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
    #stack_push_int16 184 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 178 ; Ldc_i4
    #stfld8 4 ; Stfld
    jmp Rope_SetFrame_1552 ; Br
Rope_SetFrame_471:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049505 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_6 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049527 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_7 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int16 184 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
    #stack_push_int16 184 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 172 ; Ldc_i4
    #stfld8 4 ; Stfld
    jmp Rope_SetFrame_1552 ; Br
Rope_SetFrame_563:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049549 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_4 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049571 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_5 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int16 184 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
    #stack_push_int16 184 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 166 ; Ldc_i4
    #stfld8 4 ; Stfld
    jmp Rope_SetFrame_1552 ; Br
Rope_SetFrame_655:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049593 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_2 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049615 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_3 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int16 184 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
    #stack_push_int16 184 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 158 ; Ldc_i4
    #stfld8 4 ; Stfld
    jmp Rope_SetFrame_1552 ; Br
Rope_SetFrame_747:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049637 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049659 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int16 184 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
    #stack_push_int16 184 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 152 ; Ldc_i4
    #stfld8 4 ; Stfld
    jmp Rope_SetFrame_1552 ; Br
Rope_SetFrame_839:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049681 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_16 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049705 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_17 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int16 138 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
    #stack_push_int16 138 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 148 ; Ldc_i4
    #stfld8 4 ; Stfld
    jmp Rope_SetFrame_1552 ; Br
Rope_SetFrame_931:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049729 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_18 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049753 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_19 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int16 138 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
    #stack_push_int16 138 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 138 ; Ldc_i4
    #stfld8 4 ; Stfld
    jmp Rope_SetFrame_1552 ; Br
Rope_SetFrame_1023:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049777 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_20 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049801 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_21 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int16 138 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
    #stack_push_int16 138 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 130 ; Ldc_i4
    #stfld8 4 ; Stfld
    jmp Rope_SetFrame_1552 ; Br
Rope_SetFrame_1115:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049825 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_22 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049849 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_23 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int16 138 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
    #stack_push_int16 138 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 124 ; Ldc_i4_s
    #stfld8 4 ; Stfld
    jmp Rope_SetFrame_1552 ; Br
Rope_SetFrame_1204:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049873 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_24 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049897 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_25 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int16 138 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
    #stack_push_int16 138 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 118 ; Ldc_i4_s
    #stfld8 4 ; Stfld
    jmp Rope_SetFrame_1552 ; Br
Rope_SetFrame_1293:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049921 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_26 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049945 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_27 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int16 138 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
    #stack_push_int16 138 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 110 ; Ldc_i4_s
    #stfld8 4 ; Stfld
    jmp Rope_SetFrame_1552 ; Br
Rope_SetFrame_1382:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049969 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_28 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049993 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_29 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int16 138 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
    #stack_push_int16 136 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 102 ; Ldc_i4_s
    #stfld8 4 ; Stfld
    jmp Rope_SetFrame_1552 ; Br_s
Rope_SetFrame_1468:  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
  ; OPT   #stack_push_pointer string_1879050017 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_30 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
  ; OPT   #stack_push_pointer string_1879050041 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_31 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 2 ; Ldfld
    #pushfld8 2 ; Nop
    #stack_push_int16 138 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 3 ; Ldfld
    #pushfld8 3 ; Nop
    #stack_push_int16 130 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 98 ; Ldc_i4_s
    #stfld8 4 ; Stfld
Rope_SetFrame_1552:    #locals_push_value8 1 ; Ldarg_0
  ; OPT   #locals_push_value8 1 ; Ldarg_0
  ; OPT   #ldfld8 4 ; Ldfld
    #pushfld8 4 ; Nop
    #stack_push_int8 20 ; Ldc_i4_s
    #add8 ; Add
    #stfld8 4 ; Stfld
    #method_exit 4, [] ; Ret
Rope_SetFrame_Jump_29 .word  Rope_SetFrame_103-1,  Rope_SetFrame_195-1,  Rope_SetFrame_287-1,  Rope_SetFrame_379-1,  Rope_SetFrame_471-1,  Rope_SetFrame_563-1,  Rope_SetFrame_655-1,  Rope_SetFrame_747-1,  Rope_SetFrame_839-1,  Rope_SetFrame_931-1,  Rope_SetFrame_1023-1,  Rope_SetFrame_1115-1,  Rope_SetFrame_1204-1,  Rope_SetFrame_1293-1,  Rope_SetFrame_1382-1,  Rope_SetFrame_1468-1, 0
