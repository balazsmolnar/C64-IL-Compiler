

Program_Main 
    nop 
    jsr Program_ClearScreen
    nop 
    +stack_push_int 0
    +stack_pull_int .Program_Main_var0
    jmp label_Main_0
label_Main_1:    nop 
    +stack_push_var .Program_Main_var0
    jsr Program_WriteSpaces
    nop 
    +stack_push_pointer .string_1879048197
    jsr Program_Hello
    nop 
    nop 
    +stack_push_var .Program_Main_var0
    +stack_push_int 1
    +add16 
    +stack_pull_int .Program_Main_var0
label_Main_0:    +stack_push_var .Program_Main_var0
    +stack_push_int 10
    +compareLess16 
    +stack_pull_int .Program_Main_var1
    +stack_push_var .Program_Main_var1
    +branch_true label_Main_1
    jsr Program_RunBall
    nop 
    rts 
.Program_Main_var0 !byte 0,0
.Program_Main_var1 !byte 0,0


Program_ClearScreen 
    nop 
    +stack_push_int 0
    +stack_pull_int .Program_ClearScreen_var0
    jmp label_ClearScreen_0
label_ClearScreen_3:    nop 
    +stack_push_int 0
    +stack_pull_int .Program_ClearScreen_var1
    jmp label_ClearScreen_1
label_ClearScreen_2:    nop 
    +stack_push_var .Program_ClearScreen_var0
    +stack_push_var .Program_ClearScreen_var1
    +stack_push_int 32
    +stack_push_int 14
    jsr C64_SetChar
    nop 
    nop 
    +stack_push_var .Program_ClearScreen_var1
    +stack_push_int 1
    +add16 
    +stack_pull_int .Program_ClearScreen_var1
label_ClearScreen_1:    +stack_push_var .Program_ClearScreen_var1
    +stack_push_int 40
    +compareLess16 
    +stack_pull_int .Program_ClearScreen_var2
    +stack_push_var .Program_ClearScreen_var2
    +branch_true label_ClearScreen_2
    nop 
    +stack_push_var .Program_ClearScreen_var0
    +stack_push_int 1
    +add16 
    +stack_pull_int .Program_ClearScreen_var0
label_ClearScreen_0:    +stack_push_var .Program_ClearScreen_var0
    +stack_push_int 40
    +compareLess16 
    +stack_pull_int .Program_ClearScreen_var3
    +stack_push_var .Program_ClearScreen_var3
    +branch_true label_ClearScreen_3
    rts 
.Program_ClearScreen_var0 !byte 0,0
.Program_ClearScreen_var1 !byte 0,0
.Program_ClearScreen_var2 !byte 0,0
.Program_ClearScreen_var3 !byte 0,0


Program_Hello 
+stack_pull_int .Program_Hello_s
    nop 
    +stack_push_var .Program_Hello_s
    jsr Console_WriteLine
    nop 
    rts 
.Program_Hello_s !byte 0,0


Program_WriteSpaces 
+stack_pull_int .Program_WriteSpaces_n
    nop 
    +stack_push_int 0
    +stack_pull_int .Program_WriteSpaces_var0
    jmp label_WriteSpaces_0
label_WriteSpaces_1:    nop 
    +stack_push_pointer .string_1879048193
    jsr Console_Write
    nop 
    nop 
    +stack_push_var .Program_WriteSpaces_var0
    +stack_push_int 1
    +add16 
    +stack_pull_int .Program_WriteSpaces_var0
label_WriteSpaces_0:    +stack_push_var .Program_WriteSpaces_var0
    +stack_push_var .Program_WriteSpaces_n
    +compareLess16 
    +stack_pull_int .Program_WriteSpaces_var1
    +stack_push_var .Program_WriteSpaces_var1
    +branch_true label_WriteSpaces_1
    rts 
.Program_WriteSpaces_n !byte 0,0
.Program_WriteSpaces_var0 !byte 0,0
.Program_WriteSpaces_var1 !byte 0,0


Program_RunBall 
    nop 
    +stack_push_int 0
    +stack_pull_int .Program_RunBall_var0
    +stack_push_int 0
    +stack_pull_int .Program_RunBall_var1
    +stack_push_int 1
    +stack_pull_int .Program_RunBall_var2
    +stack_push_int 1
    +stack_pull_int .Program_RunBall_var3
    jmp label_RunBall_0
label_RunBall_7:    nop 
    +stack_push_var .Program_RunBall_var0
    +stack_push_var .Program_RunBall_var1
    +stack_push_int 32
    +stack_push_int 8
    jsr C64_SetChar
    nop 
    +stack_push_var .Program_RunBall_var0
    +stack_push_var .Program_RunBall_var2
    +add16 
    +stack_pull_int .Program_RunBall_var0
    +stack_push_var .Program_RunBall_var1
    +stack_push_var .Program_RunBall_var3
    +add16 
    +stack_pull_int .Program_RunBall_var1
    +stack_push_var .Program_RunBall_var0
    +stack_push_int 39
    +compareEqual16 
    +stack_pull_int .Program_RunBall_var4
    +stack_push_var .Program_RunBall_var4
    +branch_false label_RunBall_1
    +stack_push_int 65535
    +stack_pull_int .Program_RunBall_var2
label_RunBall_1:    +stack_push_var .Program_RunBall_var0
    +stack_push_int 0
    +compareEqual16 
    +stack_pull_int .Program_RunBall_var5
    +stack_push_var .Program_RunBall_var5
    +branch_false label_RunBall_2
    +stack_push_int 1
    +stack_pull_int .Program_RunBall_var2
label_RunBall_2:    +stack_push_var .Program_RunBall_var1
    +stack_push_int 24
    +compareEqual16 
    +stack_pull_int .Program_RunBall_var6
    +stack_push_var .Program_RunBall_var6
    +branch_false label_RunBall_3
    +stack_push_int 65535
    +stack_pull_int .Program_RunBall_var3
label_RunBall_3:    +stack_push_var .Program_RunBall_var1
    +stack_push_int 0
    +compareEqual16 
    +stack_pull_int .Program_RunBall_var7
    +stack_push_var .Program_RunBall_var7
    +branch_false label_RunBall_4
    +stack_push_int 1
    +stack_pull_int .Program_RunBall_var3
label_RunBall_4:    +stack_push_var .Program_RunBall_var0
    +stack_push_var .Program_RunBall_var1
    +stack_push_int 81
    +stack_push_int 8
    jsr C64_SetChar
    nop 
    +stack_push_int 0
    +stack_pull_int .Program_RunBall_var8
    jmp label_RunBall_5
label_RunBall_6:    nop 
    +stack_push_var .Program_RunBall_var8
    +stack_push_int 1
    +add16 
    +stack_pull_int .Program_RunBall_var8
label_RunBall_5:    +stack_push_var .Program_RunBall_var8
    +stack_push_int 100
    +compareLess16 
    +stack_pull_int .Program_RunBall_var9
    +stack_push_var .Program_RunBall_var9
    +branch_true label_RunBall_6
    nop 
label_RunBall_0:    jmp label_RunBall_7
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
.string_1879048197 !pet "hello world from parameter",0
.string_1879048193 !pet " ",0
