

;----------------------------------------
; TYPE: Demo.Program
; METHOD: Foo
;----------------------------------------
Program_Foo 
    #init_locals_pull_parameters 0, [0,0,0]
    nop ; Nop
    #method_exit 5, [] ; Ret


;----------------------------------------
; TYPE: Demo.Program
; METHOD: GetReady
;----------------------------------------
Program_GetReady 
    #init_locals_pull_parameters 4, []
    nop ; Nop
    #stack_push_int 16 ; Ldc_i4_s
    #stack_push_int 6 ; Ldc_i4_6
    #stack_push_pointer string_1879048193 ; Ldstr
    #stack_push_int 1 ; Ldc_i4_1
    jsr C64_Write ; Call
    nop ; Nop
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 1, 0 ; Stloc_0
    #init_var 1, 0 ; Nop
    jmp Program_GetReady_45 ; Br_s
Program_GetReady_20:    nop ; Nop
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_1
    #init_var 2, 0 ; Nop
    jmp Program_GetReady_31 ; Br_s
Program_GetReady_25:    nop ; Nop
    nop ; Nop
  ; OPT   #locals_push_value_8 2 ; Ldloc_1
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_1
    #inc_var 2 ; Nop
Program_GetReady_31:  ; OPT   #locals_push_value_8 2 ; Ldloc_1
  ; OPT   #stack_push_int 55 ; Ldc_i4_s
  ; OPT   #compareLess ; Clt_un
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_2
  ; OPT   #locals_push_value_8 3 ; Ldloc_2
  ; OPT   #branch_true Program_GetReady_25 ; Brtrue_s
    #branch_if_var_less 2, 55, Program_GetReady_25 ; Nop
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldloc_0
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 1, 0 ; Stloc_0
    #inc_var 1 ; Nop
Program_GetReady_45:  ; OPT   #locals_push_value_8 1 ; Ldloc_0
  ; OPT   #stack_push_int 255 ; Ldc_i4
  ; OPT   #compareLess ; Clt_un
  ; OPT   #locals_pull_value_8 4, 0 ; Stloc_3
  ; OPT   #locals_push_value_8 4 ; Ldloc_3
  ; OPT   #branch_true Program_GetReady_20 ; Brtrue_s
    #branch_if_var_less 1, 255, Program_GetReady_20 ; Nop
    #stack_push_int 16 ; Ldc_i4_s
    #stack_push_int 6 ; Ldc_i4_6
    #stack_push_pointer string_1879048213 ; Ldstr
    #stack_push_int 1 ; Ldc_i4_1
    jsr C64_Write ; Call
    nop ; Nop
    #method_exit 6, [] ; Ret


;----------------------------------------
; TYPE: Demo.Program
; METHOD: Main
;----------------------------------------
Program_Main 
    #init_locals_pull_parameters 3, []
    nop ; Nop
  ; OPT   #stack_push_pointer string_1879048233 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer charset ; Nop
    jsr C64_SetCharSet ; Call
    nop ; Nop
    jsr C64_SetMultiColor ; Call
    nop ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_SetBackgroundColor ; Call
    nop ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_SetBorderColor ; Call
    nop ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    #stack_push_int 11 ; Ldc_i4_s
    jsr C64_SetCharBackgroundColor ; Call
    nop ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #stack_push_int 1 ; Ldc_i4_1
    jsr C64_SetCharBackgroundColor ; Call
    nop ; Nop
    jsr C64_get_Sprites ; Call
    #stack_push_int 9 ; Ldc_i4_s
    jsr SpriteCollection_set_CommonColor1 ; Callvirt
    nop ; Nop
    jsr C64_get_Sprites ; Call
    #stack_push_int 15 ; Ldc_i4_s
    jsr SpriteCollection_set_CommonColor2 ; Callvirt
    nop ; Nop
    jsr TitleScreen_Display ; Call
    nop ; Nop
    #stack_push_int 12 ; Ldc_i4_s
    jsr Screen_Clear ; Call
    nop ; Nop
    #stack_push_int 11 ; Ldc_i4_s
    jsr Wall_Draw ; Call
    nop ; Nop
    #newObj 10, 1 ; Newobj
    #stack_duplicate ; Dup
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite0 ; Callvirt
    jsr Player_set_Sprite ; Callvirt
    nop ; Nop
    #locals_pull_value_8 1, 1 ; Stloc_0
    #locals_push_value_8 1 ; Ldloc_0
    jsr Player_Init ; Callvirt
    nop ; Nop
    #newObj 6, 0 ; Newobj
    #stack_duplicate ; Dup
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite1 ; Callvirt
    jsr Knight_set_Sprite ; Callvirt
    nop ; Nop
    #locals_pull_value_8 2, 1 ; Stloc_1
    #locals_push_value_8 2 ; Ldloc_1
    jsr Knight_Init ; Callvirt
    nop ; Nop
    jsr Program_GetReady ; Call
    nop ; Nop
    #newObj 6, 0 ; Newobj
    #stack_duplicate ; Dup
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite2 ; Callvirt
    jsr Enemy_set_Sprite ; Callvirt
    nop ; Nop
    #locals_pull_value_8 3, 1 ; Stloc_2
    #locals_push_value_8 3 ; Ldloc_2
    jsr Enemy_Init ; Callvirt
    nop ; Nop
    jmp Program_Main_231 ; Br_s
Program_Main_200:    nop ; Nop
    #locals_push_value_8 1 ; Ldloc_0
    jsr Player_Move ; Callvirt
    nop ; Nop
    #locals_push_value_8 2 ; Ldloc_1
    jsr Knight_Move ; Callvirt
    nop ; Nop
    #locals_push_value_8 3 ; Ldloc_2
    jsr Enemy_Move ; Callvirt
    nop ; Nop
    #stack_push_int 100 ; Ldc_i4_s
    jsr Delay_Wait ; Call
    nop ; Nop
    nop ; Nop
Program_Main_231:    jmp Program_Main_200 ; Br_s
