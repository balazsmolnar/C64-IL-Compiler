

;----------------------------------------
; TYPE: Hunchback.LevelPlay
; METHOD: Play
;----------------------------------------
LevelPlay_Play 
    #init_locals_pull_parameters 6, [1,1,0]
    #newObj 4, 0 ; Newobj
    #locals_pull_value_8 4, 1 ; Stloc_0
    #locals_push_value_8 4 ; Ldloc_0
    #locals_push_value_8 2 ; Ldarg_1
    #ldfld 2 ; Ldfld
    #locals_push_value_8 2 ; Ldarg_1
    #ldfld 0 ; Ldfld
    jsr Wall_Draw ; Callvirt
    #locals_push_value_8 3 ; Ldarg_2
    jsr PlayerStats_Draw ; Callvirt
    #newObj 15, 3 ; Newobj
    #stack_duplicate ; Dup
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite0 ; Callvirt
    jsr Player_set_Sprite ; Callvirt
    #locals_pull_value_8 5, 1 ; Stloc_1
    #locals_push_value_8 5 ; Ldloc_1
    #locals_push_value_8 4 ; Ldloc_0
    jsr Player_Init ; Callvirt
    #newObj 6, 0 ; Newobj
    #stack_duplicate ; Dup
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite1 ; Callvirt
    jsr Knight_set_Sprite ; Callvirt
    #locals_pull_value_8 6, 1 ; Stloc_2
    #locals_push_value_8 6 ; Ldloc_2
    jsr Knight_Init ; Callvirt
    jsr LevelPlay_GetReady ; Call
    #stack_push_int 0 ; Ldnull
    #locals_pull_value_8 7, 1 ; Stloc_3
    #locals_push_value_8 2 ; Ldarg_1
    #ldfld 0 ; Ldfld
  ; OPT   #stack_push_int 3 ; Ldc_i4_3
  ; OPT   #branch_not_equal LevelPlay_Play_149 ; Bne_un_s
    #branch_not_equal_const 3, LevelPlay_Play_149 ; Nop
    #newObj 5, 0 ; Newobj
    #stack_duplicate ; Dup
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite3 ; Callvirt
    jsr Rope_set_Sprite1 ; Callvirt
    #stack_duplicate ; Dup
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite4 ; Callvirt
    jsr Rope_set_Sprite2 ; Callvirt
    #locals_pull_value_8 7, 1 ; Stloc_3
    #locals_push_value_8 7 ; Ldloc_3
    jsr Rope_Init ; Callvirt
    jmp LevelPlay_Play_181 ; Br_s
LevelPlay_Play_149:    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite3 ; Callvirt
    #stack_push_int 0 ; Ldc_i4_0
    jsr Sprite_set_Visible ; Callvirt
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite4 ; Callvirt
    #stack_push_int 0 ; Ldc_i4_0
    jsr Sprite_set_Visible ; Callvirt
LevelPlay_Play_181:    #newObj 8, 0 ; Newobj
    #stack_duplicate ; Dup
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite2 ; Callvirt
    jsr Enemy_set_Sprite ; Callvirt
    #locals_pull_value_8 8, 1 ; Stloc_s
    #locals_push_value_8 8 ; Ldloc_s
    #locals_push_value_8 2 ; Ldarg_1
    #ldfld 1 ; Ldfld
    jsr Enemy_Init ; Callvirt
LevelPlay_Play_217:    #locals_push_value_8 5 ; Ldloc_1
    jsr Player_Move ; Callvirt
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Collisions ; Callvirt
    #locals_pull_value_8 9, 0 ; Stloc_s
    #locals_push_value_8 9 ; Ldloc_s
    #stack_push_int 1 ; Ldc_i4_1
    #and8 ; And
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #branch_less_equal_unsigned LevelPlay_Play_264 ; Ble_un_s
    #branch_less_equal_unsigned_const 0, LevelPlay_Play_264 ; Nop
    #locals_push_value_8 9 ; Ldloc_s
    #stack_push_int 6 ; Ldc_i4_6
    #and8 ; And
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #branch_less_equal_unsigned LevelPlay_Play_257 ; Ble_un_s
    #branch_less_equal_unsigned_const 0, LevelPlay_Play_257 ; Nop
    #locals_push_value_8 5 ; Ldloc_1
    jsr Player_Die ; Callvirt
    jmp LevelPlay_Play_264 ; Br_s
LevelPlay_Play_257:    #locals_push_value_8 5 ; Ldloc_1
    #locals_push_value_8 7 ; Ldloc_3
    jsr Player_SetOnRope ; Callvirt
LevelPlay_Play_264:    #locals_push_value_8 5 ; Ldloc_1
    #ldfld 10 ; Ldfld
    #branch_false LevelPlay_Play_274 ; Brfalse_s
    #stack_push_int 0 ; Ldc_i4_0
    #method_exit 11, [2,3,4,5,6,7,8] ; Ret
LevelPlay_Play_274:    #locals_push_value_8 5 ; Ldloc_1
    #ldfld 12 ; Ldfld
    #branch_false LevelPlay_Play_284 ; Brfalse_s
    #stack_push_int 1 ; Ldc_i4_1
    #method_exit 11, [2,3,4,5,6,7,8] ; Ret
LevelPlay_Play_284:    #locals_push_value_8 6 ; Ldloc_2
    jsr Knight_Move ; Callvirt
    #locals_push_value_8 8 ; Ldloc_s
    jsr Enemy_Move ; Callvirt
    #locals_push_value_8 4 ; Ldloc_0
    jsr Wall_Move ; Callvirt
    #locals_push_value_8 7 ; Ldloc_3
    #branch_false LevelPlay_Play_312 ; Brfalse_s
    #locals_push_value_8 7 ; Ldloc_3
    jsr Rope_Move ; Callvirt
LevelPlay_Play_312:    #stack_push_int 100 ; Ldc_i4_s
    jsr Delay_Wait ; Call
    #stack_push_int 11 ; Ldc_i4_s
    jsr C64_IsKeyPressed ; Call
    #branch_false LevelPlay_Play_217 ; Brfalse_s
    #stack_push_int 1 ; Ldc_i4_1
    #method_exit 11, [2,3,4,5,6,7,8] ; Ret


;----------------------------------------
; TYPE: Hunchback.LevelPlay
; METHOD: GetReady
;----------------------------------------
LevelPlay_GetReady 
    #init_locals_pull_parameters 1, []
    #stack_push_int 16 ; Ldc_i4_s
    #stack_push_int 6 ; Ldc_i4_6
    #stack_push_pointer string_1879048509 ; Ldstr
    #stack_push_int 1 ; Ldc_i4_1
    jsr C64_Write ; Call
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 1, 0 ; Stloc_0
    #init_var 1, 0 ; Nop
    jmp LevelPlay_GetReady_29 ; Br_s
LevelPlay_GetReady_18:    #stack_push_int 100 ; Ldc_i4_s
    jsr Delay_Wait ; Call
  ; OPT   #locals_push_value_8 1 ; Ldloc_0
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 1, 0 ; Stloc_0
    #inc_var 1 ; Nop
LevelPlay_GetReady_29:    #locals_push_value_8 1 ; Ldloc_0
  ; OPT   #stack_push_int 40 ; Ldc_i4_s
  ; OPT   #branch_less LevelPlay_GetReady_18 ; Blt_s
    #branch_less_const 40, LevelPlay_GetReady_18 ; Nop
    #stack_push_int 16 ; Ldc_i4_s
    #stack_push_int 6 ; Ldc_i4_6
    #stack_push_pointer string_1879048529 ; Ldstr
    #stack_push_int 1 ; Ldc_i4_1
    jsr C64_Write ; Call
    #method_exit 3, [] ; Ret
