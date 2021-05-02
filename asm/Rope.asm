

;----------------------------------------
; TYPE: Hunchback.Rope
; METHOD: set_Sprite1
;----------------------------------------
Rope_set_Sprite1 
    #init_locals_pull_parameters 0, [0,0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stfld 2 ; Stfld
    #setfld 1, 2, 2 ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Rope
; METHOD: set_Sprite2
;----------------------------------------
Rope_set_Sprite2 
    #init_locals_pull_parameters 0, [0,0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stfld 3 ; Stfld
    #setfld 1, 2, 3 ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Rope
; METHOD: Init
;----------------------------------------
Rope_Init 
    #init_locals_pull_parameters 1, [0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_Visible ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_Visible ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_Color ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_Color ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #stack_duplicate ; Dup
    #locals_pull_value_8 2, 0 ; Stloc_0
    jsr Sprite_set_ExpandY ; Callvirt
    #locals_push_value_8 2 ; Ldloc_0
    jsr Sprite_set_ExpandX ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #stack_duplicate ; Dup
    #locals_pull_value_8 2, 0 ; Stloc_0
    jsr Sprite_set_ExpandY ; Callvirt
    #locals_push_value_8 2 ; Ldloc_0
    jsr Sprite_set_ExpandX ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 184 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 192 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 66 ; Ldc_i4_s
    jsr Sprite_set_Y ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 108 ; Ldc_i4_s
    jsr Sprite_set_Y ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049291 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_14 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049315 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_15 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 0 ; Stfld
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Rope
; METHOD: Move
;----------------------------------------
Rope_Move 
    #init_locals_pull_parameters 0, [0]
    #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #stfld 1 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
  ; OPT   #stack_push_int 2 ; Ldc_i4_2
  ; OPT   #branch_greater_equal_unsigned Rope_Move_24 ; Bge_un_s
    #branch_greater_equal_unsigned_const 2, Rope_Move_24 ; Nop
    #method_exit 3, [] ; Ret
Rope_Move_24:    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 1 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #branch_false Rope_Move_49 ; Brfalse_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
  ; OPT   #stack_push_int 16 ; Ldc_i4_s
  ; OPT   #branch_not_equal Rope_Move_77 ; Bne_un_s
    #branch_not_equal_const 16, Rope_Move_77 ; Nop
Rope_Move_49:    jsr C64_get_Sound ; Call
    #stack_push_int 128 ; Ldc_i4
    #stack_push_int 100 ; Ldc_i4_s
    #stack_push_int 15 ; Ldc_i4_s
    #stack_push_int 0 ; Ldc_i4_0
    #stack_push_int 0 ; Ldc_i4_0
    #stack_push_int 176 ; Ldc_i4
    #stack_push_int 0 ; Ldc_i4_0
    #stack_push_int 0 ; Ldc_i4_0
    jsr Sound_PlayEffectReg2 ; Callvirt
Rope_Move_77:    #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #stfld 0 ; Stfld
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
  ; OPT   #stack_push_int 32 ; Ldc_i4_s
  ; OPT   #branch_not_equal Rope_Move_108 ; Bne_un_s
    #branch_not_equal_const 32, Rope_Move_108 ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 0 ; Stfld
Rope_Move_108:    #locals_push_value_8 1 ; Ldarg_0
    jsr Rope_SetFrame ; Call
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Rope
; METHOD: SetFrame
;----------------------------------------
Rope_SetFrame 
    #init_locals_pull_parameters 1, [0]
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
  ; OPT   #stack_push_int 16 ; Ldc_i4_s
  ; OPT   #branch_greater_equal_unsigned Rope_SetFrame_18 ; Bge_un_s
    #branch_greater_equal_unsigned_const 16, Rope_SetFrame_18 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    jmp Rope_SetFrame_27 ; Br_s
Rope_SetFrame_18:    #stack_push_int 31 ; Ldc_i4_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #sub ; Sub
Rope_SetFrame_27:    #locals_pull_value_8 2, 0 ; Stloc_0
    #locals_push_value_8 2 ; Ldloc_0
    #switch Rope_SetFrame_Jump_29 ; Switch
    jmp Rope_SetFrame_1520 ; Br
Rope_SetFrame_103:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049291 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_14 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049315 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_15 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 184 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 192 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 198 ; Ldc_i4
    #stfld 4 ; Stfld
    jmp Rope_SetFrame_1520 ; Br
Rope_SetFrame_193:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049339 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_12 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049363 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_13 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 184 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 186 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 194 ; Ldc_i4
    #stfld 4 ; Stfld
    jmp Rope_SetFrame_1520 ; Br
Rope_SetFrame_283:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049387 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_10 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049411 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_11 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 184 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 184 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 186 ; Ldc_i4
    #stfld 4 ; Stfld
    jmp Rope_SetFrame_1520 ; Br
Rope_SetFrame_373:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049435 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_8 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049457 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_9 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 184 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 184 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 178 ; Ldc_i4
    #stfld 4 ; Stfld
    jmp Rope_SetFrame_1520 ; Br
Rope_SetFrame_463:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049479 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_6 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049501 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_7 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 184 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 184 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 172 ; Ldc_i4
    #stfld 4 ; Stfld
    jmp Rope_SetFrame_1520 ; Br
Rope_SetFrame_553:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049523 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_4 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049545 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_5 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 184 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 184 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 166 ; Ldc_i4
    #stfld 4 ; Stfld
    jmp Rope_SetFrame_1520 ; Br
Rope_SetFrame_643:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049567 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_2 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049589 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_3 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 184 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 184 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 158 ; Ldc_i4
    #stfld 4 ; Stfld
    jmp Rope_SetFrame_1520 ; Br
Rope_SetFrame_733:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049611 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049633 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_1 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 184 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 184 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 152 ; Ldc_i4
    #stfld 4 ; Stfld
    jmp Rope_SetFrame_1520 ; Br
Rope_SetFrame_823:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049655 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_16 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049679 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_17 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 138 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 138 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 148 ; Ldc_i4
    #stfld 4 ; Stfld
    jmp Rope_SetFrame_1520 ; Br
Rope_SetFrame_913:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049703 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_18 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049727 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_19 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 138 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 138 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 138 ; Ldc_i4
    #stfld 4 ; Stfld
    jmp Rope_SetFrame_1520 ; Br
Rope_SetFrame_1003:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049751 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_20 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049775 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_21 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 138 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 138 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 130 ; Ldc_i4
    #stfld 4 ; Stfld
    jmp Rope_SetFrame_1520 ; Br
Rope_SetFrame_1093:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049799 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_22 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049823 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_23 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 138 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 138 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 124 ; Ldc_i4_s
    #stfld 4 ; Stfld
    jmp Rope_SetFrame_1520 ; Br
Rope_SetFrame_1180:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049847 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_24 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049871 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_25 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 138 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 138 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 118 ; Ldc_i4_s
    #stfld 4 ; Stfld
    jmp Rope_SetFrame_1520 ; Br
Rope_SetFrame_1267:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049895 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_26 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049919 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_27 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 138 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 138 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 110 ; Ldc_i4_s
    #stfld 4 ; Stfld
    jmp Rope_SetFrame_1520 ; Br
Rope_SetFrame_1354:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049943 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_28 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
  ; OPT   #stack_push_pointer string_1879049967 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_29 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 138 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 136 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 102 ; Ldc_i4_s
    #stfld 4 ; Stfld
    jmp Rope_SetFrame_1520 ; Br_s
Rope_SetFrame_1438:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049991 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_30 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
  ; OPT   #stack_push_pointer string_1879050015 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_31 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 138 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld 3 ; Nop
    #stack_push_int 130 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 98 ; Ldc_i4_s
    #stfld 4 ; Stfld
Rope_SetFrame_1520:    #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld 4 ; Nop
    #stack_push_int 20 ; Ldc_i4_s
    #add ; Add
    #stfld 4 ; Stfld
    #method_exit 4, [] ; Ret
Rope_SetFrame_Jump_29 .word  Rope_SetFrame_103-1,  Rope_SetFrame_193-1,  Rope_SetFrame_283-1,  Rope_SetFrame_373-1,  Rope_SetFrame_463-1,  Rope_SetFrame_553-1,  Rope_SetFrame_643-1,  Rope_SetFrame_733-1,  Rope_SetFrame_823-1,  Rope_SetFrame_913-1,  Rope_SetFrame_1003-1,  Rope_SetFrame_1093-1,  Rope_SetFrame_1180-1,  Rope_SetFrame_1267-1,  Rope_SetFrame_1354-1,  Rope_SetFrame_1438-1, 0
