

;----------------------------------------
; TYPE: Hunchback.LevelPlay
; METHOD: Play
;----------------------------------------
LevelPlay_Play 
    #init_locals_pull_parameters 11, [1,1,0]
    nop ; Nop
    #newObj 3, 0 ; Newobj
    #locals_pull_value_8 4, 1 ; Stloc_0
    #locals_push_value_8 4 ; Ldloc_0
    #locals_push_value_8 2 ; Ldarg_1
    #ldfld 2 ; Ldfld
    #locals_push_value_8 2 ; Ldarg_1
    #ldfld 0 ; Ldfld
    jsr Wall_Draw ; Callvirt
    nop ; Nop
    #locals_push_value_8 3 ; Ldarg_2
    jsr PlayerStats_Draw ; Callvirt
    nop ; Nop
    #newObj 12, 2 ; Newobj
    #stack_duplicate ; Dup
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite0 ; Callvirt
    jsr Player_set_Sprite ; Callvirt
    nop ; Nop
    #locals_pull_value_8 5, 1 ; Stloc_1
    #locals_push_value_8 5 ; Ldloc_1
    #locals_push_value_8 4 ; Ldloc_0
    jsr Player_Init ; Callvirt
    nop ; Nop
    #newObj 6, 0 ; Newobj
    #stack_duplicate ; Dup
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite1 ; Callvirt
    jsr Knight_set_Sprite ; Callvirt
    nop ; Nop
    #locals_pull_value_8 6, 1 ; Stloc_2
    #locals_push_value_8 6 ; Ldloc_2
    jsr Knight_Init ; Callvirt
    nop ; Nop
    jsr LevelPlay_GetReady ; Call
    nop ; Nop
    #stack_push_int 0 ; Ldnull
    #locals_pull_value_8 7, 1 ; Stloc_3
    #locals_push_value_8 2 ; Ldarg_1
    #ldfld 0 ; Ldfld
    #stack_push_int 3 ; Ldc_i4_3
    #compareEqual ; Ceq
    #locals_pull_value_8 9, 0 ; Stloc_s
    #locals_push_value_8 9 ; Ldloc_s
    #branch_false LevelPlay_Play_168 ; Brfalse_s
    nop ; Nop
    #newObj 3, 0 ; Newobj
    #stack_duplicate ; Dup
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite2 ; Callvirt
    jsr Rope_set_Sprite1 ; Callvirt
    nop ; Nop
    #stack_duplicate ; Dup
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite3 ; Callvirt
    jsr Rope_set_Sprite2 ; Callvirt
    nop ; Nop
    #locals_pull_value_8 7, 1 ; Stloc_3
    #locals_push_value_8 7 ; Ldloc_3
    jsr Rope_Init ; Callvirt
    nop ; Nop
    nop ; Nop
    jmp LevelPlay_Play_204 ; Br_s
LevelPlay_Play_168:    nop ; Nop
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite2 ; Callvirt
    #stack_push_int 0 ; Ldc_i4_0
    jsr Sprite_set_Visible ; Callvirt
    nop ; Nop
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite3 ; Callvirt
    #stack_push_int 0 ; Ldc_i4_0
    jsr Sprite_set_Visible ; Callvirt
    nop ; Nop
    nop ; Nop
LevelPlay_Play_204:    #newObj 6, 0 ; Newobj
    #stack_duplicate ; Dup
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite4 ; Callvirt
    jsr Enemy_set_Sprite ; Callvirt
    nop ; Nop
    #locals_pull_value_8 8, 1 ; Stloc_s
    #locals_push_value_8 8 ; Ldloc_s
    #locals_push_value_8 2 ; Ldarg_1
    #ldfld 1 ; Ldfld
    jsr Enemy_Init ; Callvirt
    nop ; Nop
    jmp LevelPlay_Play_352 ; Br_s
LevelPlay_Play_244:    nop ; Nop
    #locals_push_value_8 5 ; Ldloc_1
    jsr Player_Move ; Callvirt
    nop ; Nop
    #locals_push_value_8 5 ; Ldloc_1
    #ldfld 8 ; Ldfld
    #locals_pull_value_8 10, 0 ; Stloc_s
    #locals_push_value_8 10 ; Ldloc_s
    #branch_false LevelPlay_Play_269 ; Brfalse_s
    #stack_push_int 0 ; Ldc_i4_0
    #locals_pull_value_8 11, 0 ; Stloc_s
    jmp LevelPlay_Play_354 ; Br_s
LevelPlay_Play_269:    #locals_push_value_8 5 ; Ldloc_1
    #ldfld 9 ; Ldfld
    #locals_pull_value_8 12, 0 ; Stloc_s
    #locals_push_value_8 12 ; Ldloc_s
    #branch_false LevelPlay_Play_286 ; Brfalse_s
    #stack_push_int 1 ; Ldc_i4_1
    #locals_pull_value_8 11, 0 ; Stloc_s
    jmp LevelPlay_Play_354 ; Br_s
LevelPlay_Play_286:    #locals_push_value_8 6 ; Ldloc_2
    jsr Knight_Move ; Callvirt
    nop ; Nop
    #locals_push_value_8 8 ; Ldloc_s
    jsr Enemy_Move ; Callvirt
    nop ; Nop
    #locals_push_value_8 4 ; Ldloc_0
    jsr Wall_Move ; Callvirt
    nop ; Nop
    #locals_push_value_8 7 ; Ldloc_3
    #stack_push_int 0 ; Ldnull
    #compareGreater_unsigned ; Cgt_un
    #locals_pull_value_8 13, 0 ; Stloc_s
    #locals_push_value_8 13 ; Ldloc_s
    #branch_false LevelPlay_Play_325 ; Brfalse_s
    #locals_push_value_8 7 ; Ldloc_3
    jsr Rope_Move ; Callvirt
    nop ; Nop
LevelPlay_Play_325:    #stack_push_int 100 ; Ldc_i4_s
    jsr Delay_Wait ; Call
    nop ; Nop
    #stack_push_int 11 ; Ldc_i4_s
    jsr C64_IsKeyPressed ; Call
    #locals_pull_value_8 14, 0 ; Stloc_s
    #locals_push_value_8 14 ; Ldloc_s
    #branch_false LevelPlay_Play_351 ; Brfalse_s
    #stack_push_int 1 ; Ldc_i4_1
    #locals_pull_value_8 11, 0 ; Stloc_s
    jmp LevelPlay_Play_354 ; Br_s
LevelPlay_Play_351:    nop ; Nop
LevelPlay_Play_352:    jmp LevelPlay_Play_244 ; Br_s
LevelPlay_Play_354:    #locals_push_value_8 11 ; Ldloc_s
    #method_exit 16, [2,3,4,5,6,7,8] ; Ret


;----------------------------------------
; TYPE: Hunchback.LevelPlay
; METHOD: GetReady
;----------------------------------------
LevelPlay_GetReady 
    #init_locals_pull_parameters 2, []
    nop ; Nop
    #stack_push_int 16 ; Ldc_i4_s
    #stack_push_int 6 ; Ldc_i4_6
    #stack_push_pointer string_1879048477 ; Ldstr
    #stack_push_int 1 ; Ldc_i4_1
    jsr C64_Write ; Call
    nop ; Nop
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 1, 0 ; Stloc_0
    #init_var 1, 0 ; Nop
    jmp LevelPlay_GetReady_32 ; Br_s
LevelPlay_GetReady_20:    #stack_push_int 100 ; Ldc_i4_s
    jsr Delay_Wait ; Call
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldloc_0
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 1, 0 ; Stloc_0
    #inc_var 1 ; Nop
LevelPlay_GetReady_32:  ; OPT   #locals_push_value_8 1 ; Ldloc_0
  ; OPT   #stack_push_int 40 ; Ldc_i4_s
  ; OPT   #compareLess ; Clt
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_1
  ; OPT   #locals_push_value_8 2 ; Ldloc_1
  ; OPT   #branch_true LevelPlay_GetReady_20 ; Brtrue_s
    #branch_if_var_less 1, 40, LevelPlay_GetReady_20 ; Nop
    #stack_push_int 16 ; Ldc_i4_s
    #stack_push_int 6 ; Ldc_i4_6
    #stack_push_pointer string_1879048497 ; Ldstr
    #stack_push_int 1 ; Ldc_i4_1
    jsr C64_Write ; Call
    nop ; Nop
    #method_exit 4, [] ; Ret
