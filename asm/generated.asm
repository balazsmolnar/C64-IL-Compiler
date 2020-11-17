Program_Main 
    nop 
    +stack_push_int 0 
    +stack_pull_int .Program_Main_var0
    jmp label_0
label_3:    nop 
    +stack_push_int 0 
    +stack_pull_int .Program_Main_var1
    jmp label_1
label_2:    nop 
    +stack_push_pointer .string_1879048241
    jsr Console_Write
    nop 
    nop 
    +stack_push_var .Program_Main_var1
    +stack_push_int 1 
    +add16 
    +stack_pull_int .Program_Main_var1
label_1:    +stack_push_var .Program_Main_var1
    +stack_push_var .Program_Main_var0
    +compareLess16 
    +stack_pull_int .Program_Main_var2
    +stack_push_var .Program_Main_var2
    +branch_true label_2
    jsr Program_Hello
    nop 
    nop 
    +stack_push_var .Program_Main_var0
    +stack_push_int 1 
    +add16 
    +stack_pull_int .Program_Main_var0
label_0:    +stack_push_var .Program_Main_var0
    +stack_push_int 10
    +compareLess16 
    +stack_pull_int .Program_Main_var3
    +stack_push_var .Program_Main_var3
    +branch_true label_3
    rts 
.Program_Main_var0 !byte 0,0
.Program_Main_var1 !byte 0,0
.Program_Main_var2 !byte 0,0
.Program_Main_var3 !byte 0,0
Program_Hello 
    nop 
    +stack_push_pointer .string_1879048193
    jsr Console_WriteLine
    nop 
    rts 
.string_1879048241 !pet " ",0
.string_1879048193 !pet "hello world from method",0
