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
    +newObj 3
    +stack_pull_int .Ball_RunBalls_var3
    +stack_push_var .Ball_RunBalls_var3
    jsr C64_get_Sprites
    jsr SpriteCollection_get_Sprite3
    jsr Player_set_Sprite
    nop
    +stack_push_var .Ball_RunBalls_var3
    +stack_push_int 10
    jsr Player_set_PlayerColor
    nop
    jmp label_Ball_RunBalls_260
label_Ball_RunBalls_203:    nop
    +stack_push_var .Ball_RunBalls_var0
    jsr Ball_Move
    nop
    +stack_push_var .Ball_RunBalls_var1
    jsr Ball_Move
    nop
    +stack_push_var .Ball_RunBalls_var2
    jsr Ball_Move
    nop
    +stack_push_var .Ball_RunBalls_var3
    jsr Player_Move
    nop
    +stack_push_int 0
    +stack_pull_int .Ball_RunBalls_var4
    jmp label_Ball_RunBalls_244
label_Ball_RunBalls_237:    nop
    +stack_push_var .Ball_RunBalls_var4
    +stack_push_int 1
    +add
    +stack_pull_int .Ball_RunBalls_var4
label_Ball_RunBalls_244:    +stack_push_var .Ball_RunBalls_var4
    +stack_push_int 255
    +compareLess
    +stack_pull_int .Ball_RunBalls_var5
    +stack_push_var .Ball_RunBalls_var5
    +branch_true label_Ball_RunBalls_237
    nop
label_Ball_RunBalls_260:    jmp label_Ball_RunBalls_203
.Ball_RunBalls_ReturnAddress !byte 0,0
.Ball_RunBalls_var0 !byte 0,0
.Ball_RunBalls_var1 !byte 0,0
.Ball_RunBalls_var2 !byte 0,0
.Ball_RunBalls_var3 !byte 0,0
.Ball_RunBalls_var4 !byte 0,0
.Ball_RunBalls_var5 !byte 0,0


Game_Init 
+stack_save_return_adress .Game_Init_ReturnAddress
+stack_pull_int .Game_Init_this
    nop
    +stack_push_int 0
    jsr C64_SetBorderColor
    nop
    +stack_push_int 0
    jsr C64_SetBackgroundColor
    nop
    +stack_push_var .Game_Init_this
    +newObj 7
    +stack_duplicate
    +stack_push_int 2
    jsr PlatformEnemy_set_Color
    nop
    +stack_duplicate
    +stack_push_int 100
    jsr PlatformEnemy_set_MinX
    nop
    +stack_duplicate
    +stack_push_int 150
    jsr PlatformEnemy_set_MaxX
    nop
    +stack_duplicate
    +stack_push_int 100
    jsr PlatformEnemy_set_Y
    nop
    +stack_duplicate
    jsr C64_get_Sprites
    jsr SpriteCollection_get_Sprite3
    jsr PlatformEnemy_set_Sprite
    nop
    +stfld 0
    +stack_return_to_saved_address .Game_Init_ReturnAddress
.Game_Init_this !byte 0,0
.Game_Init_ReturnAddress !byte 0,0


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
  ;  +stack_push_int 10
  ;  +compareLess
  ;  +stack_pull_int .Game_Run_var2
  ;  +stack_push_var .Game_Run_var2
  ;  +branch_true label_Game_Run_19
    +branch_if_var_less .Game_Run_var1, 10, label_Game_Run_19
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
    +stack_push_var .Game_Step_this
    +ldfld 0
    jsr PlatformEnemy_Move
    nop
    +stack_return_to_saved_address .Game_Step_ReturnAddress
.Game_Step_this !byte 0,0
.Game_Step_ReturnAddress !byte 0,0


PlatformEnemy_set_Color 
+stack_save_return_adress .PlatformEnemy_set_Color_ReturnAddress
+stack_pull_int .PlatformEnemy_set_Color_value
+stack_pull_int .PlatformEnemy_set_Color_this
    nop
    +stack_push_var .PlatformEnemy_set_Color_this
    +ldfld 6
    +stack_push_var .PlatformEnemy_set_Color_value
    jsr Sprite_set_Color
    nop
    +stack_return_to_saved_address .PlatformEnemy_set_Color_ReturnAddress
.PlatformEnemy_set_Color_this !byte 0,0
.PlatformEnemy_set_Color_value !byte 0,0
.PlatformEnemy_set_Color_ReturnAddress !byte 0,0


PlatformEnemy_set_MinX 
+stack_save_return_adress .PlatformEnemy_set_MinX_ReturnAddress
+stack_pull_int .PlatformEnemy_set_MinX_value
+stack_pull_int .PlatformEnemy_set_MinX_this
    +stack_push_var .PlatformEnemy_set_MinX_this
    +stack_push_var .PlatformEnemy_set_MinX_value
    +stfld 3
    +stack_return_to_saved_address .PlatformEnemy_set_MinX_ReturnAddress
.PlatformEnemy_set_MinX_this !byte 0,0
.PlatformEnemy_set_MinX_value !byte 0,0
.PlatformEnemy_set_MinX_ReturnAddress !byte 0,0


PlatformEnemy_set_MaxX 
+stack_save_return_adress .PlatformEnemy_set_MaxX_ReturnAddress
+stack_pull_int .PlatformEnemy_set_MaxX_value
+stack_pull_int .PlatformEnemy_set_MaxX_this
    +stack_push_var .PlatformEnemy_set_MaxX_this
    +stack_push_var .PlatformEnemy_set_MaxX_value
    +stfld 4
    +stack_return_to_saved_address .PlatformEnemy_set_MaxX_ReturnAddress
.PlatformEnemy_set_MaxX_this !byte 0,0
.PlatformEnemy_set_MaxX_value !byte 0,0
.PlatformEnemy_set_MaxX_ReturnAddress !byte 0,0


PlatformEnemy_set_Y 
+stack_save_return_adress .PlatformEnemy_set_Y_ReturnAddress
+stack_pull_int .PlatformEnemy_set_Y_value
+stack_pull_int .PlatformEnemy_set_Y_this
    nop
    +stack_push_var .PlatformEnemy_set_Y_this
    +stack_push_var .PlatformEnemy_set_Y_value
    +stfld 1
    +stack_push_var .PlatformEnemy_set_Y_this
    +ldfld 6
    +stack_push_var .PlatformEnemy_set_Y_value
    jsr Sprite_set_Y
    nop
    +stack_return_to_saved_address .PlatformEnemy_set_Y_ReturnAddress
.PlatformEnemy_set_Y_this !byte 0,0
.PlatformEnemy_set_Y_value !byte 0,0
.PlatformEnemy_set_Y_ReturnAddress !byte 0,0


PlatformEnemy_set_Sprite 
+stack_save_return_adress .PlatformEnemy_set_Sprite_ReturnAddress
+stack_pull_int .PlatformEnemy_set_Sprite_value
+stack_pull_int .PlatformEnemy_set_Sprite_this
    nop
    +stack_push_var .PlatformEnemy_set_Sprite_this
    +stack_push_var .PlatformEnemy_set_Sprite_value
    +stfld 6
    +stack_push_var .PlatformEnemy_set_Sprite_this
    +ldfld 6
    +stack_push_int 1
    jsr Sprite_set_DataBlock
    nop
    +stack_push_var .PlatformEnemy_set_Sprite_this
    +stack_push_int 1
    +stfld 5
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
    +ldfld 6
    +stack_push_int 1
    jsr Sprite_set_Visible
    nop
    +stack_return_to_saved_address .PlatformEnemy_set_Sprite_ReturnAddress
.PlatformEnemy_set_Sprite_this !byte 0,0
.PlatformEnemy_set_Sprite_value !byte 0,0
.PlatformEnemy_set_Sprite_ReturnAddress !byte 0,0


PlatformEnemy_Move 
+stack_save_return_adress .PlatformEnemy_Move_ReturnAddress
+stack_pull_int .PlatformEnemy_Move_this
    nop
    +stack_push_var .PlatformEnemy_Move_this
    +ldfld 5
    +stack_pull_int .PlatformEnemy_Move_var0
    +stack_push_var .PlatformEnemy_Move_var0
    +branch_false label_PlatformEnemy_Move_57
    nop
    +stack_push_var .PlatformEnemy_Move_this
    jsr PlatformEnemy_get_X
    +stack_pull_int .PlatformEnemy_Move_var1
    +stack_push_var .PlatformEnemy_Move_this
    +stack_push_var .PlatformEnemy_Move_var1
    +stack_push_int 1
    +add
    jsr PlatformEnemy_set_X
    nop
    +stack_push_var .PlatformEnemy_Move_this
    jsr PlatformEnemy_get_X
    +stack_push_var .PlatformEnemy_Move_this
    jsr PlatformEnemy_get_MaxX
    +compareGreater
    +stack_pull_int .PlatformEnemy_Move_var2
    +stack_push_var .PlatformEnemy_Move_var2
    +branch_false label_PlatformEnemy_Move_54
    +stack_push_var .PlatformEnemy_Move_this
    +stack_push_int 0
    +stfld 5
label_PlatformEnemy_Move_54:    nop
    jmp label_PlatformEnemy_Move_101
label_PlatformEnemy_Move_57:    nop
    +stack_push_var .PlatformEnemy_Move_this
    jsr PlatformEnemy_get_X
    +stack_pull_int .PlatformEnemy_Move_var1
    +stack_push_var .PlatformEnemy_Move_this
    +stack_push_var .PlatformEnemy_Move_var1
    +stack_push_int 1
    +sub
    jsr PlatformEnemy_set_X
    nop
    +stack_push_var .PlatformEnemy_Move_this
    jsr PlatformEnemy_get_X
    +stack_push_var .PlatformEnemy_Move_this
    jsr PlatformEnemy_get_MinX
    +compareLess
    +stack_pull_int .PlatformEnemy_Move_var3
    +stack_push_var .PlatformEnemy_Move_var3
    +branch_false label_PlatformEnemy_Move_100
    +stack_push_var .PlatformEnemy_Move_this
    +stack_push_int 1
    +stfld 5
label_PlatformEnemy_Move_100:    nop
label_PlatformEnemy_Move_101:    +stack_return_to_saved_address .PlatformEnemy_Move_ReturnAddress
.PlatformEnemy_Move_this !byte 0,0
.PlatformEnemy_Move_ReturnAddress !byte 0,0
.PlatformEnemy_Move_var0 !byte 0,0
.PlatformEnemy_Move_var1 !byte 0,0
.PlatformEnemy_Move_var2 !byte 0,0
.PlatformEnemy_Move_var3 !byte 0,0


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
    +ldfld 6
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
    +ldfld 3
    +stack_return_to_saved_address .PlatformEnemy_get_MinX_ReturnAddress
.PlatformEnemy_get_MinX_this !byte 0,0
.PlatformEnemy_get_MinX_ReturnAddress !byte 0,0


PlatformEnemy_get_MaxX 
+stack_save_return_adress .PlatformEnemy_get_MaxX_ReturnAddress
+stack_pull_int .PlatformEnemy_get_MaxX_this
    +stack_push_var .PlatformEnemy_get_MaxX_this
    +ldfld 4
    +stack_return_to_saved_address .PlatformEnemy_get_MaxX_ReturnAddress
.PlatformEnemy_get_MaxX_this !byte 0,0
.PlatformEnemy_get_MaxX_ReturnAddress !byte 0,0
.Platform_field_67108886 !byte 0,0
.Platform_field_67108887 !byte 0,0
.Platform_field_67108888 !byte 0,0
.Platform_field_67108889 !byte 0,0
.Platform_field_67108890 !byte 0,0


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
    +ldfld 2
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
    +stack_push_var .Platform_Draw_var0
    +stack_pull_int .Platform_Draw_var4
    jmp label_Platform_Draw_87
label_Platform_Draw_45:    nop
    +stack_push_var .Platform_Draw_var4
    +stack_push_var .Platform_Draw_var2
    +stack_push_int 67
    +stack_push_var .Platform_Draw_this
    +ldfld 3
    jsr C64_SetChar
    nop
    +stack_push_var .Platform_Draw_var4
    +stack_push_var .Platform_Draw_var3
    +stack_push_int 67
    +stack_push_var .Platform_Draw_this
    +ldfld 3
    jsr C64_SetChar
    nop
    nop
    +stack_push_var .Platform_Draw_var4
    +stack_push_int 1
    +add
    +stack_pull_int .Platform_Draw_var4
label_Platform_Draw_87:    +stack_push_var .Platform_Draw_var4
    +stack_push_var .Platform_Draw_var1
    +compareLess
    +stack_pull_int .Platform_Draw_var5
    +stack_push_var .Platform_Draw_var5
    +branch_true label_Platform_Draw_45
    +stack_push_var .Platform_Draw_this
    +ldfld 0
    +stack_push_var .Platform_Draw_var2
    +stack_push_int 85
    +stack_push_var .Platform_Draw_this
    +ldfld 3
    jsr C64_SetChar
    nop
    +stack_push_var .Platform_Draw_this
    +ldfld 0
    +stack_push_var .Platform_Draw_var3
    +stack_push_int 74
    +stack_push_var .Platform_Draw_this
    +ldfld 3
    jsr C64_SetChar
    nop
    +stack_push_var .Platform_Draw_var1
    +stack_push_var .Platform_Draw_var2
    +stack_push_int 73
    +stack_push_var .Platform_Draw_this
    +ldfld 3
    jsr C64_SetChar
    nop
    +stack_push_var .Platform_Draw_var1
    +stack_push_var .Platform_Draw_var3
    +stack_push_int 75
    +stack_push_var .Platform_Draw_this
    +ldfld 3
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


Player_set_Sprite 
+stack_save_return_adress .Player_set_Sprite_ReturnAddress
+stack_pull_int .Player_set_Sprite_value
+stack_pull_int .Player_set_Sprite_this
    nop
    +stack_push_var .Player_set_Sprite_this
    +stack_push_var .Player_set_Sprite_value
    +stfld 2
    +stack_push_var .Player_set_Sprite_this
    +ldfld 2
    +stack_push_int 0
    jsr Sprite_set_DataBlock
    nop
    +stack_push_var .Player_set_Sprite_this
    +ldfld 2
    +stack_push_int 1
    jsr Sprite_set_Visible
    nop
    +stack_return_to_saved_address .Player_set_Sprite_ReturnAddress
.Player_set_Sprite_this !byte 0,0
.Player_set_Sprite_value !byte 0,0
.Player_set_Sprite_ReturnAddress !byte 0,0


Player_set_PlayerColor 
+stack_save_return_adress .Player_set_PlayerColor_ReturnAddress
+stack_pull_int .Player_set_PlayerColor_value
+stack_pull_int .Player_set_PlayerColor_this
    nop
    +stack_push_var .Player_set_PlayerColor_this
    +ldfld 2
    +stack_push_var .Player_set_PlayerColor_value
    jsr Sprite_set_Color
    nop
    +stack_return_to_saved_address .Player_set_PlayerColor_ReturnAddress
.Player_set_PlayerColor_this !byte 0,0
.Player_set_PlayerColor_value !byte 0,0
.Player_set_PlayerColor_ReturnAddress !byte 0,0


Player_Move 
+stack_save_return_adress .Player_Move_ReturnAddress
+stack_pull_int .Player_Move_this
    nop
    +stack_push_int 22
    jsr C64_IsKeyPressed
    +stack_pull_int .Player_Move_var0
    +stack_push_var .Player_Move_var0
    +branch_false label_Player_Move_31
    +stack_push_var .Player_Move_this
    +stack_push_var .Player_Move_this
    jsr Player_get_Y
    +stack_push_int 255
    +add
    jsr Player_set_Y
    nop
label_Player_Move_31:    +stack_push_int 0
    jsr C64_IsKeyPressed
    +stack_pull_int .Player_Move_var1
    +stack_push_var .Player_Move_var1
    +branch_false label_Player_Move_60
    +stack_push_var .Player_Move_this
    +stack_push_var .Player_Move_this
    jsr Player_get_X
    +stack_push_int 255
    +add
    jsr Player_set_X
    nop
label_Player_Move_60:    +stack_push_int 18
    jsr C64_IsKeyPressed
    +stack_pull_int .Player_Move_var2
    +stack_push_var .Player_Move_var2
    +branch_false label_Player_Move_88
    +stack_push_var .Player_Move_this
    jsr Player_get_Y
    +stack_pull_int .Player_Move_var3
    +stack_push_var .Player_Move_this
    +stack_push_var .Player_Move_var3
    +stack_push_int 1
    +add
    jsr Player_set_Y
    nop
label_Player_Move_88:    +stack_push_int 3
    jsr C64_IsKeyPressed
    +stack_pull_int .Player_Move_var4
    +stack_push_var .Player_Move_var4
    +branch_false label_Player_Move_117
    +stack_push_var .Player_Move_this
    jsr Player_get_X
    +stack_pull_int .Player_Move_var3
    +stack_push_var .Player_Move_this
    +stack_push_var .Player_Move_var3
    +stack_push_int 1
    +add
    jsr Player_set_X
    nop
label_Player_Move_117:    +stack_return_to_saved_address .Player_Move_ReturnAddress
.Player_Move_this !byte 0,0
.Player_Move_ReturnAddress !byte 0,0
.Player_Move_var0 !byte 0,0
.Player_Move_var1 !byte 0,0
.Player_Move_var2 !byte 0,0
.Player_Move_var3 !byte 0,0
.Player_Move_var4 !byte 0,0


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


Player_set_X 
+stack_save_return_adress .Player_set_X_ReturnAddress
+stack_pull_int .Player_set_X_value
+stack_pull_int .Player_set_X_this
    nop
    +stack_push_var .Player_set_X_this
    +stack_push_var .Player_set_X_value
    +stfld 0
    +stack_push_var .Player_set_X_this
    +ldfld 2
    +stack_push_var .Player_set_X_value
    jsr Sprite_set_X
    nop
    +stack_return_to_saved_address .Player_set_X_ReturnAddress
.Player_set_X_this !byte 0,0
.Player_set_X_value !byte 0,0
.Player_set_X_ReturnAddress !byte 0,0


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


Player_set_Y 
+stack_save_return_adress .Player_set_Y_ReturnAddress
+stack_pull_int .Player_set_Y_value
+stack_pull_int .Player_set_Y_this
    nop
    +stack_push_var .Player_set_Y_this
    +stack_push_var .Player_set_Y_value
    +stfld 1
    +stack_push_var .Player_set_Y_this
    +ldfld 2
    +stack_push_var .Player_set_Y_value
    jsr Sprite_set_Y
    nop
    +stack_return_to_saved_address .Player_set_Y_ReturnAddress
.Player_set_Y_this !byte 0,0
.Player_set_Y_value !byte 0,0
.Player_set_Y_ReturnAddress !byte 0,0
.Program_field_67108894 !byte 0,0


Program_ClearScreen 
+stack_save_return_adress .Program_ClearScreen_ReturnAddress
    nop
  ;  +stack_push_int 0
  ;  +stack_pull_int .Program_ClearScreen_var0
    +init_var .Program_ClearScreen_var0, 0
    jmp label_Program_ClearScreen_42
label_Program_ClearScreen_5:    nop
  ;  +stack_push_int 0
  ;  +stack_pull_int .Program_ClearScreen_var1
    +init_var .Program_ClearScreen_var1, 0
    jmp label_Program_ClearScreen_28
label_Program_ClearScreen_10:    nop
    +stack_push_var .Program_ClearScreen_var0
    +stack_push_var .Program_ClearScreen_var1
    +stack_push_int 32
    +stack_push_int 14
    jsr C64_SetChar
    nop
    nop
  ;  +stack_push_var .Program_ClearScreen_var1
  ;  +stack_push_int 1
  ;  +add
  ;  +stack_pull_int .Program_ClearScreen_var1
    +inc_var .Program_ClearScreen_var1
label_Program_ClearScreen_28:  ;  +stack_push_var .Program_ClearScreen_var1
  ;  +stack_push_int 25
  ;  +compareLess
  ;  +stack_pull_int .Program_ClearScreen_var2
  ;  +stack_push_var .Program_ClearScreen_var2
  ;  +branch_true label_Program_ClearScreen_10
    +branch_if_var_less .Program_ClearScreen_var1, 25, label_Program_ClearScreen_10
    nop
  ;  +stack_push_var .Program_ClearScreen_var0
  ;  +stack_push_int 1
  ;  +add
  ;  +stack_pull_int .Program_ClearScreen_var0
    +inc_var .Program_ClearScreen_var0
label_Program_ClearScreen_42:  ;  +stack_push_var .Program_ClearScreen_var0
  ;  +stack_push_int 40
  ;  +compareLess
  ;  +stack_pull_int .Program_ClearScreen_var3
  ;  +stack_push_var .Program_ClearScreen_var3
  ;  +branch_true label_Program_ClearScreen_5
    +branch_if_var_less .Program_ClearScreen_var0, 40, label_Program_ClearScreen_5
    +stack_return_to_saved_address .Program_ClearScreen_ReturnAddress
.Program_ClearScreen_ReturnAddress !byte 0,0
.Program_ClearScreen_var0 !byte 0,0
.Program_ClearScreen_var1 !byte 0,0
.Program_ClearScreen_var2 !byte 0,0
.Program_ClearScreen_var3 !byte 0,0


Program_OnInterrupt 
+stack_save_return_adress .Program_OnInterrupt_ReturnAddress
+stack_pull_int .Program_OnInterrupt_sender
+stack_pull_int .Program_OnInterrupt_args
    nop
    +stack_push_var .Program_field_67108894
    +stack_push_int 1
    +add
    +stack_pull_int .Program_field_67108894
    +stack_push_var .Program_field_67108894
    jsr C64_SetBorderColor
    nop
    +stack_return_to_saved_address .Program_OnInterrupt_ReturnAddress
.Program_OnInterrupt_sender !byte 0,0
.Program_OnInterrupt_args !byte 0,0
.Program_OnInterrupt_ReturnAddress !byte 0,0


Program_Main 
+stack_save_return_adress .Program_Main_ReturnAddress
    nop
    jsr Program_ClearScreen
    nop
  ;  +stack_push_int 0
  ;  +stack_pull_int .Program_Main_var2
    +init_var .Program_Main_var2, 0
    jmp label_Program_Main_28
label_Program_Main_11:    nop
    +stack_push_pointer .string_1879048193
    jsr Console_WriteLine
    nop
    nop
  ;  +stack_push_var .Program_Main_var2
  ;  +stack_push_int 1
  ;  +add
  ;  +stack_pull_int .Program_Main_var2
    +inc_var .Program_Main_var2
label_Program_Main_28:  ;  +stack_push_var .Program_Main_var2
  ;  +stack_push_int 10
  ;  +compareLess
  ;  +stack_pull_int .Program_Main_var3
  ;  +stack_push_var .Program_Main_var3
  ;  +branch_true label_Program_Main_11
    +branch_if_var_less .Program_Main_var2, 10, label_Program_Main_11
    +newObj 4
    +stack_duplicate
    +stack_push_int 10
    +stfld 0
    +stack_duplicate
    +stack_push_int 10
    +stfld 1
    +stack_duplicate
    +stack_push_int 7
    +stfld 3
    +stack_duplicate
    +stack_push_int 10
    +stfld 2
    +stack_pull_int .Program_Main_var0
    +stack_push_var .Program_Main_var0
    jsr Platform_Draw
    nop
    +newObj 1
    +stack_pull_int .Program_Main_var1
    +stack_push_var .Program_Main_var1
    jsr Game_Init
    nop
    +stack_push_var .Program_Main_var1
    jsr Game_Run
    nop
    +stack_return_to_saved_address .Program_Main_ReturnAddress
.Program_Main_ReturnAddress !byte 0,0
.Program_Main_var0 !byte 0,0
.Program_Main_var1 !byte 0,0
.Program_Main_var2 !byte 0,0
.Program_Main_var3 !byte 0,0
.string_1879048193 !pet "hello world",0
