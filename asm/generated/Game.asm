

;----------------------------------------
; TYPE: Hunchback.Game
; METHOD: Start
;----------------------------------------
Game_Start 
    #init_locals_pull_parameters 6, [0]
    ; nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    jsr Game_Init ; Call
    ; nop ; Nop
    jsr TitleScreen_Display ; Call
    ; nop ; Nop
    #newObj 0, 0 ; Newobj
    #locals_pull_value_8 2, 1 ; Stloc_0
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #init_var 3, 0 ; Nop
    jsr LevelDescription_get_Levels ; Call
    #locals_pull_value_8 4, 1 ; Stloc_2
    #newObj 0, 0 ; Newobj
    #locals_pull_value_8 5, 1 ; Stloc_3
    jmp Game_Start_82 ; Br_s
Game_Start_36:    ; nop ; Nop
    #stack_push_int 12 ; Ldc_i4_s
    jsr Screen_Clear ; Call
    ; nop ; Nop
    #locals_push_value_8 2 ; Ldloc_0
    #locals_push_value_8 4 ; Ldloc_2
    #locals_push_value_8 3 ; Ldloc_1
    #ldelemRef ; Ldelem_ref
    #locals_push_value_8 5 ; Ldloc_3
    jsr LevelPlay_Play ; Callvirt
    #locals_pull_value_8 6, 0 ; Stloc_s
    #locals_push_value_8 6 ; Ldloc_s
    #branch_false Game_Start_75 ; Brfalse_s
    ; nop ; Nop
  ; OPT   #locals_push_value_8 3 ; Ldloc_1
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #inc_var 3 ; Nop
    #stack_push_int 100 ; Ldc_i4_s
    jsr Delay_Wait ; Call
    ; nop ; Nop
    ; nop ; Nop
Game_Start_75:    jsr GC_Collect ; Call
    ; nop ; Nop
    ; nop ; Nop
Game_Start_82:    #locals_push_value_8 3 ; Ldloc_1
    #locals_push_value_8 4 ; Ldloc_2
    #ldlen ; Ldlen
    nop ;Conv_i4 ; Conv_i4
    #compareLess ; Clt
    #locals_pull_value_8 7, 0 ; Stloc_s
    #locals_push_value_8 7 ; Ldloc_s
    #branch_true Game_Start_36 ; Brtrue_s
    #method_exit 9, [2,4,5] ; Ret


;----------------------------------------
; TYPE: Hunchback.Game
; METHOD: Init
;----------------------------------------
Game_Init 
    #init_locals_pull_parameters 0, [0]
    ; nop ; Nop
  ; OPT   #stack_push_pointer string_1879048345 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer charset ; Nop
    jsr C64_SetCharSet ; Call
    ; nop ; Nop
    jsr C64_SetMultiColor ; Call
    ; nop ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_SetBackgroundColor ; Call
    ; nop ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_SetBorderColor ; Call
    ; nop ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    #stack_push_int 11 ; Ldc_i4_s
    jsr C64_SetCharBackgroundColor ; Call
    ; nop ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #stack_push_int 1 ; Ldc_i4_1
    jsr C64_SetCharBackgroundColor ; Call
    ; nop ; Nop
    jsr C64_get_Sprites ; Call
    #stack_push_int 9 ; Ldc_i4_s
    jsr SpriteCollection_set_CommonColor1 ; Callvirt
    ; nop ; Nop
    jsr C64_get_Sprites ; Call
    #stack_push_int 15 ; Ldc_i4_s
    jsr SpriteCollection_set_CommonColor2 ; Callvirt
    ; nop ; Nop
    jsr C64_get_Sound ; Call
    #stack_push_int 15 ; Ldc_i4_s
    jsr Sound_set_Volume ; Callvirt
    ; nop ; Nop
    #method_exit 3, [] ; Ret
