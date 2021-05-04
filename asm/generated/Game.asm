

;----------------------------------------
; TYPE: Hunchback.Game
; METHOD: Start
;----------------------------------------
Game_Start 
    #init_locals_pull_parameters 4, [0]
    #locals_push_value8 1 ; Ldarg_0
    jsr Game_Init ; Call
    jsr TitleScreen_Display ; Call
    #newObj 0, 0 ; Newobj
    #locals_pull_value8 2, 1 ; Stloc_0
  ; OPT   #stack_push_int8 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value8 3, 0 ; Stloc_1
    #init_var 3, 0 ; Nop
    jsr LevelDescription_get_Levels ; Call
    #locals_pull_value8 4, 1 ; Stloc_2
    #newObj 0, 0 ; Newobj
    #locals_pull_value8 5, 1 ; Stloc_3
    jmp Game_Start_68 ; Br_s
Game_Start_33:    #stack_push_int8 12 ; Ldc_i4_s
    jsr Screen_Clear ; Call
    #locals_push_value8 2 ; Ldloc_0
    #locals_push_value8 4 ; Ldloc_2
    #locals_push_value8 3 ; Ldloc_1
    #ldelemRef ; Ldelem_ref
    #locals_push_value8 5 ; Ldloc_3
    jsr LevelPlay_Play ; Callvirt
    #branch_false Game_Start_63 ; Brfalse_s
  ; OPT   #locals_push_value8 3 ; Ldloc_1
  ; OPT   #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #add8 ; Add
  ; OPT   #locals_pull_value8 3, 0 ; Stloc_1
    #inc_var 3 ; Nop
    #stack_push_int8 100 ; Ldc_i4_s
    jsr Delay_Wait ; Call
Game_Start_63:    jsr GC_Collect ; Call
Game_Start_68:    #locals_push_value8 3 ; Ldloc_1
    #locals_push_value8 4 ; Ldloc_2
    #ldlen ; Ldlen
    ; conv ; Conv_i4
    #branch_less Game_Start_33 ; Blt_s
    #method_exit 7, [2,4,5] ; Ret


;----------------------------------------
; TYPE: Hunchback.Game
; METHOD: Init
;----------------------------------------
Game_Init 
    #init_locals_pull_parameters 0, [0]
  ; OPT   #stack_push_pointer string_1879048345 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer charset ; Nop
    jsr C64_SetCharSet ; Call
    jsr C64_SetMultiColor ; Call
    #stack_push_int8 0 ; Ldc_i4_0
    jsr C64_SetBackgroundColor ; Call
    #stack_push_int8 0 ; Ldc_i4_0
    jsr C64_SetBorderColor ; Call
    #stack_push_int8 0 ; Ldc_i4_0
    #stack_push_int8 11 ; Ldc_i4_s
    jsr C64_SetCharBackgroundColor ; Call
    #stack_push_int8 1 ; Ldc_i4_1
    #stack_push_int8 1 ; Ldc_i4_1
    jsr C64_SetCharBackgroundColor ; Call
    jsr C64_get_Sprites ; Call
    #stack_push_int8 9 ; Ldc_i4_s
    jsr SpriteCollection_set_CommonColor1 ; Callvirt
    jsr C64_get_Sprites ; Call
    #stack_push_int8 15 ; Ldc_i4_s
    jsr SpriteCollection_set_CommonColor2 ; Callvirt
    jsr C64_get_Sound ; Call
    #stack_push_int8 15 ; Ldc_i4_s
    jsr Sound_set_Volume ; Callvirt
    #method_exit 3, [] ; Ret
