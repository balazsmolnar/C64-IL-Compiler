.Ball_field_67108865 !byte 0,0
.Ball_field_67108866 !byte 0,0
.Ball_field_67108867 !byte 0,0


Ball_get_X 
+stack_save_return_adress .Ball_get_X_ReturnAddress
+stack_pull_int .Ball_get_X_this
    +stack_push_var .Ball_get_X_this
    +ldfld 0
    +stack_return_to_saved_address .Ball_get_X_ReturnAddress
.Ball_get_X_this !byte 0,0
.Ball_get_X_ReturnAddress !byte 0,0


Ball_set_X 
+stack_save_return_adress .Ball_set_X_ReturnAddress
+stack_pull_int .Ball_set_X_this
+stack_pull_int .Ball_set_X_value
    +stack_push_var .Ball_set_X_this
    +stack_push_var .Ball_set_X_value
    +stfld 0
    +stack_return_to_saved_address .Ball_set_X_ReturnAddress
.Ball_set_X_this !byte 0,0
.Ball_set_X_value !byte 0,0
.Ball_set_X_ReturnAddress !byte 0,0


Ball_get_Y 
+stack_save_return_adress .Ball_get_Y_ReturnAddress
+stack_pull_int .Ball_get_Y_this
    +stack_push_var .Ball_get_Y_this
    +ldfld 1
    +stack_return_to_saved_address .Ball_get_Y_ReturnAddress
.Ball_get_Y_this !byte 0,0
.Ball_get_Y_ReturnAddress !byte 0,0


Ball_set_Y 
+stack_save_return_adress .Ball_set_Y_ReturnAddress
+stack_pull_int .Ball_set_Y_this
+stack_pull_int .Ball_set_Y_value
    +stack_push_var .Ball_set_Y_this
    +stack_push_var .Ball_set_Y_value
    +stfld 1
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
+stack_pull_int .Ball_set_VX_this
+stack_pull_int .Ball_set_VX_value
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
+stack_pull_int .Ball_set_VY_this
+stack_pull_int .Ball_set_VY_value
    +stack_push_var .Ball_set_VY_this
    +stack_push_var .Ball_set_VY_value
    +stfld 3
    +stack_return_to_saved_address .Ball_set_VY_ReturnAddress
.Ball_set_VY_this !byte 0,0
.Ball_set_VY_value !byte 0,0
.Ball_set_VY_ReturnAddress !byte 0,0


Ball_get_BallColor 
+stack_save_return_adress .Ball_get_BallColor_ReturnAddress
+stack_pull_int .Ball_get_BallColor_this
    +stack_push_var .Ball_get_BallColor_this
    +ldfld 4
    +stack_return_to_saved_address .Ball_get_BallColor_ReturnAddress
.Ball_get_BallColor_this !byte 0,0
.Ball_get_BallColor_ReturnAddress !byte 0,0


Ball_set_BallColor 
+stack_save_return_adress .Ball_set_BallColor_ReturnAddress
+stack_pull_int .Ball_set_BallColor_this
+stack_pull_int .Ball_set_BallColor_value
    +stack_push_var .Ball_set_BallColor_this
    +stack_push_var .Ball_set_BallColor_value
    +stfld 4
    +stack_return_to_saved_address .Ball_set_BallColor_ReturnAddress
.Ball_set_BallColor_this !byte 0,0
.Ball_set_BallColor_value !byte 0,0
.Ball_set_BallColor_ReturnAddress !byte 0,0


Ball_get_OldChar 
+stack_save_return_adress .Ball_get_OldChar_ReturnAddress
+stack_pull_int .Ball_get_OldChar_this
    +stack_push_var .Ball_get_OldChar_this
    +ldfld 5
    +stack_return_to_saved_address .Ball_get_OldChar_ReturnAddress
.Ball_get_OldChar_this !byte 0,0
.Ball_get_OldChar_ReturnAddress !byte 0,0


Ball_set_OldChar 
+stack_save_return_adress .Ball_set_OldChar_ReturnAddress
+stack_pull_int .Ball_set_OldChar_this
+stack_pull_int .Ball_set_OldChar_value
    +stack_push_var .Ball_set_OldChar_this
    +stack_push_var .Ball_set_OldChar_value
    +stfld 5
    +stack_return_to_saved_address .Ball_set_OldChar_ReturnAddress
.Ball_set_OldChar_this !byte 0,0
.Ball_set_OldChar_value !byte 0,0
.Ball_set_OldChar_ReturnAddress !byte 0,0


Ball_Clear 
+stack_save_return_adress .Ball_Clear_ReturnAddress
+stack_pull_int .Ball_Clear_this
    nop
    +stack_push_var .Ball_Clear_this
    jsr Ball_get_OldChar
    +stack_push_int 81
    +compareEqual
    +stack_push_int 0
    +compareEqual
    +stack_pull_int .Ball_Clear_var0
    +stack_push_var .Ball_Clear_var0
    +branch_false label_Clear_44
    +stack_push_var .Ball_Clear_this
    jsr Ball_get_X
    +stack_push_var .Ball_Clear_this
    jsr Ball_get_Y
    +stack_push_var .Ball_Clear_this
    jsr Ball_get_OldChar
    +stack_push_int 14
    jsr C64_SetChar
    nop
label_Clear_44:    +stack_push_var .Ball_Clear_this
    +stack_push_var .Ball_Clear_this
    jsr Ball_get_X
    +stack_push_var .Ball_Clear_this
    jsr Ball_get_VX
    +add
    jsr Ball_set_X
    nop
    +stack_push_var .Ball_Clear_this
    +stack_push_var .Ball_Clear_this
    jsr Ball_get_Y
    +stack_push_var .Ball_Clear_this
    jsr Ball_get_VY
    +add
    jsr Ball_set_Y
    nop
    +stack_push_var .Ball_Clear_this
    jsr Ball_get_X
    +stack_push_int 38
    +compareEqual
    +stack_pull_int .Ball_Clear_var1
    +stack_push_var .Ball_Clear_var1
    +branch_false label_Clear_112
    +stack_push_var .Ball_Clear_this
    +stack_push_var .Ball_Clear_this
    jsr Ball_get_VX
    +negate
    jsr Ball_set_VX
    nop
label_Clear_112:    +stack_push_var .Ball_Clear_this
    jsr Ball_get_X
    +stack_push_int 0
    +compareEqual
    +stack_pull_int .Ball_Clear_var2
    +stack_push_var .Ball_Clear_var2
    +branch_false label_Clear_139
    +stack_push_var .Ball_Clear_this
    +stack_push_var .Ball_Clear_this
    jsr Ball_get_VX
    +negate
    jsr Ball_set_VX
    nop
label_Clear_139:    +stack_push_var .Ball_Clear_this
    jsr Ball_get_Y
    +stack_push_int 24
    +compareEqual
    +stack_pull_int .Ball_Clear_var3
    +stack_push_var .Ball_Clear_var3
    +branch_false label_Clear_167
    +stack_push_var .Ball_Clear_this
    +stack_push_var .Ball_Clear_this
    jsr Ball_get_VY
    +negate
    jsr Ball_set_VY
    nop
label_Clear_167:    +stack_push_var .Ball_Clear_this
    jsr Ball_get_Y
    +stack_push_int 0
    +compareEqual
    +stack_pull_int .Ball_Clear_var4
    +stack_push_var .Ball_Clear_var4
    +branch_false label_Clear_196
    +stack_push_var .Ball_Clear_this
    +stack_push_var .Ball_Clear_this
    jsr Ball_get_VY
    +negate
    jsr Ball_set_VY
    nop
label_Clear_196:    +stack_return_to_saved_address .Ball_Clear_ReturnAddress
.Ball_Clear_this !byte 0,0
.Ball_Clear_ReturnAddress !byte 0,0
.Ball_Clear_var0 !byte 0,0
.Ball_Clear_var1 !byte 0,0
.Ball_Clear_var2 !byte 0,0
.Ball_Clear_var3 !byte 0,0
.Ball_Clear_var4 !byte 0,0


Ball_Set 
+stack_save_return_adress .Ball_Set_ReturnAddress
+stack_pull_int .Ball_Set_this
    nop
    +stack_push_var .Ball_Set_this
    +stack_push_var .Ball_Set_this
    jsr Ball_get_X
    +stack_push_var .Ball_Set_this
    jsr Ball_get_Y
    jsr C64_GetChar
    jsr Ball_set_OldChar
    nop
    +stack_push_var .Ball_Set_this
    jsr Ball_get_X
    +stack_push_var .Ball_Set_this
    jsr Ball_get_Y
    +stack_push_int 81
    +stack_push_var .Ball_Set_this
    jsr Ball_get_BallColor
    jsr C64_SetChar
    nop
    +stack_return_to_saved_address .Ball_Set_ReturnAddress
.Ball_Set_this !byte 0,0
.Ball_Set_ReturnAddress !byte 0,0
.Program_field_67108874 !byte 0,0


Program_ClearScreen 
+stack_save_return_adress .Program_ClearScreen_ReturnAddress
    nop
  ;  +stack_push_int 0
  ;  +stack_pull_int .Program_ClearScreen_var0
    +init_var .Program_ClearScreen_var0, 0
    jmp label_ClearScreen_42
label_ClearScreen_5:    nop
  ;  +stack_push_int 0
  ;  +stack_pull_int .Program_ClearScreen_var1
    +init_var .Program_ClearScreen_var1, 0
    jmp label_ClearScreen_28
label_ClearScreen_10:    nop
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
label_ClearScreen_28:  ;  +stack_push_var .Program_ClearScreen_var1
  ;  +stack_push_int 25
  ;  +compareLess
  ;  +stack_pull_int .Program_ClearScreen_var2
  ;  +stack_push_var .Program_ClearScreen_var2
  ;  +branch_true label_ClearScreen_10
    +branch_if_var_less .Program_ClearScreen_var1, 25, label_ClearScreen_10
    nop
  ;  +stack_push_var .Program_ClearScreen_var0
  ;  +stack_push_int 1
  ;  +add
  ;  +stack_pull_int .Program_ClearScreen_var0
    +inc_var .Program_ClearScreen_var0
label_ClearScreen_42:  ;  +stack_push_var .Program_ClearScreen_var0
  ;  +stack_push_int 40
  ;  +compareLess
  ;  +stack_pull_int .Program_ClearScreen_var3
  ;  +stack_push_var .Program_ClearScreen_var3
  ;  +branch_true label_ClearScreen_5
    +branch_if_var_less .Program_ClearScreen_var0, 40, label_ClearScreen_5
    +stack_return_to_saved_address .Program_ClearScreen_ReturnAddress
.Program_ClearScreen_ReturnAddress !byte 0,0
.Program_ClearScreen_var0 !byte 0,0
.Program_ClearScreen_var1 !byte 0,0
.Program_ClearScreen_var2 !byte 0,0
.Program_ClearScreen_var3 !byte 0,0


Program_RunBall 
+stack_save_return_adress .Program_RunBall_ReturnAddress
    nop
    +newObj 6
    +stack_pull_int .Program_RunBall_var0
    +stack_push_var .Program_RunBall_var0
    +stack_push_int 1
    jsr Ball_set_VX
    nop
    +stack_push_var .Program_RunBall_var0
    +stack_push_int 1
    jsr Ball_set_VY
    nop
    +stack_push_var .Program_RunBall_var0
    +stack_push_int 2
    jsr Ball_set_BallColor
    nop
    +newObj 6
    +stack_pull_int .Program_RunBall_var1
    +stack_push_var .Program_RunBall_var1
    +stack_push_int 2
    jsr Ball_set_VX
    nop
    +stack_push_var .Program_RunBall_var1
    +stack_push_int 1
    jsr Ball_set_VY
    nop
    +stack_push_var .Program_RunBall_var1
    +stack_push_int 1
    jsr Ball_set_BallColor
    nop
    +newObj 6
    +stack_pull_int .Program_RunBall_var2
    +stack_push_var .Program_RunBall_var2
    +stack_push_int 1
    jsr Ball_set_VX
    nop
    +stack_push_var .Program_RunBall_var2
    +stack_push_int 2
    jsr Ball_set_VY
    nop
    +stack_push_var .Program_RunBall_var0
    +stack_push_int 7
    jsr Ball_set_BallColor
    nop
    jmp label_RunBall_179
label_RunBall_93:    nop
    +stack_push_var .Program_RunBall_var0
    jsr Ball_Clear
    nop
    +stack_push_var .Program_RunBall_var1
    jsr Ball_Clear
    nop
    +stack_push_var .Program_RunBall_var2
    jsr Ball_Clear
    nop
    +stack_push_var .Program_RunBall_var0
    jsr Ball_Set
    nop
    +stack_push_var .Program_RunBall_var1
    jsr Ball_Set
    nop
    +stack_push_var .Program_RunBall_var2
    jsr Ball_Set
    nop
  ;  +stack_push_int 0
  ;  +stack_pull_int .Program_RunBall_var3
    +init_var .Program_RunBall_var3, 0
    jmp label_RunBall_167
label_RunBall_140:    +stack_push_int 0
    +stack_pull_int .Program_RunBall_var4
    jmp label_RunBall_152
label_RunBall_145:    nop
    +stack_push_var .Program_RunBall_var4
    +stack_push_int 1
    +add
    +stack_pull_int .Program_RunBall_var4
label_RunBall_152:    +stack_push_var .Program_RunBall_var4
    +stack_push_int 3
    +compareLess
    +stack_pull_int .Program_RunBall_var5
    +stack_push_var .Program_RunBall_var5
    +branch_true label_RunBall_145
  ;  +stack_push_var .Program_RunBall_var3
  ;  +stack_push_int 1
  ;  +add
  ;  +stack_pull_int .Program_RunBall_var3
    +inc_var .Program_RunBall_var3
label_RunBall_167:    +stack_push_var .Program_RunBall_var3
    +stack_push_int 100
    +compareLess
    +stack_pull_int .Program_RunBall_var6
    +stack_push_var .Program_RunBall_var6
    +branch_true label_RunBall_140
    nop
label_RunBall_179:    jmp label_RunBall_93
.Program_RunBall_ReturnAddress !byte 0,0
.Program_RunBall_var0 !byte 0,0
.Program_RunBall_var1 !byte 0,0
.Program_RunBall_var2 !byte 0,0
.Program_RunBall_var3 !byte 0,0
.Program_RunBall_var4 !byte 0,0
.Program_RunBall_var5 !byte 0,0
.Program_RunBall_var6 !byte 0,0


Program_OnInterrupt 
+stack_save_return_adress .Program_OnInterrupt_ReturnAddress
+stack_pull_int .Program_OnInterrupt_sender
+stack_pull_int .Program_OnInterrupt_args
    nop
    +stack_push_var .Program_field_67108874
    +stack_push_int 1
    +add
    +stack_pull_int .Program_field_67108874
    +stack_push_var .Program_field_67108874
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
    jmp label_Main_28
label_Main_11:    nop
    +stack_push_pointer .string_1879048193
    jsr Console_WriteLine
    nop
    nop
  ;  +stack_push_var .Program_Main_var0
  ;  +stack_push_int 1
  ;  +add
  ;  +stack_pull_int .Program_Main_var0
    +inc_var .Program_Main_var0
label_Main_28:  ;  +stack_push_var .Program_Main_var0
  ;  +stack_push_int 10
  ;  +compareLess
  ;  +stack_pull_int .Program_Main_var1
  ;  +stack_push_var .Program_Main_var1
  ;  +branch_true label_Main_11
    +branch_if_var_less .Program_Main_var0, 10, label_Main_11
    jsr Program_RunBall
    nop
    +stack_return_to_saved_address .Program_Main_ReturnAddress
.Program_Main_ReturnAddress !byte 0,0
.Program_Main_var0 !byte 0,0
.Program_Main_var1 !byte 0,0
.string_1879048193 !pet "hello world",0
