

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
.string_1879048197 !pet "hello world from parameter",0
.string_1879048193 !pet " ",0
