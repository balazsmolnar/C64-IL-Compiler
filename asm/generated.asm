.Program_field_67108865 !byte 0,0


Program_get_BallColor 
+stack_save_return_adress .Program_get_BallColor_ReturnAddress
    +stack_push_int 8
    +stack_return_to_saved_address .Program_get_BallColor_ReturnAddress
.Program_get_BallColor_ReturnAddress !byte 0,0


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


Program_Hello 
+stack_save_return_adress .Program_Hello_ReturnAddress
+stack_pull_int .Program_Hello_s
    nop
    +stack_push_var .Program_Hello_s
    jsr Console_WriteLine
    nop
    +stack_return_to_saved_address .Program_Hello_ReturnAddress
.Program_Hello_s !byte 0,0
.Program_Hello_ReturnAddress !byte 0,0


Program_WriteSpaces 
+stack_save_return_adress .Program_WriteSpaces_ReturnAddress
+stack_pull_int .Program_WriteSpaces_n
    nop
  ;  +stack_push_int 0
  ;  +stack_pull_int .Program_WriteSpaces_var0
    +init_var .Program_WriteSpaces_var0, 0
    jmp label_WriteSpaces_22
label_WriteSpaces_5:    nop
    +stack_push_pointer .string_1879048193
    jsr Console_Write
    nop
    nop
  ;  +stack_push_var .Program_WriteSpaces_var0
  ;  +stack_push_int 1
  ;  +add
  ;  +stack_pull_int .Program_WriteSpaces_var0
    +inc_var .Program_WriteSpaces_var0
label_WriteSpaces_22:    +stack_push_var .Program_WriteSpaces_var0
    +stack_push_var .Program_WriteSpaces_n
    +compareLess
    +stack_pull_int .Program_WriteSpaces_var1
    +stack_push_var .Program_WriteSpaces_var1
    +branch_true label_WriteSpaces_5
    +stack_return_to_saved_address .Program_WriteSpaces_ReturnAddress
.Program_WriteSpaces_n !byte 0,0
.Program_WriteSpaces_ReturnAddress !byte 0,0
.Program_WriteSpaces_var0 !byte 0,0
.Program_WriteSpaces_var1 !byte 0,0


Program_RunBall 
+stack_save_return_adress .Program_RunBall_ReturnAddress
    nop
  ;  +stack_push_int 0
  ;  +stack_pull_int .Program_RunBall_var0
    +init_var .Program_RunBall_var0, 0
  ;  +stack_push_int 0
  ;  +stack_pull_int .Program_RunBall_var1
    +init_var .Program_RunBall_var1, 0
  ;  +stack_push_int 1
  ;  +stack_pull_int .Program_RunBall_var2
    +init_var .Program_RunBall_var2, 1
  ;  +stack_push_int 1
  ;  +stack_pull_int .Program_RunBall_var3
    +init_var .Program_RunBall_var3, 1
    +stack_push_var .Program_RunBall_var0
    +stack_push_var .Program_RunBall_var1
    jsr C64_GetChar
    +stack_pull_int .Program_RunBall_var4
    jmp label_RunBall_143
label_RunBall_20:    nop
    +stack_push_var .Program_RunBall_var0
    +stack_push_var .Program_RunBall_var1
    +stack_push_var .Program_RunBall_var4
    jsr Program_get_BallColor
    jsr C64_SetChar
    nop
    +stack_push_var .Program_RunBall_var0
    +stack_push_var .Program_RunBall_var2
    +add
    +stack_pull_int .Program_RunBall_var0
    +stack_push_var .Program_RunBall_var1
    +stack_push_var .Program_RunBall_var3
    +add
    +stack_pull_int .Program_RunBall_var1
    +stack_push_var .Program_RunBall_var0
    +stack_push_var .Program_RunBall_var1
    jsr C64_GetChar
    +stack_pull_int .Program_RunBall_var4
  ;  +stack_push_var .Program_RunBall_var0
  ;  +stack_push_int 39
  ;  +compareEqual
  ;  +stack_pull_int .Program_RunBall_var5
  ;  +stack_push_var .Program_RunBall_var5
  ;  +branch_false label_RunBall_66
    +branch_if_not_equal .Program_RunBall_var0, 39, label_RunBall_66
  ;  +stack_push_int 65535
  ;  +stack_pull_int .Program_RunBall_var2
    +init_var .Program_RunBall_var2, 65535
label_RunBall_66:  ;  +stack_push_var .Program_RunBall_var0
  ;  +stack_push_int 0
  ;  +compareEqual
  ;  +stack_pull_int .Program_RunBall_var6
  ;  +stack_push_var .Program_RunBall_var6
  ;  +branch_false label_RunBall_78
    +branch_if_not_equal .Program_RunBall_var0, 0, label_RunBall_78
  ;  +stack_push_int 1
  ;  +stack_pull_int .Program_RunBall_var2
    +init_var .Program_RunBall_var2, 1
label_RunBall_78:  ;  +stack_push_var .Program_RunBall_var1
  ;  +stack_push_int 24
  ;  +compareEqual
  ;  +stack_pull_int .Program_RunBall_var7
  ;  +stack_push_var .Program_RunBall_var7
  ;  +branch_false label_RunBall_91
    +branch_if_not_equal .Program_RunBall_var1, 24, label_RunBall_91
  ;  +stack_push_int 65535
  ;  +stack_pull_int .Program_RunBall_var3
    +init_var .Program_RunBall_var3, 65535
label_RunBall_91:  ;  +stack_push_var .Program_RunBall_var1
  ;  +stack_push_int 0
  ;  +compareEqual
  ;  +stack_pull_int .Program_RunBall_var8
  ;  +stack_push_var .Program_RunBall_var8
  ;  +branch_false label_RunBall_103
    +branch_if_not_equal .Program_RunBall_var1, 0, label_RunBall_103
  ;  +stack_push_int 1
  ;  +stack_pull_int .Program_RunBall_var3
    +init_var .Program_RunBall_var3, 1
label_RunBall_103:    +stack_push_var .Program_RunBall_var0
    +stack_push_var .Program_RunBall_var1
    +stack_push_int 81
    jsr Program_get_BallColor
    jsr C64_SetChar
    nop
    +stack_push_int 0
    +stack_pull_int .Program_RunBall_var9
    jmp label_RunBall_130
label_RunBall_123:    nop
    +stack_push_var .Program_RunBall_var9
    +stack_push_int 1
    +add
    +stack_pull_int .Program_RunBall_var9
label_RunBall_130:    +stack_push_var .Program_RunBall_var9
    +stack_push_int 100
    +compareLess
    +stack_pull_int .Program_RunBall_var10
    +stack_push_var .Program_RunBall_var10
    +branch_true label_RunBall_123
    nop
label_RunBall_143:    jmp label_RunBall_20
.Program_RunBall_ReturnAddress !byte 0,0
.Program_RunBall_var0 !byte 0,0
.Program_RunBall_var1 !byte 0,0
.Program_RunBall_var2 !byte 0,0
.Program_RunBall_var3 !byte 0,0
.Program_RunBall_var4 !byte 0,0
.Program_RunBall_var5 !byte 0,0
.Program_RunBall_var6 !byte 0,0
.Program_RunBall_var7 !byte 0,0
.Program_RunBall_var8 !byte 0,0
.Program_RunBall_var9 !byte 0,0
.Program_RunBall_var10 !byte 0,0


Program_OnInterrupt 
+stack_save_return_adress .Program_OnInterrupt_ReturnAddress
+stack_pull_int .Program_OnInterrupt_sender
+stack_pull_int .Program_OnInterrupt_args
    nop
    +stack_push_var .Program_field_67108865
    +stack_push_int 1
    +add
    +stack_pull_int .Program_field_67108865
    +stack_push_var .Program_field_67108865
    jsr C64_SetBorderColor
    nop
    +stack_return_to_saved_address .Program_OnInterrupt_ReturnAddress
.Program_OnInterrupt_sender !byte 0,0
.Program_OnInterrupt_args !byte 0,0
.Program_OnInterrupt_ReturnAddress !byte 0,0


Program_Main 
+stack_save_return_adress .Program_Main_ReturnAddress
    nop
    +stack_push_int 0
    +stack_pull_int .Program_field_67108865
    jsr Program_ClearScreen
    nop
  ;  +stack_push_int 0
  ;  +stack_pull_int .Program_Main_var0
    +init_var .Program_Main_var0, 0
    jmp label_Main_34
label_Main_17:    nop
    +stack_push_pointer .string_1879048197
    jsr Console_WriteLine
    nop
    nop
  ;  +stack_push_var .Program_Main_var0
  ;  +stack_push_int 1
  ;  +add
  ;  +stack_pull_int .Program_Main_var0
    +inc_var .Program_Main_var0
label_Main_34:  ;  +stack_push_var .Program_Main_var0
  ;  +stack_push_int 10
  ;  +compareLess
  ;  +stack_pull_int .Program_Main_var1
  ;  +stack_push_var .Program_Main_var1
  ;  +branch_true label_Main_17
    +branch_if_var_less .Program_Main_var0, 10, label_Main_17
    jsr Program_RunBall
    nop
    +stack_return_to_saved_address .Program_Main_ReturnAddress
.Program_Main_ReturnAddress !byte 0,0
.Program_Main_var0 !byte 0,0
.Program_Main_var1 !byte 0,0
.string_1879048193 !pet " ",0
.string_1879048197 !pet "hello world",0
