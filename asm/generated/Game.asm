

;----------------------------------------
; TYPE: Game
; METHOD: Init
;----------------------------------------
Game_Init 
    +stack_save_return_adress .Game_Init_ReturnAddress
    +stack_pull_int .Game_Init_this
    nop ; Nop
    jsr Game_ClearScreen ; Call
    nop ; Nop
    +stack_push_int 0 ; Ldc_i4_0
    jsr C64_SetBorderColor ; Call
    nop ; Nop
    +stack_push_int 0 ; Ldc_i4_0
    jsr C64_SetBackgroundColor ; Call
    nop ; Nop
    +stack_push_var .Game_Init_this ; Ldarg_0
    jsr Game_InitPlatforms ; Call
    nop ; Nop
    +stack_push_var .Game_Init_this ; Ldarg_0
    +stack_push_int 2 ; Ldc_i4_2
    +newArr  ; Newarr
    +stack_duplicate ; Dup
    +stack_push_int 0 ; Ldc_i4_0
    +newObj 9, 0 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_var .Game_Init_this ; Ldarg_0
    +ldfld 2 ; Ldfld
    +stack_push_int 0 ; Ldc_i4_0
    +ldelemRef ; Ldelem_ref
    jsr PlatformEnemy_set_Platform ; Callvirt
    nop ; Nop
    +stack_duplicate ; Dup
    +stack_push_int 2 ; Ldc_i4_2
    +stfld 4 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 2 ; Ldc_i4_2
    +stfld 3 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 0 ; Ldc_i4_0
    +stfld 2 ; Stfld
    +stack_duplicate ; Dup
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite2 ; Callvirt
    jsr PlatformEnemy_set_Sprite ; Callvirt
    nop ; Nop
    +stack_duplicate ; Dup
    +stack_push_int 2 ; Ldc_i4_2
    jsr PlatformEnemy_set_Color ; Callvirt
    nop ; Nop
    +stelemRef ; Stelem_ref
    +stack_duplicate ; Dup
    +stack_push_int 1 ; Ldc_i4_1
    +newObj 9, 0 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_var .Game_Init_this ; Ldarg_0
    +ldfld 2 ; Ldfld
    +stack_push_int 2 ; Ldc_i4_2
    +ldelemRef ; Ldelem_ref
    jsr PlatformEnemy_set_Platform ; Callvirt
    nop ; Nop
    +stack_duplicate ; Dup
    +stack_push_int 8 ; Ldc_i4_8
    +stfld 4 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 1 ; Ldc_i4_1
    +stfld 3 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 0 ; Ldc_i4_0
    +stfld 2 ; Stfld
    +stack_duplicate ; Dup
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite5 ; Callvirt
    jsr PlatformEnemy_set_Sprite ; Callvirt
    nop ; Nop
    +stack_duplicate ; Dup
    +stack_push_int 2 ; Ldc_i4_2
    jsr PlatformEnemy_set_Color ; Callvirt
    nop ; Nop
    +stelemRef ; Stelem_ref
    +stfld 0 ; Stfld
    +stack_push_var .Game_Init_this ; Ldarg_0
    +newObj 6, 0 ; Newobj
    +stack_duplicate ; Dup
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite4 ; Callvirt
    jsr Player_set_Sprite ; Callvirt
    nop ; Nop
    +stfld 1 ; Stfld
    +stack_return_to_saved_address .Game_Init_ReturnAddress ; Ret
.Game_Init_this !byte 0, 0
.Game_Init_ReturnAddress !byte 0,0


;----------------------------------------
; TYPE: Game
; METHOD: InitPlatforms
;----------------------------------------
Game_InitPlatforms 
    +stack_save_return_adress .Game_InitPlatforms_ReturnAddress
    +stack_pull_int .Game_InitPlatforms_this
    nop ; Nop
    +stack_push_var .Game_InitPlatforms_this ; Ldarg_0
    +stack_push_int 11 ; Ldc_i4_s
    +newArr  ; Newarr
    +stack_duplicate ; Dup
    +stack_push_int 0 ; Ldc_i4_0
    +newObj 7, 0 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 10 ; Ldc_i4_s
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 10 ; Ldc_i4_s
    +stfld 1 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 7 ; Ldc_i4_7
    +stfld 6 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 10 ; Ldc_i4_s
    +stfld 5 ; Stfld
    +stelemRef ; Stelem_ref
    +stack_duplicate ; Dup
    +stack_push_int 1 ; Ldc_i4_1
    +newObj 7, 0 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 1 ; Ldc_i4_1
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 23 ; Ldc_i4_s
    +stfld 1 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 5 ; Ldc_i4_5
    +stfld 6 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 10 ; Ldc_i4_s
    +stfld 5 ; Stfld
    +stelemRef ; Stelem_ref
    +stack_duplicate ; Dup
    +stack_push_int 2 ; Ldc_i4_2
    +newObj 7, 0 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 12 ; Ldc_i4_s
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 20 ; Ldc_i4_s
    +stfld 1 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 5 ; Ldc_i4_5
    +stfld 6 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 13 ; Ldc_i4_s
    +stfld 5 ; Stfld
    +stelemRef ; Stelem_ref
    +stack_duplicate ; Dup
    +stack_push_int 3 ; Ldc_i4_3
    +newObj 7, 0 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 23 ; Ldc_i4_s
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 16 ; Ldc_i4_s
    +stfld 1 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 14 ; Ldc_i4_s
    +stfld 6 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 6 ; Ldc_i4_6
    +stfld 5 ; Stfld
    +stelemRef ; Stelem_ref
    +stack_duplicate ; Dup
    +stack_push_int 4 ; Ldc_i4_4
    +newObj 7, 0 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 26 ; Ldc_i4_s
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 12 ; Ldc_i4_s
    +stfld 1 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 14 ; Ldc_i4_s
    +stfld 6 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 1 ; Ldc_i4_1
    +stfld 5 ; Stfld
    +stelemRef ; Stelem_ref
    +stack_duplicate ; Dup
    +stack_push_int 5 ; Ldc_i4_5
    +newObj 7, 0 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 0 ; Ldc_i4_0
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 9 ; Ldc_i4_s
    +stfld 1 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 7 ; Ldc_i4_7
    +stfld 6 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 9 ; Ldc_i4_s
    +stfld 5 ; Stfld
    +stelemRef ; Stelem_ref
    +stack_duplicate ; Dup
    +stack_push_int 6 ; Ldc_i4_6
    +newObj 7, 0 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 9 ; Ldc_i4_s
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 5 ; Ldc_i4_5
    +stfld 1 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 2 ; Ldc_i4_2
    +stfld 6 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 1 ; Ldc_i4_1
    +stfld 5 ; Stfld
    +stelemRef ; Stelem_ref
    +stack_duplicate ; Dup
    +stack_push_int 7 ; Ldc_i4_7
    +newObj 7, 0 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 15 ; Ldc_i4_s
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 5 ; Ldc_i4_5
    +stfld 1 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 2 ; Ldc_i4_2
    +stfld 6 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 1 ; Ldc_i4_1
    +stfld 5 ; Stfld
    +stelemRef ; Stelem_ref
    +stack_duplicate ; Dup
    +stack_push_int 8 ; Ldc_i4_8
    +newObj 7, 0 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 21 ; Ldc_i4_s
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 5 ; Ldc_i4_5
    +stfld 1 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 2 ; Ldc_i4_2
    +stfld 6 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 1 ; Ldc_i4_1
    +stfld 5 ; Stfld
    +stelemRef ; Stelem_ref
    +stack_duplicate ; Dup
    +stack_push_int 9 ; Ldc_i4_s
    +newObj 7, 0 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 27 ; Ldc_i4_s
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 5 ; Ldc_i4_5
    +stfld 1 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 2 ; Ldc_i4_2
    +stfld 6 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 1 ; Ldc_i4_1
    +stfld 5 ; Stfld
    +stelemRef ; Stelem_ref
    +stack_duplicate ; Dup
    +stack_push_int 10 ; Ldc_i4_s
    +newObj 7, 0 ; Newobj
    +stack_duplicate ; Dup
    +stack_push_int 33 ; Ldc_i4_s
    +stfld 0 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 5 ; Ldc_i4_5
    +stfld 1 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 2 ; Ldc_i4_2
    +stfld 6 ; Stfld
    +stack_duplicate ; Dup
    +stack_push_int 1 ; Ldc_i4_1
    +stfld 5 ; Stfld
    +stelemRef ; Stelem_ref
    +stfld 2 ; Stfld
  ; OPT   +stack_push_int 0 ; Ldc_i4_0
  ; OPT   +stack_pull_int_ref .Game_InitPlatforms_var0, 0 ; Stloc_0
    +init_var .Game_InitPlatforms_var0, 0 ; Nop
    jmp label_Game_InitPlatforms_455 ; Br_s
label_Game_InitPlatforms_437:    +stack_push_var .Game_InitPlatforms_this ; Ldarg_0
    +ldfld 2 ; Ldfld
    +stack_push_var .Game_InitPlatforms_var0 ; Ldloc_0
    +ldelemRef ; Ldelem_ref
    jsr Platform_Draw ; Callvirt
    nop ; Nop
  ; OPT   +stack_push_var .Game_InitPlatforms_var0 ; Ldloc_0
  ; OPT   +stack_push_int 1 ; Ldc_i4_1
  ; OPT   +add ; Add
  ; OPT   +stack_pull_int_ref .Game_InitPlatforms_var0, 0 ; Stloc_0
    +inc_var .Game_InitPlatforms_var0 ; Nop
label_Game_InitPlatforms_455:    +stack_push_var .Game_InitPlatforms_var0 ; Ldloc_0
    nop ;Conv_u8 ; Conv_u8
    +stack_push_var .Game_InitPlatforms_this ; Ldarg_0
    +ldfld 2 ; Ldfld
    +ldlen ; Ldlen
    nop ;Conv_i4 ; Conv_i4
    nop ;Conv_i8 ; Conv_i8
    +compareLess ; Clt
    +stack_pull_int_ref .Game_InitPlatforms_var1, 0 ; Stloc_1
    +stack_push_var .Game_InitPlatforms_var1 ; Ldloc_1
    +branch_true label_Game_InitPlatforms_437 ; Brtrue_s
    +stack_return_to_saved_address .Game_InitPlatforms_ReturnAddress ; Ret
.Game_InitPlatforms_this !byte 0, 0
.Game_InitPlatforms_ReturnAddress !byte 0,0
.Game_InitPlatforms_var0 !byte 0,0
.Game_InitPlatforms_var1 !byte 0,0


;----------------------------------------
; TYPE: Game
; METHOD: Run
;----------------------------------------
Game_Run 
    +stack_save_return_adress .Game_Run_ReturnAddress
    +stack_pull_int .Game_Run_this
    nop ; Nop
    jmp label_Game_Run_47 ; Br_s
label_Game_Run_3:    nop ; Nop
    +stack_push_var .Game_Run_this ; Ldarg_0
    jsr Game_Step ; Call
    nop ; Nop
  ; OPT   +stack_push_int 0 ; Ldc_i4_0
  ; OPT   +stack_pull_int_ref .Game_Run_var0, 0 ; Stloc_0
    +init_var .Game_Run_var0, 0 ; Nop
    jmp label_Game_Run_37 ; Br_s
label_Game_Run_15:  ; OPT   +stack_push_int 0 ; Ldc_i4_0
  ; OPT   +stack_pull_int_ref .Game_Run_var1, 0 ; Stloc_1
    +init_var .Game_Run_var1, 0 ; Nop
    jmp label_Game_Run_24 ; Br_s
label_Game_Run_19:    nop ; Nop
  ; OPT   +stack_push_var .Game_Run_var1 ; Ldloc_1
  ; OPT   +stack_push_int 1 ; Ldc_i4_1
  ; OPT   +add ; Add
  ; OPT   +stack_pull_int_ref .Game_Run_var1, 0 ; Stloc_1
    +inc_var .Game_Run_var1 ; Nop
label_Game_Run_24:  ; OPT   +stack_push_var .Game_Run_var1 ; Ldloc_1
  ; OPT   +stack_push_int 15 ; Ldc_i4_s
  ; OPT   +compareLess ; Clt_un
  ; OPT   +stack_pull_int_ref .Game_Run_var2, 0 ; Stloc_2
  ; OPT   +stack_push_var .Game_Run_var2 ; Ldloc_2
  ; OPT   +branch_true label_Game_Run_19 ; Brtrue_s
    +branch_if_var_less .Game_Run_var1, 15, label_Game_Run_19 ; Nop
  ; OPT   +stack_push_var .Game_Run_var0 ; Ldloc_0
  ; OPT   +stack_push_int 1 ; Ldc_i4_1
  ; OPT   +add ; Add
  ; OPT   +stack_pull_int_ref .Game_Run_var0, 0 ; Stloc_0
    +inc_var .Game_Run_var0 ; Nop
label_Game_Run_37:  ; OPT   +stack_push_var .Game_Run_var0 ; Ldloc_0
  ; OPT   +stack_push_int 100 ; Ldc_i4_s
  ; OPT   +compareLess ; Clt_un
  ; OPT   +stack_pull_int_ref .Game_Run_var3, 0 ; Stloc_3
  ; OPT   +stack_push_var .Game_Run_var3 ; Ldloc_3
  ; OPT   +branch_true label_Game_Run_15 ; Brtrue_s
    +branch_if_var_less .Game_Run_var0, 100, label_Game_Run_15 ; Nop
    nop ; Nop
label_Game_Run_47:    jmp label_Game_Run_3 ; Br_s
.Game_Run_this !byte 0, 0
.Game_Run_ReturnAddress !byte 0,0
.Game_Run_var0 !byte 0,0
.Game_Run_var1 !byte 0,0
.Game_Run_var2 !byte 0,0
.Game_Run_var3 !byte 0,0


;----------------------------------------
; TYPE: Game
; METHOD: Step
;----------------------------------------
Game_Step 
    +stack_save_return_adress .Game_Step_ReturnAddress
    +stack_pull_int .Game_Step_this
    nop ; Nop
  ; OPT   +stack_push_int 0 ; Ldc_i4_0
  ; OPT   +stack_pull_int_ref .Game_Step_var1, 0 ; Stloc_1
    +init_var .Game_Step_var1, 0 ; Nop
    jmp label_Game_Step_23 ; Br_s
label_Game_Step_5:    +stack_push_var .Game_Step_this ; Ldarg_0
    +ldfld 0 ; Ldfld
    +stack_push_var .Game_Step_var1 ; Ldloc_1
    +ldelemRef ; Ldelem_ref
    jsr PlatformEnemy_Move ; Callvirt
    nop ; Nop
  ; OPT   +stack_push_var .Game_Step_var1 ; Ldloc_1
  ; OPT   +stack_push_int 1 ; Ldc_i4_1
  ; OPT   +add ; Add
  ; OPT   +stack_pull_int_ref .Game_Step_var1, 0 ; Stloc_1
    +inc_var .Game_Step_var1 ; Nop
label_Game_Step_23:    +stack_push_var .Game_Step_var1 ; Ldloc_1
    +stack_push_var .Game_Step_this ; Ldarg_0
    +ldfld 0 ; Ldfld
    +ldlen ; Ldlen
    nop ;Conv_i4 ; Conv_i4
    +compareLess ; Clt
    +stack_pull_int_ref .Game_Step_var2, 0 ; Stloc_2
    +stack_push_var .Game_Step_var2 ; Ldloc_2
    +branch_true label_Game_Step_5 ; Brtrue_s
  ; OPT   +stack_push_int 255 ; Ldc_i4
  ; OPT   +stack_pull_int_ref .Game_Step_var0, 0 ; Stloc_0
    +init_var .Game_Step_var0, 255 ; Nop
  ; OPT   +stack_push_int 0 ; Ldc_i4_0
  ; OPT   +stack_pull_int_ref .Game_Step_var3, 0 ; Stloc_3
    +init_var .Game_Step_var3, 0 ; Nop
    jmp label_Game_Step_105 ; Br_s
label_Game_Step_48:    nop ; Nop
    +stack_push_var .Game_Step_this ; Ldarg_0
    +ldfld 2 ; Ldfld
    +stack_push_var .Game_Step_var3 ; Ldloc_3
    +ldelemRef ; Ldelem_ref
    +stack_push_var .Game_Step_this ; Ldarg_0
    +ldfld 1 ; Ldfld
    jsr Player_get_X ; Callvirt
    +stack_push_var .Game_Step_this ; Ldarg_0
    +ldfld 1 ; Ldfld
    jsr Player_get_Y ; Callvirt
    jsr Platform_DistanceToPlatform ; Callvirt
    +stack_pull_int_ref .Game_Step_var4, 0 ; Stloc_s
    +stack_push_var .Game_Step_var4 ; Ldloc_s
    +stack_push_var .Game_Step_var0 ; Ldloc_0
    +compareLess ; Clt_un
    +stack_pull_int_ref .Game_Step_var5, 0 ; Stloc_s
    +stack_push_var .Game_Step_var5 ; Ldloc_s
    +branch_false label_Game_Step_100 ; Brfalse_s
    +stack_push_var .Game_Step_var4 ; Ldloc_s
    +stack_pull_int_ref .Game_Step_var0, 0 ; Stloc_0
label_Game_Step_100:    nop ; Nop
  ; OPT   +stack_push_var .Game_Step_var3 ; Ldloc_3
  ; OPT   +stack_push_int 1 ; Ldc_i4_1
  ; OPT   +add ; Add
  ; OPT   +stack_pull_int_ref .Game_Step_var3, 0 ; Stloc_3
    +inc_var .Game_Step_var3 ; Nop
label_Game_Step_105:    +stack_push_var .Game_Step_var3 ; Ldloc_3
    +stack_push_var .Game_Step_this ; Ldarg_0
    +ldfld 2 ; Ldfld
    +ldlen ; Ldlen
    nop ;Conv_i4 ; Conv_i4
    +compareLess ; Clt
    +stack_pull_int_ref .Game_Step_var6, 0 ; Stloc_s
    +stack_push_var .Game_Step_var6 ; Ldloc_s
    +branch_true label_Game_Step_48 ; Brtrue_s
    +stack_push_var .Game_Step_this ; Ldarg_0
    +ldfld 1 ; Ldfld
    +stack_push_var .Game_Step_var0 ; Ldloc_0
    jsr Player_Move ; Callvirt
    nop ; Nop
    +stack_return_to_saved_address .Game_Step_ReturnAddress ; Ret
.Game_Step_this !byte 0, 0
.Game_Step_ReturnAddress !byte 0,0
.Game_Step_var0 !byte 0,0
.Game_Step_var1 !byte 0,0
.Game_Step_var2 !byte 0,0
.Game_Step_var3 !byte 0,0
.Game_Step_var4 !byte 0,0
.Game_Step_var5 !byte 0,0
.Game_Step_var6 !byte 0,0


;----------------------------------------
; TYPE: Game
; METHOD: ClearScreen
;----------------------------------------
Game_ClearScreen 
    +stack_save_return_adress .Game_ClearScreen_ReturnAddress
    nop ; Nop
  ; OPT   +stack_push_int 0 ; Ldc_i4_0
  ; OPT   +stack_pull_int_ref .Game_ClearScreen_var0, 0 ; Stloc_0
    +init_var .Game_ClearScreen_var0, 0 ; Nop
    jmp label_Game_ClearScreen_42 ; Br_s
label_Game_ClearScreen_5:    nop ; Nop
  ; OPT   +stack_push_int 0 ; Ldc_i4_0
  ; OPT   +stack_pull_int_ref .Game_ClearScreen_var1, 0 ; Stloc_1
    +init_var .Game_ClearScreen_var1, 0 ; Nop
    jmp label_Game_ClearScreen_28 ; Br_s
label_Game_ClearScreen_10:    nop ; Nop
    +stack_push_var .Game_ClearScreen_var0 ; Ldloc_0
    +stack_push_var .Game_ClearScreen_var1 ; Ldloc_1
    +stack_push_int 32 ; Ldc_i4_s
    +stack_push_int 14 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    nop ; Nop
  ; OPT   +stack_push_var .Game_ClearScreen_var1 ; Ldloc_1
  ; OPT   +stack_push_int 1 ; Ldc_i4_1
  ; OPT   +add ; Add
  ; OPT   +stack_pull_int_ref .Game_ClearScreen_var1, 0 ; Stloc_1
    +inc_var .Game_ClearScreen_var1 ; Nop
label_Game_ClearScreen_28:  ; OPT   +stack_push_var .Game_ClearScreen_var1 ; Ldloc_1
  ; OPT   +stack_push_int 25 ; Ldc_i4_s
  ; OPT   +compareLess ; Clt_un
  ; OPT   +stack_pull_int_ref .Game_ClearScreen_var2, 0 ; Stloc_2
  ; OPT   +stack_push_var .Game_ClearScreen_var2 ; Ldloc_2
  ; OPT   +branch_true label_Game_ClearScreen_10 ; Brtrue_s
    +branch_if_var_less .Game_ClearScreen_var1, 25, label_Game_ClearScreen_10 ; Nop
    nop ; Nop
  ; OPT   +stack_push_var .Game_ClearScreen_var0 ; Ldloc_0
  ; OPT   +stack_push_int 1 ; Ldc_i4_1
  ; OPT   +add ; Add
  ; OPT   +stack_pull_int_ref .Game_ClearScreen_var0, 0 ; Stloc_0
    +inc_var .Game_ClearScreen_var0 ; Nop
label_Game_ClearScreen_42:  ; OPT   +stack_push_var .Game_ClearScreen_var0 ; Ldloc_0
  ; OPT   +stack_push_int 40 ; Ldc_i4_s
  ; OPT   +compareLess ; Clt_un
  ; OPT   +stack_pull_int_ref .Game_ClearScreen_var3, 0 ; Stloc_3
  ; OPT   +stack_push_var .Game_ClearScreen_var3 ; Ldloc_3
  ; OPT   +branch_true label_Game_ClearScreen_5 ; Brtrue_s
    +branch_if_var_less .Game_ClearScreen_var0, 40, label_Game_ClearScreen_5 ; Nop
    +stack_return_to_saved_address .Game_ClearScreen_ReturnAddress ; Ret
.Game_ClearScreen_ReturnAddress !byte 0,0
.Game_ClearScreen_var0 !byte 0,0
.Game_ClearScreen_var1 !byte 0,0
.Game_ClearScreen_var2 !byte 0,0
.Game_ClearScreen_var3 !byte 0,0
