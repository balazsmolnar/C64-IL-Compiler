.Ball_field_67108865 !byte 0,0
.Ball_field_67108866 !byte 0,0
.Ball_field_67108867 !byte 0,0
.Ball_field_67108868 !byte 0,0


Ball_get_X 
+stack_save_return_adress .Ball_get_X_ReturnAddress
+stack_pull_int .Ball_get_X_this
    nop
    +stack_push_var .Ball_get_X_this
    +ldfld 0
    +stack_pull_int .Ball_get_X_var0
    jmp label_Ball_get_X_10
label_Ball_get_X_10:    +stack_push_var .Ball_get_X_var0
    +stack_return_to_saved_address .Ball_get_X_ReturnAddress
.Ball_get_X_this !byte 0,0
.Ball_get_X_ReturnAddress !byte 0,0
.Ball_get_X_var0 !byte 0,0


Ball_set_X 
+stack_save_return_adress .Ball_set_X_ReturnAddress
+stack_pull_int .Ball_set_X_value
+stack_pull_int .Ball_set_X_this
    nop
    +stack_push_var .Ball_set_X_this
    +stack_push_var .Ball_set_X_value
    +stfld 0
    +stack_push_var .Ball_set_X_this
    +ldfld 4
    +stack_push_var .Ball_set_X_value
    jsr Sprite_set_X
    nop
    +stack_return_to_saved_address .Ball_set_X_ReturnAddress
.Ball_set_X_this !byte 0,0
.Ball_set_X_value !byte 0,0
.Ball_set_X_ReturnAddress !byte 0,0


Ball_get_Y 
+stack_save_return_adress .Ball_get_Y_ReturnAddress
+stack_pull_int .Ball_get_Y_this
    nop
    +stack_push_var .Ball_get_Y_this
    +ldfld 1
    +stack_pull_int .Ball_get_Y_var0
    jmp label_Ball_get_Y_10
label_Ball_get_Y_10:    +stack_push_var .Ball_get_Y_var0
    +stack_return_to_saved_address .Ball_get_Y_ReturnAddress
.Ball_get_Y_this !byte 0,0
.Ball_get_Y_ReturnAddress !byte 0,0
.Ball_get_Y_var0 !byte 0,0


Ball_set_Y 
+stack_save_return_adress .Ball_set_Y_ReturnAddress
+stack_pull_int .Ball_set_Y_value
+stack_pull_int .Ball_set_Y_this
    nop
    +stack_push_var .Ball_set_Y_this
    +stack_push_var .Ball_set_Y_value
    +stfld 1
    +stack_push_var .Ball_set_Y_this
    +ldfld 4
    +stack_push_var .Ball_set_Y_value
    jsr Sprite_set_Y
    nop
    +stack_return_to_saved_address .Ball_set_Y_ReturnAddress
.Ball_set_Y_this !byte 0,0
.Ball_set_Y_value !byte 0,0
.Ball_set_Y_ReturnAddress !byte 0,0


Ball_get_VX 
+stack_save_return_adress .Ball_get_VX_ReturnAddress
+stack_pull_int .Ball_get_VX_this
    +stack_push_var .Ball_get_VX_this
    +ldfld 2
    +stack_return_to_saved_address .Ball_get_VX_ReturnAddress
.Ball_get_VX_this !byte 0,0
.Ball_get_VX_ReturnAddress !byte 0,0


Ball_set_VX 
+stack_save_return_adress .Ball_set_VX_ReturnAddress
+stack_pull_int .Ball_set_VX_value
+stack_pull_int .Ball_set_VX_this
    +stack_push_var .Ball_set_VX_this
    +stack_push_var .Ball_set_VX_value
    +stfld 2
    +stack_return_to_saved_address .Ball_set_VX_ReturnAddress
.Ball_set_VX_this !byte 0,0
.Ball_set_VX_value !byte 0,0
.Ball_set_VX_ReturnAddress !byte 0,0


Ball_get_VY 
+stack_save_return_adress .Ball_get_VY_ReturnAddress
+stack_pull_int .Ball_get_VY_this
    +stack_push_var .Ball_get_VY_this
    +ldfld 3
    +stack_return_to_saved_address .Ball_get_VY_ReturnAddress
.Ball_get_VY_this !byte 0,0
.Ball_get_VY_ReturnAddress !byte 0,0


Ball_set_VY 
+stack_save_return_adress .Ball_set_VY_ReturnAddress
+stack_pull_int .Ball_set_VY_value
+stack_pull_int .Ball_set_VY_this
    +stack_push_var .Ball_set_VY_this
    +stack_push_var .Ball_set_VY_value
    +stfld 3
    +stack_return_to_saved_address .Ball_set_VY_ReturnAddress
.Ball_set_VY_this !byte 0,0
.Ball_set_VY_value !byte 0,0
.Ball_set_VY_ReturnAddress !byte 0,0


Ball_set_Sprite 
+stack_save_return_adress .Ball_set_Sprite_ReturnAddress
+stack_pull_int .Ball_set_Sprite_value
+stack_pull_int .Ball_set_Sprite_this
    nop
    +stack_push_var .Ball_set_Sprite_this
    +stack_push_var .Ball_set_Sprite_value
    +stfld 4
    +stack_push_var .Ball_set_Sprite_this
    +ldfld 4
    +stack_push_int 0
    jsr Sprite_set_DataBlock
    nop
    +stack_push_var .Ball_set_Sprite_this
    +stack_push_int 20
    jsr Ball_set_X
    nop
    +stack_push_var .Ball_set_Sprite_this
    +stack_push_int 45
    jsr Ball_set_Y
    nop
    +stack_push_var .Ball_set_Sprite_this
    +ldfld 4
    +stack_push_int 1
    jsr Sprite_set_Visible
    nop
    +stack_return_to_saved_address .Ball_set_Sprite_ReturnAddress
.Ball_set_Sprite_this !byte 0,0
.Ball_set_Sprite_value !byte 0,0
.Ball_set_Sprite_ReturnAddress !byte 0,0


Ball_set_BallColor 
+stack_save_return_adress .Ball_set_BallColor_ReturnAddress
+stack_pull_int .Ball_set_BallColor_value
+stack_pull_int .Ball_set_BallColor_this
    nop
    +stack_push_var .Ball_set_BallColor_this
    +ldfld 4
    +stack_push_var .Ball_set_BallColor_value
    jsr Sprite_set_Color
    nop
    +stack_return_to_saved_address .Ball_set_BallColor_ReturnAddress
.Ball_set_BallColor_this !byte 0,0
.Ball_set_BallColor_value !byte 0,0
.Ball_set_BallColor_ReturnAddress !byte 0,0


Ball_Move 
+stack_save_return_adress .Ball_Move_ReturnAddress
+stack_pull_int .Ball_Move_this
    nop
    +stack_push_var .Ball_Move_this
    +stack_push_var .Ball_Move_this
    jsr Ball_get_VX
    nop ;Conv_i8
    +stack_push_var .Ball_Move_this
    jsr Ball_get_X
    nop ;Conv_u8
    +add
    nop ;Conv_u4
    jsr Ball_set_X
    nop
    +stack_push_var .Ball_Move_this
    +stack_push_var .Ball_Move_this
    jsr Ball_get_VY
    nop ;Conv_i8
    +stack_push_var .Ball_Move_this
    jsr Ball_get_Y
    nop ;Conv_u8
    +add
    nop ;Conv_u4
    jsr Ball_set_Y
    nop
    +stack_push_var .Ball_Move_this
    jsr Ball_get_X
    +stack_push_int 250
    +compareGreater
    +stack_pull_int .Ball_Move_var0
    +stack_push_var .Ball_Move_var0
    +branch_false label_Ball_Move_78
    +stack_push_var .Ball_Move_this
    +stack_push_var .Ball_Move_this
    jsr Ball_get_VX
    +negate
    jsr Ball_set_VX
    nop
label_Ball_Move_78:    +stack_push_var .Ball_Move_this
    jsr Ball_get_X
    +stack_push_int 20
    +compareLess
    +stack_pull_int .Ball_Move_var1
    +stack_push_var .Ball_Move_var1
    +branch_false label_Ball_Move_106
    +stack_push_var .Ball_Move_this
    +stack_push_var .Ball_Move_this
    jsr Ball_get_VX
    +negate
    jsr Ball_set_VX
    nop
label_Ball_Move_106:    +stack_push_var .Ball_Move_this
    jsr Ball_get_Y
    +stack_push_int 228
    +compareGreater
    +stack_pull_int .Ball_Move_var2
    +stack_push_var .Ball_Move_var2
    +branch_false label_Ball_Move_137
    +stack_push_var .Ball_Move_this
    +stack_push_var .Ball_Move_this
    jsr Ball_get_VY
    +negate
    jsr Ball_set_VY
    nop
label_Ball_Move_137:    +stack_push_var .Ball_Move_this
    jsr Ball_get_Y
    +stack_push_int 45
    +compareLess
    +stack_pull_int .Ball_Move_var3
    +stack_push_var .Ball_Move_var3
    +branch_false label_Ball_Move_165
    +stack_push_var .Ball_Move_this
    +stack_push_var .Ball_Move_this
    jsr Ball_get_VY
    +negate
    jsr Ball_set_VY
    nop
label_Ball_Move_165:    +stack_return_to_saved_address .Ball_Move_ReturnAddress
.Ball_Move_this !byte 0,0
.Ball_Move_ReturnAddress !byte 0,0
.Ball_Move_var0 !byte 0,0
.Ball_Move_var1 !byte 0,0
.Ball_Move_var2 !byte 0,0
.Ball_Move_var3 !byte 0,0


Ball_RunBalls 
+stack_save_return_adress .Ball_RunBalls_ReturnAddress
    nop
    +newObj 5
    +stack_pull_int .Ball_RunBalls_var0
    +stack_push_var .Ball_RunBalls_var0
    jsr C64_get_Sprites
    jsr SpriteCollection_get_Sprite0
    jsr Ball_set_Sprite
    nop
    +stack_push_var .Ball_RunBalls_var0
    +stack_push_int 1
    jsr Ball_set_VX
    nop
    +stack_push_var .Ball_RunBalls_var0
    +stack_push_int 1
    jsr Ball_set_VY
    nop
    +stack_push_var .Ball_RunBalls_var0
    +stack_push_int 70
    jsr Ball_set_Y
    nop
    +stack_push_var .Ball_RunBalls_var0
    +stack_push_int 2
    jsr Ball_set_BallColor
    nop
    +newObj 5
    +stack_pull_int .Ball_RunBalls_var1
    +stack_push_var .Ball_RunBalls_var1
    jsr C64_get_Sprites
    jsr SpriteCollection_get_Sprite1
    jsr Ball_set_Sprite
    nop
    +stack_push_var .Ball_RunBalls_var1
    +stack_push_int 1
    jsr Ball_set_VX
    nop
    +stack_push_var .Ball_RunBalls_var1
    +stack_push_int 2
    jsr Ball_set_VY
    nop
    +stack_push_var .Ball_RunBalls_var1
    +stack_push_int 100
    jsr Ball_set_X
    nop
    +stack_push_var .Ball_RunBalls_var1
    +stack_push_int 7
    jsr Ball_set_BallColor
    nop
    +newObj 5
    +stack_pull_int .Ball_RunBalls_var2
    +stack_push_var .Ball_RunBalls_var2
    jsr C64_get_Sprites
    jsr SpriteCollection_get_Sprite2
    jsr Ball_set_Sprite
    nop
    +stack_push_var .Ball_RunBalls_var2
    +stack_push_int 2
    jsr Ball_set_VX
    nop
    +stack_push_var .Ball_RunBalls_var2
    +stack_push_int 1
    jsr Ball_set_VY
    nop
    +stack_push_var .Ball_RunBalls_var2
    +stack_push_int 120
    jsr Ball_set_X
    nop
    +stack_push_var .Ball_RunBalls_var2
    +stack_push_int 4
    jsr Ball_set_BallColor
    nop
    jmp label_Ball_RunBalls_217
label_Ball_RunBalls_171:    nop
    +stack_push_var .Ball_RunBalls_var0
    jsr Ball_Move
    nop
    +stack_push_var .Ball_RunBalls_var1
    jsr Ball_Move
    nop
    +stack_push_var .Ball_RunBalls_var2
    jsr Ball_Move
    nop
  ;  +stack_push_int 0
  ;  +stack_pull_int .Ball_RunBalls_var3
    +init_var .Ball_RunBalls_var3, 0
    jmp label_Ball_RunBalls_202
label_Ball_RunBalls_197:    nop
  ;  +stack_push_var .Ball_RunBalls_var3
  ;  +stack_push_int 1
  ;  +add
  ;  +stack_pull_int .Ball_RunBalls_var3
    +inc_var .Ball_RunBalls_var3
label_Ball_RunBalls_202:    +stack_push_var .Ball_RunBalls_var3
    +stack_push_int 255
    +compareLess
    +stack_pull_int .Ball_RunBalls_var4
    +stack_push_var .Ball_RunBalls_var4
    +branch_true label_Ball_RunBalls_197
    nop
label_Ball_RunBalls_217:    jmp label_Ball_RunBalls_171
.Ball_RunBalls_ReturnAddress !byte 0,0
.Ball_RunBalls_var0 !byte 0,0
.Ball_RunBalls_var1 !byte 0,0
.Ball_RunBalls_var2 !byte 0,0
.Ball_RunBalls_var3 !byte 0,0
.Ball_RunBalls_var4 !byte 0,0


Game_Init 
+stack_save_return_adress .Game_Init_ReturnAddress
+stack_pull_int .Game_Init_this
    nop
    jsr Game_ClearScreen
    nop
    +stack_push_int 0
    jsr C64_SetBorderColor
    nop
    +stack_push_int 0
    jsr C64_SetBackgroundColor
    nop
    +stack_push_var .Game_Init_this
    jsr Game_InitPlatforms
    nop
    +stack_push_var .Game_Init_this
    +stack_push_int 3
    +newArr 
    +stack_duplicate
    +stack_push_int 0
    +newObj 9
    +stack_duplicate
    +stack_push_var .Game_Init_this
    +ldfld 2
    +stack_push_int 0
    +ldelemRef
    jsr PlatformEnemy_set_Platform
    nop
    +stack_duplicate
    +stack_push_int 2
    +stfld 4
    +stack_duplicate
    +stack_push_int 2
    +stfld 3
    +stack_duplicate
    +stack_push_int 0
    +stfld 2
    +stack_duplicate
    jsr C64_get_Sprites
    jsr SpriteCollection_get_Sprite2
    jsr PlatformEnemy_set_Sprite
    nop
    +stack_duplicate
    +stack_push_int 2
    jsr PlatformEnemy_set_Color
    nop
    +stelemRef
    +stack_duplicate
    +stack_push_int 1
    +newObj 9
    +stack_duplicate
    +stack_push_var .Game_Init_this
    +ldfld 2
    +stack_push_int 2
    +ldelemRef
    jsr PlatformEnemy_set_Platform
    nop
    +stack_duplicate
    +stack_push_int 8
    +stfld 4
    +stack_duplicate
    +stack_push_int 1
    +stfld 3
    +stack_duplicate
    +stack_push_int 0
    +stfld 2
    +stack_duplicate
    jsr C64_get_Sprites
    jsr SpriteCollection_get_Sprite5
    jsr PlatformEnemy_set_Sprite
    nop
    +stack_duplicate
    +stack_push_int 2
    jsr PlatformEnemy_set_Color
    nop
    +stelemRef
    +stack_duplicate
    +stack_push_int 2
    +newObj 9
    +stack_duplicate
    +stack_push_int 250
    jsr PlatformEnemy_set_MaxX
    nop
    +stack_duplicate
    +stack_push_int 50
    jsr PlatformEnemy_set_MinX
    nop
    +stack_duplicate
    +stack_push_int 60
    jsr PlatformEnemy_set_Y
    nop
    +stack_duplicate
    +stack_push_int 4
    +stfld 4
    +stack_duplicate
    +stack_push_int 4
    +stfld 3
    +stack_duplicate
    +stack_push_int 9
    +stfld 2
    +stack_duplicate
    jsr C64_get_Sprites
    jsr SpriteCollection_get_Sprite1
    jsr PlatformEnemy_set_Sprite
    nop
    +stack_duplicate
    +stack_push_int 7
    jsr PlatformEnemy_set_Color
    nop
    +stelemRef
    +stfld 0
    +stack_push_var .Game_Init_this
    +newObj 6
    +stack_duplicate
    jsr C64_get_Sprites
    jsr SpriteCollection_get_Sprite4
    jsr Player_set_Sprite
    nop
    +stfld 1
    +stack_return_to_saved_address .Game_Init_ReturnAddress
.Game_Init_this !byte 0,0
.Game_Init_ReturnAddress !byte 0,0


Game_InitPlatforms 
+stack_save_return_adress .Game_InitPlatforms_ReturnAddress
+stack_pull_int .Game_InitPlatforms_this
    nop
    +stack_push_var .Game_InitPlatforms_this
    +stack_push_int 11
    +newArr 
    +stack_duplicate
    +stack_push_int 0
    +newObj 7
    +stack_duplicate
    +stack_push_int 10
    +stfld 0
    +stack_duplicate
    +stack_push_int 10
    +stfld 1
    +stack_duplicate
    +stack_push_int 7
    +stfld 6
    +stack_duplicate
    +stack_push_int 10
    +stfld 5
    +stelemRef
    +stack_duplicate
    +stack_push_int 1
    +newObj 7
    +stack_duplicate
    +stack_push_int 1
    +stfld 0
    +stack_duplicate
    +stack_push_int 23
    +stfld 1
    +stack_duplicate
    +stack_push_int 5
    +stfld 6
    +stack_duplicate
    +stack_push_int 10
    +stfld 5
    +stelemRef
    +stack_duplicate
    +stack_push_int 2
    +newObj 7
    +stack_duplicate
    +stack_push_int 12
    +stfld 0
    +stack_duplicate
    +stack_push_int 20
    +stfld 1
    +stack_duplicate
    +stack_push_int 5
    +stfld 6
    +stack_duplicate
    +stack_push_int 13
    +stfld 5
    +stelemRef
    +stack_duplicate
    +stack_push_int 3
    +newObj 7
    +stack_duplicate
    +stack_push_int 23
    +stfld 0
    +stack_duplicate
    +stack_push_int 16
    +stfld 1
    +stack_duplicate
    +stack_push_int 14
    +stfld 6
    +stack_duplicate
    +stack_push_int 6
    +stfld 5
    +stelemRef
    +stack_duplicate
    +stack_push_int 4
    +newObj 7
    +stack_duplicate
    +stack_push_int 26
    +stfld 0
    +stack_duplicate
    +stack_push_int 12
    +stfld 1
    +stack_duplicate
    +stack_push_int 14
    +stfld 6
    +stack_duplicate
    +stack_push_int 1
    +stfld 5
    +stelemRef
    +stack_duplicate
    +stack_push_int 5
    +newObj 7
    +stack_duplicate
    +stack_push_int 0
    +stfld 0
    +stack_duplicate
    +stack_push_int 9
    +stfld 1
    +stack_duplicate
    +stack_push_int 7
    +stfld 6
    +stack_duplicate
    +stack_push_int 9
    +stfld 5
    +stelemRef
    +stack_duplicate
    +stack_push_int 6
    +newObj 7
    +stack_duplicate
    +stack_push_int 9
    +stfld 0
    +stack_duplicate
    +stack_push_int 5
    +stfld 1
    +stack_duplicate
    +stack_push_int 2
    +stfld 6
    +stack_duplicate
    +stack_push_int 1
    +stfld 5
    +stelemRef
    +stack_duplicate
    +stack_push_int 7
    +newObj 7
    +stack_duplicate
    +stack_push_int 15
    +stfld 0
    +stack_duplicate
    +stack_push_int 5
    +stfld 1
    +stack_duplicate
    +stack_push_int 2
    +stfld 6
    +stack_duplicate
    +stack_push_int 1
    +stfld 5
    +stelemRef
    +stack_duplicate
    +stack_push_int 8
    +newObj 7
    +stack_duplicate
    +stack_push_int 21
    +stfld 0
    +stack_duplicate
    +stack_push_int 5
    +stfld 1
    +stack_duplicate
    +stack_push_int 2
    +stfld 6
    +stack_duplicate
    +stack_push_int 1
    +stfld 5
    +stelemRef
    +stack_duplicate
    +stack_push_int 9
    +newObj 7
    +stack_duplicate
    +stack_push_int 27
    +stfld 0
    +stack_duplicate
    +stack_push_int 5
    +stfld 1
    +stack_duplicate
    +stack_push_int 2
    +stfld 6
    +stack_duplicate
    +stack_push_int 1
    +stfld 5
    +stelemRef
    +stack_duplicate
    +stack_push_int 10
    +newObj 7
    +stack_duplicate
    +stack_push_int 33
    +stfld 0
    +stack_duplicate
    +stack_push_int 5
    +stfld 1
    +stack_duplicate
    +stack_push_int 2
    +stfld 6
    +stack_duplicate
    +stack_push_int 1
    +stfld 5
    +stelemRef
    +stfld 2
  ;  +stack_push_int 0
  ;  +stack_pull_int .Game_InitPlatforms_var0
    +init_var .Game_InitPlatforms_var0, 0
    jmp label_Game_InitPlatforms_455
label_Game_InitPlatforms_437:    +stack_push_var .Game_InitPlatforms_this
    +ldfld 2
    +stack_push_var .Game_InitPlatforms_var0
    +ldelemRef
    jsr Platform_Draw
    nop
  ;  +stack_push_var .Game_InitPlatforms_var0
  ;  +stack_push_int 1
  ;  +add
  ;  +stack_pull_int .Game_InitPlatforms_var0
    +inc_var .Game_InitPlatforms_var0
label_Game_InitPlatforms_455:    +stack_push_var .Game_InitPlatforms_var0
    nop ;Conv_u8
    +stack_push_var .Game_InitPlatforms_this
    +ldfld 2
    +ldlen
    nop ;Conv_i4
    nop ;Conv_i8
    +compareLess
    +stack_pull_int .Game_InitPlatforms_var1
    +stack_push_var .Game_InitPlatforms_var1
    +branch_true label_Game_InitPlatforms_437
    +stack_return_to_saved_address .Game_InitPlatforms_ReturnAddress
.Game_InitPlatforms_this !byte 0,0
.Game_InitPlatforms_ReturnAddress !byte 0,0
.Game_InitPlatforms_var0 !byte 0,0
.Game_InitPlatforms_var1 !byte 0,0


Game_Run 
+stack_save_return_adress .Game_Run_ReturnAddress
+stack_pull_int .Game_Run_this
    nop
    jmp label_Game_Run_47
label_Game_Run_3:    nop
    +stack_push_var .Game_Run_this
    jsr Game_Step
    nop
  ;  +stack_push_int 0
  ;  +stack_pull_int .Game_Run_var0
    +init_var .Game_Run_var0, 0
    jmp label_Game_Run_37
label_Game_Run_15:  ;  +stack_push_int 0
  ;  +stack_pull_int .Game_Run_var1
    +init_var .Game_Run_var1, 0
    jmp label_Game_Run_24
label_Game_Run_19:    nop
  ;  +stack_push_var .Game_Run_var1
  ;  +stack_push_int 1
  ;  +add
  ;  +stack_pull_int .Game_Run_var1
    +inc_var .Game_Run_var1
label_Game_Run_24:  ;  +stack_push_var .Game_Run_var1
  ;  +stack_push_int 15
  ;  +compareLess
  ;  +stack_pull_int .Game_Run_var2
  ;  +stack_push_var .Game_Run_var2
  ;  +branch_true label_Game_Run_19
    +branch_if_var_less .Game_Run_var1, 15, label_Game_Run_19
  ;  +stack_push_var .Game_Run_var0
  ;  +stack_push_int 1
  ;  +add
  ;  +stack_pull_int .Game_Run_var0
    +inc_var .Game_Run_var0
label_Game_Run_37:  ;  +stack_push_var .Game_Run_var0
  ;  +stack_push_int 100
  ;  +compareLess
  ;  +stack_pull_int .Game_Run_var3
  ;  +stack_push_var .Game_Run_var3
  ;  +branch_true label_Game_Run_15
    +branch_if_var_less .Game_Run_var0, 100, label_Game_Run_15
    nop
label_Game_Run_47:    jmp label_Game_Run_3
.Game_Run_this !byte 0,0
.Game_Run_ReturnAddress !byte 0,0
.Game_Run_var0 !byte 0,0
.Game_Run_var1 !byte 0,0
.Game_Run_var2 !byte 0,0
.Game_Run_var3 !byte 0,0


Game_Step 
+stack_save_return_adress .Game_Step_ReturnAddress
+stack_pull_int .Game_Step_this
    nop
  ;  +stack_push_int 0
  ;  +stack_pull_int .Game_Step_var1
    +init_var .Game_Step_var1, 0
    jmp label_Game_Step_23
label_Game_Step_5:    +stack_push_var .Game_Step_this
    +ldfld 0
    +stack_push_var .Game_Step_var1
    +ldelemRef
    jsr PlatformEnemy_Move
    nop
  ;  +stack_push_var .Game_Step_var1
  ;  +stack_push_int 1
  ;  +add
  ;  +stack_pull_int .Game_Step_var1
    +inc_var .Game_Step_var1
label_Game_Step_23:    +stack_push_var .Game_Step_var1
    +stack_push_var .Game_Step_this
    +ldfld 0
    +ldlen
    nop ;Conv_i4
    +compareLess
    +stack_pull_int .Game_Step_var2
    +stack_push_var .Game_Step_var2
    +branch_true label_Game_Step_5
  ;  +stack_push_int 255
  ;  +stack_pull_int .Game_Step_var0
    +init_var .Game_Step_var0, 255
  ;  +stack_push_int 0
  ;  +stack_pull_int .Game_Step_var3
    +init_var .Game_Step_var3, 0
    jmp label_Game_Step_105
label_Game_Step_48:    nop
    +stack_push_var .Game_Step_this
    +ldfld 2
    +stack_push_var .Game_Step_var3
    +ldelemRef
    +stack_push_var .Game_Step_this
    +ldfld 1
    jsr Player_get_X
    +stack_push_var .Game_Step_this
    +ldfld 1
    jsr Player_get_Y
    jsr Platform_DistanceToPlatform
    +stack_pull_int .Game_Step_var4
    +stack_push_var .Game_Step_var4
    +stack_push_var .Game_Step_var0
    +compareLess
    +stack_pull_int .Game_Step_var5
    +stack_push_var .Game_Step_var5
    +branch_false label_Game_Step_100
    +stack_push_var .Game_Step_var4
    +stack_pull_int .Game_Step_var0
label_Game_Step_100:    nop
  ;  +stack_push_var .Game_Step_var3
  ;  +stack_push_int 1
  ;  +add
  ;  +stack_pull_int .Game_Step_var3
    +inc_var .Game_Step_var3
label_Game_Step_105:    +stack_push_var .Game_Step_var3
    +stack_push_var .Game_Step_this
    +ldfld 2
    +ldlen
    nop ;Conv_i4
    +compareLess
    +stack_pull_int .Game_Step_var6
    +stack_push_var .Game_Step_var6
    +branch_true label_Game_Step_48
    +stack_push_var .Game_Step_this
    +ldfld 1
    +stack_push_var .Game_Step_var0
    jsr Player_Move
    nop
    +stack_return_to_saved_address .Game_Step_ReturnAddress
.Game_Step_this !byte 0,0
.Game_Step_ReturnAddress !byte 0,0
.Game_Step_var0 !byte 0,0
.Game_Step_var1 !byte 0,0
.Game_Step_var2 !byte 0,0
.Game_Step_var3 !byte 0,0
.Game_Step_var4 !byte 0,0
.Game_Step_var5 !byte 0,0
.Game_Step_var6 !byte 0,0


Game_ClearScreen 
+stack_save_return_adress .Game_ClearScreen_ReturnAddress
    nop
  ;  +stack_push_int 0
  ;  +stack_pull_int .Game_ClearScreen_var0
    +init_var .Game_ClearScreen_var0, 0
    jmp label_Game_ClearScreen_42
label_Game_ClearScreen_5:    nop
  ;  +stack_push_int 0
  ;  +stack_pull_int .Game_ClearScreen_var1
    +init_var .Game_ClearScreen_var1, 0
    jmp label_Game_ClearScreen_28
label_Game_ClearScreen_10:    nop
    +stack_push_var .Game_ClearScreen_var0
    +stack_push_var .Game_ClearScreen_var1
    +stack_push_int 32
    +stack_push_int 14
    jsr C64_SetChar
    nop
    nop
  ;  +stack_push_var .Game_ClearScreen_var1
  ;  +stack_push_int 1
  ;  +add
  ;  +stack_pull_int .Game_ClearScreen_var1
    +inc_var .Game_ClearScreen_var1
label_Game_ClearScreen_28:  ;  +stack_push_var .Game_ClearScreen_var1
  ;  +stack_push_int 25
  ;  +compareLess
  ;  +stack_pull_int .Game_ClearScreen_var2
  ;  +stack_push_var .Game_ClearScreen_var2
  ;  +branch_true label_Game_ClearScreen_10
    +branch_if_var_less .Game_ClearScreen_var1, 25, label_Game_ClearScreen_10
    nop
  ;  +stack_push_var .Game_ClearScreen_var0
  ;  +stack_push_int 1
  ;  +add
  ;  +stack_pull_int .Game_ClearScreen_var0
    +inc_var .Game_ClearScreen_var0
label_Game_ClearScreen_42:  ;  +stack_push_var .Game_ClearScreen_var0
  ;  +stack_push_int 40
  ;  +compareLess
  ;  +stack_pull_int .Game_ClearScreen_var3
  ;  +stack_push_var .Game_ClearScreen_var3
  ;  +branch_true label_Game_ClearScreen_5
    +branch_if_var_less .Game_ClearScreen_var0, 40, label_Game_ClearScreen_5
    +stack_return_to_saved_address .Game_ClearScreen_ReturnAddress
.Game_ClearScreen_ReturnAddress !byte 0,0
.Game_ClearScreen_var0 !byte 0,0
.Game_ClearScreen_var1 !byte 0,0
.Game_ClearScreen_var2 !byte 0,0
.Game_ClearScreen_var3 !byte 0,0


PlatformEnemy_set_Platform 
+stack_save_return_adress .PlatformEnemy_set_Platform_ReturnAddress
+stack_pull_int .PlatformEnemy_set_Platform_value
+stack_pull_int .PlatformEnemy_set_Platform_this
    nop
    +stack_push_var .PlatformEnemy_set_Platform_this
    +stack_push_var .PlatformEnemy_set_Platform_value
    +ldfld 3
    +stack_push_int 20
    +add
    jsr PlatformEnemy_set_MinX
    nop
    +stack_push_var .PlatformEnemy_set_Platform_this
    +stack_push_var .PlatformEnemy_set_Platform_value
    +ldfld 4
    +stack_push_int 20
    +sub
    jsr PlatformEnemy_set_MaxX
    nop
    +stack_push_var .PlatformEnemy_set_Platform_this
    +stack_push_var .PlatformEnemy_set_Platform_value
    +ldfld 2
    +stack_push_int 2
    +sub
    +stfld 1
    +stack_return_to_saved_address .PlatformEnemy_set_Platform_ReturnAddress
.PlatformEnemy_set_Platform_this !byte 0,0
.PlatformEnemy_set_Platform_value !byte 0,0
.PlatformEnemy_set_Platform_ReturnAddress !byte 0,0


PlatformEnemy_set_Sprite 
+stack_save_return_adress .PlatformEnemy_set_Sprite_ReturnAddress
+stack_pull_int .PlatformEnemy_set_Sprite_value
+stack_pull_int .PlatformEnemy_set_Sprite_this
    nop
    +stack_push_var .PlatformEnemy_set_Sprite_this
    +stack_push_var .PlatformEnemy_set_Sprite_value
    +stfld 8
    +stack_push_var .PlatformEnemy_set_Sprite_this
    +ldfld 8
    +stack_push_var .PlatformEnemy_set_Sprite_this
    +ldfld 2
    jsr Sprite_set_DataBlock
    nop
    +stack_push_var .PlatformEnemy_set_Sprite_this
    +stack_push_int 1
    +stfld 7
    +stack_push_var .PlatformEnemy_set_Sprite_this
    +stack_push_var .PlatformEnemy_set_Sprite_this
    jsr PlatformEnemy_get_MinX
    jsr PlatformEnemy_set_X
    nop
    +stack_push_var .PlatformEnemy_set_Sprite_this
    +stack_push_var .PlatformEnemy_set_Sprite_this
    jsr PlatformEnemy_get_Y
    jsr PlatformEnemy_set_Y
    nop
    +stack_push_var .PlatformEnemy_set_Sprite_this
    +ldfld 8
    +stack_push_int 1
    jsr Sprite_set_Visible
    nop
    +stack_return_to_saved_address .PlatformEnemy_set_Sprite_ReturnAddress
.PlatformEnemy_set_Sprite_this !byte 0,0
.PlatformEnemy_set_Sprite_value !byte 0,0
.PlatformEnemy_set_Sprite_ReturnAddress !byte 0,0


PlatformEnemy_set_Color 
+stack_save_return_adress .PlatformEnemy_set_Color_ReturnAddress
+stack_pull_int .PlatformEnemy_set_Color_value
+stack_pull_int .PlatformEnemy_set_Color_this
    nop
    +stack_push_var .PlatformEnemy_set_Color_this
    +ldfld 8
    +stack_push_var .PlatformEnemy_set_Color_value
    jsr Sprite_set_Color
    nop
    +stack_return_to_saved_address .PlatformEnemy_set_Color_ReturnAddress
.PlatformEnemy_set_Color_this !byte 0,0
.PlatformEnemy_set_Color_value !byte 0,0
.PlatformEnemy_set_Color_ReturnAddress !byte 0,0


PlatformEnemy_set_MaxX 
+stack_save_return_adress .PlatformEnemy_set_MaxX_ReturnAddress
+stack_pull_int .PlatformEnemy_set_MaxX_value
+stack_pull_int .PlatformEnemy_set_MaxX_this
    +stack_push_var .PlatformEnemy_set_MaxX_this
    +stack_push_var .PlatformEnemy_set_MaxX_value
    +stfld 6
    +stack_return_to_saved_address .PlatformEnemy_set_MaxX_ReturnAddress
.PlatformEnemy_set_MaxX_this !byte 0,0
.PlatformEnemy_set_MaxX_value !byte 0,0
.PlatformEnemy_set_MaxX_ReturnAddress !byte 0,0


PlatformEnemy_set_MinX 
+stack_save_return_adress .PlatformEnemy_set_MinX_ReturnAddress
+stack_pull_int .PlatformEnemy_set_MinX_value
+stack_pull_int .PlatformEnemy_set_MinX_this
    +stack_push_var .PlatformEnemy_set_MinX_this
    +stack_push_var .PlatformEnemy_set_MinX_value
    +stfld 5
    +stack_return_to_saved_address .PlatformEnemy_set_MinX_ReturnAddress
.PlatformEnemy_set_MinX_this !byte 0,0
.PlatformEnemy_set_MinX_value !byte 0,0
.PlatformEnemy_set_MinX_ReturnAddress !byte 0,0


PlatformEnemy_set_Y 
+stack_save_return_adress .PlatformEnemy_set_Y_ReturnAddress
+stack_pull_int .PlatformEnemy_set_Y_value
+stack_pull_int .PlatformEnemy_set_Y_this
    nop
    +stack_push_var .PlatformEnemy_set_Y_this
    +stack_push_var .PlatformEnemy_set_Y_value
    +stfld 1
    +stack_push_var .PlatformEnemy_set_Y_this
    +ldfld 8
    +stack_push_var .PlatformEnemy_set_Y_value
    jsr Sprite_set_Y
    nop
    +stack_return_to_saved_address .PlatformEnemy_set_Y_ReturnAddress
.PlatformEnemy_set_Y_this !byte 0,0
.PlatformEnemy_set_Y_value !byte 0,0
.PlatformEnemy_set_Y_ReturnAddress !byte 0,0


PlatformEnemy_Move 
+stack_save_return_adress .PlatformEnemy_Move_ReturnAddress
+stack_pull_int .PlatformEnemy_Move_this
    nop
    +stack_push_var .PlatformEnemy_Move_this
    +ldfld 7
    +stack_pull_int .PlatformEnemy_Move_var0
    +stack_push_var .PlatformEnemy_Move_var0
    +branch_false label_PlatformEnemy_Move_60
    nop
    +stack_push_var .PlatformEnemy_Move_this
    +stack_push_var .PlatformEnemy_Move_this
    jsr PlatformEnemy_get_X
    +stack_push_var .PlatformEnemy_Move_this
    +ldfld 3
    +add
    jsr PlatformEnemy_set_X
    nop
    +stack_push_var .PlatformEnemy_Move_this
    jsr PlatformEnemy_get_X
    +stack_push_var .PlatformEnemy_Move_this
    jsr PlatformEnemy_get_MaxX
    +compareGreater
    +stack_pull_int .PlatformEnemy_Move_var1
    +stack_push_var .PlatformEnemy_Move_var1
    +branch_false label_PlatformEnemy_Move_57
    +stack_push_var .PlatformEnemy_Move_this
    +stack_push_int 0
    +stfld 7
label_PlatformEnemy_Move_57:    nop
    jmp label_PlatformEnemy_Move_107
label_PlatformEnemy_Move_60:    nop
    +stack_push_var .PlatformEnemy_Move_this
    +stack_push_var .PlatformEnemy_Move_this
    jsr PlatformEnemy_get_X
    +stack_push_var .PlatformEnemy_Move_this
    +ldfld 4
    +sub
    jsr PlatformEnemy_set_X
    nop
    +stack_push_var .PlatformEnemy_Move_this
    jsr PlatformEnemy_get_X
    +stack_push_var .PlatformEnemy_Move_this
    jsr PlatformEnemy_get_MinX
    +compareLess
    +stack_pull_int .PlatformEnemy_Move_var2
    +stack_push_var .PlatformEnemy_Move_var2
    +branch_false label_PlatformEnemy_Move_106
    +stack_push_var .PlatformEnemy_Move_this
    +stack_push_int 1
    +stfld 7
label_PlatformEnemy_Move_106:    nop
label_PlatformEnemy_Move_107:    +stack_return_to_saved_address .PlatformEnemy_Move_ReturnAddress
.PlatformEnemy_Move_this !byte 0,0
.PlatformEnemy_Move_ReturnAddress !byte 0,0
.PlatformEnemy_Move_var0 !byte 0,0
.PlatformEnemy_Move_var1 !byte 0,0
.PlatformEnemy_Move_var2 !byte 0,0


PlatformEnemy_get_X 
+stack_save_return_adress .PlatformEnemy_get_X_ReturnAddress
+stack_pull_int .PlatformEnemy_get_X_this
    nop
    +stack_push_var .PlatformEnemy_get_X_this
    +ldfld 0
    +stack_pull_int .PlatformEnemy_get_X_var0
    jmp label_PlatformEnemy_get_X_10
label_PlatformEnemy_get_X_10:    +stack_push_var .PlatformEnemy_get_X_var0
    +stack_return_to_saved_address .PlatformEnemy_get_X_ReturnAddress
.PlatformEnemy_get_X_this !byte 0,0
.PlatformEnemy_get_X_ReturnAddress !byte 0,0
.PlatformEnemy_get_X_var0 !byte 0,0


PlatformEnemy_set_X 
+stack_save_return_adress .PlatformEnemy_set_X_ReturnAddress
+stack_pull_int .PlatformEnemy_set_X_value
+stack_pull_int .PlatformEnemy_set_X_this
    nop
    +stack_push_var .PlatformEnemy_set_X_this
    +stack_push_var .PlatformEnemy_set_X_value
    +stfld 0
    +stack_push_var .PlatformEnemy_set_X_this
    +ldfld 8
    +stack_push_var .PlatformEnemy_set_X_value
    jsr Sprite_set_X
    nop
    +stack_return_to_saved_address .PlatformEnemy_set_X_ReturnAddress
.PlatformEnemy_set_X_this !byte 0,0
.PlatformEnemy_set_X_value !byte 0,0
.PlatformEnemy_set_X_ReturnAddress !byte 0,0


PlatformEnemy_get_Y 
+stack_save_return_adress .PlatformEnemy_get_Y_ReturnAddress
+stack_pull_int .PlatformEnemy_get_Y_this
    nop
    +stack_push_var .PlatformEnemy_get_Y_this
    +ldfld 1
    +stack_pull_int .PlatformEnemy_get_Y_var0
    jmp label_PlatformEnemy_get_Y_10
label_PlatformEnemy_get_Y_10:    +stack_push_var .PlatformEnemy_get_Y_var0
    +stack_return_to_saved_address .PlatformEnemy_get_Y_ReturnAddress
.PlatformEnemy_get_Y_this !byte 0,0
.PlatformEnemy_get_Y_ReturnAddress !byte 0,0
.PlatformEnemy_get_Y_var0 !byte 0,0


PlatformEnemy_get_MinX 
+stack_save_return_adress .PlatformEnemy_get_MinX_ReturnAddress
+stack_pull_int .PlatformEnemy_get_MinX_this
    +stack_push_var .PlatformEnemy_get_MinX_this
    +ldfld 5
    +stack_return_to_saved_address .PlatformEnemy_get_MinX_ReturnAddress
.PlatformEnemy_get_MinX_this !byte 0,0
.PlatformEnemy_get_MinX_ReturnAddress !byte 0,0


PlatformEnemy_get_MaxX 
+stack_save_return_adress .PlatformEnemy_get_MaxX_ReturnAddress
+stack_pull_int .PlatformEnemy_get_MaxX_this
    +stack_push_var .PlatformEnemy_get_MaxX_this
    +ldfld 6
    +stack_return_to_saved_address .PlatformEnemy_get_MaxX_ReturnAddress
.PlatformEnemy_get_MaxX_this !byte 0,0
.PlatformEnemy_get_MaxX_ReturnAddress !byte 0,0
.Platform_field_67108893 !byte 0,0
.Platform_field_67108894 !byte 0,0
.Platform_field_67108895 !byte 0,0
.Platform_field_67108896 !byte 0,0
.Platform_field_67108897 !byte 0,0


Platform_Draw 
+stack_save_return_adress .Platform_Draw_ReturnAddress
+stack_pull_int .Platform_Draw_this
    nop
    +stack_push_var .Platform_Draw_this
    +ldfld 0
    +stack_push_int 1
    +add
    +stack_pull_int .Platform_Draw_var0
    +stack_push_var .Platform_Draw_this
    +ldfld 0
    +stack_push_var .Platform_Draw_this
    +ldfld 5
    +add
    +stack_pull_int .Platform_Draw_var1
    +stack_push_var .Platform_Draw_this
    +ldfld 1
    +stack_pull_int .Platform_Draw_var2
    +stack_push_var .Platform_Draw_this
    +ldfld 1
    +stack_push_int 1
    +add
    +stack_pull_int .Platform_Draw_var3
    +stack_push_var .Platform_Draw_this
    +stack_push_var .Platform_Draw_var2
    +stack_push_int 3
    +shl
    +stack_push_int 33
    +add
    +stfld 2
    +stack_push_var .Platform_Draw_this
    +stack_push_var .Platform_Draw_var0
    +stack_push_int 3
    +shl
    +stack_push_int 3
    +add
    +stfld 3
    +stack_push_var .Platform_Draw_this
    +stack_push_var .Platform_Draw_var1
    +stack_push_int 3
    +shl
    +stack_push_int 20
    +add
    +stfld 4
    +stack_push_var .Platform_Draw_var0
    +stack_pull_int .Platform_Draw_var4
    jmp label_Platform_Draw_122
label_Platform_Draw_80:    nop
    +stack_push_var .Platform_Draw_var4
    +stack_push_var .Platform_Draw_var2
    +stack_push_int 67
    +stack_push_var .Platform_Draw_this
    +ldfld 6
    jsr C64_SetChar
    nop
    +stack_push_var .Platform_Draw_var4
    +stack_push_var .Platform_Draw_var3
    +stack_push_int 67
    +stack_push_var .Platform_Draw_this
    +ldfld 6
    jsr C64_SetChar
    nop
    nop
    +stack_push_var .Platform_Draw_var4
    +stack_push_int 1
    +add
    +stack_pull_int .Platform_Draw_var4
label_Platform_Draw_122:    +stack_push_var .Platform_Draw_var4
    +stack_push_var .Platform_Draw_var1
    +compareLess
    +stack_pull_int .Platform_Draw_var5
    +stack_push_var .Platform_Draw_var5
    +branch_true label_Platform_Draw_80
    +stack_push_var .Platform_Draw_this
    +ldfld 0
    +stack_push_var .Platform_Draw_var2
    +stack_push_int 85
    +stack_push_var .Platform_Draw_this
    +ldfld 6
    jsr C64_SetChar
    nop
    +stack_push_var .Platform_Draw_this
    +ldfld 0
    +stack_push_var .Platform_Draw_var3
    +stack_push_int 74
    +stack_push_var .Platform_Draw_this
    +ldfld 6
    jsr C64_SetChar
    nop
    +stack_push_var .Platform_Draw_var1
    +stack_push_var .Platform_Draw_var2
    +stack_push_int 73
    +stack_push_var .Platform_Draw_this
    +ldfld 6
    jsr C64_SetChar
    nop
    +stack_push_var .Platform_Draw_var1
    +stack_push_var .Platform_Draw_var3
    +stack_push_int 75
    +stack_push_var .Platform_Draw_this
    +ldfld 6
    jsr C64_SetChar
    nop
    +stack_return_to_saved_address .Platform_Draw_ReturnAddress
.Platform_Draw_this !byte 0,0
.Platform_Draw_ReturnAddress !byte 0,0
.Platform_Draw_var0 !byte 0,0
.Platform_Draw_var1 !byte 0,0
.Platform_Draw_var2 !byte 0,0
.Platform_Draw_var3 !byte 0,0
.Platform_Draw_var4 !byte 0,0
.Platform_Draw_var5 !byte 0,0


Platform_DistanceToPlatform 
+stack_save_return_adress .Platform_DistanceToPlatform_ReturnAddress
+stack_pull_int .Platform_DistanceToPlatform_y
+stack_pull_int .Platform_DistanceToPlatform_x
+stack_pull_int .Platform_DistanceToPlatform_this
    nop
    +stack_push_var .Platform_DistanceToPlatform_x
    +stack_push_var .Platform_DistanceToPlatform_this
    +ldfld 3
    +compareLess
    +stack_pull_int .Platform_DistanceToPlatform_var1
    +stack_push_var .Platform_DistanceToPlatform_var1
    +branch_false label_Platform_DistanceToPlatform_22
  ;  +stack_push_int 255
  ;  +stack_pull_int .Platform_DistanceToPlatform_var2
    +init_var .Platform_DistanceToPlatform_var2, 255
    jmp label_Platform_DistanceToPlatform_79
label_Platform_DistanceToPlatform_22:    +stack_push_var .Platform_DistanceToPlatform_x
    +stack_push_var .Platform_DistanceToPlatform_this
    +ldfld 4
    +compareGreater
    +stack_pull_int .Platform_DistanceToPlatform_var3
    +stack_push_var .Platform_DistanceToPlatform_var3
    +branch_false label_Platform_DistanceToPlatform_43
  ;  +stack_push_int 255
  ;  +stack_pull_int .Platform_DistanceToPlatform_var2
    +init_var .Platform_DistanceToPlatform_var2, 255
    jmp label_Platform_DistanceToPlatform_79
label_Platform_DistanceToPlatform_43:    +stack_push_var .Platform_DistanceToPlatform_y
    +stack_push_var .Platform_DistanceToPlatform_this
    +ldfld 2
    +compareGreater
    +stack_pull_int .Platform_DistanceToPlatform_var4
    +stack_push_var .Platform_DistanceToPlatform_var4
    +branch_false label_Platform_DistanceToPlatform_66
  ;  +stack_push_int 255
  ;  +stack_pull_int .Platform_DistanceToPlatform_var2
    +init_var .Platform_DistanceToPlatform_var2, 255
    jmp label_Platform_DistanceToPlatform_79
label_Platform_DistanceToPlatform_66:    +stack_push_var .Platform_DistanceToPlatform_this
    +ldfld 2
    +stack_push_var .Platform_DistanceToPlatform_y
    +sub
    +stack_pull_int .Platform_DistanceToPlatform_var0
    +stack_push_var .Platform_DistanceToPlatform_var0
    +stack_pull_int .Platform_DistanceToPlatform_var2
    jmp label_Platform_DistanceToPlatform_79
label_Platform_DistanceToPlatform_79:    +stack_push_var .Platform_DistanceToPlatform_var2
    +stack_return_to_saved_address .Platform_DistanceToPlatform_ReturnAddress
.Platform_DistanceToPlatform_this !byte 0,0
.Platform_DistanceToPlatform_x !byte 0,0
.Platform_DistanceToPlatform_y !byte 0,0
.Platform_DistanceToPlatform_ReturnAddress !byte 0,0
.Platform_DistanceToPlatform_var0 !byte 0,0
.Platform_DistanceToPlatform_var1 !byte 0,0
.Platform_DistanceToPlatform_var2 !byte 0,0
.Platform_DistanceToPlatform_var3 !byte 0,0
.Platform_DistanceToPlatform_var4 !byte 0,0


Player_set_Sprite 
+stack_save_return_adress .Player_set_Sprite_ReturnAddress
+stack_pull_int .Player_set_Sprite_value
+stack_pull_int .Player_set_Sprite_this
    nop
    jsr C64_get_Sprites
    +stack_push_int 10
    jsr SpriteCollection_set_CommonColor1
    nop
    jsr C64_get_Sprites
    +stack_push_int 6
    jsr SpriteCollection_set_CommonColor2
    nop
    +stack_push_var .Player_set_Sprite_this
    +stack_push_var .Player_set_Sprite_value
    +stfld 5
    +stack_push_var .Player_set_Sprite_this
    +ldfld 5
    +stack_push_int 3
    jsr Sprite_set_DataBlock
    nop
    +stack_push_var .Player_set_Sprite_this
    +stack_push_int 4
    +stfld 2
    +stack_push_var .Player_set_Sprite_this
    +ldfld 5
    +stack_push_int 1
    jsr Sprite_set_MultiColor
    nop
    +stack_push_var .Player_set_Sprite_this
    +ldfld 5
    +stack_push_int 1
    jsr Sprite_set_Visible
    nop
    +stack_push_var .Player_set_Sprite_this
    +ldfld 5
    +stack_push_int 9
    jsr Sprite_set_Color
    nop
    +stack_push_var .Player_set_Sprite_this
    +stack_push_int 0
    +stfld 3
    +stack_push_var .Player_set_Sprite_this
    +stack_push_int 200
    jsr Player_set_Y
    nop
    +stack_push_var .Player_set_Sprite_this
    +stack_push_int 20
    jsr Player_set_X
    nop
    +stack_return_to_saved_address .Player_set_Sprite_ReturnAddress
.Player_set_Sprite_this !byte 0,0
.Player_set_Sprite_value !byte 0,0
.Player_set_Sprite_ReturnAddress !byte 0,0


Player_get_X 
+stack_save_return_adress .Player_get_X_ReturnAddress
+stack_pull_int .Player_get_X_this
    nop
    +stack_push_var .Player_get_X_this
    +ldfld 0
    +stack_pull_int .Player_get_X_var0
    jmp label_Player_get_X_10
label_Player_get_X_10:    +stack_push_var .Player_get_X_var0
    +stack_return_to_saved_address .Player_get_X_ReturnAddress
.Player_get_X_this !byte 0,0
.Player_get_X_ReturnAddress !byte 0,0
.Player_get_X_var0 !byte 0,0


Player_get_Y 
+stack_save_return_adress .Player_get_Y_ReturnAddress
+stack_pull_int .Player_get_Y_this
    nop
    +stack_push_var .Player_get_Y_this
    +ldfld 1
    +stack_pull_int .Player_get_Y_var0
    jmp label_Player_get_Y_10
label_Player_get_Y_10:    +stack_push_var .Player_get_Y_var0
    +stack_return_to_saved_address .Player_get_Y_ReturnAddress
.Player_get_Y_this !byte 0,0
.Player_get_Y_ReturnAddress !byte 0,0
.Player_get_Y_var0 !byte 0,0


Player_Move 
+stack_save_return_adress .Player_Move_ReturnAddress
+stack_pull_int .Player_Move_distanceToPlatform
+stack_pull_int .Player_Move_this
    nop
    +stack_push_var .Player_Move_this
    +ldfld 5
    jsr Sprite_get_IsInCollision
    +branch_true label_Player_Move_29
    +stack_push_var .Player_Move_this
    jsr Player_get_Y
    +stack_push_int 240
    +compareGreater
    jmp label_Player_Move_30
label_Player_Move_29:    +stack_push_int 1
label_Player_Move_30:    +stack_pull_int .Player_Move_var1
    +stack_push_var .Player_Move_var1
    +branch_false label_Player_Move_47
    nop
    +stack_push_var .Player_Move_this
    jsr Player_Die
    nop
    jmp label_Player_Move_572
label_Player_Move_47:  ;  +stack_push_int 0
  ;  +stack_pull_int .Player_Move_var0
    +init_var .Player_Move_var0, 0
    +stack_push_var .Player_Move_this
    +ldfld 3
    +branch_true label_Player_Move_63
    +stack_push_var .Player_Move_distanceToPlatform
    +stack_push_int 4
    +compareGreater
    jmp label_Player_Move_64
label_Player_Move_63:    +stack_push_int 0
label_Player_Move_64:    +stack_pull_int .Player_Move_var2
    +stack_push_var .Player_Move_var2
    +branch_false label_Player_Move_70
  ;  +stack_push_int 1
  ;  +stack_pull_int .Player_Move_var0
    +init_var .Player_Move_var0, 1
label_Player_Move_70:    +stack_push_var .Player_Move_this
    +ldfld 3
    +branch_true label_Player_Move_90
    +stack_push_var .Player_Move_var0
    +branch_true label_Player_Move_90
    +stack_push_int 22
    jsr C64_IsKeyPressed
    jmp label_Player_Move_91
label_Player_Move_90:    +stack_push_int 0
label_Player_Move_91:    +stack_pull_int .Player_Move_var3
    +stack_push_var .Player_Move_var3
    +branch_false label_Player_Move_111
    nop
    +stack_push_var .Player_Move_this
    +stack_push_int 1
    +stfld 3
    +stack_push_var .Player_Move_this
    +stack_push_int 0
    +stfld 4
    nop
label_Player_Move_111:    +stack_push_int 0
    jsr C64_IsKeyPressed
    +stack_pull_int .Player_Move_var4
    +stack_push_var .Player_Move_var4
    +branch_false label_Player_Move_299
    nop
    +stack_push_var .Player_Move_this
    jsr Player_get_X
    +stack_pull_int .Player_Move_var5
    +stack_push_var .Player_Move_this
    +stack_push_var .Player_Move_var5
    +stack_push_int 1
    +sub
    jsr Player_set_X
    nop
    +stack_push_var .Player_Move_this
    +ldfld 2
    +stack_push_int 5
    +compareLess
    +stack_pull_int .Player_Move_var6
    +stack_push_var .Player_Move_var6
    +branch_false label_Player_Move_168
    +stack_push_var .Player_Move_this
    +stack_push_int 5
    +stfld 2
label_Player_Move_168:    +stack_push_var .Player_Move_this
    +stack_push_var .Player_Move_this
    +ldfld 2
    +stack_push_int 1
    +add
    +stfld 2
    +stack_push_var .Player_Move_this
    +ldfld 2
    +stack_push_int 9
    +compareEqual
    +stack_pull_int .Player_Move_var7
    +stack_push_var .Player_Move_var7
    +branch_false label_Player_Move_205
    +stack_push_var .Player_Move_this
    +stack_push_int 6
    +stfld 2
label_Player_Move_205:    +stack_push_var .Player_Move_this
    +ldfld 3
    +stack_pull_int .Player_Move_var8
    +stack_push_var .Player_Move_var8
    +branch_false label_Player_Move_245
    nop
    +stack_push_var .Player_Move_this
    jsr Player_get_X
    +stack_pull_int .Player_Move_var5
    +stack_push_var .Player_Move_this
    +stack_push_var .Player_Move_var5
    +stack_push_int 1
    +sub
    jsr Player_set_X
    nop
    +stack_push_var .Player_Move_this
    +stack_push_int 7
    +stfld 2
    nop
label_Player_Move_245:    +stack_push_var .Player_Move_var0
    +stack_pull_int .Player_Move_var9
    +stack_push_var .Player_Move_var9
    +branch_false label_Player_Move_280
    nop
    +stack_push_var .Player_Move_this
    jsr Player_get_X
    +stack_pull_int .Player_Move_var5
    +stack_push_var .Player_Move_this
    +stack_push_var .Player_Move_var5
    +stack_push_int 1
    +sub
    jsr Player_set_X
    nop
    +stack_push_var .Player_Move_this
    +stack_push_int 7
    +stfld 2
    nop
label_Player_Move_280:    +stack_push_var .Player_Move_this
    +ldfld 5
    +stack_push_var .Player_Move_this
    +ldfld 2
    jsr Sprite_set_DataBlock
    nop
    nop
label_Player_Move_299:    +stack_push_int 3
    jsr C64_IsKeyPressed
    +stack_pull_int .Player_Move_var10
    +stack_push_var .Player_Move_var10
    +branch_false label_Player_Move_464
    nop
    +stack_push_var .Player_Move_this
    jsr Player_get_X
    +stack_pull_int .Player_Move_var5
    +stack_push_var .Player_Move_this
    +stack_push_var .Player_Move_var5
    +stack_push_int 1
    +add
    jsr Player_set_X
    nop
    +stack_push_var .Player_Move_this
    +stack_push_var .Player_Move_this
    +ldfld 2
    +stack_push_int 1
    +add
    +stfld 2
    +stack_push_var .Player_Move_this
    +ldfld 2
    +stack_push_int 4
    +compareGreater
    +stack_pull_int .Player_Move_var11
    +stack_push_var .Player_Move_var11
    +branch_false label_Player_Move_370
    +stack_push_var .Player_Move_this
    +stack_push_int 1
    +stfld 2
label_Player_Move_370:    +stack_push_var .Player_Move_this
    +ldfld 3
    +stack_pull_int .Player_Move_var12
    +stack_push_var .Player_Move_var12
    +branch_false label_Player_Move_410
    nop
    +stack_push_var .Player_Move_this
    jsr Player_get_X
    +stack_pull_int .Player_Move_var5
    +stack_push_var .Player_Move_this
    +stack_push_var .Player_Move_var5
    +stack_push_int 1
    +add
    jsr Player_set_X
    nop
    +stack_push_var .Player_Move_this
    +stack_push_int 2
    +stfld 2
    nop
label_Player_Move_410:    +stack_push_var .Player_Move_var0
    +stack_pull_int .Player_Move_var13
    +stack_push_var .Player_Move_var13
    +branch_false label_Player_Move_445
    nop
    +stack_push_var .Player_Move_this
    jsr Player_get_X
    +stack_pull_int .Player_Move_var5
    +stack_push_var .Player_Move_this
    +stack_push_var .Player_Move_var5
    +stack_push_int 1
    +add
    jsr Player_set_X
    nop
    +stack_push_var .Player_Move_this
    +stack_push_int 2
    +stfld 2
    nop
label_Player_Move_445:    +stack_push_var .Player_Move_this
    +ldfld 5
    +stack_push_var .Player_Move_this
    +ldfld 2
    jsr Sprite_set_DataBlock
    nop
    nop
label_Player_Move_464:    +stack_push_var .Player_Move_this
    +ldfld 3
    +stack_pull_int .Player_Move_var14
    +stack_push_var .Player_Move_var14
    +branch_false label_Player_Move_548
    nop
    +stack_push_var .Player_Move_this
    +stack_push_var .Player_Move_this
    +ldfld 4
    +stack_push_int 1
    +add
    +stfld 4
    +stack_push_var .Player_Move_this
    +stack_push_var .Player_Move_this
    jsr Player_get_Y
    nop ;Conv_u8
    +stack_push_var .Player_Move_this
    +ldfld 4
    +stack_push_int 10
    +branch_less label_Player_Move_512
    +stack_push_int 1
    jmp label_Player_Move_513
label_Player_Move_512:    +stack_push_int 3
label_Player_Move_513:    nop ;Conv_i8
    +sub
    nop ;Conv_u4
    jsr Player_set_Y
    nop
    +stack_push_var .Player_Move_this
    +ldfld 4
    +stack_push_int 16
    +compareEqual
    +stack_pull_int .Player_Move_var15
    +stack_push_var .Player_Move_var15
    +branch_false label_Player_Move_547
    nop
    +stack_push_var .Player_Move_this
    +stack_push_int 0
    +stfld 3
    nop
label_Player_Move_547:    nop
label_Player_Move_548:    +stack_push_var .Player_Move_var0
    +stack_pull_int .Player_Move_var16
    +stack_push_var .Player_Move_var16
    +branch_false label_Player_Move_572
    nop
    +stack_push_var .Player_Move_this
    +stack_push_var .Player_Move_this
    jsr Player_get_Y
    +stack_push_int 3
    +add
    jsr Player_set_Y
    nop
    nop
label_Player_Move_572:    +stack_return_to_saved_address .Player_Move_ReturnAddress
.Player_Move_this !byte 0,0
.Player_Move_distanceToPlatform !byte 0,0
.Player_Move_ReturnAddress !byte 0,0
.Player_Move_var0 !byte 0,0
.Player_Move_var1 !byte 0,0
.Player_Move_var2 !byte 0,0
.Player_Move_var3 !byte 0,0
.Player_Move_var4 !byte 0,0
.Player_Move_var5 !byte 0,0
.Player_Move_var6 !byte 0,0
.Player_Move_var7 !byte 0,0
.Player_Move_var8 !byte 0,0
.Player_Move_var9 !byte 0,0
.Player_Move_var10 !byte 0,0
.Player_Move_var11 !byte 0,0
.Player_Move_var12 !byte 0,0
.Player_Move_var13 !byte 0,0
.Player_Move_var14 !byte 0,0
.Player_Move_var15 !byte 0,0
.Player_Move_var16 !byte 0,0


Player_set_X 
+stack_save_return_adress .Player_set_X_ReturnAddress
+stack_pull_int .Player_set_X_value
+stack_pull_int .Player_set_X_this
    nop
    +stack_push_var .Player_set_X_this
    +stack_push_var .Player_set_X_value
    +stfld 0
    +stack_push_var .Player_set_X_this
    +ldfld 5
    +stack_push_var .Player_set_X_value
    jsr Sprite_set_X
    nop
    +stack_return_to_saved_address .Player_set_X_ReturnAddress
.Player_set_X_this !byte 0,0
.Player_set_X_value !byte 0,0
.Player_set_X_ReturnAddress !byte 0,0


Player_set_Y 
+stack_save_return_adress .Player_set_Y_ReturnAddress
+stack_pull_int .Player_set_Y_value
+stack_pull_int .Player_set_Y_this
    nop
    +stack_push_var .Player_set_Y_this
    +stack_push_var .Player_set_Y_value
    +stfld 1
    +stack_push_var .Player_set_Y_this
    +ldfld 5
    +stack_push_var .Player_set_Y_value
    jsr Sprite_set_Y
    nop
    +stack_return_to_saved_address .Player_set_Y_ReturnAddress
.Player_set_Y_this !byte 0,0
.Player_set_Y_value !byte 0,0
.Player_set_Y_ReturnAddress !byte 0,0


Player_Die 
+stack_save_return_adress .Player_Die_ReturnAddress
+stack_pull_int .Player_Die_this
    nop
    +stack_push_int 2
    jsr C64_SetBorderColor
    nop
  ;  +stack_push_int 0
  ;  +stack_pull_int .Player_Die_var0
    +init_var .Player_Die_var0, 0
    jmp label_Player_Die_34
label_Player_Die_12:  ;  +stack_push_int 0
  ;  +stack_pull_int .Player_Die_var1
    +init_var .Player_Die_var1, 0
    jmp label_Player_Die_21
label_Player_Die_16:    nop
  ;  +stack_push_var .Player_Die_var1
  ;  +stack_push_int 1
  ;  +add
  ;  +stack_pull_int .Player_Die_var1
    +inc_var .Player_Die_var1
label_Player_Die_21:  ;  +stack_push_var .Player_Die_var1
  ;  +stack_push_int 100
  ;  +compareLess
  ;  +stack_pull_int .Player_Die_var2
  ;  +stack_push_var .Player_Die_var2
  ;  +branch_true label_Player_Die_16
    +branch_if_var_less .Player_Die_var1, 100, label_Player_Die_16
  ;  +stack_push_var .Player_Die_var0
  ;  +stack_push_int 1
  ;  +add
  ;  +stack_pull_int .Player_Die_var0
    +inc_var .Player_Die_var0
label_Player_Die_34:  ;  +stack_push_var .Player_Die_var0
  ;  +stack_push_int 100
  ;  +compareLess
  ;  +stack_pull_int .Player_Die_var3
  ;  +stack_push_var .Player_Die_var3
  ;  +branch_true label_Player_Die_12
    +branch_if_var_less .Player_Die_var0, 100, label_Player_Die_12
    +stack_push_int 0
    jsr C64_SetBorderColor
    nop
    +stack_push_var .Player_Die_this
    +stack_push_int 200
    jsr Player_set_Y
    nop
    +stack_push_var .Player_Die_this
    +stack_push_int 20
    jsr Player_set_X
    nop
    +stack_return_to_saved_address .Player_Die_ReturnAddress
.Player_Die_this !byte 0,0
.Player_Die_ReturnAddress !byte 0,0
.Player_Die_var0 !byte 0,0
.Player_Die_var1 !byte 0,0
.Player_Die_var2 !byte 0,0
.Player_Die_var3 !byte 0,0
.Program_field_67108904 !byte 0,0


Program_OnInterrupt 
+stack_save_return_adress .Program_OnInterrupt_ReturnAddress
+stack_pull_int .Program_OnInterrupt_args
+stack_pull_int .Program_OnInterrupt_sender
    nop
    +stack_push_var .Program_field_67108904
    +stack_push_int 1
    +add
    +stack_pull_int .Program_field_67108904
    +stack_push_var .Program_field_67108904
    jsr C64_SetBorderColor
    nop
    +stack_return_to_saved_address .Program_OnInterrupt_ReturnAddress
.Program_OnInterrupt_sender !byte 0,0
.Program_OnInterrupt_args !byte 0,0
.Program_OnInterrupt_ReturnAddress !byte 0,0


Program_Main 
+stack_save_return_adress .Program_Main_ReturnAddress
    nop
    +newObj 3
    +stack_pull_int .Program_Main_var0
    +stack_push_var .Program_Main_var0
    jsr Game_Init
    nop
    +stack_push_var .Program_Main_var0
    jsr Game_Run
    nop
    +stack_return_to_saved_address .Program_Main_ReturnAddress
.Program_Main_ReturnAddress !byte 0,0
.Program_Main_var0 !byte 0,0
