

;----------------------------------------
; TYPE: Game
; METHOD: Init
;----------------------------------------
Game_Init 
    #locals_init_locals 0
    #locals_pull_param_8 0
    nop ; Nop
    jsr Game_ClearScreen ; Call
    nop ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_SetBorderColor ; Call
    nop ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_SetBackgroundColor ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    jsr Game_InitPlatforms ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 2 ; Ldc_i4_2
    #newArr  ; Newarr
    #stack_duplicate ; Dup
    #stack_push_int 0 ; Ldc_i4_0
    #newObj 9, 0 ; Newobj
    #stack_duplicate ; Dup
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    #ldelemRef ; Ldelem_ref
    jsr PlatformEnemy_set_Platform ; Callvirt
    nop ; Nop
    #stack_duplicate ; Dup
    #stack_push_int 2 ; Ldc_i4_2
    #stfld 4 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 2 ; Ldc_i4_2
    #stfld 3 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 2 ; Stfld
    #stack_duplicate ; Dup
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite2 ; Callvirt
    jsr PlatformEnemy_set_Sprite ; Callvirt
    nop ; Nop
    #stack_duplicate ; Dup
    #stack_push_int 2 ; Ldc_i4_2
    jsr PlatformEnemy_set_Color ; Callvirt
    nop ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate ; Dup
    #stack_push_int 1 ; Ldc_i4_1
    #newObj 9, 0 ; Newobj
    #stack_duplicate ; Dup
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #stack_push_int 2 ; Ldc_i4_2
    #ldelemRef ; Ldelem_ref
    jsr PlatformEnemy_set_Platform ; Callvirt
    nop ; Nop
    #stack_duplicate ; Dup
    #stack_push_int 8 ; Ldc_i4_8
    #stfld 4 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 3 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 2 ; Stfld
    #stack_duplicate ; Dup
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite5 ; Callvirt
    jsr PlatformEnemy_set_Sprite ; Callvirt
    nop ; Nop
    #stack_duplicate ; Dup
    #stack_push_int 2 ; Ldc_i4_2
    jsr PlatformEnemy_set_Color ; Callvirt
    nop ; Nop
    #stelemRef ; Stelem_ref
    #stfld 0 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #newObj 6, 0 ; Newobj
    #stack_duplicate ; Dup
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite4 ; Callvirt
    jsr Player_set_Sprite ; Callvirt
    nop ; Nop
    #stfld 1 ; Stfld
    #locals_method_exit 3 ; Ret


;----------------------------------------
; TYPE: Game
; METHOD: InitPlatforms
;----------------------------------------
Game_InitPlatforms 
    #locals_init_locals 2
    #locals_pull_param_8 0
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 11 ; Ldc_i4_s
    #newArr  ; Newarr
    #stack_duplicate ; Dup
    #stack_push_int 0 ; Ldc_i4_0
    #newObj 7, 0 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 10 ; Ldc_i4_s
    #stfld 0 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 10 ; Ldc_i4_s
    #stfld 1 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 7 ; Ldc_i4_7
    #stfld 6 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 10 ; Ldc_i4_s
    #stfld 5 ; Stfld
    #stelemRef ; Stelem_ref
    #stack_duplicate ; Dup
    #stack_push_int 1 ; Ldc_i4_1
    #newObj 7, 0 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 0 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 23 ; Ldc_i4_s
    #stfld 1 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 5 ; Ldc_i4_5
    #stfld 6 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 10 ; Ldc_i4_s
    #stfld 5 ; Stfld
    #stelemRef ; Stelem_ref
    #stack_duplicate ; Dup
    #stack_push_int 2 ; Ldc_i4_2
    #newObj 7, 0 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 12 ; Ldc_i4_s
    #stfld 0 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 20 ; Ldc_i4_s
    #stfld 1 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 5 ; Ldc_i4_5
    #stfld 6 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 13 ; Ldc_i4_s
    #stfld 5 ; Stfld
    #stelemRef ; Stelem_ref
    #stack_duplicate ; Dup
    #stack_push_int 3 ; Ldc_i4_3
    #newObj 7, 0 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 23 ; Ldc_i4_s
    #stfld 0 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 16 ; Ldc_i4_s
    #stfld 1 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 14 ; Ldc_i4_s
    #stfld 6 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 6 ; Ldc_i4_6
    #stfld 5 ; Stfld
    #stelemRef ; Stelem_ref
    #stack_duplicate ; Dup
    #stack_push_int 4 ; Ldc_i4_4
    #newObj 7, 0 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 26 ; Ldc_i4_s
    #stfld 0 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 12 ; Ldc_i4_s
    #stfld 1 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 14 ; Ldc_i4_s
    #stfld 6 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 5 ; Stfld
    #stelemRef ; Stelem_ref
    #stack_duplicate ; Dup
    #stack_push_int 5 ; Ldc_i4_5
    #newObj 7, 0 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 0 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 9 ; Ldc_i4_s
    #stfld 1 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 7 ; Ldc_i4_7
    #stfld 6 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 9 ; Ldc_i4_s
    #stfld 5 ; Stfld
    #stelemRef ; Stelem_ref
    #stack_duplicate ; Dup
    #stack_push_int 6 ; Ldc_i4_6
    #newObj 7, 0 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 9 ; Ldc_i4_s
    #stfld 0 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 5 ; Ldc_i4_5
    #stfld 1 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 2 ; Ldc_i4_2
    #stfld 6 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 5 ; Stfld
    #stelemRef ; Stelem_ref
    #stack_duplicate ; Dup
    #stack_push_int 7 ; Ldc_i4_7
    #newObj 7, 0 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 15 ; Ldc_i4_s
    #stfld 0 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 5 ; Ldc_i4_5
    #stfld 1 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 2 ; Ldc_i4_2
    #stfld 6 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 5 ; Stfld
    #stelemRef ; Stelem_ref
    #stack_duplicate ; Dup
    #stack_push_int 8 ; Ldc_i4_8
    #newObj 7, 0 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 21 ; Ldc_i4_s
    #stfld 0 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 5 ; Ldc_i4_5
    #stfld 1 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 2 ; Ldc_i4_2
    #stfld 6 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 5 ; Stfld
    #stelemRef ; Stelem_ref
    #stack_duplicate ; Dup
    #stack_push_int 9 ; Ldc_i4_s
    #newObj 7, 0 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 27 ; Ldc_i4_s
    #stfld 0 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 5 ; Ldc_i4_5
    #stfld 1 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 2 ; Ldc_i4_2
    #stfld 6 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 5 ; Stfld
    #stelemRef ; Stelem_ref
    #stack_duplicate ; Dup
    #stack_push_int 10 ; Ldc_i4_s
    #newObj 7, 0 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 33 ; Ldc_i4_s
    #stfld 0 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 5 ; Ldc_i4_5
    #stfld 1 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 2 ; Ldc_i4_2
    #stfld 6 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 5 ; Stfld
    #stelemRef ; Stelem_ref
    #stfld 2 ; Stfld
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #init_var 2, 0 ; Nop
    jmp Game_InitPlatforms_455 ; Br_s
Game_InitPlatforms_437:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #locals_push_value_8 2 ; Ldloc_0
    #ldelemRef ; Ldelem_ref
    jsr Platform_Draw ; Callvirt
    nop ; Nop
  ; OPT   #locals_push_value_8 2 ; Ldloc_0
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #inc_var 2 ; Nop
Game_InitPlatforms_455:    #locals_push_value_8 2 ; Ldloc_0
    nop ;Conv_u8 ; Conv_u8
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #ldlen ; Ldlen
    nop ;Conv_i4 ; Conv_i4
    nop ;Conv_i8 ; Conv_i8
    #compareLess ; Clt
    #locals_pull_value_8 3, 0 ; Stloc_1
    #locals_push_value_8 3 ; Ldloc_1
    #branch_true Game_InitPlatforms_437 ; Brtrue_s
    #locals_method_exit 5 ; Ret


;----------------------------------------
; TYPE: Game
; METHOD: Run
;----------------------------------------
Game_Run 
    #locals_init_locals 4
    #locals_pull_param_8 0
    nop ; Nop
    jmp Game_Run_46 ; Br_s
Game_Run_3:    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    jsr Game_Step ; Call
    nop ; Nop
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #init_var 2, 0 ; Nop
    jmp Game_Run_36 ; Br_s
Game_Run_15:  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #init_var 3, 0 ; Nop
    jmp Game_Run_24 ; Br_s
Game_Run_19:    nop ; Nop
  ; OPT   #locals_push_value_8 3 ; Ldloc_1
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #inc_var 3 ; Nop
Game_Run_24:  ; OPT   #locals_push_value_8 3 ; Ldloc_1
  ; OPT   #stack_push_int 5 ; Ldc_i4_5
  ; OPT   #compareLess ; Clt_un
  ; OPT   #locals_pull_value_8 4, 0 ; Stloc_2
  ; OPT   #locals_push_value_8 4 ; Ldloc_2
  ; OPT   #branch_true Game_Run_19 ; Brtrue_s
    #branch_if_var_less 3, 5, Game_Run_19 ; Nop
  ; OPT   #locals_push_value_8 2 ; Ldloc_0
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #inc_var 2 ; Nop
Game_Run_36:  ; OPT   #locals_push_value_8 2 ; Ldloc_0
  ; OPT   #stack_push_int 100 ; Ldc_i4_s
  ; OPT   #compareLess ; Clt_un
  ; OPT   #locals_pull_value_8 5, 0 ; Stloc_3
  ; OPT   #locals_push_value_8 5 ; Ldloc_3
  ; OPT   #branch_true Game_Run_15 ; Brtrue_s
    #branch_if_var_less 2, 100, Game_Run_15 ; Nop
    nop ; Nop
Game_Run_46:    jmp Game_Run_3 ; Br_s


;----------------------------------------
; TYPE: Game
; METHOD: Step
;----------------------------------------
Game_Step 
    #locals_init_locals 7
    #locals_pull_param_8 0
    nop ; Nop
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #init_var 3, 0 ; Nop
    jmp Game_Step_23 ; Br_s
Game_Step_5:  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #locals_push_value_8 3 ; Ldloc_1
    #ldelemRef ; Ldelem_ref
    jsr PlatformEnemy_Move ; Callvirt
    nop ; Nop
  ; OPT   #locals_push_value_8 3 ; Ldloc_1
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #inc_var 3 ; Nop
Game_Step_23:    #locals_push_value_8 3 ; Ldloc_1
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #ldlen ; Ldlen
    nop ;Conv_i4 ; Conv_i4
    #compareLess ; Clt
    #locals_pull_value_8 4, 0 ; Stloc_2
    #locals_push_value_8 4 ; Ldloc_2
    #branch_true Game_Step_5 ; Brtrue_s
  ; OPT   #stack_push_int 255 ; Ldc_i4
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #init_var 2, 255 ; Nop
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 5, 0 ; Stloc_3
    #init_var 5, 0 ; Nop
    jmp Game_Step_105 ; Br_s
Game_Step_48:    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #locals_push_value_8 5 ; Ldloc_3
    #ldelemRef ; Ldelem_ref
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
    jsr Player_get_X ; Callvirt
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
    jsr Player_get_Y ; Callvirt
    jsr Platform_DistanceToPlatform ; Callvirt
    #locals_pull_value_8 6, 0 ; Stloc_s
    #locals_push_value_8 6 ; Ldloc_s
    #locals_push_value_8 2 ; Ldloc_0
    #compareLess ; Clt_un
    #locals_pull_value_8 7, 0 ; Stloc_s
    #locals_push_value_8 7 ; Ldloc_s
    #branch_false Game_Step_100 ; Brfalse_s
    #locals_push_value_8 6 ; Ldloc_s
    #locals_pull_value_8 2, 0 ; Stloc_0
Game_Step_100:    nop ; Nop
  ; OPT   #locals_push_value_8 5 ; Ldloc_3
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 5, 0 ; Stloc_3
    #inc_var 5 ; Nop
Game_Step_105:    #locals_push_value_8 5 ; Ldloc_3
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld 2 ; Nop
    #ldlen ; Ldlen
    nop ;Conv_i4 ; Conv_i4
    #compareLess ; Clt
    #locals_pull_value_8 8, 0 ; Stloc_s
    #locals_push_value_8 8 ; Ldloc_s
    #branch_true Game_Step_48 ; Brtrue_s
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld 1 ; Nop
    #locals_push_value_8 2 ; Ldloc_0
    jsr Player_Move ; Callvirt
    nop ; Nop
    #locals_method_exit 10 ; Ret


;----------------------------------------
; TYPE: Game
; METHOD: ClearScreen
;----------------------------------------
Game_ClearScreen 
    #locals_init_locals 4
    nop ; Nop
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 1, 0 ; Stloc_0
    #init_var 1, 0 ; Nop
    jmp Game_ClearScreen_42 ; Br_s
Game_ClearScreen_5:    nop ; Nop
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_1
    #init_var 2, 0 ; Nop
    jmp Game_ClearScreen_28 ; Br_s
Game_ClearScreen_10:    nop ; Nop
    #locals_push_value_8 1 ; Ldloc_0
    #locals_push_value_8 2 ; Ldloc_1
    #stack_push_int 32 ; Ldc_i4_s
    #stack_push_int 14 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    nop ; Nop
  ; OPT   #locals_push_value_8 2 ; Ldloc_1
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_1
    #inc_var 2 ; Nop
Game_ClearScreen_28:  ; OPT   #locals_push_value_8 2 ; Ldloc_1
  ; OPT   #stack_push_int 25 ; Ldc_i4_s
  ; OPT   #compareLess ; Clt_un
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_2
  ; OPT   #locals_push_value_8 3 ; Ldloc_2
  ; OPT   #branch_true Game_ClearScreen_10 ; Brtrue_s
    #branch_if_var_less 2, 25, Game_ClearScreen_10 ; Nop
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldloc_0
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 1, 0 ; Stloc_0
    #inc_var 1 ; Nop
Game_ClearScreen_42:  ; OPT   #locals_push_value_8 1 ; Ldloc_0
  ; OPT   #stack_push_int 40 ; Ldc_i4_s
  ; OPT   #compareLess ; Clt_un
  ; OPT   #locals_pull_value_8 4, 0 ; Stloc_3
  ; OPT   #locals_push_value_8 4 ; Ldloc_3
  ; OPT   #branch_true Game_ClearScreen_5 ; Brtrue_s
    #branch_if_var_less 1, 40, Game_ClearScreen_5 ; Nop
    #locals_method_exit 6 ; Ret
