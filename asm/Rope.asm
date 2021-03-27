

;----------------------------------------
; TYPE: Hunchback.Rope
; METHOD: set_Sprite1
;----------------------------------------
Rope_set_Sprite1 
    #init_locals_pull_parameters 0, [0,0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stfld 1 ; Stfld
    #setfld 1, 2, 1 ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Rope
; METHOD: set_Sprite2
;----------------------------------------
Rope_set_Sprite2 
    #init_locals_pull_parameters 0, [0,0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #locals_push_value_8 2 ; Ldarg_1
  ; OPT   #stfld 2 ; Stfld
    #setfld 1, 2, 2 ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Rope
; METHOD: Init
;----------------------------------------
Rope_Init 
    #init_locals_pull_parameters 1, [0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_Visible ; Callvirt
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_Visible ; Callvirt
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_Color ; Callvirt
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_Color ; Callvirt
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #stack_duplicate ; Dup
    #locals_pull_value_8 2, 0 ; Stloc_0
    jsr Sprite_set_ExpandY ; Callvirt
    nop ; Nop
    #locals_push_value_8 2 ; Ldloc_0
    jsr Sprite_set_ExpandX ; Callvirt
    nop ; Nop
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
    nop ; Nop
    #locals_push_value_8 2 ; Ldloc_0
    jsr Sprite_set_ExpandX ; Callvirt
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
    #stack_push_int 184 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 192 ; Ldc_i4
    jsr Sprite_set_X ; Callvirt
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
    #stack_push_int 66 ; Ldc_i4_s
    jsr Sprite_set_Y ; Callvirt
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 108 ; Ldc_i4_s
    jsr Sprite_set_Y ; Callvirt
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
  ; OPT   #stack_push_pointer string_1879049173 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_14 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049197 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_15 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
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
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    jsr Rope_SetFrame ; Call
    nop ; Nop
    #method_exit 3, [] ; Ret


;----------------------------------------
; TYPE: Hunchback.Rope
; METHOD: SetFrame
;----------------------------------------
Rope_SetFrame 
    #init_locals_pull_parameters 3, [0]
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #stack_push_int 15 ; Ldc_i4_s
    #branch_greater Rope_SetFrame_19 ; Bgt_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    jmp Rope_SetFrame_28 ; Br_s
Rope_SetFrame_19:    #stack_push_int 31 ; Ldc_i4_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #sub ; Sub
Rope_SetFrame_28:    #locals_pull_value_8 2, 0 ; Stloc_0
    #locals_push_value_8 2 ; Ldloc_0
    #locals_pull_value_8 4, 0 ; Stloc_2
    #locals_push_value_8 4 ; Ldloc_2
    #locals_pull_value_8 3, 0 ; Stloc_1
    #locals_push_value_8 3 ; Ldloc_1
    #branch_false Rope_SetFrame_44 ; Brfalse_s
    jmp Rope_SetFrame_38 ; Br_s
Rope_SetFrame_38:    #locals_push_value_8 3 ; Ldloc_1
    #stack_push_int 1 ; Ldc_i4_1
    #branch_equal Rope_SetFrame_90 ; Beq_s
    jmp Rope_SetFrame_136 ; Br_s
Rope_SetFrame_44:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
  ; OPT   #stack_push_pointer string_1879049173 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_14 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049197 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_15 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Rope_SetFrame_136 ; Br_s
Rope_SetFrame_90:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
  ; OPT   #stack_push_pointer string_1879049173 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_14 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
  ; OPT   #stack_push_pointer string_1879049197 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_rope_15 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    jmp Rope_SetFrame_136 ; Br_s
Rope_SetFrame_136:    #method_exit 6, [] ; Ret
