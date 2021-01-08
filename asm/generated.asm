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
    nop  ;Conv_i8
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
    nop  ;Conv_i8
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
.Program_field_67108877 !byte 0,0


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
    +stack_push_var .Program_field_67108877
    +stack_push_int 1
    +add
    +stack_pull_int .Program_field_67108877
    +stack_push_var .Program_field_67108877
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
  ;  +stack_pull_int .Program_Main_var0
    +init_var .Program_Main_var0, 0
    jmp label_Program_Main_28
label_Program_Main_11:    nop
    +stack_push_pointer .string_1879048193
    jsr Console_WriteLine
    nop
    nop
  ;  +stack_push_var .Program_Main_var0
  ;  +stack_push_int 1
  ;  +add
  ;  +stack_pull_int .Program_Main_var0
    +inc_var .Program_Main_var0
label_Program_Main_28:  ;  +stack_push_var .Program_Main_var0
  ;  +stack_push_int 10
  ;  +compareLess
  ;  +stack_pull_int .Program_Main_var1
  ;  +stack_push_var .Program_Main_var1
  ;  +branch_true label_Program_Main_11
    +branch_if_var_less .Program_Main_var0, 10, label_Program_Main_11
    jsr Ball_RunBalls
    nop
    +stack_return_to_saved_address .Program_Main_ReturnAddress
.Program_Main_ReturnAddress !byte 0,0
.Program_Main_var0 !byte 0,0
.Program_Main_var1 !byte 0,0
.string_1879048193 !pet "hello world",0
